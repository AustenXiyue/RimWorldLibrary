using System.Runtime.InteropServices;

namespace Standard;

[StructLayout(LayoutKind.Explicit)]
internal struct WTA_OPTIONS
{
	public const uint Size = 8u;

	[FieldOffset(0)]
	public WTNCA dwFlags;

	[FieldOffset(4)]
	public WTNCA dwMask;
}
