using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace System.Windows.Controls;

[TemplatePart(Name = "PART_VisualBrushCanvas", Type = typeof(Canvas))]
internal class DataGridColumnFloatingHeader : Control
{
	private DataGridColumnHeader _referenceHeader;

	private const string VisualBrushCanvasTemplateName = "PART_VisualBrushCanvas";

	private Canvas _visualBrushCanvas;

	internal DataGridColumnHeader ReferenceHeader
	{
		get
		{
			return _referenceHeader;
		}
		set
		{
			_referenceHeader = value;
		}
	}

	static DataGridColumnFloatingHeader()
	{
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridColumnFloatingHeader), new FrameworkPropertyMetadata(DataGridColumnHeader.ColumnFloatingHeaderStyleKey));
		FrameworkElement.WidthProperty.OverrideMetadata(typeof(DataGridColumnFloatingHeader), new FrameworkPropertyMetadata(OnWidthChanged, OnCoerceWidth));
		FrameworkElement.HeightProperty.OverrideMetadata(typeof(DataGridColumnFloatingHeader), new FrameworkPropertyMetadata(OnHeightChanged, OnCoerceHeight));
	}

	private static void OnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DataGridColumnFloatingHeader dataGridColumnFloatingHeader = (DataGridColumnFloatingHeader)d;
		double num = (double)e.NewValue;
		if (dataGridColumnFloatingHeader._visualBrushCanvas != null && !double.IsNaN(num) && dataGridColumnFloatingHeader._visualBrushCanvas.Background is VisualBrush { Viewbox: var viewbox } visualBrush)
		{
			visualBrush.Viewbox = new Rect(viewbox.X, viewbox.Y, num - dataGridColumnFloatingHeader.GetVisualCanvasMarginX(), viewbox.Height);
		}
	}

	private static object OnCoerceWidth(DependencyObject d, object baseValue)
	{
		double d2 = (double)baseValue;
		DataGridColumnFloatingHeader dataGridColumnFloatingHeader = (DataGridColumnFloatingHeader)d;
		if (dataGridColumnFloatingHeader._referenceHeader != null && double.IsNaN(d2))
		{
			return dataGridColumnFloatingHeader._referenceHeader.ActualWidth + dataGridColumnFloatingHeader.GetVisualCanvasMarginX();
		}
		return baseValue;
	}

	private static void OnHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DataGridColumnFloatingHeader dataGridColumnFloatingHeader = (DataGridColumnFloatingHeader)d;
		double num = (double)e.NewValue;
		if (dataGridColumnFloatingHeader._visualBrushCanvas != null && !double.IsNaN(num) && dataGridColumnFloatingHeader._visualBrushCanvas.Background is VisualBrush { Viewbox: var viewbox } visualBrush)
		{
			visualBrush.Viewbox = new Rect(viewbox.X, viewbox.Y, viewbox.Width, num - dataGridColumnFloatingHeader.GetVisualCanvasMarginY());
		}
	}

	private static object OnCoerceHeight(DependencyObject d, object baseValue)
	{
		double d2 = (double)baseValue;
		DataGridColumnFloatingHeader dataGridColumnFloatingHeader = (DataGridColumnFloatingHeader)d;
		if (dataGridColumnFloatingHeader._referenceHeader != null && double.IsNaN(d2))
		{
			return dataGridColumnFloatingHeader._referenceHeader.ActualHeight + dataGridColumnFloatingHeader.GetVisualCanvasMarginY();
		}
		return baseValue;
	}

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		_visualBrushCanvas = GetTemplateChild("PART_VisualBrushCanvas") as Canvas;
		UpdateVisualBrush();
	}

	private void UpdateVisualBrush()
	{
		if (_referenceHeader != null && _visualBrushCanvas != null)
		{
			VisualBrush visualBrush = new VisualBrush(_referenceHeader);
			visualBrush.ViewboxUnits = BrushMappingMode.Absolute;
			double width = base.Width;
			width = ((!double.IsNaN(width)) ? (width - GetVisualCanvasMarginX()) : _referenceHeader.ActualWidth);
			double height = base.Height;
			height = ((!double.IsNaN(height)) ? (height - GetVisualCanvasMarginY()) : _referenceHeader.ActualHeight);
			Vector offset = VisualTreeHelper.GetOffset(_referenceHeader);
			visualBrush.Viewbox = new Rect(offset.X, offset.Y, width, height);
			_visualBrushCanvas.Background = visualBrush;
		}
	}

	internal void ClearHeader()
	{
		_referenceHeader = null;
		if (_visualBrushCanvas != null)
		{
			_visualBrushCanvas.Background = null;
		}
	}

	private double GetVisualCanvasMarginX()
	{
		double num = 0.0;
		if (_visualBrushCanvas != null)
		{
			Thickness margin = _visualBrushCanvas.Margin;
			num += margin.Left;
			num += margin.Right;
		}
		return num;
	}

	private double GetVisualCanvasMarginY()
	{
		double num = 0.0;
		if (_visualBrushCanvas != null)
		{
			Thickness margin = _visualBrushCanvas.Margin;
			num += margin.Top;
			num += margin.Bottom;
		}
		return num;
	}
}
