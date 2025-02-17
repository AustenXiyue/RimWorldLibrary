namespace Iced.Intel.EncoderInternal;

internal sealed class OpX : Op
{
	internal static int GetXRegSize(OpKind opKind)
	{
		return opKind switch
		{
			OpKind.MemorySegRSI => 8, 
			OpKind.MemorySegESI => 4, 
			OpKind.MemorySegSI => 2, 
			_ => 0, 
		};
	}

	internal static int GetYRegSize(OpKind opKind)
	{
		return opKind switch
		{
			OpKind.MemoryESRDI => 8, 
			OpKind.MemoryESEDI => 4, 
			OpKind.MemoryESDI => 2, 
			_ => 0, 
		};
	}

	public override void Encode(Encoder encoder, in Instruction instruction, int operand)
	{
		int xRegSize = GetXRegSize(instruction.GetOpKind(operand));
		if (xRegSize == 0)
		{
			encoder.ErrorMessage = $"Operand {operand}: expected OpKind = {"MemorySegSI"}, {"MemorySegESI"} or {"MemorySegRSI"}";
			return;
		}
		Code code = instruction.Code;
		if ((uint)(code - 331) <= 3u)
		{
			int yRegSize = GetYRegSize(instruction.Op0Kind);
			if (xRegSize != yRegSize)
			{
				encoder.ErrorMessage = $"Same sized register must be used: reg #1 size = {yRegSize * 8}, reg #2 size = {xRegSize * 8}";
				return;
			}
		}
		encoder.SetAddrSize(xRegSize);
	}
}
