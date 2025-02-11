using System.Runtime.InteropServices;

namespace System.Windows.Media;

[StructLayout(LayoutKind.Explicit)]
internal struct MILCMD_PUSH_GUIDELINE_Y2
{
	[FieldOffset(0)]
	public double leadingCoordinate;

	[FieldOffset(8)]
	public double offsetToDrivenCoordinate;

	public MILCMD_PUSH_GUIDELINE_Y2(double leadingCoordinate, double offsetToDrivenCoordinate)
	{
		this.leadingCoordinate = leadingCoordinate;
		this.offsetToDrivenCoordinate = offsetToDrivenCoordinate;
	}
}
