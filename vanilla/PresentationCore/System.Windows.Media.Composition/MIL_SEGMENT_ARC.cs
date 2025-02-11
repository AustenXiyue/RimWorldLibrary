using System.Runtime.InteropServices;

namespace System.Windows.Media.Composition;

[StructLayout(LayoutKind.Explicit)]
internal struct MIL_SEGMENT_ARC
{
	[FieldOffset(0)]
	internal MIL_SEGMENT_TYPE Type;

	[FieldOffset(4)]
	internal MILCoreSegFlags Flags;

	[FieldOffset(8)]
	internal uint BackSize;

	[FieldOffset(12)]
	internal uint LargeArc;

	[FieldOffset(16)]
	internal Point Point;

	[FieldOffset(32)]
	internal Size Size;

	[FieldOffset(48)]
	internal double XRotation;

	[FieldOffset(56)]
	internal uint Sweep;

	[FieldOffset(60)]
	internal uint ForcePacking;
}
