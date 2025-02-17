namespace Iced.Intel.EncoderInternal;

internal sealed class OprDI : Op
{
	private static int GetRegSize(OpKind opKind)
	{
		return opKind switch
		{
			OpKind.MemorySegRDI => 8, 
			OpKind.MemorySegEDI => 4, 
			OpKind.MemorySegDI => 2, 
			_ => 0, 
		};
	}

	public override void Encode(Encoder encoder, in Instruction instruction, int operand)
	{
		int regSize = GetRegSize(instruction.GetOpKind(operand));
		if (regSize == 0)
		{
			encoder.ErrorMessage = $"Operand {operand}: expected OpKind = {"MemorySegDI"}, {"MemorySegEDI"} or {"MemorySegRDI"}";
		}
		else
		{
			encoder.SetAddrSize(regSize);
		}
	}
}
