using System.Runtime.InteropServices;

namespace MS.Internal.TextFormatting;

internal struct LsNeighborInfo
{
	public uint fNeighborIsPresent;

	public uint fNeighborIsText;

	public Plsrun plsrun;

	[MarshalAs(UnmanagedType.U2)]
	public char wch;

	public uint fGlyphBased;

	public ushort chprop;

	public ushort gindex;

	public uint gprop;
}
