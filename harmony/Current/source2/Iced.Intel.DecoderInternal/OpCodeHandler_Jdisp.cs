namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Jdisp : OpCodeHandler
{
	private readonly Code code16;

	private readonly Code code32;

	public OpCodeHandler_Jdisp(Code code16, Code code32)
	{
		this.code16 = code16;
		this.code32 = code32;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if (decoder.state.operandSize != 0)
		{
			instruction.InternalSetCodeNoCheck(code32);
			instruction.Op0Kind = OpKind.NearBranch32;
			instruction.NearBranch32 = decoder.ReadUInt32();
		}
		else
		{
			instruction.InternalSetCodeNoCheck(code16);
			instruction.Op0Kind = OpKind.NearBranch16;
			instruction.InternalNearBranch16 = decoder.ReadUInt16();
		}
	}
}
