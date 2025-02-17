namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_PrefixF3 : OpCodeHandler
{
	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetHasRepePrefix();
		decoder.state.zs.mandatoryPrefix = MandatoryPrefixByte.PF3;
		decoder.ResetRexPrefixState();
		decoder.CallOpCodeHandlerXXTable(ref instruction);
	}
}
