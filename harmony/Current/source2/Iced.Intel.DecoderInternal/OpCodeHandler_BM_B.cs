namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_BM_B : OpCodeHandlerModRM
{
	private readonly Code code32;

	private readonly Code code64;

	public OpCodeHandler_BM_B(Code code32, Code code64)
	{
		this.code32 = code32;
		this.code64 = code64;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if (decoder.state.reg > 3 || (decoder.state.zs.extraRegisterBase & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		if (decoder.is64bMode)
		{
			instruction.InternalSetCodeNoCheck(code64);
		}
		else
		{
			instruction.InternalSetCodeNoCheck(code32);
		}
		instruction.Op1Register = (Register)(decoder.state.reg + 181);
		if (decoder.state.mod == 3)
		{
			instruction.Op0Register = (Register)(decoder.state.rm + 181);
			if (decoder.state.rm > 3 || (decoder.state.zs.extraBaseRegisterBase & decoder.invalidCheckMask) != 0)
			{
				decoder.SetInvalidInstruction();
			}
		}
		else
		{
			instruction.Op0Kind = OpKind.Memory;
			decoder.ReadOpMem_MPX(ref instruction);
		}
	}
}
