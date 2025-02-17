namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Simple : OpCodeHandler
{
	private readonly Code code;

	public OpCodeHandler_Simple(Code code)
	{
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
	}
}
