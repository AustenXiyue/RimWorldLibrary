namespace Iced.Intel.EncoderInternal;

internal sealed class OpModRM_reg : Op
{
	private readonly Register regLo;

	private readonly Register regHi;

	public OpModRM_reg(Register regLo, Register regHi)
	{
		this.regLo = regLo;
		this.regHi = regHi;
	}

	public override void Encode(Encoder encoder, in Instruction instruction, int operand)
	{
		encoder.AddModRMRegister(in instruction, operand, regLo, regHi);
	}
}
