namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Iw_Ib : OpCodeHandler
{
	private readonly Code code16;

	private readonly Code code32;

	private readonly Code code64;

	public OpCodeHandler_Iw_Ib(Code code16, Code code32, Code code64)
	{
		this.code16 = code16;
		this.code32 = code32;
		this.code64 = code64;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.Op0Kind = OpKind.Immediate16;
		instruction.InternalImmediate16 = decoder.ReadUInt16();
		instruction.Op1Kind = OpKind.Immediate8_2nd;
		instruction.InternalImmediate8_2nd = decoder.ReadByte();
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
	}
}
