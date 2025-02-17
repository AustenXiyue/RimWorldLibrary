namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_EVEX_VkHW_er : OpCodeHandlerModRM
{
	private readonly Register baseReg;

	private readonly Code code;

	private readonly TupleType tupleType;

	private readonly bool onlySAE;

	private readonly bool canBroadcast;

	public OpCodeHandler_EVEX_VkHW_er(Register baseReg, Code code, TupleType tupleType, bool onlySAE, bool canBroadcast)
	{
		this.baseReg = baseReg;
		this.code = code;
		this.tupleType = tupleType;
		this.onlySAE = onlySAE;
		this.canBroadcast = canBroadcast;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = (Register)((int)(decoder.state.reg + decoder.state.zs.extraRegisterBase + decoder.state.extraRegisterBaseEVEX) + (int)baseReg);
		instruction.Op1Register = (Register)((int)decoder.state.vvvv + (int)baseReg);
		if (decoder.state.mod == 3)
		{
			instruction.Op2Register = (Register)((int)(decoder.state.rm + decoder.state.extraBaseRegisterBaseEVEX) + (int)baseReg);
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
}
