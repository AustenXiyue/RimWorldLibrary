using System.Runtime.InteropServices;

namespace System.Windows.Media;

[StructLayout(LayoutKind.Explicit)]
internal struct MILCMD_PUSH_TRANSFORM
{
	[FieldOffset(0)]
	public uint hTransform;

	[FieldOffset(4)]
	private uint QuadWordPad0;

	public MILCMD_PUSH_TRANSFORM(uint hTransform)
	{
		this.hTransform = hTransform;
		QuadWordPad0 = 0u;
	}
}
