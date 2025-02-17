namespace Iced.Intel.EncoderInternal;

internal sealed class OpHx : Op
{
	private readonly Register regLo;

	private readonly Register regHi;

	public OpHx(Register regLo, Register regHi)
	{
		this.regLo = regLo;
		this.regHi = regHi;
	}

	public override void Encode(Encoder encoder, in Instruction instruction, int operand)
	{
		if (encoder.Verify(operand, OpKind.Register, instruction.GetOpKind(operand)))
		{
			Register opRegister = instruction.GetOpRegister(operand);
			if (encoder.Verify(operand, opRegister, regLo, regHi))
			{
				encoder.EncoderFlags |= (EncoderFlags)(opRegister - regLo << 27);
			}
		}
	}
}
