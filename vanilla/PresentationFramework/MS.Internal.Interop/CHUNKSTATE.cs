using System;

namespace MS.Internal.Interop;

[Flags]
internal enum CHUNKSTATE
{
	CHUNK_TEXT = 1,
	CHUNK_VALUE = 2,
	CHUNK_FILTER_OWNED_VALUE = 4
}
