namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_IbReg : OpCodeHandler
{
	private readonly Code code;

	private readonly Register reg;

	public OpCodeHandler_IbReg(Code code, Register reg)
	{
		this.code = code;
		this.reg = reg;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op1Register = reg;
		instruction.Op0Kind = OpKind.Immediate8;
		instruction.InternalImmediate8 = decoder.ReadByte();
	}
}
