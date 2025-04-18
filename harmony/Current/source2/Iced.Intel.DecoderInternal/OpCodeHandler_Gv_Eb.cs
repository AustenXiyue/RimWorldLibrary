namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Gv_Eb : OpCodeHandlerModRM
{
	private readonly Code3 codes;

	public OpCodeHandler_Gv_Eb(Code code16, Code code32, Code code64)
	{
		codes = new Code3(code16, code32, code64);
	}

	public unsafe override void Decode(Decoder decoder, ref Instruction instruction)
	{
		nuint num = (nuint)decoder.state.operandSize;
		instruction.InternalSetCodeNoCheck((Code)codes.codes[num]);
		instruction.Op0Register = (Register)(((int)num << 4) + (int)(decoder.state.reg + decoder.state.zs.extraRegisterBase) + 21);
		if (decoder.state.mod == 3)
		{
			uint num2 = decoder.state.rm + decoder.state.zs.extraBaseRegisterBase;
			if ((decoder.state.zs.flags & StateFlags.HasRex) != 0 && num2 >= 4)
			{
				num2 += 4;
			}
			instruction.Op1Register = (Register)(num2 + 1);
		}
		else
		{
			instruction.Op1Kind = OpKind.Memory;
			decoder.ReadOpMem(ref instruction);
		}
	}
}
