using System.Runtime.InteropServices;

namespace System.Windows.Media.Composition;

[StructLayout(LayoutKind.Explicit)]
internal struct MIL_SEGMENT_BEZIER
{
	[FieldOffset(0)]
	internal MIL_SEGMENT_TYPE Type;

	[FieldOffset(4)]
	internal MILCoreSegFlags Flags;

	[FieldOffset(8)]
	internal uint BackSize;

	[FieldOffset(12)]
	internal uint ForcePacking;

	[FieldOffset(16)]
	internal Point Point1;

	[FieldOffset(32)]
	internal Point Point2;

	[FieldOffset(48)]
	internal Point Point3;
}
