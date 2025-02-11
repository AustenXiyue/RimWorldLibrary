using System.Runtime.InteropServices;

namespace System.Windows.Media;

[StructLayout(LayoutKind.Explicit)]
internal struct MILCMD_DRAW_GEOMETRY
{
	[FieldOffset(0)]
	public uint hBrush;

	[FieldOffset(4)]
	public uint hPen;

	[FieldOffset(8)]
	public uint hGeometry;

	[FieldOffset(12)]
	private uint QuadWordPad0;

	public MILCMD_DRAW_GEOMETRY(uint hBrush, uint hPen, uint hGeometry)
	{
		this.hBrush = hBrush;
		this.hPen = hPen;
		this.hGeometry = hGeometry;
		QuadWordPad0 = 0u;
	}
}
