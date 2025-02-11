using System.Runtime.InteropServices;

namespace System.Windows.Media;

[StructLayout(LayoutKind.Explicit)]
internal struct MILCMD_PUSH_OPACITY
{
	[FieldOffset(0)]
	public double opacity;

	public MILCMD_PUSH_OPACITY(double opacity)
	{
		this.opacity = opacity;
	}
}
