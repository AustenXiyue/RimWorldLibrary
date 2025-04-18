namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Gv_N : OpCodeHandlerModRM
{
	private readonly Code code32;

	private readonly Code code64;

	public OpCodeHandler_Gv_N(Code code32, Code code64)
	{
		this.code32 = code32;
		this.code64 = code64;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if ((decoder.state.zs.flags & StateFlags.W) != 0)
		{
			instruction.InternalSetCodeNoCheck(code64);
		}
		else
		{
			instruction.InternalSetCodeNoCheck(code32);
		}
		if ((decoder.state.zs.flags & StateFlags.W) != 0)
		{
			instruction.Op0Register = (Register)(decoder.state.reg + decoder.state.zs.extraRegisterBase + 53);
		}
		else
		{
			instruction.Op0Register = (Register)(decoder.state.reg + decoder.state.zs.extraRegisterBase + 37);
		}
		if (decoder.state.mod == 3)
		{
			instruction.Op1Register = (Register)(decoder.state.rm + 225);
		}
		else
		{
			decoder.SetInvalidInstruction();
		}
	}
}
