namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Ev_REXW : OpCodeHandlerModRM
{
	private readonly Code code32;

	private readonly Code code64;

	private readonly uint flags;

	private readonly uint disallowReg;

	private readonly uint disallowMem;

	public OpCodeHandler_Ev_REXW(Code code32, Code code64, uint flags)
	{
		this.code32 = code32;
		this.code64 = code64;
		this.flags = flags;
		disallowReg = (((flags & 1) == 0) ? uint.MaxValue : 0u);
		disallowMem = (((flags & 2) == 0) ? uint.MaxValue : 0u);
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if ((decoder.state.zs.flags & StateFlags.W) != 0)
		{
			instruction.InternalSetCodeNoCheck(code64);
		}
		else
		{
			instruction.InternalSetCodeNoCheck(code32);
		}
		if ((((flags & 4) | (uint)(decoder.state.zs.flags & StateFlags.Has66)) & decoder.invalidCheckMask) == 32772)
		{
			decoder.SetInvalidInstruction();
		}
		if (decoder.state.mod == 3)
		{
			if ((decoder.state.zs.flags & StateFlags.W) != 0)
			{
				instruction.Op0Register = (Register)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase + 53);
			}
			else
			{
				instruction.Op0Register = (Register)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase + 37);
			}
			if ((disallowReg & decoder.invalidCheckMask) != 0)
			{
				decoder.SetInvalidInstruction();
			}
		}
		else
		{
			if ((disallowMem & decoder.invalidCheckMask) != 0)
			{
				decoder.SetInvalidInstruction();
			}
			instruction.Op0Kind = OpKind.Memory;
			decoder.ReadOpMem(ref instruction);
		}
	}
}
