using System;

namespace Iced.Intel.BlockEncoderInternal;

internal sealed class XbeginInstr : Instr
{
	private enum InstrKind : byte
	{
		Unchanged,
		Rel16,
		Rel32,
		Uninitialized
	}

	private Instruction instruction;

	private TargetInstr targetInstr;

	private InstrKind instrKind;

	private readonly byte shortInstructionSize;

	private readonly byte nearInstructionSize;

	public XbeginInstr(BlockEncoder blockEncoder, Block block, in Instruction instruction)
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
		instruction2.InternalSetCodeNoCheck(Code.Xbegin_rel16);
		instruction2.NearBranch64 = 0uL;
		shortInstructionSize = (byte)blockEncoder.GetInstructionSize(in instruction2, 0uL);
		instruction2 = instruction;
		instruction2.InternalSetCodeNoCheck(Code.Xbegin_rel32);
		instruction2.NearBranch64 = 0uL;
		nearInstructionSize = (byte)blockEncoder.GetInstructionSize(in instruction2, 0uL);
		Size = nearInstructionSize;
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
		if (instrKind == InstrKind.Unchanged || instrKind == InstrKind.Rel16)
		{
			Done = true;
			return false;
		}
		ulong address = targetInstr.GetAddress();
		ulong num = IP + shortInstructionSize;
		long diff = (long)(address - num);
		diff = Instr.CorrectDiff(targetInstr.IsInBlock(Block), diff, gained);
		if (-32768 <= diff && diff <= 32767)
		{
			instrKind = InstrKind.Rel16;
			Size = shortInstructionSize;
			return true;
		}
		instrKind = InstrKind.Rel32;
		Size = nearInstructionSize;
		return false;
	}

	public override string? TryEncode(Encoder encoder, out ConstantOffsets constantOffsets, out bool isOriginalInstruction)
	{
		switch (instrKind)
		{
		case InstrKind.Unchanged:
		case InstrKind.Rel16:
		case InstrKind.Rel32:
		{
			isOriginalInstruction = true;
			if (instrKind != 0)
			{
				if (instrKind == InstrKind.Rel16)
				{
					instruction.InternalSetCodeNoCheck(Code.Xbegin_rel16);
				}
				else
				{
					instruction.InternalSetCodeNoCheck(Code.Xbegin_rel32);
				}
			}
			instruction.NearBranch64 = targetInstr.GetAddress();
			if (!encoder.TryEncode(in instruction, IP, out uint _, out string errorMessage))
			{
				constantOffsets = default(ConstantOffsets);
				return Instr.CreateErrorMessage(errorMessage, in instruction);
			}
			constantOffsets = encoder.GetConstantOffsets();
			return null;
		}
		default:
			throw new InvalidOperationException();
		}
	}
}
