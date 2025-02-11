using System.Runtime.InteropServices;

namespace System.Windows.Media;

[StructLayout(LayoutKind.Explicit)]
internal struct MILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE
{
	[FieldOffset(0)]
	public Rect rectangle;

	[FieldOffset(32)]
	public double radiusX;

	[FieldOffset(40)]
	public double radiusY;

	[FieldOffset(48)]
	public uint hBrush;

	[FieldOffset(52)]
	public uint hPen;

	[FieldOffset(56)]
	public uint hRectangleAnimations;

	[FieldOffset(60)]
	public uint hRadiusXAnimations;

	[FieldOffset(64)]
	public uint hRadiusYAnimations;

	[FieldOffset(68)]
	private uint QuadWordPad0;

	public MILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE(uint hBrush, uint hPen, Rect rectangle, uint hRectangleAnimations, double radiusX, uint hRadiusXAnimations, double radiusY, uint hRadiusYAnimations)
	{
		this.hBrush = hBrush;
		this.hPen = hPen;
		this.rectangle = rectangle;
		this.hRectangleAnimations = hRectangleAnimations;
		this.radiusX = radiusX;
		this.hRadiusXAnimations = hRadiusXAnimations;
		this.radiusY = radiusY;
		this.hRadiusYAnimations = hRadiusYAnimations;
		QuadWordPad0 = 0u;
	}
}
