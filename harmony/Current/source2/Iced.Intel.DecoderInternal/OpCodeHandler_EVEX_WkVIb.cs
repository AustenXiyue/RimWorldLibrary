namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_EVEX_WkVIb : OpCodeHandlerModRM
{
	private readonly Register baseReg1;

	private readonly Register baseReg2;

	private readonly Code code;

	private readonly TupleType tupleType;

	public OpCodeHandler_EVEX_WkVIb(Register baseReg1, Register baseReg2, Code code, TupleType tupleType)
	{
		this.baseReg1 = baseReg1;
		this.baseReg2 = baseReg2;
		this.code = code;
		this.tupleType = tupleType;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if ((((uint)(decoder.state.zs.flags & StateFlags.b) | decoder.state.vvvv_invalidCheck) & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		instruction.InternalSetCodeNoCheck(code);
		if (decoder.state.mod == 3)
		{
			instruction.Op0Register = (Register)((int)(decoder.state.rm + decoder.state.extraBaseRegisterBaseEVEX) + (int)baseReg1);
		}
		else
		{
			instruction.Op0Kind = OpKind.Memory;
			if (((uint)(decoder.state.zs.flags & StateFlags.z) & decoder.invalidCheckMask) != 0)
			{
				decoder.SetInvalidInstruction();
			}
			decoder.ReadOpMem(ref instruction, tupleType);
		}
		instruction.Op1Register = (Register)((int)(decoder.state.reg + decoder.state.zs.extraRegisterBase + decoder.state.extraRegisterBaseEVEX) + (int)baseReg2);
		instruction.Op2Kind = OpKind.Immediate8;
		instruction.InternalImmediate8 = decoder.ReadByte();
	}
}
