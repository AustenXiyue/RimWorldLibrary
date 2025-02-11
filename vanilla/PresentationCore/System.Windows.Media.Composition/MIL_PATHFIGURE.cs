using System.Runtime.InteropServices;

namespace System.Windows.Media.Composition;

[StructLayout(LayoutKind.Explicit)]
internal struct MIL_PATHFIGURE
{
	[FieldOffset(0)]
	internal uint BackSize;

	[FieldOffset(4)]
	internal MilPathFigureFlags Flags;

	[FieldOffset(8)]
	internal uint Count;

	[FieldOffset(12)]
	internal uint Size;

	[FieldOffset(16)]
	internal Point StartPoint;

	[FieldOffset(32)]
	internal uint OffsetToLastSegment;

	[FieldOffset(36)]
	internal uint ForcePacking;
}
