using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal struct BSTRBLOB
{
	public uint cbSize;

	public nint pData;
}
