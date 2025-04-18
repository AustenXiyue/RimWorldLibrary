namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Ep : OpCodeHandlerModRM
{
	private readonly Code code16;

	private readonly Code code32;

	private readonly Code code64;

	public OpCodeHandler_Ep(Code code16, Code code32, Code code64)
	{
		this.code16 = code16;
		this.code32 = code32;
		this.code64 = code64;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if (decoder.state.operandSize == OpSize.Size64 && (decoder.options & DecoderOptions.AMD) == 0)
		{
			instruction.InternalSetCodeNoCheck(code64);
		}
		else if (decoder.state.operandSize == OpSize.Size16)
		{
			instruction.InternalSetCodeNoCheck(code16);
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
		decoder.ReadOpMem(ref instruction);
	}
}
