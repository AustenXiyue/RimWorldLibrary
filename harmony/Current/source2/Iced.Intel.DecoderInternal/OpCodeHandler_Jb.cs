namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Jb : OpCodeHandler
{
	private readonly Code code16;

	private readonly Code code32;

	private readonly Code code64;

	public OpCodeHandler_Jb(Code code16, Code code32, Code code64)
	{
		this.code16 = code16;
		this.code32 = code32;
		this.code64 = code64;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		decoder.state.zs.flags |= StateFlags.BranchImm8;
		if (decoder.is64bMode)
		{
			if ((decoder.options & DecoderOptions.AMD) == 0 || decoder.state.operandSize != 0)
			{
				instruction.InternalSetCodeNoCheck(code64);
				instruction.Op0Kind = OpKind.NearBranch64;
				instruction.NearBranch64 = (ulong)(sbyte)decoder.ReadByte() + decoder.GetCurrentInstructionPointer64();
			}
			else
			{
				instruction.InternalSetCodeNoCheck(code16);
				instruction.Op0Kind = OpKind.NearBranch16;
				instruction.InternalNearBranch16 = (ushort)((uint)(sbyte)decoder.ReadByte() + decoder.GetCurrentInstructionPointer32());
			}
		}
		else if (decoder.state.operandSize != 0)
		{
			instruction.InternalSetCodeNoCheck(code32);
			instruction.Op0Kind = OpKind.NearBranch32;
			instruction.NearBranch32 = (uint)(sbyte)decoder.ReadByte() + decoder.GetCurrentInstructionPointer32();
		}
		else
		{
			instruction.InternalSetCodeNoCheck(code16);
			instruction.Op0Kind = OpKind.NearBranch16;
			instruction.InternalNearBranch16 = (ushort)((uint)(sbyte)decoder.ReadByte() + decoder.GetCurrentInstructionPointer32());
		}
	}
}
