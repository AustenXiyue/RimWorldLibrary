namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_PushIb2 : OpCodeHandler
{
	private readonly Code code16;

	private readonly Code code32;

	private readonly Code code64;

	public OpCodeHandler_PushIb2(Code code16, Code code32, Code code64)
	{
		this.code16 = code16;
		this.code32 = code32;
		this.code64 = code64;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if (decoder.is64bMode)
		{
			if (decoder.state.operandSize != 0)
			{
				instruction.InternalSetCodeNoCheck(code64);
				instruction.Op0Kind = OpKind.Immediate8to64;
				instruction.InternalImmediate8 = decoder.ReadByte();
			}
			else
			{
				instruction.InternalSetCodeNoCheck(code16);
				instruction.Op0Kind = OpKind.Immediate8to16;
				instruction.InternalImmediate8 = decoder.ReadByte();
			}
		}
		else if (decoder.state.operandSize == OpSize.Size32)
		{
			instruction.InternalSetCodeNoCheck(code32);
			instruction.Op0Kind = OpKind.Immediate8to32;
			instruction.InternalImmediate8 = decoder.ReadByte();
		}
		else
		{
			instruction.InternalSetCodeNoCheck(code16);
			instruction.Op0Kind = OpKind.Immediate8to16;
			instruction.InternalImmediate8 = decoder.ReadByte();
		}
	}
}
