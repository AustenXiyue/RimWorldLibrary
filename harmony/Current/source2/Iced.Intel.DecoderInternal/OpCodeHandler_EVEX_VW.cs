namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_EVEX_VW : OpCodeHandlerModRM
{
	private readonly Register baseReg1;

	private readonly Register baseReg2;

	private readonly Code code;

	private readonly TupleType tupleType;

	public OpCodeHandler_EVEX_VW(Register baseReg, Code code, TupleType tupleType)
	{
		baseReg1 = baseReg;
		baseReg2 = baseReg;
		this.code = code;
		this.tupleType = tupleType;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if ((((uint)(decoder.state.zs.flags & (StateFlags.b | StateFlags.z)) | decoder.state.vvvv_invalidCheck | decoder.state.aaa) & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = (Register)((int)(decoder.state.reg + decoder.state.zs.extraRegisterBase + decoder.state.extraRegisterBaseEVEX) + (int)baseReg1);
		if (decoder.state.mod == 3)
		{
			instruction.Op1Register = (Register)((int)(decoder.state.rm + decoder.state.extraBaseRegisterBaseEVEX) + (int)baseReg2);
			if (((uint)(decoder.state.zs.flags & StateFlags.b) & decoder.invalidCheckMask) != 0)
			{
				decoder.SetInvalidInstruction();
			}
		}
		else
		{
			instruction.Op1Kind = OpKind.Memory;
			decoder.ReadOpMem(ref instruction, tupleType);
		}
	}
}
