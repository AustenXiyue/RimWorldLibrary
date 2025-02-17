namespace Iced.Intel.EncoderInternal;

internal sealed class OpRegEmbed8 : Op
{
	private readonly Register regLo;

	private readonly Register regHi;

	public OpRegEmbed8(Register regLo, Register regHi)
	{
		this.regLo = regLo;
		this.regHi = regHi;
	}

	public override void Encode(Encoder encoder, in Instruction instruction, int operand)
	{
		encoder.AddReg(in instruction, operand, regLo, regHi);
	}
}
