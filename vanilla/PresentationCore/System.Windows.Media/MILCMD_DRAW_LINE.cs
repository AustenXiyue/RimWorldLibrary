using System.Runtime.InteropServices;

namespace System.Windows.Media;

[StructLayout(LayoutKind.Explicit)]
internal struct MILCMD_DRAW_LINE
{
	[FieldOffset(0)]
	public Point point0;

	[FieldOffset(16)]
	public Point point1;

	[FieldOffset(32)]
	public uint hPen;

	[FieldOffset(36)]
	private uint QuadWordPad0;

	public MILCMD_DRAW_LINE(uint hPen, Point point0, Point point1)
	{
		this.hPen = hPen;
		this.point0 = point0;
		this.point1 = point1;
		QuadWordPad0 = 0u;
	}
}
