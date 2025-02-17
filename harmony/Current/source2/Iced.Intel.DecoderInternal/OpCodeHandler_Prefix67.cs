namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Prefix67 : OpCodeHandler
{
	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		decoder.state.addressSize = decoder.defaultInvertedAddressSize;
		decoder.ResetRexPrefixState();
		decoder.CallOpCodeHandlerXXTable(ref instruction);
	}
}
