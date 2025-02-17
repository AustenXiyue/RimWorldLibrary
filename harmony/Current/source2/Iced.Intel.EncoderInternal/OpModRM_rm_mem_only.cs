namespace Iced.Intel.EncoderInternal;

internal sealed class OpModRM_rm_mem_only : Op
{
	private readonly bool mustUseSib;

	public OpModRM_rm_mem_only(bool mustUseSib)
	{
		this.mustUseSib = mustUseSib;
	}

	public override void Encode(Encoder encoder, in Instruction instruction, int operand)
	{
		if (mustUseSib)
		{
			encoder.EncoderFlags |= EncoderFlags.MustUseSib;
		}
		encoder.AddRegOrMem(in instruction, operand, Register.None, Register.None, allowMemOp: true, allowRegOp: false);
	}
}
