namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_EVEX_VSIB_k1 : OpCodeHandlerModRM
{
	private readonly Register vsibIndex;

	private readonly Code code;

	private readonly TupleType tupleType;

	public OpCodeHandler_EVEX_VSIB_k1(Register vsibIndex, Code code, TupleType tupleType)
	{
		this.vsibIndex = vsibIndex;
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
		if (decoder.state.mod == 3)
		{
			decoder.SetInvalidInstruction();
			return;
		}
		instruction.Op0Kind = OpKind.Memory;
		decoder.ReadOpMem_VSIB(ref instruction, vsibIndex, tupleType);
	}
}
