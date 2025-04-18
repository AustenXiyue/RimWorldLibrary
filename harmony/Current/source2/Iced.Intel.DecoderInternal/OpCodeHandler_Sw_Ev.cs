namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Sw_Ev : OpCodeHandlerModRM
{
	private readonly Code3 codes;

	public OpCodeHandler_Sw_Ev(Code code16, Code code32, Code code64)
	{
		codes = new Code3(code16, code32, code64);
	}

	public unsafe override void Decode(Decoder decoder, ref Instruction instruction)
	{
		nuint num = (nuint)decoder.state.operandSize;
		instruction.InternalSetCodeNoCheck((Code)codes.codes[num]);
		Register register = decoder.ReadOpSegReg();
		if (decoder.invalidCheckMask != 0 && register == Register.CS)
		{
			decoder.SetInvalidInstruction();
		}
		instruction.Op0Register = register;
		if (decoder.state.mod == 3)
		{
			instruction.Op1Register = (Register)(((int)num << 4) + (int)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase) + 21);
			return;
		}
		instruction.Op1Kind = OpKind.Memory;
		decoder.ReadOpMem(ref instruction);
	}
}
