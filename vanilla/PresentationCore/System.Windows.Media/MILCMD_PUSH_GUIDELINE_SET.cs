using System.Runtime.InteropServices;

namespace System.Windows.Media;

[StructLayout(LayoutKind.Explicit)]
internal struct MILCMD_PUSH_GUIDELINE_SET
{
	[FieldOffset(0)]
	public uint hGuidelines;

	[FieldOffset(4)]
	private uint QuadWordPad0;

	public MILCMD_PUSH_GUIDELINE_SET(uint hGuidelines)
	{
		this.hGuidelines = hGuidelines;
		QuadWordPad0 = 0u;
	}
}
