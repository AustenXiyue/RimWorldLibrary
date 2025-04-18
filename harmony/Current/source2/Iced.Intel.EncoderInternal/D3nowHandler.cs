namespace Iced.Intel.EncoderInternal;

internal sealed class D3nowHandler : OpCodeHandler
{
	private static readonly Op[] operands = new Op[2]
	{
		new OpModRM_reg(Register.MM0, Register.MM7),
		new OpModRM_rm(Register.MM0, Register.MM7)
	};

	private readonly uint immediate;

	public D3nowHandler(EncFlags2 encFlags2, EncFlags3 encFlags3)
		: base((EncFlags2)(((ulong)encFlags2 & 0xFFFFFFFFFFFF0000uL) | 0xF), encFlags3, isSpecialInstr: false, null, operands)
	{
		immediate = OpCodeHandler.GetOpCode(encFlags2);
	}

	public override void Encode(Encoder encoder, in Instruction instruction)
	{
		encoder.WritePrefixes(in instruction);
		encoder.WriteByteInternal(15u);
		encoder.ImmSize = ImmSize.Size1OpCode;
		encoder.Immediate = immediate;
	}
}
