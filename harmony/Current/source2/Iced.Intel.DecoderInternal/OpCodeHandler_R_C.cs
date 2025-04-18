namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_R_C : OpCodeHandlerModRM
{
	private readonly Code code32;

	private readonly Code code64;

	private readonly Register baseReg;

	public OpCodeHandler_R_C(Code code32, Code code64, Register baseReg)
	{
		this.code32 = code32;
		this.code64 = code64;
		this.baseReg = baseReg;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if (decoder.is64bMode)
		{
			instruction.InternalSetCodeNoCheck(code64);
			instruction.Op0Register = (Register)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase + 53);
		}
		else
		{
			instruction.InternalSetCodeNoCheck(code32);
			instruction.Op0Register = (Register)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase + 37);
		}
		uint num = decoder.state.zs.extraRegisterBase;
		if (baseReg == Register.CR0 && instruction.HasLockPrefix && (decoder.options & DecoderOptions.AMD) != 0)
		{
			if ((num & decoder.invalidCheckMask) != 0)
			{
				decoder.SetInvalidInstruction();
			}
			num = 8u;
			instruction.InternalClearHasLockPrefix();
			decoder.state.zs.flags &= ~StateFlags.Lock;
		}
		int num2 = (int)(decoder.state.reg + num);
		if (decoder.invalidCheckMask != 0)
		{
			if (baseReg == Register.CR0)
			{
				if (num2 == 1 || (num2 != 8 && num2 >= 5))
				{
					decoder.SetInvalidInstruction();
				}
			}
			else if (baseReg == Register.DR0 && num2 > 7)
			{
				decoder.SetInvalidInstruction();
			}
		}
		instruction.Op1Register = num2 + baseReg;
	}
}
