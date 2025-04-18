namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Ev_Gv_32_64 : OpCodeHandlerModRM
{
	private readonly Code code32;

	private readonly Code code64;

	public OpCodeHandler_Ev_Gv_32_64(Code code32, Code code64)
	{
		this.code32 = code32;
		this.code64 = code64;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		Register register;
		if (decoder.is64bMode)
		{
			instruction.InternalSetCodeNoCheck(code64);
			register = Register.RAX;
		}
		else
		{
			instruction.InternalSetCodeNoCheck(code32);
			register = Register.EAX;
		}
		instruction.Op1Register = (Register)((int)(decoder.state.reg + decoder.state.zs.extraRegisterBase) + (int)register);
		if (decoder.state.mod == 3)
		{
			instruction.Op0Register = (Register)((int)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase) + (int)register);
			return;
		}
		instruction.Op0Kind = OpKind.Memory;
		decoder.ReadOpMem(ref instruction);
	}
}
