using System.Runtime.InteropServices;

namespace System.Windows.Media;

[StructLayout(LayoutKind.Explicit)]
internal struct MILCMD_DRAW_VIDEO
{
	[FieldOffset(0)]
	public Rect rectangle;

	[FieldOffset(32)]
	public uint hPlayer;

	[FieldOffset(36)]
	private uint QuadWordPad0;

	public MILCMD_DRAW_VIDEO(uint hPlayer, Rect rectangle)
	{
		this.hPlayer = hPlayer;
		this.rectangle = rectangle;
		QuadWordPad0 = 0u;
	}
}
