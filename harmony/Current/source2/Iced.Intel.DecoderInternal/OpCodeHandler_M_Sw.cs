namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_M_Sw : OpCodeHandlerModRM
{
	private readonly Code code;

	public OpCodeHandler_M_Sw(Code code)
	{
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op1Register = decoder.ReadOpSegReg();
		if (decoder.state.mod == 3)
		{
			decoder.SetInvalidInstruction();
			return;
		}
		instruction.Op0Kind = OpKind.Memory;
		decoder.ReadOpMem(ref instruction);
	}
}
