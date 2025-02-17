using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Iced.Intel.EncoderInternal;

namespace Iced.Intel;

internal sealed class Encoder
{
	private static readonly uint[] s_immSizes = new uint[19]
	{
		0u, 1u, 2u, 4u, 8u, 3u, 2u, 4u, 6u, 1u,
		1u, 1u, 2u, 2u, 2u, 4u, 4u, 1u, 1u
	};

	internal uint Internal_PreventVEX2;

	internal uint Internal_VEX_WIG_LIG;

	internal uint Internal_VEX_LIG;

	internal uint Internal_EVEX_WIG;

	internal uint Internal_EVEX_LIG;

	internal const string ERROR_ONLY_1632_BIT_MODE = "The instruction can only be used in 16/32-bit mode";

	internal const string ERROR_ONLY_64_BIT_MODE = "The instruction can only be used in 64-bit mode";

	private readonly CodeWriter writer;

	private readonly int bitness;

	private readonly OpCodeHandler[] handlers;

	private readonly uint[] immSizes;

	private ulong currentRip;

	private string errorMessage;

	private OpCodeHandler handler;

	private uint eip;

	private uint displAddr;

	private uint immAddr;

	internal uint Immediate;

	internal uint ImmediateHi;

	private uint Displ;

	private uint DisplHi;

	private readonly EncoderFlags opSize16Flags;

	private readonly EncoderFlags opSize32Flags;

	private readonly EncoderFlags adrSize16Flags;

	private readonly EncoderFlags adrSize32Flags;

	internal uint OpCode;

	internal EncoderFlags EncoderFlags;

	private DisplSize DisplSize;

	internal ImmSize ImmSize;

	private byte ModRM;

	private byte Sib;

	public bool PreventVEX2
	{
		get
		{
			return Internal_PreventVEX2 != 0;
		}
		set
		{
			Internal_PreventVEX2 = (value ? uint.MaxValue : 0u);
		}
	}

	public uint VEX_WIG
	{
		get
		{
			return (Internal_VEX_WIG_LIG >> 7) & 1;
		}
		set
		{
			Internal_VEX_WIG_LIG = (Internal_VEX_WIG_LIG & 0xFFFFFF7Fu) | ((value & 1) << 7);
		}
	}

	public uint VEX_LIG
	{
		get
		{
			return (Internal_VEX_WIG_LIG >> 2) & 1;
		}
		set
		{
			Internal_VEX_WIG_LIG = (Internal_VEX_WIG_LIG & 0xFFFFFFFBu) | ((value & 1) << 2);
			Internal_VEX_LIG = (value & 1) << 2;
		}
	}

	public uint EVEX_WIG
	{
		get
		{
			return Internal_EVEX_WIG >> 7;
		}
		set
		{
			Internal_EVEX_WIG = (value & 1) << 7;
		}
	}

	public uint EVEX_LIG
	{
		get
		{
			return Internal_EVEX_LIG >> 5;
		}
		set
		{
			Internal_EVEX_LIG = (value & 3) << 5;
		}
	}

	public int Bitness => bitness;

	internal string? ErrorMessage
	{
		set
		{
			if (errorMessage == null)
			{
				errorMessage = value;
			}
		}
	}

	private static ReadOnlySpan<byte> SegmentOverrides => "&.6>de"u8;

	private Encoder(CodeWriter writer, int bitness)
	{
		if (writer == null)
		{
			ThrowHelper.ThrowArgumentNullException_writer();
		}
		immSizes = s_immSizes;
		this.writer = writer;
		this.bitness = bitness;
		handlers = OpCodeHandlers.Handlers;
		handler = null;
		opSize16Flags = ((bitness != 16) ? EncoderFlags.P66 : EncoderFlags.None);
		opSize32Flags = ((bitness == 16) ? EncoderFlags.P66 : EncoderFlags.None);
		adrSize16Flags = ((bitness != 16) ? EncoderFlags.P67 : EncoderFlags.None);
		adrSize32Flags = ((bitness != 32) ? EncoderFlags.P67 : EncoderFlags.None);
	}

	public static Encoder Create(int bitness, CodeWriter writer)
	{
		if (bitness == 16 || bitness == 32 || bitness == 64)
		{
			return new Encoder(writer, bitness);
		}
		throw new ArgumentOutOfRangeException("bitness");
	}

	public uint Encode(in Instruction instruction, ulong rip)
	{
		if (!TryEncode(in instruction, rip, out uint encodedLength, out string text))
		{
			ThrowEncoderException(in instruction, text);
		}
		return encodedLength;
	}

	private static void ThrowEncoderException(in Instruction instruction, string errorMessage)
	{
		throw new EncoderException(errorMessage, in instruction);
	}

