namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Gv_Eb_REX : OpCodeHandlerModRM
{
	private readonly Code code32;

	private readonly Code code64;

	public OpCodeHandler_Gv_Eb_REX(Code code32, Code code64)
	{
		this.code32 = code32;
		this.code64 = code64;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if ((decoder.state.zs.flags & StateFlags.W) != 0)
		{
			instruction.InternalSetCodeNoCheck(code64);
			instruction.Op0Register = (Register)(decoder.state.reg + decoder.state.zs.extraRegisterBase + 53);
		}
		else
		{
			instruction.InternalSetCodeNoCheck(code32);
			instruction.Op0Register = (Register)(decoder.state.reg + decoder.state.zs.extraRegisterBase + 37);
		}
		if (decoder.state.mod == 3)
		{
			uint num = decoder.state.rm + decoder.state.zs.extraBaseRegisterBase;
			if ((decoder.state.zs.flags & StateFlags.HasRex) != 0 && num >= 4)
			{
				num += 4;
			}
			instruction.Op1Register = (Register)(num + 1);
		}
		else
		{
			instruction.Op1Kind = OpKind.Memory;
			decoder.ReadOpMem(ref instruction);
		}
	}
}
