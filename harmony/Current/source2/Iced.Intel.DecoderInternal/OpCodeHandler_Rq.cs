namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Rq : OpCodeHandlerModRM
{
	private readonly Code code;

	public OpCodeHandler_Rq(Code code)
	{
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = (Register)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase + 53);
	}
}
