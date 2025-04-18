using System;

namespace Iced.Intel.BlockEncoderInternal;

internal sealed class JccInstr : Instr
{
	private enum InstrKind : byte
	{
		Unchanged,
		Short,
		Near,
		Long,
		Uninitialized
	}

	private Instruction instruction;

	private TargetInstr targetInstr;

	private BlockData pointerData;

	private InstrKind instrKind;

	private readonly byte shortInstructionSize;

	private readonly byte nearInstructionSize;

	private readonly byte longInstructionSize64;

	private static uint GetLongInstructionSize64(in Instruction instruction)
	{
		return 8u;
	}

	public JccInstr(BlockEncoder blockEncoder, Block block, in Instruction instruction)
		: base(block, instruction.IP)
	{
		this.instruction = instruction;
		instrKind = InstrKind.Uninitialized;
		longInstructionSize64 = (byte)GetLongInstructionSize64(in instruction);
		Instruction instruction2;
		if (!blockEncoder.FixBranches)
		{
			instrKind = InstrKind.Unchanged;
			instruction2 = instruction;
			instruction2.NearBranch64 = 0uL;
			Size = blockEncoder.GetInstructionSize(in instruction2, 0uL);
			return;
		}
		instruction2 = instruction;
		instruction2.InternalSetCodeNoCheck(instruction.Code.ToShortBranch());
		instruction2.NearBranch64 = 0uL;
		shortInstructionSize = (byte)blockEncoder.GetInstructionSize(in instruction2, 0uL);
		instruction2 = instruction;
		instruction2.InternalSetCodeNoCheck(instruction.Code.ToNearBranch());
		instruction2.NearBranch64 = 0uL;
		nearInstructionSize = (byte)blockEncoder.GetInstructionSize(in instruction2, 0uL);
		if (blockEncoder.Bitness == 64)
		{
			Size = Math.Max(nearInstructionSize, longInstructionSize64);
		}
		else
		{
			Size = nearInstructionSize;
		}
	}

	public override void Initialize(BlockEncoder blockEncoder)
	{
		targetInstr = blockEncoder.GetTarget(instruction.NearBranchTarget);
	}

	public override bool Optimize(ulong gained)
	{
		return TryOptimize(gained);
	}

	private bool TryOptimize(ulong gained)
	{
		if (instrKind == InstrKind.Unchanged || instrKind == InstrKind.Short)
		{
			Done = true;
			return false;
		}
		ulong address = targetInstr.GetAddress();
		ulong num = IP + shortInstructionSize;
		long diff = (long)(address - num);
		diff = Instr.CorrectDiff(targetInstr.IsInBlock(Block), diff, gained);
		if (-128 <= diff && diff <= 127)
		{
			if (pointerData != null)
			{
				pointerData.IsValid = false;
			}
			instrKind = InstrKind.Short;
			Size = shortInstructionSize;
			Done = true;
			return true;
		}
		ulong address2 = targetInstr.GetAddress();
		num = IP + nearInstructionSize;
		diff = (long)(address2 - num);
		diff = Instr.CorrectDiff(targetInstr.IsInBlock(Block), diff, gained);
		if (int.MinValue <= diff && diff <= int.MaxValue)
		{
			if (pointerData != null)
			{
				pointerData.IsValid = false;
			}
			if (diff < -1920 || diff > 1905)
			{
				Done = true;
			}
			instrKind = InstrKind.Near;
			Size = nearInstructionSize;
			return true;
		}
		if (pointerData == null)
		{
			pointerData = Block.AllocPointerLocation();
		}
		instrKind = InstrKind.Long;
		return false;
	}

	public override string? TryEncode(Encoder encoder, out ConstantOffsets constantOffsets, out bool isOriginalInstruction)
	{
		uint encodedLength2;
		string errorMessage;
		switch (instrKind)
		{
		case InstrKind.Unchanged:
		case InstrKind.Short:
		case InstrKind.Near:
			isOriginalInstruction = true;
			if (instrKind != 0)
			{
				if (instrKind == InstrKind.Short)
				{
					this.instruction.InternalSetCodeNoCheck(this.instruction.Code.ToShortBranch());
				}
				else
				{
					this.instruction.InternalSetCodeNoCheck(this.instruction.Code.ToNearBranch());
				}
			}
			this.instruction.NearBranch64 = targetInstr.GetAddress();
			if (!encoder.TryEncode(in this.instruction, IP, out encodedLength2, out errorMessage))
			{
				constantOffsets = default(ConstantOffsets);
				return Instr.CreateErrorMessage(errorMessage, in this.instruction);
			}
			constantOffsets = encoder.GetConstantOffsets();
			return null;
		case InstrKind.Long:
		{
			isOriginalInstruction = false;
			constantOffsets = default(ConstantOffsets);
			pointerData.Data = targetInstr.GetAddress();
			Instruction instruction = default(Instruction);
			instruction.InternalSetCodeNoCheck(ShortBrToNativeBr(this.instruction.Code.NegateConditionCode().ToShortBranch(), encoder.Bitness));
			if (this.instruction.OpCount == 1)
			{
				instruction.Op0Kind = OpKind.NearBranch64;
				instruction.NearBranch64 = IP + longInstructionSize64;
				if (!encoder.TryEncode(in instruction, IP, out uint encodedLength, out errorMessage))
				{
					return Instr.CreateErrorMessage(errorMessage, in this.instruction);
				}
				errorMessage = EncodeBranchToPointerData(encoder, isCall: false, IP + encodedLength, pointerData, out encodedLength2, Size - encodedLength);
				if (errorMessage != null)
				{
					return Instr.CreateErrorMessage(errorMessage, in this.instruction);
				}
				return null;
			}
			throw new InvalidOperationException();
		}
		default:
			throw new InvalidOperationException();
		}
	}

