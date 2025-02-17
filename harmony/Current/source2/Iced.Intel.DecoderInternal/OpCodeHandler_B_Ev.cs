namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_B_Ev : OpCodeHandlerModRM
{
	private readonly Code code32;

	private readonly Code code64;

	private readonly uint ripRelMask;

	public OpCodeHandler_B_Ev(Code code32, Code code64, bool supportsRipRel)
	{
		this.code32 = code32;
		this.code64 = code64;
		ripRelMask = ((!supportsRipRel) ? uint.MaxValue : 0u);
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if (decoder.state.reg > 3 || (decoder.state.zs.extraRegisterBase & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
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
		instruction.Op0Register = (Register)(decoder.state.reg + 181);
		if (decoder.state.mod == 3)
		{
			instruction.Op1Register = (Register)((int)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase) + (int)register);
			return;
		}
		instruction.Op1Kind = OpKind.Memory;
		decoder.ReadOpMem_MPX(ref instruction);
		if ((ripRelMask & decoder.invalidCheckMask) != 0 && instruction.MemoryBase == Register.RIP)
		{
			decoder.SetInvalidInstruction();
		}
	}
}
