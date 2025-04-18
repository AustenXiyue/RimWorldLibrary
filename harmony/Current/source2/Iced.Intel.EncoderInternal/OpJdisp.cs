namespace Iced.Intel.EncoderInternal;

internal sealed class OpJdisp : Op
{
	private readonly int displSize;

	public OpJdisp(int displSize)
	{
		this.displSize = displSize;
	}

	public override void Encode(Encoder encoder, in Instruction instruction, int operand)
	{
		encoder.AddBranchDisp(displSize, in instruction, operand);
	}

	public override OpKind GetNearBranchOpKind()
	{
		if (displSize != 2)
		{
			return OpKind.NearBranch32;
		}
		return OpKind.NearBranch16;
	}
}
