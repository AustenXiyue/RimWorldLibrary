using MS.Internal.FontCache;

namespace MS.Internal;

internal struct CheckedCharPointer
{
	private CheckedPointer _checkedPointer;

	internal unsafe CheckedCharPointer(char* pointer, int length)
	{
		_checkedPointer = new CheckedPointer(pointer, length * 2);
	}

	internal unsafe char* Probe(int offset, int length)
	{
		return (char*)_checkedPointer.Probe(offset * 2, length * 2);
	}
}
