namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VEX_VHM : OpCodeHandlerModRM
{
	private readonly Register baseReg;

	private readonly Code code;

	public OpCodeHandler_VEX_VHM(Register baseReg, Code code)
	{
		this.baseReg = baseReg;
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = (Register)((int)(decoder.state.reg + decoder.state.zs.extraRegisterBase) + (int)baseReg);
		instruction.Op1Register = (Register)((int)decoder.state.vvvv + (int)baseReg);
		if (decoder.state.mod == 3)
		{
			decoder.SetInvalidInstruction();
			return;
		}
		instruction.Op2Kind = OpKind.Memory;
		decoder.ReadOpMem(ref instruction);
	}
}
