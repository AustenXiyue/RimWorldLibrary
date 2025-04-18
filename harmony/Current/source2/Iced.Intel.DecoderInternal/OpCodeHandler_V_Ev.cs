namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_V_Ev : OpCodeHandlerModRM
{
	private readonly Code codeW0;

	private readonly Code codeW1;

	public OpCodeHandler_V_Ev(Code codeW0, Code codeW1)
	{
		this.codeW0 = codeW0;
		this.codeW1 = codeW1;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		Register register;
		if (decoder.state.operandSize != OpSize.Size64)
		{
			instruction.InternalSetCodeNoCheck(codeW0);
			register = Register.EAX;
		}
		else
		{
			instruction.InternalSetCodeNoCheck(codeW1);
			register = Register.RAX;
		}
		instruction.Op0Register = (Register)(decoder.state.reg + decoder.state.zs.extraRegisterBase + 77);
		if (decoder.state.mod == 3)
		{
			instruction.Op1Register = (Register)((int)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase) + (int)register);
			return;
		}
		instruction.Op1Kind = OpKind.Memory;
		decoder.ReadOpMem(ref instruction);
	}
}
