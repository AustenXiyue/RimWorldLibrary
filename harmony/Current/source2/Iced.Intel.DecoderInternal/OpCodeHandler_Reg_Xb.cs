namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Reg_Xb : OpCodeHandler
{
	private readonly Code code;

	private readonly Register reg;

	public OpCodeHandler_Reg_Xb(Code code, Register reg)
	{
		this.code = code;
		this.reg = reg;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = reg;
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
