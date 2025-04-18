namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_EVEX_VHWIb : OpCodeHandlerModRM
{
	private readonly Register baseReg;

	private readonly Code code;

	private readonly TupleType tupleType;

	public OpCodeHandler_EVEX_VHWIb(Register baseReg, Code code, TupleType tupleType)
	{
		this.baseReg = baseReg;
		this.code = code;
		this.tupleType = tupleType;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if ((((uint)(decoder.state.zs.flags & (StateFlags.b | StateFlags.z)) | decoder.state.aaa) & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = (Register)((int)(decoder.state.reg + decoder.state.zs.extraRegisterBase + decoder.state.extraRegisterBaseEVEX) + (int)baseReg);
		instruction.Op1Register = (Register)((int)decoder.state.vvvv + (int)baseReg);
		if (decoder.state.mod == 3)
		{
			instruction.Op2Register = (Register)((int)(decoder.state.rm + decoder.state.extraBaseRegisterBaseEVEX) + (int)baseReg);
		}
		else
		{
			instruction.Op2Kind = OpKind.Memory;
			decoder.ReadOpMem(ref instruction, tupleType);
		}
		instruction.Op3Kind = OpKind.Immediate8;
		instruction.InternalImmediate8 = decoder.ReadByte();
	}
}
