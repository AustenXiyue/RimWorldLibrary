namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Yb_Xb : OpCodeHandler
{
	private readonly Code code;

	public OpCodeHandler_Yb_Xb(Code code)
	{
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		if (decoder.state.addressSize == OpSize.Size64)
		{
			instruction.Op0Kind = OpKind.MemoryESRDI;
			instruction.Op1Kind = OpKind.MemorySegRSI;
		}
		else if (decoder.state.addressSize == OpSize.Size32)
		{
			instruction.Op0Kind = OpKind.MemoryESEDI;
			instruction.Op1Kind = OpKind.MemorySegESI;
		}
		else
		{
			instruction.Op0Kind = OpKind.MemoryESDI;
			instruction.Op1Kind = OpKind.MemorySegSI;
		}
	}
}
