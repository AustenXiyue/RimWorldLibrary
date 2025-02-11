namespace System.Windows.Media;

internal abstract class HitTestDrawingContextWalker : DrawingContextWalker
{
	protected bool _contains;

	internal abstract bool IsHit { get; }

	internal abstract IntersectionDetail IntersectionDetail { get; }

	internal HitTestDrawingContextWalker()
	{
	}

	public override void DrawLine(Pen pen, Point point0, Point point1)
	{
		DrawGeometry(null, pen, new LineGeometry(point0, point1));
	}

	public override void DrawRectangle(Brush brush, Pen pen, Rect rectangle)
	{
		DrawGeometry(brush, pen, new RectangleGeometry(rectangle));
	}

	public override void DrawRoundedRectangle(Brush brush, Pen pen, Rect rectangle, double radiusX, double radiusY)
	{
		DrawGeometry(brush, pen, new RectangleGeometry(rectangle, radiusX, radiusY));
	}

	public override void DrawEllipse(Brush brush, Pen pen, Point center, double radiusX, double radiusY)
	{
		DrawGeometry(brush, pen, new EllipseGeometry(center, radiusX, radiusY));
	}

	public override void DrawImage(ImageSource imageSource, Rect rectangle)
	{
		ImageBrush imageBrush = new ImageBrush();
		imageBrush.CanBeInheritanceContext = false;
		imageBrush.ImageSource = imageSource;
		DrawGeometry(imageBrush, null, new RectangleGeometry(rectangle));
	}

	public override void DrawVideo(MediaPlayer video, Rect rectangle)
	{
		DrawGeometry(Brushes.Black, null, new RectangleGeometry(rectangle));
	}
}
