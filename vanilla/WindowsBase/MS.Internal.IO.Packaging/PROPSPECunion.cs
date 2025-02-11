using System.Runtime.InteropServices;
using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging;

[StructLayout(LayoutKind.Explicit)]
[MS.Internal.WindowsBase.FriendAccessAllowed]
internal struct PROPSPECunion
{
	[FieldOffset(0)]
	internal nint name;

	[FieldOffset(0)]
	internal uint propId;
}
