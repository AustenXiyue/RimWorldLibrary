namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Simple2 : OpCodeHandler
{
	private readonly Code3 codes;

	public OpCodeHandler_Simple2(Code code16, Code code32, Code code64)
	{
		codes = new Code3(code16, code32, code64);
	}

	public unsafe override void Decode(Decoder decoder, ref Instruction instruction)
	{
		nuint num = (nuint)decoder.state.operandSize;
		instruction.InternalSetCodeNoCheck((Code)codes.codes[num]);
	}
}
