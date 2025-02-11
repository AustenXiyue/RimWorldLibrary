using System.Runtime.InteropServices;

namespace MS.Internal.TextFormatting;

internal struct LsTbd
{
	public LsKTab lskt;

	public int ur;

	[MarshalAs(UnmanagedType.U2)]
	public char wchTabLeader;

	[MarshalAs(UnmanagedType.U2)]
	public char wchCharTab;
}
