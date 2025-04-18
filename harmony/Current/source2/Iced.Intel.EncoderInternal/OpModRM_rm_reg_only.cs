namespace Iced.Intel.EncoderInternal;

internal sealed class OpModRM_rm_reg_only : Op
{
	private readonly Register regLo;

	private readonly Register regHi;

	public OpModRM_rm_reg_only(Register regLo, Register regHi)
	{
		this.regLo = regLo;
		this.regHi = regHi;
	}

	public override void Encode(Encoder encoder, in Instruction instruction, int operand)
	{
		encoder.AddRegOrMem(in instruction, operand, regLo, regHi, allowMemOp: false, allowRegOp: true);
	}
}
