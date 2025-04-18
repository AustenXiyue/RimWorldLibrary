namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Jb2 : OpCodeHandler
{
	private readonly Code code16_16;

	private readonly Code code16_32;

	private readonly Code code16_64;

	private readonly Code code32_16;

	private readonly Code code32_32;

	private readonly Code code64_32;

	private readonly Code code64_64;

	public OpCodeHandler_Jb2(Code code16_16, Code code16_32, Code code16_64, Code code32_16, Code code32_32, Code code64_32, Code code64_64)
	{
		this.code16_16 = code16_16;
		this.code16_32 = code16_32;
		this.code16_64 = code16_64;
		this.code32_16 = code32_16;
		this.code32_32 = code32_32;
		this.code64_32 = code64_32;
		this.code64_64 = code64_64;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		decoder.state.zs.flags |= StateFlags.BranchImm8;
		if (decoder.is64bMode)
		{
			if ((decoder.options & DecoderOptions.AMD) == 0 || decoder.state.operandSize != 0)
			{
				if (decoder.state.addressSize == OpSize.Size64)
				{
					instruction.InternalSetCodeNoCheck(code64_64);
				}
				else
				{
					instruction.InternalSetCodeNoCheck(code64_32);
				}
				instruction.Op0Kind = OpKind.NearBranch64;
				instruction.NearBranch64 = (ulong)(sbyte)decoder.ReadByte() + decoder.GetCurrentInstructionPointer64();
			}
			else
			{
				if (decoder.state.addressSize == OpSize.Size64)
				{
					instruction.InternalSetCodeNoCheck(code16_64);
				}
				else
				{
					instruction.InternalSetCodeNoCheck(code16_32);
				}
				instruction.Op0Kind = OpKind.NearBranch16;
				instruction.InternalNearBranch16 = (ushort)((uint)(sbyte)decoder.ReadByte() + decoder.GetCurrentInstructionPointer32());
			}
		}
		else if (decoder.state.operandSize == OpSize.Size32)
		{
			if (decoder.state.addressSize == OpSize.Size32)
			{
				instruction.InternalSetCodeNoCheck(code32_32);
			}
			else
			{
				instruction.InternalSetCodeNoCheck(code32_16);
			}
			instruction.Op0Kind = OpKind.NearBranch32;
			instruction.NearBranch32 = (uint)(sbyte)decoder.ReadByte() + decoder.GetCurrentInstructionPointer32();
		}
		else
		{
			if (decoder.state.addressSize == OpSize.Size32)
			{
				instruction.InternalSetCodeNoCheck(code16_32);
			}
			else
			{
				instruction.InternalSetCodeNoCheck(code16_16);
			}
			instruction.Op0Kind = OpKind.NearBranch16;
			instruction.InternalNearBranch16 = (ushort)((uint)(sbyte)decoder.ReadByte() + decoder.GetCurrentInstructionPointer32());
		}
	}
}
