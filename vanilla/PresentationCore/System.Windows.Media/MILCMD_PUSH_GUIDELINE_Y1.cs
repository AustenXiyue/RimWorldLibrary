using System.Runtime.InteropServices;

namespace System.Windows.Media;

[StructLayout(LayoutKind.Explicit)]
internal struct MILCMD_PUSH_GUIDELINE_Y1
{
	[FieldOffset(0)]
	public double coordinate;

	public MILCMD_PUSH_GUIDELINE_Y1(double coordinate)
	{
		this.coordinate = coordinate;
	}
}
