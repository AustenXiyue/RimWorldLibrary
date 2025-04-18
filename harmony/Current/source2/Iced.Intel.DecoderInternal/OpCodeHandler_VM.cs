namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VM : OpCodeHandlerModRM
{
	private readonly Code code;

	public OpCodeHandler_VM(Code code)
	{
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = (Register)(decoder.state.reg + decoder.state.zs.extraRegisterBase + 77);
		if (decoder.state.mod == 3)
		{
			decoder.SetInvalidInstruction();
			return;
		}
		instruction.Op1Kind = OpKind.Memory;
		decoder.ReadOpMem(ref instruction);
	}
}
