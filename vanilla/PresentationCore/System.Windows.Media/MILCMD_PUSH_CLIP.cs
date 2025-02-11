using System.Runtime.InteropServices;

namespace System.Windows.Media;

[StructLayout(LayoutKind.Explicit)]
internal struct MILCMD_PUSH_CLIP
{
	[FieldOffset(0)]
	public uint hClipGeometry;

	[FieldOffset(4)]
	private uint QuadWordPad0;

	public MILCMD_PUSH_CLIP(uint hClipGeometry)
	{
		this.hClipGeometry = hClipGeometry;
		QuadWordPad0 = 0u;
	}
}
