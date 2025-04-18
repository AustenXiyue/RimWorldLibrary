using System;
using System.Runtime.CompilerServices;
using Iced.Intel.EncoderInternal;

namespace Iced.Intel;

internal struct Instruction : IEquatable<Instruction>
{
	[Flags]
	private enum InstrFlags1 : uint
	{
		SegmentPrefixMask = 7u,
		SegmentPrefixShift = 5u,
		DataLengthMask = 0xFu,
		DataLengthShift = 8u,
		RoundingControlMask = 7u,
		RoundingControlShift = 0xCu,
		OpMaskMask = 7u,
		OpMaskShift = 0xFu,
		CodeSizeMask = 3u,
		CodeSizeShift = 0x12u,
		Broadcast = 0x4000000u,
		SuppressAllExceptions = 0x8000000u,
		ZeroingMasking = 0x10000000u,
		RepePrefix = 0x20000000u,
		RepnePrefix = 0x40000000u,
		LockPrefix = 0x80000000u,
		EqualsIgnoreMask = 0xC0000u
	}

	[Flags]
	private enum MvexInstrFlags : uint
	{
		MvexRegMemConvShift = 0x10u,
		MvexRegMemConvMask = 0x1Fu,
		EvictionHint = 0x80000000u
	}

	internal const int TOTAL_SIZE = 40;

	private ulong nextRip;

	private ulong memDispl;

	private uint flags1;

	private uint immediate;

	private ushort code;

	private byte memBaseReg;

	private byte memIndexReg;

	private byte reg0;

	private byte reg1;

	private byte reg2;

	private byte reg3;

	private byte opKind0;

	private byte opKind1;

	private byte opKind2;

	private byte opKind3;

	private byte scale;

	private byte displSize;

	private byte len;

	private byte pad;

	public ushort IP16
	{
		readonly get
		{
			return (ushort)((int)nextRip - Length);
		}
		set
		{
			nextRip = (uint)(value + Length);
		}
	}

	public uint IP32
	{
		readonly get
		{
			return (uint)((int)nextRip - Length);
		}
		set
		{
			nextRip = value + (uint)Length;
		}
	}

	public ulong IP
	{
		readonly get
		{
			return nextRip - (uint)Length;
		}
		set
		{
			nextRip = value + (uint)Length;
		}
	}

	public ushort NextIP16
	{
		readonly get
		{
			return (ushort)nextRip;
		}
		set
		{
			nextRip = value;
		}
	}

	public uint NextIP32
	{
		readonly get
		{
			return (uint)nextRip;
		}
		set
		{
			nextRip = value;
		}
	}

	public ulong NextIP
	{
		readonly get
		{
			return nextRip;
		}
		set
		{
			nextRip = value;
		}
	}

