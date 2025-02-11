using System.ComponentModel;
using MS.Internal;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Positions child elements in sequential position from left to right, breaking content to the next line at the edge of the containing box. Subsequent ordering happens sequentially from top to bottom or from right to left, depending on the value of the <see cref="P:System.Windows.Controls.WrapPanel.Orientation" /> property.</summary>
public class WrapPanel : Panel
{
	private struct UVSize
	{
		internal double U;

		internal double V;

		private Orientation _orientation;

		internal double Width
		{
			get
			{
				if (_orientation != 0)
				{
					return V;
				}
				return U;
			}
			set
			{
				if (_orientation == Orientation.Horizontal)
				{
					U = value;
				}
				else
				{
					V = value;
				}
			}
		}

		internal double Height
		{
			get
			{
				if (_orientation != 0)
				{
					return U;
				}
				return V;
			}
			set
			{
				if (_orientation == Orientation.Horizontal)
				{
					V = value;
				}
				else
				{
					U = value;
				}
			}
		}

		internal UVSize(Orientation orientation, double width, double height)
		{
			U = (V = 0.0);
			_orientation = orientation;
			Width = width;
			Height = height;
		}

		internal UVSize(Orientation orientation)
		{
			U = (V = 0.0);
			_orientation = orientation;
		}
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.WrapPanel.ItemWidth" />  dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.WrapPanel.ItemWidth" />  dependency property.</returns>
	public static readonly DependencyProperty ItemWidthProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.WrapPanel.ItemHeight" />  dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.WrapPanel.ItemHeight" />  dependency property.</returns>
	public static readonly DependencyProperty ItemHeightProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.WrapPanel.Orientation" />  dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.WrapPanel.Orientation" />  dependency property.</returns>
	public static readonly DependencyProperty OrientationProperty;

	private Orientation _orientation;

	/// <summary>Gets or sets a value that specifies the width of all items that are contained within a <see cref="T:System.Windows.Controls.WrapPanel" />. </summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the uniform width of all items that are contained within the <see cref="T:System.Windows.Controls.WrapPanel" />. The default value is <see cref="F:System.Double.NaN" />.</returns>
	[TypeConverter(typeof(LengthConverter))]
	public double ItemWidth
	{
		get
		{
			return (double)GetValue(ItemWidthProperty);
		}
		set
		{
			SetValue(ItemWidthProperty, value);
		}
	}

	/// <summary>Gets or sets a value that specifies the height of all items that are contained within a <see cref="T:System.Windows.Controls.WrapPanel" />. </summary>
	/// <returns>The <see cref="T:System.Double" /> that represents the uniform height of all items that are contained within the <see cref="T:System.Windows.Controls.WrapPanel" />. The default value is <see cref="F:System.Double.NaN" />.</returns>
	[TypeConverter(typeof(LengthConverter))]
	public double ItemHeight
	{
		get
		{
			return (double)GetValue(ItemHeightProperty);
		}
		set
		{
			SetValue(ItemHeightProperty, value);
		}
	}

	/// <summary>Gets or sets a value that specifies the dimension in which child content is arranged. </summary>
	/// <returns>An <see cref="T:System.Windows.Controls.Orientation" /> value that represents the physical orientation of content within the <see cref="T:System.Windows.Controls.WrapPanel" /> as horizontal or vertical. The default value is <see cref="F:System.Windows.Controls.Orientation.Horizontal" />.</returns>
	public Orientation Orientation
	{
		get
		{
			return _orientation;
		}
		set
		{
			SetValue(OrientationProperty, value);
		}
	}

