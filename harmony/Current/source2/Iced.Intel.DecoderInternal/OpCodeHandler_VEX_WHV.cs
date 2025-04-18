namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VEX_WHV : OpCodeHandlerModRM
{
	private readonly Register baseReg;

	private readonly Code codeR;

	public OpCodeHandler_VEX_WHV(Register baseReg, Code code)
	{
		this.baseReg = baseReg;
		codeR = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(codeR);
		instruction.Op0Register = (Register)((int)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase) + (int)baseReg);
		instruction.Op1Register = (Register)((int)decoder.state.vvvv + (int)baseReg);
		instruction.Op2Register = (Register)((int)(decoder.state.reg + decoder.state.zs.extraRegisterBase) + (int)baseReg);
	}
}
