namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_WV : OpCodeHandlerModRM
{
	private readonly Code code;

	public OpCodeHandler_WV(Code code)
	{
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op1Register = (Register)(decoder.state.reg + decoder.state.zs.extraRegisterBase + 77);
		if (decoder.state.mod < 3)
		{
			instruction.Op0Kind = OpKind.Memory;
			decoder.ReadOpMem(ref instruction);
		}
		else
		{
			instruction.Op0Register = (Register)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase + 77);
		}
	}
}
