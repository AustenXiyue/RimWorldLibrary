namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Gv_Ev_32_64 : OpCodeHandlerModRM
{
	private readonly Code code32;

	private readonly Code code64;

	private readonly uint disallowReg;

	private readonly uint disallowMem;

	public OpCodeHandler_Gv_Ev_32_64(Code code32, Code code64, bool allowReg, bool allowMem)
	{
		this.code32 = code32;
		this.code64 = code64;
		disallowMem = ((!allowMem) ? uint.MaxValue : 0u);
		disallowReg = ((!allowReg) ? uint.MaxValue : 0u);
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		Register register;
		if (decoder.is64bMode)
		{
			instruction.InternalSetCodeNoCheck(code64);
			register = Register.RAX;
		}
		else
		{
			instruction.InternalSetCodeNoCheck(code32);
			register = Register.EAX;
		}
		instruction.Op0Register = (Register)((int)(decoder.state.reg + decoder.state.zs.extraRegisterBase) + (int)register);
		if (decoder.state.mod == 3)
		{
			instruction.Op1Register = (Register)((int)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase) + (int)register);
			if ((disallowReg & decoder.invalidCheckMask) != 0)
			{
				decoder.SetInvalidInstruction();
			}
			return;
		}
		if ((disallowMem & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		instruction.Op1Kind = OpKind.Memory;
		decoder.ReadOpMem(ref instruction);
	}
}
