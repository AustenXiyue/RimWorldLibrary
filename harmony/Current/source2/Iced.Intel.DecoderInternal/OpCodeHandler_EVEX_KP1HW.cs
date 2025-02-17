namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_EVEX_KP1HW : OpCodeHandlerModRM
{
	private readonly Register baseReg;

	private readonly Code code;

	private readonly TupleType tupleType;

	public OpCodeHandler_EVEX_KP1HW(Register baseReg, Code code, TupleType tupleType)
	{
		this.baseReg = baseReg;
		this.code = code;
		this.tupleType = tupleType;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = (Register)(decoder.state.reg + 173);
		instruction.Op1Register = (Register)((int)decoder.state.vvvv + (int)baseReg);
		if ((((uint)(decoder.state.zs.flags & StateFlags.z) | decoder.state.aaa | decoder.state.zs.extraRegisterBase | decoder.state.extraRegisterBaseEVEX) & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		if (decoder.state.mod == 3)
		{
			instruction.Op2Register = (Register)((int)(decoder.state.rm + decoder.state.extraBaseRegisterBaseEVEX) + (int)baseReg);
			if (((uint)(decoder.state.zs.flags & StateFlags.b) & decoder.invalidCheckMask) != 0)
			{
				decoder.SetInvalidInstruction();
			}
			return;
		}
		instruction.Op2Kind = OpKind.Memory;
		if ((decoder.state.zs.flags & StateFlags.b) != 0)
		{
			instruction.InternalSetIsBroadcast();
		}
		decoder.ReadOpMem(ref instruction, tupleType);
	}
}
