namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_EVEX_VkW_er : OpCodeHandlerModRM
{
	private readonly Register baseReg1;

	private readonly Register baseReg2;

	private readonly Code code;

	private readonly TupleType tupleType;

	private readonly bool onlySAE;

	private readonly bool canBroadcast;

	public OpCodeHandler_EVEX_VkW_er(Register baseReg, Code code, TupleType tupleType, bool onlySAE)
	{
		baseReg1 = baseReg;
		baseReg2 = baseReg;
		this.code = code;
		this.tupleType = tupleType;
		this.onlySAE = onlySAE;
		canBroadcast = true;
	}

	public OpCodeHandler_EVEX_VkW_er(Register baseReg1, Register baseReg2, Code code, TupleType tupleType, bool onlySAE)
	{
		this.baseReg1 = baseReg1;
		this.baseReg2 = baseReg2;
		this.code = code;
		this.tupleType = tupleType;
		this.onlySAE = onlySAE;
		canBroadcast = true;
	}

	public OpCodeHandler_EVEX_VkW_er(Register baseReg1, Register baseReg2, Code code, TupleType tupleType, bool onlySAE, bool canBroadcast)
	{
		this.baseReg1 = baseReg1;
		this.baseReg2 = baseReg2;
		this.code = code;
		this.tupleType = tupleType;
		this.onlySAE = onlySAE;
		this.canBroadcast = canBroadcast;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if ((decoder.state.vvvv_invalidCheck & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = (Register)((int)(decoder.state.reg + decoder.state.zs.extraRegisterBase + decoder.state.extraRegisterBaseEVEX) + (int)baseReg1);
		if (decoder.state.mod == 3)
		{
			instruction.Op1Register = (Register)((int)(decoder.state.rm + decoder.state.extraBaseRegisterBaseEVEX) + (int)baseReg2);
			if ((decoder.state.zs.flags & StateFlags.b) != 0)
			{
				if (onlySAE)
				{
					instruction.InternalSetSuppressAllExceptions();
				}
				else
				{
					instruction.InternalRoundingControl = decoder.state.vectorLength + 1;
				}
			}
			return;
		}
		instruction.Op1Kind = OpKind.Memory;
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
}
