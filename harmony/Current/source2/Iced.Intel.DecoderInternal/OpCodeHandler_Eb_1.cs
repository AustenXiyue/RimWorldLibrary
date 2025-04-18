namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Eb_1 : OpCodeHandlerModRM
{
	private readonly Code code;

	public OpCodeHandler_Eb_1(Code code)
	{
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op1Kind = OpKind.Immediate8;
		instruction.InternalImmediate8 = 1u;
		decoder.state.zs.flags |= StateFlags.NoImm;
		if (decoder.state.mod == 3)
		{
			uint num = decoder.state.rm + decoder.state.zs.extraBaseRegisterBase;
			if ((decoder.state.zs.flags & StateFlags.HasRex) != 0 && num >= 4)
			{
				num += 4;
			}
			instruction.Op0Register = (Register)(num + 1);
		}
		else
		{
			instruction.Op0Kind = OpKind.Memory;
			decoder.ReadOpMem(ref instruction);
		}
	}
}
