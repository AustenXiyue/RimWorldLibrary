namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Reg_Iz : OpCodeHandler
{
	private readonly Code code16;

	private readonly Code code32;

	private readonly Code code64;

	public OpCodeHandler_Reg_Iz(Code code16, Code code32, Code code64)
	{
		this.code16 = code16;
		this.code32 = code32;
		this.code64 = code64;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if (decoder.state.operandSize == OpSize.Size32)
		{
			instruction.InternalSetCodeNoCheck(code32);
			instruction.Op0Register = Register.EAX;
			instruction.Op1Kind = OpKind.Immediate32;
			instruction.Immediate32 = decoder.ReadUInt32();
		}
		else if (decoder.state.operandSize == OpSize.Size64)
		{
			instruction.InternalSetCodeNoCheck(code64);
			instruction.Op0Register = Register.RAX;
			instruction.Op1Kind = OpKind.Immediate32to64;
			instruction.Immediate32 = decoder.ReadUInt32();
		}
		else
		{
			instruction.InternalSetCodeNoCheck(code16);
			instruction.Op0Register = Register.AX;
			instruction.Op1Kind = OpKind.Immediate16;
			instruction.InternalImmediate16 = decoder.ReadUInt16();
		}
	}
}
