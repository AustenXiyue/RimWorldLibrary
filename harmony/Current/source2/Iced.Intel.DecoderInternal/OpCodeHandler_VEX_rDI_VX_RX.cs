namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VEX_rDI_VX_RX : OpCodeHandlerModRM
{
	private readonly Register baseReg;

	private readonly Code code;

	public OpCodeHandler_VEX_rDI_VX_RX(Register baseReg, Code code)
	{
		this.baseReg = baseReg;
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if ((decoder.state.vvvv_invalidCheck & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		instruction.InternalSetCodeNoCheck(code);
		if (decoder.state.addressSize == OpSize.Size64)
		{
			instruction.Op0Kind = OpKind.MemorySegRDI;
		}
		else if (decoder.state.addressSize == OpSize.Size32)
		{
			instruction.Op0Kind = OpKind.MemorySegEDI;
		}
		else
		{
			instruction.Op0Kind = OpKind.MemorySegDI;
		}
		instruction.Op1Register = (Register)((int)(decoder.state.reg + decoder.state.zs.extraRegisterBase) + (int)baseReg);
		if (decoder.state.mod == 3)
		{
			instruction.Op2Register = (Register)((int)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase) + (int)baseReg);
		}
		else
		{
			decoder.SetInvalidInstruction();
		}
	}
}
