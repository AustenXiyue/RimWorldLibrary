using System.Runtime.InteropServices;

namespace System.Windows.Media;

[StructLayout(LayoutKind.Explicit)]
internal struct MILCMD_DRAW_RECTANGLE
{
	[FieldOffset(0)]
	public Rect rectangle;

	[FieldOffset(32)]
	public uint hBrush;

	[FieldOffset(36)]
	public uint hPen;

	public MILCMD_DRAW_RECTANGLE(uint hBrush, uint hPen, Rect rectangle)
	{
		this.hBrush = hBrush;
		this.hPen = hPen;
		this.rectangle = rectangle;
	}
}