	public bool TryEncode(in Instruction instruction, ulong rip, out uint encodedLength, [_003Cb2ffb5d6_002D6a81_002D4f20_002D8e75_002D7064682f7f7c_003ENotNullWhen(false)] out string? errorMessage)
	{
		currentRip = rip;
		eip = (uint)rip;
		this.errorMessage = null;
		EncoderFlags = EncoderFlags.None;
		DisplSize = DisplSize.None;
		ImmSize = ImmSize.None;
		ModRM = 0;
		OpCodeHandler opCodeHandler = (handler = handlers[(int)instruction.Code]);
		OpCode = opCodeHandler.OpCode;
		if (opCodeHandler.GroupIndex >= 0)
		{
			EncoderFlags = EncoderFlags.ModRM;
			ModRM = (byte)(opCodeHandler.GroupIndex << 3);
		}
		if (opCodeHandler.RmGroupIndex >= 0)
		{
			EncoderFlags = EncoderFlags.ModRM;
			ModRM |= (byte)(opCodeHandler.RmGroupIndex | 0xC0);
		}
		switch (opCodeHandler.EncFlags3 & (EncFlags3.Bit16or32 | EncFlags3.Bit64))
		{
		case EncFlags3.Bit16or32:
			if (bitness == 64)
			{
				ErrorMessage = "The instruction can only be used in 16/32-bit mode";
			}
			break;
		case EncFlags3.Bit64:
			if (bitness != 64)
			{
				ErrorMessage = "The instruction can only be used in 64-bit mode";
			}
			break;
		default:
			throw new InvalidOperationException();
		case EncFlags3.Bit16or32 | EncFlags3.Bit64:
			break;
		}
		switch (opCodeHandler.OpSize)
		{
		case CodeSize.Code16:
			EncoderFlags |= opSize16Flags;
			break;
		case CodeSize.Code32:
			EncoderFlags |= opSize32Flags;
			break;
		case CodeSize.Code64:
			if ((opCodeHandler.EncFlags3 & EncFlags3.DefaultOpSize64) == 0)
			{
				EncoderFlags |= EncoderFlags.W;
			}
			break;
		default:
			throw new InvalidOperationException();
		case CodeSize.Unknown:
			break;
		}
		switch (opCodeHandler.AddrSize)
		{
		case CodeSize.Code16:
			EncoderFlags |= adrSize16Flags;
			break;
		case CodeSize.Code32:
			EncoderFlags |= adrSize32Flags;
			break;
		default:
			throw new InvalidOperationException();
		case CodeSize.Unknown:
		case CodeSize.Code64:
			break;
		}
		if (!opCodeHandler.IsSpecialInstr)
		{
			Op[] operands = opCodeHandler.Operands;
			for (int i = 0; i < operands.Length; i++)
			{
				operands[i].Encode(this, in instruction, i);
			}
			if ((opCodeHandler.EncFlags3 & EncFlags3.Fwait) != 0)
			{
				WriteByteInternal(155u);
			}
			opCodeHandler.Encode(this, in instruction);
			uint opCode = OpCode;
			if (!opCodeHandler.Is2ByteOpCode)
			{
				WriteByteInternal(opCode);
			}
			else
			{
				WriteByteInternal(opCode >> 8);
				WriteByteInternal(opCode);
			}
			if ((EncoderFlags & (EncoderFlags.ModRM | EncoderFlags.Displ)) != 0)
			{
				WriteModRM();
			}
			if (ImmSize != 0)
			{
				WriteImmediate();
			}
		}
		else
		{
			opCodeHandler.Encode(this, in instruction);
		}
		uint num = (uint)((int)currentRip - (int)rip);
		if (num > 15 && !opCodeHandler.IsSpecialInstr)
		{
			ErrorMessage = $"Instruction length > {15} bytes";
		}
		errorMessage = this.errorMessage;
		if (errorMessage != null)
		{
			encodedLength = 0u;
			return false;
		}
		encodedLength = num;
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool Verify(int operand, OpKind expected, OpKind actual)
	{
		if (expected == actual)
		{
			return true;
		}
		ErrorMessage = $"Operand {operand}: Expected: {expected}, actual: {actual}";
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool Verify(int operand, Register expected, Register actual)
	{
		if (expected == actual)
		{
			return true;
		}
		ErrorMessage = $"Operand {operand}: Expected: {expected}, actual: {actual}";
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool Verify(int operand, Register register, Register regLo, Register regHi)
	{
		if (bitness != 64 && regHi > regLo + 7)
		{
			regHi = regLo + 7;
		}
		if (regLo <= register && register <= regHi)
		{
			return true;
		}
		ErrorMessage = $"Operand {operand}: Register {register} is not between {regLo} and {regHi} (inclusive)";
		return false;
	}

	internal void AddBranch(OpKind opKind, int immSize, in Instruction instruction, int operand)
	{
		if (!Verify(operand, opKind, instruction.GetOpKind(operand)))
		{
			return;
		}
		switch (immSize)
		{
		case 1:
			switch (opKind)
			{
			case OpKind.NearBranch16:
				EncoderFlags |= opSize16Flags;
				ImmSize = ImmSize.RipRelSize1_Target16;
				Immediate = instruction.NearBranch16;
				break;
			case OpKind.NearBranch32:
				EncoderFlags |= opSize32Flags;
				ImmSize = ImmSize.RipRelSize1_Target32;
				Immediate = instruction.NearBranch32;
				break;
			case OpKind.NearBranch64:
			{
				ImmSize = ImmSize.RipRelSize1_Target64;
				ulong nearBranch = instruction.NearBranch64;
				Immediate = (uint)nearBranch;
				ImmediateHi = (uint)(nearBranch >> 32);
				break;
			}
			default:
				throw new InvalidOperationException();
			}
			break;
		case 2:
			if (opKind == OpKind.NearBranch16)
			{
				EncoderFlags |= opSize16Flags;
				ImmSize = ImmSize.RipRelSize2_Target16;
				Immediate = instruction.NearBranch16;
				break;
			}
			throw new InvalidOperationException();
		case 4:
			switch (opKind)
			{
			case OpKind.NearBranch32:
				EncoderFlags |= opSize32Flags;
				ImmSize = ImmSize.RipRelSize4_Target32;
				Immediate = instruction.NearBranch32;
				break;
			case OpKind.NearBranch64:
			{
				ImmSize = ImmSize.RipRelSize4_Target64;
				ulong nearBranch = instruction.NearBranch64;
				Immediate = (uint)nearBranch;
				ImmediateHi = (uint)(nearBranch >> 32);
				break;
			}
			default:
				throw new InvalidOperationException();
			}
			break;
		default:
			throw new InvalidOperationException();
		}
	}

	internal void AddBranchX(int immSize, in Instruction instruction, int operand)
	{
		if (bitness == 64)
		{
			if (Verify(operand, OpKind.NearBranch64, instruction.GetOpKind(operand)))
			{
				ulong nearBranch = instruction.NearBranch64;
				switch (immSize)
				{
				case 2:
					EncoderFlags |= EncoderFlags.P66;
					ImmSize = ImmSize.RipRelSize2_Target64;
					Immediate = (uint)nearBranch;
					ImmediateHi = (uint)(nearBranch >> 32);
					break;
				case 4:
					ImmSize = ImmSize.RipRelSize4_Target64;
					Immediate = (uint)nearBranch;
					ImmediateHi = (uint)(nearBranch >> 32);
					break;
				default:
					throw new InvalidOperationException();
				}
			}
		}
		else if (Verify(operand, OpKind.NearBranch32, instruction.GetOpKind(operand)))
		{
			switch (immSize)
			{
			case 2:
				EncoderFlags |= (EncoderFlags)((bitness & 0x20) << 2);
				ImmSize = ImmSize.RipRelSize2_Target32;
				Immediate = instruction.NearBranch32;
				break;
			case 4:
				EncoderFlags |= (EncoderFlags)((bitness & 0x10) << 3);
				ImmSize = ImmSize.RipRelSize4_Target32;
				Immediate = instruction.NearBranch32;
				break;
			default:
				throw new InvalidOperationException();
			}
		}
	}

	internal void AddBranchDisp(int displSize, in Instruction instruction, int operand)
	{
		OpKind expected;
		switch (displSize)
		{
		case 2:
			expected = OpKind.NearBranch16;
			ImmSize = ImmSize.Size2;
			Immediate = instruction.NearBranch16;
			break;
		case 4:
			expected = OpKind.NearBranch32;
			ImmSize = ImmSize.Size4;
			Immediate = instruction.NearBranch32;
			break;
		default:
			throw new InvalidOperationException();
		}
		Verify(operand, expected, instruction.GetOpKind(operand));
	}

	internal void AddFarBranch(in Instruction instruction, int operand, int size)
	{
		if (size == 2)
		{
			if (!Verify(operand, OpKind.FarBranch16, instruction.GetOpKind(operand)))
			{
				return;
			}
			ImmSize = ImmSize.Size2_2;
			Immediate = instruction.FarBranch16;
			ImmediateHi = instruction.FarBranchSelector;
		}
		else
		{
			if (!Verify(operand, OpKind.FarBranch32, instruction.GetOpKind(operand)))
			{
				return;
			}
			ImmSize = ImmSize.Size4_2;
			Immediate = instruction.FarBranch32;
			ImmediateHi = instruction.FarBranchSelector;
		}
		if (bitness != size * 8)
		{
			EncoderFlags |= EncoderFlags.P66;
		}
	}

	internal void SetAddrSize(int regSize)
	{
		if (bitness == 64)
		{
			switch (regSize)
			{
			case 2:
				ErrorMessage = $"Invalid register size: {regSize * 8}, must be 32-bit or 64-bit";
				break;
			case 4:
				EncoderFlags |= EncoderFlags.P67;
				break;
			}
		}
		else if (regSize == 8)
		{
			ErrorMessage = $"Invalid register size: {regSize * 8}, must be 16-bit or 32-bit";
		}
		else if (bitness == 16)
		{
			if (regSize == 4)
			{
				EncoderFlags |= EncoderFlags.P67;
			}
		}
		else if (regSize == 2)
		{
			EncoderFlags |= EncoderFlags.P67;
		}
	}

	internal void AddAbsMem(in Instruction instruction, int operand)
	{
		EncoderFlags |= EncoderFlags.Displ;
		OpKind opKind = instruction.GetOpKind(operand);
		if (opKind == OpKind.Memory)
		{
			if (instruction.MemoryBase != 0 || instruction.MemoryIndex != 0)
			{
				ErrorMessage = $"Operand {operand}: Absolute addresses can't have base and/or index regs";
				return;
			}
			if (instruction.MemoryIndexScale != 1)
			{
				ErrorMessage = $"Operand {operand}: Absolute addresses must have scale == *1";
				return;
			}
			switch (instruction.MemoryDisplSize)
			{
			case 2:
				if (bitness == 64)
				{
					ErrorMessage = $"Operand {operand}: 16-bit abs addresses can't be used in 64-bit mode";
					break;
				}
				if (bitness == 32)
				{
					EncoderFlags |= EncoderFlags.P67;
				}
				DisplSize = DisplSize.Size2;
				if (instruction.MemoryDisplacement64 > 65535)
				{
					ErrorMessage = $"Operand {operand}: Displacement must fit in a ushort";
				}
				else
				{
					Displ = instruction.MemoryDisplacement32;
				}
				break;
			case 4:
				EncoderFlags |= adrSize32Flags;
				DisplSize = DisplSize.Size4;
				if (instruction.MemoryDisplacement64 > uint.MaxValue)
				{
					ErrorMessage = $"Operand {operand}: Displacement must fit in a uint";
				}
				else
				{
					Displ = instruction.MemoryDisplacement32;
				}
				break;
			case 8:
				if (bitness != 64)
				{
					ErrorMessage = $"Operand {operand}: 64-bit abs address is only available in 64-bit mode";
				}
				else
				{
					DisplSize = DisplSize.Size8;
					ulong memoryDisplacement = instruction.MemoryDisplacement64;
					Displ = (uint)memoryDisplacement;
					DisplHi = (uint)(memoryDisplacement >> 32);
				}
				break;
			default:
				ErrorMessage = $"Operand {operand}: {"Instruction"}.{"MemoryDisplSize"} must be initialized to 2 (16-bit), 4 (32-bit) or 8 (64-bit)";
				break;
			}
		}
		else
		{
			ErrorMessage = $"Operand {operand}: Expected OpKind {"Memory"}, actual: {opKind}";
		}
	}

	internal void AddModRMRegister(in Instruction instruction, int operand, Register regLo, Register regHi)
	{
		if (!Verify(operand, OpKind.Register, instruction.GetOpKind(operand)))
		{
			return;
		}
		Register opRegister = instruction.GetOpRegister(operand);
		if (!Verify(operand, opRegister, regLo, regHi))
		{
			return;
		}
		uint num = (uint)(opRegister - regLo);
		if (regLo == Register.AL)
		{
			if (opRegister >= Register.SPL)
			{
				num -= 4;
				EncoderFlags |= EncoderFlags.REX;
			}
			else if (opRegister >= Register.AH)
			{
				EncoderFlags |= EncoderFlags.HighLegacy8BitRegs;
			}
		}
		ModRM |= (byte)((num & 7) << 3);
		EncoderFlags |= EncoderFlags.ModRM;
		EncoderFlags |= (EncoderFlags)((num & 8) >> 1);
		EncoderFlags |= (EncoderFlags)((num & 0x10) << 5);
	}

	internal void AddReg(in Instruction instruction, int operand, Register regLo, Register regHi)
	{
		if (!Verify(operand, OpKind.Register, instruction.GetOpKind(operand)))
		{
			return;
		}
		Register opRegister = instruction.GetOpRegister(operand);
		if (!Verify(operand, opRegister, regLo, regHi))
		{
			return;
		}
		uint num = (uint)(opRegister - regLo);
		if (regLo == Register.AL)
		{
			if (opRegister >= Register.SPL)
			{
				num -= 4;
				EncoderFlags |= EncoderFlags.REX;
			}
			else if (opRegister >= Register.AH)
			{
				EncoderFlags |= EncoderFlags.HighLegacy8BitRegs;
			}
		}
		OpCode |= num & 7;
		EncoderFlags |= (EncoderFlags)(num >> 3);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void AddRegOrMem(in Instruction instruction, int operand, Register regLo, Register regHi, bool allowMemOp, bool allowRegOp)
	{
		AddRegOrMem(in instruction, operand, regLo, regHi, Register.None, Register.None, allowMemOp, allowRegOp);
	}

	internal void AddRegOrMem(in Instruction instruction, int operand, Register regLo, Register regHi, Register vsibIndexRegLo, Register vsibIndexRegHi, bool allowMemOp, bool allowRegOp)
	{
		OpKind opKind = instruction.GetOpKind(operand);
		EncoderFlags |= EncoderFlags.ModRM;
		switch (opKind)
		{
		case OpKind.Register:
		{
			if (!allowRegOp)
			{
				ErrorMessage = $"Operand {operand}: register operand is not allowed";
				break;
			}
			Register opRegister = instruction.GetOpRegister(operand);
			if (!Verify(operand, opRegister, regLo, regHi))
			{
				break;
			}
			uint num2 = (uint)(opRegister - regLo);
			if (regLo == Register.AL)
			{
				if (opRegister >= Register.R8L)
				{
					num2 -= 4;
				}
				else if (opRegister >= Register.SPL)
				{
					num2 -= 4;
					EncoderFlags |= EncoderFlags.REX;
				}
				else if (opRegister >= Register.AH)
				{
					EncoderFlags |= EncoderFlags.HighLegacy8BitRegs;
				}
			}
			ModRM |= (byte)(num2 & 7);
			ModRM |= 192;
			EncoderFlags |= (EncoderFlags)((num2 >> 3) & 3);
			break;
		}
		case OpKind.Memory:
		{
			if (!allowMemOp)
			{
				ErrorMessage = $"Operand {operand}: memory operand is not allowed";
				break;
			}
			if (instruction.MemorySize.IsBroadcast())
			{
				EncoderFlags |= EncoderFlags.Broadcast;
			}
			CodeSize codeSize = instruction.CodeSize;
			if (codeSize == CodeSize.Unknown)
			{
				codeSize = ((bitness == 64) ? CodeSize.Code64 : ((bitness != 32) ? CodeSize.Code16 : CodeSize.Code32));
			}
			int num = InstructionUtils.GetAddressSizeInBytes(instruction.MemoryBase, instruction.MemoryIndex, instruction.MemoryDisplSize, codeSize) * 8;
			if (num != bitness)
			{
				EncoderFlags |= EncoderFlags.P67;
			}
			if ((EncoderFlags & EncoderFlags.RegIsMemory) != 0 && GetRegisterOpSize(in instruction) != num)
			{
				ErrorMessage = $"Operand {operand}: Register operand size must equal memory addressing mode (16/32/64)";
			}
			else if (num == 16)
			{
				if (vsibIndexRegLo != 0)
				{
					ErrorMessage = $"Operand {operand}: VSIB operands can't use 16-bit addressing. It must be 32-bit or 64-bit addressing";
				}
				else
				{
					AddMemOp16(in instruction, operand);
				}
			}
			else
			{
				AddMemOp(in instruction, operand, num, vsibIndexRegLo, vsibIndexRegHi);
			}
			break;
		}
		default:
			ErrorMessage = $"Operand {operand}: Expected a register or memory operand, but opKind is {opKind}";
			break;
		}
	}

	private static int GetRegisterOpSize(in Instruction instruction)
	{
		if (instruction.Op0Kind == OpKind.Register)
		{
			Register op0Register = instruction.Op0Register;
			if (op0Register.IsGPR64())
			{
				return 64;
			}
			if (op0Register.IsGPR32())
			{
				return 32;
			}
			if (op0Register.IsGPR16())
			{
				return 16;
			}
		}
		return 0;
	}

	private bool TryConvertToDisp8N(in Instruction instruction, int displ, out sbyte compressedValue)
	{
		TryConvertToDisp8N tryConvertToDisp8N = handler.TryConvertToDisp8N;
		if (tryConvertToDisp8N != null)
		{
			return tryConvertToDisp8N(this, handler, in instruction, displ, out compressedValue);
		}
		if (-128 <= displ && displ <= 127)
		{
			compressedValue = (sbyte)displ;
			return true;
		}
		compressedValue = 0;
		return false;
	}

	private void AddMemOp16(in Instruction instruction, int operand)
	{
		if (bitness == 64)
		{
			ErrorMessage = $"Operand {operand}: 16-bit addressing can't be used by 64-bit code";
			return;
		}
		Register memoryBase = instruction.MemoryBase;
		Register memoryIndex = instruction.MemoryIndex;
		int num = instruction.MemoryDisplSize;
		if (memoryBase != Register.BX || memoryIndex != Register.SI)
		{
			if (memoryBase == Register.BX && memoryIndex == Register.DI)
			{
				ModRM |= 1;
			}
			else if (memoryBase == Register.BP && memoryIndex == Register.SI)
			{
				ModRM |= 2;
			}
			else if (memoryBase == Register.BP && memoryIndex == Register.DI)
			{
				ModRM |= 3;
			}
			else if (memoryBase == Register.SI && memoryIndex == Register.None)
			{
				ModRM |= 4;
			}
			else if (memoryBase == Register.DI && memoryIndex == Register.None)
			{
				ModRM |= 5;
			}
			else if (memoryBase == Register.BP && memoryIndex == Register.None)
			{
				ModRM |= 6;
			}
			else if (memoryBase == Register.BX && memoryIndex == Register.None)
			{
				ModRM |= 7;
			}
			else
			{
				if (memoryBase != 0 || memoryIndex != 0)
				{
					ErrorMessage = $"Operand {operand}: Invalid 16-bit base + index registers: base={memoryBase}, index={memoryIndex}";
					return;
				}
				ModRM |= 6;
				DisplSize = DisplSize.Size2;
				if (instruction.MemoryDisplacement64 > 65535)
				{
					ErrorMessage = $"Operand {operand}: Displacement must fit in a ushort";
					return;
				}
				Displ = instruction.MemoryDisplacement32;
			}
		}
		if (memoryBase == Register.None && memoryIndex == Register.None)
		{
			return;
		}
		if ((long)instruction.MemoryDisplacement64 < -32768L || (long)instruction.MemoryDisplacement64 > 65535L)
		{
			ErrorMessage = $"Operand {operand}: Displacement must fit in a short or a ushort";
			return;
		}
		Displ = instruction.MemoryDisplacement32;
		if (num == 0 && memoryBase == Register.BP && memoryIndex == Register.None)
		{
			num = 1;
			if (Displ != 0)
			{
				ErrorMessage = $"Operand {operand}: Displacement must be 0 if displSize == 0";
				return;
			}
		}
		if (num == 1)
		{
			if (TryConvertToDisp8N(in instruction, (short)Displ, out var compressedValue))
			{
				Displ = (uint)compressedValue;
			}
			else
			{
				num = 2;
			}
		}
		switch (num)
		{
		case 0:
			if (Displ != 0)
			{
				ErrorMessage = $"Operand {operand}: Displacement must be 0 if displSize == 0";
			}
			break;
		case 1:
			if ((int)Displ < -128 || (int)Displ > 127)
			{
				ErrorMessage = $"Operand {operand}: Displacement must fit in an sbyte";
			}
			else
			{
				ModRM |= 64;
				DisplSize = DisplSize.Size1;
			}
			break;
		case 2:
			ModRM |= 128;
			DisplSize = DisplSize.Size2;
			break;
		default:
			ErrorMessage = $"Operand {operand}: Invalid displacement size: {num}, must be 0, 1, or 2";
			break;
		}
	}

	private void AddMemOp(in Instruction instruction, int operand, int addrSize, Register vsibIndexRegLo, Register vsibIndexRegHi)
	{
		if (bitness != 64 && addrSize == 64)
		{
			ErrorMessage = $"Operand {operand}: 64-bit addressing can only be used in 64-bit mode";
			return;
		}
		Register memoryBase = instruction.MemoryBase;
		Register memoryIndex = instruction.MemoryIndex;
		int num = instruction.MemoryDisplSize;
		Register register;
		Register register2;
		if (addrSize == 64)
		{
			register = Register.RAX;
			register2 = Register.R15;
		}
		else
		{
			register = Register.EAX;
			register2 = Register.R15D;
		}
		Register register3;
		Register regHi;
		if (vsibIndexRegLo != 0)
		{
			register3 = vsibIndexRegLo;
			regHi = vsibIndexRegHi;
		}
		else
		{
			register3 = register;
			regHi = register2;
		}
		if ((memoryBase != 0 && memoryBase != Register.RIP && memoryBase != Register.EIP && !Verify(operand, memoryBase, register, register2)) || (memoryIndex != 0 && !Verify(operand, memoryIndex, register3, regHi)))
		{
			return;
		}
		if (num != 0 && num != 1 && num != 4 && num != 8)
		{
			ErrorMessage = $"Operand {operand}: Invalid displ size: {num}, must be 0, 1, 4, 8";
			return;
		}
		if (memoryBase == Register.RIP || memoryBase == Register.EIP)
		{
			if (memoryIndex != 0)
			{
				ErrorMessage = $"Operand {operand}: RIP relative addressing can't use an index register";
				return;
			}
			if (instruction.InternalMemoryIndexScale != 0)
			{
				ErrorMessage = $"Operand {operand}: RIP relative addressing must use scale *1";
				return;
			}
			if (bitness != 64)
			{
				ErrorMessage = $"Operand {operand}: RIP/EIP relative addressing is only available in 64-bit mode";
				return;
			}
			if ((EncoderFlags & EncoderFlags.MustUseSib) != 0)
			{
				ErrorMessage = $"Operand {operand}: RIP/EIP relative addressing isn't supported";
				return;
			}
			ModRM |= 5;
			ulong memoryDisplacement = instruction.MemoryDisplacement64;
			if (memoryBase == Register.RIP)
			{
				DisplSize = DisplSize.RipRelSize4_Target64;
				Displ = (uint)memoryDisplacement;
				DisplHi = (uint)(memoryDisplacement >> 32);
				return;
			}
			DisplSize = DisplSize.RipRelSize4_Target32;
			if (memoryDisplacement > uint.MaxValue)
			{
				ErrorMessage = $"Operand {operand}: Target address doesn't fit in 32 bits: 0x{memoryDisplacement:X}";
			}
			else
			{
				Displ = (uint)memoryDisplacement;
			}
			return;
		}
		int internalMemoryIndexScale = instruction.InternalMemoryIndexScale;
		Displ = instruction.MemoryDisplacement32;
		if (addrSize == 64)
		{
			if ((long)instruction.MemoryDisplacement64 < -2147483648L || (long)instruction.MemoryDisplacement64 > 2147483647L)
			{
				ErrorMessage = $"Operand {operand}: Displacement must fit in an int";
				return;
			}
		}
		else if ((long)instruction.MemoryDisplacement64 < -2147483648L || (long)instruction.MemoryDisplacement64 > 4294967295L)
		{
			ErrorMessage = $"Operand {operand}: Displacement must fit in an int or a uint";
			return;
		}
		if (memoryBase == Register.None && memoryIndex == Register.None)
		{
			if (vsibIndexRegLo != 0)
			{
				ErrorMessage = $"Operand {operand}: VSIB addressing can't use an offset-only address";
			}
			else if (bitness == 64 || internalMemoryIndexScale != 0 || (EncoderFlags & EncoderFlags.MustUseSib) != 0)
			{
				ModRM |= 4;
				DisplSize = DisplSize.Size4;
				EncoderFlags |= EncoderFlags.Sib;
				Sib = (byte)(0x25 | (internalMemoryIndexScale << 6));
			}
			else
			{
				ModRM |= 5;
				DisplSize = DisplSize.Size4;
			}
			return;
		}
		int num2 = ((memoryBase == Register.None) ? (-1) : (memoryBase - register));
		int num3 = ((memoryIndex == Register.None) ? (-1) : (memoryIndex - register3));
		if (num == 0 && (num2 & 7) == 5)
		{
			num = 1;
			if (Displ != 0)
			{
				ErrorMessage = $"Operand {operand}: Displacement must be 0 if displSize == 0";
				return;
			}
		}
		if (num == 1)
		{
			if (TryConvertToDisp8N(in instruction, (int)Displ, out var compressedValue))
			{
				Displ = (uint)compressedValue;
			}
			else
			{
				num = addrSize / 8;
			}
		}
		if (memoryBase == Register.None)
		{
			DisplSize = DisplSize.Size4;
		}
		else if (num == 1)
		{
			if ((int)Displ < -128 || (int)Displ > 127)
			{
				ErrorMessage = $"Operand {operand}: Displacement must fit in an sbyte";
				return;
			}
			ModRM |= 64;
			DisplSize = DisplSize.Size1;
		}
		else if (num == addrSize / 8)
		{
			ModRM |= 128;
			DisplSize = DisplSize.Size4;
		}
		else
		{
			if (num != 0)
			{
				ErrorMessage = $"Operand {operand}: Invalid {"MemoryDisplSize"} value";
				return;
			}
			if (Displ != 0)
			{
				ErrorMessage = $"Operand {operand}: Displacement must be 0 if displSize == 0";
				return;
			}
		}
		if (memoryIndex == Register.None && (num2 & 7) != 4 && internalMemoryIndexScale == 0 && (EncoderFlags & EncoderFlags.MustUseSib) == 0)
		{
			ModRM |= (byte)(num2 & 7);
		}
		else
		{
			EncoderFlags |= EncoderFlags.Sib;
			Sib = (byte)(internalMemoryIndexScale << 6);
			ModRM |= 4;
			if (memoryIndex == Register.RSP || memoryIndex == Register.ESP)
			{
				ErrorMessage = $"Operand {operand}: ESP/RSP can't be used as an index register";
				return;
			}
			if (num2 < 0)
			{
				Sib |= 5;
			}
			else
			{
				Sib |= (byte)(num2 & 7);
			}
			if (num3 < 0)
			{
				Sib |= 32;
			}
			else
			{
				Sib |= (byte)((num3 & 7) << 3);
			}
		}
		if (num2 >= 0)
		{
			EncoderFlags |= (EncoderFlags)(num2 >> 3);
		}
		if (num3 >= 0)
		{
			EncoderFlags |= (EncoderFlags)((num3 >> 2) & 2);
			EncoderFlags |= (EncoderFlags)((num3 & 0x10) << 27);
		}
	}

	internal void WritePrefixes(in Instruction instruction, bool canWriteF3 = true)
	{
		Register segmentPrefix = instruction.SegmentPrefix;
		if (segmentPrefix != 0)
		{
			WriteByteInternal(SegmentOverrides[(int)(segmentPrefix - 71)]);
		}
		if ((EncoderFlags & EncoderFlags.PF0) != 0 || instruction.HasLockPrefix)
		{
			WriteByteInternal(240u);
		}
		if ((EncoderFlags & EncoderFlags.P66) != 0)
		{
			WriteByteInternal(102u);
		}
		if ((EncoderFlags & EncoderFlags.P67) != 0)
		{
			WriteByteInternal(103u);
		}
		if (canWriteF3 && instruction.HasRepePrefix)
		{
			WriteByteInternal(243u);
		}
		if (instruction.HasRepnePrefix)
		{
			WriteByteInternal(242u);
		}
	}

	private void WriteModRM()
	{
		if ((EncoderFlags & EncoderFlags.ModRM) != 0)
		{
			WriteByteInternal(ModRM);
			if ((EncoderFlags & EncoderFlags.Sib) != 0)
			{
				WriteByteInternal(Sib);
			}
		}
		displAddr = (uint)currentRip;
		switch (DisplSize)
		{
		case DisplSize.Size1:
			WriteByteInternal(Displ);
			break;
		case DisplSize.Size2:
		{
			uint num3 = Displ;
			WriteByteInternal(num3);
			WriteByteInternal(num3 >> 8);
			break;
		}
		case DisplSize.Size4:
		{
			uint num3 = Displ;
			WriteByteInternal(num3);
			WriteByteInternal(num3 >> 8);
			WriteByteInternal(num3 >> 16);
			WriteByteInternal(num3 >> 24);
			break;
		}
		case DisplSize.Size8:
		{
			uint num3 = Displ;
			WriteByteInternal(num3);
			WriteByteInternal(num3 >> 8);
			WriteByteInternal(num3 >> 16);
			WriteByteInternal(num3 >> 24);
			num3 = DisplHi;
			WriteByteInternal(num3);
			WriteByteInternal(num3 >> 8);
			WriteByteInternal(num3 >> 16);
			WriteByteInternal(num3 >> 24);
			break;
		}
		case DisplSize.RipRelSize4_Target32:
		{
			uint num4 = (uint)((int)currentRip + 4) + immSizes[(int)ImmSize];
			uint num3 = Displ - num4;
			WriteByteInternal(num3);
			WriteByteInternal(num3 >> 8);
			WriteByteInternal(num3 >> 16);
			WriteByteInternal(num3 >> 24);
			break;
		}
		case DisplSize.RipRelSize4_Target64:
		{
			ulong num = currentRip + 4 + immSizes[(int)ImmSize];
			long num2 = (long)((((ulong)DisplHi << 32) | Displ) - num);
			if (num2 < int.MinValue || num2 > int.MaxValue)
			{
				ErrorMessage = $"RIP relative distance is too far away: NextIP: 0x{num:X16} target: 0x{DisplHi:X8}{Displ:X8}, diff = {num2}, diff must fit in an Int32";
			}
			uint num3 = (uint)num2;
			WriteByteInternal(num3);
			WriteByteInternal(num3 >> 8);
			WriteByteInternal(num3 >> 16);
			WriteByteInternal(num3 >> 24);
			break;
		}
		default:
			throw new InvalidOperationException();
		case DisplSize.None:
			break;
		}
	}

	private void WriteImmediate()
	{
		immAddr = (uint)currentRip;
		switch (ImmSize)
		{
		case ImmSize.Size1:
		case ImmSize.SizeIbReg:
		case ImmSize.Size1OpCode:
			WriteByteInternal(Immediate);
			break;
		case ImmSize.Size2:
		{
			uint num3 = Immediate;
			WriteByteInternal(num3);
			WriteByteInternal(num3 >> 8);
			break;
		}
		case ImmSize.Size4:
		{
			uint num3 = Immediate;
			WriteByteInternal(num3);
			WriteByteInternal(num3 >> 8);
			WriteByteInternal(num3 >> 16);
			WriteByteInternal(num3 >> 24);
			break;
		}
		case ImmSize.Size8:
		{
			uint num3 = Immediate;
			WriteByteInternal(num3);
			WriteByteInternal(num3 >> 8);
			WriteByteInternal(num3 >> 16);
			WriteByteInternal(num3 >> 24);
			num3 = ImmediateHi;
			WriteByteInternal(num3);
			WriteByteInternal(num3 >> 8);
			WriteByteInternal(num3 >> 16);
			WriteByteInternal(num3 >> 24);
			break;
		}
		case ImmSize.Size2_1:
		{
			uint num3 = Immediate;
			WriteByteInternal(num3);
			WriteByteInternal(num3 >> 8);
			WriteByteInternal(ImmediateHi);
			break;
		}
		case ImmSize.Size1_1:
			WriteByteInternal(Immediate);
			WriteByteInternal(ImmediateHi);
			break;
		case ImmSize.Size2_2:
		{
			uint num3 = Immediate;
			WriteByteInternal(num3);
			WriteByteInternal(num3 >> 8);
			num3 = ImmediateHi;
			WriteByteInternal(num3);
			WriteByteInternal(num3 >> 8);
			break;
		}
		case ImmSize.Size4_2:
		{
			uint num3 = Immediate;
			WriteByteInternal(num3);
			WriteByteInternal(num3 >> 8);
			WriteByteInternal(num3 >> 16);
			WriteByteInternal(num3 >> 24);
			num3 = ImmediateHi;
			WriteByteInternal(num3);
			WriteByteInternal(num3 >> 8);
			break;
		}
		case ImmSize.RipRelSize1_Target16:
		{
			ushort num6 = (ushort)((int)currentRip + 1);
			short num7 = (short)((short)Immediate - (short)num6);
			if (num7 < -128 || num7 > 127)
			{
				ErrorMessage = $"Branch distance is too far away: NextIP: 0x{num6:X4} target: 0x{(ushort)Immediate:X4}, diff = {num7}, diff must fit in an Int8";
			}
			WriteByteInternal((uint)num7);
			break;
		}
		case ImmSize.RipRelSize1_Target32:
		{
			uint num4 = (uint)((int)currentRip + 1);
			int num5 = (int)(Immediate - num4);
			if (num5 < -128 || num5 > 127)
			{
				ErrorMessage = $"Branch distance is too far away: NextIP: 0x{num4:X8} target: 0x{Immediate:X8}, diff = {num5}, diff must fit in an Int8";
			}
			WriteByteInternal((uint)num5);
			break;
		}
		case ImmSize.RipRelSize1_Target64:
		{
			ulong num = currentRip + 1;
			long num2 = (long)((((ulong)ImmediateHi << 32) | Immediate) - num);
			if (num2 < -128 || num2 > 127)
			{
				ErrorMessage = $"Branch distance is too far away: NextIP: 0x{num:X16} target: 0x{ImmediateHi:X8}{Immediate:X8}, diff = {num2}, diff must fit in an Int8";
			}
			WriteByteInternal((uint)num2);
			break;
		}
		case ImmSize.RipRelSize2_Target16:
		{
			uint num4 = (uint)((int)currentRip + 2);
			uint num3 = Immediate - num4;
			WriteByteInternal(num3);
			WriteByteInternal(num3 >> 8);
			break;
		}
		case ImmSize.RipRelSize2_Target32:
		{
			uint num4 = (uint)((int)currentRip + 2);
			int num5 = (int)(Immediate - num4);
			if (num5 < -32768 || num5 > 32767)
			{
				ErrorMessage = $"Branch distance is too far away: NextIP: 0x{num4:X8} target: 0x{Immediate:X8}, diff = {num5}, diff must fit in an Int16";
			}
			uint num3 = (uint)num5;
			WriteByteInternal(num3);
			WriteByteInternal(num3 >> 8);
			break;
		}
		case ImmSize.RipRelSize2_Target64:
		{
			ulong num = currentRip + 2;
			long num2 = (long)((((ulong)ImmediateHi << 32) | Immediate) - num);
			if (num2 < -32768 || num2 > 32767)
			{
				ErrorMessage = $"Branch distance is too far away: NextIP: 0x{num:X16} target: 0x{ImmediateHi:X8}{Immediate:X8}, diff = {num2}, diff must fit in an Int16";
			}
			uint num3 = (uint)num2;
			WriteByteInternal(num3);
			WriteByteInternal(num3 >> 8);
			break;
		}
		case ImmSize.RipRelSize4_Target32:
		{
			uint num4 = (uint)((int)currentRip + 4);
			uint num3 = Immediate - num4;
			WriteByteInternal(num3);
			WriteByteInternal(num3 >> 8);
			WriteByteInternal(num3 >> 16);
			WriteByteInternal(num3 >> 24);
			break;
		}
		case ImmSize.RipRelSize4_Target64:
		{
			ulong num = currentRip + 4;
			long num2 = (long)((((ulong)ImmediateHi << 32) | Immediate) - num);
			if (num2 < int.MinValue || num2 > int.MaxValue)
			{
				ErrorMessage = $"Branch distance is too far away: NextIP: 0x{num:X16} target: 0x{ImmediateHi:X8}{Immediate:X8}, diff = {num2}, diff must fit in an Int32";
			}
			uint num3 = (uint)num2;
			WriteByteInternal(num3);
			WriteByteInternal(num3 >> 8);
			WriteByteInternal(num3 >> 16);
			WriteByteInternal(num3 >> 24);
			break;
		}
		default:
			throw new InvalidOperationException();
		case ImmSize.None:
			break;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void WriteByte(byte value)
	{
		WriteByteInternal(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void WriteByteInternal(uint value)
	{
		writer.WriteByte((byte)value);
		currentRip++;
	}

	public ConstantOffsets GetConstantOffsets()
	{
		ConstantOffsets result = default(ConstantOffsets);
		switch (DisplSize)
		{
		case DisplSize.Size1:
			result.DisplacementSize = 1;
			result.DisplacementOffset = (byte)(displAddr - eip);
			break;
		case DisplSize.Size2:
			result.DisplacementSize = 2;
			result.DisplacementOffset = (byte)(displAddr - eip);
			break;
		case DisplSize.Size4:
		case DisplSize.RipRelSize4_Target32:
		case DisplSize.RipRelSize4_Target64:
			result.DisplacementSize = 4;
			result.DisplacementOffset = (byte)(displAddr - eip);
			break;
		case DisplSize.Size8:
			result.DisplacementSize = 8;
			result.DisplacementOffset = (byte)(displAddr - eip);
			break;
		default:
			throw new InvalidOperationException();
		case DisplSize.None:
			break;
		}
		switch (ImmSize)
		{
		case ImmSize.Size1:
		case ImmSize.RipRelSize1_Target16:
		case ImmSize.RipRelSize1_Target32:
		case ImmSize.RipRelSize1_Target64:
			result.ImmediateSize = 1;
			result.ImmediateOffset = (byte)(immAddr - eip);
			break;
		case ImmSize.Size1_1:
			result.ImmediateSize = 1;
			result.ImmediateOffset = (byte)(immAddr - eip);
			result.ImmediateSize2 = 1;
			result.ImmediateOffset2 = (byte)(immAddr - eip + 1);
			break;
		case ImmSize.Size2:
		case ImmSize.RipRelSize2_Target16:
		case ImmSize.RipRelSize2_Target32:
		case ImmSize.RipRelSize2_Target64:
			result.ImmediateSize = 2;
			result.ImmediateOffset = (byte)(immAddr - eip);
			break;
		case ImmSize.Size2_1:
			result.ImmediateSize = 2;
			result.ImmediateOffset = (byte)(immAddr - eip);
			result.ImmediateSize2 = 1;
			result.ImmediateOffset2 = (byte)(immAddr - eip + 2);
			break;
		case ImmSize.Size2_2:
			result.ImmediateSize = 2;
			result.ImmediateOffset = (byte)(immAddr - eip);
			result.ImmediateSize2 = 2;
			result.ImmediateOffset2 = (byte)(immAddr - eip + 2);
			break;
		case ImmSize.Size4:
		case ImmSize.RipRelSize4_Target32:
		case ImmSize.RipRelSize4_Target64:
			result.ImmediateSize = 4;
			result.ImmediateOffset = (byte)(immAddr - eip);
			break;
		case ImmSize.Size4_2:
			result.ImmediateSize = 4;
			result.ImmediateOffset = (byte)(immAddr - eip);
			result.ImmediateSize2 = 2;
			result.ImmediateOffset2 = (byte)(immAddr - eip + 4);
			break;
		case ImmSize.Size8:
			result.ImmediateSize = 8;
			result.ImmediateOffset = (byte)(immAddr - eip);
			break;
		default:
			throw new InvalidOperationException();
		case ImmSize.None:
		case ImmSize.SizeIbReg:
		case ImmSize.Size1OpCode:
			break;
		}
		return result;
	}
}
