namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Ib3 : OpCodeHandlerModRM
{
	private readonly Code code;

	public OpCodeHandler_Ib3(Code code)
	{
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Kind = OpKind.Immediate8;
		instruction.InternalImmediate8 = decoder.ReadByte();
	}
}
