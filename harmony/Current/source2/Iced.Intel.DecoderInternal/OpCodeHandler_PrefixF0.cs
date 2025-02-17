namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_PrefixF0 : OpCodeHandler
{
	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetHasLockPrefix();
		decoder.state.zs.flags |= StateFlags.Lock;
		decoder.ResetRexPrefixState();
		decoder.CallOpCodeHandlerXXTable(ref instruction);
	}
}
