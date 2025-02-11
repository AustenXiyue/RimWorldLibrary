using System.Runtime.InteropServices;

namespace System.Windows.Media;

[StructLayout(LayoutKind.Explicit)]
internal struct MILCMD_DRAW_VIDEO_ANIMATE
{
	[FieldOffset(0)]
	public Rect rectangle;

	[FieldOffset(32)]
	public uint hPlayer;

	[FieldOffset(36)]
	public uint hRectangleAnimations;

	public MILCMD_DRAW_VIDEO_ANIMATE(uint hPlayer, Rect rectangle, uint hRectangleAnimations)
	{
		this.hPlayer = hPlayer;
		this.rectangle = rectangle;
		this.hRectangleAnimations = hRectangleAnimations;
	}
}
