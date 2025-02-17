namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_EVEX_Gv_W_er : OpCodeHandlerModRM
{
	private readonly Register baseReg;

	private readonly Code codeW0;

	private readonly Code codeW1;

	private readonly TupleType tupleType;

	private readonly bool onlySAE;

	public OpCodeHandler_EVEX_Gv_W_er(Register baseReg, Code codeW0, Code codeW1, TupleType tupleType, bool onlySAE)
	{
		this.baseReg = baseReg;
		this.codeW0 = codeW0;
		this.codeW1 = codeW1;
		this.tupleType = tupleType;
		this.onlySAE = onlySAE;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if ((((uint)(decoder.state.zs.flags & StateFlags.z) | decoder.state.vvvv_invalidCheck | decoder.state.aaa | decoder.state.extraRegisterBaseEVEX) & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		if (((uint)decoder.state.zs.flags & decoder.is64bMode_and_W) != 0)
		{
			instruction.InternalSetCodeNoCheck(codeW1);
			instruction.Op0Register = (Register)(decoder.state.reg + decoder.state.zs.extraRegisterBase + 53);
		}
		else
		{
			instruction.InternalSetCodeNoCheck(codeW0);
			instruction.Op0Register = (Register)(decoder.state.reg + decoder.state.zs.extraRegisterBase + 37);
		}
		if (decoder.state.mod == 3)
		{
			instruction.Op1Register = (Register)((int)(decoder.state.rm + decoder.state.extraBaseRegisterBaseEVEX) + (int)baseReg);
			if ((decoder.state.zs.flags & StateFlags.b) != 0)
			{
				if (onlySAE)
				{
					instruction.InternalSetSuppressAllExceptions();
				}
				else
				{
					instruction.InternalRoundingControl = decoder.state.vectorLength + 1;
				}
			}
		}
		else
		{
			if (((uint)(decoder.state.zs.flags & StateFlags.b) & decoder.invalidCheckMask) != 0)
			{
				decoder.SetInvalidInstruction();
			}
			instruction.Op1Kind = OpKind.Memory;
			decoder.ReadOpMem(ref instruction, tupleType);
		}
	}
}
