using System.Runtime.InteropServices;

namespace System.Windows.Media.Composition;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct MilMatrix3x2D
{
	internal double S_11;

	internal double S_12;

	internal double S_21;

	internal double S_22;

	internal double DX;

	internal double DY;
}
