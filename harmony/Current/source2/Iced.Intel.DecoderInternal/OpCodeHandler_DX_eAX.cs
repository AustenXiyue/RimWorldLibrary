namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_DX_eAX : OpCodeHandler
{
	private readonly Code code16;

	private readonly Code code32;

	public OpCodeHandler_DX_eAX(Code code16, Code code32)
	{
		this.code16 = code16;
		this.code32 = code32;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.Op0Register = Register.DX;
		if (decoder.state.operandSize != 0)
		{
			instruction.InternalSetCodeNoCheck(code32);
			instruction.Op1Register = Register.EAX;
		}
		else
		{
			instruction.InternalSetCodeNoCheck(code16);
			instruction.Op1Register = Register.AX;
		}
	}
}
