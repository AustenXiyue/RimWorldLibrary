using System.Runtime.InteropServices;

namespace System.Windows.Media;

[StructLayout(LayoutKind.Explicit)]
internal struct MILCMD_DRAW_IMAGE
{
	[FieldOffset(0)]
	public Rect rectangle;

	[FieldOffset(32)]
	public uint hImageSource;

	[FieldOffset(36)]
	private uint QuadWordPad0;

	public MILCMD_DRAW_IMAGE(uint hImageSource, Rect rectangle)
	{
		this.hImageSource = hImageSource;
		this.rectangle = rectangle;
		QuadWordPad0 = 0u;
	}
}
