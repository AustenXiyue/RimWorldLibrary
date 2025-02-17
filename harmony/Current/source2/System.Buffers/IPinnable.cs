namespace System.Buffers;

internal interface IPinnable
{
	MemoryHandle Pin(int elementIndex);

	void Unpin();
}
