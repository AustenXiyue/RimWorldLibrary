namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Evj : OpCodeHandlerModRM
{
	private readonly Code code16;

	private readonly Code code32;

	private readonly Code code64;

	public OpCodeHandler_Evj(Code code16, Code code32, Code code64)
	{
		this.code16 = code16;
		this.code32 = code32;
		this.code64 = code64;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if (decoder.is64bMode)
		{
			if ((decoder.options & DecoderOptions.AMD) == 0 || decoder.state.operandSize != 0)
			{
				instruction.InternalSetCodeNoCheck(code64);
			}
			else
			{
				instruction.InternalSetCodeNoCheck(code16);
			}
			if (decoder.state.mod < 3)
			{
				instruction.Op0Kind = OpKind.Memory;
				decoder.ReadOpMem(ref instruction);
			}
			else if ((decoder.options & DecoderOptions.AMD) == 0 || decoder.state.operandSize != 0)
			{
				instruction.Op0Register = (Register)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase + 53);
			}
			else
			{
				instruction.Op0Register = (Register)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase + 21);
			}
		}
		else
		{
			if (decoder.state.operandSize == OpSize.Size32)
			{
				instruction.InternalSetCodeNoCheck(code32);
			}
			else
			{
				instruction.InternalSetCodeNoCheck(code16);
			}
			if (decoder.state.mod < 3)
			{
				instruction.Op0Kind = OpKind.Memory;
				decoder.ReadOpMem(ref instruction);
			}
			else if (decoder.state.operandSize == OpSize.Size32)
			{
				instruction.Op0Register = (Register)(decoder.state.rm + 37);
			}
			else
			{
				instruction.Op0Register = (Register)(decoder.state.rm + 21);
			}
		}
	}
}
