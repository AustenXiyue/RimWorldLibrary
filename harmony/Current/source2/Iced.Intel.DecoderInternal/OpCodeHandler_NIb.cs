namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_NIb : OpCodeHandlerModRM
{
	private readonly Code code;

	public OpCodeHandler_NIb(Code code)
	{
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		if (decoder.state.mod == 3)
		{
			instruction.Op0Register = (Register)(decoder.state.rm + 225);
		}
		else
		{
			decoder.SetInvalidInstruction();
		}
		instruction.Op1Kind = OpKind.Immediate8;
		instruction.InternalImmediate8 = decoder.ReadByte();
	}
}
