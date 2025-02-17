namespace Iced.Intel.EncoderInternal;

internal sealed class OpModRM_rm : Op
{
	private readonly Register regLo;

	private readonly Register regHi;

	public OpModRM_rm(Register regLo, Register regHi)
	{
		this.regLo = regLo;
		this.regHi = regHi;
	}

	public override void Encode(Encoder encoder, in Instruction instruction, int operand)
	{
		encoder.AddRegOrMem(in instruction, operand, regLo, regHi, allowMemOp: true, allowRegOp: true);
	}
}
