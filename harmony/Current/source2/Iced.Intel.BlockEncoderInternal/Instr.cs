using System;

namespace Iced.Intel.BlockEncoderInternal;

internal abstract class Instr
{
	public readonly Block Block;

	public uint Size;

	public ulong IP;

	public readonly ulong OrigIP;

	public bool Done;

	protected const uint CallOrJmpPointerDataInstructionSize64 = 6u;

	protected Instr(Block block, ulong origIp)
	{
		OrigIP = origIp;
		Block = block;
	}

	public abstract void Initialize(BlockEncoder blockEncoder);

	public abstract bool Optimize(ulong gained);

	public abstract string? TryEncode(Encoder encoder, out ConstantOffsets constantOffsets, out bool isOriginalInstruction);

	protected static string CreateErrorMessage(string errorMessage, in Instruction instruction)
	{
		return $"{errorMessage} : 0x{instruction.IP:X} {instruction.ToString()}";
	}

	public static Instr Create(BlockEncoder blockEncoder, Block block, in Instruction instruction)
	{
		switch (instruction.Code)
		{
		case Code.Jo_rel8_16:
		case Code.Jo_rel8_32:
		case Code.Jo_rel8_64:
		case Code.Jno_rel8_16:
		case Code.Jno_rel8_32:
		case Code.Jno_rel8_64:
		case Code.Jb_rel8_16:
		case Code.Jb_rel8_32:
		case Code.Jb_rel8_64:
		case Code.Jae_rel8_16:
		case Code.Jae_rel8_32:
		case Code.Jae_rel8_64:
		case Code.Je_rel8_16:
		case Code.Je_rel8_32:
		case Code.Je_rel8_64:
		case Code.Jne_rel8_16:
		case Code.Jne_rel8_32:
		case Code.Jne_rel8_64:
		case Code.Jbe_rel8_16:
		case Code.Jbe_rel8_32:
		case Code.Jbe_rel8_64:
		case Code.Ja_rel8_16:
		case Code.Ja_rel8_32:
		case Code.Ja_rel8_64:
		case Code.Js_rel8_16:
		case Code.Js_rel8_32:
		case Code.Js_rel8_64:
		case Code.Jns_rel8_16:
		case Code.Jns_rel8_32:
		case Code.Jns_rel8_64:
		case Code.Jp_rel8_16:
		case Code.Jp_rel8_32:
		case Code.Jp_rel8_64:
		case Code.Jnp_rel8_16:
		case Code.Jnp_rel8_32:
		case Code.Jnp_rel8_64:
		case Code.Jl_rel8_16:
		case Code.Jl_rel8_32:
		case Code.Jl_rel8_64:
		case Code.Jge_rel8_16:
		case Code.Jge_rel8_32:
		case Code.Jge_rel8_64:
		case Code.Jle_rel8_16:
		case Code.Jle_rel8_32:
		case Code.Jle_rel8_64:
		case Code.Jg_rel8_16:
		case Code.Jg_rel8_32:
		case Code.Jg_rel8_64:
		case Code.Jo_rel16:
		case Code.Jo_rel32_32:
		case Code.Jo_rel32_64:
		case Code.Jno_rel16:
		case Code.Jno_rel32_32:
		case Code.Jno_rel32_64:
		case Code.Jb_rel16:
		case Code.Jb_rel32_32:
		case Code.Jb_rel32_64:
		case Code.Jae_rel16:
		case Code.Jae_rel32_32:
		case Code.Jae_rel32_64:
		case Code.Je_rel16:
		case Code.Je_rel32_32:
		case Code.Je_rel32_64:
		case Code.Jne_rel16:
		case Code.Jne_rel32_32:
		case Code.Jne_rel32_64:
		case Code.Jbe_rel16:
		case Code.Jbe_rel32_32:
		case Code.Jbe_rel32_64:
		case Code.Ja_rel16:
		case Code.Ja_rel32_32:
		case Code.Ja_rel32_64:
		case Code.Js_rel16:
		case Code.Js_rel32_32:
		case Code.Js_rel32_64:
		case Code.Jns_rel16:
		case Code.Jns_rel32_32:
		case Code.Jns_rel32_64:
		case Code.Jp_rel16:
		case Code.Jp_rel32_32:
		case Code.Jp_rel32_64:
		case Code.Jnp_rel16:
		case Code.Jnp_rel32_32:
		case Code.Jnp_rel32_64:
		case Code.Jl_rel16:
		case Code.Jl_rel32_32:
		case Code.Jl_rel32_64:
		case Code.Jge_rel16:
		case Code.Jge_rel32_32:
		case Code.Jge_rel32_64:
		case Code.Jle_rel16:
		case Code.Jle_rel32_32:
		case Code.Jle_rel32_64:
		case Code.Jg_rel16:
		case Code.Jg_rel32_32:
		case Code.Jg_rel32_64:
		case Code.VEX_KNC_Jkzd_kr_rel8_64:
		case Code.VEX_KNC_Jknzd_kr_rel8_64:
		case Code.VEX_KNC_Jkzd_kr_rel32_64:
		case Code.VEX_KNC_Jknzd_kr_rel32_64:
			return new JccInstr(blockEncoder, block, in instruction);
		case Code.Loopne_rel8_16_CX:
		case Code.Loopne_rel8_32_CX:
		case Code.Loopne_rel8_16_ECX:
		case Code.Loopne_rel8_32_ECX:
		case Code.Loopne_rel8_64_ECX:
		case Code.Loopne_rel8_16_RCX:
		case Code.Loopne_rel8_64_RCX:
		case Code.Loope_rel8_16_CX:
		case Code.Loope_rel8_32_CX:
		case Code.Loope_rel8_16_ECX:
		case Code.Loope_rel8_32_ECX:
		case Code.Loope_rel8_64_ECX:
		case Code.Loope_rel8_16_RCX:
		case Code.Loope_rel8_64_RCX:
		case Code.Loop_rel8_16_CX:
		case Code.Loop_rel8_32_CX:
		case Code.Loop_rel8_16_ECX:
		case Code.Loop_rel8_32_ECX:
		case Code.Loop_rel8_64_ECX:
		case Code.Loop_rel8_16_RCX:
		case Code.Loop_rel8_64_RCX:
		case Code.Jcxz_rel8_16:
		case Code.Jcxz_rel8_32:
		case Code.Jecxz_rel8_16:
		case Code.Jecxz_rel8_32:
		case Code.Jecxz_rel8_64:
		case Code.Jrcxz_rel8_16:
		case Code.Jrcxz_rel8_64:
			return new SimpleBranchInstr(blockEncoder, block, in instruction);
		case Code.Call_rel16:
		case Code.Call_rel32_32:
		case Code.Call_rel32_64:
			return new CallInstr(blockEncoder, block, in instruction);
		case Code.Jmp_rel16:
		case Code.Jmp_rel32_32:
		case Code.Jmp_rel32_64:
		case Code.Jmp_rel8_16:
		case Code.Jmp_rel8_32:
		case Code.Jmp_rel8_64:
			return new JmpInstr(blockEncoder, block, in instruction);
		case Code.Xbegin_rel16:
		case Code.Xbegin_rel32:
			return new XbeginInstr(blockEncoder, block, in instruction);
		default:
			if (blockEncoder.Bitness == 64)
			{
				int opCount = instruction.OpCount;
				for (int i = 0; i < opCount; i++)
				{
					if (instruction.GetOpKind(i) == OpKind.Memory)
					{
						if (!instruction.IsIPRelativeMemoryOperand)
						{
							break;
						}
						return new IpRelMemOpInstr(blockEncoder, block, in instruction);
					}
				}
			}
			return new SimpleInstr(blockEncoder, block, in instruction);
		}
	}

