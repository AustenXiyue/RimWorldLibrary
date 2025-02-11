using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal struct PROPVARIANT
{
	internal VARTYPE vt;

	internal ushort wReserved1;

	internal ushort wReserved2;

	internal ushort wReserved3;

	internal PropVariantUnion union;
}
