namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Reg_Xv2 : OpCodeHandler
{
	private readonly Code code16;

	private readonly Code code32;

	public OpCodeHandler_Reg_Xv2(Code code16, Code code32)
	{
		this.code16 = code16;
		this.code32 = code32;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if (decoder.state.operandSize != 0)
		{
			instruction.InternalSetCodeNoCheck(code32);
			instruction.Op0Register = Register.DX;
		}
		else
		{
			instruction.InternalSetCodeNoCheck(code16);
			instruction.Op0Register = Register.DX;
		}
		if (decoder.state.addressSize == OpSize.Size64)
		{
			instruction.Op1Kind = OpKind.MemorySegRSI;
		}
		else if (decoder.state.addressSize == OpSize.Size32)
		{
			instruction.Op1Kind = OpKind.MemorySegESI;
		}
		else
		{
			instruction.Op1Kind = OpKind.MemorySegSI;
		}
	}
}
