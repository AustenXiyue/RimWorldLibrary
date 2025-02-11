namespace System.Windows.Media.Composition;

[Flags]
internal enum MilPathGeometryFlags
{
	HasCurves = 1,
	BoundsValid = 2,
	HasGaps = 4,
	HasHollows = 8,
	IsRegionData = 0x10,
	Mask = 0x1F,
	FORCE_DWORD = -1
}
