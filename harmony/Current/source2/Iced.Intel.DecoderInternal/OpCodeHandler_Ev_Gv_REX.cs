namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Ev_Gv_REX : OpCodeHandlerModRM
{
	private readonly Code code32;

	private readonly Code code64;

	public OpCodeHandler_Ev_Gv_REX(Code code32, Code code64)
	{
		this.code32 = code32;
		this.code64 = code64;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if ((decoder.state.zs.flags & StateFlags.W) != 0)
		{
			instruction.InternalSetCodeNoCheck(code64);
			instruction.Op1Register = (Register)(decoder.state.reg + decoder.state.zs.extraRegisterBase + 53);
		}
		else
		{
			instruction.InternalSetCodeNoCheck(code32);
			instruction.Op1Register = (Register)(decoder.state.reg + decoder.state.zs.extraRegisterBase + 37);
		}
		instruction.Op0Kind = OpKind.Memory;
		decoder.ReadOpMem(ref instruction);
	}
}
