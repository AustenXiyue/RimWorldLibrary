namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Invalid_NoModRM : OpCodeHandler
{
	public static readonly OpCodeHandler_Invalid_NoModRM Instance = new OpCodeHandler_Invalid_NoModRM();

	private OpCodeHandler_Invalid_NoModRM()
	{
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		decoder.SetInvalidInstruction();
	}
}
