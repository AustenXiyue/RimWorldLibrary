namespace Iced.Intel.EncoderInternal;

internal sealed class OpModRM_regF0 : Op
{
	private readonly Register regLo;

	private readonly Register regHi;

	public OpModRM_regF0(Register regLo, Register regHi)
	{
		this.regLo = regLo;
		this.regHi = regHi;
	}

	public override void Encode(Encoder encoder, in Instruction instruction, int operand)
	{
		if (encoder.Bitness != 64 && instruction.GetOpKind(operand) == OpKind.Register && instruction.GetOpRegister(operand) >= regLo + 8 && instruction.GetOpRegister(operand) <= regLo + 15)
		{
			encoder.EncoderFlags |= EncoderFlags.PF0;
			encoder.AddModRMRegister(in instruction, operand, regLo + 8, regLo + 15);
		}
		else
		{
			encoder.AddModRMRegister(in instruction, operand, regLo, regHi);
		}
	}
}
