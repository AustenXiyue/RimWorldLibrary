using System.Runtime.InteropServices;

namespace System.Windows.Media.Composition;

internal static class MilCoreApi
{
	[DllImport("wpfgfx_cor3.dll")]
	internal static extern int MilComposition_SyncFlush(nint pChannel);

	[DllImport("wpfgfx_cor3.dll")]
	internal unsafe static extern int MilUtility_GetPointAtLengthFraction(MilMatrix3x2D* pMatrix, FillRule fillRule, byte* pPathData, uint nSize, double rFraction, out Point pt, out Point vecTangent);

	[DllImport("wpfgfx_cor3.dll")]
	internal unsafe static extern int MilUtility_PolygonBounds(MilMatrix3x2D* pWorldMatrix, MIL_PEN_DATA* pPenData, double* pDashArray, Point* pPoints, byte* pTypes, uint pointCount, uint segmentCount, MilMatrix3x2D* pGeometryMatrix, double rTolerance, bool fRelative, bool fSkipHollows, Rect* pBounds);

	[DllImport("wpfgfx_cor3.dll")]
	internal unsafe static extern int MilUtility_PolygonHitTest(MilMatrix3x2D* pGeometryMatrix, MIL_PEN_DATA* pPenData, double* pDashArray, Point* pPoints, byte* pTypes, uint cPoints, uint cSegments, double rTolerance, bool fRelative, Point* pHitPoint, out bool pDoesContain);

	[DllImport("wpfgfx_cor3.dll")]
	internal unsafe static extern int MilUtility_PathGeometryHitTest(MilMatrix3x2D* pMatrix, MIL_PEN_DATA* pPenData, double* pDashArray, FillRule fillRule, byte* pPathData, uint nSize, double rTolerance, bool fRelative, Point* pHitPoint, out bool pDoesContain);

	[DllImport("wpfgfx_cor3.dll")]
	internal unsafe static extern int MilUtility_PathGeometryHitTestPathGeometry(MilMatrix3x2D* pMatrix1, FillRule fillRule1, byte* pPathData1, uint nSize1, MilMatrix3x2D* pMatrix2, FillRule fillRule2, byte* pPathData2, uint nSize2, double rTolerance, bool fRelative, IntersectionDetail* pDetail);

	[DllImport("wpfgfx_cor3.dll")]
	internal unsafe static extern int MilUtility_GeometryGetArea(FillRule fillRule, byte* pPathData, uint nSize, MilMatrix3x2D* pMatrix, double rTolerance, bool fRelative, double* pArea);

	[DllImport("wpfgfx_cor3.dll")]
	internal unsafe static extern void MilUtility_ArcToBezier(Point ptStart, Size rRadii, double rRotation, bool fLargeArc, SweepDirection fSweepUp, Point ptEnd, MilMatrix3x2D* pMatrix, Point* pPt, out int cPieces);
}
