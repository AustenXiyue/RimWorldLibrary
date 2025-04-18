namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_SimpleReg : OpCodeHandler
{
	private readonly Code code;

	private readonly int index;

	public OpCodeHandler_SimpleReg(Code code, int index)
	{
		this.code = code;
		this.index = index;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		int operandSize = (int)decoder.state.operandSize;
		instruction.InternalSetCodeNoCheck(operandSize + code);
		instruction.Op0Register = (Register)(operandSize * 16 + index + (int)decoder.state.zs.extraBaseRegisterBase + 21);
	}
}
