namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Reg_Ob : OpCodeHandler
{
	private readonly Code code;

	private readonly Register reg;

	public OpCodeHandler_Reg_Ob(Code code, Register reg)
	{
		this.code = code;
		this.reg = reg;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.InternalSetCodeNoCheck(code);
		instruction.Op0Register = reg;
		decoder.displIndex = decoder.state.zs.instructionLength;
		instruction.Op1Kind = OpKind.Memory;
		if (decoder.state.addressSize == OpSize.Size64)
		{
			instruction.InternalSetMemoryDisplSize(4u);
			decoder.state.zs.flags |= StateFlags.Addr64;
			instruction.MemoryDisplacement64 = decoder.ReadUInt64();
		}
		else if (decoder.state.addressSize == OpSize.Size32)
		{
			instruction.InternalSetMemoryDisplSize(3u);
			instruction.MemoryDisplacement64 = decoder.ReadUInt32();
		}
		else
		{
			instruction.InternalSetMemoryDisplSize(2u);
			instruction.MemoryDisplacement64 = decoder.ReadUInt16();
		}
	}
}
