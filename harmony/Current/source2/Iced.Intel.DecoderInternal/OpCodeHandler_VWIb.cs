namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VWIb : OpCodeHandlerModRM
{
	private readonly Code codeW0;

	private readonly Code codeW1;

	public OpCodeHandler_VWIb(Code code)
	{
		codeW0 = code;
		codeW1 = code;
	}

	public OpCodeHandler_VWIb(Code codeW0, Code codeW1)
	{
		this.codeW0 = codeW0;
		this.codeW1 = codeW1;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if ((decoder.state.zs.flags & StateFlags.W) != 0)
		{
			instruction.InternalSetCodeNoCheck(codeW1);
		}
		else
		{
			instruction.InternalSetCodeNoCheck(codeW0);
		}
		instruction.Op0Register = (Register)(decoder.state.reg + decoder.state.zs.extraRegisterBase + 77);
		if (decoder.state.mod == 3)
		{
			instruction.Op1Register = (Register)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase + 77);
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
