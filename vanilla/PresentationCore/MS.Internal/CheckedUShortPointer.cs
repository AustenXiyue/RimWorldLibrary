using MS.Internal.FontCache;

namespace MS.Internal;

internal struct CheckedUShortPointer
{
	private CheckedPointer _checkedPointer;

	internal unsafe CheckedUShortPointer(ushort* pointer, int length)
	{
		_checkedPointer = new CheckedPointer(pointer, length * 2);
	}

	internal unsafe ushort* Probe(int offset, int length)
	{
		return (ushort*)_checkedPointer.Probe(offset * 2, length * 2);
	}
}
