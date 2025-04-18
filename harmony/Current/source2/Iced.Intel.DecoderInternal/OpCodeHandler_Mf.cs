namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Mf : OpCodeHandlerModRM
{
	private readonly Code code16;

	private readonly Code code32;

	public OpCodeHandler_Mf(Code code)
	{
		code16 = code;
		code32 = code;
	}

	public OpCodeHandler_Mf(Code code16, Code code32)
	{
		this.code16 = code16;
		this.code32 = code32;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if (decoder.state.operandSize != 0)
		{
			instruction.InternalSetCodeNoCheck(code32);
		}
		else
		{
			instruction.InternalSetCodeNoCheck(code16);
		}
		instruction.Op0Kind = OpKind.Memory;
		decoder.ReadOpMem(ref instruction);
	}
}
