using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Iced.Intel.DecoderInternal;

namespace Iced.Intel;

internal sealed class Decoder : IEnumerable<Instruction>, IEnumerable
{
	internal struct ZState
	{
		public uint instructionLength;

		public uint extraRegisterBase;

		public uint extraIndexRegisterBase;

		public uint extraBaseRegisterBase;

		public uint extraIndexRegisterBaseVSIB;

		public StateFlags flags;

		public MandatoryPrefixByte mandatoryPrefix;

		public byte segmentPrio;
	}

	internal struct State
	{
		public uint modrm;

		public uint mod;

		public uint reg;

		public uint rm;

		public ZState zs;

		public uint vvvv;

		public uint vvvv_invalidCheck;

		public uint aaa;

		public uint extraRegisterBaseEVEX;

		public uint extraBaseRegisterBaseEVEX;

		public uint vectorLength;

		public OpSize operandSize;

		public OpSize addressSize;

		public readonly EncodingKind Encoding => (EncodingKind)(((uint)zs.flags >> 29) & 7);
	}

	private readonly struct RegInfo2
	{
		public readonly Register baseReg;

		public readonly Register indexReg;

		public RegInfo2(Register baseReg, Register indexReg)
		{
			this.baseReg = baseReg;
			this.indexReg = indexReg;
		}

		public void Deconstruct(out Register baseReg, out Register indexReg)
		{
			baseReg = this.baseReg;
			indexReg = this.indexReg;
		}
	}

	public struct Enumerator : IEnumerator<Instruction>, IDisposable, IEnumerator
	{
		private readonly Decoder decoder;

		private Instruction instruction;

		public Instruction Current => instruction;

		Instruction IEnumerator<Instruction>.Current => Current;

		object IEnumerator.Current => Current;

		internal Enumerator(Decoder decoder)
		{
			this.decoder = decoder;
			instruction = default(Instruction);
		}

		public bool MoveNext()
		{
			decoder.Decode(out instruction);
			return instruction.Length != 0;
		}

		void IEnumerator.Reset()
		{
			throw new InvalidOperationException();
		}

		public void Dispose()
		{
		}
	}

	private ulong instructionPointer;

	private readonly CodeReader reader;

	private readonly RegInfo2[] memRegs16;

	private readonly OpCodeHandler[] handlers_MAP0;

	private readonly OpCodeHandler[] handlers_VEX_0F;

	private readonly OpCodeHandler[] handlers_VEX_0F38;

	private readonly OpCodeHandler[] handlers_VEX_0F3A;

	private readonly OpCodeHandler[] handlers_EVEX_0F;

	private readonly OpCodeHandler[] handlers_EVEX_0F38;

	private readonly OpCodeHandler[] handlers_EVEX_0F3A;

	private readonly OpCodeHandler[] handlers_EVEX_MAP5;

	private readonly OpCodeHandler[] handlers_EVEX_MAP6;

	private readonly OpCodeHandler[] handlers_XOP_MAP8;

	private readonly OpCodeHandler[] handlers_XOP_MAP9;

	private readonly OpCodeHandler[] handlers_XOP_MAP10;

	internal State state;

	internal uint displIndex;

	internal readonly DecoderOptions options;

	internal readonly uint invalidCheckMask;

	internal readonly uint is64bMode_and_W;

	internal readonly uint reg15Mask;

	private readonly uint maskE0;

	private readonly uint rexMask;

	internal readonly CodeSize defaultCodeSize;

	internal readonly OpSize defaultOperandSize;

	private readonly OpSize defaultAddressSize;

	internal readonly OpSize defaultInvertedOperandSize;

	internal readonly OpSize defaultInvertedAddressSize;

	internal readonly bool is64bMode;

	private static readonly RegInfo2[] s_memRegs16;

	public ulong IP
	{
		get
		{
			return instructionPointer;
		}
		set
		{
			instructionPointer = value;
		}
	}

	public int Bitness { get; }

	public DecoderError LastError
	{
		get
		{
			if ((state.zs.flags & StateFlags.NoMoreBytes) != 0)
			{
				return DecoderError.NoMoreBytes;
			}
			if ((state.zs.flags & StateFlags.IsInvalid) != 0)
			{
				return DecoderError.InvalidInstruction;
			}
			return DecoderError.None;
		}
	}

