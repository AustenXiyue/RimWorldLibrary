using System.Runtime.InteropServices;

namespace MS.Internal.TextFormatting;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct LsHyph
{
	public LsKysr kysr;

	public char wchYsr;

	public char wchYsr2;

	public LsHyphenQuality lshq;
}
