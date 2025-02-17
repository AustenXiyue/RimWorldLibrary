namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_EVEX_VkHW_er_ur : OpCodeHandlerModRM
{
	private readonly Register baseReg;

	private readonly Code code;

	private readonly TupleType tupleType;

	private readonly bool canBroadcast;

	public OpCodeHandler_EVEX_VkHW_er_ur(Register baseReg, Code code, TupleType tupleType, bool canBroadcast)
	{
		this.baseReg = baseReg;
		this.code = code;
		this.tupleType = tupleType;
		this.canBroadcast = canBroadcast;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		int num = (int)(decoder.state.reg + decoder.state.zs.extraRegisterBase + decoder.state.extraRegisterBaseEVEX);
		instruction.Op0Register = num + baseReg;
		instruction.Op1Register = (Register)((int)decoder.state.vvvv + (int)baseReg);
		if (decoder.state.mod == 3)
		{
			int num2 = (int)(decoder.state.rm + decoder.state.extraBaseRegisterBaseEVEX);
			instruction.Op2Register = num2 + baseReg;
			if (decoder.invalidCheckMask != 0 && (num == (int)decoder.state.vvvv || num == num2))
			{
				decoder.SetInvalidInstruction();
			}
			if ((decoder.state.zs.flags & StateFlags.b) != 0)
			{
				instruction.InternalRoundingControl = decoder.state.vectorLength + 1;
			}
			return;
		}
		if (decoder.invalidCheckMask != 0 && num == (int)decoder.state.vvvv)
		{
			decoder.SetInvalidInstruction();
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
