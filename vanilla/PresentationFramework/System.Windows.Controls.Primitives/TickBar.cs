using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.KnownBoxes;

namespace System.Windows.Controls.Primitives;

/// <summary>Represents a control that draws a set of tick marks for a <see cref="T:System.Windows.Controls.Slider" /> control.</summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public class TickBar : FrameworkElement
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.TickBar.Fill" /> dependency property. This property is read-only.</summary>
	public static readonly DependencyProperty FillProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.TickBar.Minimum" /> dependency property. This property is read-only.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.TickBar.Minimum" /> dependency property.</returns>
	public static readonly DependencyProperty MinimumProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.TickBar.Maximum" /> dependency property. This property is read-only.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.TickBar.Maximum" /> dependency property.</returns>
	public static readonly DependencyProperty MaximumProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.TickBar.SelectionStart" /> dependency property. This property is read-only.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.TickBar.SelectionStart" /> dependency property.</returns>
	public static readonly DependencyProperty SelectionStartProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.TickBar.SelectionEnd" /> dependency property. This property is read-only.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.TickBar.SelectionEnd" /> dependency property.</returns>
	public static readonly DependencyProperty SelectionEndProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Slider.IsSelectionRangeEnabled" /> dependency property. This property is read-only.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Slider.IsSelectionRangeEnabled" /> dependency property.</returns>
	public static readonly DependencyProperty IsSelectionRangeEnabledProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.TickBar.TickFrequency" /> dependency property. This property is read-only.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.TickBar.TickFrequency" /> dependency property.</returns>
	public static readonly DependencyProperty TickFrequencyProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.TickBar.Ticks" /> dependency property. This property is read-only.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.TickBar.Ticks" /> dependency property.</returns>
	public static readonly DependencyProperty TicksProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.TickBar.IsDirectionReversed" /> dependency property. This property is read-only.</summary>
	public static readonly DependencyProperty IsDirectionReversedProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.TickBar.Placement" /> dependency property. This property is read-only.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.TickBar.Placement" /> dependency property.</returns>
	public static readonly DependencyProperty PlacementProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.TickBar.ReservedSpace" /> dependency property. This property is read-only.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.TickBar.ReservedSpace" /> dependency property.</returns>
	public static readonly DependencyProperty ReservedSpaceProperty;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Brush" /> that is used to draw the tick marks.  </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Brush" /> to use to draw tick marks. The default value is null.</returns>
	public Brush Fill
	{
		get
		{
			return (Brush)GetValue(FillProperty);
		}
		set
		{
			SetValue(FillProperty, value);
		}
	}

	/// <summary>Gets or sets the minimum value that is possible for a tick mark.  </summary>
	/// <returns>The smallest possible value for a tick mark. The default value is zero (0.0).</returns>
	[Bindable(true)]
	[Category("Appearance")]
	public double Minimum
	{
		get
		{
			return (double)GetValue(MinimumProperty);
		}
		set
		{
			SetValue(MinimumProperty, value);
		}
	}

	/// <summary>Gets or sets the maximum value that is possible for a tick mark.  </summary>
	/// <returns>The largest possible value for a tick mark. The default value is 100.0.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	public double Maximum
	{
		get
		{
			return (double)GetValue(MaximumProperty);
		}
		set
		{
			SetValue(MaximumProperty, value);
		}
	}

	/// <summary>Gets or sets the start point of a selection.   </summary>
	/// <returns>The first value in a range of values for a selection. The default value is -1.0.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	public double SelectionStart
	{
		get
		{
			return (double)GetValue(SelectionStartProperty);
		}
		set
		{
			SetValue(SelectionStartProperty, value);
		}
	}

	/// <summary>Gets or sets the end point of a selection.  </summary>
	/// <returns>The last value in a range of values for a selection. The default value is -1.0.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	public double SelectionEnd
	{
		get
		{
			return (double)GetValue(SelectionEndProperty);
		}
		set
		{
			SetValue(SelectionEndProperty, value);
		}
	}

	/// <summary>Gets or sets whether the <see cref="T:System.Windows.Controls.Primitives.TickBar" /> displays a selection range.   </summary>
	/// <returns>true if a selection range is displayed; otherwise, false. The default value is false.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	public bool IsSelectionRangeEnabled
	{
		get
		{
			return (bool)GetValue(IsSelectionRangeEnabledProperty);
		}
		set
		{
			SetValue(IsSelectionRangeEnabledProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets the interval between tick marks.  </summary>
	/// <returns>The distance between tick marks. The default value is one (1.0).</returns>
	[Bindable(true)]
	[Category("Appearance")]
	public double TickFrequency
	{
		get
		{
			return (double)GetValue(TickFrequencyProperty);
		}
		set
		{
			SetValue(TickFrequencyProperty, value);
		}
	}

	/// <summary>Gets or sets the positions of the tick marks.  </summary>
	/// <returns>A <see cref="T:System.Windows.Media.DoubleCollection" /> that specifies the locations of the tick marks. The default value is null.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	public DoubleCollection Ticks
	{
		get
		{
			return (DoubleCollection)GetValue(TicksProperty);
		}
		set
		{
			SetValue(TicksProperty, value);
		}
	}

	/// <summary>Gets or sets the direction of increasing value of the tick marks.  </summary>
	/// <returns>true if the direction of increasing value is to the left for a horizontal <see cref="T:System.Windows.Controls.Slider" /> or down for a vertical <see cref="T:System.Windows.Controls.Slider" />; otherwise, false. The default value is false.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	public bool IsDirectionReversed
	{
		get
		{
			return (bool)GetValue(IsDirectionReversedProperty);
		}
		set
		{
			SetValue(IsDirectionReversedProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets where tick marks appear  relative to a <see cref="T:System.Windows.Controls.Primitives.Track" /> of a <see cref="T:System.Windows.Controls.Slider" /> control.  </summary>
	/// <returns>A <see cref="T:System.Windows.Controls.Primitives.TickBarPlacement" /> enumeration value that identifies the position of the <see cref="T:System.Windows.Controls.Primitives.TickBar" /> in the <see cref="T:System.Windows.Style" /> layout of a <see cref="T:System.Windows.Controls.Slider" />. The default value is <see cref="F:System.Windows.Controls.Primitives.TickBarPlacement.Top" />.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	public TickBarPlacement Placement
	{
		get
		{
			return (TickBarPlacement)GetValue(PlacementProperty);
		}
		set
		{
			SetValue(PlacementProperty, value);
		}
	}

	/// <summary>Gets or sets a space buffer for the area that contains the tick marks that are specified for a <see cref="T:System.Windows.Controls.Primitives.TickBar" />.  </summary>
	/// <returns>A value that represents the total buffer area on either side of the row or column of tick marks. The default value is zero (0.0).</returns>
	[Bindable(true)]
	[Category("Appearance")]
	public double ReservedSpace
	{
		get
		{
			return (double)GetValue(ReservedSpaceProperty);
		}
		set
		{
			SetValue(ReservedSpaceProperty, value);
		}
	}

	static TickBar()
	{
		FillProperty = DependencyProperty.Register("Fill", typeof(Brush), typeof(TickBar), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, null, null));
		MinimumProperty = RangeBase.MinimumProperty.AddOwner(typeof(TickBar), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
		MaximumProperty = RangeBase.MaximumProperty.AddOwner(typeof(TickBar), new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.AffectsRender));
		SelectionStartProperty = Slider.SelectionStartProperty.AddOwner(typeof(TickBar), new FrameworkPropertyMetadata(-1.0, FrameworkPropertyMetadataOptions.AffectsRender));
		SelectionEndProperty = Slider.SelectionEndProperty.AddOwner(typeof(TickBar), new FrameworkPropertyMetadata(-1.0, FrameworkPropertyMetadataOptions.AffectsRender));
		IsSelectionRangeEnabledProperty = Slider.IsSelectionRangeEnabledProperty.AddOwner(typeof(TickBar), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsRender));
		TickFrequencyProperty = Slider.TickFrequencyProperty.AddOwner(typeof(TickBar), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));
		TicksProperty = Slider.TicksProperty.AddOwner(typeof(TickBar), new FrameworkPropertyMetadata(new FreezableDefaultValueFactory(DoubleCollection.Empty), FrameworkPropertyMetadataOptions.AffectsRender));
		IsDirectionReversedProperty = Slider.IsDirectionReversedProperty.AddOwner(typeof(TickBar), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsRender));
		PlacementProperty = DependencyProperty.Register("Placement", typeof(TickBarPlacement), typeof(TickBar), new FrameworkPropertyMetadata(TickBarPlacement.Top, FrameworkPropertyMetadataOptions.AffectsRender), IsValidTickBarPlacement);
		ReservedSpaceProperty = DependencyProperty.Register("ReservedSpace", typeof(double), typeof(TickBar), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
		UIElement.SnapsToDevicePixelsProperty.OverrideMetadata(typeof(TickBar), new FrameworkPropertyMetadata(true));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.TickBar" /> class. </summary>
	public TickBar()
	{
	}

	private static bool IsValidTickBarPlacement(object o)
	{
		TickBarPlacement tickBarPlacement = (TickBarPlacement)o;
		if (tickBarPlacement != 0 && tickBarPlacement != TickBarPlacement.Top && tickBarPlacement != TickBarPlacement.Right)
		{
			return tickBarPlacement == TickBarPlacement.Bottom;
		}
		return true;
	}

	/// <summary>Draws the tick marks for a <see cref="T:System.Windows.Controls.Slider" /> control. </summary>
	/// <param name="dc">The <see cref="T:System.Windows.Media.DrawingContext" /> that is used to draw the ticks.</param>
	protected override void OnRender(DrawingContext dc)
	{
		Size size = new Size(base.ActualWidth, base.ActualHeight);
		double num = Maximum - Minimum;
		double num2 = 0.0;
		double num3 = 1.0;
		double num4 = 1.0;
		Point point = new Point(0.0, 0.0);
		Point point2 = new Point(0.0, 0.0);
		double num5 = ReservedSpace * 0.5;
		switch (Placement)
		{
		case TickBarPlacement.Top:
			if (DoubleUtil.GreaterThanOrClose(ReservedSpace, size.Width))
			{
				return;
			}
			size.Width -= ReservedSpace;
			num2 = 0.0 - size.Height;
			point = new Point(num5, size.Height);
			point2 = new Point(num5 + size.Width, size.Height);
			num3 = size.Width / num;
			num4 = 1.0;
			break;
		case TickBarPlacement.Bottom:
			if (DoubleUtil.GreaterThanOrClose(ReservedSpace, size.Width))
			{
				return;
			}
			size.Width -= ReservedSpace;
			num2 = size.Height;
			point = new Point(num5, 0.0);
			point2 = new Point(num5 + size.Width, 0.0);
			num3 = size.Width / num;
			num4 = 1.0;
			break;
		case TickBarPlacement.Left:
			if (DoubleUtil.GreaterThanOrClose(ReservedSpace, size.Height))
			{
				return;
			}
			size.Height -= ReservedSpace;
			num2 = 0.0 - size.Width;
			point = new Point(size.Width, size.Height + num5);
			point2 = new Point(size.Width, num5);
			num3 = size.Height / num * -1.0;
			num4 = -1.0;
			break;
		case TickBarPlacement.Right:
			if (DoubleUtil.GreaterThanOrClose(ReservedSpace, size.Height))
			{
				return;
			}
			size.Height -= ReservedSpace;
			num2 = size.Width;
			point = new Point(0.0, size.Height + num5);
			point2 = new Point(0.0, num5);
			num3 = size.Height / num * -1.0;
			num4 = -1.0;
			break;
		}
		double num6 = num2 * 0.75;
		if (IsDirectionReversed)
		{
			num4 = 0.0 - num4;
			num3 *= -1.0;
			Point point3 = point;
			point = point2;
			point2 = point3;
		}
		Pen pen = new Pen(Fill, 1.0);
		bool snapsToDevicePixels = base.SnapsToDevicePixels;
		DoubleCollection doubleCollection = (snapsToDevicePixels ? new DoubleCollection() : null);
		DoubleCollection doubleCollection2 = (snapsToDevicePixels ? new DoubleCollection() : null);
		if (Placement == TickBarPlacement.Left || Placement == TickBarPlacement.Right)
		{
			double num7 = TickFrequency;
			if (num7 > 0.0)
			{
				double num8 = (Maximum - Minimum) / size.Height;
				if (num7 < num8)
				{
					num7 = num8;
				}
			}
			dc.DrawLine(pen, point, new Point(point.X + num2, point.Y));
			dc.DrawLine(pen, new Point(point.X, point2.Y), new Point(point.X + num2, point2.Y));
			if (snapsToDevicePixels)
			{
				doubleCollection.Add(point.X);
				doubleCollection2.Add(point.Y - 0.5);
				doubleCollection.Add(point.X + num2);
				doubleCollection2.Add(point2.Y - 0.5);
				doubleCollection.Add(point.X + num6);
			}
			DoubleCollection doubleCollection3 = null;
			if (GetValueSource(TicksProperty, null, out var hasModifiers) != BaseValueSourceInternal.Default || hasModifiers)
			{
				doubleCollection3 = Ticks;
			}
			if (doubleCollection3 != null && doubleCollection3.Count > 0)
			{
				for (int i = 0; i < doubleCollection3.Count; i++)
				{
					if (!DoubleUtil.LessThanOrClose(doubleCollection3[i], Minimum) && !DoubleUtil.GreaterThanOrClose(doubleCollection3[i], Maximum))
					{
						double num9 = (doubleCollection3[i] - Minimum) * num3 + point.Y;
						dc.DrawLine(pen, new Point(point.X, num9), new Point(point.X + num6, num9));
						if (snapsToDevicePixels)
						{
							doubleCollection2.Add(num9 - 0.5);
						}
					}
				}
			}
			else if (num7 > 0.0)
			{
				for (double num10 = num7; num10 < num; num10 += num7)
				{
					double num11 = num10 * num3 + point.Y;
					dc.DrawLine(pen, new Point(point.X, num11), new Point(point.X + num6, num11));
					if (snapsToDevicePixels)
					{
						doubleCollection2.Add(num11 - 0.5);
					}
				}
			}
			if (IsSelectionRangeEnabled)
			{
				double num12 = (SelectionStart - Minimum) * num3 + point.Y;
				Point point4 = new Point(point.X, num12);
				Point start = new Point(point.X + num6, num12);
				Point point5 = new Point(point.X + num6, num12 + Math.Abs(num6) * num4);
				PathSegment[] segments = new PathSegment[2]
				{
					new LineSegment(point5, isStroked: true),
					new LineSegment(point4, isStroked: true)
				};
				PathGeometry geometry = new PathGeometry(new PathFigure[1]
				{
					new PathFigure(start, segments, closed: true)
				});
				dc.DrawGeometry(Fill, pen, geometry);
				num12 = (SelectionEnd - Minimum) * num3 + point.Y;
				point4 = new Point(point.X, num12);
				start = new Point(point.X + num6, num12);
				point5 = new Point(point.X + num6, num12 - Math.Abs(num6) * num4);
				segments = new PathSegment[2]
				{
					new LineSegment(point5, isStroked: true),
					new LineSegment(point4, isStroked: true)
				};
				geometry = new PathGeometry(new PathFigure[1]
				{
					new PathFigure(start, segments, closed: true)
				});
				dc.DrawGeometry(Fill, pen, geometry);
			}
		}
		else
		{
			double num13 = TickFrequency;
			if (num13 > 0.0)
			{
				double num14 = (Maximum - Minimum) / size.Width;
				if (num13 < num14)
				{
					num13 = num14;
				}
			}
			dc.DrawLine(pen, point, new Point(point.X, point.Y + num2));
			dc.DrawLine(pen, new Point(point2.X, point.Y), new Point(point2.X, point.Y + num2));
			if (snapsToDevicePixels)
			{
				doubleCollection.Add(point.X - 0.5);
				doubleCollection2.Add(point.Y);
				doubleCollection.Add(point.X - 0.5);
				doubleCollection2.Add(point2.Y + num2);
				doubleCollection2.Add(point2.Y + num6);
			}
			DoubleCollection doubleCollection4 = null;
			if (GetValueSource(TicksProperty, null, out var hasModifiers2) != BaseValueSourceInternal.Default || hasModifiers2)
			{
				doubleCollection4 = Ticks;
			}
			if (doubleCollection4 != null && doubleCollection4.Count > 0)
			{
				for (int j = 0; j < doubleCollection4.Count; j++)
				{
					if (!DoubleUtil.LessThanOrClose(doubleCollection4[j], Minimum) && !DoubleUtil.GreaterThanOrClose(doubleCollection4[j], Maximum))
					{
						double num15 = (doubleCollection4[j] - Minimum) * num3 + point.X;
						dc.DrawLine(pen, new Point(num15, point.Y), new Point(num15, point.Y + num6));
						if (snapsToDevicePixels)
						{
							doubleCollection.Add(num15 - 0.5);
						}
					}
				}
			}
			else if (num13 > 0.0)
			{
				for (double num16 = num13; num16 < num; num16 += num13)
				{
					double num17 = num16 * num3 + point.X;
					dc.DrawLine(pen, new Point(num17, point.Y), new Point(num17, point.Y + num6));
					if (snapsToDevicePixels)
					{
						doubleCollection.Add(num17 - 0.5);
					}
				}
			}
			if (IsSelectionRangeEnabled)
			{
				double num18 = (SelectionStart - Minimum) * num3 + point.X;
				Point point6 = new Point(num18, point.Y);
				Point start2 = new Point(num18, point.Y + num6);
				Point point7 = new Point(num18 + Math.Abs(num6) * num4, point.Y + num6);
				PathSegment[] segments2 = new PathSegment[2]
				{
					new LineSegment(point7, isStroked: true),
					new LineSegment(point6, isStroked: true)
				};
				PathGeometry geometry2 = new PathGeometry(new PathFigure[1]
				{
					new PathFigure(start2, segments2, closed: true)
				});
				dc.DrawGeometry(Fill, pen, geometry2);
				num18 = (SelectionEnd - Minimum) * num3 + point.X;
				point6 = new Point(num18, point.Y);
				start2 = new Point(num18, point.Y + num6);
				point7 = new Point(num18 - Math.Abs(num6) * num4, point.Y + num6);
				segments2 = new PathSegment[2]
				{
					new LineSegment(point7, isStroked: true),
					new LineSegment(point6, isStroked: true)
				};
				geometry2 = new PathGeometry(new PathFigure[1]
				{
					new PathFigure(start2, segments2, closed: true)
				});
				dc.DrawGeometry(Fill, pen, geometry2);
			}
		}
		if (snapsToDevicePixels)
		{
			doubleCollection.Add(base.ActualWidth);
			doubleCollection2.Add(base.ActualHeight);
			base.VisualXSnappingGuidelines = doubleCollection;
			base.VisualYSnappingGuidelines = doubleCollection2;
		}
	}

	private void BindToTemplatedParent(DependencyProperty target, DependencyProperty source)
	{
		if (!HasNonDefaultValue(target))
		{
			Binding binding = new Binding();
			binding.RelativeSource = RelativeSource.TemplatedParent;
			binding.Path = new PropertyPath(source);
			SetBinding(target, binding);
		}
	}

	internal override void OnPreApplyTemplate()
	{
		base.OnPreApplyTemplate();
		if (!(base.TemplatedParent is Slider slider))
		{
			return;
		}
		BindToTemplatedParent(TicksProperty, Slider.TicksProperty);
		BindToTemplatedParent(TickFrequencyProperty, Slider.TickFrequencyProperty);
		BindToTemplatedParent(IsSelectionRangeEnabledProperty, Slider.IsSelectionRangeEnabledProperty);
		BindToTemplatedParent(SelectionStartProperty, Slider.SelectionStartProperty);
		BindToTemplatedParent(SelectionEndProperty, Slider.SelectionEndProperty);
		BindToTemplatedParent(MinimumProperty, RangeBase.MinimumProperty);
		BindToTemplatedParent(MaximumProperty, RangeBase.MaximumProperty);
		BindToTemplatedParent(IsDirectionReversedProperty, Slider.IsDirectionReversedProperty);
		if (!HasNonDefaultValue(ReservedSpaceProperty) && slider.Track != null)
		{
			Binding binding = new Binding();
			binding.Source = slider.Track.Thumb;
			if (slider.Orientation == Orientation.Horizontal)
			{
				binding.Path = new PropertyPath(FrameworkElement.ActualWidthProperty);
			}
			else
			{
				binding.Path = new PropertyPath(FrameworkElement.ActualHeightProperty);
			}
			SetBinding(ReservedSpaceProperty, binding);
		}
	}
}
