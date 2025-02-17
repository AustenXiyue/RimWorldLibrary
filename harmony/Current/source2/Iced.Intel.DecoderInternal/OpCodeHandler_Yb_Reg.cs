namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Yb_Reg : OpCodeHandler
{
	private readonly Code code;

	private readonly Register reg;

	public OpCodeHandler_Yb_Reg(Code code, Register reg)
	{
		this.code = code;
		this.reg = reg;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op1Register = reg;
		if (decoder.state.addressSize == OpSize.Size64)
		{
			instruction.Op0Kind = OpKind.MemoryESRDI;
		}
		else if (decoder.state.addressSize == OpSize.Size32)
		{
			instruction.Op0Kind = OpKind.MemoryESEDI;
		}
		else
		{
			instruction.Op0Kind = OpKind.MemoryESDI;
		}
	}
}
