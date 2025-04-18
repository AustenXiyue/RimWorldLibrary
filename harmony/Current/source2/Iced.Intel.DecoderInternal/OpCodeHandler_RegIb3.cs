namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_RegIb3 : OpCodeHandler
{
	private readonly int index;

	private readonly Register[] withRexPrefix;

	private static readonly Register[] s_withRexPrefix = new Register[16]
	{
		Register.AL,
		Register.CL,
		Register.DL,
		Register.BL,
		Register.SPL,
		Register.BPL,
		Register.SIL,
		Register.DIL,
		Register.R8L,
		Register.R9L,
		Register.R10L,
		Register.R11L,
		Register.R12L,
		Register.R13L,
		Register.R14L,
		Register.R15L
	};

	public OpCodeHandler_RegIb3(int index)
	{
		this.index = index;
		withRexPrefix = s_withRexPrefix;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		Register op0Register = (((decoder.state.zs.flags & StateFlags.HasRex) == 0) ? ((Register)(index + 1)) : withRexPrefix[index + (int)decoder.state.zs.extraBaseRegisterBase]);
		instruction.InternalSetCodeNoCheck(Code.Mov_r8_imm8);
		instruction.Op0Register = op0Register;
		instruction.Op1Kind = OpKind.Immediate8;
		instruction.InternalImmediate8 = decoder.ReadByte();
	}
}
