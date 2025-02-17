namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Xb_Yb : OpCodeHandler
{
	private readonly Code code;

	public OpCodeHandler_Xb_Yb(Code code)
	{
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		if (decoder.state.addressSize == OpSize.Size64)
		{
			instruction.Op0Kind = OpKind.MemorySegRSI;
			instruction.Op1Kind = OpKind.MemoryESRDI;
		}
		else if (decoder.state.addressSize == OpSize.Size32)
		{
			instruction.Op0Kind = OpKind.MemorySegESI;
			instruction.Op1Kind = OpKind.MemoryESEDI;
		}
		else
		{
			instruction.Op0Kind = OpKind.MemorySegSI;
			instruction.Op1Kind = OpKind.MemoryESDI;
		}
	}
}
