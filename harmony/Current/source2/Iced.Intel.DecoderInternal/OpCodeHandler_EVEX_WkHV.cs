namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_EVEX_WkHV : OpCodeHandlerModRM
{
	private readonly Register baseReg;

	private readonly Code code;

	public OpCodeHandler_EVEX_WkHV(Register baseReg, Code code)
	{
		this.baseReg = baseReg;
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = (Register)((int)(decoder.state.rm + decoder.state.extraBaseRegisterBaseEVEX) + (int)baseReg);
		if (((uint)(decoder.state.zs.flags & StateFlags.b) & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		instruction.Op1Register = (Register)((int)decoder.state.vvvv + (int)baseReg);
		instruction.Op2Register = (Register)((int)(decoder.state.reg + decoder.state.zs.extraRegisterBase + decoder.state.extraRegisterBaseEVEX) + (int)baseReg);
	}
}
