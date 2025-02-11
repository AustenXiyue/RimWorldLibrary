using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MS.Internal.Controls;

internal class InkCanvasSelectionAdorner : Adorner
{
	private Pen _adornerBorderPen;

	private Pen _adornerPenBrush;

	private Brush _adornerFillBrush;

	private Pen _hatchPen;

	private Rect _strokesBounds;

	private List<Rect> _elementsBounds;

	private const double BorderMargin = 8.0;

	private const double HatchBorderMargin = 6.0;

	private const int CornerResizeHandleSize = 8;

	private const int MiddleResizeHandleSize = 6;

	private const double ResizeHandleTolerance = 3.0;

	private const double LineThickness = 0.16;

	internal InkCanvasSelectionAdorner(UIElement adornedElement)
		: base(adornedElement)
	{
		_adornerBorderPen = new Pen(Brushes.Black, 1.0);
		DoubleCollection dashes = new DoubleCollection { 4.5, 4.5 };
		_adornerBorderPen.DashStyle = new DashStyle(dashes, 2.25);
		_adornerBorderPen.DashCap = PenLineCap.Flat;
		_adornerBorderPen.Freeze();
		_adornerPenBrush = new Pen(new SolidColorBrush(Color.FromRgb(132, 146, 222)), 1.0);
		_adornerPenBrush.Freeze();
		_adornerFillBrush = new LinearGradientBrush(Color.FromRgb(240, 242, byte.MaxValue), Color.FromRgb(180, 207, 248), 45.0);
		_adornerFillBrush.Freeze();
		DrawingGroup drawingGroup = new DrawingGroup();
		DrawingContext drawingContext = null;
		try
		{
			drawingContext = drawingGroup.Open();
			drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, 1.0, 1.0));
			drawingContext.DrawLine(new Pen(Brushes.Black, 0.16)
			{
				StartLineCap = PenLineCap.Square,
				EndLineCap = PenLineCap.Square
			}, new Point(1.0, 0.0), new Point(0.0, 1.0));
		}
		finally
		{
			drawingContext?.Close();
		}
		drawingGroup.Freeze();
		DrawingBrush drawingBrush = new DrawingBrush(drawingGroup);
		drawingBrush.TileMode = TileMode.Tile;
		drawingBrush.Viewport = new Rect(0.0, 0.0, 6.0, 6.0);
		drawingBrush.ViewportUnits = BrushMappingMode.Absolute;
		drawingBrush.Freeze();
		_hatchPen = new Pen(drawingBrush, 6.0);
		_hatchPen.Freeze();
		_elementsBounds = new List<Rect>();
		_strokesBounds = Rect.Empty;
	}

	internal InkCanvasSelectionHitResult SelectionHandleHitTest(Point point)
	{
		InkCanvasSelectionHitResult inkCanvasSelectionHitResult = InkCanvasSelectionHitResult.None;
		Rect wireFrameRect = GetWireFrameRect();
		if (!wireFrameRect.IsEmpty)
		{
			for (InkCanvasSelectionHitResult inkCanvasSelectionHitResult2 = InkCanvasSelectionHitResult.TopLeft; inkCanvasSelectionHitResult2 <= InkCanvasSelectionHitResult.Left; inkCanvasSelectionHitResult2++)
			{
				GetHandleRect(inkCanvasSelectionHitResult2, wireFrameRect, out var _, out var toleranceRect);
				if (toleranceRect.Contains(point))
				{
					inkCanvasSelectionHitResult = inkCanvasSelectionHitResult2;
					break;
				}
			}
			if (inkCanvasSelectionHitResult == InkCanvasSelectionHitResult.None && Rect.Inflate(wireFrameRect, 4.0, 4.0).Contains(point))
			{
				inkCanvasSelectionHitResult = InkCanvasSelectionHitResult.Selection;
			}
		}
		return inkCanvasSelectionHitResult;
	}

	internal void UpdateSelectionWireFrame(Rect strokesBounds, List<Rect> hatchBounds)
	{
		bool flag = false;
		bool flag2 = false;
		if (_strokesBounds != strokesBounds)
		{
			_strokesBounds = strokesBounds;
			flag = true;
		}
		int count = hatchBounds.Count;
		if (count != _elementsBounds.Count)
		{
			flag2 = true;
		}
		else
		{
			for (int i = 0; i < count; i++)
			{
				if (_elementsBounds[i] != hatchBounds[i])
				{
					flag2 = true;
					break;
				}
			}
		}
		if (flag || flag2)
		{
			if (flag2)
			{
				_elementsBounds = hatchBounds;
			}
			InvalidateVisual();
		}
	}

	protected override void OnRender(DrawingContext drawingContext)
	{
		DrawBackgound(drawingContext);
		Rect wireFrameRect = GetWireFrameRect();
		if (!wireFrameRect.IsEmpty)
		{
			drawingContext.DrawRectangle(null, _adornerBorderPen, wireFrameRect);
			DrawHandles(drawingContext, wireFrameRect);
		}
	}

	private void DrawHandles(DrawingContext drawingContext, Rect rectWireFrame)
	{
		for (InkCanvasSelectionHitResult inkCanvasSelectionHitResult = InkCanvasSelectionHitResult.TopLeft; inkCanvasSelectionHitResult <= InkCanvasSelectionHitResult.Left; inkCanvasSelectionHitResult++)
		{
			GetHandleRect(inkCanvasSelectionHitResult, rectWireFrame, out var visibleRect, out var _);
			drawingContext.DrawRectangle(_adornerFillBrush, _adornerPenBrush, visibleRect);
		}
	}

	private void DrawBackgound(DrawingContext drawingContext)
	{
		PathGeometry pathGeometry = null;
		Geometry geometry = null;
		int count = _elementsBounds.Count;
		if (count != 0)
		{
			for (int i = 0; i < count; i++)
			{
				Rect rect = _elementsBounds[i];
				if (!rect.IsEmpty)
				{
					rect.Inflate(3.0, 3.0);
					if (pathGeometry == null)
					{
						PathFigure pathFigure = new PathFigure();
						pathFigure.StartPoint = new Point(rect.Left, rect.Top);
						PathSegmentCollection pathSegmentCollection = new PathSegmentCollection();
						PathSegment pathSegment = new LineSegment(new Point(rect.Right, rect.Top), isStroked: true);
						pathSegment.Freeze();
						pathSegmentCollection.Add(pathSegment);
						pathSegment = new LineSegment(new Point(rect.Right, rect.Bottom), isStroked: true);
						pathSegment.Freeze();
						pathSegmentCollection.Add(pathSegment);
						pathSegment = new LineSegment(new Point(rect.Left, rect.Bottom), isStroked: true);
						pathSegment.Freeze();
						pathSegmentCollection.Add(pathSegment);
						pathSegment = new LineSegment(new Point(rect.Left, rect.Top), isStroked: true);
						pathSegment.Freeze();
						pathSegmentCollection.Add(pathSegment);
						pathSegmentCollection.Freeze();
						pathFigure.Segments = pathSegmentCollection;
						pathFigure.IsClosed = true;
						pathFigure.Freeze();
						pathGeometry = new PathGeometry();
						pathGeometry.Figures.Add(pathFigure);
					}
					else
					{
						geometry = new RectangleGeometry(rect);
						geometry.Freeze();
						pathGeometry = Geometry.Combine(pathGeometry, geometry, GeometryCombineMode.Union, null);
					}
				}
			}
		}
		GeometryGroup geometryGroup = new GeometryGroup();
		GeometryCollection geometryCollection = new GeometryCollection();
		geometry = new RectangleGeometry(new Rect(0.0, 0.0, base.RenderSize.Width, base.RenderSize.Height));
		geometry.Freeze();
		geometryCollection.Add(geometry);
		Geometry geometry2 = null;
		if (pathGeometry != null)
		{
			pathGeometry.Freeze();
			geometry2 = pathGeometry.GetOutlinedPathGeometry();
			geometry2.Freeze();
			if (count == 1 && ((InkCanvasInnerCanvas)base.AdornedElement).InkCanvas.GetSelectedStrokes().Count == 0)
			{
				geometryCollection.Add(geometry2);
			}
		}
		geometryCollection.Freeze();
		geometryGroup.Children = geometryCollection;
		geometryGroup.Freeze();
		drawingContext.DrawGeometry(Brushes.Transparent, null, geometryGroup);
		if (geometry2 != null)
		{
			drawingContext.DrawGeometry(null, _hatchPen, geometry2);
		}
	}

	private void GetHandleRect(InkCanvasSelectionHitResult hitResult, Rect rectWireFrame, out Rect visibleRect, out Rect toleranceRect)
	{
		Point point = default(Point);
		double num = 0.0;
		double num2 = 3.0;
		switch (hitResult)
		{
		case InkCanvasSelectionHitResult.TopLeft:
			num = 8.0;
			point = new Point(rectWireFrame.Left, rectWireFrame.Top);
			break;
		case InkCanvasSelectionHitResult.Top:
			num = 6.0;
			point = new Point(rectWireFrame.Left + rectWireFrame.Width / 2.0, rectWireFrame.Top);
			num2 = 5.0;
			break;
		case InkCanvasSelectionHitResult.TopRight:
			num = 8.0;
			point = new Point(rectWireFrame.Right, rectWireFrame.Top);
			break;
		case InkCanvasSelectionHitResult.Left:
			num = 6.0;
			point = new Point(rectWireFrame.Left, rectWireFrame.Top + rectWireFrame.Height / 2.0);
			num2 = 5.0;
			break;
		case InkCanvasSelectionHitResult.Right:
			num = 6.0;
			point = new Point(rectWireFrame.Right, rectWireFrame.Top + rectWireFrame.Height / 2.0);
			num2 = 5.0;
			break;
		case InkCanvasSelectionHitResult.BottomLeft:
			num = 8.0;
			point = new Point(rectWireFrame.Left, rectWireFrame.Bottom);
			break;
		case InkCanvasSelectionHitResult.Bottom:
			num = 6.0;
			point = new Point(rectWireFrame.Left + rectWireFrame.Width / 2.0, rectWireFrame.Bottom);
			num2 = 5.0;
			break;
		case InkCanvasSelectionHitResult.BottomRight:
			num = 8.0;
			point = new Point(rectWireFrame.Right, rectWireFrame.Bottom);
			break;
		}
		visibleRect = new Rect(point.X - num / 2.0, point.Y - num / 2.0, num, num);
		toleranceRect = visibleRect;
		toleranceRect.Inflate(num2, num2);
	}

	private Rect GetWireFrameRect()
	{
		Rect result = Rect.Empty;
		Rect selectionBounds = ((InkCanvasInnerCanvas)base.AdornedElement).InkCanvas.GetSelectionBounds();
		if (!selectionBounds.IsEmpty)
		{
			result = Rect.Inflate(selectionBounds, 8.0, 8.0);
		}
		return result;
	}
}
