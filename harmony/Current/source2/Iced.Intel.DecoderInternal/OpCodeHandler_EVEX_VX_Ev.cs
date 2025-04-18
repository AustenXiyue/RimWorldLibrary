namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_EVEX_VX_Ev : OpCodeHandlerModRM
{
	private readonly Code code32;

	private readonly Code code64;

	private readonly TupleType tupleTypeW0;

	private readonly TupleType tupleTypeW1;

	public OpCodeHandler_EVEX_VX_Ev(Code code32, Code code64, TupleType tupleTypeW0, TupleType tupleTypeW1)
	{
		this.code32 = code32;
		this.code64 = code64;
		this.tupleTypeW0 = tupleTypeW0;
		this.tupleTypeW1 = tupleTypeW1;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if ((((uint)(decoder.state.zs.flags & (StateFlags.b | StateFlags.z)) | decoder.state.vvvv_invalidCheck | decoder.state.aaa) & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		TupleType tupleType;
		Register register;
		if (((uint)decoder.state.zs.flags & decoder.is64bMode_and_W) != 0)
		{
			instruction.InternalSetCodeNoCheck(code64);
			tupleType = tupleTypeW1;
			register = Register.RAX;
		}
		else
		{
			instruction.InternalSetCodeNoCheck(code32);
			tupleType = tupleTypeW0;
			register = Register.EAX;
		}
		instruction.Op0Register = (Register)(decoder.state.reg + decoder.state.zs.extraRegisterBase + decoder.state.extraRegisterBaseEVEX + 77);
		if (decoder.state.mod == 3)
		{
			instruction.Op1Register = (Register)((int)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase) + (int)register);
			return;
		}
		instruction.Op1Kind = OpKind.Memory;
		decoder.ReadOpMem(ref instruction, tupleType);
	}
}
