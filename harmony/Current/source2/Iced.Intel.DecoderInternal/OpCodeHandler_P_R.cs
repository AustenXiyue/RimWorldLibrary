namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_P_R : OpCodeHandlerModRM
{
	private readonly Code code;

	public OpCodeHandler_P_R(Code code)
	{
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = (Register)(decoder.state.reg + 225);
		if (decoder.state.mod == 3)
		{
			instruction.Op1Register = (Register)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase + 77);
		}
		else
		{
			decoder.SetInvalidInstruction();
		}
	}
}
