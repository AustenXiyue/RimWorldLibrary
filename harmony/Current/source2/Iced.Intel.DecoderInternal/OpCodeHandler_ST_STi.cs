namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_ST_STi : OpCodeHandlerModRM
{
	private readonly Code code;

	public OpCodeHandler_ST_STi(Code code)
	{
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = Register.ST0;
		instruction.Op1Register = (Register)(217 + decoder.state.rm);
	}
}
