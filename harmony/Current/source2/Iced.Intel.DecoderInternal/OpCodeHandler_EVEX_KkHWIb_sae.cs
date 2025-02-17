namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_EVEX_KkHWIb_sae : OpCodeHandlerModRM
{
	private readonly Register baseReg;

	private readonly Code code;

	private readonly TupleType tupleType;

	private readonly bool canBroadcast;

	public OpCodeHandler_EVEX_KkHWIb_sae(Register baseReg, Code code, TupleType tupleType, bool canBroadcast)
	{
		this.baseReg = baseReg;
		this.code = code;
		this.tupleType = tupleType;
		this.canBroadcast = canBroadcast;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if ((((uint)(decoder.state.zs.flags & StateFlags.z) | decoder.state.zs.extraRegisterBase | decoder.state.extraRegisterBaseEVEX) & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = (Register)(decoder.state.reg + 173);
		instruction.Op1Register = (Register)((int)decoder.state.vvvv + (int)baseReg);
		if (decoder.state.mod == 3)
		{
			instruction.Op2Register = (Register)((int)(decoder.state.rm + decoder.state.extraBaseRegisterBaseEVEX) + (int)baseReg);
			if ((decoder.state.zs.flags & StateFlags.b) != 0)
			{
				instruction.InternalSetSuppressAllExceptions();
			}
		}
		else
		{
			instruction.Op2Kind = OpKind.Memory;
			if ((decoder.state.zs.flags & StateFlags.b) != 0)
			{
				if (canBroadcast)
				{
					instruction.InternalSetIsBroadcast();
				}
				else if (decoder.invalidCheckMask != 0)
				{
					decoder.SetInvalidInstruction();
				}
			}
			decoder.ReadOpMem(ref instruction, tupleType);
		}
		instruction.Op3Kind = OpKind.Immediate8;
		instruction.InternalImmediate8 = decoder.ReadByte();
	}
}
