namespace Iced.Intel.EncoderInternal;

internal sealed class OpRegSTi : Op
{
	public override void Encode(Encoder encoder, in Instruction instruction, int operand)
	{
		if (encoder.Verify(operand, OpKind.Register, instruction.GetOpKind(operand)))
		{
			Register opRegister = instruction.GetOpRegister(operand);
			if (encoder.Verify(operand, opRegister, Register.ST0, Register.ST7))
			{
				encoder.OpCode |= (uint)(opRegister - 217);
			}
		}
	}
}
