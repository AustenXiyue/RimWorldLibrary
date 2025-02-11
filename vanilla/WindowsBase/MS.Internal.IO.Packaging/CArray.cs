using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal struct CArray
{
	public uint cElems;

	public nint pElems;
}
