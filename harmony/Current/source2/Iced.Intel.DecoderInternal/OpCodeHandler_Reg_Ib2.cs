namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Reg_Ib2 : OpCodeHandler
{
	private readonly Code code16;

	private readonly Code code32;

	public OpCodeHandler_Reg_Ib2(Code code16, Code code32)
	{
		this.code16 = code16;
		this.code32 = code32;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.Op1Kind = OpKind.Immediate8;
		instruction.InternalImmediate8 = decoder.ReadByte();
		if (decoder.state.operandSize != 0)
		{
			instruction.InternalSetCodeNoCheck(code32);
			instruction.Op0Register = Register.EAX;
		}
		else
		{
			instruction.InternalSetCodeNoCheck(code16);
			instruction.Op0Register = Register.AX;
		}
	}
}
