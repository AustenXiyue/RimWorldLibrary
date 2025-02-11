using System.Runtime.InteropServices;

namespace System.Windows.Media;

[StructLayout(LayoutKind.Explicit)]
internal struct MILCMD_DRAW_ROUNDED_RECTANGLE
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

	public MILCMD_DRAW_ROUNDED_RECTANGLE(uint hBrush, uint hPen, Rect rectangle, double radiusX, double radiusY)
	{
		this.hBrush = hBrush;
		this.hPen = hPen;
		this.rectangle = rectangle;
		this.radiusX = radiusX;
		this.radiusY = radiusY;
	}
}
