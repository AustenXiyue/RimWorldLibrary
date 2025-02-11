using System.Runtime.InteropServices;

namespace System.Windows.Media;

[StructLayout(LayoutKind.Explicit)]
internal struct MILCMD_DRAW_GLYPH_RUN
{
	[FieldOffset(0)]
	public uint hForegroundBrush;

	[FieldOffset(4)]
	public uint hGlyphRun;

	public MILCMD_DRAW_GLYPH_RUN(uint hForegroundBrush, uint hGlyphRun)
	{
		this.hForegroundBrush = hForegroundBrush;
		this.hGlyphRun = hGlyphRun;
	}
}
