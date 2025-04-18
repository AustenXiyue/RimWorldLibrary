namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VEX_VHWIb : OpCodeHandlerModRM
{
	private readonly Register baseReg1;

	private readonly Register baseReg2;

	private readonly Register baseReg3;

	private readonly Code code;

	public OpCodeHandler_VEX_VHWIb(Register baseReg, Code code)
	{
		baseReg1 = baseReg;
		baseReg2 = baseReg;
		baseReg3 = baseReg;
		this.code = code;
	}

	public OpCodeHandler_VEX_VHWIb(Register baseReg1, Register baseReg2, Register baseReg3, Code code)
	{
		this.baseReg1 = baseReg1;
		this.baseReg2 = baseReg2;
		this.baseReg3 = baseReg3;
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = (Register)((int)(decoder.state.reg + decoder.state.zs.extraRegisterBase) + (int)baseReg1);
		instruction.Op1Register = (Register)((int)decoder.state.vvvv + (int)baseReg2);
		if (decoder.state.mod == 3)
		{
			instruction.Op2Register = (Register)((int)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase) + (int)baseReg3);
		}
		else
		{
			instruction.Op2Kind = OpKind.Memory;
			decoder.ReadOpMem(ref instruction);
		}
		instruction.Op3Kind = OpKind.Immediate8;
		instruction.InternalImmediate8 = decoder.ReadByte();
	}
}
