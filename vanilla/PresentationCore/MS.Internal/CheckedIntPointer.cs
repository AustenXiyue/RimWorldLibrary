using MS.Internal.FontCache;

namespace MS.Internal;

internal struct CheckedIntPointer
{
	private CheckedPointer _checkedPointer;

	internal unsafe CheckedIntPointer(int* pointer, int length)
	{
		_checkedPointer = new CheckedPointer(pointer, length * 4);
	}

	internal unsafe int* Probe(int offset, int length)
	{
		return (int*)_checkedPointer.Probe(offset * 4, length * 4);
	}
}
