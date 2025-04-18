namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Reg_Ov : OpCodeHandler
{
	private readonly Code3 codes;

	public OpCodeHandler_Reg_Ov(Code code16, Code code32, Code code64)
	{
		codes = new Code3(code16, code32, code64);
	}

	public unsafe override void Decode(Decoder decoder, ref Instruction instruction)
	{
		decoder.displIndex = decoder.state.zs.instructionLength;
		instruction.Op1Kind = OpKind.Memory;
		nuint num = (nuint)decoder.state.operandSize;
		instruction.InternalSetCodeNoCheck((Code)codes.codes[num]);
		instruction.Op0Register = (Register)(((int)num << 4) + 21);
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
