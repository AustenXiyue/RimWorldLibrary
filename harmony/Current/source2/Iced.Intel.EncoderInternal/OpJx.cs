namespace Iced.Intel.EncoderInternal;

internal sealed class OpJx : Op
{
	private readonly int immSize;

	public OpJx(int immSize)
	{
		this.immSize = immSize;
	}

	public override void Encode(Encoder encoder, in Instruction instruction, int operand)
	{
		encoder.AddBranchX(immSize, in instruction, operand);
	}

	public override OpKind GetNearBranchOpKind()
	{
		return base.GetNearBranchOpKind();
	}
}
