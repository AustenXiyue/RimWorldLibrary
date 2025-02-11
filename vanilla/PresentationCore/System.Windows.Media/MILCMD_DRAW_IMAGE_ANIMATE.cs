using System.Runtime.InteropServices;

namespace System.Windows.Media;

[StructLayout(LayoutKind.Explicit)]
internal struct MILCMD_DRAW_IMAGE_ANIMATE
{
	[FieldOffset(0)]
	public Rect rectangle;

	[FieldOffset(32)]
	public uint hImageSource;

	[FieldOffset(36)]
	public uint hRectangleAnimations;

	public MILCMD_DRAW_IMAGE_ANIMATE(uint hImageSource, Rect rectangle, uint hRectangleAnimations)
	{
		this.hImageSource = hImageSource;
		this.rectangle = rectangle;
		this.hRectangleAnimations = hRectangleAnimations;
	}
}
