namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Sw_M : OpCodeHandlerModRM
{
	private readonly Code code;

	public OpCodeHandler_Sw_M(Code code)
	{
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = decoder.ReadOpSegReg();
		if (decoder.state.mod == 3)
		{
			decoder.SetInvalidInstruction();
			return;
		}
		instruction.Op1Kind = OpKind.Memory;
		decoder.ReadOpMem(ref instruction);
	}
}
