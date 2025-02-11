using System.Runtime.InteropServices;

namespace System.Windows.Media;

[StructLayout(LayoutKind.Explicit)]
internal struct MILCMD_DRAW_DRAWING
{
	[FieldOffset(0)]
	public uint hDrawing;

	[FieldOffset(4)]
	private uint QuadWordPad0;

	public MILCMD_DRAW_DRAWING(uint hDrawing)
	{
		this.hDrawing = hDrawing;
		QuadWordPad0 = 0u;
	}
}
