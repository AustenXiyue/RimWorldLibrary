namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VEX_RdRq : OpCodeHandlerModRM
{
	private readonly Code code32;

	private readonly Code code64;

	public OpCodeHandler_VEX_RdRq(Code code32, Code code64)
	{
		this.code32 = code32;
		this.code64 = code64;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if ((decoder.state.vvvv_invalidCheck & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		if (((uint)decoder.state.zs.flags & decoder.is64bMode_and_W) != 0)
		{
			instruction.InternalSetCodeNoCheck(code64);
			instruction.Op0Register = (Register)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase + 53);
		}
		else
		{
			instruction.InternalSetCodeNoCheck(code32);
			instruction.Op0Register = (Register)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase + 37);
		}
		if (decoder.state.mod != 3)
		{
			decoder.SetInvalidInstruction();
		}
	}
}
