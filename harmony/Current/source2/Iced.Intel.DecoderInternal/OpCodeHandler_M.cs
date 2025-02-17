namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_M : OpCodeHandlerModRM
{
	private readonly Code codeW0;

	private readonly Code codeW1;

	public OpCodeHandler_M(Code codeW0, Code codeW1)
	{
		this.codeW0 = codeW0;
		this.codeW1 = codeW1;
	}

	public OpCodeHandler_M(Code codeW0)
	{
		this.codeW0 = codeW0;
		codeW1 = codeW0;
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
		if (decoder.state.mod == 3)
		{
			decoder.SetInvalidInstruction();
			return;
		}
		instruction.Op0Kind = OpKind.Memory;
		decoder.ReadOpMem(ref instruction);
	}
}
