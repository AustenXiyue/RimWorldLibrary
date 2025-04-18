namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_MemBx : OpCodeHandler
{
	private readonly Code code;

	public OpCodeHandler_MemBx(Code code)
	{
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		instruction.InternalMemoryIndex = Register.AL;
		instruction.Op0Kind = OpKind.Memory;
		if (decoder.state.addressSize == OpSize.Size64)
		{
			instruction.InternalMemoryBase = Register.RBX;
		}
		else if (decoder.state.addressSize == OpSize.Size32)
		{
			instruction.InternalMemoryBase = Register.EBX;
		}
		else
		{
			instruction.InternalMemoryBase = Register.BX;
		}
	}
}
