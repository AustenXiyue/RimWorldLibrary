using System.Runtime.InteropServices;

namespace MS.Internal.TextFormatting;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct LscbkRedefined
{
	public FetchRunRedefined pfnFetchRunRedefined;

	public GetGlyphsRedefined pfnGetGlyphsRedefined;

	public FetchLineProps pfnFetchLineProps;
}