	static WrapPanel()
	{
		ItemWidthProperty = DependencyProperty.Register("ItemWidth", typeof(double), typeof(WrapPanel), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure), IsWidthHeightValid);
		ItemHeightProperty = DependencyProperty.Register("ItemHeight", typeof(double), typeof(WrapPanel), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure), IsWidthHeightValid);
		OrientationProperty = StackPanel.OrientationProperty.AddOwner(typeof(WrapPanel), new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure, OnOrientationChanged));
		ControlsTraceLogger.AddControl(TelemetryControls.WrapPanel);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.WrapPanel" /> class.</summary>
	public WrapPanel()
	{
		_orientation = (Orientation)OrientationProperty.GetDefaultValue(base.DependencyObjectType);
	}

	private static bool IsWidthHeightValid(object value)
	{
		double num = (double)value;
		if (!double.IsNaN(num))
		{
			if (num >= 0.0)
			{
				return !double.IsPositiveInfinity(num);
			}
			return false;
		}
		return true;
	}

	private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((WrapPanel)d)._orientation = (Orientation)e.NewValue;
	}

	/// <summary>Measures the child elements of a <see cref="T:System.Windows.Controls.WrapPanel" /> in anticipation of arranging them during the <see cref="M:System.Windows.Controls.WrapPanel.ArrangeOverride(System.Windows.Size)" /> pass.</summary>
	/// <returns>The <see cref="T:System.Windows.Size" /> that represents the desired size of the element.</returns>
	/// <param name="constraint">An upper limit <see cref="T:System.Windows.Size" /> that should not be exceeded.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		UVSize uVSize = new UVSize(Orientation);
		UVSize uVSize2 = new UVSize(Orientation);
		UVSize uVSize3 = new UVSize(Orientation, constraint.Width, constraint.Height);
		double itemWidth = ItemWidth;
		double itemHeight = ItemHeight;
		bool flag = !double.IsNaN(itemWidth);
		bool flag2 = !double.IsNaN(itemHeight);
		Size availableSize = new Size(flag ? itemWidth : constraint.Width, flag2 ? itemHeight : constraint.Height);
		UIElementCollection internalChildren = base.InternalChildren;
		int i = 0;
		for (int count = internalChildren.Count; i < count; i++)
		{
			UIElement uIElement = internalChildren[i];
			if (uIElement == null)
			{
				continue;
			}
			uIElement.Measure(availableSize);
			UVSize uVSize4 = new UVSize(Orientation, flag ? itemWidth : uIElement.DesiredSize.Width, flag2 ? itemHeight : uIElement.DesiredSize.Height);
			if (DoubleUtil.GreaterThan(uVSize.U + uVSize4.U, uVSize3.U))
			{
				uVSize2.U = Math.Max(uVSize.U, uVSize2.U);
				uVSize2.V += uVSize.V;
				uVSize = uVSize4;
				if (DoubleUtil.GreaterThan(uVSize4.U, uVSize3.U))
				{
					uVSize2.U = Math.Max(uVSize4.U, uVSize2.U);
					uVSize2.V += uVSize4.V;
					uVSize = new UVSize(Orientation);
				}
			}
			else
			{
				uVSize.U += uVSize4.U;
				uVSize.V = Math.Max(uVSize4.V, uVSize.V);
			}
		}
		uVSize2.U = Math.Max(uVSize.U, uVSize2.U);
		uVSize2.V += uVSize.V;
		return new Size(uVSize2.Width, uVSize2.Height);
	}

	/// <summary>Arranges the content of a <see cref="T:System.Windows.Controls.WrapPanel" /> element.</summary>
	/// <returns>The <see cref="T:System.Windows.Size" /> that represents the arranged size of this <see cref="T:System.Windows.Controls.WrapPanel" /> element and its children.</returns>
	/// <param name="finalSize">The <see cref="T:System.Windows.Size" /> that this element should use to arrange its child elements.</param>
	protected override Size ArrangeOverride(Size finalSize)
	{
		int num = 0;
		double itemWidth = ItemWidth;
		double itemHeight = ItemHeight;
		double num2 = 0.0;
		double itemU = ((Orientation == Orientation.Horizontal) ? itemWidth : itemHeight);
		UVSize uVSize = new UVSize(Orientation);
		UVSize uVSize2 = new UVSize(Orientation, finalSize.Width, finalSize.Height);
		bool flag = !double.IsNaN(itemWidth);
		bool flag2 = !double.IsNaN(itemHeight);
		bool useItemU = ((Orientation == Orientation.Horizontal) ? flag : flag2);
		UIElementCollection internalChildren = base.InternalChildren;
		int i = 0;
		for (int count = internalChildren.Count; i < count; i++)
		{
			UIElement uIElement = internalChildren[i];
			if (uIElement == null)
			{
				continue;
			}
			UVSize uVSize3 = new UVSize(Orientation, flag ? itemWidth : uIElement.DesiredSize.Width, flag2 ? itemHeight : uIElement.DesiredSize.Height);
			if (DoubleUtil.GreaterThan(uVSize.U + uVSize3.U, uVSize2.U))
			{
				arrangeLine(num2, uVSize.V, num, i, useItemU, itemU);
				num2 += uVSize.V;
				uVSize = uVSize3;
				if (DoubleUtil.GreaterThan(uVSize3.U, uVSize2.U))
				{
					arrangeLine(num2, uVSize3.V, i, ++i, useItemU, itemU);
					num2 += uVSize3.V;
					uVSize = new UVSize(Orientation);
				}
				num = i;
			}
			else
			{
				uVSize.U += uVSize3.U;
				uVSize.V = Math.Max(uVSize3.V, uVSize.V);
			}
		}
		if (num < internalChildren.Count)
		{
			arrangeLine(num2, uVSize.V, num, internalChildren.Count, useItemU, itemU);
		}
		return finalSize;
	}

	private void arrangeLine(double v, double lineV, int start, int end, bool useItemU, double itemU)
	{
		double num = 0.0;
		bool flag = Orientation == Orientation.Horizontal;
		UIElementCollection internalChildren = base.InternalChildren;
		for (int i = start; i < end; i++)
		{
			UIElement uIElement = internalChildren[i];
			if (uIElement != null)
			{
				UVSize uVSize = new UVSize(Orientation, uIElement.DesiredSize.Width, uIElement.DesiredSize.Height);
				double num2 = (useItemU ? itemU : uVSize.U);
				uIElement.Arrange(new Rect(flag ? num : v, flag ? v : num, flag ? num2 : lineV, flag ? lineV : num2));
				num += num2;
			}
		}
	}
}
