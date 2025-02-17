namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VEX_VX_VSIB_HX : OpCodeHandlerModRM
{
	private readonly Register baseReg1;

	private readonly Register vsibIndex;

	private readonly Register baseReg3;

	private readonly Code code;

	public OpCodeHandler_VEX_VX_VSIB_HX(Register baseReg1, Register vsibIndex, Register baseReg3, Code code)
	{
		this.baseReg1 = baseReg1;
		this.vsibIndex = vsibIndex;
		this.baseReg3 = baseReg3;
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		int num = (int)(decoder.state.reg + decoder.state.zs.extraRegisterBase);
		instruction.Op0Register = num + baseReg1;
		instruction.Op2Register = (Register)((int)decoder.state.vvvv + (int)baseReg3);
		if (decoder.state.mod == 3)
		{
			decoder.SetInvalidInstruction();
			return;
		}
		instruction.Op1Kind = OpKind.Memory;
		decoder.ReadOpMem_VSIB(ref instruction, vsibIndex, TupleType.N1);
		if (decoder.invalidCheckMask != 0)
		{
			uint num2 = (uint)(instruction.MemoryIndex - 77) % 32u;
			if (num == (int)num2 || decoder.state.vvvv == num2 || num == (int)decoder.state.vvvv)
			{
				decoder.SetInvalidInstruction();
			}
		}
	}
}
