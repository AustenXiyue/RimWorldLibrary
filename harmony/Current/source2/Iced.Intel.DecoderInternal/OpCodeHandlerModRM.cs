namespace Iced.Intel.DecoderInternal;

internal abstract class OpCodeHandlerModRM : OpCodeHandler
{
	protected OpCodeHandlerModRM()
		: base(hasModRM: true)
	{
	}
}
