namespace Iced.Intel;

internal readonly struct MemoryOperand
{
	public readonly Register SegmentPrefix;

	public readonly Register Base;

	public readonly Register Index;

	public readonly int Scale;

	public readonly long Displacement;

	public readonly int DisplSize;

	public readonly bool IsBroadcast;

	public MemoryOperand(Register @base, Register index, int scale, long displacement, int displSize, bool isBroadcast, Register segmentPrefix)
	{
		SegmentPrefix = segmentPrefix;
		Base = @base;
		Index = index;
		Scale = scale;
		Displacement = displacement;
		DisplSize = displSize;
		IsBroadcast = isBroadcast;
	}

	public MemoryOperand(Register @base, Register index, int scale, bool isBroadcast, Register segmentPrefix)
	{
		SegmentPrefix = segmentPrefix;
		Base = @base;
		Index = index;
		Scale = scale;
		Displacement = 0L;
		DisplSize = 0;
		IsBroadcast = isBroadcast;
	}

	public MemoryOperand(Register @base, long displacement, int displSize, bool isBroadcast, Register segmentPrefix)
	{
		SegmentPrefix = segmentPrefix;
		Base = @base;
		Index = Register.None;
		Scale = 1;
		Displacement = displacement;
		DisplSize = displSize;
		IsBroadcast = isBroadcast;
	}

	public MemoryOperand(Register index, int scale, long displacement, int displSize, bool isBroadcast, Register segmentPrefix)
	{
		SegmentPrefix = segmentPrefix;
		Base = Register.None;
		Index = index;
		Scale = scale;
		Displacement = displacement;
		DisplSize = displSize;
		IsBroadcast = isBroadcast;
	}

	public MemoryOperand(Register @base, long displacement, bool isBroadcast, Register segmentPrefix)
	{
		SegmentPrefix = segmentPrefix;
		Base = @base;
		Index = Register.None;
		Scale = 1;
		Displacement = displacement;
		DisplSize = 1;
		IsBroadcast = isBroadcast;
	}

	public MemoryOperand(Register @base, Register index, int scale, long displacement, int displSize)
	{
		SegmentPrefix = Register.None;
		Base = @base;
		Index = index;
		Scale = scale;
		Displacement = displacement;
		DisplSize = displSize;
		IsBroadcast = false;
	}

	public MemoryOperand(Register @base, Register index, int scale)
	{
		SegmentPrefix = Register.None;
		Base = @base;
		Index = index;
		Scale = scale;
		Displacement = 0L;
		DisplSize = 0;
		IsBroadcast = false;
	}

	public MemoryOperand(Register @base, Register index)
	{
		SegmentPrefix = Register.None;
		Base = @base;
		Index = index;
		Scale = 1;
		Displacement = 0L;
		DisplSize = 0;
		IsBroadcast = false;
	}

	public MemoryOperand(Register @base, long displacement, int displSize)
	{
		SegmentPrefix = Register.None;
		Base = @base;
		Index = Register.None;
		Scale = 1;
		Displacement = displacement;
		DisplSize = displSize;
		IsBroadcast = false;
	}

	public MemoryOperand(Register index, int scale, long displacement, int displSize)
	{
		SegmentPrefix = Register.None;
		Base = Register.None;
		Index = index;
		Scale = scale;
		Displacement = displacement;
		DisplSize = displSize;
		IsBroadcast = false;
	}

	public MemoryOperand(Register @base, long displacement)
	{
		SegmentPrefix = Register.None;
		Base = @base;
		Index = Register.None;
		Scale = 1;
		Displacement = displacement;
		DisplSize = 1;
		IsBroadcast = false;
	}

	public MemoryOperand(Register @base)
	{
		SegmentPrefix = Register.None;
		Base = @base;
		Index = Register.None;
		Scale = 1;
		Displacement = 0L;
		DisplSize = 0;
		IsBroadcast = false;
	}

	public MemoryOperand(ulong displacement, int displSize)
	{
		SegmentPrefix = Register.None;
		Base = Register.None;
		Index = Register.None;
		Scale = 1;
		Displacement = (long)displacement;
		DisplSize = displSize;
		IsBroadcast = false;
	}
}
