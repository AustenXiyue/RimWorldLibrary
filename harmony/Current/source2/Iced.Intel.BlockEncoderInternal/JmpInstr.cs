using System;

namespace Iced.Intel.BlockEncoderInternal;

internal sealed class JmpInstr : Instr
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

	public JmpInstr(BlockEncoder blockEncoder, Block block, in Instruction instruction)
		: base(block, instruction.IP)
	{
		this.instruction = instruction;
		instrKind = InstrKind.Uninitialized;
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
			Size = Math.Max(nearInstructionSize, 6u);
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
		uint encodedLength;
		switch (instrKind)
		{
		case InstrKind.Unchanged:
		case InstrKind.Short:
		case InstrKind.Near:
		{
			isOriginalInstruction = true;
			if (instrKind != 0)
			{
				if (instrKind == InstrKind.Short)
				{
					instruction.InternalSetCodeNoCheck(instruction.Code.ToShortBranch());
				}
				else
				{
					instruction.InternalSetCodeNoCheck(instruction.Code.ToNearBranch());
				}
			}
			instruction.NearBranch64 = targetInstr.GetAddress();
			if (!encoder.TryEncode(in instruction, IP, out encodedLength, out string text))
			{
				constantOffsets = default(ConstantOffsets);
				return Instr.CreateErrorMessage(text, in instruction);
			}
			constantOffsets = encoder.GetConstantOffsets();
			return null;
		}
		case InstrKind.Long:
		{
			isOriginalInstruction = false;
			constantOffsets = default(ConstantOffsets);
			pointerData.Data = targetInstr.GetAddress();
			string text = EncodeBranchToPointerData(encoder, isCall: false, IP, pointerData, out encodedLength, Size);
			if (text != null)
			{
				return Instr.CreateErrorMessage(text, in instruction);
			}
			return null;
		}
		default:
			throw new InvalidOperationException();
		}
	}
}
