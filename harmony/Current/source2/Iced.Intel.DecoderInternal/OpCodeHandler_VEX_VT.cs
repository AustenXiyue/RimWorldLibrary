namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VEX_VT : OpCodeHandlerModRM
{
	private readonly Code code;

	public OpCodeHandler_VEX_VT(Code code)
	{
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if (((decoder.state.vvvv_invalidCheck | decoder.state.zs.extraRegisterBase) & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = (Register)(decoder.state.reg + 241);
	}
}
