using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal struct BLOB
{
	public uint cbSize;

	public nint pBlobData;
}
