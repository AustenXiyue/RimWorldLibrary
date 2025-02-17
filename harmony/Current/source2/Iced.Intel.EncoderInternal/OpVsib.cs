namespace Iced.Intel.EncoderInternal;

internal sealed class OpVsib : Op
{
	private readonly Register vsibIndexRegLo;

	private readonly Register vsibIndexRegHi;

	public OpVsib(Register regLo, Register regHi)
	{
		vsibIndexRegLo = regLo;
		vsibIndexRegHi = regHi;
	}

	public override void Encode(Encoder encoder, in Instruction instruction, int operand)
	{
		encoder.EncoderFlags |= EncoderFlags.MustUseSib;
		encoder.AddRegOrMem(in instruction, operand, Register.None, Register.None, vsibIndexRegLo, vsibIndexRegHi, allowMemOp: true, allowRegOp: false);
	}
}
