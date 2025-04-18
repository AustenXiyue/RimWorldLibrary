namespace Iced.Intel.EncoderInternal;

internal sealed class OpI4 : Op
{
	public override void Encode(Encoder encoder, in Instruction instruction, int operand)
	{
		OpKind opKind = instruction.GetOpKind(operand);
		if (encoder.Verify(operand, OpKind.Immediate8, opKind))
		{
			if (instruction.Immediate8 > 15)
			{
				encoder.ErrorMessage = $"Operand {operand}: Immediate value must be 0-15, but value is 0x{instruction.Immediate8:X2}";
			}
			else
			{
				encoder.ImmSize = ImmSize.Size1;
				encoder.Immediate |= instruction.Immediate8;
			}
		}
	}

	public override OpKind GetImmediateOpKind()
	{
		return OpKind.Immediate8;
	}
}
