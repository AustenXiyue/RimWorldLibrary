namespace System.Windows.Media.Composition;

[Flags]
internal enum MilPathFigureFlags
{
	HasGaps = 1,
	HasCurves = 2,
	IsClosed = 4,
	IsFillable = 8,
	IsRectangleData = 0x10,
	Mask = 0x1F,
	FORCE_DWORD = -1
}
