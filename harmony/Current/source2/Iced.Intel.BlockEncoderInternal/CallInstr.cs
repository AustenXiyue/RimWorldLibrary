using System;

namespace Iced.Intel.BlockEncoderInternal;

internal sealed class CallInstr : Instr
{
	private readonly byte bitness;

	private Instruction instruction;

	private TargetInstr targetInstr;

	private readonly byte origInstructionSize;

	private BlockData pointerData;

	private bool useOrigInstruction;

	public CallInstr(BlockEncoder blockEncoder, Block block, in Instruction instruction)
		: base(block, instruction.IP)
	{
		bitness = (byte)blockEncoder.Bitness;
		this.instruction = instruction;
		Instruction instruction2 = instruction;
		instruction2.NearBranch64 = 0uL;
		origInstructionSize = (byte)blockEncoder.GetInstructionSize(in instruction2, 0uL);
		if (!blockEncoder.FixBranches)
		{
			Size = origInstructionSize;
			useOrigInstruction = true;
		}
		else if (blockEncoder.Bitness == 64)
		{
			Size = Math.Max(origInstructionSize, 6u);
		}
		else
		{
			Size = origInstructionSize;
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
		if (Done || useOrigInstruction)
		{
			Done = true;
			return false;
		}
		bool flag = bitness != 64 || targetInstr.IsInBlock(Block);
		if (!flag)
		{
			ulong address = targetInstr.GetAddress();
			ulong num = IP + origInstructionSize;
			long diff = (long)(address - num);
			diff = Instr.CorrectDiff(targetInstr.IsInBlock(Block), diff, gained);
			flag = int.MinValue <= diff && diff <= int.MaxValue;
		}
		if (flag)
		{
			if (pointerData != null)
			{
				pointerData.IsValid = false;
			}
			Size = origInstructionSize;
			useOrigInstruction = true;
			Done = true;
			return true;
		}
		if (pointerData == null)
		{
			pointerData = Block.AllocPointerLocation();
		}
		return false;
	}

	public override string? TryEncode(Encoder encoder, out ConstantOffsets constantOffsets, out bool isOriginalInstruction)
	{
		uint size;
		if (useOrigInstruction)
		{
			isOriginalInstruction = true;
			instruction.NearBranch64 = targetInstr.GetAddress();
			if (!encoder.TryEncode(in instruction, IP, out size, out string errorMessage))
			{
				constantOffsets = default(ConstantOffsets);
				return Instr.CreateErrorMessage(errorMessage, in instruction);
			}
			constantOffsets = encoder.GetConstantOffsets();
			return null;
		}
		isOriginalInstruction = false;
		constantOffsets = default(ConstantOffsets);
		pointerData.Data = targetInstr.GetAddress();
		string text = EncodeBranchToPointerData(encoder, isCall: true, IP, pointerData, out size, Size);
		if (text != null)
		{
			return Instr.CreateErrorMessage(text, in instruction);
		}
		return null;
	}
}
