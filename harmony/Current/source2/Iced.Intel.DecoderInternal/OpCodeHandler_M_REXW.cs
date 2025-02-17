namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_M_REXW : OpCodeHandlerModRM
{
	private readonly Code code32;

	private readonly Code code64;

	private readonly HandlerFlags flags32;

	private readonly HandlerFlags flags64;

	public OpCodeHandler_M_REXW(Code code32, Code code64)
	{
		this.code32 = code32;
		this.code64 = code64;
	}

	public OpCodeHandler_M_REXW(Code code32, Code code64, HandlerFlags flags32, HandlerFlags flags64)
	{
		this.code32 = code32;
		this.code64 = code64;
		this.flags32 = flags32;
		this.flags64 = flags64;
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
		if (decoder.state.mod == 3)
		{
			decoder.SetInvalidInstruction();
			return;
		}
		instruction.Op0Kind = OpKind.Memory;
		HandlerFlags handlerFlags = (((decoder.state.zs.flags & StateFlags.W) != 0) ? flags64 : flags32);
		if ((handlerFlags & (HandlerFlags.Xacquire | HandlerFlags.Xrelease)) != 0)
		{
			decoder.SetXacquireXrelease(ref instruction);
		}
		decoder.state.zs.flags |= (StateFlags)((uint)(handlerFlags & HandlerFlags.Lock) << 10);
		decoder.ReadOpMem(ref instruction);
	}
}
