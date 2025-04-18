namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VEX_VHW : OpCodeHandlerModRM
{
	private readonly Register baseReg1;

	private readonly Register baseReg2;

	private readonly Register baseReg3;

	private readonly Code codeR;

	private readonly Code codeM;

	public OpCodeHandler_VEX_VHW(Register baseReg, Code codeR, Code codeM)
	{
		baseReg1 = baseReg;
		baseReg2 = baseReg;
		baseReg3 = baseReg;
		this.codeR = codeR;
		this.codeM = codeM;
	}

	public OpCodeHandler_VEX_VHW(Register baseReg, Code code)
	{
		baseReg1 = baseReg;
		baseReg2 = baseReg;
		baseReg3 = baseReg;
		codeR = code;
		codeM = code;
	}

	public OpCodeHandler_VEX_VHW(Register baseReg1, Register baseReg2, Register baseReg3, Code code)
	{
		this.baseReg1 = baseReg1;
		this.baseReg2 = baseReg2;
		this.baseReg3 = baseReg3;
		codeR = code;
		codeM = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.Op0Register = (Register)((int)(decoder.state.reg + decoder.state.zs.extraRegisterBase) + (int)baseReg1);
		instruction.Op1Register = (Register)((int)decoder.state.vvvv + (int)baseReg2);
		if (decoder.state.mod == 3)
		{
			instruction.InternalSetCodeNoCheck(codeR);
			instruction.Op2Register = (Register)((int)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase) + (int)baseReg3);
		}
		else
		{
			instruction.InternalSetCodeNoCheck(codeM);
			instruction.Op2Kind = OpKind.Memory;
			decoder.ReadOpMem(ref instruction);
		}
	}
}
