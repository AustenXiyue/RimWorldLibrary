namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Gv_Ev_Ib_REX : OpCodeHandlerModRM
{
	private readonly Code code32;

	private readonly Code code64;

	public OpCodeHandler_Gv_Ev_Ib_REX(Code code32, Code code64)
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
		instruction.Op1Register = (Register)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase + 77);
		instruction.Op2Kind = OpKind.Immediate8;
		instruction.InternalImmediate8 = decoder.ReadByte();
	}
}
