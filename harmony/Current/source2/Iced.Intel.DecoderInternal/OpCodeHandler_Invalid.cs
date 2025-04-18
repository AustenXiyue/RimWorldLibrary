namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Invalid : OpCodeHandlerModRM
{
	public static readonly OpCodeHandler_Invalid Instance = new OpCodeHandler_Invalid();

	private OpCodeHandler_Invalid()
	{
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		decoder.SetInvalidInstruction();
	}
}
