namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_RegIb : OpCodeHandler
{
	private readonly Code code;

	private readonly Register reg;

	public OpCodeHandler_RegIb(Code code, Register reg)
	{
		this.code = code;
		this.reg = reg;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = reg;
		instruction.Op1Kind = OpKind.Immediate8;
		instruction.InternalImmediate8 = decoder.ReadByte();
	}
}
