namespace Iced.Intel.EncoderInternal;

internal sealed class OpId : Op
{
	private readonly OpKind opKind;

	public OpId(OpKind opKind)
	{
		this.opKind = opKind;
	}

	public override void Encode(Encoder encoder, in Instruction instruction, int operand)
	{
		OpKind actual = instruction.GetOpKind(operand);
		if (encoder.Verify(operand, opKind, actual))
		{
			encoder.ImmSize = ImmSize.Size4;
			encoder.Immediate = instruction.Immediate32;
		}
	}

	public override OpKind GetImmediateOpKind()
	{
		return opKind;
	}
}
