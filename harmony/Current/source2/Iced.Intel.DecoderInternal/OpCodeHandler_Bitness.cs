namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Bitness : OpCodeHandler
{
	private readonly OpCodeHandler handler1632;

	private readonly OpCodeHandler handler64;

	public OpCodeHandler_Bitness(OpCodeHandler handler1632, OpCodeHandler handler64)
	{
		this.handler1632 = handler1632;
		this.handler64 = handler64;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		OpCodeHandler opCodeHandler = ((!decoder.is64bMode) ? handler1632 : handler64);
		if (opCodeHandler.HasModRM)
		{
			decoder.ReadModRM();
		}
		opCodeHandler.Decode(decoder, ref instruction);
	}
}
