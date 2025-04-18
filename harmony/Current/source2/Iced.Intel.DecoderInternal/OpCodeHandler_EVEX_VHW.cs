namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_EVEX_VHW : OpCodeHandlerModRM
{
	private readonly Register baseReg1;

	private readonly Register baseReg2;

	private readonly Register baseReg3;

	private readonly Code codeR;

	private readonly Code codeM;

	private readonly TupleType tupleType;

	public OpCodeHandler_EVEX_VHW(Register baseReg, Code codeR, Code codeM, TupleType tupleType)
	{
		baseReg1 = baseReg;
		baseReg2 = baseReg;
		baseReg3 = baseReg;
		this.codeR = codeR;
		this.codeM = codeM;
		this.tupleType = tupleType;
	}

	public OpCodeHandler_EVEX_VHW(Register baseReg, Code code, TupleType tupleType)
	{
		baseReg1 = baseReg;
		baseReg2 = baseReg;
		baseReg3 = baseReg;
		codeR = code;
		codeM = code;
		this.tupleType = tupleType;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if ((((uint)(decoder.state.zs.flags & (StateFlags.b | StateFlags.z)) | decoder.state.aaa) & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		instruction.Op0Register = (Register)((int)(decoder.state.reg + decoder.state.zs.extraRegisterBase + decoder.state.extraRegisterBaseEVEX) + (int)baseReg1);
		instruction.Op1Register = (Register)((int)decoder.state.vvvv + (int)baseReg2);
		if (decoder.state.mod == 3)
		{
			instruction.InternalSetCodeNoCheck(codeR);
			instruction.Op2Register = (Register)((int)(decoder.state.rm + decoder.state.extraBaseRegisterBaseEVEX) + (int)baseReg3);
		}
		else
		{
			instruction.InternalSetCodeNoCheck(codeM);
			instruction.Op2Kind = OpKind.Memory;
			decoder.ReadOpMem(ref instruction, tupleType);
		}
	}
}
