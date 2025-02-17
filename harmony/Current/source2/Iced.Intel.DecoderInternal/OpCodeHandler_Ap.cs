namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Ap : OpCodeHandler
{
	private readonly Code code16;

	private readonly Code code32;

	public OpCodeHandler_Ap(Code code16, Code code32)
	{
		this.code16 = code16;
		this.code32 = code32;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if (decoder.state.operandSize != 0)
		{
			instruction.InternalSetCodeNoCheck(code32);
		}
		else
		{
			instruction.InternalSetCodeNoCheck(code16);
		}
		if (decoder.state.operandSize != 0)
		{
			instruction.Op0Kind = OpKind.FarBranch32;
			instruction.FarBranch32 = decoder.ReadUInt32();
		}
		else
		{
			instruction.Op0Kind = OpKind.FarBranch16;
			instruction.InternalFarBranch16 = decoder.ReadUInt16();
		}
		instruction.InternalFarBranchSelector = decoder.ReadUInt16();
	}
}
