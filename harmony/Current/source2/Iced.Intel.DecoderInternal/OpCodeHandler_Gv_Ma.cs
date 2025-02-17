namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Gv_Ma : OpCodeHandlerModRM
{
	private readonly Code code16;

	private readonly Code code32;

	public OpCodeHandler_Gv_Ma(Code code16, Code code32)
	{
		this.code16 = code16;
		this.code32 = code32;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if (decoder.state.operandSize != 0)
		{
			instruction.InternalSetCodeNoCheck(code32);
			instruction.Op0Register = (Register)(decoder.state.reg + decoder.state.zs.extraRegisterBase + 37);
		}
		else
		{
			instruction.InternalSetCodeNoCheck(code16);
			instruction.Op0Register = (Register)(decoder.state.reg + decoder.state.zs.extraRegisterBase + 21);
		}
		instruction.Op1Kind = OpKind.Memory;
		decoder.ReadOpMem(ref instruction);
	}
}
