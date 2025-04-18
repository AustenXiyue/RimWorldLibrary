namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_PushOpSizeReg : OpCodeHandler
{
	private readonly Code code16;

	private readonly Code code32;

	private readonly Code code64;

	private readonly Register reg;

	public OpCodeHandler_PushOpSizeReg(Code code16, Code code32, Code code64, Register reg)
	{
		this.code16 = code16;
		this.code32 = code32;
		this.code64 = code64;
		this.reg = reg;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if (decoder.is64bMode)
		{
			if (decoder.state.operandSize != 0)
			{
				instruction.InternalSetCodeNoCheck(code64);
			}
			else
			{
				instruction.InternalSetCodeNoCheck(code16);
			}
		}
		else if (decoder.state.operandSize == OpSize.Size32)
		{
			instruction.InternalSetCodeNoCheck(code32);
		}
		else
		{
			instruction.InternalSetCodeNoCheck(code16);
		}
		instruction.Op0Register = reg;
	}
}
