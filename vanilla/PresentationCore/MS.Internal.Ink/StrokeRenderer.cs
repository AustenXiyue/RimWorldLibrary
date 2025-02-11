using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using MS.Internal.PresentationCore;

namespace MS.Internal.Ink;

internal static class StrokeRenderer
{
	private enum RectCompareResult
	{
		Rect1ContainsRect2,
		Rect2ContainsRect1,
		NoItersection
	}

	internal const double HighlighterOpacity = 0.5;

	internal const byte SolidStrokeAlpha = byte.MaxValue;

	internal static readonly Point ArcToMarker = new Point(double.MinValue, double.MinValue);

	internal static void CalcGeometryAndBoundsWithTransform(StrokeNodeIterator iterator, DrawingAttributes drawingAttributes, MatrixTypes stylusTipMatrixType, bool calculateBounds, out Geometry geometry, out Rect bounds)
	{
		StreamGeometry streamGeometry = new StreamGeometry();
		streamGeometry.FillRule = FillRule.Nonzero;
		StreamGeometryContext streamGeometryContext = streamGeometry.Open();
		geometry = streamGeometry;
		bounds = Rect.Empty;
		try
		{
			List<Point> list = new List<Point>(iterator.Count * 4);
			int num = iterator.Count * 2;
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				list.Add(new Point(0.0, 0.0));
			}
			List<Point> list2 = new List<Point>();
			double lastAngle = 0.0;
			bool flag = false;
			Rect rect = new Rect(0.0, 0.0, 0.0, 0.0);
			for (int j = 0; j < iterator.Count; j++)
			{
				StrokeNode strokeNode = iterator[j];
				Rect bounds2 = strokeNode.GetBounds();
				if (calculateBounds)
				{
					bounds.Union(bounds2);
				}
				double num3 = Math.Abs(GetAngleDeltaFromLast(strokeNode.PreviousPosition, strokeNode.Position, ref lastAngle));
				double num4 = 45.0;
				if (stylusTipMatrixType == MatrixTypes.TRANSFORM_IS_UNKNOWN)
				{
					num4 = 10.0;
				}
				else if (bounds2.Height > 40.0 || bounds2.Width > 40.0)
				{
					num4 = 20.0;
				}
				bool flag2 = num3 > num4 && num3 < 360.0 - num4;
				double val = rect.Height * rect.Width;
				double val2 = bounds2.Height * bounds2.Width;
				bool flag3 = false;
				if (Math.Min(val, val2) / Math.Max(val, val2) <= 0.7)
				{
					flag3 = true;
				}
				rect = bounds2;
				if (j <= 1 || j >= iterator.Count - 2 || flag2 || flag3)
				{
					if (flag2 && !flag && j > 1 && j < iterator.Count - 1)
					{
						list2.Clear();
						strokeNode.GetPreviousContourPoints(list2);
						AddFigureToStreamGeometryContext(streamGeometryContext, list2, strokeNode.IsEllipse);
						flag = true;
					}
					list2.Clear();
					strokeNode.GetContourPoints(list2);
					AddFigureToStreamGeometryContext(streamGeometryContext, list2, strokeNode.IsEllipse);
				}
				if (!flag2)
				{
					flag = false;
				}
				Quad connectingQuad = strokeNode.GetConnectingQuad();
				if (!connectingQuad.IsEmpty)
				{
					list[num2++] = connectingQuad.A;
					list[num2++] = connectingQuad.B;
					list.Add(connectingQuad.D);
					list.Add(connectingQuad.C);
				}
				if (strokeNode.IsLastNode && num2 > 0)
				{
					int num5 = iterator.Count * 2;
					int num6 = list.Count - 1;
					int num7 = num2;
					for (int k = num5; k <= num6; k++)
					{
						list[num7] = list[k];
						num7++;
					}
					int num8 = num5 - num2;
					list.RemoveRange(num6 - num8 + 1, num8);
					int num9 = num2;
					int num10 = list.Count - 1;
					while (num9 < num10)
					{
						Point value = list[num9];
						list[num9] = list[num10];
						list[num10] = value;
						num9++;
						num10--;
					}
					AddFigureToStreamGeometryContext(streamGeometryContext, list, isBezierFigure: false);
				}
			}
		}
		finally
		{
			streamGeometryContext.Close();
			geometry.Freeze();
		}
	}

	[FriendAccessAllowed]
	internal static void CalcGeometryAndBounds(StrokeNodeIterator iterator, DrawingAttributes drawingAttributes, bool calculateBounds, out Geometry geometry, out Rect bounds)
	{
		Matrix stylusTipTransform = drawingAttributes.StylusTipTransform;
		if (stylusTipTransform != Matrix.Identity && stylusTipTransform._type != MatrixTypes.TRANSFORM_IS_SCALING)
		{
			CalcGeometryAndBoundsWithTransform(iterator, drawingAttributes, stylusTipTransform._type, calculateBounds, out geometry, out bounds);
			return;
		}
		StreamGeometry streamGeometry = new StreamGeometry();
		streamGeometry.FillRule = FillRule.Nonzero;
		StreamGeometryContext streamGeometryContext = streamGeometry.Open();
		geometry = streamGeometry;
		Rect rect = (bounds = Rect.Empty);
		try
		{
			StrokeNode strokeNode = default(StrokeNode);
			StrokeNode strokeNodePrevious = default(StrokeNode);
			StrokeNode strokeNode2 = default(StrokeNode);
			StrokeNode strokeNode3 = default(StrokeNode);
			Rect rect2 = rect;
			Rect rect3 = rect;
			Rect rect4 = rect;
			double num = 95.0;
			double num2 = Math.Max(drawingAttributes.Height, drawingAttributes.Width);
			num += Math.Min(4.99999, num2 / 20.0 * 5.0);
			double lastAngle = double.MinValue;
			bool flag = true;
			bool isEllipse = drawingAttributes.StylusTip == StylusTip.Ellipse;
			bool ignorePressure = drawingAttributes.IgnorePressure;
			List<Point> list = new List<Point>();
			List<Point> list2 = new List<Point>();
			List<Point> list3 = new List<Point>(4);
			int count = iterator.Count;
			int num3 = 0;
			int previousIndex = -1;
			while (num3 < count)
			{
				if (!strokeNodePrevious.IsValid)
				{
					if (!strokeNode2.IsValid)
					{
						strokeNodePrevious = iterator[num3++, previousIndex++];
						rect2 = strokeNodePrevious.GetBounds();
						continue;
					}
					strokeNodePrevious = strokeNode2;
					rect2 = rect3;
					strokeNode2 = strokeNode;
				}
				if (!strokeNode2.IsValid)
				{
					if (!strokeNode3.IsValid)
					{
						strokeNode2 = iterator[num3++, previousIndex];
						rect3 = strokeNode2.GetBounds();
						switch (FuzzyContains(rect3, rect2, flag ? 99.99999 : num))
						{
						case RectCompareResult.Rect1ContainsRect2:
							strokeNodePrevious = iterator[num3 - 1, strokeNodePrevious.Index - 1];
							rect2 = Rect.Union(rect3, rect2);
							strokeNode2 = strokeNode;
							previousIndex = num3 - 1;
							break;
						case RectCompareResult.Rect2ContainsRect1:
							strokeNode2 = strokeNode;
							break;
						default:
							previousIndex = num3 - 1;
							break;
						}
						continue;
					}
					strokeNode2 = strokeNode3;
					rect3 = rect4;
					strokeNode3 = strokeNode;
				}
				if (!strokeNode3.IsValid)
				{
					strokeNode3 = iterator[num3++, previousIndex];
					rect4 = strokeNode3.GetBounds();
					RectCompareResult rectCompareResult = FuzzyContains(rect4, rect3, flag ? 99.99999 : num);
					RectCompareResult rectCompareResult2 = FuzzyContains(rect4, rect2, flag ? 99.99999 : num);
					if (flag && rectCompareResult == RectCompareResult.Rect1ContainsRect2 && rectCompareResult2 == RectCompareResult.Rect1ContainsRect2)
					{
						if (list.Count > 0)
						{
							strokeNode2.GetPointsAtEndOfSegment(list, list2);
							ReverseDCPointsRenderAndClear(streamGeometryContext, list, list2, list3, isEllipse, clear: true);
						}
						strokeNodePrevious = iterator[num3 - 1, strokeNodePrevious.Index - 1];
						rect2 = strokeNodePrevious.GetBounds();
						strokeNode2 = strokeNode;
						strokeNode3 = strokeNode;
						previousIndex = num3 - 1;
						continue;
					}
					switch (rectCompareResult)
					{
					case RectCompareResult.Rect1ContainsRect2:
						strokeNode3 = iterator[num3 - 1, strokeNode2.Index - 1];
						if (!strokeNode3.GetConnectingQuad().IsEmpty)
						{
							strokeNode2 = strokeNode3;
							rect3 = Rect.Union(rect4, rect3);
							previousIndex = num3 - 1;
						}
						strokeNode3 = strokeNode;
						lastAngle = double.MinValue;
						continue;
					case RectCompareResult.Rect2ContainsRect1:
						strokeNode3 = strokeNode;
						continue;
					}
					previousIndex = num3 - 1;
				}
				bool flag2 = rect2.IntersectsWith(rect4);
				if (calculateBounds)
				{
					bounds.Union(rect3);
				}
				if (list.Count == 0)
				{
					if (calculateBounds)
					{
						bounds.Union(rect2);
					}
					if (flag && flag2)
					{
						strokeNodePrevious.GetContourPoints(list3);
						AddFigureToStreamGeometryContext(streamGeometryContext, list3, strokeNodePrevious.IsEllipse);
						list3.Clear();
					}
					strokeNode2.GetPointsAtStartOfSegment(list, list2);
					flag = false;
				}
				if (lastAngle == double.MinValue)
				{
					lastAngle = GetAngleBetween(strokeNodePrevious.Position, strokeNode2.Position);
				}
				double angleDeltaFromLast = GetAngleDeltaFromLast(strokeNode2.Position, strokeNode3.Position, ref lastAngle);
				bool flag3 = Math.Abs(angleDeltaFromLast) > 90.0 && Math.Abs(angleDeltaFromLast) < 270.0;
				bool flag4 = flag2 && !ignorePressure && strokeNode3.PressureFactor != 1f && Math.Abs(angleDeltaFromLast) > 30.0 && Math.Abs(angleDeltaFromLast) < 330.0;
				double num4 = rect3.Height * rect3.Width;
				double num5 = rect4.Height * rect4.Width;
				bool flag5 = num4 != num5 || num4 != rect2.Height * rect2.Width;
				bool flag6 = false;
				if (flag2 && flag5 && Math.Min(num4, num5) / Math.Max(num4, num5) <= 0.9)
				{
					flag6 = true;
				}
				if (flag5 || angleDeltaFromLast != 0.0 || num3 >= count)
				{
					if ((flag2 && (flag4 || flag6)) || flag3)
					{
						strokeNode2.GetPointsAtEndOfSegment(list, list2);
						ReverseDCPointsRenderAndClear(streamGeometryContext, list, list2, list3, isEllipse, clear: true);
						if (flag6)
						{
							strokeNode2.GetContourPoints(list3);
							AddFigureToStreamGeometryContext(streamGeometryContext, list3, strokeNode2.IsEllipse);
							list3.Clear();
						}
					}
					else
					{
						strokeNode3.GetPointsAtMiddleSegment(strokeNode2, angleDeltaFromLast, list, list2, out var missingIntersection);
						if (missingIntersection)
						{
							strokeNode2.GetPointsAtEndOfSegment(list, list2);
							ReverseDCPointsRenderAndClear(streamGeometryContext, list, list2, list3, isEllipse, clear: true);
						}
					}
				}
				strokeNodePrevious = strokeNode;
				rect2 = rect;
			}
			if (strokeNodePrevious.IsValid)
			{
				if (strokeNode2.IsValid)
				{
					if (calculateBounds)
					{
						bounds.Union(rect2);
						bounds.Union(rect3);
					}
					if (list.Count > 0)
					{
						strokeNode2.GetPointsAtEndOfSegment(list, list2);
						ReverseDCPointsRenderAndClear(streamGeometryContext, list, list2, list3, isEllipse, clear: false);
					}
					else
					{
						RenderTwoStrokeNodes(streamGeometryContext, strokeNodePrevious, rect2, strokeNode2, rect3, list, list2, list3);
					}
				}
				else
				{
					if (calculateBounds)
					{
						bounds.Union(rect2);
					}
					strokeNodePrevious.GetContourPoints(list);
					AddFigureToStreamGeometryContext(streamGeometryContext, list, strokeNodePrevious.IsEllipse);
				}
			}
			else
			{
				if (!strokeNode2.IsValid || !strokeNode3.IsValid)
				{
					return;
				}
				if (calculateBounds)
				{
					bounds.Union(rect3);
					bounds.Union(rect4);
				}
				if (list.Count > 0)
				{
					strokeNode3.GetPointsAtEndOfSegment(list, list2);
					ReverseDCPointsRenderAndClear(streamGeometryContext, list, list2, list3, isEllipse, clear: false);
					if (FuzzyContains(rect4, rect3, 70.0) != RectCompareResult.NoItersection)
					{
						strokeNode3.GetContourPoints(list3);
						AddFigureToStreamGeometryContext(streamGeometryContext, list3, strokeNode3.IsEllipse);
					}
				}
				else
				{
					RenderTwoStrokeNodes(streamGeometryContext, strokeNode2, rect3, strokeNode3, rect4, list, list2, list3);
				}
			}
		}
		finally
		{
			streamGeometryContext.Close();
			geometry.Freeze();
		}
	}

	private static void RenderTwoStrokeNodes(StreamGeometryContext context, StrokeNode strokeNodePrevious, Rect strokeNodePreviousBounds, StrokeNode strokeNodeCurrent, Rect strokeNodeCurrentBounds, List<Point> pointBuffer1, List<Point> pointBuffer2, List<Point> pointBuffer3)
	{
		if (FuzzyContains(strokeNodePreviousBounds, strokeNodeCurrentBounds, 70.0) != RectCompareResult.NoItersection)
		{
			strokeNodePrevious.GetContourPoints(pointBuffer1);
			AddFigureToStreamGeometryContext(context, pointBuffer1, strokeNodePrevious.IsEllipse);
			Quad connectingQuad = strokeNodeCurrent.GetConnectingQuad();
			if (!connectingQuad.IsEmpty)
			{
				pointBuffer3.Add(connectingQuad.A);
				pointBuffer3.Add(connectingQuad.B);
				pointBuffer3.Add(connectingQuad.C);
				pointBuffer3.Add(connectingQuad.D);
				AddFigureToStreamGeometryContext(context, pointBuffer3, isBezierFigure: false);
			}
			strokeNodeCurrent.GetContourPoints(pointBuffer2);
			AddFigureToStreamGeometryContext(context, pointBuffer2, strokeNodeCurrent.IsEllipse);
		}
		else
		{
			strokeNodeCurrent.GetPointsAtStartOfSegment(pointBuffer1, pointBuffer2);
			strokeNodeCurrent.GetPointsAtEndOfSegment(pointBuffer1, pointBuffer2);
			ReverseDCPointsRenderAndClear(context, pointBuffer1, pointBuffer2, pointBuffer3, strokeNodeCurrent.IsEllipse, clear: false);
		}
	}

	private static void ReverseDCPointsRenderAndClear(StreamGeometryContext context, List<Point> abPoints, List<Point> dcPoints, List<Point> polyLinePoints, bool isEllipse, bool clear)
	{
		int num = 0;
		int num2 = dcPoints.Count - 1;
		while (num < num2)
		{
			Point value = dcPoints[num];
			dcPoints[num] = dcPoints[num2];
			dcPoints[num2] = value;
			num++;
			num2--;
		}
		if (isEllipse)
		{
			AddArcToFigureToStreamGeometryContext(context, abPoints, dcPoints, polyLinePoints);
		}
		else
		{
			AddPolylineFigureToStreamGeometryContext(context, abPoints, dcPoints);
		}
		if (clear)
		{
			abPoints.Clear();
			dcPoints.Clear();
		}
	}

	private static RectCompareResult FuzzyContains(Rect rect1, Rect rect2, double percentIntersect)
	{
		double num = Math.Max(rect1.Left, rect2.Left);
		double num2 = Math.Max(rect1.Top, rect2.Top);
		double num3 = Math.Max(Math.Min(rect1.Right, rect2.Right) - num, 0.0);
		double num4 = Math.Max(Math.Min(rect1.Bottom, rect2.Bottom) - num2, 0.0);
		if (num3 == 0.0 || num4 == 0.0)
		{
			return RectCompareResult.NoItersection;
		}
		double num5 = rect1.Height * rect1.Width;
		double num6 = rect2.Height * rect2.Width;
		double num7 = Math.Min(num5, num6);
		if (num3 * num4 / num7 * 100.0 >= percentIntersect)
		{
			if (num5 >= num6)
			{
				return RectCompareResult.Rect1ContainsRect2;
			}
			return RectCompareResult.Rect2ContainsRect1;
		}
		return RectCompareResult.NoItersection;
	}

	private static void AddFigureToStreamGeometryContext(StreamGeometryContext context, List<Point> points, bool isBezierFigure)
	{
		context.BeginFigure(points[points.Count - 1], isFilled: true, isClosed: true);
		if (isBezierFigure)
		{
			context.PolyBezierTo(points, isStroked: true, isSmoothJoin: true);
		}
		else
		{
			context.PolyLineTo(points, isStroked: true, isSmoothJoin: true);
		}
	}

	private static void AddPolylineFigureToStreamGeometryContext(StreamGeometryContext context, List<Point> abPoints, List<Point> dcPoints)
	{
		context.BeginFigure(abPoints[0], isFilled: true, isClosed: true);
		context.PolyLineTo(abPoints, isStroked: true, isSmoothJoin: true);
		context.PolyLineTo(dcPoints, isStroked: true, isSmoothJoin: true);
	}

	private static void AddArcToFigureToStreamGeometryContext(StreamGeometryContext context, List<Point> abPoints, List<Point> dcPoints, List<Point> polyLinePoints)
	{
		if (abPoints.Count == 0 || dcPoints.Count == 0)
		{
			return;
		}
		context.BeginFigure(abPoints[0], isFilled: true, isClosed: true);
		for (int i = 0; i < 2; i++)
		{
			List<Point> list = ((i == 0) ? abPoints : dcPoints);
			int num = ((i == 0) ? 1 : 0);
			while (num < list.Count)
			{
				Point point = list[num];
				if (point == ArcToMarker)
				{
					if (polyLinePoints.Count > 0)
					{
						context.PolyLineTo(polyLinePoints, isStroked: true, isSmoothJoin: true);
						polyLinePoints.Clear();
					}
					if (num + 2 < list.Count)
					{
						Point point2 = list[num + 1];
						Size size = new Size(point2.X / 2.0, point2.Y / 2.0);
						Point point3 = list[num + 2];
						bool isLargeArc = false;
						context.ArcTo(point3, size, 0.0, isLargeArc, SweepDirection.Clockwise, isStroked: true, isSmoothJoin: true);
					}
					num += 3;
				}
				else
				{
					polyLinePoints.Add(point);
					num++;
				}
			}
			if (polyLinePoints.Count > 0)
			{
				context.PolyLineTo(polyLinePoints, isStroked: true, isSmoothJoin: true);
				polyLinePoints.Clear();
			}
		}
	}

	private static double GetAngleDeltaFromLast(Point previousPosition, Point currentPosition, ref double lastAngle)
	{
		double result = 0.0;
		double num = currentPosition.X * 1000.0 - previousPosition.X * 1000.0;
		double num2 = currentPosition.Y * 1000.0 - previousPosition.Y * 1000.0;
		if ((long)num == 0L && (long)num2 == 0L)
		{
			return result;
		}
		double angleBetween = GetAngleBetween(previousPosition, currentPosition);
		result = ((lastAngle >= 270.0 && angleBetween <= 90.0) ? (lastAngle - (360.0 + angleBetween)) : ((!(lastAngle <= 90.0) || !(angleBetween >= 270.0)) ? (lastAngle - angleBetween) : (360.0 + lastAngle - angleBetween)));
		lastAngle = angleBetween;
		return result;
	}

	private static double GetAngleBetween(Point previousPosition, Point currentPosition)
	{
		double result = 0.0;
		double num = currentPosition.X * 1000.0 - previousPosition.X * 1000.0;
		double num2 = currentPosition.Y * 1000.0 - previousPosition.Y * 1000.0;
		if ((long)num == 0L && (long)num2 == 0L)
		{
			return result;
		}
		result = ((num == 0.0) ? ((num2 == 0.0) ? 0.0 : ((!(num2 > 0.0)) ? 4.71238898038469 : (Math.PI / 2.0))) : ((num2 == 0.0) ? ((!(num > 0.0)) ? Math.PI : 0.0) : ((num < 0.0) ? (Math.Atan(num2 / num) + Math.PI) : ((!(num2 < 0.0)) ? Math.Atan(num2 / num) : (Math.Atan(num2 / num) + Math.PI * 2.0)))));
		return result * 180.0 / Math.PI;
	}

	internal static DrawingAttributes GetHighlighterAttributes(Stroke stroke, DrawingAttributes da)
	{
		if (da.Color.A != byte.MaxValue)
		{
			DrawingAttributes drawingAttributes = stroke.DrawingAttributes.Clone();
			drawingAttributes.Color = GetHighlighterColor(drawingAttributes.Color);
			return drawingAttributes;
		}
		return da;
	}

	internal static Color GetHighlighterColor(Color color)
	{
		color.A = byte.MaxValue;
		return color;
	}
}
