using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MS.Internal.AppModel;
using MS.Win32;

namespace MS.Internal.Ink;

internal static class PenCursorManager
{
	private static Cursor s_StrokeEraserCursor;

	internal static Cursor GetPenCursor(DrawingAttributes drawingAttributes, bool isHollow, bool isRightToLeft, double dpiScaleX, double dpiScaleY)
	{
		return CreateCursorFromDrawing(CreatePenDrawing(drawingAttributes, isHollow, isRightToLeft, dpiScaleX, dpiScaleY), new Point(0.0, 0.0));
	}

	internal static Cursor GetPointEraserCursor(StylusShape stylusShape, Matrix tranform, double dpiScaleX, double dpiScaleY)
	{
		DrawingAttributes drawingAttributes = new DrawingAttributes();
		if (stylusShape.GetType() == typeof(RectangleStylusShape))
		{
			drawingAttributes.StylusTip = StylusTip.Rectangle;
		}
		else
		{
			drawingAttributes.StylusTip = StylusTip.Ellipse;
		}
		drawingAttributes.Height = stylusShape.Height;
		drawingAttributes.Width = stylusShape.Width;
		drawingAttributes.Color = Colors.Black;
		if (!tranform.IsIdentity)
		{
			drawingAttributes.StylusTipTransform *= tranform;
		}
		if (!DoubleUtil.IsZero(stylusShape.Rotation))
		{
			Matrix identity = Matrix.Identity;
			identity.Rotate(stylusShape.Rotation);
			drawingAttributes.StylusTipTransform *= identity;
		}
		return GetPenCursor(drawingAttributes, isHollow: true, isRightToLeft: false, dpiScaleX, dpiScaleY);
	}

	internal static Cursor GetStrokeEraserCursor()
	{
		if (s_StrokeEraserCursor == null)
		{
			s_StrokeEraserCursor = CreateCursorFromDrawing(CreateStrokeEraserDrawing(), new Point(5.0, 5.0));
		}
		return s_StrokeEraserCursor;
	}

	internal static Cursor GetSelectionCursor(InkCanvasSelectionHitResult hitResult, bool isRightToLeft)
	{
		switch (hitResult)
		{
		case InkCanvasSelectionHitResult.TopLeft:
		case InkCanvasSelectionHitResult.BottomRight:
			if (isRightToLeft)
			{
				return Cursors.SizeNESW;
			}
			return Cursors.SizeNWSE;
		case InkCanvasSelectionHitResult.Top:
		case InkCanvasSelectionHitResult.Bottom:
			return Cursors.SizeNS;
		case InkCanvasSelectionHitResult.TopRight:
		case InkCanvasSelectionHitResult.BottomLeft:
			if (isRightToLeft)
			{
				return Cursors.SizeNWSE;
			}
			return Cursors.SizeNESW;
		case InkCanvasSelectionHitResult.Right:
		case InkCanvasSelectionHitResult.Left:
			return Cursors.SizeWE;
		case InkCanvasSelectionHitResult.Selection:
			return Cursors.SizeAll;
		default:
			return Cursors.Cross;
		}
	}

	private static Cursor CreateCursorFromDrawing(Drawing drawing, Point hotspot)
	{
		_ = Cursors.Arrow;
		Rect bounds = drawing.Bounds;
		double width = bounds.Width;
		double height = bounds.Height;
		int num = IconHelper.AlignToBytes(bounds.Width, 1);
		int num2 = IconHelper.AlignToBytes(bounds.Height, 1);
		bounds.Inflate(((double)num - width) / 2.0, ((double)num2 - height) / 2.0);
		int xHotspot = (int)Math.Round(hotspot.X - bounds.Left);
		int yHotspot = (int)Math.Round(hotspot.Y - bounds.Top);
		MS.Win32.NativeMethods.IconHandle iconHandle = IconHelper.CreateIconCursor(GetPixels(RenderVisualToBitmap(CreateCursorDrawingVisual(drawing, num, num2), num, num2), num, num2), num, num2, xHotspot, yHotspot, isIcon: false);
		if (iconHandle.IsInvalid)
		{
			return Cursors.Arrow;
		}
		return CursorInteropHelper.CriticalCreate(iconHandle);
	}

