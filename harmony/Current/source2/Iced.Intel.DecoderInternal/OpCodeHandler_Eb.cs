namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Eb : OpCodeHandlerModRM
{
	private readonly Code code;

	private readonly HandlerFlags flags;

	public OpCodeHandler_Eb(Code code)
	{
		this.code = code;
	}

	public OpCodeHandler_Eb(Code code, HandlerFlags flags)
	{
		this.code = code;
		this.flags = flags;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		if (decoder.state.mod == 3)
		{
			uint num = decoder.state.rm + decoder.state.zs.extraBaseRegisterBase;
			if ((decoder.state.zs.flags & StateFlags.HasRex) != 0 && num >= 4)
			{
				num += 4;
			}
			instruction.Op0Register = (Register)(num + 1);
		}
		else
		{
			decoder.state.zs.flags |= (StateFlags)((uint)(flags & HandlerFlags.Lock) << 10);
			instruction.Op0Kind = OpKind.Memory;
			decoder.ReadOpMem(ref instruction);
		}
	}
}