	protected string? EncodeBranchToPointerData(Encoder encoder, bool isCall, ulong ip, BlockData pointerData, out uint size, uint minSize)
	{
		if (minSize > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("minSize");
		}
		Instruction instruction = default(Instruction);
		instruction.Op0Kind = OpKind.Memory;
		instruction.MemoryDisplSize = encoder.Bitness / 8;
		if (encoder.Bitness == 64)
		{
			instruction.InternalSetCodeNoCheck(isCall ? Code.Call_rm64 : Code.Jmp_rm64);
			instruction.MemoryBase = Register.RIP;
			ulong num = ip + 6;
			long num2 = (long)(pointerData.Address - num);
			if (int.MinValue > num2 || num2 > int.MaxValue)
			{
				size = 0u;
				return "Block is too big";
			}
			instruction.MemoryDisplacement64 = pointerData.Address;
			RelocKind relocKind = RelocKind.Offset64;
			if (!encoder.TryEncode(in instruction, ip, out size, out string errorMessage))
			{
				return errorMessage;
			}
			if (Block.CanAddRelocInfos && relocKind != 0)
			{
				ConstantOffsets constantOffsets = encoder.GetConstantOffsets();
				if (!constantOffsets.HasDisplacement)
				{
					return "Internal error: no displ";
				}
				Block.AddRelocInfo(new RelocInfo(relocKind, IP + constantOffsets.DisplacementOffset));
			}
			while (size < minSize)
			{
				size++;
				Block.CodeWriter.WriteByte(144);
			}
			return null;
		}
		throw new InvalidOperationException();
	}

	protected static long CorrectDiff(bool inBlock, long diff, ulong gained)
	{
		if (inBlock && diff >= 0)
		{
			return diff - (long)gained;
		}
		return diff;
	}
}
