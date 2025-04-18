namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_EVEX_Vk_VSIB : OpCodeHandlerModRM
{
	private readonly Register baseReg;

	private readonly Register vsibBase;

	private readonly Code code;

	private readonly TupleType tupleType;

	public OpCodeHandler_EVEX_Vk_VSIB(Register baseReg, Register vsibBase, Code code, TupleType tupleType)
	{
		this.baseReg = baseReg;
		this.vsibBase = vsibBase;
		this.code = code;
		this.tupleType = tupleType;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if (decoder.invalidCheckMask != 0 && (((uint)(decoder.state.zs.flags & (StateFlags.b | StateFlags.z)) | (decoder.state.vvvv_invalidCheck & 0xF)) != 0 || decoder.state.aaa == 0))
		{
			decoder.SetInvalidInstruction();
		}
		instruction.InternalSetCodeNoCheck(code);
		int num = (int)(decoder.state.reg + decoder.state.zs.extraRegisterBase + decoder.state.extraRegisterBaseEVEX);
		instruction.Op0Register = num + baseReg;
		if (decoder.state.mod == 3)
		{
			decoder.SetInvalidInstruction();
			return;
		}
		instruction.Op1Kind = OpKind.Memory;
		decoder.ReadOpMem_VSIB(ref instruction, vsibBase, tupleType);
		if (decoder.invalidCheckMask != 0 && num == (int)((uint)(instruction.MemoryIndex - 77) % 32u))
		{
			decoder.SetInvalidInstruction();
		}
	}
}
