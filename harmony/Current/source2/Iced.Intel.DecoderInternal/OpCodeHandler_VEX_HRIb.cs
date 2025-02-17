namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VEX_HRIb : OpCodeHandlerModRM
{
	private readonly Register baseReg;

	private readonly Code code;

	public OpCodeHandler_VEX_HRIb(Register baseReg, Code code)
	{
		this.baseReg = baseReg;
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = (Register)((int)decoder.state.vvvv + (int)baseReg);
		if (decoder.state.mod == 3)
		{
			instruction.Op1Register = (Register)((int)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase) + (int)baseReg);
		}
		else
		{
			decoder.SetInvalidInstruction();
		}
		instruction.Op2Kind = OpKind.Immediate8;
		instruction.InternalImmediate8 = decoder.ReadByte();
	}
}
