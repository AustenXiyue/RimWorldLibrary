namespace Iced.Intel.EncoderInternal;

internal sealed class OpO : Op
{
	public override void Encode(Encoder encoder, in Instruction instruction, int operand)
	{
		encoder.AddAbsMem(in instruction, operand);
	}
}
