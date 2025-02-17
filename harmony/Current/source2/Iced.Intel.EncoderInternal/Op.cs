namespace Iced.Intel.EncoderInternal;

internal abstract class Op
{
	public abstract void Encode(Encoder encoder, in Instruction instruction, int operand);

	public virtual OpKind GetImmediateOpKind()
	{
		return (OpKind)(-1);
	}

	public virtual OpKind GetNearBranchOpKind()
	{
		return (OpKind)(-1);
	}

	public virtual OpKind GetFarBranchOpKind()
	{
		return (OpKind)(-1);
	}
}
