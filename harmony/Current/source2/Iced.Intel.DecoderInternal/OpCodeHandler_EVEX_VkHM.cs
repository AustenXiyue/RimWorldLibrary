namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_EVEX_VkHM : OpCodeHandlerModRM
{
	private readonly Register baseReg1;

	private readonly Register baseReg2;

	private readonly Code code;

	private readonly TupleType tupleType;

	public OpCodeHandler_EVEX_VkHM(Register baseReg, Code code, TupleType tupleType)
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
		instruction.Op1Register = (Register)((int)decoder.state.vvvv + (int)baseReg2);
		if (decoder.state.mod == 3)
		{
			decoder.SetInvalidInstruction();
			return;
		}
		instruction.Op2Kind = OpKind.Memory;
		if (((uint)(decoder.state.zs.flags & StateFlags.b) & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		decoder.ReadOpMem(ref instruction, tupleType);
	}
}
