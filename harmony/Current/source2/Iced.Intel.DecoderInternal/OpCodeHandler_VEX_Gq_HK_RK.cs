namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VEX_Gq_HK_RK : OpCodeHandlerModRM
{
	private readonly Code code;

	public OpCodeHandler_VEX_Gq_HK_RK(Code code)
	{
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if (decoder.invalidCheckMask != 0 && decoder.state.vvvv > 7)
		{
			decoder.SetInvalidInstruction();
		}
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = (Register)(decoder.state.reg + decoder.state.zs.extraRegisterBase + 53);
		instruction.Op1Register = (Register)((decoder.state.vvvv & 7) + 173);
		if (decoder.state.mod == 3)
		{
			instruction.Op2Register = (Register)(decoder.state.rm + 173);
		}
		else
		{
			decoder.SetInvalidInstruction();
		}
	}
}
