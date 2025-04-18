namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Gv_W : OpCodeHandlerModRM
{
	private readonly Code codeW0;

	private readonly Code codeW1;

	public OpCodeHandler_Gv_W(Code codeW0, Code codeW1)
	{
		this.codeW0 = codeW0;
		this.codeW1 = codeW1;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if ((decoder.state.zs.flags & StateFlags.W) != 0)
		{
			instruction.InternalSetCodeNoCheck(codeW1);
			instruction.Op0Register = (Register)(decoder.state.reg + decoder.state.zs.extraRegisterBase + 53);
		}
		else
		{
			instruction.InternalSetCodeNoCheck(codeW0);
			instruction.Op0Register = (Register)(decoder.state.reg + decoder.state.zs.extraRegisterBase + 37);
		}
		if (decoder.state.mod == 3)
		{
			instruction.Op1Register = (Register)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase + 77);
			return;
		}
		instruction.Op1Kind = OpKind.Memory;
		decoder.ReadOpMem(ref instruction);
	}
}
