namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_EVEX_WkV : OpCodeHandlerModRM
{
	private readonly Register baseReg1;

	private readonly Register baseReg2;

	private readonly Code code;

	private readonly TupleType tupleType;

	private readonly uint disallowZeroingMasking;

	public OpCodeHandler_EVEX_WkV(Register baseReg, Code code, TupleType tupleType)
	{
		baseReg1 = baseReg;
		baseReg2 = baseReg;
		this.code = code;
		this.tupleType = tupleType;
		disallowZeroingMasking = 0u;
	}

	public OpCodeHandler_EVEX_WkV(Register baseReg, Code code, TupleType tupleType, bool allowZeroingMasking)
	{
		baseReg1 = baseReg;
		baseReg2 = baseReg;
		this.code = code;
		this.tupleType = tupleType;
		disallowZeroingMasking = ((!allowZeroingMasking) ? uint.MaxValue : 0u);
	}

	public OpCodeHandler_EVEX_WkV(Register baseReg1, Register baseReg2, Code code, TupleType tupleType)
	{
		this.baseReg1 = baseReg1;
		this.baseReg2 = baseReg2;
		this.code = code;
		this.tupleType = tupleType;
		disallowZeroingMasking = 0u;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if ((((uint)(decoder.state.zs.flags & StateFlags.b) | decoder.state.vvvv_invalidCheck) & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op1Register = (Register)((int)(decoder.state.reg + decoder.state.zs.extraRegisterBase + decoder.state.extraRegisterBaseEVEX) + (int)baseReg2);
		if (((uint)(decoder.state.zs.flags & StateFlags.z) & disallowZeroingMasking & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		if (decoder.state.mod == 3)
		{
			instruction.Op0Register = (Register)((int)(decoder.state.rm + decoder.state.extraBaseRegisterBaseEVEX) + (int)baseReg1);
			return;
		}
		instruction.Op0Kind = OpKind.Memory;
		if (((uint)(decoder.state.zs.flags & StateFlags.z) & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		decoder.ReadOpMem(ref instruction, tupleType);
	}
}
