using System;

namespace Iced.Intel.BlockEncoderInternal;

internal sealed class IpRelMemOpInstr : Instr
{
	private enum InstrKind : byte
	{
		Unchanged,
		Rip,
		Eip,
		Long,
		Uninitialized
	}

	private Instruction instruction;

	private InstrKind instrKind;

	private readonly byte eipInstructionSize;

	private readonly byte ripInstructionSize;

	private TargetInstr targetInstr;

	public IpRelMemOpInstr(BlockEncoder blockEncoder, Block block, in Instruction instruction)
		: base(block, instruction.IP)
	{
		this.instruction = instruction;
		instrKind = InstrKind.Uninitialized;
		Instruction instruction2 = instruction;
		instruction2.MemoryBase = Register.RIP;
		instruction2.MemoryDisplacement64 = 0uL;
		ripInstructionSize = (byte)blockEncoder.GetInstructionSize(in instruction2, instruction2.IPRelativeMemoryAddress);
		instruction2.MemoryBase = Register.EIP;
		eipInstructionSize = (byte)blockEncoder.GetInstructionSize(in instruction2, instruction2.IPRelativeMemoryAddress);
		Size = eipInstructionSize;
	}

	public override void Initialize(BlockEncoder blockEncoder)
	{
		targetInstr = blockEncoder.GetTarget(instruction.IPRelativeMemoryAddress);
	}

	public override bool Optimize(ulong gained)
	{
		return TryOptimize(gained);
	}

	private bool TryOptimize(ulong gained)
	{
		if (instrKind == InstrKind.Unchanged || instrKind == InstrKind.Rip || instrKind == InstrKind.Eip)
		{
			Done = true;
			return false;
		}
		bool flag = targetInstr.IsInBlock(Block);
		ulong address = targetInstr.GetAddress();
		if (!flag)
		{
			ulong num = IP + ripInstructionSize;
			long diff = (long)(address - num);
			diff = Instr.CorrectDiff(targetInstr.IsInBlock(Block), diff, gained);
			flag = int.MinValue <= diff && diff <= int.MaxValue;
		}
		if (flag)
		{
			Size = ripInstructionSize;
			instrKind = InstrKind.Rip;
			Done = true;
			return true;
		}
		if (address <= uint.MaxValue)
		{
			Size = eipInstructionSize;
			instrKind = InstrKind.Eip;
			Done = true;
			return true;
		}
		instrKind = InstrKind.Long;
		return false;
	}

	public override string? TryEncode(Encoder encoder, out ConstantOffsets constantOffsets, out bool isOriginalInstruction)
	{
		switch (instrKind)
		{
		case InstrKind.Unchanged:
		case InstrKind.Rip:
		case InstrKind.Eip:
		{
			isOriginalInstruction = true;
			if (instrKind == InstrKind.Rip)
			{
				instruction.MemoryBase = Register.RIP;
			}
			else if (instrKind == InstrKind.Eip)
			{
				instruction.MemoryBase = Register.EIP;
			}
			ulong address = targetInstr.GetAddress();
			instruction.MemoryDisplacement64 = address;
			encoder.TryEncode(in instruction, IP, out uint _, out string errorMessage);
			if (instruction.IPRelativeMemoryAddress != ((instruction.MemoryBase == Register.EIP) ? ((uint)address) : address))
			{
				errorMessage = "Invalid IP relative address";
			}
			if (errorMessage != null)
			{
				constantOffsets = default(ConstantOffsets);
				return Instr.CreateErrorMessage(errorMessage, in instruction);
			}
			constantOffsets = encoder.GetConstantOffsets();
			return null;
		}
		case InstrKind.Long:
			isOriginalInstruction = false;
			constantOffsets = default(ConstantOffsets);
			return "IP relative memory operand is too far away and isn't currently supported. Try to allocate memory close to the original instruction (+/-2GB).";
		default:
			throw new InvalidOperationException();
		}
	}
}
