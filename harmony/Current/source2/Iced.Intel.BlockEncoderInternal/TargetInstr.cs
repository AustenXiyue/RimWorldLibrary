namespace Iced.Intel.BlockEncoderInternal;

internal readonly struct TargetInstr
{
	private readonly Instr instruction;

	private readonly ulong address;

	public TargetInstr(Instr instruction)
	{
		this.instruction = instruction;
		address = 0uL;
	}

	public TargetInstr(ulong address)
	{
		instruction = null;
		this.address = address;
	}

	public bool IsInBlock(Block block)
	{
		return instruction?.Block == block;
	}

	public ulong GetAddress()
	{
		return instruction?.IP ?? address;
	}
}
