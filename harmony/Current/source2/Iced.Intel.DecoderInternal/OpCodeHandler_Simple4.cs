namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Simple4 : OpCodeHandler
{
	private readonly Code code32;

	private readonly Code code64;

	public OpCodeHandler_Simple4(Code code32, Code code64)
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
	}
}
