using System.Runtime.InteropServices;
using System.Windows.Media.Composition;

namespace System.Windows.Media;

[StructLayout(LayoutKind.Explicit)]
internal struct MILCMD_PUSH_OPACITY_MASK
{
	[FieldOffset(0)]
	public MilRectF boundingBoxCacheLocalSpace;

	[FieldOffset(16)]
	public uint hOpacityMask;

	[FieldOffset(20)]
	private uint QuadWordPad0;

	public MILCMD_PUSH_OPACITY_MASK(uint hOpacityMask)
	{
		this.hOpacityMask = hOpacityMask;
		boundingBoxCacheLocalSpace = default(MilRectF);
		QuadWordPad0 = 0u;
	}
}
