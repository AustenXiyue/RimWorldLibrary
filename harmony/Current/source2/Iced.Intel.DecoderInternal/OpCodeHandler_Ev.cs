namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Ev : OpCodeHandlerModRM
{
	private readonly Code3 codes;

	private readonly HandlerFlags flags;

	public OpCodeHandler_Ev(Code code16, Code code32, Code code64)
	{
		codes = new Code3(code16, code32, code64);
	}

	public OpCodeHandler_Ev(Code code16, Code code32, Code code64, HandlerFlags flags)
	{
		codes = new Code3(code16, code32, code64);
		this.flags = flags;
	}

	public unsafe override void Decode(Decoder decoder, ref Instruction instruction)
	{
		nuint num = (nuint)decoder.state.operandSize;
		instruction.InternalSetCodeNoCheck((Code)codes.codes[num]);
		if (decoder.state.mod == 3)
		{
			instruction.Op0Register = (Register)(((int)num << 4) + (int)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase) + 21);
			return;
		}
		decoder.state.zs.flags |= (StateFlags)((uint)(flags & HandlerFlags.Lock) << 10);
		instruction.Op0Kind = OpKind.Memory;
		decoder.ReadOpMem(ref instruction);
	}
}
