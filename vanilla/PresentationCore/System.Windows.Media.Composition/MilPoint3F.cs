using System.Runtime.InteropServices;

namespace System.Windows.Media.Composition;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct MilPoint3F
{
	internal float X;

	internal float Y;

	internal float Z;
}
