namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Gb_Eb : OpCodeHandlerModRM
{
	private readonly Code code;

	public OpCodeHandler_Gb_Eb(Code code)
	{
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		uint num = decoder.state.reg + decoder.state.zs.extraRegisterBase;
		if ((decoder.state.zs.flags & StateFlags.HasRex) != 0 && num >= 4)
		{
			num += 4;
		}
		instruction.Op0Register = (Register)(num + 1);
		if (decoder.state.mod == 3)
		{
			num = decoder.state.rm + decoder.state.zs.extraBaseRegisterBase;
			if ((decoder.state.zs.flags & StateFlags.HasRex) != 0 && num >= 4)
			{
				num += 4;
			}
			instruction.Op1Register = (Register)(num + 1);
		}
		else
		{
			instruction.Op1Kind = OpKind.Memory;
			decoder.ReadOpMem(ref instruction);
		}
	}
}
