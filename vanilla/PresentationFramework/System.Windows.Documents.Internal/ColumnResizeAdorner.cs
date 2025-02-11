using System.Windows.Media;

namespace System.Windows.Documents.Internal;

internal class ColumnResizeAdorner : Adorner
{
	private double _x;

	private double _top;

	private double _height;

	private Pen _pen;

	private AdornerLayer _adornerLayer;

	internal ColumnResizeAdorner(UIElement scope)
		: base(scope)
	{
		_pen = new Pen(new SolidColorBrush(Colors.LightSlateGray), 2.0);
		_x = double.NaN;
		_top = double.NaN;
		_height = double.NaN;
	}

	public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
	{
		GeneralTransformGroup generalTransformGroup = new GeneralTransformGroup();
		TranslateTransform value = new TranslateTransform(_x, _top);
		generalTransformGroup.Children.Add(value);
		if (transform != null)
		{
			generalTransformGroup.Children.Add(transform);
		}
		return generalTransformGroup;
	}

	protected override void OnRender(DrawingContext drawingContext)
	{
		drawingContext.DrawLine(_pen, new Point(0.0, 0.0), new Point(0.0, _height));
	}

	internal void Update(double newX)
	{
		if (_x != newX)
		{
			_x = newX;
			if (VisualTreeHelper.GetParent(this) is AdornerLayer adornerLayer)
			{
				adornerLayer.Update(base.AdornedElement);
				adornerLayer.InvalidateVisual();
			}
		}
	}

	internal void Initialize(UIElement renderScope, double xPos, double yPos, double height)
	{
		_adornerLayer = AdornerLayer.GetAdornerLayer(renderScope);
		if (_adornerLayer != null)
		{
			_adornerLayer.Add(this);
		}
		_x = xPos;
		_top = yPos;
		_height = height;
	}

	internal void Uninitialize()
	{
		if (_adornerLayer != null)
		{
			_adornerLayer.Remove(this);
			_adornerLayer = null;
		}
	}
}
