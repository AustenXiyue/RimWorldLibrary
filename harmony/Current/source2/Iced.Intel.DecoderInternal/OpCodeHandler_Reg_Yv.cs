namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Reg_Yv : OpCodeHandler
{
	private readonly Code3 codes;

	public OpCodeHandler_Reg_Yv(Code code16, Code code32, Code code64)
	{
		codes = new Code3(code16, code32, code64);
	}

	public unsafe override void Decode(Decoder decoder, ref Instruction instruction)
	{
		nuint num = (nuint)decoder.state.operandSize;
		instruction.InternalSetCodeNoCheck((Code)codes.codes[num]);
		instruction.Op0Register = (Register)(((int)num << 4) + 21);
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
