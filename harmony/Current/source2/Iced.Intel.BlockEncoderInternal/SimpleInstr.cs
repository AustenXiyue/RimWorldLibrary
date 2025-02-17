namespace Iced.Intel.BlockEncoderInternal;

internal sealed class SimpleInstr : Instr
{
	private Instruction instruction;

	public SimpleInstr(BlockEncoder blockEncoder, Block block, in Instruction instruction)
		: base(block, instruction.IP)
	{
		Done = true;
		this.instruction = instruction;
		Size = blockEncoder.GetInstructionSize(in instruction, instruction.IP);
	}

	public override void Initialize(BlockEncoder blockEncoder)
	{
	}

	public override bool Optimize(ulong gained)
	{
		return false;
	}

	public override string? TryEncode(Encoder encoder, out ConstantOffsets constantOffsets, out bool isOriginalInstruction)
	{
		isOriginalInstruction = true;
		if (!encoder.TryEncode(in instruction, IP, out uint _, out string errorMessage))
		{
			constantOffsets = default(ConstantOffsets);
			return Instr.CreateErrorMessage(errorMessage, in instruction);
		}
		constantOffsets = encoder.GetConstantOffsets();
		return null;
	}
}
