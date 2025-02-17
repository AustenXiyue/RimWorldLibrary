namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_P_Q_Ib : OpCodeHandlerModRM
{
	private readonly Code code;

	public OpCodeHandler_P_Q_Ib(Code code)
	{
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = (Register)(decoder.state.reg + 225);
		if (decoder.state.mod == 3)
		{
			instruction.Op1Register = (Register)(decoder.state.rm + 225);
		}
		else
		{
			instruction.Op1Kind = OpKind.Memory;
			decoder.ReadOpMem(ref instruction);
		}
		instruction.Op2Kind = OpKind.Immediate8;
		instruction.InternalImmediate8 = decoder.ReadByte();
	}
}
