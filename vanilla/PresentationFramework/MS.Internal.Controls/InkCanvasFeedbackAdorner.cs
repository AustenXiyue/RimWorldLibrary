using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MS.Internal.Controls;

internal class InkCanvasFeedbackAdorner : Adorner
{
	private InkCanvas _inkCanvas;

	private Size _frameSize = new Size(0.0, 0.0);

	private Rect _previousRect = Rect.Empty;

	private double _offsetX;

	private double _offsetY;

	private Pen _adornerBorderPen;

	private const int CornerResizeHandleSize = 8;

	private const double BorderMargin = 8.0;

	private InkCanvasFeedbackAdorner()
		: base(null)
	{
	}

	internal InkCanvasFeedbackAdorner(InkCanvas inkCanvas)
		: base(inkCanvas?.InnerCanvas)
	{
		if (inkCanvas == null)
		{
			throw new ArgumentNullException("inkCanvas");
		}
		_inkCanvas = inkCanvas;
		_adornerBorderPen = new Pen(Brushes.Black, 1.0);
		DoubleCollection dashes = new DoubleCollection { 4.5, 4.5 };
		_adornerBorderPen.DashStyle = new DashStyle(dashes, 2.25);
		_adornerBorderPen.DashCap = PenLineCap.Flat;
	}

	public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
	{
		if (transform == null)
		{
			throw new ArgumentNullException("transform");
		}
		VerifyAccess();
		GeneralTransformGroup generalTransformGroup = new GeneralTransformGroup();
		generalTransformGroup.Children.Add(transform);
		if (!DoubleUtil.AreClose(_offsetX, 0.0) || !DoubleUtil.AreClose(_offsetY, 0.0))
		{
			generalTransformGroup.Children.Add(new TranslateTransform(_offsetX, _offsetY));
		}
		return generalTransformGroup;
	}

	private void OnBoundsUpdated(Rect rect)
	{
		VerifyAccess();
		if (!(rect != _previousRect))
		{
			return;
		}
		bool flag = false;
		Size size;
		double num2;
		double num3;
		if (!rect.IsEmpty)
		{
			double num = 12.0;
			Rect rect2 = Rect.Inflate(rect, num, num);
			size = new Size(rect2.Width, rect2.Height);
			num2 = rect2.Left;
			num3 = rect2.Top;
		}
		else
		{
			size = new Size(0.0, 0.0);
			num2 = 0.0;
			num3 = 0.0;
		}
		if (_frameSize != size)
		{
			_frameSize = size;
			flag = true;
		}
		if (!DoubleUtil.AreClose(_offsetX, num2) || !DoubleUtil.AreClose(_offsetY, num3))
		{
			_offsetX = num2;
			_offsetY = num3;
			flag = true;
		}
		if (flag)
		{
			InvalidateMeasure();
			InvalidateVisual();
			if ((UIElement)VisualTreeHelper.GetParent(this) != null)
			{
				((UIElement)VisualTreeHelper.GetParent(this)).InvalidateArrange();
			}
		}
		_previousRect = rect;
	}

	protected override Size MeasureOverride(Size constraint)
	{
		VerifyAccess();
		return _frameSize;
	}

	protected override void OnRender(DrawingContext drawingContext)
	{
		drawingContext.DrawRectangle(null, _adornerBorderPen, new Rect(4.0, 4.0, _frameSize.Width - 8.0, _frameSize.Height - 8.0));
	}

	internal void UpdateBounds(Rect rect)
	{
		OnBoundsUpdated(rect);
	}
}
