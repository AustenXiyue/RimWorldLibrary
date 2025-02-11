using System.Runtime.InteropServices;

namespace System.Windows.Media;

[StructLayout(LayoutKind.Explicit)]
internal struct MILCMD_DRAW_ELLIPSE
{
	[FieldOffset(0)]
	public Point center;

	[FieldOffset(16)]
	public double radiusX;

	[FieldOffset(24)]
	public double radiusY;

	[FieldOffset(32)]
	public uint hBrush;

	[FieldOffset(36)]
	public uint hPen;

	public MILCMD_DRAW_ELLIPSE(uint hBrush, uint hPen, Point center, double radiusX, double radiusY)
	{
		this.hBrush = hBrush;
		this.hPen = hPen;
		this.center = center;
		this.radiusX = radiusX;
		this.radiusY = radiusY;
	}
}
