namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_PrefixF2 : OpCodeHandler
{
	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetHasRepnePrefix();
		decoder.state.zs.mandatoryPrefix = MandatoryPrefixByte.PF2;
		decoder.ResetRexPrefixState();
		decoder.CallOpCodeHandlerXXTable(ref instruction);
	}
}
