namespace Iced.Intel.DecoderInternal;

internal abstract class OpCodeHandlerReader
{
	public abstract int ReadHandlers(ref TableDeserializer deserializer, OpCodeHandler?[] result, int resultIndex);
}
