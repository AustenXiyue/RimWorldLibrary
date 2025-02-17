namespace Iced.Intel.DecoderInternal;

internal abstract class OpCodeHandler
{
	public readonly bool HasModRM;

	protected OpCodeHandler()
	{
	}

	protected OpCodeHandler(bool hasModRM)
	{
		HasModRM = hasModRM;
	}

	public abstract void Decode(Decoder decoder, ref Instruction instruction);
}
