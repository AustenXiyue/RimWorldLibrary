using System.Runtime.InteropServices;

namespace System.Windows.Media.Composition;

[StructLayout(LayoutKind.Explicit)]
internal struct MIL_SEGMENT
{
	[FieldOffset(0)]
	internal MIL_SEGMENT_TYPE Type;

	[FieldOffset(4)]
	internal MILCoreSegFlags Flags;

	[FieldOffset(8)]
	internal uint BackSize;
}
