namespace Iced.Intel.EncoderInternal;

internal sealed class OpY : Op
{
	public override void Encode(Encoder encoder, in Instruction instruction, int operand)
	{
		int yRegSize = OpX.GetYRegSize(instruction.GetOpKind(operand));
		if (yRegSize == 0)
		{
			encoder.ErrorMessage = $"Operand {operand}: expected OpKind = {"MemoryESDI"}, {"MemoryESEDI"} or {"MemoryESRDI"}";
			return;
		}
		Code code = instruction.Code;
		if ((uint)(code - 335) <= 3u)
		{
			int xRegSize = OpX.GetXRegSize(instruction.Op0Kind);
			if (xRegSize != yRegSize)
			{
				encoder.ErrorMessage = $"Same sized register must be used: reg #1 size = {xRegSize * 8}, reg #2 size = {yRegSize * 8}";
				return;
			}
		}
		encoder.SetAddrSize(yRegSize);
	}
}
