namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VEX_K_Jz : OpCodeHandlerModRM
{
	private readonly Code code;

	public OpCodeHandler_VEX_K_Jz(Code code)
	{
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if (decoder.invalidCheckMask != 0 && decoder.state.vvvv > 7)
		{
			decoder.SetInvalidInstruction();
		}
		instruction.Op0Register = (Register)((decoder.state.vvvv & 7) + 173);
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op1Kind = OpKind.NearBranch64;
		uint num = decoder.state.modrm | (decoder.ReadByte() << 8) | (decoder.ReadByte() << 16) | (decoder.ReadByte() << 24);
		instruction.NearBranch64 = (ulong)(int)num + decoder.GetCurrentInstructionPointer64();
	}
}