	public CodeSize CodeSize
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return (CodeSize)((flags1 >> 18) & 3);
		}
		set
		{
			flags1 = (flags1 & 0xFFF3FFFFu) | (uint)((int)(value & CodeSize.Code64) << 18);
		}
	}

	internal CodeSize InternalCodeSize
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			flags1 |= (uint)((int)value << 18);
		}
	}

	public readonly bool IsInvalid
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return code == 0;
		}
	}

	public Code Code
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return (Code)code;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			if ((uint)value >= 4834u)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException_value();
			}
			code = (ushort)value;
		}
	}

	public readonly Mnemonic Mnemonic
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Code.Mnemonic();
		}
	}

	public readonly int OpCount
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return InstructionOpCounts.OpCount[code];
		}
	}

	public int Length
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return len;
		}
		set
		{
			len = (byte)value;
		}
	}

	internal readonly bool Internal_HasRepeOrRepnePrefix
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (flags1 & 0x60000000) != 0;
		}
	}

	internal readonly uint HasAnyOf_Lock_Rep_Repne_Prefix
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return flags1 & 0xE0000000u;
		}
	}

	public bool HasXacquirePrefix
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			if ((flags1 & 0x40000000) != 0)
			{
				return IsXacquireInstr();
			}
			return false;
		}
		set
		{
			if (value)
			{
				flags1 |= 1073741824u;
			}
			else
			{
				flags1 &= 3221225471u;
			}
		}
	}

	public bool HasXreleasePrefix
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			if ((flags1 & 0x20000000) != 0)
			{
				return IsXreleaseInstr();
			}
			return false;
		}
		set
		{
			if (value)
			{
				flags1 |= 536870912u;
			}
			else
			{
				flags1 &= 3758096383u;
			}
		}
	}

	public bool HasRepPrefix
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return (flags1 & 0x20000000) != 0;
		}
		set
		{
			if (value)
			{
				flags1 |= 536870912u;
			}
			else
			{
				flags1 &= 3758096383u;
			}
		}
	}

	public bool HasRepePrefix
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return (flags1 & 0x20000000) != 0;
		}
		set
		{
			if (value)
			{
				flags1 |= 536870912u;
			}
			else
			{
				flags1 &= 3758096383u;
			}
		}
	}

	public bool HasRepnePrefix
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return (flags1 & 0x40000000) != 0;
		}
		set
		{
			if (value)
			{
				flags1 |= 1073741824u;
			}
			else
			{
				flags1 &= 3221225471u;
			}
		}
	}

	public bool HasLockPrefix
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return (flags1 & 0x80000000u) != 0;
		}
		set
		{
			if (value)
			{
				flags1 |= 2147483648u;
			}
			else
			{
				flags1 &= 2147483647u;
			}
		}
	}

	public OpKind Op0Kind
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return (OpKind)opKind0;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			opKind0 = (byte)value;
		}
	}

	internal readonly bool Internal_Op0IsNotReg_or_Op1IsNotReg
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (opKind0 | opKind1) != 0;
		}
	}

	public OpKind Op1Kind
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return (OpKind)opKind1;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			opKind1 = (byte)value;
		}
	}

	public OpKind Op2Kind
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return (OpKind)opKind2;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			opKind2 = (byte)value;
		}
	}

	public OpKind Op3Kind
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return (OpKind)opKind3;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			opKind3 = (byte)value;
		}
	}

	public OpKind Op4Kind
	{
		readonly get
		{
			return OpKind.Immediate8;
		}
		set
		{
			if (value != OpKind.Immediate8)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException_value();
			}
		}
	}

	public readonly bool HasSegmentPrefix
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return ((flags1 >> 5) & 7) - 1 < 6;
		}
	}

	public Register SegmentPrefix
	{
		readonly get
		{
			uint num = ((flags1 >> 5) & 7) - 1;
			if (num >= 6)
			{
				return Register.None;
			}
			return (Register)(71 + num);
		}
		set
		{
			uint num = (uint)((value != 0) ? ((value - 71 + 1) & Register.DH) : Register.None);
			flags1 = (flags1 & 0xFFFFFF1Fu) | (num << 5);
		}
	}

	public readonly Register MemorySegment
	{
		get
		{
			Register segmentPrefix = SegmentPrefix;
			if (segmentPrefix != 0)
			{
				return segmentPrefix;
			}
			Register memoryBase = MemoryBase;
			if (memoryBase == Register.BP || memoryBase == Register.EBP || memoryBase == Register.ESP || memoryBase == Register.RBP || memoryBase == Register.RSP)
			{
				return Register.SS;
			}
			return Register.DS;
		}
	}

	public int MemoryDisplSize
	{
		readonly get
		{
			return displSize switch
			{
				0 => 0, 
				1 => 1, 
				2 => 2, 
				3 => 4, 
				_ => 8, 
			};
		}
		set
		{
			displSize = value switch
			{
				0 => 0, 
				1 => 1, 
				2 => 2, 
				4 => 3, 
				_ => 4, 
			};
		}
	}

	public bool IsBroadcast
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return (flags1 & 0x4000000) != 0;
		}
		set
		{
			if (value)
			{
				flags1 |= 67108864u;
			}
			else
			{
				flags1 &= 4227858431u;
			}
		}
	}

	public readonly MemorySize MemorySize
	{
		get
		{
			int index = (int)Code;
			if (IsBroadcast)
			{
				return (MemorySize)InstructionMemorySizes.SizesBcst[index];
			}
			return (MemorySize)InstructionMemorySizes.SizesNormal[index];
		}
	}

	public int MemoryIndexScale
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return 1 << (int)scale;
		}
		set
		{
			switch (value)
			{
			case 1:
				scale = 0;
				break;
			case 2:
				scale = 1;
				break;
			case 4:
				scale = 2;
				break;
			default:
				scale = 3;
				break;
			}
		}
	}

	internal int InternalMemoryIndexScale
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return scale;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			scale = (byte)value;
		}
	}

	public uint MemoryDisplacement32
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return (uint)memDispl;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			memDispl = value;
		}
	}

	public ulong MemoryDisplacement64
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return memDispl;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			memDispl = value;
		}
	}

	public byte Immediate8
	{
		readonly get
		{
			return (byte)immediate;
		}
		set
		{
			immediate = value;
		}
	}

	internal uint InternalImmediate8
	{
		set
		{
			immediate = value;
		}
	}

	public byte Immediate8_2nd
	{
		readonly get
		{
			return (byte)memDispl;
		}
		set
		{
			memDispl = value;
		}
	}

	internal uint InternalImmediate8_2nd
	{
		set
		{
			memDispl = value;
		}
	}

	public ushort Immediate16
	{
		readonly get
		{
			return (ushort)immediate;
		}
		set
		{
			immediate = value;
		}
	}

	internal uint InternalImmediate16
	{
		set
		{
			immediate = value;
		}
	}

	public uint Immediate32
	{
		readonly get
		{
			return immediate;
		}
		set
		{
			immediate = value;
		}
	}

	public ulong Immediate64
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return (memDispl << 32) | immediate;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			immediate = (uint)value;
			memDispl = (uint)(value >> 32);
		}
	}

	internal uint InternalImmediate64_lo
	{
		set
		{
			immediate = value;
		}
	}

	internal uint InternalImmediate64_hi
	{
		set
		{
			memDispl = value;
		}
	}

	public short Immediate8to16
	{
		readonly get
		{
			return (sbyte)immediate;
		}
		set
		{
			immediate = (uint)(sbyte)value;
		}
	}

	public int Immediate8to32
	{
		readonly get
		{
			return (sbyte)immediate;
		}
		set
		{
			immediate = (uint)(sbyte)value;
		}
	}

	public long Immediate8to64
	{
		readonly get
		{
			return (sbyte)immediate;
		}
		set
		{
			immediate = (uint)(sbyte)value;
		}
	}

	public long Immediate32to64
	{
		readonly get
		{
			return (int)immediate;
		}
		set
		{
			immediate = (uint)value;
		}
	}

	public ushort NearBranch16
	{
		readonly get
		{
			return (ushort)memDispl;
		}
		set
		{
			memDispl = value;
		}
	}

	internal uint InternalNearBranch16
	{
		set
		{
			memDispl = value;
		}
	}

	public uint NearBranch32
	{
		readonly get
		{
			return (uint)memDispl;
		}
		set
		{
			memDispl = value;
		}
	}

	public ulong NearBranch64
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return memDispl;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			memDispl = value;
		}
	}

	public readonly ulong NearBranchTarget => Op0Kind switch
	{
		OpKind.NearBranch16 => NearBranch16, 
		OpKind.NearBranch32 => NearBranch32, 
		OpKind.NearBranch64 => NearBranch64, 
		_ => 0uL, 
	};

	public ushort FarBranch16
	{
		readonly get
		{
			return (ushort)immediate;
		}
		set
		{
			immediate = value;
		}
	}

	internal uint InternalFarBranch16
	{
		set
		{
			immediate = value;
		}
	}

	public uint FarBranch32
	{
		readonly get
		{
			return immediate;
		}
		set
		{
			immediate = value;
		}
	}

	public ushort FarBranchSelector
	{
		readonly get
		{
			return (ushort)memDispl;
		}
		set
		{
			memDispl = value;
		}
	}

	internal uint InternalFarBranchSelector
	{
		set
		{
			memDispl = value;
		}
	}

	public Register MemoryBase
	{
		readonly get
		{
			return (Register)memBaseReg;
		}
		set
		{
			memBaseReg = (byte)value;
		}
	}

	internal Register InternalMemoryBase
	{
		set
		{
			memBaseReg = (byte)value;
		}
	}

	public Register MemoryIndex
	{
		readonly get
		{
			return (Register)memIndexReg;
		}
		set
		{
			memIndexReg = (byte)value;
		}
	}

	internal Register InternalMemoryIndex
	{
		set
		{
			memIndexReg = (byte)value;
		}
	}

	public Register Op0Register
	{
		readonly get
		{
			return (Register)reg0;
		}
		set
		{
			reg0 = (byte)value;
		}
	}

	internal Register InternalOp0Register
	{
		set
		{
			reg0 = (byte)value;
		}
	}

	public Register Op1Register
	{
		readonly get
		{
			return (Register)reg1;
		}
		set
		{
			reg1 = (byte)value;
		}
	}

	internal Register InternalOp1Register
	{
		set
		{
			reg1 = (byte)value;
		}
	}

	public Register Op2Register
	{
		readonly get
		{
			return (Register)reg2;
		}
		set
		{
			reg2 = (byte)value;
		}
	}

	internal Register InternalOp2Register
	{
		set
		{
			reg2 = (byte)value;
		}
	}

	public Register Op3Register
	{
		readonly get
		{
			return (Register)reg3;
		}
		set
		{
			reg3 = (byte)value;
		}
	}

	internal Register InternalOp3Register
	{
		set
		{
			reg3 = (byte)value;
		}
	}

	public Register Op4Register
	{
		readonly get
		{
			return Register.None;
		}
		set
		{
			if (value != 0)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException_value();
			}
		}
	}

	public Register OpMask
	{
		readonly get
		{
			int num = (int)((flags1 >> 15) & 7);
			if (num != 0)
			{
				return (Register)(num + 173);
			}
			return Register.None;
		}
		set
		{
			uint num = (uint)((value != 0) ? ((value - 173) & Register.DH) : Register.None);
			flags1 = (flags1 & 0xFFFC7FFFu) | (num << 15);
		}
	}

	internal uint InternalOpMask
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return (flags1 >> 15) & 7;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			flags1 |= value << 15;
		}
	}

	public readonly bool HasOpMask
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (flags1 & 0x38000) != 0;
		}
	}

	internal readonly bool HasOpMask_or_ZeroingMasking
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (flags1 & 0x10038000) != 0;
		}
	}

	public bool ZeroingMasking
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return (flags1 & 0x10000000) != 0;
		}
		set
		{
			if (value)
			{
				flags1 |= 268435456u;
			}
			else
			{
				flags1 &= 4026531839u;
			}
		}
	}

	public bool MergingMasking
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return (flags1 & 0x10000000) == 0;
		}
		set
		{
			if (value)
			{
				flags1 &= 4026531839u;
			}
			else
			{
				flags1 |= 268435456u;
			}
		}
	}

	public RoundingControl RoundingControl
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return (RoundingControl)((flags1 >> 12) & 7);
		}
		set
		{
			flags1 = (flags1 & 0xFFFF8FFFu) | (uint)((int)value << 12);
		}
	}

	internal uint InternalRoundingControl
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			flags1 |= value << 12;
		}
	}

	internal readonly bool HasRoundingControlOrSae
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (flags1 & 0x8007000) != 0;
		}
	}

	public int DeclareDataCount
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return (int)(((flags1 >> 8) & 0xF) + 1);
		}
		set
		{
			flags1 = (flags1 & 0xFFFFF0FFu) | (uint)(((value - 1) & 0xF) << 8);
		}
	}

	internal uint InternalDeclareDataCount
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			flags1 |= value - 1 << 8;
		}
	}

	public readonly bool IsVsib
	{
		get
		{
			bool vsib;
			return TryGetVsib64(out vsib);
		}
	}

	public readonly bool IsVsib32
	{
		get
		{
			if (TryGetVsib64(out var vsib))
			{
				return !vsib;
			}
			return false;
		}
	}

	public readonly bool IsVsib64
	{
		get
		{
			bool vsib;
			return TryGetVsib64(out vsib) && vsib;
		}
	}

	public bool SuppressAllExceptions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return (flags1 & 0x8000000) != 0;
		}
		set
		{
			if (value)
			{
				flags1 |= 134217728u;
			}
			else
			{
				flags1 &= 4160749567u;
			}
		}
	}

	public readonly bool IsIPRelativeMemoryOperand
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if (MemoryBase != Register.RIP)
			{
				return MemoryBase == Register.EIP;
			}
			return true;
		}
	}

	public readonly ulong IPRelativeMemoryAddress
	{
		get
		{
			if (MemoryBase != Register.EIP)
			{
				return MemoryDisplacement64;
			}
			return MemoryDisplacement32;
		}
	}

	private static void InitializeSignedImmediate(ref Instruction instruction, int operand, long immediate)
	{
		OpKind immediateOpKind = GetImmediateOpKind(instruction.Code, operand);
		instruction.SetOpKind(operand, immediateOpKind);
		switch (immediateOpKind)
		{
		case OpKind.Immediate8:
			if (-128 > immediate || immediate > 255)
			{
				throw new ArgumentOutOfRangeException("immediate");
			}
			instruction.InternalImmediate8 = (byte)immediate;
			break;
		case OpKind.Immediate8_2nd:
			if (-128 > immediate || immediate > 255)
			{
				throw new ArgumentOutOfRangeException("immediate");
			}
			instruction.InternalImmediate8_2nd = (byte)immediate;
			break;
		case OpKind.Immediate8to16:
			if (-128 > immediate || immediate > 127)
			{
				throw new ArgumentOutOfRangeException("immediate");
			}
			instruction.InternalImmediate8 = (byte)immediate;
			break;
		case OpKind.Immediate8to32:
			if (-128 > immediate || immediate > 127)
			{
				throw new ArgumentOutOfRangeException("immediate");
			}
			instruction.InternalImmediate8 = (byte)immediate;
			break;
		case OpKind.Immediate8to64:
			if (-128 > immediate || immediate > 127)
			{
				throw new ArgumentOutOfRangeException("immediate");
			}
			instruction.InternalImmediate8 = (byte)immediate;
			break;
		case OpKind.Immediate16:
			if (-32768 > immediate || immediate > 65535)
			{
				throw new ArgumentOutOfRangeException("immediate");
			}
			instruction.InternalImmediate16 = (ushort)immediate;
			break;
		case OpKind.Immediate32:
			if (int.MinValue > immediate || immediate > uint.MaxValue)
			{
				throw new ArgumentOutOfRangeException("immediate");
			}
			instruction.Immediate32 = (uint)immediate;
			break;
		case OpKind.Immediate32to64:
			if (int.MinValue > immediate || immediate > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("immediate");
			}
			instruction.Immediate32 = (uint)immediate;
			break;
		case OpKind.Immediate64:
			instruction.Immediate64 = (ulong)immediate;
			break;
		default:
			throw new ArgumentOutOfRangeException("instruction");
		}
	}

	private static void InitializeUnsignedImmediate(ref Instruction instruction, int operand, ulong immediate)
	{
		OpKind immediateOpKind = GetImmediateOpKind(instruction.Code, operand);
		instruction.SetOpKind(operand, immediateOpKind);
		switch (immediateOpKind)
		{
		case OpKind.Immediate8:
			if (immediate > 255)
			{
				throw new ArgumentOutOfRangeException("immediate");
			}
			instruction.InternalImmediate8 = (byte)immediate;
			break;
		case OpKind.Immediate8_2nd:
			if (immediate > 255)
			{
				throw new ArgumentOutOfRangeException("immediate");
			}
			instruction.InternalImmediate8_2nd = (byte)immediate;
			break;
		case OpKind.Immediate8to16:
			if (immediate > 127 && (65408 > immediate || immediate > 65535))
			{
				throw new ArgumentOutOfRangeException("immediate");
			}
			instruction.InternalImmediate8 = (byte)immediate;
			break;
		case OpKind.Immediate8to32:
			if (immediate > 127 && (4294967168u > immediate || immediate > uint.MaxValue))
			{
				throw new ArgumentOutOfRangeException("immediate");
			}
			instruction.InternalImmediate8 = (byte)immediate;
			break;
		case OpKind.Immediate8to64:
			if (immediate + 128 > 255)
			{
				throw new ArgumentOutOfRangeException("immediate");
			}
			instruction.InternalImmediate8 = (byte)immediate;
			break;
		case OpKind.Immediate16:
			if (immediate > 65535)
			{
				throw new ArgumentOutOfRangeException("immediate");
			}
			instruction.InternalImmediate16 = (ushort)immediate;
			break;
		case OpKind.Immediate32:
			if (immediate > uint.MaxValue)
			{
				throw new ArgumentOutOfRangeException("immediate");
			}
			instruction.Immediate32 = (uint)immediate;
			break;
		case OpKind.Immediate32to64:
			if (immediate + 2147483648u > uint.MaxValue)
			{
				throw new ArgumentOutOfRangeException("immediate");
			}
			instruction.Immediate32 = (uint)immediate;
			break;
		case OpKind.Immediate64:
			instruction.Immediate64 = immediate;
			break;
		default:
			throw new ArgumentOutOfRangeException("instruction");
		}
	}

	private static OpKind GetImmediateOpKind(Code code, int operand)
	{
		OpCodeHandler[] handlers = OpCodeHandlers.Handlers;
		if ((uint)code >= (uint)handlers.Length)
		{
			throw new ArgumentOutOfRangeException("code");
		}
		Op[] operands = handlers[(int)code].Operands;
		if ((uint)operand >= (uint)operands.Length)
		{
			throw new ArgumentOutOfRangeException("operand", $"{code} doesn't have at least {operand + 1} operands");
		}
		OpKind opKind = operands[operand].GetImmediateOpKind();
		if (opKind == OpKind.Immediate8 && operand > 0 && operand + 1 == operands.Length)
		{
			OpKind immediateOpKind = operands[operand - 1].GetImmediateOpKind();
			if (immediateOpKind == OpKind.Immediate8 || immediateOpKind == OpKind.Immediate16)
			{
				opKind = OpKind.Immediate8_2nd;
			}
		}
		if (opKind == (OpKind)(-1))
		{
			throw new ArgumentException($"{code}'s op{operand} isn't an immediate operand");
		}
		return opKind;
	}

	private static OpKind GetNearBranchOpKind(Code code, int operand)
	{
		OpCodeHandler[] handlers = OpCodeHandlers.Handlers;
		if ((uint)code >= (uint)handlers.Length)
		{
			throw new ArgumentOutOfRangeException("code");
		}
		Op[] operands = handlers[(int)code].Operands;
		if ((uint)operand >= (uint)operands.Length)
		{
			throw new ArgumentOutOfRangeException("operand", $"{code} doesn't have at least {operand + 1} operands");
		}
		OpKind nearBranchOpKind = operands[operand].GetNearBranchOpKind();
		if (nearBranchOpKind == (OpKind)(-1))
		{
			throw new ArgumentException($"{code}'s op{operand} isn't a near branch operand");
		}
		return nearBranchOpKind;
	}

	private static OpKind GetFarBranchOpKind(Code code, int operand)
	{
		OpCodeHandler[] handlers = OpCodeHandlers.Handlers;
		if ((uint)code >= (uint)handlers.Length)
		{
			throw new ArgumentOutOfRangeException("code");
		}
		Op[] operands = handlers[(int)code].Operands;
		if ((uint)operand >= (uint)operands.Length)
		{
			throw new ArgumentOutOfRangeException("operand", $"{code} doesn't have at least {operand + 1} operands");
		}
		OpKind farBranchOpKind = operands[operand].GetFarBranchOpKind();
		if (farBranchOpKind == (OpKind)(-1))
		{
			throw new ArgumentException($"{code}'s op{operand} isn't a far branch operand");
		}
		return farBranchOpKind;
	}

	private static Instruction CreateString_Reg_SegRSI(Code code, int addressSize, Register register, Register segmentPrefix, RepPrefixKind repPrefix)
	{
		Instruction result = default(Instruction);
		result.Code = code;
		switch (repPrefix)
		{
		case RepPrefixKind.Repe:
			result.InternalSetHasRepePrefix();
			break;
		case RepPrefixKind.Repne:
			result.InternalSetHasRepnePrefix();
			break;
		}
		result.Op0Register = register;
		switch (addressSize)
		{
		case 64:
			result.Op1Kind = OpKind.MemorySegRSI;
			break;
		case 32:
			result.Op1Kind = OpKind.MemorySegESI;
			break;
		case 16:
			result.Op1Kind = OpKind.MemorySegSI;
			break;
		default:
			throw new ArgumentOutOfRangeException("addressSize");
		}
		result.SegmentPrefix = segmentPrefix;
		return result;
	}

	private static Instruction CreateString_Reg_ESRDI(Code code, int addressSize, Register register, RepPrefixKind repPrefix)
	{
		Instruction result = default(Instruction);
		result.Code = code;
		switch (repPrefix)
		{
		case RepPrefixKind.Repe:
			result.InternalSetHasRepePrefix();
			break;
		case RepPrefixKind.Repne:
			result.InternalSetHasRepnePrefix();
			break;
		}
		result.Op0Register = register;
		switch (addressSize)
		{
		case 64:
			result.Op1Kind = OpKind.MemoryESRDI;
			break;
		case 32:
			result.Op1Kind = OpKind.MemoryESEDI;
			break;
		case 16:
			result.Op1Kind = OpKind.MemoryESDI;
			break;
		default:
			throw new ArgumentOutOfRangeException("addressSize");
		}
		return result;
	}

	private static Instruction CreateString_ESRDI_Reg(Code code, int addressSize, Register register, RepPrefixKind repPrefix)
	{
		Instruction result = default(Instruction);
		result.Code = code;
		switch (repPrefix)
		{
		case RepPrefixKind.Repe:
			result.InternalSetHasRepePrefix();
			break;
		case RepPrefixKind.Repne:
			result.InternalSetHasRepnePrefix();
			break;
		}
		switch (addressSize)
		{
		case 64:
			result.Op0Kind = OpKind.MemoryESRDI;
			break;
		case 32:
			result.Op0Kind = OpKind.MemoryESEDI;
			break;
		case 16:
			result.Op0Kind = OpKind.MemoryESDI;
			break;
		default:
			throw new ArgumentOutOfRangeException("addressSize");
		}
		result.Op1Register = register;
		return result;
	}

	private static Instruction CreateString_SegRSI_ESRDI(Code code, int addressSize, Register segmentPrefix, RepPrefixKind repPrefix)
	{
		Instruction result = default(Instruction);
		result.Code = code;
		switch (repPrefix)
		{
		case RepPrefixKind.Repe:
			result.InternalSetHasRepePrefix();
			break;
		case RepPrefixKind.Repne:
			result.InternalSetHasRepnePrefix();
			break;
		}
		switch (addressSize)
		{
		case 64:
			result.Op0Kind = OpKind.MemorySegRSI;
			result.Op1Kind = OpKind.MemoryESRDI;
			break;
		case 32:
			result.Op0Kind = OpKind.MemorySegESI;
			result.Op1Kind = OpKind.MemoryESEDI;
			break;
		case 16:
			result.Op0Kind = OpKind.MemorySegSI;
			result.Op1Kind = OpKind.MemoryESDI;
			break;
		default:
			throw new ArgumentOutOfRangeException("addressSize");
		}
		result.SegmentPrefix = segmentPrefix;
		return result;
	}

	private static Instruction CreateString_ESRDI_SegRSI(Code code, int addressSize, Register segmentPrefix, RepPrefixKind repPrefix)
	{
		Instruction result = default(Instruction);
		result.Code = code;
		switch (repPrefix)
		{
		case RepPrefixKind.Repe:
			result.InternalSetHasRepePrefix();
			break;
		case RepPrefixKind.Repne:
			result.InternalSetHasRepnePrefix();
			break;
		}
		switch (addressSize)
		{
		case 64:
			result.Op0Kind = OpKind.MemoryESRDI;
			result.Op1Kind = OpKind.MemorySegRSI;
			break;
		case 32:
			result.Op0Kind = OpKind.MemoryESEDI;
			result.Op1Kind = OpKind.MemorySegESI;
			break;
		case 16:
			result.Op0Kind = OpKind.MemoryESDI;
			result.Op1Kind = OpKind.MemorySegSI;
			break;
		default:
			throw new ArgumentOutOfRangeException("addressSize");
		}
		result.SegmentPrefix = segmentPrefix;
		return result;
	}

	private static Instruction CreateMaskmov(Code code, int addressSize, Register register1, Register register2, Register segmentPrefix)
	{
		Instruction result = default(Instruction);
		result.Code = code;
		switch (addressSize)
		{
		case 64:
			result.Op0Kind = OpKind.MemorySegRDI;
			break;
		case 32:
			result.Op0Kind = OpKind.MemorySegEDI;
			break;
		case 16:
			result.Op0Kind = OpKind.MemorySegDI;
			break;
		default:
			throw new ArgumentOutOfRangeException("addressSize");
		}
		result.Op1Register = register1;
		result.Op2Register = register2;
		result.SegmentPrefix = segmentPrefix;
		return result;
	}

	private static void InitMemoryOperand(ref Instruction instruction, in MemoryOperand memory)
	{
		instruction.InternalMemoryBase = memory.Base;
		instruction.InternalMemoryIndex = memory.Index;
		instruction.MemoryIndexScale = memory.Scale;
		instruction.MemoryDisplSize = memory.DisplSize;
		instruction.MemoryDisplacement64 = (ulong)memory.Displacement;
		instruction.IsBroadcast = memory.IsBroadcast;
		instruction.SegmentPrefix = memory.SegmentPrefix;
	}

	public static Instruction Create(Code code)
	{
		Instruction result = default(Instruction);
		result.Code = code;
		return result;
	}

	public static Instruction Create(Code code, Register register)
	{
		Instruction result = default(Instruction);
		result.Code = code;
		result.Op0Register = register;
		return result;
	}

	public static Instruction Create(Code code, int immediate)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		InitializeSignedImmediate(ref instruction, 0, immediate);
		return instruction;
	}

	public static Instruction Create(Code code, uint immediate)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		InitializeUnsignedImmediate(ref instruction, 0, immediate);
		return instruction;
	}

	public static Instruction Create(Code code, in MemoryOperand memory)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Kind = OpKind.Memory;
		InitMemoryOperand(ref instruction, in memory);
		return instruction;
	}

	public static Instruction Create(Code code, Register register1, Register register2)
	{
		Instruction result = default(Instruction);
		result.Code = code;
		result.Op0Register = register1;
		result.Op1Register = register2;
		return result;
	}

	public static Instruction Create(Code code, Register register, int immediate)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Register = register;
		InitializeSignedImmediate(ref instruction, 1, immediate);
		return instruction;
	}

	public static Instruction Create(Code code, Register register, uint immediate)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Register = register;
		InitializeUnsignedImmediate(ref instruction, 1, immediate);
		return instruction;
	}

	public static Instruction Create(Code code, Register register, long immediate)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Register = register;
		InitializeSignedImmediate(ref instruction, 1, immediate);
		return instruction;
	}

	public static Instruction Create(Code code, Register register, ulong immediate)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Register = register;
		InitializeUnsignedImmediate(ref instruction, 1, immediate);
		return instruction;
	}

	public static Instruction Create(Code code, Register register, in MemoryOperand memory)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Register = register;
		instruction.Op1Kind = OpKind.Memory;
		InitMemoryOperand(ref instruction, in memory);
		return instruction;
	}

	public static Instruction Create(Code code, int immediate, Register register)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		InitializeSignedImmediate(ref instruction, 0, immediate);
		instruction.Op1Register = register;
		return instruction;
	}

	public static Instruction Create(Code code, uint immediate, Register register)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		InitializeUnsignedImmediate(ref instruction, 0, immediate);
		instruction.Op1Register = register;
		return instruction;
	}

	public static Instruction Create(Code code, int immediate1, int immediate2)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		InitializeSignedImmediate(ref instruction, 0, immediate1);
		InitializeSignedImmediate(ref instruction, 1, immediate2);
		return instruction;
	}

	public static Instruction Create(Code code, uint immediate1, uint immediate2)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		InitializeUnsignedImmediate(ref instruction, 0, immediate1);
		InitializeUnsignedImmediate(ref instruction, 1, immediate2);
		return instruction;
	}

	public static Instruction Create(Code code, in MemoryOperand memory, Register register)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Kind = OpKind.Memory;
		InitMemoryOperand(ref instruction, in memory);
		instruction.Op1Register = register;
		return instruction;
	}

	public static Instruction Create(Code code, in MemoryOperand memory, int immediate)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Kind = OpKind.Memory;
		InitMemoryOperand(ref instruction, in memory);
		InitializeSignedImmediate(ref instruction, 1, immediate);
		return instruction;
	}

	public static Instruction Create(Code code, in MemoryOperand memory, uint immediate)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Kind = OpKind.Memory;
		InitMemoryOperand(ref instruction, in memory);
		InitializeUnsignedImmediate(ref instruction, 1, immediate);
		return instruction;
	}

	public static Instruction Create(Code code, Register register1, Register register2, Register register3)
	{
		Instruction result = default(Instruction);
		result.Code = code;
		result.Op0Register = register1;
		result.Op1Register = register2;
		result.Op2Register = register3;
		return result;
	}

	public static Instruction Create(Code code, Register register1, Register register2, int immediate)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Register = register1;
		instruction.Op1Register = register2;
		InitializeSignedImmediate(ref instruction, 2, immediate);
		return instruction;
	}

	public static Instruction Create(Code code, Register register1, Register register2, uint immediate)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Register = register1;
		instruction.Op1Register = register2;
		InitializeUnsignedImmediate(ref instruction, 2, immediate);
		return instruction;
	}

	public static Instruction Create(Code code, Register register1, Register register2, in MemoryOperand memory)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Register = register1;
		instruction.Op1Register = register2;
		instruction.Op2Kind = OpKind.Memory;
		InitMemoryOperand(ref instruction, in memory);
		return instruction;
	}

	public static Instruction Create(Code code, Register register, int immediate1, int immediate2)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Register = register;
		InitializeSignedImmediate(ref instruction, 1, immediate1);
		InitializeSignedImmediate(ref instruction, 2, immediate2);
		return instruction;
	}

	public static Instruction Create(Code code, Register register, uint immediate1, uint immediate2)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Register = register;
		InitializeUnsignedImmediate(ref instruction, 1, immediate1);
		InitializeUnsignedImmediate(ref instruction, 2, immediate2);
		return instruction;
	}

	public static Instruction Create(Code code, Register register1, in MemoryOperand memory, Register register2)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Register = register1;
		instruction.Op1Kind = OpKind.Memory;
		InitMemoryOperand(ref instruction, in memory);
		instruction.Op2Register = register2;
		return instruction;
	}

	public static Instruction Create(Code code, Register register, in MemoryOperand memory, int immediate)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Register = register;
		instruction.Op1Kind = OpKind.Memory;
		InitMemoryOperand(ref instruction, in memory);
		InitializeSignedImmediate(ref instruction, 2, immediate);
		return instruction;
	}

	public static Instruction Create(Code code, Register register, in MemoryOperand memory, uint immediate)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Register = register;
		instruction.Op1Kind = OpKind.Memory;
		InitMemoryOperand(ref instruction, in memory);
		InitializeUnsignedImmediate(ref instruction, 2, immediate);
		return instruction;
	}

	public static Instruction Create(Code code, in MemoryOperand memory, Register register1, Register register2)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Kind = OpKind.Memory;
		InitMemoryOperand(ref instruction, in memory);
		instruction.Op1Register = register1;
		instruction.Op2Register = register2;
		return instruction;
	}

	public static Instruction Create(Code code, in MemoryOperand memory, Register register, int immediate)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Kind = OpKind.Memory;
		InitMemoryOperand(ref instruction, in memory);
		instruction.Op1Register = register;
		InitializeSignedImmediate(ref instruction, 2, immediate);
		return instruction;
	}

	public static Instruction Create(Code code, in MemoryOperand memory, Register register, uint immediate)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Kind = OpKind.Memory;
		InitMemoryOperand(ref instruction, in memory);
		instruction.Op1Register = register;
		InitializeUnsignedImmediate(ref instruction, 2, immediate);
		return instruction;
	}

	public static Instruction Create(Code code, Register register1, Register register2, Register register3, Register register4)
	{
		Instruction result = default(Instruction);
		result.Code = code;
		result.Op0Register = register1;
		result.Op1Register = register2;
		result.Op2Register = register3;
		result.Op3Register = register4;
		return result;
	}

	public static Instruction Create(Code code, Register register1, Register register2, Register register3, int immediate)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Register = register1;
		instruction.Op1Register = register2;
		instruction.Op2Register = register3;
		InitializeSignedImmediate(ref instruction, 3, immediate);
		return instruction;
	}

	public static Instruction Create(Code code, Register register1, Register register2, Register register3, uint immediate)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Register = register1;
		instruction.Op1Register = register2;
		instruction.Op2Register = register3;
		InitializeUnsignedImmediate(ref instruction, 3, immediate);
		return instruction;
	}

	public static Instruction Create(Code code, Register register1, Register register2, Register register3, in MemoryOperand memory)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Register = register1;
		instruction.Op1Register = register2;
		instruction.Op2Register = register3;
		instruction.Op3Kind = OpKind.Memory;
		InitMemoryOperand(ref instruction, in memory);
		return instruction;
	}

	public static Instruction Create(Code code, Register register1, Register register2, int immediate1, int immediate2)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Register = register1;
		instruction.Op1Register = register2;
		InitializeSignedImmediate(ref instruction, 2, immediate1);
		InitializeSignedImmediate(ref instruction, 3, immediate2);
		return instruction;
	}

	public static Instruction Create(Code code, Register register1, Register register2, uint immediate1, uint immediate2)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Register = register1;
		instruction.Op1Register = register2;
		InitializeUnsignedImmediate(ref instruction, 2, immediate1);
		InitializeUnsignedImmediate(ref instruction, 3, immediate2);
		return instruction;
	}

	public static Instruction Create(Code code, Register register1, Register register2, in MemoryOperand memory, Register register3)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Register = register1;
		instruction.Op1Register = register2;
		instruction.Op2Kind = OpKind.Memory;
		InitMemoryOperand(ref instruction, in memory);
		instruction.Op3Register = register3;
		return instruction;
	}

	public static Instruction Create(Code code, Register register1, Register register2, in MemoryOperand memory, int immediate)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Register = register1;
		instruction.Op1Register = register2;
		instruction.Op2Kind = OpKind.Memory;
		InitMemoryOperand(ref instruction, in memory);
		InitializeSignedImmediate(ref instruction, 3, immediate);
		return instruction;
	}

	public static Instruction Create(Code code, Register register1, Register register2, in MemoryOperand memory, uint immediate)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Register = register1;
		instruction.Op1Register = register2;
		instruction.Op2Kind = OpKind.Memory;
		InitMemoryOperand(ref instruction, in memory);
		InitializeUnsignedImmediate(ref instruction, 3, immediate);
		return instruction;
	}

	public static Instruction Create(Code code, Register register1, Register register2, Register register3, Register register4, int immediate)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Register = register1;
		instruction.Op1Register = register2;
		instruction.Op2Register = register3;
		instruction.Op3Register = register4;
		InitializeSignedImmediate(ref instruction, 4, immediate);
		return instruction;
	}

	public static Instruction Create(Code code, Register register1, Register register2, Register register3, Register register4, uint immediate)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Register = register1;
		instruction.Op1Register = register2;
		instruction.Op2Register = register3;
		instruction.Op3Register = register4;
		InitializeUnsignedImmediate(ref instruction, 4, immediate);
		return instruction;
	}

	public static Instruction Create(Code code, Register register1, Register register2, Register register3, in MemoryOperand memory, int immediate)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Register = register1;
		instruction.Op1Register = register2;
		instruction.Op2Register = register3;
		instruction.Op3Kind = OpKind.Memory;
		InitMemoryOperand(ref instruction, in memory);
		InitializeSignedImmediate(ref instruction, 4, immediate);
		return instruction;
	}

	public static Instruction Create(Code code, Register register1, Register register2, Register register3, in MemoryOperand memory, uint immediate)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Register = register1;
		instruction.Op1Register = register2;
		instruction.Op2Register = register3;
		instruction.Op3Kind = OpKind.Memory;
		InitMemoryOperand(ref instruction, in memory);
		InitializeUnsignedImmediate(ref instruction, 4, immediate);
		return instruction;
	}

	public static Instruction Create(Code code, Register register1, Register register2, in MemoryOperand memory, Register register3, int immediate)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Register = register1;
		instruction.Op1Register = register2;
		instruction.Op2Kind = OpKind.Memory;
		InitMemoryOperand(ref instruction, in memory);
		instruction.Op3Register = register3;
		InitializeSignedImmediate(ref instruction, 4, immediate);
		return instruction;
	}

	public static Instruction Create(Code code, Register register1, Register register2, in MemoryOperand memory, Register register3, uint immediate)
	{
		Instruction instruction = default(Instruction);
		instruction.Code = code;
		instruction.Op0Register = register1;
		instruction.Op1Register = register2;
		instruction.Op2Kind = OpKind.Memory;
		InitMemoryOperand(ref instruction, in memory);
		instruction.Op3Register = register3;
		InitializeUnsignedImmediate(ref instruction, 4, immediate);
		return instruction;
	}

	public static Instruction CreateBranch(Code code, ulong target)
	{
		Instruction result = default(Instruction);
		result.Code = code;
		result.Op0Kind = GetNearBranchOpKind(code, 0);
		result.NearBranch64 = target;
		return result;
	}

	public static Instruction CreateBranch(Code code, ushort selector, uint offset)
	{
		Instruction result = default(Instruction);
		result.Code = code;
		result.Op0Kind = GetFarBranchOpKind(code, 0);
		result.FarBranchSelector = selector;
		result.FarBranch32 = offset;
		return result;
	}

	public static Instruction CreateXbegin(int bitness, ulong target)
	{
		Instruction result = default(Instruction);
		switch (bitness)
		{
		case 16:
			result.Code = Code.Xbegin_rel16;
			result.Op0Kind = OpKind.NearBranch32;
			result.NearBranch32 = (uint)target;
			break;
		case 32:
			result.Code = Code.Xbegin_rel32;
			result.Op0Kind = OpKind.NearBranch32;
			result.NearBranch32 = (uint)target;
			break;
		case 64:
			result.Code = Code.Xbegin_rel32;
			result.Op0Kind = OpKind.NearBranch64;
			result.NearBranch64 = target;
			break;
		default:
			throw new ArgumentOutOfRangeException("bitness");
		}
		return result;
	}

	public static Instruction CreateOutsb(int addressSize, Register segmentPrefix = Register.None, RepPrefixKind repPrefix = RepPrefixKind.None)
	{
		return CreateString_Reg_SegRSI(Code.Outsb_DX_m8, addressSize, Register.DX, segmentPrefix, repPrefix);
	}

	public static Instruction CreateRepOutsb(int addressSize)
	{
		return CreateString_Reg_SegRSI(Code.Outsb_DX_m8, addressSize, Register.DX, Register.None, RepPrefixKind.Repe);
	}

	public static Instruction CreateOutsw(int addressSize, Register segmentPrefix = Register.None, RepPrefixKind repPrefix = RepPrefixKind.None)
	{
		return CreateString_Reg_SegRSI(Code.Outsw_DX_m16, addressSize, Register.DX, segmentPrefix, repPrefix);
	}

	public static Instruction CreateRepOutsw(int addressSize)
	{
		return CreateString_Reg_SegRSI(Code.Outsw_DX_m16, addressSize, Register.DX, Register.None, RepPrefixKind.Repe);
	}

	public static Instruction CreateOutsd(int addressSize, Register segmentPrefix = Register.None, RepPrefixKind repPrefix = RepPrefixKind.None)
	{
		return CreateString_Reg_SegRSI(Code.Outsd_DX_m32, addressSize, Register.DX, segmentPrefix, repPrefix);
	}

	public static Instruction CreateRepOutsd(int addressSize)
	{
		return CreateString_Reg_SegRSI(Code.Outsd_DX_m32, addressSize, Register.DX, Register.None, RepPrefixKind.Repe);
	}

	public static Instruction CreateLodsb(int addressSize, Register segmentPrefix = Register.None, RepPrefixKind repPrefix = RepPrefixKind.None)
	{
		return CreateString_Reg_SegRSI(Code.Lodsb_AL_m8, addressSize, Register.AL, segmentPrefix, repPrefix);
	}

	public static Instruction CreateRepLodsb(int addressSize)
	{
		return CreateString_Reg_SegRSI(Code.Lodsb_AL_m8, addressSize, Register.AL, Register.None, RepPrefixKind.Repe);
	}

	public static Instruction CreateLodsw(int addressSize, Register segmentPrefix = Register.None, RepPrefixKind repPrefix = RepPrefixKind.None)
	{
		return CreateString_Reg_SegRSI(Code.Lodsw_AX_m16, addressSize, Register.AX, segmentPrefix, repPrefix);
	}

	public static Instruction CreateRepLodsw(int addressSize)
	{
		return CreateString_Reg_SegRSI(Code.Lodsw_AX_m16, addressSize, Register.AX, Register.None, RepPrefixKind.Repe);
	}

	public static Instruction CreateLodsd(int addressSize, Register segmentPrefix = Register.None, RepPrefixKind repPrefix = RepPrefixKind.None)
	{
		return CreateString_Reg_SegRSI(Code.Lodsd_EAX_m32, addressSize, Register.EAX, segmentPrefix, repPrefix);
	}

	public static Instruction CreateRepLodsd(int addressSize)
	{
		return CreateString_Reg_SegRSI(Code.Lodsd_EAX_m32, addressSize, Register.EAX, Register.None, RepPrefixKind.Repe);
	}

	public static Instruction CreateLodsq(int addressSize, Register segmentPrefix = Register.None, RepPrefixKind repPrefix = RepPrefixKind.None)
	{
		return CreateString_Reg_SegRSI(Code.Lodsq_RAX_m64, addressSize, Register.RAX, segmentPrefix, repPrefix);
	}

	public static Instruction CreateRepLodsq(int addressSize)
	{
		return CreateString_Reg_SegRSI(Code.Lodsq_RAX_m64, addressSize, Register.RAX, Register.None, RepPrefixKind.Repe);
	}

	public static Instruction CreateScasb(int addressSize, RepPrefixKind repPrefix = RepPrefixKind.None)
	{
		return CreateString_Reg_ESRDI(Code.Scasb_AL_m8, addressSize, Register.AL, repPrefix);
	}

	public static Instruction CreateRepeScasb(int addressSize)
	{
		return CreateString_Reg_ESRDI(Code.Scasb_AL_m8, addressSize, Register.AL, RepPrefixKind.Repe);
	}

	public static Instruction CreateRepneScasb(int addressSize)
	{
		return CreateString_Reg_ESRDI(Code.Scasb_AL_m8, addressSize, Register.AL, RepPrefixKind.Repne);
	}

	public static Instruction CreateScasw(int addressSize, RepPrefixKind repPrefix = RepPrefixKind.None)
	{
		return CreateString_Reg_ESRDI(Code.Scasw_AX_m16, addressSize, Register.AX, repPrefix);
	}

	public static Instruction CreateRepeScasw(int addressSize)
	{
		return CreateString_Reg_ESRDI(Code.Scasw_AX_m16, addressSize, Register.AX, RepPrefixKind.Repe);
	}

	public static Instruction CreateRepneScasw(int addressSize)
	{
		return CreateString_Reg_ESRDI(Code.Scasw_AX_m16, addressSize, Register.AX, RepPrefixKind.Repne);
	}

	public static Instruction CreateScasd(int addressSize, RepPrefixKind repPrefix = RepPrefixKind.None)
	{
		return CreateString_Reg_ESRDI(Code.Scasd_EAX_m32, addressSize, Register.EAX, repPrefix);
	}

	public static Instruction CreateRepeScasd(int addressSize)
	{
		return CreateString_Reg_ESRDI(Code.Scasd_EAX_m32, addressSize, Register.EAX, RepPrefixKind.Repe);
	}

	public static Instruction CreateRepneScasd(int addressSize)
	{
		return CreateString_Reg_ESRDI(Code.Scasd_EAX_m32, addressSize, Register.EAX, RepPrefixKind.Repne);
	}

	public static Instruction CreateScasq(int addressSize, RepPrefixKind repPrefix = RepPrefixKind.None)
	{
		return CreateString_Reg_ESRDI(Code.Scasq_RAX_m64, addressSize, Register.RAX, repPrefix);
	}

	public static Instruction CreateRepeScasq(int addressSize)
	{
		return CreateString_Reg_ESRDI(Code.Scasq_RAX_m64, addressSize, Register.RAX, RepPrefixKind.Repe);
	}

	public static Instruction CreateRepneScasq(int addressSize)
	{
		return CreateString_Reg_ESRDI(Code.Scasq_RAX_m64, addressSize, Register.RAX, RepPrefixKind.Repne);
	}

	public static Instruction CreateInsb(int addressSize, RepPrefixKind repPrefix = RepPrefixKind.None)
	{
		return CreateString_ESRDI_Reg(Code.Insb_m8_DX, addressSize, Register.DX, repPrefix);
	}

	public static Instruction CreateRepInsb(int addressSize)
	{
		return CreateString_ESRDI_Reg(Code.Insb_m8_DX, addressSize, Register.DX, RepPrefixKind.Repe);
	}

	public static Instruction CreateInsw(int addressSize, RepPrefixKind repPrefix = RepPrefixKind.None)
	{
		return CreateString_ESRDI_Reg(Code.Insw_m16_DX, addressSize, Register.DX, repPrefix);
	}

	public static Instruction CreateRepInsw(int addressSize)
	{
		return CreateString_ESRDI_Reg(Code.Insw_m16_DX, addressSize, Register.DX, RepPrefixKind.Repe);
	}

	public static Instruction CreateInsd(int addressSize, RepPrefixKind repPrefix = RepPrefixKind.None)
	{
		return CreateString_ESRDI_Reg(Code.Insd_m32_DX, addressSize, Register.DX, repPrefix);
	}

	public static Instruction CreateRepInsd(int addressSize)
	{
		return CreateString_ESRDI_Reg(Code.Insd_m32_DX, addressSize, Register.DX, RepPrefixKind.Repe);
	}

	public static Instruction CreateStosb(int addressSize, RepPrefixKind repPrefix = RepPrefixKind.None)
	{
		return CreateString_ESRDI_Reg(Code.Stosb_m8_AL, addressSize, Register.AL, repPrefix);
	}

	public static Instruction CreateRepStosb(int addressSize)
	{
		return CreateString_ESRDI_Reg(Code.Stosb_m8_AL, addressSize, Register.AL, RepPrefixKind.Repe);
	}

	public static Instruction CreateStosw(int addressSize, RepPrefixKind repPrefix = RepPrefixKind.None)
	{
		return CreateString_ESRDI_Reg(Code.Stosw_m16_AX, addressSize, Register.AX, repPrefix);
	}

	public static Instruction CreateRepStosw(int addressSize)
	{
		return CreateString_ESRDI_Reg(Code.Stosw_m16_AX, addressSize, Register.AX, RepPrefixKind.Repe);
	}

	public static Instruction CreateStosd(int addressSize, RepPrefixKind repPrefix = RepPrefixKind.None)
	{
		return CreateString_ESRDI_Reg(Code.Stosd_m32_EAX, addressSize, Register.EAX, repPrefix);
	}

	public static Instruction CreateRepStosd(int addressSize)
	{
		return CreateString_ESRDI_Reg(Code.Stosd_m32_EAX, addressSize, Register.EAX, RepPrefixKind.Repe);
	}

	public static Instruction CreateStosq(int addressSize, RepPrefixKind repPrefix = RepPrefixKind.None)
	{
		return CreateString_ESRDI_Reg(Code.Stosq_m64_RAX, addressSize, Register.RAX, repPrefix);
	}

	public static Instruction CreateRepStosq(int addressSize)
	{
		return CreateString_ESRDI_Reg(Code.Stosq_m64_RAX, addressSize, Register.RAX, RepPrefixKind.Repe);
	}

	public static Instruction CreateCmpsb(int addressSize, Register segmentPrefix = Register.None, RepPrefixKind repPrefix = RepPrefixKind.None)
	{
		return CreateString_SegRSI_ESRDI(Code.Cmpsb_m8_m8, addressSize, segmentPrefix, repPrefix);
	}

	public static Instruction CreateRepeCmpsb(int addressSize)
	{
		return CreateString_SegRSI_ESRDI(Code.Cmpsb_m8_m8, addressSize, Register.None, RepPrefixKind.Repe);
	}

	public static Instruction CreateRepneCmpsb(int addressSize)
	{
		return CreateString_SegRSI_ESRDI(Code.Cmpsb_m8_m8, addressSize, Register.None, RepPrefixKind.Repne);
	}

	public static Instruction CreateCmpsw(int addressSize, Register segmentPrefix = Register.None, RepPrefixKind repPrefix = RepPrefixKind.None)
	{
		return CreateString_SegRSI_ESRDI(Code.Cmpsw_m16_m16, addressSize, segmentPrefix, repPrefix);
	}

	public static Instruction CreateRepeCmpsw(int addressSize)
	{
		return CreateString_SegRSI_ESRDI(Code.Cmpsw_m16_m16, addressSize, Register.None, RepPrefixKind.Repe);
	}

	public static Instruction CreateRepneCmpsw(int addressSize)
	{
		return CreateString_SegRSI_ESRDI(Code.Cmpsw_m16_m16, addressSize, Register.None, RepPrefixKind.Repne);
	}

	public static Instruction CreateCmpsd(int addressSize, Register segmentPrefix = Register.None, RepPrefixKind repPrefix = RepPrefixKind.None)
	{
		return CreateString_SegRSI_ESRDI(Code.Cmpsd_m32_m32, addressSize, segmentPrefix, repPrefix);
	}

	public static Instruction CreateRepeCmpsd(int addressSize)
	{
		return CreateString_SegRSI_ESRDI(Code.Cmpsd_m32_m32, addressSize, Register.None, RepPrefixKind.Repe);
	}

	public static Instruction CreateRepneCmpsd(int addressSize)
	{
		return CreateString_SegRSI_ESRDI(Code.Cmpsd_m32_m32, addressSize, Register.None, RepPrefixKind.Repne);
	}

	public static Instruction CreateCmpsq(int addressSize, Register segmentPrefix = Register.None, RepPrefixKind repPrefix = RepPrefixKind.None)
	{
		return CreateString_SegRSI_ESRDI(Code.Cmpsq_m64_m64, addressSize, segmentPrefix, repPrefix);
	}

	public static Instruction CreateRepeCmpsq(int addressSize)
	{
		return CreateString_SegRSI_ESRDI(Code.Cmpsq_m64_m64, addressSize, Register.None, RepPrefixKind.Repe);
	}

	public static Instruction CreateRepneCmpsq(int addressSize)
	{
		return CreateString_SegRSI_ESRDI(Code.Cmpsq_m64_m64, addressSize, Register.None, RepPrefixKind.Repne);
	}

	public static Instruction CreateMovsb(int addressSize, Register segmentPrefix = Register.None, RepPrefixKind repPrefix = RepPrefixKind.None)
	{
		return CreateString_ESRDI_SegRSI(Code.Movsb_m8_m8, addressSize, segmentPrefix, repPrefix);
	}

	public static Instruction CreateRepMovsb(int addressSize)
	{
		return CreateString_ESRDI_SegRSI(Code.Movsb_m8_m8, addressSize, Register.None, RepPrefixKind.Repe);
	}

	public static Instruction CreateMovsw(int addressSize, Register segmentPrefix = Register.None, RepPrefixKind repPrefix = RepPrefixKind.None)
	{
		return CreateString_ESRDI_SegRSI(Code.Movsw_m16_m16, addressSize, segmentPrefix, repPrefix);
	}

	public static Instruction CreateRepMovsw(int addressSize)
	{
		return CreateString_ESRDI_SegRSI(Code.Movsw_m16_m16, addressSize, Register.None, RepPrefixKind.Repe);
	}

	public static Instruction CreateMovsd(int addressSize, Register segmentPrefix = Register.None, RepPrefixKind repPrefix = RepPrefixKind.None)
	{
		return CreateString_ESRDI_SegRSI(Code.Movsd_m32_m32, addressSize, segmentPrefix, repPrefix);
	}

	public static Instruction CreateRepMovsd(int addressSize)
	{
		return CreateString_ESRDI_SegRSI(Code.Movsd_m32_m32, addressSize, Register.None, RepPrefixKind.Repe);
	}

	public static Instruction CreateMovsq(int addressSize, Register segmentPrefix = Register.None, RepPrefixKind repPrefix = RepPrefixKind.None)
	{
		return CreateString_ESRDI_SegRSI(Code.Movsq_m64_m64, addressSize, segmentPrefix, repPrefix);
	}

	public static Instruction CreateRepMovsq(int addressSize)
	{
		return CreateString_ESRDI_SegRSI(Code.Movsq_m64_m64, addressSize, Register.None, RepPrefixKind.Repe);
	}

	public static Instruction CreateMaskmovq(int addressSize, Register register1, Register register2, Register segmentPrefix = Register.None)
	{
		return CreateMaskmov(Code.Maskmovq_rDI_mm_mm, addressSize, register1, register2, segmentPrefix);
	}

	public static Instruction CreateMaskmovdqu(int addressSize, Register register1, Register register2, Register segmentPrefix = Register.None)
	{
		return CreateMaskmov(Code.Maskmovdqu_rDI_xmm_xmm, addressSize, register1, register2, segmentPrefix);
	}

	public static Instruction CreateVmaskmovdqu(int addressSize, Register register1, Register register2, Register segmentPrefix = Register.None)
	{
		return CreateMaskmov(Code.VEX_Vmaskmovdqu_rDI_xmm_xmm, addressSize, register1, register2, segmentPrefix);
	}

	public static Instruction CreateDeclareByte(byte b0)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareByte;
		result.InternalDeclareDataCount = 1u;
		result.SetDeclareByteValue(0, b0);
		return result;
	}

	public static Instruction CreateDeclareByte(byte b0, byte b1)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareByte;
		result.InternalDeclareDataCount = 2u;
		result.SetDeclareByteValue(0, b0);
		result.SetDeclareByteValue(1, b1);
		return result;
	}

	public static Instruction CreateDeclareByte(byte b0, byte b1, byte b2)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareByte;
		result.InternalDeclareDataCount = 3u;
		result.SetDeclareByteValue(0, b0);
		result.SetDeclareByteValue(1, b1);
		result.SetDeclareByteValue(2, b2);
		return result;
	}

	public static Instruction CreateDeclareByte(byte b0, byte b1, byte b2, byte b3)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareByte;
		result.InternalDeclareDataCount = 4u;
		result.SetDeclareByteValue(0, b0);
		result.SetDeclareByteValue(1, b1);
		result.SetDeclareByteValue(2, b2);
		result.SetDeclareByteValue(3, b3);
		return result;
	}

	public static Instruction CreateDeclareByte(byte b0, byte b1, byte b2, byte b3, byte b4)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareByte;
		result.InternalDeclareDataCount = 5u;
		result.SetDeclareByteValue(0, b0);
		result.SetDeclareByteValue(1, b1);
		result.SetDeclareByteValue(2, b2);
		result.SetDeclareByteValue(3, b3);
		result.SetDeclareByteValue(4, b4);
		return result;
	}

	public static Instruction CreateDeclareByte(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareByte;
		result.InternalDeclareDataCount = 6u;
		result.SetDeclareByteValue(0, b0);
		result.SetDeclareByteValue(1, b1);
		result.SetDeclareByteValue(2, b2);
		result.SetDeclareByteValue(3, b3);
		result.SetDeclareByteValue(4, b4);
		result.SetDeclareByteValue(5, b5);
		return result;
	}

	public static Instruction CreateDeclareByte(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareByte;
		result.InternalDeclareDataCount = 7u;
		result.SetDeclareByteValue(0, b0);
		result.SetDeclareByteValue(1, b1);
		result.SetDeclareByteValue(2, b2);
		result.SetDeclareByteValue(3, b3);
		result.SetDeclareByteValue(4, b4);
		result.SetDeclareByteValue(5, b5);
		result.SetDeclareByteValue(6, b6);
		return result;
	}

	public static Instruction CreateDeclareByte(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareByte;
		result.InternalDeclareDataCount = 8u;
		result.SetDeclareByteValue(0, b0);
		result.SetDeclareByteValue(1, b1);
		result.SetDeclareByteValue(2, b2);
		result.SetDeclareByteValue(3, b3);
		result.SetDeclareByteValue(4, b4);
		result.SetDeclareByteValue(5, b5);
		result.SetDeclareByteValue(6, b6);
		result.SetDeclareByteValue(7, b7);
		return result;
	}

	public static Instruction CreateDeclareByte(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7, byte b8)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareByte;
		result.InternalDeclareDataCount = 9u;
		result.SetDeclareByteValue(0, b0);
		result.SetDeclareByteValue(1, b1);
		result.SetDeclareByteValue(2, b2);
		result.SetDeclareByteValue(3, b3);
		result.SetDeclareByteValue(4, b4);
		result.SetDeclareByteValue(5, b5);
		result.SetDeclareByteValue(6, b6);
		result.SetDeclareByteValue(7, b7);
		result.SetDeclareByteValue(8, b8);
		return result;
	}

	public static Instruction CreateDeclareByte(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7, byte b8, byte b9)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareByte;
		result.InternalDeclareDataCount = 10u;
		result.SetDeclareByteValue(0, b0);
		result.SetDeclareByteValue(1, b1);
		result.SetDeclareByteValue(2, b2);
		result.SetDeclareByteValue(3, b3);
		result.SetDeclareByteValue(4, b4);
		result.SetDeclareByteValue(5, b5);
		result.SetDeclareByteValue(6, b6);
		result.SetDeclareByteValue(7, b7);
		result.SetDeclareByteValue(8, b8);
		result.SetDeclareByteValue(9, b9);
		return result;
	}

	public static Instruction CreateDeclareByte(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7, byte b8, byte b9, byte b10)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareByte;
		result.InternalDeclareDataCount = 11u;
		result.SetDeclareByteValue(0, b0);
		result.SetDeclareByteValue(1, b1);
		result.SetDeclareByteValue(2, b2);
		result.SetDeclareByteValue(3, b3);
		result.SetDeclareByteValue(4, b4);
		result.SetDeclareByteValue(5, b5);
		result.SetDeclareByteValue(6, b6);
		result.SetDeclareByteValue(7, b7);
		result.SetDeclareByteValue(8, b8);
		result.SetDeclareByteValue(9, b9);
		result.SetDeclareByteValue(10, b10);
		return result;
	}

	public static Instruction CreateDeclareByte(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7, byte b8, byte b9, byte b10, byte b11)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareByte;
		result.InternalDeclareDataCount = 12u;
		result.SetDeclareByteValue(0, b0);
		result.SetDeclareByteValue(1, b1);
		result.SetDeclareByteValue(2, b2);
		result.SetDeclareByteValue(3, b3);
		result.SetDeclareByteValue(4, b4);
		result.SetDeclareByteValue(5, b5);
		result.SetDeclareByteValue(6, b6);
		result.SetDeclareByteValue(7, b7);
		result.SetDeclareByteValue(8, b8);
		result.SetDeclareByteValue(9, b9);
		result.SetDeclareByteValue(10, b10);
		result.SetDeclareByteValue(11, b11);
		return result;
	}

	public static Instruction CreateDeclareByte(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7, byte b8, byte b9, byte b10, byte b11, byte b12)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareByte;
		result.InternalDeclareDataCount = 13u;
		result.SetDeclareByteValue(0, b0);
		result.SetDeclareByteValue(1, b1);
		result.SetDeclareByteValue(2, b2);
		result.SetDeclareByteValue(3, b3);
		result.SetDeclareByteValue(4, b4);
		result.SetDeclareByteValue(5, b5);
		result.SetDeclareByteValue(6, b6);
		result.SetDeclareByteValue(7, b7);
		result.SetDeclareByteValue(8, b8);
		result.SetDeclareByteValue(9, b9);
		result.SetDeclareByteValue(10, b10);
		result.SetDeclareByteValue(11, b11);
		result.SetDeclareByteValue(12, b12);
		return result;
	}

	public static Instruction CreateDeclareByte(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7, byte b8, byte b9, byte b10, byte b11, byte b12, byte b13)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareByte;
		result.InternalDeclareDataCount = 14u;
		result.SetDeclareByteValue(0, b0);
		result.SetDeclareByteValue(1, b1);
		result.SetDeclareByteValue(2, b2);
		result.SetDeclareByteValue(3, b3);
		result.SetDeclareByteValue(4, b4);
		result.SetDeclareByteValue(5, b5);
		result.SetDeclareByteValue(6, b6);
		result.SetDeclareByteValue(7, b7);
		result.SetDeclareByteValue(8, b8);
		result.SetDeclareByteValue(9, b9);
		result.SetDeclareByteValue(10, b10);
		result.SetDeclareByteValue(11, b11);
		result.SetDeclareByteValue(12, b12);
		result.SetDeclareByteValue(13, b13);
		return result;
	}

	public static Instruction CreateDeclareByte(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7, byte b8, byte b9, byte b10, byte b11, byte b12, byte b13, byte b14)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareByte;
		result.InternalDeclareDataCount = 15u;
		result.SetDeclareByteValue(0, b0);
		result.SetDeclareByteValue(1, b1);
		result.SetDeclareByteValue(2, b2);
		result.SetDeclareByteValue(3, b3);
		result.SetDeclareByteValue(4, b4);
		result.SetDeclareByteValue(5, b5);
		result.SetDeclareByteValue(6, b6);
		result.SetDeclareByteValue(7, b7);
		result.SetDeclareByteValue(8, b8);
		result.SetDeclareByteValue(9, b9);
		result.SetDeclareByteValue(10, b10);
		result.SetDeclareByteValue(11, b11);
		result.SetDeclareByteValue(12, b12);
		result.SetDeclareByteValue(13, b13);
		result.SetDeclareByteValue(14, b14);
		return result;
	}

	public static Instruction CreateDeclareByte(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7, byte b8, byte b9, byte b10, byte b11, byte b12, byte b13, byte b14, byte b15)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareByte;
		result.InternalDeclareDataCount = 16u;
		result.SetDeclareByteValue(0, b0);
		result.SetDeclareByteValue(1, b1);
		result.SetDeclareByteValue(2, b2);
		result.SetDeclareByteValue(3, b3);
		result.SetDeclareByteValue(4, b4);
		result.SetDeclareByteValue(5, b5);
		result.SetDeclareByteValue(6, b6);
		result.SetDeclareByteValue(7, b7);
		result.SetDeclareByteValue(8, b8);
		result.SetDeclareByteValue(9, b9);
		result.SetDeclareByteValue(10, b10);
		result.SetDeclareByteValue(11, b11);
		result.SetDeclareByteValue(12, b12);
		result.SetDeclareByteValue(13, b13);
		result.SetDeclareByteValue(14, b14);
		result.SetDeclareByteValue(15, b15);
		return result;
	}

	public static Instruction CreateDeclareByte(ReadOnlySpan<byte> data)
	{
		if ((uint)(data.Length - 1) > 15u)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_data();
		}
		Instruction result = default(Instruction);
		result.Code = Code.DeclareByte;
		result.InternalDeclareDataCount = (uint)data.Length;
		for (int i = 0; i < data.Length; i++)
		{
			result.SetDeclareByteValue(i, data[i]);
		}
		return result;
	}

	public static Instruction CreateDeclareByte(byte[] data)
	{
		if (data == null)
		{
			ThrowHelper.ThrowArgumentNullException_data();
		}
		return CreateDeclareByte(data, 0, data.Length);
	}

	public static Instruction CreateDeclareByte(byte[] data, int index, int length)
	{
		if (data == null)
		{
			ThrowHelper.ThrowArgumentNullException_data();
		}
		if ((uint)(length - 1) > 15u)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_length();
		}
		if ((ulong)((long)(uint)index + (long)(uint)length) > (ulong)(uint)data.Length)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_index();
		}
		Instruction result = default(Instruction);
		result.Code = Code.DeclareByte;
		result.InternalDeclareDataCount = (uint)length;
		for (int i = 0; i < length; i++)
		{
			result.SetDeclareByteValue(i, data[index + i]);
		}
		return result;
	}

	public static Instruction CreateDeclareWord(ushort w0)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareWord;
		result.InternalDeclareDataCount = 1u;
		result.SetDeclareWordValue(0, w0);
		return result;
	}

	public static Instruction CreateDeclareWord(ushort w0, ushort w1)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareWord;
		result.InternalDeclareDataCount = 2u;
		result.SetDeclareWordValue(0, w0);
		result.SetDeclareWordValue(1, w1);
		return result;
	}

	public static Instruction CreateDeclareWord(ushort w0, ushort w1, ushort w2)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareWord;
		result.InternalDeclareDataCount = 3u;
		result.SetDeclareWordValue(0, w0);
		result.SetDeclareWordValue(1, w1);
		result.SetDeclareWordValue(2, w2);
		return result;
	}

	public static Instruction CreateDeclareWord(ushort w0, ushort w1, ushort w2, ushort w3)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareWord;
		result.InternalDeclareDataCount = 4u;
		result.SetDeclareWordValue(0, w0);
		result.SetDeclareWordValue(1, w1);
		result.SetDeclareWordValue(2, w2);
		result.SetDeclareWordValue(3, w3);
		return result;
	}

	public static Instruction CreateDeclareWord(ushort w0, ushort w1, ushort w2, ushort w3, ushort w4)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareWord;
		result.InternalDeclareDataCount = 5u;
		result.SetDeclareWordValue(0, w0);
		result.SetDeclareWordValue(1, w1);
		result.SetDeclareWordValue(2, w2);
		result.SetDeclareWordValue(3, w3);
		result.SetDeclareWordValue(4, w4);
		return result;
	}

	public static Instruction CreateDeclareWord(ushort w0, ushort w1, ushort w2, ushort w3, ushort w4, ushort w5)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareWord;
		result.InternalDeclareDataCount = 6u;
		result.SetDeclareWordValue(0, w0);
		result.SetDeclareWordValue(1, w1);
		result.SetDeclareWordValue(2, w2);
		result.SetDeclareWordValue(3, w3);
		result.SetDeclareWordValue(4, w4);
		result.SetDeclareWordValue(5, w5);
		return result;
	}

	public static Instruction CreateDeclareWord(ushort w0, ushort w1, ushort w2, ushort w3, ushort w4, ushort w5, ushort w6)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareWord;
		result.InternalDeclareDataCount = 7u;
		result.SetDeclareWordValue(0, w0);
		result.SetDeclareWordValue(1, w1);
		result.SetDeclareWordValue(2, w2);
		result.SetDeclareWordValue(3, w3);
		result.SetDeclareWordValue(4, w4);
		result.SetDeclareWordValue(5, w5);
		result.SetDeclareWordValue(6, w6);
		return result;
	}

	public static Instruction CreateDeclareWord(ushort w0, ushort w1, ushort w2, ushort w3, ushort w4, ushort w5, ushort w6, ushort w7)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareWord;
		result.InternalDeclareDataCount = 8u;
		result.SetDeclareWordValue(0, w0);
		result.SetDeclareWordValue(1, w1);
		result.SetDeclareWordValue(2, w2);
		result.SetDeclareWordValue(3, w3);
		result.SetDeclareWordValue(4, w4);
		result.SetDeclareWordValue(5, w5);
		result.SetDeclareWordValue(6, w6);
		result.SetDeclareWordValue(7, w7);
		return result;
	}

	public static Instruction CreateDeclareWord(ReadOnlySpan<byte> data)
	{
		if ((uint)(data.Length - 1) > 15u || (data.Length & 1) != 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_data();
		}
		Instruction result = default(Instruction);
		result.Code = Code.DeclareWord;
		result.InternalDeclareDataCount = (uint)data.Length / 2u;
		for (int i = 0; i < data.Length; i += 2)
		{
			uint num = (uint)(data[i] | (data[i + 1] << 8));
			result.SetDeclareWordValue(i / 2, (ushort)num);
		}
		return result;
	}

	public static Instruction CreateDeclareWord(byte[] data)
	{
		if (data == null)
		{
			ThrowHelper.ThrowArgumentNullException_data();
		}
		return CreateDeclareWord(data, 0, data.Length);
	}

	public static Instruction CreateDeclareWord(byte[] data, int index, int length)
	{
		if (data == null)
		{
			ThrowHelper.ThrowArgumentNullException_data();
		}
		if ((uint)(length - 1) > 15u || (length & 1) != 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_length();
		}
		if ((ulong)((long)(uint)index + (long)(uint)length) > (ulong)(uint)data.Length)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_index();
		}
		Instruction result = default(Instruction);
		result.Code = Code.DeclareWord;
		result.InternalDeclareDataCount = (uint)length / 2u;
		for (int i = 0; i < length; i += 2)
		{
			uint num = (uint)(data[index + i] | (data[index + i + 1] << 8));
			result.SetDeclareWordValue(i / 2, (ushort)num);
		}
		return result;
	}

	public static Instruction CreateDeclareWord(ReadOnlySpan<ushort> data)
	{
		if ((uint)(data.Length - 1) > 7u)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_data();
		}
		Instruction result = default(Instruction);
		result.Code = Code.DeclareWord;
		result.InternalDeclareDataCount = (uint)data.Length;
		for (int i = 0; i < data.Length; i++)
		{
			result.SetDeclareWordValue(i, data[i]);
		}
		return result;
	}

	public static Instruction CreateDeclareWord(ushort[] data)
	{
		if (data == null)
		{
			ThrowHelper.ThrowArgumentNullException_data();
		}
		return CreateDeclareWord(data, 0, data.Length);
	}

	public static Instruction CreateDeclareWord(ushort[] data, int index, int length)
	{
		if (data == null)
		{
			ThrowHelper.ThrowArgumentNullException_data();
		}
		if ((uint)(length - 1) > 7u)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_length();
		}
		if ((ulong)((long)(uint)index + (long)(uint)length) > (ulong)(uint)data.Length)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_index();
		}
		Instruction result = default(Instruction);
		result.Code = Code.DeclareWord;
		result.InternalDeclareDataCount = (uint)length;
		for (int i = 0; i < length; i++)
		{
			result.SetDeclareWordValue(i, data[index + i]);
		}
		return result;
	}

	public static Instruction CreateDeclareDword(uint d0)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareDword;
		result.InternalDeclareDataCount = 1u;
		result.SetDeclareDwordValue(0, d0);
		return result;
	}

	public static Instruction CreateDeclareDword(uint d0, uint d1)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareDword;
		result.InternalDeclareDataCount = 2u;
		result.SetDeclareDwordValue(0, d0);
		result.SetDeclareDwordValue(1, d1);
		return result;
	}

	public static Instruction CreateDeclareDword(uint d0, uint d1, uint d2)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareDword;
		result.InternalDeclareDataCount = 3u;
		result.SetDeclareDwordValue(0, d0);
		result.SetDeclareDwordValue(1, d1);
		result.SetDeclareDwordValue(2, d2);
		return result;
	}

	public static Instruction CreateDeclareDword(uint d0, uint d1, uint d2, uint d3)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareDword;
		result.InternalDeclareDataCount = 4u;
		result.SetDeclareDwordValue(0, d0);
		result.SetDeclareDwordValue(1, d1);
		result.SetDeclareDwordValue(2, d2);
		result.SetDeclareDwordValue(3, d3);
		return result;
	}

	public static Instruction CreateDeclareDword(ReadOnlySpan<byte> data)
	{
		if ((uint)(data.Length - 1) > 15u || (data.Length & 3) != 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_data();
		}
		Instruction result = default(Instruction);
		result.Code = Code.DeclareDword;
		result.InternalDeclareDataCount = (uint)data.Length / 4u;
		for (int i = 0; i < data.Length; i += 4)
		{
			uint value = (uint)(data[i] | (data[i + 1] << 8) | (data[i + 2] << 16) | (data[i + 3] << 24));
			result.SetDeclareDwordValue(i / 4, value);
		}
		return result;
	}

	public static Instruction CreateDeclareDword(byte[] data)
	{
		if (data == null)
		{
			ThrowHelper.ThrowArgumentNullException_data();
		}
		return CreateDeclareDword(data, 0, data.Length);
	}

	public static Instruction CreateDeclareDword(byte[] data, int index, int length)
	{
		if (data == null)
		{
			ThrowHelper.ThrowArgumentNullException_data();
		}
		if ((uint)(length - 1) > 15u || (length & 3) != 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_length();
		}
		if ((ulong)((long)(uint)index + (long)(uint)length) > (ulong)(uint)data.Length)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_index();
		}
		Instruction result = default(Instruction);
		result.Code = Code.DeclareDword;
		result.InternalDeclareDataCount = (uint)length / 4u;
		for (int i = 0; i < length; i += 4)
		{
			uint value = (uint)(data[index + i] | (data[index + i + 1] << 8) | (data[index + i + 2] << 16) | (data[index + i + 3] << 24));
			result.SetDeclareDwordValue(i / 4, value);
		}
		return result;
	}

	public static Instruction CreateDeclareDword(ReadOnlySpan<uint> data)
	{
		if ((uint)(data.Length - 1) > 3u)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_data();
		}
		Instruction result = default(Instruction);
		result.Code = Code.DeclareDword;
		result.InternalDeclareDataCount = (uint)data.Length;
		for (int i = 0; i < data.Length; i++)
		{
			result.SetDeclareDwordValue(i, data[i]);
		}
		return result;
	}

	public static Instruction CreateDeclareDword(uint[] data)
	{
		if (data == null)
		{
			ThrowHelper.ThrowArgumentNullException_data();
		}
		return CreateDeclareDword(data, 0, data.Length);
	}

	public static Instruction CreateDeclareDword(uint[] data, int index, int length)
	{
		if (data == null)
		{
			ThrowHelper.ThrowArgumentNullException_data();
		}
		if ((uint)(length - 1) > 3u)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_length();
		}
		if ((ulong)((long)(uint)index + (long)(uint)length) > (ulong)(uint)data.Length)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_index();
		}
		Instruction result = default(Instruction);
		result.Code = Code.DeclareDword;
		result.InternalDeclareDataCount = (uint)length;
		for (int i = 0; i < length; i++)
		{
			result.SetDeclareDwordValue(i, data[index + i]);
		}
		return result;
	}

	public static Instruction CreateDeclareQword(ulong q0)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareQword;
		result.InternalDeclareDataCount = 1u;
		result.SetDeclareQwordValue(0, q0);
		return result;
	}

	public static Instruction CreateDeclareQword(ulong q0, ulong q1)
	{
		Instruction result = default(Instruction);
		result.Code = Code.DeclareQword;
		result.InternalDeclareDataCount = 2u;
		result.SetDeclareQwordValue(0, q0);
		result.SetDeclareQwordValue(1, q1);
		return result;
	}

	public static Instruction CreateDeclareQword(ReadOnlySpan<byte> data)
	{
		if ((uint)(data.Length - 1) > 15u || (data.Length & 7) != 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_data();
		}
		Instruction result = default(Instruction);
		result.Code = Code.DeclareQword;
		result.InternalDeclareDataCount = (uint)data.Length / 8u;
		for (int i = 0; i < data.Length; i += 8)
		{
			uint num = (uint)(data[i] | (data[i + 1] << 8) | (data[i + 2] << 16) | (data[i + 3] << 24));
			uint num2 = (uint)(data[i + 4] | (data[i + 5] << 8) | (data[i + 6] << 16) | (data[i + 7] << 24));
			result.SetDeclareQwordValue(i / 8, num | ((ulong)num2 << 32));
		}
		return result;
	}

	public static Instruction CreateDeclareQword(byte[] data)
	{
		if (data == null)
		{
			ThrowHelper.ThrowArgumentNullException_data();
		}
		return CreateDeclareQword(data, 0, data.Length);
	}

	public static Instruction CreateDeclareQword(byte[] data, int index, int length)
	{
		if (data == null)
		{
			ThrowHelper.ThrowArgumentNullException_data();
		}
		if ((uint)(length - 1) > 15u || (length & 7) != 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_length();
		}
		if ((ulong)((long)(uint)index + (long)(uint)length) > (ulong)(uint)data.Length)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_index();
		}
		Instruction result = default(Instruction);
		result.Code = Code.DeclareQword;
		result.InternalDeclareDataCount = (uint)length / 8u;
		for (int i = 0; i < length; i += 8)
		{
			uint num = (uint)(data[index + i] | (data[index + i + 1] << 8) | (data[index + i + 2] << 16) | (data[index + i + 3] << 24));
			uint num2 = (uint)(data[index + i + 4] | (data[index + i + 5] << 8) | (data[index + i + 6] << 16) | (data[index + i + 7] << 24));
			result.SetDeclareQwordValue(i / 8, num | ((ulong)num2 << 32));
		}
		return result;
	}

	public static Instruction CreateDeclareQword(ReadOnlySpan<ulong> data)
	{
		if ((uint)(data.Length - 1) > 1u)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_data();
		}
		Instruction result = default(Instruction);
		result.Code = Code.DeclareQword;
		result.InternalDeclareDataCount = (uint)data.Length;
		for (int i = 0; i < data.Length; i++)
		{
			result.SetDeclareQwordValue(i, data[i]);
		}
		return result;
	}

	public static Instruction CreateDeclareQword(ulong[] data)
	{
		if (data == null)
		{
			ThrowHelper.ThrowArgumentNullException_data();
		}
		return CreateDeclareQword(data, 0, data.Length);
	}

	public static Instruction CreateDeclareQword(ulong[] data, int index, int length)
	{
		if (data == null)
		{
			ThrowHelper.ThrowArgumentNullException_data();
		}
		if ((uint)(length - 1) > 1u)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_length();
		}
		if ((ulong)((long)(uint)index + (long)(uint)length) > (ulong)(uint)data.Length)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_index();
		}
		Instruction result = default(Instruction);
		result.Code = Code.DeclareQword;
		result.InternalDeclareDataCount = (uint)length;
		for (int i = 0; i < length; i++)
		{
			result.SetDeclareQwordValue(i, data[index + i]);
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(in Instruction left, in Instruction right)
	{
		return EqualsInternal(in left, in right);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(in Instruction left, in Instruction right)
	{
		return !EqualsInternal(in left, in right);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool Equals(in Instruction other)
	{
		return EqualsInternal(in this, in other);
	}

	readonly bool IEquatable<Instruction>.Equals(Instruction other)
	{
		return EqualsInternal(in this, in other);
	}

	private static bool EqualsInternal(in Instruction a, in Instruction b)
	{
		if (a.memDispl == b.memDispl && ((a.flags1 ^ b.flags1) & 0xFFF3FFFFu) == 0 && a.immediate == b.immediate && a.code == b.code && a.memBaseReg == b.memBaseReg && a.memIndexReg == b.memIndexReg && a.reg0 == b.reg0 && a.reg1 == b.reg1 && a.reg2 == b.reg2 && a.reg3 == b.reg3 && a.opKind0 == b.opKind0 && a.opKind1 == b.opKind1 && a.opKind2 == b.opKind2 && a.opKind3 == b.opKind3 && a.scale == b.scale && a.displSize == b.displSize)
		{
			return a.pad == b.pad;
		}
		return false;
	}

	public override readonly int GetHashCode()
	{
		return (int)((uint)((int)memDispl ^ (int)(memDispl >> 32)) ^ (flags1 & 0xFFF3FFFFu) ^ immediate ^ (uint)(code << 8) ^ (uint)(memBaseReg << 16) ^ (uint)(memIndexReg << 24) ^ reg3 ^ (uint)(reg2 << 8) ^ (uint)(reg1 << 16) ^ (uint)(reg0 << 24) ^ opKind3 ^ (uint)(opKind2 << 8) ^ (uint)(opKind1 << 16) ^ (uint)(opKind0 << 24) ^ scale ^ (uint)(displSize << 8)) ^ (pad << 16);
	}

	public override readonly bool Equals(object? obj)
	{
		if (obj is Instruction b)
		{
			return EqualsInternal(in this, in b);
		}
		return false;
	}

	public static bool EqualsAllBits(in Instruction a, in Instruction b)
	{
		if (a.nextRip == b.nextRip && a.memDispl == b.memDispl && a.flags1 == b.flags1 && a.immediate == b.immediate && a.code == b.code && a.memBaseReg == b.memBaseReg && a.memIndexReg == b.memIndexReg && a.reg0 == b.reg0 && a.reg1 == b.reg1 && a.reg2 == b.reg2 && a.reg3 == b.reg3 && a.opKind0 == b.opKind0 && a.opKind1 == b.opKind1 && a.opKind2 == b.opKind2 && a.opKind3 == b.opKind3 && a.scale == b.scale && a.displSize == b.displSize && a.len == b.len)
		{
			return a.pad == b.pad;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void InternalSetCodeNoCheck(Code code)
	{
		this.code = (ushort)code;
	}

	private readonly bool IsXacquireInstr()
	{
		if (Op0Kind != OpKind.Memory)
		{
			return false;
		}
		if (HasLockPrefix)
		{
			return Code != Code.Cmpxchg16b_m128;
		}
		return Mnemonic == Mnemonic.Xchg;
	}

	private readonly bool IsXreleaseInstr()
	{
		if (Op0Kind != OpKind.Memory)
		{
			return false;
		}
		if (HasLockPrefix)
		{
			return Code != Code.Cmpxchg16b_m128;
		}
		Code code = Code;
		if ((uint)(code - 275) <= 7u || code == Code.Mov_rm8_imm8 || (uint)(code - 403) <= 2u)
		{
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void InternalSetHasXacquirePrefix()
	{
		flags1 |= 1073741824u;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void InternalSetHasXreleasePrefix()
	{
		flags1 |= 536870912u;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void InternalSetHasRepePrefix()
	{
		flags1 = (flags1 & 0xBFFFFFFFu) | 0x20000000;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void InternalClearHasRepePrefix()
	{
		flags1 &= 3758096383u;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void InternalClearHasRepeRepnePrefix()
	{
		flags1 &= 2684354559u;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void InternalSetHasRepnePrefix()
	{
		flags1 = (flags1 & 0xDFFFFFFFu) | 0x40000000;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void InternalClearHasRepnePrefix()
	{
		flags1 &= 3221225471u;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void InternalSetHasLockPrefix()
	{
		flags1 |= 2147483648u;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void InternalClearHasLockPrefix()
	{
		flags1 &= 2147483647u;
	}

	public readonly OpKind GetOpKind(int operand)
	{
		switch (operand)
		{
		case 0:
			return Op0Kind;
		case 1:
			return Op1Kind;
		case 2:
			return Op2Kind;
		case 3:
			return Op3Kind;
		case 4:
			return Op4Kind;
		default:
			ThrowHelper.ThrowArgumentOutOfRangeException_operand();
			return OpKind.Register;
		}
	}

	public readonly bool HasOpKind(OpKind opKind)
	{
		for (int i = 0; i < OpCount; i++)
		{
			if (GetOpKind(i) == opKind)
			{
				return true;
			}
		}
		return false;
	}

	public void SetOpKind(int operand, OpKind opKind)
	{
		switch (operand)
		{
		case 0:
			Op0Kind = opKind;
			break;
		case 1:
			Op1Kind = opKind;
			break;
		case 2:
			Op2Kind = opKind;
			break;
		case 3:
			Op3Kind = opKind;
			break;
		case 4:
			Op4Kind = opKind;
			break;
		default:
			ThrowHelper.ThrowArgumentOutOfRangeException_operand();
			break;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void InternalSetMemoryDisplSize(uint scale)
	{
		displSize = (byte)scale;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void InternalSetIsBroadcast()
	{
		flags1 |= 67108864u;
	}

	public readonly ulong GetImmediate(int operand)
	{
		return GetOpKind(operand) switch
		{
			OpKind.Immediate8 => Immediate8, 
			OpKind.Immediate8_2nd => Immediate8_2nd, 
			OpKind.Immediate16 => Immediate16, 
			OpKind.Immediate32 => Immediate32, 
			OpKind.Immediate64 => Immediate64, 
			OpKind.Immediate8to16 => (ulong)Immediate8to16, 
			OpKind.Immediate8to32 => (ulong)Immediate8to32, 
			OpKind.Immediate8to64 => (ulong)Immediate8to64, 
			OpKind.Immediate32to64 => (ulong)Immediate32to64, 
			_ => throw new ArgumentException($"Op{operand} isn't an immediate operand", "operand"), 
		};
	}

	public void SetImmediate(int operand, int immediate)
	{
		SetImmediate(operand, (ulong)immediate);
	}

	public void SetImmediate(int operand, uint immediate)
	{
		SetImmediate(operand, (ulong)immediate);
	}

	public void SetImmediate(int operand, long immediate)
	{
		SetImmediate(operand, (ulong)immediate);
	}

	public void SetImmediate(int operand, ulong immediate)
	{
		switch (GetOpKind(operand))
		{
		case OpKind.Immediate8:
			Immediate8 = (byte)immediate;
			return;
		case OpKind.Immediate8to16:
			Immediate8to16 = (short)immediate;
			return;
		case OpKind.Immediate8to32:
			Immediate8to32 = (int)immediate;
			return;
		case OpKind.Immediate8to64:
			Immediate8to64 = (long)immediate;
			return;
		case OpKind.Immediate8_2nd:
			Immediate8_2nd = (byte)immediate;
			return;
		case OpKind.Immediate16:
			Immediate16 = (ushort)immediate;
			return;
		case OpKind.Immediate32to64:
			Immediate32to64 = (long)immediate;
			return;
		case OpKind.Immediate32:
			Immediate32 = (uint)immediate;
			return;
		case OpKind.Immediate64:
			Immediate64 = immediate;
			return;
		}
		throw new ArgumentException($"Op{operand} isn't an immediate operand", "operand");
	}

	public readonly Register GetOpRegister(int operand)
	{
		switch (operand)
		{
		case 0:
			return Op0Register;
		case 1:
			return Op1Register;
		case 2:
			return Op2Register;
		case 3:
			return Op3Register;
		case 4:
			return Op4Register;
		default:
			ThrowHelper.ThrowArgumentOutOfRangeException_operand();
			return Register.None;
		}
	}

	public void SetOpRegister(int operand, Register register)
	{
		switch (operand)
		{
		case 0:
			Op0Register = register;
			break;
		case 1:
			Op1Register = register;
			break;
		case 2:
			Op2Register = register;
			break;
		case 3:
			Op3Register = register;
			break;
		case 4:
			Op4Register = register;
			break;
		default:
			ThrowHelper.ThrowArgumentOutOfRangeException_operand();
			break;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void InternalSetZeroingMasking()
	{
		flags1 |= 268435456u;
	}

	public void SetDeclareByteValue(int index, sbyte value)
	{
		SetDeclareByteValue(index, (byte)value);
	}

	public void SetDeclareByteValue(int index, byte value)
	{
		switch (index)
		{
		case 0:
			reg0 = value;
			break;
		case 1:
			reg1 = value;
			break;
		case 2:
			reg2 = value;
			break;
		case 3:
			reg3 = value;
			break;
		case 4:
			immediate = (immediate & 0xFFFFFF00u) | value;
			break;
		case 5:
			immediate = (immediate & 0xFFFF00FFu) | (uint)(value << 8);
			break;
		case 6:
			immediate = (immediate & 0xFF00FFFFu) | (uint)(value << 16);
			break;
		case 7:
			immediate = (immediate & 0xFFFFFF) | (uint)(value << 24);
			break;
		case 8:
			memDispl = (memDispl & 0xFFFFFFFFFFFFFF00uL) | value;
			break;
		case 9:
			memDispl = (memDispl & 0xFFFFFFFFFFFF00FFuL) | ((ulong)value << 8);
			break;
		case 10:
			memDispl = (memDispl & 0xFFFFFFFFFF00FFFFuL) | ((ulong)value << 16);
			break;
		case 11:
			memDispl = (memDispl & 0xFFFFFFFF00FFFFFFuL) | ((ulong)value << 24);
			break;
		case 12:
			memDispl = (memDispl & 0xFFFFFF00FFFFFFFFuL) | ((ulong)value << 32);
			break;
		case 13:
			memDispl = (memDispl & 0xFFFF00FFFFFFFFFFuL) | ((ulong)value << 40);
			break;
		case 14:
			memDispl = (memDispl & 0xFF00FFFFFFFFFFFFuL) | ((ulong)value << 48);
			break;
		case 15:
			memDispl = (memDispl & 0xFFFFFFFFFFFFFFL) | ((ulong)value << 56);
			break;
		default:
			ThrowHelper.ThrowArgumentOutOfRangeException_index();
			break;
		}
	}

	public readonly byte GetDeclareByteValue(int index)
	{
		switch (index)
		{
		case 0:
			return reg0;
		case 1:
			return reg1;
		case 2:
			return reg2;
		case 3:
			return reg3;
		case 4:
			return (byte)immediate;
		case 5:
			return (byte)(immediate >> 8);
		case 6:
			return (byte)(immediate >> 16);
		case 7:
			return (byte)(immediate >> 24);
		case 8:
			return (byte)memDispl;
		case 9:
			return (byte)((uint)memDispl >> 8);
		case 10:
			return (byte)((uint)memDispl >> 16);
		case 11:
			return (byte)((uint)memDispl >> 24);
		case 12:
			return (byte)(memDispl >> 32);
		case 13:
			return (byte)(memDispl >> 40);
		case 14:
			return (byte)(memDispl >> 48);
		case 15:
			return (byte)(memDispl >> 56);
		default:
			ThrowHelper.ThrowArgumentOutOfRangeException_index();
			return 0;
		}
	}

	public void SetDeclareWordValue(int index, short value)
	{
		SetDeclareWordValue(index, (ushort)value);
	}

	public void SetDeclareWordValue(int index, ushort value)
	{
		switch (index)
		{
		case 0:
			reg0 = (byte)value;
			reg1 = (byte)(value >> 8);
			break;
		case 1:
			reg2 = (byte)value;
			reg3 = (byte)(value >> 8);
			break;
		case 2:
			immediate = (immediate & 0xFFFF0000u) | value;
			break;
		case 3:
			immediate = (uint)((ushort)immediate | (value << 16));
			break;
		case 4:
			memDispl = (memDispl & 0xFFFFFFFFFFFF0000uL) | value;
			break;
		case 5:
			memDispl = (memDispl & 0xFFFFFFFF0000FFFFuL) | ((ulong)value << 16);
			break;
		case 6:
			memDispl = (memDispl & 0xFFFF0000FFFFFFFFuL) | ((ulong)value << 32);
			break;
		case 7:
			memDispl = (memDispl & 0xFFFFFFFFFFFFL) | ((ulong)value << 48);
			break;
		default:
			ThrowHelper.ThrowArgumentOutOfRangeException_index();
			break;
		}
	}

	public readonly ushort GetDeclareWordValue(int index)
	{
		switch (index)
		{
		case 0:
			return (ushort)(reg0 | (reg1 << 8));
		case 1:
			return (ushort)(reg2 | (reg3 << 8));
		case 2:
			return (ushort)immediate;
		case 3:
			return (ushort)(immediate >> 16);
		case 4:
			return (ushort)memDispl;
		case 5:
			return (ushort)((uint)memDispl >> 16);
		case 6:
			return (ushort)(memDispl >> 32);
		case 7:
			return (ushort)(memDispl >> 48);
		default:
			ThrowHelper.ThrowArgumentOutOfRangeException_index();
			return 0;
		}
	}

	public void SetDeclareDwordValue(int index, int value)
	{
		SetDeclareDwordValue(index, (uint)value);
	}

	public void SetDeclareDwordValue(int index, uint value)
	{
		switch (index)
		{
		case 0:
			reg0 = (byte)value;
			reg1 = (byte)(value >> 8);
			reg2 = (byte)(value >> 16);
			reg3 = (byte)(value >> 24);
			break;
		case 1:
			immediate = value;
			break;
		case 2:
			memDispl = (memDispl & 0xFFFFFFFF00000000uL) | value;
			break;
		case 3:
			memDispl = (memDispl & 0xFFFFFFFFu) | ((ulong)value << 32);
			break;
		default:
			ThrowHelper.ThrowArgumentOutOfRangeException_index();
			break;
		}
	}

	public readonly uint GetDeclareDwordValue(int index)
	{
		switch (index)
		{
		case 0:
			return (uint)(reg0 | (reg1 << 8) | (reg2 << 16) | (reg3 << 24));
		case 1:
			return immediate;
		case 2:
			return (uint)memDispl;
		case 3:
			return (uint)(memDispl >> 32);
		default:
			ThrowHelper.ThrowArgumentOutOfRangeException_index();
			return 0u;
		}
	}

	public void SetDeclareQwordValue(int index, long value)
	{
		SetDeclareQwordValue(index, (ulong)value);
	}

	public void SetDeclareQwordValue(int index, ulong value)
	{
		switch (index)
		{
		case 0:
		{
			uint num = (uint)value;
			reg0 = (byte)num;
			reg1 = (byte)(num >> 8);
			reg2 = (byte)(num >> 16);
			reg3 = (byte)(num >> 24);
			immediate = (uint)(value >> 32);
			break;
		}
		case 1:
			memDispl = value;
			break;
		default:
			ThrowHelper.ThrowArgumentOutOfRangeException_index();
			break;
		}
	}

	public readonly ulong GetDeclareQwordValue(int index)
	{
		switch (index)
		{
		case 0:
			return (ulong)reg0 | (ulong)(uint)(reg1 << 8) | (uint)(reg2 << 16) | (uint)(reg3 << 24) | ((ulong)immediate << 32);
		case 1:
			return memDispl;
		default:
			ThrowHelper.ThrowArgumentOutOfRangeException_index();
			return 0uL;
		}
	}

	public readonly bool TryGetVsib64(out bool vsib64)
	{
		switch (Code)
		{
		case Code.VEX_Vpgatherdd_xmm_vm32x_xmm:
		case Code.VEX_Vpgatherdd_ymm_vm32y_ymm:
		case Code.VEX_Vpgatherdq_xmm_vm32x_xmm:
		case Code.VEX_Vpgatherdq_ymm_vm32x_ymm:
		case Code.EVEX_Vpgatherdd_xmm_k1_vm32x:
		case Code.EVEX_Vpgatherdd_ymm_k1_vm32y:
		case Code.EVEX_Vpgatherdd_zmm_k1_vm32z:
		case Code.EVEX_Vpgatherdq_xmm_k1_vm32x:
		case Code.EVEX_Vpgatherdq_ymm_k1_vm32x:
		case Code.EVEX_Vpgatherdq_zmm_k1_vm32y:
		case Code.VEX_Vgatherdps_xmm_vm32x_xmm:
		case Code.VEX_Vgatherdps_ymm_vm32y_ymm:
		case Code.VEX_Vgatherdpd_xmm_vm32x_xmm:
		case Code.VEX_Vgatherdpd_ymm_vm32x_ymm:
		case Code.EVEX_Vgatherdps_xmm_k1_vm32x:
		case Code.EVEX_Vgatherdps_ymm_k1_vm32y:
		case Code.EVEX_Vgatherdps_zmm_k1_vm32z:
		case Code.EVEX_Vgatherdpd_xmm_k1_vm32x:
		case Code.EVEX_Vgatherdpd_ymm_k1_vm32x:
		case Code.EVEX_Vgatherdpd_zmm_k1_vm32y:
		case Code.EVEX_Vpscatterdd_vm32x_k1_xmm:
		case Code.EVEX_Vpscatterdd_vm32y_k1_ymm:
		case Code.EVEX_Vpscatterdd_vm32z_k1_zmm:
		case Code.EVEX_Vpscatterdq_vm32x_k1_xmm:
		case Code.EVEX_Vpscatterdq_vm32x_k1_ymm:
		case Code.EVEX_Vpscatterdq_vm32y_k1_zmm:
		case Code.EVEX_Vscatterdps_vm32x_k1_xmm:
		case Code.EVEX_Vscatterdps_vm32y_k1_ymm:
		case Code.EVEX_Vscatterdps_vm32z_k1_zmm:
		case Code.EVEX_Vscatterdpd_vm32x_k1_xmm:
		case Code.EVEX_Vscatterdpd_vm32x_k1_ymm:
		case Code.EVEX_Vscatterdpd_vm32y_k1_zmm:
		case Code.EVEX_Vgatherpf0dps_vm32z_k1:
		case Code.EVEX_Vgatherpf0dpd_vm32y_k1:
		case Code.EVEX_Vgatherpf1dps_vm32z_k1:
		case Code.EVEX_Vgatherpf1dpd_vm32y_k1:
		case Code.EVEX_Vscatterpf0dps_vm32z_k1:
		case Code.EVEX_Vscatterpf0dpd_vm32y_k1:
		case Code.EVEX_Vscatterpf1dps_vm32z_k1:
		case Code.EVEX_Vscatterpf1dpd_vm32y_k1:
		case Code.MVEX_Vpgatherdd_zmm_k1_mvt:
		case Code.MVEX_Vpgatherdq_zmm_k1_mvt:
		case Code.MVEX_Vgatherdps_zmm_k1_mvt:
		case Code.MVEX_Vgatherdpd_zmm_k1_mvt:
		case Code.MVEX_Vpscatterdd_mvt_k1_zmm:
		case Code.MVEX_Vpscatterdq_mvt_k1_zmm:
		case Code.MVEX_Vscatterdps_mvt_k1_zmm:
		case Code.MVEX_Vscatterdpd_mvt_k1_zmm:
		case Code.MVEX_Undoc_zmm_k1_mvt_512_66_0F38_W0_B0:
		case Code.MVEX_Undoc_zmm_k1_mvt_512_66_0F38_W0_B2:
		case Code.MVEX_Undoc_zmm_k1_mvt_512_66_0F38_W0_C0:
		case Code.MVEX_Vgatherpf0hintdps_mvt_k1:
		case Code.MVEX_Vgatherpf0hintdpd_mvt_k1:
		case Code.MVEX_Vgatherpf0dps_mvt_k1:
		case Code.MVEX_Vgatherpf1dps_mvt_k1:
		case Code.MVEX_Vscatterpf0hintdps_mvt_k1:
		case Code.MVEX_Vscatterpf0hintdpd_mvt_k1:
		case Code.MVEX_Vscatterpf0dps_mvt_k1:
		case Code.MVEX_Vscatterpf1dps_mvt_k1:
			vsib64 = false;
			return true;
		case Code.VEX_Vpgatherqd_xmm_vm64x_xmm:
		case Code.VEX_Vpgatherqd_xmm_vm64y_xmm:
		case Code.VEX_Vpgatherqq_xmm_vm64x_xmm:
		case Code.VEX_Vpgatherqq_ymm_vm64y_ymm:
		case Code.EVEX_Vpgatherqd_xmm_k1_vm64x:
		case Code.EVEX_Vpgatherqd_xmm_k1_vm64y:
		case Code.EVEX_Vpgatherqd_ymm_k1_vm64z:
		case Code.EVEX_Vpgatherqq_xmm_k1_vm64x:
		case Code.EVEX_Vpgatherqq_ymm_k1_vm64y:
		case Code.EVEX_Vpgatherqq_zmm_k1_vm64z:
		case Code.VEX_Vgatherqps_xmm_vm64x_xmm:
		case Code.VEX_Vgatherqps_xmm_vm64y_xmm:
		case Code.VEX_Vgatherqpd_xmm_vm64x_xmm:
		case Code.VEX_Vgatherqpd_ymm_vm64y_ymm:
		case Code.EVEX_Vgatherqps_xmm_k1_vm64x:
		case Code.EVEX_Vgatherqps_xmm_k1_vm64y:
		case Code.EVEX_Vgatherqps_ymm_k1_vm64z:
		case Code.EVEX_Vgatherqpd_xmm_k1_vm64x:
		case Code.EVEX_Vgatherqpd_ymm_k1_vm64y:
		case Code.EVEX_Vgatherqpd_zmm_k1_vm64z:
		case Code.EVEX_Vpscatterqd_vm64x_k1_xmm:
		case Code.EVEX_Vpscatterqd_vm64y_k1_xmm:
		case Code.EVEX_Vpscatterqd_vm64z_k1_ymm:
		case Code.EVEX_Vpscatterqq_vm64x_k1_xmm:
		case Code.EVEX_Vpscatterqq_vm64y_k1_ymm:
		case Code.EVEX_Vpscatterqq_vm64z_k1_zmm:
		case Code.EVEX_Vscatterqps_vm64x_k1_xmm:
		case Code.EVEX_Vscatterqps_vm64y_k1_xmm:
		case Code.EVEX_Vscatterqps_vm64z_k1_ymm:
		case Code.EVEX_Vscatterqpd_vm64x_k1_xmm:
		case Code.EVEX_Vscatterqpd_vm64y_k1_ymm:
		case Code.EVEX_Vscatterqpd_vm64z_k1_zmm:
		case Code.EVEX_Vgatherpf0qps_vm64z_k1:
		case Code.EVEX_Vgatherpf0qpd_vm64z_k1:
		case Code.EVEX_Vgatherpf1qps_vm64z_k1:
		case Code.EVEX_Vgatherpf1qpd_vm64z_k1:
		case Code.EVEX_Vscatterpf0qps_vm64z_k1:
		case Code.EVEX_Vscatterpf0qpd_vm64z_k1:
		case Code.EVEX_Vscatterpf1qps_vm64z_k1:
		case Code.EVEX_Vscatterpf1qpd_vm64z_k1:
			vsib64 = true;
			return true;
		default:
			vsib64 = false;
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void InternalSetSuppressAllExceptions()
	{
		flags1 |= 134217728u;
	}

	public override readonly string ToString()
	{
		return base.ToString() ?? string.Empty;
	}

	public readonly ulong GetVirtualAddress(int operand, int elementIndex, VAGetRegisterValue getRegisterValue)
	{
		if (getRegisterValue == null)
		{
			throw new ArgumentNullException("getRegisterValue");
		}
		VARegisterValueProviderDelegateImpl registerValueProvider = new VARegisterValueProviderDelegateImpl(getRegisterValue);
		if (TryGetVirtualAddress(operand, elementIndex, registerValueProvider, out var result))
		{
			return result;
		}
		return 0uL;
	}

	public readonly ulong GetVirtualAddress(int operand, int elementIndex, IVARegisterValueProvider registerValueProvider)
	{
		if (registerValueProvider == null)
		{
			throw new ArgumentNullException("registerValueProvider");
		}
		VARegisterValueProviderAdapter registerValueProvider2 = new VARegisterValueProviderAdapter(registerValueProvider);
		if (TryGetVirtualAddress(operand, elementIndex, registerValueProvider2, out var result))
		{
			return result;
		}
		return 0uL;
	}

	public readonly bool TryGetVirtualAddress(int operand, int elementIndex, out ulong result, VATryGetRegisterValue getRegisterValue)
	{
		if (getRegisterValue == null)
		{
			throw new ArgumentNullException("getRegisterValue");
		}
		VATryGetRegisterValueDelegateImpl registerValueProvider = new VATryGetRegisterValueDelegateImpl(getRegisterValue);
		return TryGetVirtualAddress(operand, elementIndex, registerValueProvider, out result);
	}

	public readonly bool TryGetVirtualAddress(int operand, int elementIndex, IVATryGetRegisterValueProvider registerValueProvider, out ulong result)
	{
		if (registerValueProvider == null)
		{
			throw new ArgumentNullException("registerValueProvider");
		}
		ulong value2;
		ulong value;
		switch (GetOpKind(operand))
		{
		case OpKind.Register:
		case OpKind.NearBranch16:
		case OpKind.NearBranch32:
		case OpKind.NearBranch64:
		case OpKind.FarBranch16:
		case OpKind.FarBranch32:
		case OpKind.Immediate8:
		case OpKind.Immediate8_2nd:
		case OpKind.Immediate16:
		case OpKind.Immediate32:
		case OpKind.Immediate64:
		case OpKind.Immediate8to16:
		case OpKind.Immediate8to32:
		case OpKind.Immediate8to64:
		case OpKind.Immediate32to64:
			result = 0uL;
			return true;
		case OpKind.MemorySegSI:
			if (registerValueProvider.TryGetRegisterValue(MemorySegment, 0, 0, out value2) && registerValueProvider.TryGetRegisterValue(Register.SI, 0, 0, out value))
			{
				result = value2 + (ushort)value;
				return true;
			}
			break;
		case OpKind.MemorySegESI:
			if (registerValueProvider.TryGetRegisterValue(MemorySegment, 0, 0, out value2) && registerValueProvider.TryGetRegisterValue(Register.ESI, 0, 0, out value))
			{
				result = value2 + (uint)value;
				return true;
			}
			break;
		case OpKind.MemorySegRSI:
			if (registerValueProvider.TryGetRegisterValue(MemorySegment, 0, 0, out value2) && registerValueProvider.TryGetRegisterValue(Register.RSI, 0, 0, out value))
			{
				result = value2 + value;
				return true;
			}
			break;
		case OpKind.MemorySegDI:
			if (registerValueProvider.TryGetRegisterValue(MemorySegment, 0, 0, out value2) && registerValueProvider.TryGetRegisterValue(Register.DI, 0, 0, out value))
			{
				result = value2 + (ushort)value;
				return true;
			}
			break;
		case OpKind.MemorySegEDI:
			if (registerValueProvider.TryGetRegisterValue(MemorySegment, 0, 0, out value2) && registerValueProvider.TryGetRegisterValue(Register.EDI, 0, 0, out value))
			{
				result = value2 + (uint)value;
				return true;
			}
			break;
		case OpKind.MemorySegRDI:
			if (registerValueProvider.TryGetRegisterValue(MemorySegment, 0, 0, out value2) && registerValueProvider.TryGetRegisterValue(Register.RDI, 0, 0, out value))
			{
				result = value2 + value;
				return true;
			}
			break;
		case OpKind.MemoryESDI:
			if (registerValueProvider.TryGetRegisterValue(Register.ES, 0, 0, out value2) && registerValueProvider.TryGetRegisterValue(Register.DI, 0, 0, out value))
			{
				result = value2 + (ushort)value;
				return true;
			}
			break;
		case OpKind.MemoryESEDI:
			if (registerValueProvider.TryGetRegisterValue(Register.ES, 0, 0, out value2) && registerValueProvider.TryGetRegisterValue(Register.EDI, 0, 0, out value))
			{
				result = value2 + (uint)value;
				return true;
			}
			break;
		case OpKind.MemoryESRDI:
			if (registerValueProvider.TryGetRegisterValue(Register.ES, 0, 0, out value2) && registerValueProvider.TryGetRegisterValue(Register.RDI, 0, 0, out value))
			{
				result = value2 + value;
				return true;
			}
			break;
		case OpKind.Memory:
		{
			Register memoryBase = MemoryBase;
			Register memoryIndex = MemoryIndex;
			int addressSizeInBytes = InstructionUtils.GetAddressSizeInBytes(memoryBase, memoryIndex, MemoryDisplSize, CodeSize);
			ulong num = MemoryDisplacement64;
			ulong num2 = addressSizeInBytes switch
			{
				8 => ulong.MaxValue, 
				4 => 4294967295uL, 
				_ => 65535uL, 
			};
			if (memoryBase != 0 && memoryBase != Register.RIP && memoryBase != Register.EIP)
			{
				if (!registerValueProvider.TryGetRegisterValue(memoryBase, 0, 0, out value))
				{
					break;
				}
				num += value;
			}
			Code code = Code;
			if (memoryIndex != 0 && !code.IgnoresIndex() && !code.IsTileStrideIndex())
			{
				if (TryGetVsib64(out var vsib))
				{
					bool flag;
					if (vsib)
					{
						flag = registerValueProvider.TryGetRegisterValue(memoryIndex, elementIndex, 8, out value);
					}
					else
					{
						flag = registerValueProvider.TryGetRegisterValue(memoryIndex, elementIndex, 4, out value);
						value = (ulong)(int)value;
					}
					if (!flag)
					{
						break;
					}
					num += value << InternalMemoryIndexScale;
				}
				else
				{
					if (!registerValueProvider.TryGetRegisterValue(memoryIndex, 0, 0, out value))
					{
						break;
					}
					num += value << InternalMemoryIndexScale;
				}
			}
			num &= num2;
			if (!code.IgnoresSegment())
			{
				if (!registerValueProvider.TryGetRegisterValue(MemorySegment, 0, 0, out value2))
				{
					break;
				}
				num += value2;
			}
			result = num;
			return true;
		}
		default:
			throw new InvalidOperationException();
		}
		result = 0uL;
		return false;
	}
}
