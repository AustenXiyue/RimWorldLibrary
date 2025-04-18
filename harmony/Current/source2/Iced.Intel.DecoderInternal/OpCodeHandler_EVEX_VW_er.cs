namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_EVEX_VW_er : OpCodeHandlerModRM
{
	private readonly Register baseReg1;

	private readonly Register baseReg2;

	private readonly Code code;

	private readonly TupleType tupleType;

	public OpCodeHandler_EVEX_VW_er(Register baseReg, Code code, TupleType tupleType)
	{
		baseReg1 = baseReg;
		baseReg2 = baseReg;
		this.code = code;
		this.tupleType = tupleType;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = (Register)((int)(decoder.state.reg + decoder.state.zs.extraRegisterBase + decoder.state.extraRegisterBaseEVEX) + (int)baseReg1);
		if ((((uint)(decoder.state.zs.flags & StateFlags.z) | decoder.state.vvvv_invalidCheck | decoder.state.aaa) & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		if (decoder.state.mod == 3)
		{
			instruction.Op1Register = (Register)((int)(decoder.state.rm + decoder.state.extraBaseRegisterBaseEVEX) + (int)baseReg2);
			if ((decoder.state.zs.flags & StateFlags.b) != 0)
			{
				instruction.InternalSetSuppressAllExceptions();
			}
			return;
		}
		instruction.Op1Kind = OpKind.Memory;
		if (((uint)(decoder.state.zs.flags & StateFlags.b) & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		decoder.ReadOpMem(ref instruction, tupleType);
	}
}
