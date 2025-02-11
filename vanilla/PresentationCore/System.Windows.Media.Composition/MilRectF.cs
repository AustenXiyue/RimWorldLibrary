using System.Runtime.InteropServices;

namespace System.Windows.Media.Composition;

[StructLayout(LayoutKind.Explicit)]
internal struct MilRectF
{
	[FieldOffset(0)]
	internal float _left;

	[FieldOffset(4)]
	internal float _top;

	[FieldOffset(8)]
	internal float _right;

	[FieldOffset(12)]
	internal float _bottom;
}
