namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_EVEX_GvM_VX_Ib : OpCodeHandlerModRM
{
	private readonly Register baseReg;

	private readonly Code code32;

	private readonly Code code64;

	private readonly TupleType tupleType32;

	private readonly TupleType tupleType64;

	public OpCodeHandler_EVEX_GvM_VX_Ib(Register baseReg, Code code32, Code code64, TupleType tupleType32, TupleType tupleType64)
	{
		this.baseReg = baseReg;
		this.code32 = code32;
		this.code64 = code64;
		this.tupleType32 = tupleType32;
		this.tupleType64 = tupleType64;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if ((((uint)(decoder.state.zs.flags & (StateFlags.b | StateFlags.z)) | decoder.state.vvvv_invalidCheck | decoder.state.aaa) & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		Register register;
		if (((uint)decoder.state.zs.flags & decoder.is64bMode_and_W) != 0)
		{
			instruction.InternalSetCodeNoCheck(code64);
			register = Register.RAX;
		}
		else
		{
			instruction.InternalSetCodeNoCheck(code32);
			register = Register.EAX;
		}
		if (decoder.state.mod == 3)
		{
			instruction.Op0Register = (Register)((int)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase) + (int)register);
		}
		else
		{
			instruction.Op0Kind = OpKind.Memory;
			if (((uint)decoder.state.zs.flags & decoder.is64bMode_and_W) != 0)
			{
				decoder.ReadOpMem(ref instruction, tupleType64);
			}
			else
			{
				decoder.ReadOpMem(ref instruction, tupleType32);
			}
		}
		instruction.Op1Register = (Register)((int)(decoder.state.reg + decoder.state.zs.extraRegisterBase + decoder.state.extraRegisterBaseEVEX) + (int)baseReg);
		instruction.Op2Kind = OpKind.Immediate8;
		instruction.InternalImmediate8 = decoder.ReadByte();
	}
}
