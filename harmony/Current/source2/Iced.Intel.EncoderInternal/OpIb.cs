namespace Iced.Intel.EncoderInternal;

internal sealed class OpIb : Op
{
	private readonly OpKind opKind;

	public OpIb(OpKind opKind)
	{
		this.opKind = opKind;
	}

	public override void Encode(Encoder encoder, in Instruction instruction, int operand)
	{
		switch (encoder.ImmSize)
		{
		case ImmSize.Size1:
			if (encoder.Verify(operand, OpKind.Immediate8_2nd, instruction.GetOpKind(operand)))
			{
				encoder.ImmSize = ImmSize.Size1_1;
				encoder.ImmediateHi = instruction.Immediate8_2nd;
			}
			return;
		case ImmSize.Size2:
			if (encoder.Verify(operand, OpKind.Immediate8_2nd, instruction.GetOpKind(operand)))
			{
				encoder.ImmSize = ImmSize.Size2_1;
				encoder.ImmediateHi = instruction.Immediate8_2nd;
			}
			return;
		}
		OpKind actual = instruction.GetOpKind(operand);
		if (encoder.Verify(operand, opKind, actual))
		{
			encoder.ImmSize = ImmSize.Size1;
			encoder.Immediate = instruction.Immediate8;
		}
	}

	public override OpKind GetImmediateOpKind()
	{
		return opKind;
	}
}
