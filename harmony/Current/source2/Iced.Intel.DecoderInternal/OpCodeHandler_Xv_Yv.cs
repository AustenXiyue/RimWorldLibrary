namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Xv_Yv : OpCodeHandler
{
	private readonly Code3 codes;

	public OpCodeHandler_Xv_Yv(Code code16, Code code32, Code code64)
	{
		codes = new Code3(code16, code32, code64);
	}

	public unsafe override void Decode(Decoder decoder, ref Instruction instruction)
	{
		nuint num = (nuint)decoder.state.operandSize;
		instruction.InternalSetCodeNoCheck((Code)codes.codes[num]);
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
