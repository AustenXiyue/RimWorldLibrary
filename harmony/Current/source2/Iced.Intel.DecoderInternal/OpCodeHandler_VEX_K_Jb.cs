namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VEX_K_Jb : OpCodeHandlerModRM
{
	private readonly Code code;

	public OpCodeHandler_VEX_K_Jb(Code code)
	{
		this.code = code;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		decoder.state.zs.flags |= StateFlags.BranchImm8;
		if (decoder.invalidCheckMask != 0 && decoder.state.vvvv > 7)
		{
			decoder.SetInvalidInstruction();
		}
		instruction.Op0Register = (Register)((decoder.state.vvvv & 7) + 173);
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op1Kind = OpKind.NearBranch64;
		instruction.NearBranch64 = (ulong)(sbyte)decoder.state.modrm + decoder.GetCurrentInstructionPointer64();
	}
}
