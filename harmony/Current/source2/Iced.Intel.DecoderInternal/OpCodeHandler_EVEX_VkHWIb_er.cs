namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_EVEX_VkHWIb_er : OpCodeHandlerModRM
{
	private readonly Register baseReg1;

	private readonly Register baseReg2;

	private readonly Register baseReg3;

	private readonly Code code;

	private readonly TupleType tupleType;

	private readonly bool canBroadcast;

	public OpCodeHandler_EVEX_VkHWIb_er(Register baseReg, Code code, TupleType tupleType, bool canBroadcast)
	{
		baseReg1 = baseReg;
		baseReg2 = baseReg;
		baseReg3 = baseReg;
		this.code = code;
		this.tupleType = tupleType;
		this.canBroadcast = canBroadcast;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = (Register)((int)(decoder.state.reg + decoder.state.zs.extraRegisterBase + decoder.state.extraRegisterBaseEVEX) + (int)baseReg1);
		instruction.Op1Register = (Register)((int)decoder.state.vvvv + (int)baseReg2);
		if (decoder.state.mod == 3)
		{
			instruction.Op2Register = (Register)((int)(decoder.state.rm + decoder.state.extraBaseRegisterBaseEVEX) + (int)baseReg3);
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
