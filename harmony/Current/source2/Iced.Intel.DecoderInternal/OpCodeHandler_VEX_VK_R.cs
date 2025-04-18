namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VEX_VK_R : OpCodeHandlerModRM
{
	private readonly Code code;

	private readonly Register gpr;

	public OpCodeHandler_VEX_VK_R(Code code, Register gpr)
	{
		this.code = code;
		this.gpr = gpr;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if (((decoder.state.vvvv_invalidCheck | decoder.state.zs.extraRegisterBase) & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = (Register)(decoder.state.reg + 173);
		if (decoder.state.mod == 3)
		{
			instruction.Op1Register = (Register)((int)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase) + (int)gpr);
		}
		else
		{
			decoder.SetInvalidInstruction();
		}
	}
}
