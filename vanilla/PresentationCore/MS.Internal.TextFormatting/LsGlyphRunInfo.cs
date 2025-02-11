using MS.Internal.Text.TextInterface;

namespace MS.Internal.TextFormatting;

internal struct LsGlyphRunInfo
{
	public Plsrun plsrun;

	public unsafe char* pwch;

	public unsafe ushort* rggmap;

	public unsafe ushort* rgchprop;

	public int cwch;

	public int duChangeRight;

	public unsafe ushort* rggindex;

	public unsafe uint* rggprop;

	public unsafe int* rgduWidth;

	public unsafe GlyphOffset* rggoffset;

	public int cgindex;
}
