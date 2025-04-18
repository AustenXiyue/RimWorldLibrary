namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_AL_DX : OpCodeHandler
{
	private readonly Code code;

	public OpCodeHandler_AL_DX(Code code)
	{
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = Register.AL;
		instruction.Op1Register = Register.DX;
	}
}
