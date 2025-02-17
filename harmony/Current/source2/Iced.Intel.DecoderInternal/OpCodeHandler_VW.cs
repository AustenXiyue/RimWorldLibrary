namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VW : OpCodeHandlerModRM
{
	private readonly Code codeR;

	private readonly Code codeM;

	public OpCodeHandler_VW(Code codeR, Code codeM)
	{
		this.codeR = codeR;
		this.codeM = codeM;
	}

	public OpCodeHandler_VW(Code code)
	{
		codeR = code;
		codeM = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.Op0Register = (Register)(decoder.state.reg + decoder.state.zs.extraRegisterBase + 77);
		if (decoder.state.mod == 3)
		{
			instruction.InternalSetCodeNoCheck(codeR);
			instruction.Op1Register = (Register)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase + 77);
		}
		else
		{
			instruction.InternalSetCodeNoCheck(codeM);
			instruction.Op1Kind = OpKind.Memory;
			decoder.ReadOpMem(ref instruction);
		}
	}
}