	static Decoder()
	{
		s_memRegs16 = new RegInfo2[8]
		{
			new RegInfo2(Register.BX, Register.SI),
			new RegInfo2(Register.BX, Register.DI),
			new RegInfo2(Register.BP, Register.SI),
			new RegInfo2(Register.BP, Register.DI),
			new RegInfo2(Register.SI, Register.None),
			new RegInfo2(Register.DI, Register.None),
			new RegInfo2(Register.BP, Register.None),
			new RegInfo2(Register.BX, Register.None)
		};
		_ = OpCodeHandler_Invalid.Instance;
		_ = InstructionMemorySizes.SizesNormal;
		_ = OpCodeHandler_D3NOW.CodeValues;
		_ = InstructionOpCounts.OpCount;
		_ = MnemonicUtilsData.toMnemonic;
	}

	private Decoder(CodeReader reader, ulong ip, DecoderOptions options, int bitness)
	{
		this.reader = reader ?? throw new ArgumentNullException("reader");
		instructionPointer = ip;
		this.options = options;
		invalidCheckMask = (((options & DecoderOptions.NoInvalidCheck) == 0) ? uint.MaxValue : 0u);
		memRegs16 = s_memRegs16;
		Bitness = bitness;
		switch (bitness)
		{
		case 64:
			is64bMode = true;
			defaultCodeSize = CodeSize.Code64;
			defaultOperandSize = OpSize.Size32;
			defaultInvertedOperandSize = OpSize.Size16;
			defaultAddressSize = OpSize.Size64;
			defaultInvertedAddressSize = OpSize.Size32;
			maskE0 = 224u;
			rexMask = 240u;
			break;
		case 32:
			is64bMode = false;
			defaultCodeSize = CodeSize.Code32;
			defaultOperandSize = OpSize.Size32;
			defaultInvertedOperandSize = OpSize.Size16;
			defaultAddressSize = OpSize.Size32;
			defaultInvertedAddressSize = OpSize.Size16;
			maskE0 = 0u;
			rexMask = 0u;
			break;
		default:
			is64bMode = false;
			defaultCodeSize = CodeSize.Code16;
			defaultOperandSize = OpSize.Size16;
			defaultInvertedOperandSize = OpSize.Size32;
			defaultAddressSize = OpSize.Size16;
			defaultInvertedAddressSize = OpSize.Size32;
			maskE0 = 0u;
			rexMask = 0u;
			break;
		}
		is64bMode_and_W = (is64bMode ? 128u : 0u);
		reg15Mask = (is64bMode ? 15u : 7u);
		handlers_MAP0 = OpCodeHandlersTables_Legacy.Handlers_MAP0;
		handlers_VEX_0F = OpCodeHandlersTables_VEX.Handlers_0F;
		handlers_VEX_0F38 = OpCodeHandlersTables_VEX.Handlers_0F38;
		handlers_VEX_0F3A = OpCodeHandlersTables_VEX.Handlers_0F3A;
		handlers_EVEX_0F = OpCodeHandlersTables_EVEX.Handlers_0F;
		handlers_EVEX_0F38 = OpCodeHandlersTables_EVEX.Handlers_0F38;
		handlers_EVEX_0F3A = OpCodeHandlersTables_EVEX.Handlers_0F3A;
		handlers_EVEX_MAP5 = OpCodeHandlersTables_EVEX.Handlers_MAP5;
		handlers_EVEX_MAP6 = OpCodeHandlersTables_EVEX.Handlers_MAP6;
		handlers_XOP_MAP8 = OpCodeHandlersTables_XOP.Handlers_MAP8;
		handlers_XOP_MAP9 = OpCodeHandlersTables_XOP.Handlers_MAP9;
		handlers_XOP_MAP10 = OpCodeHandlersTables_XOP.Handlers_MAP10;
	}

	public static Decoder Create(int bitness, CodeReader reader, ulong ip, DecoderOptions options = DecoderOptions.None)
	{
		if (bitness == 16 || bitness == 32 || bitness == 64)
		{
			return new Decoder(reader, ip, options, bitness);
		}
		throw new ArgumentOutOfRangeException("bitness");
	}

	public static Decoder Create(int bitness, byte[] data, ulong ip, DecoderOptions options = DecoderOptions.None)
	{
		return Create(bitness, new ByteArrayCodeReader(data), ip, options);
	}

	public static Decoder Create(int bitness, CodeReader reader, DecoderOptions options = DecoderOptions.None)
	{
		return Create(bitness, reader, 0uL, options);
	}

