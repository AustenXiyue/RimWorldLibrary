namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VEX_VW : OpCodeHandlerModRM
{
	private readonly Register baseReg1;

	private readonly Register baseReg2;

	private readonly Code code;

	public OpCodeHandler_VEX_VW(Register baseReg, Code code)
	{
		baseReg1 = baseReg;
		baseReg2 = baseReg;
		this.code = code;
	}

	public OpCodeHandler_VEX_VW(Register baseReg1, Register baseReg2, Code code)
	{
		this.baseReg1 = baseReg1;
		this.baseReg2 = baseReg2;
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if ((decoder.state.vvvv_invalidCheck & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = (Register)((int)(decoder.state.reg + decoder.state.zs.extraRegisterBase) + (int)baseReg1);
		if (decoder.state.mod == 3)
		{
			instruction.Op1Register = (Register)((int)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase) + (int)baseReg2);
			return;
		}
		instruction.Op1Kind = OpKind.Memory;
		decoder.ReadOpMem(ref instruction);
	}
}
