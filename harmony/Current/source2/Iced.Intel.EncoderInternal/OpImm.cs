namespace Iced.Intel.EncoderInternal;

internal sealed class OpImm : Op
{
	private readonly byte value;

	public OpImm(byte value)
	{
		this.value = value;
	}

	public override void Encode(Encoder encoder, in Instruction instruction, int operand)
	{
		if (encoder.Verify(operand, OpKind.Immediate8, instruction.GetOpKind(operand)) && instruction.Immediate8 != value)
		{
			encoder.ErrorMessage = $"Operand {operand}: Expected 0x{value:X2}, actual: 0x{instruction.Immediate8:X2}";
		}
	}

	public override OpKind GetImmediateOpKind()
	{
		return OpKind.Immediate8;
	}
}
