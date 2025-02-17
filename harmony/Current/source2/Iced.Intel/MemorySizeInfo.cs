namespace Iced.Intel;

internal readonly struct MemorySizeInfo
{
	private readonly ushort size;

	private readonly ushort elementSize;

	private readonly byte memorySize;

	private readonly byte elementType;

	private readonly bool isSigned;

	private readonly bool isBroadcast;

	public MemorySize MemorySize => (MemorySize)memorySize;

	public int Size => size;

	public int ElementSize => elementSize;

	public MemorySize ElementType => (MemorySize)elementType;

	public bool IsSigned => isSigned;

	public bool IsBroadcast => isBroadcast;

	public bool IsPacked => elementSize < size;

	public int ElementCount
	{
		get
		{
			if (elementSize != size)
			{
				return size / elementSize;
			}
			return 1;
		}
	}

	public MemorySizeInfo(MemorySize memorySize, int size, int elementSize, MemorySize elementType, bool isSigned, bool isBroadcast)
	{
		if (size < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_size();
		}
		if (elementSize < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_elementSize();
		}
		if (elementSize > size)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_elementSize();
		}
		this.memorySize = (byte)memorySize;
		this.size = (ushort)size;
		this.elementSize = (ushort)elementSize;
		this.elementType = (byte)elementType;
		this.isSigned = isSigned;
		this.isBroadcast = isBroadcast;
	}
}
