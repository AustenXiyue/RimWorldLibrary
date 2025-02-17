namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_DX_AL : OpCodeHandler
{
	private readonly Code code;

	public OpCodeHandler_DX_AL(Code code)
	{
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = Register.DX;
		instruction.Op1Register = Register.AL;
	}
}
