namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Ev_1 : OpCodeHandlerModRM
{
	private readonly Code3 codes;

	public OpCodeHandler_Ev_1(Code code16, Code code32, Code code64)
	{
		codes = new Code3(code16, code32, code64);
	}

	public unsafe override void Decode(Decoder decoder, ref Instruction instruction)
	{
		nuint num = (nuint)decoder.state.operandSize;
		instruction.InternalSetCodeNoCheck((Code)codes.codes[num]);
		instruction.Op1Kind = OpKind.Immediate8;
		instruction.InternalImmediate8 = 1u;
		decoder.state.zs.flags |= StateFlags.NoImm;
		if (decoder.state.mod == 3)
		{
			instruction.Op0Register = (Register)(((int)num << 4) + (int)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase) + 21);
			return;
		}
		instruction.Op0Kind = OpKind.Memory;
		decoder.ReadOpMem(ref instruction);
	}
}
