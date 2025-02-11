using System.Runtime.InteropServices;

namespace System.Windows.Media.Composition;

[StructLayout(LayoutKind.Explicit)]
internal struct MIL_PATHGEOMETRY
{
	[FieldOffset(0)]
	internal uint Size;

	[FieldOffset(4)]
	internal MilPathGeometryFlags Flags;

	[FieldOffset(8)]
	internal MilRectD Bounds;

	[FieldOffset(40)]
	internal uint FigureCount;

	[FieldOffset(44)]
	internal uint ForcePacking;
}
