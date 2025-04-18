namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_RvMw_Gw : OpCodeHandlerModRM
{
	private readonly Code code16;

	private readonly Code code32;

	public OpCodeHandler_RvMw_Gw(Code code16, Code code32)
	{
		this.code16 = code16;
		this.code32 = code32;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		Register register;
		if (decoder.state.operandSize != 0)
		{
			instruction.InternalSetCodeNoCheck(code32);
			instruction.Op1Register = (Register)(decoder.state.reg + decoder.state.zs.extraRegisterBase + 37);
			register = Register.EAX;
		}
		else
		{
			instruction.InternalSetCodeNoCheck(code16);
			instruction.Op1Register = (Register)(decoder.state.reg + decoder.state.zs.extraRegisterBase + 21);
			register = Register.AX;
		}
		if (decoder.state.mod == 3)
		{
			instruction.Op0Register = (Register)((int)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase) + (int)register);
			return;
		}
		instruction.Op0Kind = OpKind.Memory;
		decoder.ReadOpMem(ref instruction);
	}
}
