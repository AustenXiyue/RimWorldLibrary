namespace System.Windows.Media.Composition;

internal enum MIL_SEGMENT_TYPE
{
	MilSegmentNone = 0,
	MilSegmentLine = 1,
	MilSegmentBezier = 2,
	MilSegmentQuadraticBezier = 3,
	MilSegmentArc = 4,
	MilSegmentPolyLine = 5,
	MilSegmentPolyBezier = 6,
	MilSegmentPolyQuadraticBezier = 7,
	MIL_SEGMENT_TYPE_FORCE_DWORD = -1
}
