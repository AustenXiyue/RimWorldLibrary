namespace Iced.Intel.EncoderInternal;

internal sealed class OpJ : Op
{
	private readonly OpKind opKind;

	private readonly int immSize;

	public OpJ(OpKind opKind, int immSize)
	{
		this.opKind = opKind;
		this.immSize = immSize;
	}

	public override void Encode(Encoder encoder, in Instruction instruction, int operand)
	{
		encoder.AddBranch(opKind, immSize, in instruction, operand);
	}

	public override OpKind GetNearBranchOpKind()
	{
		return opKind;
	}
}
