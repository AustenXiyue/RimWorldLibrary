namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Gv_Ev_Iz : OpCodeHandlerModRM
{
	private readonly Code3 codes;

	public OpCodeHandler_Gv_Ev_Iz(Code code16, Code code32, Code code64)
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
			instruction.Op1Register = (Register)(((int)num << 4) + (int)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase) + 21);
		}
		else
		{
			instruction.Op1Kind = OpKind.Memory;
			decoder.ReadOpMem(ref instruction);
		}
		if ((int)num == 1)
		{
			instruction.Op2Kind = OpKind.Immediate32;
			instruction.Immediate32 = decoder.ReadUInt32();
		}
		else if ((int)num == 2)
		{
			instruction.Op2Kind = OpKind.Immediate32to64;
			instruction.Immediate32 = decoder.ReadUInt32();
		}
		else
		{
			instruction.Op2Kind = OpKind.Immediate16;
			instruction.InternalImmediate16 = decoder.ReadUInt16();
		}
	}
}
