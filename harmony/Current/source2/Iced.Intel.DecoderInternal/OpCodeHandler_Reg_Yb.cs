namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Reg_Yb : OpCodeHandler
{
	private readonly Code code;

	private readonly Register reg;

	public OpCodeHandler_Reg_Yb(Code code, Register reg)
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
			instruction.Op1Kind = OpKind.MemoryESRDI;
		}
		else if (decoder.state.addressSize == OpSize.Size32)
		{
			instruction.Op1Kind = OpKind.MemoryESEDI;
		}
		else
		{
			instruction.Op1Kind = OpKind.MemoryESDI;
		}
	}
}
