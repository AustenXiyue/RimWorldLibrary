using System;

namespace MS.Internal.Interop;

[Flags]
internal enum SICHINT : uint
{
	DISPLAY = 0u,
	ALLFIELDS = 0x80000000u,
	CANONICAL = 0x10000000u,
	TEST_FILESYSPATH_IF_NOT_EQUAL = 0x20000000u
}
