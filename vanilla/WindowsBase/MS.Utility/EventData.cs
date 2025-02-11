using System.Runtime.InteropServices;

namespace MS.Utility;

[StructLayout(LayoutKind.Explicit, Size = 16)]
internal struct EventData
{
	[FieldOffset(0)]
	internal ulong Ptr;

	[FieldOffset(8)]
	internal uint Size;

	[FieldOffset(12)]
	internal uint Reserved;
}
