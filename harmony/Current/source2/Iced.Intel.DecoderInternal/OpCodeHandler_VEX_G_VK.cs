namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VEX_G_VK : OpCodeHandlerModRM
{
	private readonly Code code;

	private readonly Register gpr;

	public OpCodeHandler_VEX_G_VK(Code code, Register gpr)
	{
		this.code = code;
		this.gpr = gpr;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if ((decoder.state.vvvv_invalidCheck & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = (Register)((int)(decoder.state.reg + decoder.state.zs.extraRegisterBase) + (int)gpr);
		if (decoder.state.mod == 3)
		{
			instruction.Op1Register = (Register)(decoder.state.rm + 173);
		}
		else
		{
			decoder.SetInvalidInstruction();
		}
	}
}
