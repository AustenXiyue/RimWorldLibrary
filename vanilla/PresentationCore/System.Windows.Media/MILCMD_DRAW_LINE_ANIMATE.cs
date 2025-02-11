using System.Runtime.InteropServices;

namespace System.Windows.Media;

[StructLayout(LayoutKind.Explicit)]
internal struct MILCMD_DRAW_LINE_ANIMATE
{
	[FieldOffset(0)]
	public Point point0;

	[FieldOffset(16)]
	public Point point1;

	[FieldOffset(32)]
	public uint hPen;

	[FieldOffset(36)]
	public uint hPoint0Animations;

	[FieldOffset(40)]
	public uint hPoint1Animations;

	[FieldOffset(44)]
	private uint QuadWordPad0;

	public MILCMD_DRAW_LINE_ANIMATE(uint hPen, Point point0, uint hPoint0Animations, Point point1, uint hPoint1Animations)
	{
		this.hPen = hPen;
		this.point0 = point0;
		this.hPoint0Animations = hPoint0Animations;
		this.point1 = point1;
		this.hPoint1Animations = hPoint1Animations;
		QuadWordPad0 = 0u;
	}
}
