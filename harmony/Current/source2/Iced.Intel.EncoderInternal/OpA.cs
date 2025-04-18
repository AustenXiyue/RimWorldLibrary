namespace Iced.Intel.EncoderInternal;

internal sealed class OpA : Op
{
	private readonly int size;

	public OpA(int size)
	{
		this.size = size;
	}

	public override void Encode(Encoder encoder, in Instruction instruction, int operand)
	{
		encoder.AddFarBranch(in instruction, operand, size);
	}

	public override OpKind GetFarBranchOpKind()
	{
		if (size != 2)
		{
			return OpKind.FarBranch32;
		}
		return OpKind.FarBranch16;
	}
}
