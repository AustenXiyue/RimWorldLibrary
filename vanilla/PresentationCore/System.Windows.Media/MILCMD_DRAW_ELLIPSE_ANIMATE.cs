using System.Runtime.InteropServices;

namespace System.Windows.Media;

[StructLayout(LayoutKind.Explicit)]
internal struct MILCMD_DRAW_ELLIPSE_ANIMATE
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

	[FieldOffset(40)]
	public uint hCenterAnimations;

	[FieldOffset(44)]
	public uint hRadiusXAnimations;

	[FieldOffset(48)]
	public uint hRadiusYAnimations;

	[FieldOffset(52)]
	private uint QuadWordPad0;

	public MILCMD_DRAW_ELLIPSE_ANIMATE(uint hBrush, uint hPen, Point center, uint hCenterAnimations, double radiusX, uint hRadiusXAnimations, double radiusY, uint hRadiusYAnimations)
	{
		this.hBrush = hBrush;
		this.hPen = hPen;
		this.center = center;
		this.hCenterAnimations = hCenterAnimations;
		this.radiusX = radiusX;
		this.hRadiusXAnimations = hRadiusXAnimations;
		this.radiusY = radiusY;
		this.hRadiusYAnimations = hRadiusYAnimations;
		QuadWordPad0 = 0u;
	}
}