	private static Code ShortBrToNativeBr(Code code, int bitness)
	{
		Code code2;
		Code code3;
		Code code4;
		switch (code)
		{
		case Code.Jo_rel8_16:
		case Code.Jo_rel8_32:
		case Code.Jo_rel8_64:
			code2 = Code.Jo_rel8_16;
			code3 = Code.Jo_rel8_32;
			code4 = Code.Jo_rel8_64;
			break;
		case Code.Jno_rel8_16:
		case Code.Jno_rel8_32:
		case Code.Jno_rel8_64:
			code2 = Code.Jno_rel8_16;
			code3 = Code.Jno_rel8_32;
			code4 = Code.Jno_rel8_64;
			break;
		case Code.Jb_rel8_16:
		case Code.Jb_rel8_32:
		case Code.Jb_rel8_64:
			code2 = Code.Jb_rel8_16;
			code3 = Code.Jb_rel8_32;
			code4 = Code.Jb_rel8_64;
			break;
		case Code.Jae_rel8_16:
		case Code.Jae_rel8_32:
		case Code.Jae_rel8_64:
			code2 = Code.Jae_rel8_16;
			code3 = Code.Jae_rel8_32;
			code4 = Code.Jae_rel8_64;
			break;
		case Code.Je_rel8_16:
		case Code.Je_rel8_32:
		case Code.Je_rel8_64:
			code2 = Code.Je_rel8_16;
			code3 = Code.Je_rel8_32;
			code4 = Code.Je_rel8_64;
			break;
		case Code.Jne_rel8_16:
		case Code.Jne_rel8_32:
		case Code.Jne_rel8_64:
			code2 = Code.Jne_rel8_16;
			code3 = Code.Jne_rel8_32;
			code4 = Code.Jne_rel8_64;
			break;
		case Code.Jbe_rel8_16:
		case Code.Jbe_rel8_32:
		case Code.Jbe_rel8_64:
			code2 = Code.Jbe_rel8_16;
			code3 = Code.Jbe_rel8_32;
			code4 = Code.Jbe_rel8_64;
			break;
		case Code.Ja_rel8_16:
		case Code.Ja_rel8_32:
		case Code.Ja_rel8_64:
			code2 = Code.Ja_rel8_16;
			code3 = Code.Ja_rel8_32;
			code4 = Code.Ja_rel8_64;
			break;
		case Code.Js_rel8_16:
		case Code.Js_rel8_32:
		case Code.Js_rel8_64:
			code2 = Code.Js_rel8_16;
			code3 = Code.Js_rel8_32;
			code4 = Code.Js_rel8_64;
			break;
		case Code.Jns_rel8_16:
		case Code.Jns_rel8_32:
		case Code.Jns_rel8_64:
			code2 = Code.Jns_rel8_16;
			code3 = Code.Jns_rel8_32;
			code4 = Code.Jns_rel8_64;
			break;
		case Code.Jp_rel8_16:
		case Code.Jp_rel8_32:
		case Code.Jp_rel8_64:
			code2 = Code.Jp_rel8_16;
			code3 = Code.Jp_rel8_32;
			code4 = Code.Jp_rel8_64;
			break;
		case Code.Jnp_rel8_16:
		case Code.Jnp_rel8_32:
		case Code.Jnp_rel8_64:
			code2 = Code.Jnp_rel8_16;
			code3 = Code.Jnp_rel8_32;
			code4 = Code.Jnp_rel8_64;
			break;
		case Code.Jl_rel8_16:
		case Code.Jl_rel8_32:
		case Code.Jl_rel8_64:
			code2 = Code.Jl_rel8_16;
			code3 = Code.Jl_rel8_32;
			code4 = Code.Jl_rel8_64;
			break;
		case Code.Jge_rel8_16:
		case Code.Jge_rel8_32:
		case Code.Jge_rel8_64:
			code2 = Code.Jge_rel8_16;
			code3 = Code.Jge_rel8_32;
			code4 = Code.Jge_rel8_64;
			break;
		case Code.Jle_rel8_16:
		case Code.Jle_rel8_32:
		case Code.Jle_rel8_64:
			code2 = Code.Jle_rel8_16;
			code3 = Code.Jle_rel8_32;
			code4 = Code.Jle_rel8_64;
			break;
		case Code.Jg_rel8_16:
		case Code.Jg_rel8_32:
		case Code.Jg_rel8_64:
			code2 = Code.Jg_rel8_16;
			code3 = Code.Jg_rel8_32;
			code4 = Code.Jg_rel8_64;
			break;
		default:
			throw new ArgumentOutOfRangeException("code");
		}
		return bitness switch
		{
			16 => code2, 
			32 => code3, 
			64 => code4, 
			_ => throw new ArgumentOutOfRangeException("bitness"), 
		};
	}
}
