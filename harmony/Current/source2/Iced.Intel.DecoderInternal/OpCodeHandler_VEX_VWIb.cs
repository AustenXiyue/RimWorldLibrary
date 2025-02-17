namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VEX_VWIb : OpCodeHandlerModRM
{
	private readonly Register baseReg1;

	private readonly Register baseReg2;

	private readonly Code codeW0;

	private readonly Code codeW1;

	public OpCodeHandler_VEX_VWIb(Register baseReg, Code code)
	{
		baseReg1 = baseReg;
		baseReg2 = baseReg;
		codeW0 = code;
		codeW1 = code;
	}

	public OpCodeHandler_VEX_VWIb(Register baseReg, Code codeW0, Code codeW1)
	{
		baseReg1 = baseReg;
		baseReg2 = baseReg;
		this.codeW0 = codeW0;
		this.codeW1 = codeW1;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if ((decoder.state.vvvv_invalidCheck & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		if (((uint)decoder.state.zs.flags & decoder.is64bMode_and_W) != 0)
		{
			instruction.InternalSetCodeNoCheck(codeW1);
		}
		else
		{
			instruction.InternalSetCodeNoCheck(codeW0);
		}
		instruction.Op0Register = (Register)((int)(decoder.state.reg + decoder.state.zs.extraRegisterBase) + (int)baseReg1);
		if (decoder.state.mod == 3)
		{
			instruction.Op1Register = (Register)((int)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase) + (int)baseReg2);
		}
		else
		{
			instruction.Op1Kind = OpKind.Memory;
			decoder.ReadOpMem(ref instruction);
		}
		instruction.Op2Kind = OpKind.Immediate8;
		instruction.InternalImmediate8 = decoder.ReadByte();
	}
}
