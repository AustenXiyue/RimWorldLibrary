namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VN : OpCodeHandlerModRM
{
	private readonly Code code;

	public OpCodeHandler_VN(Code code)
	{
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = (Register)(decoder.state.reg + decoder.state.zs.extraRegisterBase + 77);
		if (decoder.state.mod == 3)
		{
			instruction.Op1Register = (Register)(decoder.state.rm + 225);
		}
		else
		{
			decoder.SetInvalidInstruction();
		}
	}
}
