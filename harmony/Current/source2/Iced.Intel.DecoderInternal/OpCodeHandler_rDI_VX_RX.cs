namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_rDI_VX_RX : OpCodeHandlerModRM
{
	private readonly Code code;

	public OpCodeHandler_rDI_VX_RX(Code code)
	{
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
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
		instruction.Op1Register = (Register)(decoder.state.reg + decoder.state.zs.extraRegisterBase + 77);
		if (decoder.state.mod == 3)
		{
			instruction.Op2Register = (Register)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase + 77);
		}
		else
		{
			decoder.SetInvalidInstruction();
		}
	}
}
