using MS.Internal.Interop;

namespace MS.Internal.IO.Packaging;

internal interface IManagedFilter
{
	IFILTER_FLAGS Init(IFILTER_INIT grfFlags, ManagedFullPropSpec[] aAttributes);

	ManagedChunk GetChunk();

	string GetText(int bufferCharacterCount);

	object GetValue();
}
