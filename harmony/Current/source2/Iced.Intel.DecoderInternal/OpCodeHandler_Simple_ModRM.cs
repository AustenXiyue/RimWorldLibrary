namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Simple_ModRM : OpCodeHandlerModRM
{
	private readonly Code code;

	public OpCodeHandler_Simple_ModRM(Code code)
	{
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
	}
}
