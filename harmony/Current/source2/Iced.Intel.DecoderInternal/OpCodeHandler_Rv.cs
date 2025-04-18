namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Rv : OpCodeHandlerModRM
{
	private readonly Code3 codes;

	public OpCodeHandler_Rv(Code code16, Code code32, Code code64)
	{
		codes = new Code3(code16, code32, code64);
	}

	public unsafe override void Decode(Decoder decoder, ref Instruction instruction)
	{
		nuint num = (nuint)decoder.state.operandSize;
		instruction.InternalSetCodeNoCheck((Code)codes.codes[num]);
		instruction.Op0Register = (Register)(((int)num << 4) + (int)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase) + 21);
	}
}
