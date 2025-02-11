using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationFramework;

namespace System.Windows.Controls.Primitives;

/// <summary>Represents a control primitive that handles the positioning and sizing of a <see cref="T:System.Windows.Controls.Primitives.Thumb" /> control and two <see cref="T:System.Windows.Controls.Primitives.RepeatButton" /> controls that are used to set a <see cref="P:System.Windows.Controls.Primitives.Track.Value" />. </summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public class Track : FrameworkElement
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.Track.Orientation" /> dependency property. </summary>
	[CommonDependencyProperty]
	public static readonly DependencyProperty OrientationProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.Track.Minimum" /> dependency property. </summary>
	[CommonDependencyProperty]
	public static readonly DependencyProperty MinimumProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.Track.Maximum" /> dependency property. </summary>
	[CommonDependencyProperty]
	public static readonly DependencyProperty MaximumProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.Track.Value" /> dependency property. </summary>
	[CommonDependencyProperty]
	public static readonly DependencyProperty ValueProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.Track.ViewportSize" /> dependency property. </summary>
	[CommonDependencyProperty]
	public static readonly DependencyProperty ViewportSizeProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.Track.IsDirectionReversed" /> dependency property. </summary>
	[CommonDependencyProperty]
	public static readonly DependencyProperty IsDirectionReversedProperty;

	private RepeatButton _increaseButton;

	private RepeatButton _decreaseButton;

	private Thumb _thumb;

	private Visual[] _visualChildren;

	private double _density = double.NaN;

	private double _thumbCenterOffset = double.NaN;

	/// <summary>Gets the <see cref="T:System.Windows.Controls.Primitives.RepeatButton" /> that decreases the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> property of the <see cref="T:System.Windows.Controls.Primitives.Track" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Controls.Primitives.RepeatButton" /> that reduces the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> of the <see cref="T:System.Windows.Controls.Primitives.Track" /> control when the <see cref="T:System.Windows.Controls.Primitives.RepeatButton" /> is pressed. The default is a <see cref="T:System.Windows.Controls.Primitives.RepeatButton" /> control that has default settings.</returns>
	public RepeatButton DecreaseRepeatButton
	{
		get
		{
			return _decreaseButton;
		}
		set
		{
			if (_increaseButton == value)
			{
				throw new NotSupportedException(SR.Track_SameButtons);
			}
			UpdateComponent(_decreaseButton, value);
			_decreaseButton = value;
			if (_decreaseButton != null)
			{
				CommandManager.InvalidateRequerySuggested();
			}
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> control that is used to change the <see cref="P:System.Windows.Controls.Primitives.Track.Value" /> of a <see cref="T:System.Windows.Controls.Primitives.Track" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Controls.Primitives.Thumb" /> control that is used with the <see cref="T:System.Windows.Controls.Primitives.Track" />. The default is a <see cref="T:System.Windows.Controls.Primitives.Thumb" /> control that has default settings.</returns>
	public Thumb Thumb
	{
		get
		{
			return _thumb;
		}
		set
		{
			UpdateComponent(_thumb, value);
			_thumb = value;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.Controls.Primitives.RepeatButton" /> that increases the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> property of the <see cref="T:System.Windows.Controls.Primitives.Track" /> class.</summary>
	/// <returns>The <see cref="T:System.Windows.Controls.Primitives.RepeatButton" /> that increases the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> of the <see cref="T:System.Windows.Controls.Primitives.Track" /> control when the <see cref="T:System.Windows.Controls.Primitives.RepeatButton" /> is pressed. The default is a <see cref="T:System.Windows.Controls.Primitives.RepeatButton" /> control that has default settings.</returns>
	public RepeatButton IncreaseRepeatButton
	{
		get
		{
			return _increaseButton;
		}
		set
		{
			if (_decreaseButton == value)
			{
				throw new NotSupportedException(SR.Track_SameButtons);
			}
			UpdateComponent(_increaseButton, value);
			_increaseButton = value;
			if (_increaseButton != null)
			{
				CommandManager.InvalidateRequerySuggested();
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Controls.Primitives.Track" /> is displayed horizontally or vertically.  </summary>
	/// <returns>An <see cref="T:System.Windows.Controls.Orientation" /> enumeration value that indicates whether the <see cref="T:System.Windows.Controls.Primitives.Track" /> is displayed horizontally or vertically. The default is <see cref="F:System.Windows.Controls.Orientation.Horizontal" />.</returns>
	public Orientation Orientation
	{
		get
		{
			return (Orientation)GetValue(OrientationProperty);
		}
		set
		{
			SetValue(OrientationProperty, value);
		}
	}

	/// <summary>Gets or sets the minimum possible <see cref="P:System.Windows.Controls.Primitives.Track.Value" /> of the <see cref="T:System.Windows.Controls.Primitives.Track" />.  </summary>
	/// <returns>The smallest allowable <see cref="P:System.Windows.Controls.Primitives.Track.Value" /> for the <see cref="T:System.Windows.Controls.Primitives.Track" />. The default is 0.</returns>
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

	/// <summary>Gets or sets the maximum possible <see cref="P:System.Windows.Controls.Primitives.Track.Value" /> of the <see cref="T:System.Windows.Controls.Primitives.Track" />.  </summary>
	/// <returns>The largest allowable <see cref="P:System.Windows.Controls.Primitives.Track.Value" /> for the <see cref="T:System.Windows.Controls.Primitives.Track" />. The default is 1.</returns>
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

	/// <summary>Gets or sets the current value of the <see cref="T:System.Windows.Controls.Primitives.Track" /> as determined by the position of the <see cref="P:System.Windows.Controls.Primitives.Track.Thumb" /> control.  </summary>
	/// <returns>The current value of the <see cref="T:System.Windows.Controls.Primitives.Track" />. The default is 0.</returns>
	public double Value
	{
		get
		{
			return (double)GetValue(ValueProperty);
		}
		set
		{
			SetValue(ValueProperty, value);
		}
	}

	/// <summary>Gets or sets the size of the part of the scrollable content that is visible.  </summary>
	/// <returns>The size of the visible area of the scrollable content. The default is <see cref="F:System.Double.NaN" />, which means that the content size is not defined.</returns>
	public double ViewportSize
	{
		get
		{
			return (double)GetValue(ViewportSizeProperty);
		}
		set
		{
			SetValue(ViewportSizeProperty, value);
		}
	}

	/// <summary>Gets or sets whether the direction of increasing <see cref="P:System.Windows.Controls.Primitives.Track.Value" /> is reversed from the default direction.  </summary>
	/// <returns>true if the <see cref="P:System.Windows.Controls.Primitives.Track.IncreaseRepeatButton" /> and the <see cref="P:System.Windows.Controls.Primitives.Track.DecreaseRepeatButton" /> exchanged positions and the direction of increasing value is reversed; otherwise false. The default is false.</returns>
	public bool IsDirectionReversed
	{
		get
		{
			return (bool)GetValue(IsDirectionReversedProperty);
		}
		set
		{
			SetValue(IsDirectionReversedProperty, value);
		}
	}

	/// <summary>Gets the number of child elements of a <see cref="T:System.Windows.Controls.Primitives.Track" />.</summary>
	/// <returns>An integer between 0 and 2 that specifies the number of child elements in the <see cref="T:System.Windows.Controls.Primitives.Track" />.</returns>
	protected override int VisualChildrenCount
	{
		get
		{
			if (_visualChildren == null || _visualChildren[0] == null)
			{
				return 0;
			}
			if (_visualChildren[1] == null)
			{
				return 1;
			}
			if (_visualChildren[2] != null)
			{
				return 3;
			}
			return 2;
		}
	}

	private double ThumbCenterOffset
	{
		get
		{
			return _thumbCenterOffset;
		}
		set
		{
			_thumbCenterOffset = value;
		}
	}

	private double Density
	{
		get
		{
			return _density;
		}
		set
		{
			_density = value;
		}
	}

	internal override int EffectiveValuesInitialSize => 28;

	static Track()
	{
		OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(Track), new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure), ScrollBar.IsValidOrientation);
		MinimumProperty = RangeBase.MinimumProperty.AddOwner(typeof(Track), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsArrange));
		MaximumProperty = RangeBase.MaximumProperty.AddOwner(typeof(Track), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsArrange));
		ValueProperty = RangeBase.ValueProperty.AddOwner(typeof(Track), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
		ViewportSizeProperty = DependencyProperty.Register("ViewportSize", typeof(double), typeof(Track), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsArrange), IsValidViewport);
		IsDirectionReversedProperty = DependencyProperty.Register("IsDirectionReversed", typeof(bool), typeof(Track), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		UIElement.IsEnabledProperty.OverrideMetadata(typeof(Track), new UIPropertyMetadata(OnIsEnabledChanged));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.Track" /> class.</summary>
	public Track()
	{
	}

	/// <summary>Calculates the distance from the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> to a specified point along the <see cref="T:System.Windows.Controls.Primitives.Track" />.</summary>
	/// <returns>The distance between the <see cref="P:System.Windows.Controls.Primitives.Track.Thumb" /> and the specified <paramref name="pt" /> value.</returns>
	/// <param name="pt">The specified point.</param>
	public virtual double ValueFromPoint(Point pt)
	{
		double val = ((Orientation != 0) ? (Value + ValueFromDistance(pt.X - base.RenderSize.Width * 0.5, pt.Y - ThumbCenterOffset)) : (Value + ValueFromDistance(pt.X - ThumbCenterOffset, pt.Y - base.RenderSize.Height * 0.5)));
		return Math.Max(Minimum, Math.Min(Maximum, val));
	}

	/// <summary>Calculates the change in the <see cref="P:System.Windows.Controls.Primitives.Track.Value" /> of the <see cref="T:System.Windows.Controls.Primitives.Track" /> when the <see cref="P:System.Windows.Controls.Primitives.Track.Thumb" /> moves.</summary>
	/// <returns>The change in <see cref="P:System.Windows.Controls.Primitives.Track.Value" /> that corresponds to the displacement of the <see cref="P:System.Windows.Controls.Primitives.Track.Thumb" /> of the <see cref="T:System.Windows.Controls.Primitives.Track" />.</returns>
	/// <param name="horizontal">The horizontal displacement of the <see cref="P:System.Windows.Controls.Primitives.Track.Thumb" />.</param>
	/// <param name="vertical">The vertical displacement of the <see cref="P:System.Windows.Controls.Primitives.Track.Thumb" />.</param>
	public virtual double ValueFromDistance(double horizontal, double vertical)
	{
		double num = ((!IsDirectionReversed) ? 1 : (-1));
		if (Orientation == Orientation.Horizontal)
		{
			return num * horizontal * Density;
		}
		return -1.0 * num * vertical * Density;
	}

	private void UpdateComponent(Control oldValue, Control newValue)
	{
		if (oldValue == newValue)
		{
			return;
		}
		if (_visualChildren == null)
		{
			_visualChildren = new Visual[3];
		}
		if (oldValue != null)
		{
			RemoveVisualChild(oldValue);
		}
		int i = 0;
		while (i < 3 && _visualChildren[i] != null)
		{
			if (_visualChildren[i] == oldValue)
			{
				for (; i < 2 && _visualChildren[i + 1] != null; i++)
				{
					_visualChildren[i] = _visualChildren[i + 1];
				}
			}
			else
			{
				i++;
			}
		}
		_visualChildren[i] = newValue;
		AddVisualChild(newValue);
		InvalidateMeasure();
		InvalidateArrange();
	}

	private static bool IsValidViewport(object o)
	{
		double num = (double)o;
		if (!(num >= 0.0))
		{
			return double.IsNaN(num);
		}
		return true;
	}

	/// <summary>Gets the child of the <see cref="T:System.Windows.Controls.Primitives.Track" /> at the specified index.</summary>
	/// <returns>Returns the object of the <see cref="T:System.Windows.Controls.Primitives.Track" /> at the specified index in the list of visual child elements. The index must be a number between zero (0) and the value of the <see cref="P:System.Windows.Controls.Primitives.Track.VisualChildrenCount" /> property minus one (1).</returns>
	/// <param name="index">The index of the child.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The specified index is greater than the value of the <see cref="P:System.Windows.Controls.Primitives.Track.VisualChildrenCount" /> property minus one (1).</exception>
	protected override Visual GetVisualChild(int index)
	{
		if (_visualChildren == null || _visualChildren[index] == null)
		{
			throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
		}
		return _visualChildren[index];
	}

	/// <summary>Measures the layout size that is required for the <see cref="T:System.Windows.Controls.Primitives.Track" /> and its components.</summary>
	/// <returns>The layout <see cref="T:System.Windows.Size" /> that is required by the <see cref="T:System.Windows.Controls.Primitives.Track" />.</returns>
	/// <param name="availableSize">The size of the available space for the track.</param>
	protected override Size MeasureOverride(Size availableSize)
	{
		Size result = new Size(0.0, 0.0);
		if (Thumb != null)
		{
			Thumb.Measure(availableSize);
			result = Thumb.DesiredSize;
		}
		if (!double.IsNaN(ViewportSize))
		{
			if (Orientation == Orientation.Vertical)
			{
				result.Height = 0.0;
			}
			else
			{
				result.Width = 0.0;
			}
		}
		return result;
	}

	private static void CoerceLength(ref double componentLength, double trackLength)
	{
		if (componentLength < 0.0)
		{
			componentLength = 0.0;
		}
		else if (componentLength > trackLength || double.IsNaN(componentLength))
		{
			componentLength = trackLength;
		}
	}

	/// <summary>Creates the layout for the <see cref="T:System.Windows.Controls.Primitives.Track" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Size" /> to use for the <see cref="T:System.Windows.Controls.Primitives.Track" /> content.</returns>
	/// <param name="arrangeSize">The area that is provided for the <see cref="T:System.Windows.Controls.Primitives.Track" />.</param>
	protected override Size ArrangeOverride(Size arrangeSize)
	{
		bool flag = Orientation == Orientation.Vertical;
		double viewportSize = Math.Max(0.0, ViewportSize);
		double decreaseButtonLength;
		double thumbLength;
		double increaseButtonLength;
		if (double.IsNaN(ViewportSize))
		{
			ComputeSliderLengths(arrangeSize, flag, out decreaseButtonLength, out thumbLength, out increaseButtonLength);
		}
		else if (!ComputeScrollBarLengths(arrangeSize, viewportSize, flag, out decreaseButtonLength, out thumbLength, out increaseButtonLength))
		{
			return arrangeSize;
		}
		Point location = default(Point);
		Size size = arrangeSize;
		bool isDirectionReversed = IsDirectionReversed;
		if (flag)
		{
			CoerceLength(ref decreaseButtonLength, arrangeSize.Height);
			CoerceLength(ref increaseButtonLength, arrangeSize.Height);
			CoerceLength(ref thumbLength, arrangeSize.Height);
			location.Y = (isDirectionReversed ? (decreaseButtonLength + thumbLength) : 0.0);
			size.Height = increaseButtonLength;
			if (IncreaseRepeatButton != null)
			{
				IncreaseRepeatButton.Arrange(new Rect(location, size));
			}
			location.Y = (isDirectionReversed ? 0.0 : (increaseButtonLength + thumbLength));
			size.Height = decreaseButtonLength;
			if (DecreaseRepeatButton != null)
			{
				DecreaseRepeatButton.Arrange(new Rect(location, size));
			}
			location.Y = (isDirectionReversed ? decreaseButtonLength : increaseButtonLength);
			size.Height = thumbLength;
			if (Thumb != null)
			{
				Thumb.Arrange(new Rect(location, size));
			}
			ThumbCenterOffset = location.Y + thumbLength * 0.5;
		}
		else
		{
			CoerceLength(ref decreaseButtonLength, arrangeSize.Width);
			CoerceLength(ref increaseButtonLength, arrangeSize.Width);
			CoerceLength(ref thumbLength, arrangeSize.Width);
			location.X = (isDirectionReversed ? (increaseButtonLength + thumbLength) : 0.0);
			size.Width = decreaseButtonLength;
			if (DecreaseRepeatButton != null)
			{
				DecreaseRepeatButton.Arrange(new Rect(location, size));
			}
			location.X = (isDirectionReversed ? 0.0 : (decreaseButtonLength + thumbLength));
			size.Width = increaseButtonLength;
			if (IncreaseRepeatButton != null)
			{
				IncreaseRepeatButton.Arrange(new Rect(location, size));
			}
			location.X = (isDirectionReversed ? increaseButtonLength : decreaseButtonLength);
			size.Width = thumbLength;
			if (Thumb != null)
			{
				Thumb.Arrange(new Rect(location, size));
			}
			ThumbCenterOffset = location.X + thumbLength * 0.5;
		}
		return arrangeSize;
	}

	private void ComputeSliderLengths(Size arrangeSize, bool isVertical, out double decreaseButtonLength, out double thumbLength, out double increaseButtonLength)
	{
		double minimum = Minimum;
		double num = Math.Max(0.0, Maximum - minimum);
		double num2 = Math.Min(num, Value - minimum);
		double num3;
		if (isVertical)
		{
			num3 = arrangeSize.Height;
			thumbLength = ((Thumb == null) ? 0.0 : Thumb.DesiredSize.Height);
		}
		else
		{
			num3 = arrangeSize.Width;
			thumbLength = ((Thumb == null) ? 0.0 : Thumb.DesiredSize.Width);
		}
		CoerceLength(ref thumbLength, num3);
		double num4 = num3 - thumbLength;
		decreaseButtonLength = num4 * num2 / num;
		CoerceLength(ref decreaseButtonLength, num4);
		increaseButtonLength = num4 - decreaseButtonLength;
		CoerceLength(ref increaseButtonLength, num4);
		Density = num / num4;
	}

	private bool ComputeScrollBarLengths(Size arrangeSize, double viewportSize, bool isVertical, out double decreaseButtonLength, out double thumbLength, out double increaseButtonLength)
	{
		double minimum = Minimum;
		double num = Math.Max(0.0, Maximum - minimum);
		double num2 = Math.Min(num, Value - minimum);
		double num3 = Math.Max(0.0, num) + viewportSize;
		double num4;
		double val;
		if (isVertical)
		{
			num4 = arrangeSize.Height;
			object obj = TryFindResource(SystemParameters.VerticalScrollBarButtonHeightKey);
			val = Math.Floor(((obj is double) ? ((double)obj) : SystemParameters.VerticalScrollBarButtonHeight) * 0.5);
		}
		else
		{
			num4 = arrangeSize.Width;
			object obj2 = TryFindResource(SystemParameters.HorizontalScrollBarButtonWidthKey);
			val = Math.Floor(((obj2 is double) ? ((double)obj2) : SystemParameters.HorizontalScrollBarButtonWidth) * 0.5);
		}
		thumbLength = num4 * viewportSize / num3;
		CoerceLength(ref thumbLength, num4);
		thumbLength = Math.Max(val, thumbLength);
		bool num5 = DoubleUtil.LessThanOrClose(num, 0.0);
		bool flag = thumbLength > num4;
		if (num5 || flag)
		{
			if (base.Visibility != Visibility.Hidden)
			{
				base.Visibility = Visibility.Hidden;
			}
			ThumbCenterOffset = double.NaN;
			Density = double.NaN;
			decreaseButtonLength = 0.0;
			increaseButtonLength = 0.0;
			return false;
		}
		if (base.Visibility != 0)
		{
			base.Visibility = Visibility.Visible;
		}
		double num6 = num4 - thumbLength;
		decreaseButtonLength = num6 * num2 / num;
		CoerceLength(ref decreaseButtonLength, num6);
		increaseButtonLength = num6 - decreaseButtonLength;
		CoerceLength(ref increaseButtonLength, num6);
		Density = num / num6;
		return true;
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

	private void BindChildToTemplatedParent(FrameworkElement element, DependencyProperty target, DependencyProperty source)
	{
		if (element != null && !element.HasNonDefaultValue(target))
		{
			Binding binding = new Binding();
			binding.Source = base.TemplatedParent;
			binding.Path = new PropertyPath(source);
			element.SetBinding(target, binding);
		}
	}

	internal override void OnPreApplyTemplate()
	{
		base.OnPreApplyTemplate();
		if (base.TemplatedParent is RangeBase rangeBase)
		{
			BindToTemplatedParent(MinimumProperty, RangeBase.MinimumProperty);
			BindToTemplatedParent(MaximumProperty, RangeBase.MaximumProperty);
			BindToTemplatedParent(ValueProperty, RangeBase.ValueProperty);
			if (rangeBase is ScrollBar)
			{
				BindToTemplatedParent(ViewportSizeProperty, ScrollBar.ViewportSizeProperty);
				BindToTemplatedParent(OrientationProperty, ScrollBar.OrientationProperty);
			}
			else if (rangeBase is Slider)
			{
				BindToTemplatedParent(OrientationProperty, Slider.OrientationProperty);
				BindToTemplatedParent(IsDirectionReversedProperty, Slider.IsDirectionReversedProperty);
				BindChildToTemplatedParent(DecreaseRepeatButton, RepeatButton.DelayProperty, Slider.DelayProperty);
				BindChildToTemplatedParent(DecreaseRepeatButton, RepeatButton.IntervalProperty, Slider.IntervalProperty);
				BindChildToTemplatedParent(IncreaseRepeatButton, RepeatButton.DelayProperty, Slider.DelayProperty);
				BindChildToTemplatedParent(IncreaseRepeatButton, RepeatButton.IntervalProperty, Slider.IntervalProperty);
			}
		}
	}

	private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if ((bool)e.NewValue)
		{
			Mouse.Synchronize();
		}
	}
}
