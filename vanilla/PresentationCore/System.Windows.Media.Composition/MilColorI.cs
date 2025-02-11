using System.Runtime.InteropServices;

namespace System.Windows.Media.Composition;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct MilColorI
{
	internal int r;

	internal int g;

	internal int b;

	internal int a;
}
