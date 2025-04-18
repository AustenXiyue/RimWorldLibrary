namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VEX_Hv_Ed_Id : OpCodeHandlerModRM
{
	private readonly Code code32;

	private readonly Code code64;

	public OpCodeHandler_VEX_Hv_Ed_Id(Code code32, Code code64)
	{
		this.code32 = code32;
		this.code64 = code64;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if (((uint)decoder.state.zs.flags & decoder.is64bMode_and_W) != 0)
		{
			instruction.InternalSetCodeNoCheck(code64);
			instruction.Op0Register = (Register)(decoder.state.vvvv + 53);
		}
		else
		{
			instruction.InternalSetCodeNoCheck(code32);
			instruction.Op0Register = (Register)(decoder.state.vvvv + 37);
		}
		if (decoder.state.mod == 3)
		{
			instruction.Op1Register = (Register)(decoder.state.rm + decoder.state.zs.extraBaseRegisterBase + 37);
		}
		else
		{
			instruction.Op1Kind = OpKind.Memory;
			decoder.ReadOpMem(ref instruction);
		}
		instruction.Op2Kind = OpKind.Immediate32;
		instruction.Immediate32 = decoder.ReadUInt32();
	}
}