	private static DrawingVisual CreateCursorDrawingVisual(Drawing drawing, int width, int height)
	{
		DrawingBrush drawingBrush = new DrawingBrush(drawing);
		drawingBrush.Stretch = Stretch.None;
		drawingBrush.AlignmentX = AlignmentX.Center;
		drawingBrush.AlignmentY = AlignmentY.Center;
		DrawingVisual drawingVisual = new DrawingVisual();
		DrawingContext drawingContext = null;
		try
		{
			drawingContext = drawingVisual.RenderOpen();
			drawingContext.DrawRectangle(drawingBrush, null, new Rect(0.0, 0.0, width, height));
			return drawingVisual;
		}
		finally
		{
			drawingContext?.Close();
		}
	}

	private static RenderTargetBitmap RenderVisualToBitmap(Visual visual, int width, int height)
	{
		RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(width, height, 96.0, 96.0, PixelFormats.Pbgra32);
		renderTargetBitmap.Render(visual);
		return renderTargetBitmap;
	}

	private static byte[] GetPixels(RenderTargetBitmap rtb, int width, int height)
	{
		int num = width * 4;
		FormatConvertedBitmap formatConvertedBitmap = new FormatConvertedBitmap();
		formatConvertedBitmap.BeginInit();
		formatConvertedBitmap.Source = rtb;
		formatConvertedBitmap.DestinationFormat = PixelFormats.Bgra32;
		formatConvertedBitmap.EndInit();
		byte[] array = new byte[num * height];
		formatConvertedBitmap.CriticalCopyPixels(Int32Rect.Empty, array, num, 0);
		return array;
	}

	private static Drawing CreatePenDrawing(DrawingAttributes drawingAttributes, bool isHollow, bool isRightToLeft, double dpiScaleX, double dpiScaleY)
	{
		Stroke stroke = new Stroke(new StylusPointCollection
		{
			new StylusPoint(0.0, 0.0)
		}, new DrawingAttributes
		{
			Color = drawingAttributes.Color,
			Width = drawingAttributes.Width,
			Height = drawingAttributes.Height,
			StylusTipTransform = drawingAttributes.StylusTipTransform,
			IsHighlighter = drawingAttributes.IsHighlighter,
			StylusTip = drawingAttributes.StylusTip
		});
		stroke.DrawingAttributes.Width = ConvertToPixel(stroke.DrawingAttributes.Width, dpiScaleX);
		stroke.DrawingAttributes.Height = ConvertToPixel(stroke.DrawingAttributes.Height, dpiScaleY);
		double num = Math.Min(SystemParameters.PrimaryScreenWidth / 2.0, SystemParameters.PrimaryScreenHeight / 2.0);
		Rect bounds = stroke.GetBounds();
		bool flag = false;
		if (DoubleUtil.LessThan(bounds.Width, 1.0))
		{
			stroke.DrawingAttributes.Width = 1.0;
			flag = true;
		}
		else if (DoubleUtil.GreaterThan(bounds.Width, num))
		{
			stroke.DrawingAttributes.Width = num;
			flag = true;
		}
		if (DoubleUtil.LessThan(bounds.Height, 1.0))
		{
			stroke.DrawingAttributes.Height = 1.0;
			flag = true;
		}
		else if (DoubleUtil.GreaterThan(bounds.Height, num))
		{
			stroke.DrawingAttributes.Height = num;
			flag = true;
		}
		if (flag)
		{
			stroke.DrawingAttributes.StylusTipTransform = Matrix.Identity;
		}
		if (isRightToLeft)
		{
			Matrix stylusTipTransform = stroke.DrawingAttributes.StylusTipTransform;
			stylusTipTransform.Scale(-1.0, 1.0);
			if (stylusTipTransform.HasInverse)
			{
				stroke.DrawingAttributes.StylusTipTransform = stylusTipTransform;
			}
		}
		DrawingGroup drawingGroup = new DrawingGroup();
		DrawingContext drawingContext = null;
		try
		{
			drawingContext = drawingGroup.Open();
			if (isHollow)
			{
				stroke.DrawInternal(drawingContext, stroke.DrawingAttributes, isHollow);
			}
			else
			{
				stroke.Draw(drawingContext, stroke.DrawingAttributes);
			}
		}
		finally
		{
			drawingContext?.Close();
		}
		return drawingGroup;
	}

