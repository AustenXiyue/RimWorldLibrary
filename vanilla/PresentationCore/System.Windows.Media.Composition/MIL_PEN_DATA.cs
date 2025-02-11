using System.Runtime.InteropServices;

namespace System.Windows.Media.Composition;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct MIL_PEN_DATA
{
	internal double Thickness;

	internal double MiterLimit;

	internal double DashOffset;

	internal MIL_PEN_CAP StartLineCap;

	internal MIL_PEN_CAP EndLineCap;

	internal MIL_PEN_CAP DashCap;

	internal MIL_PEN_JOIN LineJoin;

	internal uint DashArraySize;
}
