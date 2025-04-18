namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Jx : OpCodeHandler
{
	private readonly Code code16;

	private readonly Code code32;

	private readonly Code code64;

	public OpCodeHandler_Jx(Code code16, Code code32, Code code64)
	{
		this.code16 = code16;
		this.code32 = code32;
		this.code64 = code64;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		decoder.state.zs.flags |= StateFlags.Xbegin;
		if (decoder.is64bMode)
		{
			if (decoder.state.operandSize == OpSize.Size32)
			{
				instruction.InternalSetCodeNoCheck(code32);
				instruction.Op0Kind = OpKind.NearBranch64;
				instruction.NearBranch64 = (ulong)(int)decoder.ReadUInt32() + decoder.GetCurrentInstructionPointer64();
			}
			else if (decoder.state.operandSize == OpSize.Size64)
			{
				instruction.InternalSetCodeNoCheck(code64);
				instruction.Op0Kind = OpKind.NearBranch64;
				instruction.NearBranch64 = (ulong)(int)decoder.ReadUInt32() + decoder.GetCurrentInstructionPointer64();
			}
			else
			{
				instruction.InternalSetCodeNoCheck(code16);
				instruction.Op0Kind = OpKind.NearBranch64;
				instruction.NearBranch64 = (ulong)(short)decoder.ReadUInt16() + decoder.GetCurrentInstructionPointer64();
			}
		}
		else if (decoder.state.operandSize == OpSize.Size32)
		{
			instruction.InternalSetCodeNoCheck(code32);
			instruction.Op0Kind = OpKind.NearBranch32;
			instruction.NearBranch32 = decoder.ReadUInt32() + decoder.GetCurrentInstructionPointer32();
		}
		else
		{
			instruction.InternalSetCodeNoCheck(code16);
			instruction.Op0Kind = OpKind.NearBranch32;
			instruction.NearBranch32 = (uint)(short)decoder.ReadUInt16() + decoder.GetCurrentInstructionPointer32();
		}
	}
}
