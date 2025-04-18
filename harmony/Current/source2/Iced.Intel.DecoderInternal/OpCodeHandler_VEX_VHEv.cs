namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VEX_VHEv : OpCodeHandlerModRM
{
	private readonly Register baseReg;

	private readonly Code codeW0;

	private readonly Code codeW1;

	public OpCodeHandler_VEX_VHEv(Register baseReg, Code codeW0, Code codeW1)
	{
		this.baseReg = baseReg;
		this.codeW0 = codeW0;
		this.codeW1 = codeW1;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		Register register;
		if (((uint)decoder.state.zs.flags & decoder.is64bMode_and_W) != 0)
		{
			instruction.InternalSetCodeNoCheck(codeW1);
			register = Register.RAX;
		}
		else
		{
			instruction.InternalSetCodeNoCheck(codeW0);
			register = Register.EAX;
		}
		instruction.Op0Register = (Register)((int)(decoder.state.reg + decoder.state.zs.extraRegisterBase) + (int)baseReg);
		instruction.Op1Register = (Register)((int)decoder.state.vvvv + (int)baseReg);
		if (decoder.state.mod == 3)
		{
			instruction.Op2Register = (Register)((int)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase) + (int)register);
			return;
		}
		instruction.Op2Kind = OpKind.Memory;
		decoder.ReadOpMem(ref instruction);
	}
}
