namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VEX_VK_RK_Ib : OpCodeHandlerModRM
{
	private readonly Code code;

	public OpCodeHandler_VEX_VK_RK_Ib(Code code)
	{
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if (((decoder.state.vvvv_invalidCheck | decoder.state.zs.extraRegisterBase) & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = (Register)(decoder.state.reg + 173);
		if (decoder.state.mod == 3)
		{
			instruction.Op1Register = (Register)(decoder.state.rm + 173);
		}
		else
		{
			decoder.SetInvalidInstruction();
		}
		instruction.Op2Kind = OpKind.Immediate8;
		instruction.InternalImmediate8 = decoder.ReadByte();
	}
}
