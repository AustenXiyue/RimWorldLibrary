namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Mv_Gv : OpCodeHandlerModRM
{
	private readonly Code3 codes;

	public OpCodeHandler_Mv_Gv(Code code16, Code code32, Code code64)
	{
		codes = new Code3(code16, code32, code64);
	}

	public unsafe override void Decode(Decoder decoder, ref Instruction instruction)
	{
		nuint num = (nuint)decoder.state.operandSize;
		instruction.InternalSetCodeNoCheck((Code)codes.codes[num]);
		instruction.Op1Register = (Register)(((int)num << 4) + (int)(decoder.state.reg + decoder.state.zs.extraRegisterBase) + 21);
		if (decoder.state.mod == 3)
		{
			decoder.SetInvalidInstruction();
			return;
		}
		instruction.Op0Kind = OpKind.Memory;
		decoder.ReadOpMem(ref instruction);
	}
}
