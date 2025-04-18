namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_RIbIb : OpCodeHandlerModRM
{
	private readonly Code code;

	public OpCodeHandler_RIbIb(Code code)
	{
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		if (decoder.state.mod == 3)
		{
			instruction.Op0Register = (Register)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase + 77);
		}
		else
		{
			decoder.SetInvalidInstruction();
		}
		instruction.Op1Kind = OpKind.Immediate8;
		instruction.InternalImmediate8 = decoder.ReadByte();
		instruction.Op2Kind = OpKind.Immediate8_2nd;
		instruction.InternalImmediate8_2nd = decoder.ReadByte();
	}
}
