namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_P_Ev_Ib : OpCodeHandlerModRM
{
	private readonly Code code32;

	private readonly Code code64;

	public OpCodeHandler_P_Ev_Ib(Code code32, Code code64)
	{
		this.code32 = code32;
		this.code64 = code64;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		Register register;
		if ((decoder.state.zs.flags & StateFlags.W) != 0)
		{
			instruction.InternalSetCodeNoCheck(code64);
			register = Register.RAX;
		}
		else
		{
			instruction.InternalSetCodeNoCheck(code32);
			register = Register.EAX;
		}
		instruction.Op0Register = (Register)(decoder.state.reg + 225);
		if (decoder.state.mod == 3)
		{
			instruction.Op1Register = (Register)((int)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase) + (int)register);
		}
		else
		{
			instruction.Op1Kind = OpKind.Memory;
			decoder.ReadOpMem(ref instruction);
		}
		instruction.Op2Kind = OpKind.Immediate8;
		instruction.InternalImmediate8 = decoder.ReadByte();
	}
}
