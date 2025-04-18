using System;

namespace Iced.Intel.EncoderInternal;

internal sealed class DeclareDataHandler : OpCodeHandler
{
	private readonly int elemLength;

	private readonly int maxLength;

	public DeclareDataHandler(Code code)
		: base(EncFlags2.None, EncFlags3.Bit16or32 | EncFlags3.Bit64, isSpecialInstr: true, null, Array2.Empty<Op>())
	{
		elemLength = code switch
		{
			Code.DeclareByte => 1, 
			Code.DeclareWord => 2, 
			Code.DeclareDword => 4, 
			Code.DeclareQword => 8, 
			_ => throw new InvalidOperationException(), 
		};
		maxLength = 16 / elemLength;
	}

	public override void Encode(Encoder encoder, in Instruction instruction)
	{
		int declareDataCount = instruction.DeclareDataCount;
		if (declareDataCount < 1 || declareDataCount > maxLength)
		{
			encoder.ErrorMessage = $"Invalid db/dw/dd/dq data count. Count = {declareDataCount}, max count = {maxLength}";
		}
		else
		{
			int num = declareDataCount * elemLength;
			for (int i = 0; i < num; i++)
			{
				encoder.WriteByteInternal(instruction.GetDeclareByteValue(i));
			}
		}
	}
}
