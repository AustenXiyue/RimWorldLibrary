using System.Runtime.InteropServices;

namespace System.Windows.Media;

[StructLayout(LayoutKind.Explicit)]
internal struct MILCMD_DRAW_RECTANGLE_ANIMATE
{
	[FieldOffset(0)]
	public Rect rectangle;

	[FieldOffset(32)]
	public uint hBrush;

	[FieldOffset(36)]
	public uint hPen;

	[FieldOffset(40)]
	public uint hRectangleAnimations;

	[FieldOffset(44)]
	private uint QuadWordPad0;

	public MILCMD_DRAW_RECTANGLE_ANIMATE(uint hBrush, uint hPen, Rect rectangle, uint hRectangleAnimations)
	{
		this.hBrush = hBrush;
		this.hPen = hPen;
		this.rectangle = rectangle;
		this.hRectangleAnimations = hRectangleAnimations;
		QuadWordPad0 = 0u;
	}
}
