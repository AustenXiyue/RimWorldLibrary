namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VEX_Gv_RX : OpCodeHandlerModRM
{
	private readonly Register baseReg;

	private readonly Code code32;

	private readonly Code code64;

	public OpCodeHandler_VEX_Gv_RX(Register baseReg, Code code32, Code code64)
	{
		this.baseReg = baseReg;
		this.code32 = code32;
		this.code64 = code64;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if ((decoder.state.vvvv_invalidCheck & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		Register register;
		if (((uint)decoder.state.zs.flags & decoder.is64bMode_and_W) != 0)
		{
			instruction.InternalSetCodeNoCheck(code64);
			register = Register.RAX;
		}
		else
		{
			instruction.InternalSetCodeNoCheck(code32);
			register = Register.EAX;
		}
		instruction.Op0Register = (Register)((int)(decoder.state.reg + decoder.state.zs.extraRegisterBase) + (int)register);
		if (decoder.state.mod == 3)
		{
			instruction.Op1Register = (Register)((int)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase) + (int)baseReg);
		}
		else
		{
			decoder.SetInvalidInstruction();
		}
	}
}
