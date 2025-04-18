namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_MIB_B : OpCodeHandlerModRM
{
	private readonly Code code;

	public OpCodeHandler_MIB_B(Code code)
	{
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if (decoder.state.reg > 3 || (decoder.state.zs.extraRegisterBase & decoder.invalidCheckMask) != 0)
		{
			decoder.SetInvalidInstruction();
		}
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op1Register = (Register)(decoder.state.reg + 181);
		instruction.Op0Kind = OpKind.Memory;
		decoder.ReadOpMem_MPX(ref instruction);
		if (decoder.invalidCheckMask != 0 && instruction.MemoryBase == Register.RIP)
		{
			decoder.SetInvalidInstruction();
		}
	}
}
