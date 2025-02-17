namespace Iced.Intel.EncoderInternal;

internal sealed class OpIw : Op
{
	public override void Encode(Encoder encoder, in Instruction instruction, int operand)
	{
		if (encoder.Verify(operand, OpKind.Immediate16, instruction.GetOpKind(operand)))
		{
			encoder.ImmSize = ImmSize.Size2;
			encoder.Immediate = instruction.Immediate16;
		}
	}

	public override OpKind GetImmediateOpKind()
	{
		return OpKind.Immediate16;
	}
}
