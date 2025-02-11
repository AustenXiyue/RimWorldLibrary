using System.Runtime.InteropServices;

namespace System.Windows.Media.Composition;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct MilQuaternionF
{
	internal float X;

	internal float Y;

	internal float Z;

	internal float W;
}