	public static Decoder Create(int bitness, byte[] data, DecoderOptions options = DecoderOptions.None)
	{
		return Create(bitness, new ByteArrayCodeReader(data), 0uL, options);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal uint ReadByte()
	{
		uint instructionLength = state.zs.instructionLength;
		if (instructionLength < 15)
		{
			uint num = (uint)reader.ReadByte();
			if (num <= 255)
			{
				state.zs.instructionLength = instructionLength + 1;
				return num;
			}
			state.zs.flags |= StateFlags.NoMoreBytes;
		}
		state.zs.flags |= StateFlags.IsInvalid;
		return 0u;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal uint ReadUInt16()
	{
		return ReadByte() | (ReadByte() << 8);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal uint ReadUInt32()
	{
		return ReadByte() | (ReadByte() << 8) | (ReadByte() << 16) | (ReadByte() << 24);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal ulong ReadUInt64()
	{
		return ReadUInt32() | ((ulong)ReadUInt32() << 32);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Instruction Decode()
	{
		Decode(out var instruction);
		return instruction;
	}

	public void Decode(out Instruction instruction)
	{
		instruction = default(Instruction);
		state.zs = default(ZState);
		state.operandSize = defaultOperandSize;
		state.addressSize = defaultAddressSize;
		uint num = ReadByte();
		if ((num & rexMask) == 64)
		{
			StateFlags stateFlags = state.zs.flags | StateFlags.HasRex;
			if ((num & 8) != 0)
			{
				stateFlags |= StateFlags.W;
				state.operandSize = OpSize.Size64;
			}
			state.zs.flags = stateFlags;
			state.zs.extraRegisterBase = (num << 1) & 8;
			state.zs.extraIndexRegisterBase = (num << 2) & 8;
			state.zs.extraBaseRegisterBase = (num << 3) & 8;
			num = ReadByte();
		}
		DecodeTable(handlers_MAP0[num], ref instruction);
		instruction.InternalCodeSize = defaultCodeSize;
		uint num2 = (uint)(instruction.Length = (int)state.zs.instructionLength);
		ulong num3 = instructionPointer;
		num3 = (instruction.NextIP = (instructionPointer = num3 + num2));
		StateFlags flags = state.zs.flags;
		if ((flags & (StateFlags.IpRel64 | StateFlags.IpRel32 | StateFlags.IsInvalid | StateFlags.Lock)) == 0)
		{
			return;
		}
		ulong num5 = (instruction.MemoryDisplacement64 += num3);
		if ((flags & (StateFlags.IpRel64 | StateFlags.IsInvalid | StateFlags.Lock)) != StateFlags.IpRel64)
		{
			if ((flags & StateFlags.IpRel64) == 0)
			{
				instruction.MemoryDisplacement64 = num5 - num3;
			}
			if ((flags & StateFlags.IpRel32) != 0)
			{
				instruction.MemoryDisplacement64 = (uint)((int)instruction.MemoryDisplacement64 + (int)num3);
			}
			if ((flags & StateFlags.IsInvalid) != 0 || ((uint)(flags & (StateFlags.Lock | StateFlags.AllowLock)) & invalidCheckMask) == 4096)
			{
				instruction = default(Instruction);
				state.zs.flags = flags | StateFlags.IsInvalid;
				instruction.InternalCodeSize = defaultCodeSize;
				instruction.Length = (int)num2;
				instruction.NextIP = num3;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void ResetRexPrefixState()
	{
		state.zs.flags &= ~(StateFlags.HasRex | StateFlags.W);
		if ((state.zs.flags & StateFlags.Has66) == 0)
		{
			state.operandSize = defaultOperandSize;
		}
		else
		{
			state.operandSize = defaultInvertedOperandSize;
		}
		state.zs.extraRegisterBase = 0u;
		state.zs.extraIndexRegisterBase = 0u;
		state.zs.extraBaseRegisterBase = 0u;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void CallOpCodeHandlerXXTable(ref Instruction instruction)
	{
		uint num = ReadByte();
		DecodeTable(handlers_MAP0[num], ref instruction);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal uint GetCurrentInstructionPointer32()
	{
		return (uint)(int)instructionPointer + state.zs.instructionLength;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal ulong GetCurrentInstructionPointer64()
	{
		return instructionPointer + state.zs.instructionLength;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void ClearMandatoryPrefix(ref Instruction instruction)
	{
		instruction.InternalClearHasRepeRepnePrefix();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void SetXacquireXrelease(ref Instruction instruction)
	{
		if (instruction.HasLockPrefix)
		{
			if (state.zs.mandatoryPrefix == MandatoryPrefixByte.PF2)
			{
				ClearMandatoryPrefixF2(ref instruction);
				instruction.InternalSetHasXacquirePrefix();
			}
			else if (state.zs.mandatoryPrefix == MandatoryPrefixByte.PF3)
			{
				ClearMandatoryPrefixF3(ref instruction);
				instruction.InternalSetHasXreleasePrefix();
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void ClearMandatoryPrefixF3(ref Instruction instruction)
	{
		instruction.InternalClearHasRepePrefix();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void ClearMandatoryPrefixF2(ref Instruction instruction)
	{
		instruction.InternalClearHasRepnePrefix();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void SetInvalidInstruction()
	{
		state.zs.flags |= StateFlags.IsInvalid;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void DecodeTable(OpCodeHandler[] table, ref Instruction instruction)
	{
		DecodeTable(table[ReadByte()], ref instruction);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void DecodeTable(OpCodeHandler handler, ref Instruction instruction)
	{
		if (handler.HasModRM)
		{
			uint num = ReadByte();
			state.modrm = num;
			state.mod = num >> 6;
			state.reg = (num >> 3) & 7;
			state.rm = num & 7;
		}
		handler.Decode(this, ref instruction);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void ReadModRM()
	{
		uint num = ReadByte();
		state.modrm = num;
		state.mod = num >> 6;
		state.reg = (num >> 3) & 7;
		state.rm = num & 7;
	}

	internal void VEX2(ref Instruction instruction)
	{
		if ((((uint)(state.zs.flags & StateFlags.HasRex) | (uint)state.zs.mandatoryPrefix) & invalidCheckMask) != 0)
		{
			SetInvalidInstruction();
		}
		state.zs.flags &= ~StateFlags.W;
		state.zs.extraIndexRegisterBase = 0u;
		state.zs.extraBaseRegisterBase = 0u;
		uint modrm = state.modrm;
		state.vectorLength = (modrm >> 2) & 1;
		state.zs.mandatoryPrefix = (MandatoryPrefixByte)(modrm & 3);
		modrm = ~modrm;
		state.zs.extraRegisterBase = (modrm >> 4) & 8;
		modrm = (modrm >> 3) & 0xF;
		state.vvvv = modrm;
		state.vvvv_invalidCheck = modrm;
		DecodeTable(handlers_VEX_0F, ref instruction);
	}

	internal void VEX3(ref Instruction instruction)
	{
		if ((((uint)(state.zs.flags & StateFlags.HasRex) | (uint)state.zs.mandatoryPrefix) & invalidCheckMask) != 0)
		{
			SetInvalidInstruction();
		}
		state.zs.flags &= ~StateFlags.W;
		uint num = ReadByte();
		state.zs.flags |= (StateFlags)(num & 0x80);
		state.vectorLength = (num >> 2) & 1;
		state.zs.mandatoryPrefix = (MandatoryPrefixByte)(num & 3);
		num = (~num >> 3) & 0xF;
		state.vvvv_invalidCheck = num;
		state.vvvv = num & reg15Mask;
		uint modrm = state.modrm;
		uint num2 = ~modrm & maskE0;
		state.zs.extraRegisterBase = (num2 >> 4) & 8;
		state.zs.extraIndexRegisterBase = (num2 >> 3) & 8;
		state.zs.extraBaseRegisterBase = (num2 >> 2) & 8;
		uint num3 = ReadByte();
		OpCodeHandler[] array;
		switch ((int)(modrm & 0x1F))
		{
		case 1:
			array = handlers_VEX_0F;
			break;
		case 2:
			array = handlers_VEX_0F38;
			break;
		case 3:
			array = handlers_VEX_0F3A;
			break;
		default:
			SetInvalidInstruction();
			return;
		}
		DecodeTable(array[num3], ref instruction);
	}

	internal void XOP(ref Instruction instruction)
	{
		if ((((uint)(state.zs.flags & StateFlags.HasRex) | (uint)state.zs.mandatoryPrefix) & invalidCheckMask) != 0)
		{
			SetInvalidInstruction();
		}
		state.zs.flags &= ~StateFlags.W;
		uint num = ReadByte();
		state.zs.flags |= (StateFlags)(num & 0x80);
		state.vectorLength = (num >> 2) & 1;
		state.zs.mandatoryPrefix = (MandatoryPrefixByte)(num & 3);
		num = (~num >> 3) & 0xF;
		state.vvvv_invalidCheck = num;
		state.vvvv = num & reg15Mask;
		uint modrm = state.modrm;
		uint num2 = ~modrm & maskE0;
		state.zs.extraRegisterBase = (num2 >> 4) & 8;
		state.zs.extraIndexRegisterBase = (num2 >> 3) & 8;
		state.zs.extraBaseRegisterBase = (num2 >> 2) & 8;
		uint num3 = ReadByte();
		OpCodeHandler[] array;
		switch ((int)(modrm & 0x1F))
		{
		case 8:
			array = handlers_XOP_MAP8;
			break;
		case 9:
			array = handlers_XOP_MAP9;
			break;
		case 10:
			array = handlers_XOP_MAP10;
			break;
		default:
			SetInvalidInstruction();
			return;
		}
		DecodeTable(array[num3], ref instruction);
	}

	internal void EVEX_MVEX(ref Instruction instruction)
	{
		if ((((uint)(state.zs.flags & StateFlags.HasRex) | (uint)state.zs.mandatoryPrefix) & invalidCheckMask) != 0)
		{
			SetInvalidInstruction();
		}
		state.zs.flags &= ~StateFlags.W;
		uint modrm = state.modrm;
		uint num = ReadByte();
		uint num2 = ReadByte();
		uint num3 = ReadByte();
		uint num4 = ReadByte();
		if ((num & 4) != 0)
		{
			if ((modrm & 8) == 0)
			{
				state.zs.mandatoryPrefix = (MandatoryPrefixByte)(num & 3);
				state.zs.flags |= (StateFlags)(num & 0x80);
				uint num5 = num2 & 7;
				state.aaa = num5;
				instruction.InternalOpMask = num5;
				if ((num2 & 0x80) != 0)
				{
					if ((num5 ^ invalidCheckMask) == uint.MaxValue)
					{
						SetInvalidInstruction();
					}
					state.zs.flags |= StateFlags.z;
					instruction.InternalSetZeroingMasking();
				}
				state.zs.flags |= (StateFlags)(num2 & 0x10);
				state.vectorLength = (num2 >> 5) & 3;
				num = (~num >> 3) & 0xF;
				if (is64bMode)
				{
					uint num6 = (~num2 & 8) << 1;
					state.zs.extraIndexRegisterBaseVSIB = num6;
					num6 += num;
					state.vvvv = num6;
					state.vvvv_invalidCheck = num6;
					uint num7 = ~modrm;
					state.zs.extraRegisterBase = (num7 >> 4) & 8;
					state.zs.extraIndexRegisterBase = (num7 >> 3) & 8;
					state.extraRegisterBaseEVEX = num7 & 0x10;
					num7 >>= 2;
					state.extraBaseRegisterBaseEVEX = num7 & 0x18;
					state.zs.extraBaseRegisterBase = num7 & 8;
				}
				else
				{
					state.vvvv_invalidCheck = num;
					state.vvvv = num & 7;
					state.zs.flags |= (StateFlags)((~num2 & 8) << 3);
				}
				OpCodeHandler[] array;
				switch ((int)(modrm & 7))
				{
				case 1:
					array = handlers_EVEX_0F;
					break;
				case 2:
					array = handlers_EVEX_0F38;
					break;
				case 3:
					array = handlers_EVEX_0F3A;
					break;
				case 5:
					array = handlers_EVEX_MAP5;
					break;
				case 6:
					array = handlers_EVEX_MAP6;
					break;
				default:
					SetInvalidInstruction();
					return;
				}
				OpCodeHandler obj = array[num3];
				state.modrm = num4;
				state.mod = num4 >> 6;
				state.reg = (num4 >> 3) & 7;
				state.rm = num4 & 7;
				if ((((uint)(state.zs.flags & StateFlags.b) | state.vectorLength) & invalidCheckMask) == 3)
				{
					SetInvalidInstruction();
				}
				obj.Decode(this, ref instruction);
			}
			else
			{
				SetInvalidInstruction();
			}
		}
		else
		{
			SetInvalidInstruction();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal Register ReadOpSegReg()
	{
		uint reg = state.reg;
		if (reg < 6)
		{
			return (Register)(71 + reg);
		}
		SetInvalidInstruction();
		return Register.None;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool ReadOpMem(ref Instruction instruction)
	{
		if (state.addressSize == OpSize.Size64)
		{
			return ReadOpMem32Or64(ref instruction, Register.RAX, Register.RAX, TupleType.N1, isVsib: false);
		}
		if (state.addressSize == OpSize.Size32)
		{
			return ReadOpMem32Or64(ref instruction, Register.EAX, Register.EAX, TupleType.N1, isVsib: false);
		}
		ReadOpMem16(ref instruction, TupleType.N1);
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void ReadOpMemSib(ref Instruction instruction)
	{
		bool flag;
		if (state.addressSize == OpSize.Size64)
		{
			flag = ReadOpMem32Or64(ref instruction, Register.RAX, Register.RAX, TupleType.N1, isVsib: false);
		}
		else if (state.addressSize == OpSize.Size32)
		{
			flag = ReadOpMem32Or64(ref instruction, Register.EAX, Register.EAX, TupleType.N1, isVsib: false);
		}
		else
		{
			ReadOpMem16(ref instruction, TupleType.N1);
			flag = false;
		}
		if (invalidCheckMask != 0 && !flag)
		{
			SetInvalidInstruction();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void ReadOpMem_MPX(ref Instruction instruction)
	{
		if (is64bMode)
		{
			state.addressSize = OpSize.Size64;
			ReadOpMem32Or64(ref instruction, Register.RAX, Register.RAX, TupleType.N1, isVsib: false);
			return;
		}
		if (state.addressSize == OpSize.Size32)
		{
			ReadOpMem32Or64(ref instruction, Register.EAX, Register.EAX, TupleType.N1, isVsib: false);
			return;
		}
		ReadOpMem16(ref instruction, TupleType.N1);
		if (invalidCheckMask != 0)
		{
			SetInvalidInstruction();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void ReadOpMem(ref Instruction instruction, TupleType tupleType)
	{
		if (state.addressSize == OpSize.Size64)
		{
			ReadOpMem32Or64(ref instruction, Register.RAX, Register.RAX, tupleType, isVsib: false);
		}
		else if (state.addressSize == OpSize.Size32)
		{
			ReadOpMem32Or64(ref instruction, Register.EAX, Register.EAX, tupleType, isVsib: false);
		}
		else
		{
			ReadOpMem16(ref instruction, tupleType);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void ReadOpMem_VSIB(ref Instruction instruction, Register vsibIndex, TupleType tupleType)
	{
		bool flag;
		if (state.addressSize == OpSize.Size64)
		{
			flag = ReadOpMem32Or64(ref instruction, Register.RAX, vsibIndex, tupleType, isVsib: true);
		}
		else if (state.addressSize == OpSize.Size32)
		{
			flag = ReadOpMem32Or64(ref instruction, Register.EAX, vsibIndex, tupleType, isVsib: true);
		}
		else
		{
			ReadOpMem16(ref instruction, tupleType);
			flag = false;
		}
		if (invalidCheckMask != 0 && !flag)
		{
			SetInvalidInstruction();
		}
	}

	private void ReadOpMem16(ref Instruction instruction, TupleType tupleType)
	{
		RegInfo2 regInfo = memRegs16[state.rm];
		var (internalMemoryBase, internalMemoryIndex) = (RegInfo2)(ref regInfo);
		switch ((int)state.mod)
		{
		case 0:
			if (state.rm == 6)
			{
				instruction.InternalSetMemoryDisplSize(2u);
				displIndex = state.zs.instructionLength;
				instruction.MemoryDisplacement64 = ReadUInt16();
				internalMemoryBase = Register.None;
			}
			break;
		case 1:
			instruction.InternalSetMemoryDisplSize(1u);
			displIndex = state.zs.instructionLength;
			if (tupleType == TupleType.N1)
			{
				instruction.MemoryDisplacement64 = (ushort)(sbyte)ReadByte();
			}
			else
			{
				instruction.MemoryDisplacement64 = (ushort)(GetDisp8N(tupleType) * (uint)(sbyte)ReadByte());
			}
			break;
		default:
			instruction.InternalSetMemoryDisplSize(2u);
			displIndex = state.zs.instructionLength;
			instruction.MemoryDisplacement64 = ReadUInt16();
			break;
		}
		instruction.InternalMemoryBase = internalMemoryBase;
		instruction.InternalMemoryIndex = internalMemoryIndex;
	}

	private bool ReadOpMem32Or64(ref Instruction instruction, Register baseReg, Register indexReg, TupleType tupleType, bool isVsib)
	{
		uint num;
		uint scale;
		uint num2;
		switch ((int)state.mod)
		{
		case 0:
			if (state.rm == 4)
			{
				num = ReadByte();
				scale = 0u;
				num2 = 0u;
				break;
			}
			if (state.rm == 5)
			{
				displIndex = state.zs.instructionLength;
				if (state.addressSize == OpSize.Size64)
				{
					instruction.MemoryDisplacement64 = (ulong)(int)ReadUInt32();
					instruction.InternalSetMemoryDisplSize(4u);
				}
				else
				{
					instruction.MemoryDisplacement64 = ReadUInt32();
					instruction.InternalSetMemoryDisplSize(3u);
				}
				if (is64bMode)
				{
					if (state.addressSize == OpSize.Size64)
					{
						state.zs.flags |= StateFlags.IpRel64;
						instruction.InternalMemoryBase = Register.RIP;
					}
					else
					{
						state.zs.flags |= StateFlags.IpRel32;
						instruction.InternalMemoryBase = Register.EIP;
					}
				}
				return false;
			}
			instruction.InternalMemoryBase = (Register)((int)(state.zs.extraBaseRegisterBase + state.rm) + (int)baseReg);
			return false;
		case 1:
			if (state.rm == 4)
			{
				num = ReadByte();
				scale = 1u;
				displIndex = state.zs.instructionLength;
				num2 = ((tupleType != 0) ? (GetDisp8N(tupleType) * (uint)(sbyte)ReadByte()) : ((uint)(sbyte)ReadByte()));
				break;
			}
			instruction.InternalSetMemoryDisplSize(1u);
			displIndex = state.zs.instructionLength;
			if (state.addressSize == OpSize.Size64)
			{
				if (tupleType == TupleType.N1)
				{
					instruction.MemoryDisplacement64 = (ulong)(sbyte)ReadByte();
				}
				else
				{
					instruction.MemoryDisplacement64 = (ulong)(GetDisp8N(tupleType) * (sbyte)ReadByte());
				}
			}
			else if (tupleType == TupleType.N1)
			{
				instruction.MemoryDisplacement64 = (uint)(sbyte)ReadByte();
			}
			else
			{
				instruction.MemoryDisplacement64 = GetDisp8N(tupleType) * (uint)(sbyte)ReadByte();
			}
			instruction.InternalMemoryBase = (Register)((int)(state.zs.extraBaseRegisterBase + state.rm) + (int)baseReg);
			return false;
		default:
			if (state.rm == 4)
			{
				num = ReadByte();
				scale = ((state.addressSize == OpSize.Size64) ? 4u : 3u);
				displIndex = state.zs.instructionLength;
				num2 = ReadUInt32();
				break;
			}
			displIndex = state.zs.instructionLength;
			if (state.addressSize == OpSize.Size64)
			{
				instruction.MemoryDisplacement64 = (ulong)(int)ReadUInt32();
				instruction.InternalSetMemoryDisplSize(4u);
			}
			else
			{
				instruction.MemoryDisplacement64 = ReadUInt32();
				instruction.InternalSetMemoryDisplSize(3u);
			}
			instruction.InternalMemoryBase = (Register)((int)(state.zs.extraBaseRegisterBase + state.rm) + (int)baseReg);
			return false;
		}
		uint num3 = ((num >> 3) & 7) + state.zs.extraIndexRegisterBase;
		uint num4 = num & 7;
		instruction.InternalMemoryIndexScale = (int)(num >> 6);
		if (!isVsib)
		{
			if (num3 != 4)
			{
				instruction.InternalMemoryIndex = (Register)((int)num3 + (int)indexReg);
			}
		}
		else
		{
			instruction.InternalMemoryIndex = (Register)((int)(num3 + state.zs.extraIndexRegisterBaseVSIB) + (int)indexReg);
		}
		if (num4 == 5 && state.mod == 0)
		{
			displIndex = state.zs.instructionLength;
			if (state.addressSize == OpSize.Size64)
			{
				instruction.MemoryDisplacement64 = (ulong)(int)ReadUInt32();
				instruction.InternalSetMemoryDisplSize(4u);
			}
			else
			{
				instruction.MemoryDisplacement64 = ReadUInt32();
				instruction.InternalSetMemoryDisplSize(3u);
			}
		}
		else
		{
			instruction.InternalMemoryBase = (Register)((int)(num4 + state.zs.extraBaseRegisterBase) + (int)baseReg);
			instruction.InternalSetMemoryDisplSize(scale);
			if (state.addressSize == OpSize.Size64)
			{
				instruction.MemoryDisplacement64 = (ulong)(int)num2;
			}
			else
			{
				instruction.MemoryDisplacement64 = num2;
			}
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private uint GetDisp8N(TupleType tupleType)
	{
		return TupleTypeTable.GetDisp8N(tupleType, (state.zs.flags & StateFlags.b) != 0);
	}

	public ConstantOffsets GetConstantOffsets(in Instruction instruction)
	{
		ConstantOffsets result = default(ConstantOffsets);
		int memoryDisplSize = instruction.MemoryDisplSize;
		if (memoryDisplSize != 0)
		{
			result.DisplacementOffset = (byte)displIndex;
			if (memoryDisplSize == 8 && (state.zs.flags & StateFlags.Addr64) == 0)
			{
				result.DisplacementSize = 4;
			}
			else
			{
				result.DisplacementSize = (byte)memoryDisplSize;
			}
		}
		if ((state.zs.flags & StateFlags.NoImm) == 0)
		{
			int num = 0;
			for (int num2 = instruction.OpCount - 1; num2 >= 0; num2--)
			{
				switch (instruction.GetOpKind(num2))
				{
				case OpKind.Immediate8:
				case OpKind.Immediate8to16:
				case OpKind.Immediate8to32:
				case OpKind.Immediate8to64:
					result.ImmediateOffset = (byte)(instruction.Length - num - 1);
					result.ImmediateSize = 1;
					break;
				case OpKind.Immediate16:
					result.ImmediateOffset = (byte)(instruction.Length - num - 2);
					result.ImmediateSize = 2;
					break;
				case OpKind.Immediate32:
				case OpKind.Immediate32to64:
					result.ImmediateOffset = (byte)(instruction.Length - num - 4);
					result.ImmediateSize = 4;
					break;
				case OpKind.Immediate64:
					result.ImmediateOffset = (byte)(instruction.Length - num - 8);
					result.ImmediateSize = 8;
					break;
				case OpKind.Immediate8_2nd:
					result.ImmediateOffset2 = (byte)(instruction.Length - 1);
					result.ImmediateSize2 = 1;
					num = 1;
					continue;
				case OpKind.NearBranch16:
					if ((state.zs.flags & StateFlags.BranchImm8) != 0)
					{
						result.ImmediateOffset = (byte)(instruction.Length - 1);
						result.ImmediateSize = 1;
					}
					else if ((state.zs.flags & StateFlags.Xbegin) == 0)
					{
						result.ImmediateOffset = (byte)(instruction.Length - 2);
						result.ImmediateSize = 2;
					}
					else if (state.operandSize != 0)
					{
						result.ImmediateOffset = (byte)(instruction.Length - 4);
						result.ImmediateSize = 4;
					}
					else
					{
						result.ImmediateOffset = (byte)(instruction.Length - 2);
						result.ImmediateSize = 2;
					}
					continue;
				case OpKind.NearBranch32:
				case OpKind.NearBranch64:
					if ((state.zs.flags & StateFlags.BranchImm8) != 0)
					{
						result.ImmediateOffset = (byte)(instruction.Length - 1);
						result.ImmediateSize = 1;
					}
					else if ((state.zs.flags & StateFlags.Xbegin) == 0)
					{
						result.ImmediateOffset = (byte)(instruction.Length - 4);
						result.ImmediateSize = 4;
					}
					else if (state.operandSize != 0)
					{
						result.ImmediateOffset = (byte)(instruction.Length - 4);
						result.ImmediateSize = 4;
					}
					else
					{
						result.ImmediateOffset = (byte)(instruction.Length - 2);
						result.ImmediateSize = 2;
					}
					continue;
				case OpKind.FarBranch16:
					result.ImmediateOffset = (byte)(instruction.Length - 4);
					result.ImmediateSize = 2;
					result.ImmediateOffset2 = (byte)(instruction.Length - 2);
					result.ImmediateSize2 = 2;
					continue;
				case OpKind.FarBranch32:
					result.ImmediateOffset = (byte)(instruction.Length - 6);
					result.ImmediateSize = 4;
					result.ImmediateOffset2 = (byte)(instruction.Length - 2);
					result.ImmediateSize2 = 2;
					continue;
				default:
					continue;
				}
				break;
			}
		}
		return result;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<Instruction> IEnumerable<Instruction>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