	private static Drawing CreateStrokeEraserDrawing()
	{
		DrawingGroup drawingGroup = new DrawingGroup();
		DrawingContext drawingContext = null;
		try
		{
			drawingContext = drawingGroup.Open();
			LinearGradientBrush linearGradientBrush = new LinearGradientBrush(Color.FromRgb(240, 242, byte.MaxValue), Color.FromRgb(180, 207, 248), 45.0);
			linearGradientBrush.Freeze();
			SolidColorBrush solidColorBrush = new SolidColorBrush(Color.FromRgb(180, 207, 248));
			solidColorBrush.Freeze();
			Pen pen = new Pen(Brushes.Gray, 0.7);
			pen.Freeze();
			PathGeometry pathGeometry = new PathGeometry();
			PathFigure pathFigure = new PathFigure();
			pathFigure.StartPoint = new Point(5.0, 5.0);
			LineSegment lineSegment = new LineSegment(new Point(16.0, 5.0), isStroked: true);
			lineSegment.Freeze();
			pathFigure.Segments.Add(lineSegment);
			lineSegment = new LineSegment(new Point(26.0, 15.0), isStroked: true);
			lineSegment.Freeze();
			pathFigure.Segments.Add(lineSegment);
			lineSegment = new LineSegment(new Point(15.0, 15.0), isStroked: true);
			lineSegment.Freeze();
			pathFigure.Segments.Add(lineSegment);
			lineSegment = new LineSegment(new Point(5.0, 5.0), isStroked: true);
			lineSegment.Freeze();
			pathFigure.Segments.Add(lineSegment);
			pathFigure.IsClosed = true;
			pathFigure.Freeze();
			pathGeometry.Figures.Add(pathFigure);
			pathFigure = new PathFigure();
			pathFigure.StartPoint = new Point(5.0, 5.0);
			lineSegment = new LineSegment(new Point(5.0, 10.0), isStroked: true);
			lineSegment.Freeze();
			pathFigure.Segments.Add(lineSegment);
			lineSegment = new LineSegment(new Point(15.0, 19.0), isStroked: true);
			lineSegment.Freeze();
			pathFigure.Segments.Add(lineSegment);
			lineSegment = new LineSegment(new Point(15.0, 15.0), isStroked: true);
			lineSegment.Freeze();
			pathFigure.Segments.Add(lineSegment);
			lineSegment = new LineSegment(new Point(5.0, 5.0), isStroked: true);
			lineSegment.Freeze();
			pathFigure.Segments.Add(lineSegment);
			pathFigure.IsClosed = true;
			pathFigure.Freeze();
			pathGeometry.Figures.Add(pathFigure);
			pathGeometry.Freeze();
			PathGeometry pathGeometry2 = new PathGeometry();
			pathFigure = new PathFigure();
			pathFigure.StartPoint = new Point(15.0, 15.0);
			lineSegment = new LineSegment(new Point(15.0, 19.0), isStroked: true);
			lineSegment.Freeze();
			pathFigure.Segments.Add(lineSegment);
			lineSegment = new LineSegment(new Point(26.0, 19.0), isStroked: true);
			lineSegment.Freeze();
			pathFigure.Segments.Add(lineSegment);
			lineSegment = new LineSegment(new Point(26.0, 15.0), isStroked: true);
			lineSegment.Freeze();
			pathFigure.Segments.Add(lineSegment);
			lineSegment.Freeze();
			lineSegment = new LineSegment(new Point(15.0, 15.0), isStroked: true);
			pathFigure.Segments.Add(lineSegment);
			pathFigure.IsClosed = true;
			pathFigure.Freeze();
			pathGeometry2.Figures.Add(pathFigure);
			pathGeometry2.Freeze();
			drawingContext.DrawGeometry(linearGradientBrush, pen, pathGeometry);
			drawingContext.DrawGeometry(solidColorBrush, pen, pathGeometry2);
			drawingContext.DrawLine(pen, new Point(5.0, 5.0), new Point(5.0, 0.0));
			drawingContext.DrawLine(pen, new Point(5.0, 5.0), new Point(0.0, 5.0));
			drawingContext.DrawLine(pen, new Point(5.0, 5.0), new Point(2.0, 2.0));
			drawingContext.DrawLine(pen, new Point(5.0, 5.0), new Point(8.0, 2.0));
			drawingContext.DrawLine(pen, new Point(5.0, 5.0), new Point(2.0, 8.0));
			return drawingGroup;
		}
		finally
		{
			drawingContext?.Close();
		}
	}

	private static double ConvertToPixel(double value, double dpiScale)
	{
		if (dpiScale != 0.0)
		{
			return value * dpiScale;
		}
		return value;
	}
}
