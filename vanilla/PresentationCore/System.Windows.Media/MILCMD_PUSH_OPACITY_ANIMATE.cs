using System.Runtime.InteropServices;

namespace System.Windows.Media;

[StructLayout(LayoutKind.Explicit)]
internal struct MILCMD_PUSH_OPACITY_ANIMATE
{
	[FieldOffset(0)]
	public double opacity;

	[FieldOffset(8)]
	public uint hOpacityAnimations;

	[FieldOffset(12)]
	private uint QuadWordPad0;

	public MILCMD_PUSH_OPACITY_ANIMATE(double opacity, uint hOpacityAnimations)
	{
		this.opacity = opacity;
		this.hOpacityAnimations = hOpacityAnimations;
		QuadWordPad0 = 0u;
	}
}
