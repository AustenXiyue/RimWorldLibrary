using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal struct PROPSPEC
{
	internal uint propType;

	internal PROPSPECunion union;
}
