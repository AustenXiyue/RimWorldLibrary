using System.ComponentModel;
using System.Globalization;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using MS.Internal;
using MS.Internal.Commands;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Represents a control that lets the user select from a range of values by moving a <see cref="P:System.Windows.Controls.Primitives.Track.Thumb" /> control along a <see cref="T:System.Windows.Controls.Primitives.Track" />.</summary>
[Localizability(LocalizationCategory.Ignore)]
[DefaultEvent("ValueChanged")]
[DefaultProperty("Value")]
[TemplatePart(Name = "PART_Track", Type = typeof(Track))]
[TemplatePart(Name = "PART_SelectionRange", Type = typeof(FrameworkElement))]
public class Slider : RangeBase
{
	private class SliderGesture : InputGesture
	{
		private Key _normal;

		private Key _inverted;

		private bool _forHorizontal;

		public SliderGesture(Key normal, Key inverted, bool forHorizontal)
		{
			_normal = normal;
			_inverted = inverted;
			_forHorizontal = forHorizontal;
		}

		public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
		{
			KeyEventArgs keyEventArgs = inputEventArgs as KeyEventArgs;
			Slider slider = targetElement as Slider;
			if (keyEventArgs != null && slider != null && Keyboard.Modifiers == ModifierKeys.None)
			{
				if (_normal == keyEventArgs.RealKey)
				{
					return !IsInverted(slider);
				}
				if (_inverted == keyEventArgs.RealKey)
				{
					return IsInverted(slider);
				}
			}
			return false;
		}

		private bool IsInverted(Slider slider)
		{
			if (_forHorizontal)
			{
				return slider.IsDirectionReversed != (slider.FlowDirection == FlowDirection.RightToLeft);
			}
			return slider.IsDirectionReversed;
		}
	}

	private static RoutedCommand _increaseLargeCommand;

	private static RoutedCommand _increaseSmallCommand;

	private static RoutedCommand _decreaseLargeCommand;

	private static RoutedCommand _decreaseSmallCommand;

	private static RoutedCommand _minimizeValueCommand;

	private static RoutedCommand _maximizeValueCommand;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Slider.Orientation" /> dependency property. </summary>
	public static readonly DependencyProperty OrientationProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Slider.IsDirectionReversed" /> dependency property. </summary>
	public static readonly DependencyProperty IsDirectionReversedProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Slider.Delay" /> dependency property. </summary>
	public static readonly DependencyProperty DelayProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Slider.Interval" /> dependency property. </summary>
	public static readonly DependencyProperty IntervalProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Slider.AutoToolTipPlacement" /> dependency property. </summary>
	public static readonly DependencyProperty AutoToolTipPlacementProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Slider.AutoToolTipPrecision" /> dependency property. </summary>
	public static readonly DependencyProperty AutoToolTipPrecisionProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Slider.IsSnapToTickEnabled" /> dependency property. </summary>
	public static readonly DependencyProperty IsSnapToTickEnabledProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Slider.TickPlacement" /> dependency property. </summary>
	public static readonly DependencyProperty TickPlacementProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Slider.TickFrequency" /> dependency property. </summary>
	public static readonly DependencyProperty TickFrequencyProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Slider.Ticks" /> dependency property. </summary>
	public static readonly DependencyProperty TicksProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Slider.IsSelectionRangeEnabled" /> dependency property. </summary>
	public static readonly DependencyProperty IsSelectionRangeEnabledProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Slider.SelectionStart" /> dependency property. </summary>
	public static readonly DependencyProperty SelectionStartProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Slider.SelectionEnd" /> dependency property. </summary>
	public static readonly DependencyProperty SelectionEndProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Slider.IsMoveToPointEnabled" /> dependency property. </summary>
	public static readonly DependencyProperty IsMoveToPointEnabledProperty;

	private const string TrackName = "PART_Track";

	private const string SelectionRangeElementName = "PART_SelectionRange";

	private FrameworkElement _selectionRangeElement;

	private Track _track;

	private ToolTip _autoToolTip;

	private object _thumbOriginalToolTip;

	private static DependencyObjectType _dType;

	/// <summary>Gets a command that increases the value of the slider by the same amount as the <see cref="P:System.Windows.Controls.Primitives.RangeBase.LargeChange" /> property.</summary>
	/// <returns>The <see cref="T:System.Windows.Input.RoutedCommand" /> that increases the value of the <see cref="F:System.Windows.Controls.Slider.SelectionStartProperty" /> by the same amount as the <see cref="P:System.Windows.Controls.Primitives.RangeBase.LargeChange" /> property. The default <see cref="T:System.Windows.Input.InputGesture" /> for this command is <see cref="F:System.Windows.Input.Key.PageUp" />. </returns>
	public static RoutedCommand IncreaseLarge => _increaseLargeCommand;

	/// <summary>Gets a command that decreases the value of the <see cref="T:System.Windows.Controls.Slider" /> by the same amount as the <see cref="P:System.Windows.Controls.Primitives.RangeBase.LargeChange" /> property.</summary>
	/// <returns>The <see cref="T:System.Windows.Input.RoutedCommand" /> that decreases the value of the <see cref="T:System.Windows.Controls.Slider" /> by the same amount as the <see cref="P:System.Windows.Controls.Primitives.RangeBase.LargeChange" /> property. The default <see cref="T:System.Windows.Input.InputGesture" /> is <see cref="F:System.Windows.Input.Key.PageDown" />.</returns>
	public static RoutedCommand DecreaseLarge => _decreaseLargeCommand;

	/// <summary>Gets a command that increases the value of the slider by the same amount as the <see cref="P:System.Windows.Controls.Primitives.RangeBase.SmallChange" /> property.</summary>
	/// <returns>Returns the <see cref="T:System.Windows.Input.RoutedCommand" /> that increases the value of the slider by the same amount as the <see cref="P:System.Windows.Controls.Primitives.RangeBase.SmallChange" /> property. The default <see cref="T:System.Windows.Input.InputGesture" /> objects for this command are <see cref="F:System.Windows.Input.Key.Up" /> and <see cref="F:System.Windows.Input.Key.Right" />. </returns>
	public static RoutedCommand IncreaseSmall => _increaseSmallCommand;

	/// <summary>Gets a command that decreases the value of the <see cref="T:System.Windows.Controls.Slider" /> by the same amount as the <see cref="P:System.Windows.Controls.Primitives.RangeBase.SmallChange" /> property.</summary>
	/// <returns>The <see cref="T:System.Windows.Input.RoutedCommand" /> that decreases the value of the <see cref="T:System.Windows.Controls.Slider" /> by the same amount as the <see cref="P:System.Windows.Controls.Primitives.RangeBase.SmallChange" /> property. The default <see cref="T:System.Windows.Input.InputGesture" /> objects are <see cref="F:System.Windows.Input.Key.Down" /> and <see cref="F:System.Windows.Input.Key.Left" />. </returns>
	public static RoutedCommand DecreaseSmall => _decreaseSmallCommand;

	/// <summary>Gets a command that sets the <see cref="T:System.Windows.Controls.Slider" /> <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> to the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Minimum" /> value.</summary>
	/// <returns>The <see cref="T:System.Windows.Input.RoutedCommand" /> to use. The default is <see cref="F:System.Windows.Input.Key.Home" />.</returns>
	public static RoutedCommand MinimizeValue => _minimizeValueCommand;

	/// <summary>Gets a command that sets the <see cref="T:System.Windows.Controls.Slider" /> <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> to the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Maximum" /> value.</summary>
	/// <returns>The <see cref="T:System.Windows.Input.RoutedCommand" /> to use. The default is <see cref="F:System.Windows.Input.Key.End" />.</returns>
	public static RoutedCommand MaximizeValue => _maximizeValueCommand;

	/// <summary>Gets or sets the orientation of a <see cref="T:System.Windows.Controls.Slider" />.  </summary>
	/// <returns>One of the <see cref="P:System.Windows.Controls.Slider.Orientation" /> values. The default is <see cref="F:System.Windows.Controls.Orientation.Horizontal" />.</returns>
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

	/// <summary>Gets or sets the direction of increasing value. </summary>
	/// <returns>true if the direction of increasing value is to the left for a horizontal slider or down for a vertical slider; otherwise, false. The default is false.</returns>
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
			SetValue(IsDirectionReversedProperty, value);
		}
	}

	/// <summary>Gets or sets the amount of time in milliseconds that a <see cref="T:System.Windows.Controls.Primitives.RepeatButton" /> waits, while it is pressed, before a command to move the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> executes, such as a <see cref="P:System.Windows.Controls.Slider.DecreaseLarge" /> command. </summary>
	/// <returns>A time delay in milliseconds. The default is the system key press delay. For more information, see <see cref="P:System.Windows.SystemParameters.KeyboardDelay" />.</returns>
	[Bindable(true)]
	[Category("Behavior")]
	public int Delay
	{
		get
		{
			return (int)GetValue(DelayProperty);
		}
		set
		{
			SetValue(DelayProperty, value);
		}
	}

	/// <summary>Gets or sets the amount of time in milliseconds between increase or decrease commands when a user clicks the <see cref="T:System.Windows.Controls.Primitives.RepeatButton" /> of a <see cref="T:System.Windows.Controls.Slider" />. </summary>
	/// <returns>A time in milliseconds between commands that change the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> of a <see cref="T:System.Windows.Controls.Slider" />. The default is the system key repeat rate. For more information, see SystemParametersInfo (SPI_GETKEYBOARDSPEED).</returns>
	[Bindable(true)]
	[Category("Behavior")]
	public int Interval
	{
		get
		{
			return (int)GetValue(IntervalProperty);
		}
		set
		{
			SetValue(IntervalProperty, value);
		}
	}

	/// <summary>Gets or sets whether a tooltip that contains the current value of the <see cref="T:System.Windows.Controls.Slider" /> displays when the <see cref="P:System.Windows.Controls.Primitives.Track.Thumb" /> is pressed. If a tooltip is displayed, this property also specifies the placement of the tooltip. </summary>
	/// <returns>One of the <see cref="T:System.Windows.Controls.Primitives.AutoToolTipPlacement" /> values that determines where to display the tooltip with respect to the <see cref="P:System.Windows.Controls.Primitives.Track.Thumb" /> of the <see cref="T:System.Windows.Controls.Slider" />, or that specifies to not show a tooltip. The default is <see cref="F:System.Windows.Controls.Primitives.AutoToolTipPlacement.None" />, which specifies that a tooltip is not displayed.</returns>
	[Bindable(true)]
	[Category("Behavior")]
	public AutoToolTipPlacement AutoToolTipPlacement
	{
		get
		{
			return (AutoToolTipPlacement)GetValue(AutoToolTipPlacementProperty);
		}
		set
		{
			SetValue(AutoToolTipPlacementProperty, value);
		}
	}

	/// <summary>Gets or sets the number of digits that are displayed to the right side of the decimal point for the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> of the <see cref="T:System.Windows.Controls.Slider" /> in a tooltip. </summary>
	/// <returns>The precision of the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> that displays in the tooltip, specified as the number of digits that appear to the right of the decimal point. The default is zero (0).</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="P:System.Windows.Controls.Slider.AutoToolTipPrecision" /> is set to a value other than a non-negative integer.</exception>
	[Bindable(true)]
	[Category("Appearance")]
	public int AutoToolTipPrecision
	{
		get
		{
			return (int)GetValue(AutoToolTipPrecisionProperty);
		}
		set
		{
			SetValue(AutoToolTipPrecisionProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Controls.Slider" /> automatically moves the <see cref="P:System.Windows.Controls.Primitives.Track.Thumb" /> to the closest tick mark.  </summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.Slider" /> requires the position of the <see cref="P:System.Windows.Controls.Primitives.Track.Thumb" /> to be a tick mark; otherwise, false. The default is false.</returns>
	[Bindable(true)]
	[Category("Behavior")]
	public bool IsSnapToTickEnabled
	{
		get
		{
			return (bool)GetValue(IsSnapToTickEnabledProperty);
		}
		set
		{
			SetValue(IsSnapToTickEnabledProperty, value);
		}
	}

	/// <summary>Gets or sets the position of tick marks with respect to the <see cref="T:System.Windows.Controls.Primitives.Track" /> of the <see cref="T:System.Windows.Controls.Slider" />.  </summary>
	/// <returns>A <see cref="P:System.Windows.Controls.Slider.TickPlacement" /> value that defines how to position the tick marks in a <see cref="T:System.Windows.Controls.Slider" /> with respect to the slider bar. The default is <see cref="F:System.Windows.Controls.Primitives.TickPlacement.None" />.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	public TickPlacement TickPlacement
	{
		get
		{
			return (TickPlacement)GetValue(TickPlacementProperty);
		}
		set
		{
			SetValue(TickPlacementProperty, value);
		}
	}

	/// <summary>Gets or sets the interval between tick marks.  </summary>
	/// <returns>The distance between tick marks. The default is (1.0).</returns>
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

	/// <summary>Gets or sets the positions of the tick marks to display for a <see cref="T:System.Windows.Controls.Slider" />. </summary>
	/// <returns>A set of tick marks to display for a <see cref="T:System.Windows.Controls.Slider" />. The default is null.</returns>
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

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Controls.Slider" /> displays a selection range along the <see cref="T:System.Windows.Controls.Slider" />.  </summary>
	/// <returns>true if a selection range is displayed; otherwise, false. The default is false.</returns>
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
			SetValue(IsSelectionRangeEnabledProperty, value);
		}
	}

	/// <summary>Gets or sets the smallest value of a specified selection for a <see cref="T:System.Windows.Controls.Slider" />.  </summary>
	/// <returns>The largest value of a selected range of values of a <see cref="T:System.Windows.Controls.Slider" />. The default is zero (0.0).</returns>
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

	/// <summary>Gets or sets the largest value of a specified selection for a <see cref="T:System.Windows.Controls.Slider" />.  </summary>
	/// <returns>The largest value of a selected range of values of a <see cref="T:System.Windows.Controls.Slider" />. The default is zero (0.0).</returns>
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

	/// <summary>Gets or sets a value that indicates whether the <see cref="P:System.Windows.Controls.Primitives.Track.Thumb" /> of a <see cref="T:System.Windows.Controls.Slider" /> moves immediately to the location of the mouse click that occurs while the mouse pointer pauses on the <see cref="T:System.Windows.Controls.Slider" /> track.  </summary>
	/// <returns>true if the <see cref="P:System.Windows.Controls.Primitives.Track.Thumb" /> moves immediately to the location of a mouse click; otherwise, false. The default is false.</returns>
	[Bindable(true)]
	[Category("Behavior")]
	public bool IsMoveToPointEnabled
	{
		get
		{
			return (bool)GetValue(IsMoveToPointEnabledProperty);
		}
		set
		{
			SetValue(IsMoveToPointEnabledProperty, value);
		}
	}

	internal Track Track
	{
		get
		{
			return _track;
		}
		set
		{
			_track = value;
		}
	}

	internal FrameworkElement SelectionRangeElement
	{
		get
		{
			return _selectionRangeElement;
		}
		set
		{
			_selectionRangeElement = value;
		}
	}

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Slider" /> class. </summary>
	public Slider()
	{
	}

	static Slider()
	{
		_increaseLargeCommand = null;
		_increaseSmallCommand = null;
		_decreaseLargeCommand = null;
		_decreaseSmallCommand = null;
		_minimizeValueCommand = null;
		_maximizeValueCommand = null;
		OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(Slider), new FrameworkPropertyMetadata(Orientation.Horizontal), ScrollBar.IsValidOrientation);
		IsDirectionReversedProperty = DependencyProperty.Register("IsDirectionReversed", typeof(bool), typeof(Slider), new FrameworkPropertyMetadata(false));
		DelayProperty = RepeatButton.DelayProperty.AddOwner(typeof(Slider), new FrameworkPropertyMetadata(RepeatButton.GetKeyboardDelay()));
		IntervalProperty = RepeatButton.IntervalProperty.AddOwner(typeof(Slider), new FrameworkPropertyMetadata(RepeatButton.GetKeyboardSpeed()));
		AutoToolTipPlacementProperty = DependencyProperty.Register("AutoToolTipPlacement", typeof(AutoToolTipPlacement), typeof(Slider), new FrameworkPropertyMetadata(AutoToolTipPlacement.None), IsValidAutoToolTipPlacement);
		AutoToolTipPrecisionProperty = DependencyProperty.Register("AutoToolTipPrecision", typeof(int), typeof(Slider), new FrameworkPropertyMetadata(0), IsValidAutoToolTipPrecision);
		IsSnapToTickEnabledProperty = DependencyProperty.Register("IsSnapToTickEnabled", typeof(bool), typeof(Slider), new FrameworkPropertyMetadata(false));
		TickPlacementProperty = DependencyProperty.Register("TickPlacement", typeof(TickPlacement), typeof(Slider), new FrameworkPropertyMetadata(TickPlacement.None), IsValidTickPlacement);
		TickFrequencyProperty = DependencyProperty.Register("TickFrequency", typeof(double), typeof(Slider), new FrameworkPropertyMetadata(1.0), IsValidDoubleValue);
		TicksProperty = DependencyProperty.Register("Ticks", typeof(DoubleCollection), typeof(Slider), new FrameworkPropertyMetadata(new FreezableDefaultValueFactory(DoubleCollection.Empty)));
		IsSelectionRangeEnabledProperty = DependencyProperty.Register("IsSelectionRangeEnabled", typeof(bool), typeof(Slider), new FrameworkPropertyMetadata(false));
		SelectionStartProperty = DependencyProperty.Register("SelectionStart", typeof(double), typeof(Slider), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectionStartChanged, CoerceSelectionStart), IsValidDoubleValue);
		SelectionEndProperty = DependencyProperty.Register("SelectionEnd", typeof(double), typeof(Slider), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectionEndChanged, CoerceSelectionEnd), IsValidDoubleValue);
		IsMoveToPointEnabledProperty = DependencyProperty.Register("IsMoveToPointEnabled", typeof(bool), typeof(Slider), new FrameworkPropertyMetadata(false));
		InitializeCommands();
		RangeBase.MinimumProperty.OverrideMetadata(typeof(Slider), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure));
		RangeBase.MaximumProperty.OverrideMetadata(typeof(Slider), new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsMeasure));
		RangeBase.ValueProperty.OverrideMetadata(typeof(Slider), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure));
		EventManager.RegisterClassHandler(typeof(Slider), Thumb.DragStartedEvent, new DragStartedEventHandler(OnThumbDragStarted));
		EventManager.RegisterClassHandler(typeof(Slider), Thumb.DragDeltaEvent, new DragDeltaEventHandler(OnThumbDragDelta));
		EventManager.RegisterClassHandler(typeof(Slider), Thumb.DragCompletedEvent, new DragCompletedEventHandler(OnThumbDragCompleted));
		EventManager.RegisterClassHandler(typeof(Slider), Mouse.MouseDownEvent, new MouseButtonEventHandler(_OnMouseLeftButtonDown), handledEventsToo: true);
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(Slider), new FrameworkPropertyMetadata(typeof(Slider)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(Slider));
		ControlsTraceLogger.AddControl(TelemetryControls.Slider);
	}

	private static void InitializeCommands()
	{
		_increaseLargeCommand = new RoutedCommand("IncreaseLarge", typeof(Slider));
		_decreaseLargeCommand = new RoutedCommand("DecreaseLarge", typeof(Slider));
		_increaseSmallCommand = new RoutedCommand("IncreaseSmall", typeof(Slider));
		_decreaseSmallCommand = new RoutedCommand("DecreaseSmall", typeof(Slider));
		_minimizeValueCommand = new RoutedCommand("MinimizeValue", typeof(Slider));
		_maximizeValueCommand = new RoutedCommand("MaximizeValue", typeof(Slider));
		CommandHelpers.RegisterCommandHandler(typeof(Slider), _increaseLargeCommand, OnIncreaseLargeCommand, new SliderGesture(Key.Prior, Key.Next, forHorizontal: false));
		CommandHelpers.RegisterCommandHandler(typeof(Slider), _decreaseLargeCommand, OnDecreaseLargeCommand, new SliderGesture(Key.Next, Key.Prior, forHorizontal: false));
		CommandHelpers.RegisterCommandHandler(typeof(Slider), _increaseSmallCommand, OnIncreaseSmallCommand, new SliderGesture(Key.Up, Key.Down, forHorizontal: false), new SliderGesture(Key.Right, Key.Left, forHorizontal: true));
		CommandHelpers.RegisterCommandHandler(typeof(Slider), _decreaseSmallCommand, OnDecreaseSmallCommand, new SliderGesture(Key.Down, Key.Up, forHorizontal: false), new SliderGesture(Key.Left, Key.Right, forHorizontal: true));
		CommandHelpers.RegisterCommandHandler(typeof(Slider), _minimizeValueCommand, OnMinimizeValueCommand, Key.Home);
		CommandHelpers.RegisterCommandHandler(typeof(Slider), _maximizeValueCommand, OnMaximizeValueCommand, Key.End);
	}

	private static void OnIncreaseSmallCommand(object sender, ExecutedRoutedEventArgs e)
	{
		if (sender is Slider slider)
		{
			slider.OnIncreaseSmall();
		}
	}

	private static void OnDecreaseSmallCommand(object sender, ExecutedRoutedEventArgs e)
	{
		if (sender is Slider slider)
		{
			slider.OnDecreaseSmall();
		}
	}

	private static void OnMaximizeValueCommand(object sender, ExecutedRoutedEventArgs e)
	{
		if (sender is Slider slider)
		{
			slider.OnMaximizeValue();
		}
	}

	private static void OnMinimizeValueCommand(object sender, ExecutedRoutedEventArgs e)
	{
		if (sender is Slider slider)
		{
			slider.OnMinimizeValue();
		}
	}

	private static void OnIncreaseLargeCommand(object sender, ExecutedRoutedEventArgs e)
	{
		if (sender is Slider slider)
		{
			slider.OnIncreaseLarge();
		}
	}

	private static void OnDecreaseLargeCommand(object sender, ExecutedRoutedEventArgs e)
	{
		if (sender is Slider slider)
		{
			slider.OnDecreaseLarge();
		}
	}

	private static bool IsValidAutoToolTipPlacement(object o)
	{
		AutoToolTipPlacement autoToolTipPlacement = (AutoToolTipPlacement)o;
		if (autoToolTipPlacement != 0 && autoToolTipPlacement != AutoToolTipPlacement.TopLeft)
		{
			return autoToolTipPlacement == AutoToolTipPlacement.BottomRight;
		}
		return true;
	}

	private static bool IsValidAutoToolTipPrecision(object o)
	{
		return (int)o >= 0;
	}

	private static bool IsValidTickPlacement(object o)
	{
		TickPlacement tickPlacement = (TickPlacement)o;
		if (tickPlacement != 0 && tickPlacement != TickPlacement.TopLeft && tickPlacement != TickPlacement.BottomRight)
		{
			return tickPlacement == TickPlacement.Both;
		}
		return true;
	}

	private static void OnSelectionStartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Slider obj = (Slider)d;
		_ = (double)e.OldValue;
		_ = (double)e.NewValue;
		obj.CoerceValue(SelectionEndProperty);
		obj.UpdateSelectionRangeElementPositionAndSize();
	}

	private static object CoerceSelectionStart(DependencyObject d, object value)
	{
		Slider obj = (Slider)d;
		double num = (double)value;
		double minimum = obj.Minimum;
		double maximum = obj.Maximum;
		if (num < minimum)
		{
			return minimum;
		}
		if (num > maximum)
		{
			return maximum;
		}
		return value;
	}

	private static void OnSelectionEndChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Slider)d).UpdateSelectionRangeElementPositionAndSize();
	}

	private static object CoerceSelectionEnd(DependencyObject d, object value)
	{
		Slider obj = (Slider)d;
		double num = (double)value;
		double selectionStart = obj.SelectionStart;
		double maximum = obj.Maximum;
		if (num < selectionStart)
		{
			return selectionStart;
		}
		if (num > maximum)
		{
			return maximum;
		}
		return value;
	}

	private static object OnGetSelectionEnd(DependencyObject d)
	{
		return ((Slider)d).SelectionEnd;
	}

	/// <summary>Responds to a change in the value of the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Minimum" /> property.</summary>
	/// <param name="oldMinimum">The old value of the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Minimum" /> property.</param>
	/// <param name="newMinimum">The new value of the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Minimum" /> property.</param>
	protected override void OnMinimumChanged(double oldMinimum, double newMinimum)
	{
		CoerceValue(SelectionStartProperty);
	}

	/// <summary>Responds to a change in the value of the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Maximum" /> property.</summary>
	/// <param name="oldMaximum">The old value of the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Maximum" /> property.</param>
	/// <param name="newMaximum">The new value of the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Maximum" /> property.</param>
	protected override void OnMaximumChanged(double oldMaximum, double newMaximum)
	{
		CoerceValue(SelectionStartProperty);
		CoerceValue(SelectionEndProperty);
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.ContentElement.PreviewMouseLeftButtonDown" /> routed event.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		if (IsMoveToPointEnabled && Track != null && Track.Thumb != null && !Track.Thumb.IsMouseOver)
		{
			Point position = e.MouseDevice.GetPosition(Track);
			double num = Track.ValueFromPoint(position);
			if (Shape.IsDoubleFinite(num))
			{
				UpdateValue(num);
			}
			e.Handled = true;
		}
		base.OnPreviewMouseLeftButtonDown(e);
	}

	private static void OnThumbDragStarted(object sender, DragStartedEventArgs e)
	{
		(sender as Slider).OnThumbDragStarted(e);
	}

	private static void OnThumbDragDelta(object sender, DragDeltaEventArgs e)
	{
		(sender as Slider).OnThumbDragDelta(e);
	}

	private static void OnThumbDragCompleted(object sender, DragCompletedEventArgs e)
	{
		(sender as Slider).OnThumbDragCompleted(e);
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.Controls.Primitives.Thumb.DragStarted" /> event that occurs when the user starts to drag the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> of the <see cref="T:System.Windows.Controls.Slider" />.</summary>
	/// <param name="e">The event data.</param>
	protected virtual void OnThumbDragStarted(DragStartedEventArgs e)
	{
		if (e.OriginalSource is Thumb thumb && AutoToolTipPlacement != 0)
		{
			_thumbOriginalToolTip = thumb.ToolTip;
			if (_autoToolTip == null)
			{
				_autoToolTip = new ToolTip();
				_autoToolTip.Placement = PlacementMode.Custom;
				_autoToolTip.PlacementTarget = thumb;
				_autoToolTip.CustomPopupPlacementCallback = AutoToolTipCustomPlacementCallback;
			}
			thumb.ToolTip = _autoToolTip;
			_autoToolTip.Content = GetAutoToolTipNumber();
			_autoToolTip.IsOpen = true;
			((Popup)_autoToolTip.Parent).Reposition();
		}
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.Controls.Primitives.Thumb.DragDelta" /> event that occurs when the user drags the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> of the <see cref="T:System.Windows.Controls.Slider" />.</summary>
	/// <param name="e">The event data.</param>
	protected virtual void OnThumbDragDelta(DragDeltaEventArgs e)
	{
		Thumb thumb = e.OriginalSource as Thumb;
		if (Track == null || thumb != Track.Thumb)
		{
			return;
		}
		double num = base.Value + Track.ValueFromDistance(e.HorizontalChange, e.VerticalChange);
		if (Shape.IsDoubleFinite(num))
		{
			UpdateValue(num);
		}
		if (AutoToolTipPlacement != 0)
		{
			if (_autoToolTip == null)
			{
				_autoToolTip = new ToolTip();
			}
			_autoToolTip.Content = GetAutoToolTipNumber();
			if (thumb.ToolTip != _autoToolTip)
			{
				thumb.ToolTip = _autoToolTip;
			}
			if (!_autoToolTip.IsOpen)
			{
				_autoToolTip.IsOpen = true;
			}
			((Popup)_autoToolTip.Parent).Reposition();
		}
	}

	private string GetAutoToolTipNumber()
	{
		NumberFormatInfo numberFormatInfo = (NumberFormatInfo)NumberFormatInfo.CurrentInfo.Clone();
		numberFormatInfo.NumberDecimalDigits = AutoToolTipPrecision;
		return base.Value.ToString("N", numberFormatInfo);
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.Controls.Primitives.Thumb.DragCompleted" /> event that occurs when the user stops dragging the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> of the <see cref="T:System.Windows.Controls.Slider" />.</summary>
	/// <param name="e">The event data.</param>
	protected virtual void OnThumbDragCompleted(DragCompletedEventArgs e)
	{
		if (e.OriginalSource is Thumb thumb && AutoToolTipPlacement != 0)
		{
			if (_autoToolTip != null)
			{
				_autoToolTip.IsOpen = false;
			}
			thumb.ToolTip = _thumbOriginalToolTip;
		}
	}

	private CustomPopupPlacement[] AutoToolTipCustomPlacementCallback(Size popupSize, Size targetSize, Point offset)
	{
		switch (AutoToolTipPlacement)
		{
		case AutoToolTipPlacement.TopLeft:
			if (Orientation != 0)
			{
				return new CustomPopupPlacement[1]
				{
					new CustomPopupPlacement(new Point(0.0 - popupSize.Width, (targetSize.Height - popupSize.Height) * 0.5), PopupPrimaryAxis.Vertical)
				};
			}
			return new CustomPopupPlacement[1]
			{
				new CustomPopupPlacement(new Point((targetSize.Width - popupSize.Width) * 0.5, 0.0 - popupSize.Height), PopupPrimaryAxis.Horizontal)
			};
		case AutoToolTipPlacement.BottomRight:
			if (Orientation != 0)
			{
				return new CustomPopupPlacement[1]
				{
					new CustomPopupPlacement(new Point(targetSize.Width, (targetSize.Height - popupSize.Height) * 0.5), PopupPrimaryAxis.Vertical)
				};
			}
			return new CustomPopupPlacement[1]
			{
				new CustomPopupPlacement(new Point((targetSize.Width - popupSize.Width) * 0.5, targetSize.Height), PopupPrimaryAxis.Horizontal)
			};
		default:
			return new CustomPopupPlacement[0];
		}
	}

	private void UpdateSelectionRangeElementPositionAndSize()
	{
		Size size = new Size(0.0, 0.0);
		Size size2 = new Size(0.0, 0.0);
		if (Track == null || DoubleUtil.LessThan(SelectionEnd, SelectionStart))
		{
			return;
		}
		size = Track.RenderSize;
		size2 = ((Track.Thumb != null) ? Track.Thumb.RenderSize : new Size(0.0, 0.0));
		double num = base.Maximum - base.Minimum;
		FrameworkElement selectionRangeElement = SelectionRangeElement;
		if (selectionRangeElement == null)
		{
			return;
		}
		if (Orientation == Orientation.Horizontal)
		{
			double num2 = ((!DoubleUtil.AreClose(num, 0.0) && !DoubleUtil.AreClose(size.Width, size2.Width)) ? Math.Max(0.0, (size.Width - size2.Width) / num) : 0.0);
			selectionRangeElement.Width = (SelectionEnd - SelectionStart) * num2;
			if (IsDirectionReversed)
			{
				Canvas.SetLeft(selectionRangeElement, size2.Width * 0.5 + Math.Max(base.Maximum - SelectionEnd, 0.0) * num2);
			}
			else
			{
				Canvas.SetLeft(selectionRangeElement, size2.Width * 0.5 + Math.Max(SelectionStart - base.Minimum, 0.0) * num2);
			}
		}
		else
		{
			double num2 = ((!DoubleUtil.AreClose(num, 0.0) && !DoubleUtil.AreClose(size.Height, size2.Height)) ? Math.Max(0.0, (size.Height - size2.Height) / num) : 0.0);
			selectionRangeElement.Height = (SelectionEnd - SelectionStart) * num2;
			if (IsDirectionReversed)
			{
				Canvas.SetTop(selectionRangeElement, size2.Height * 0.5 + Math.Max(SelectionStart - base.Minimum, 0.0) * num2);
			}
			else
			{
				Canvas.SetTop(selectionRangeElement, size2.Height * 0.5 + Math.Max(base.Maximum - SelectionEnd, 0.0) * num2);
			}
		}
	}

	private double SnapToTick(double value)
	{
		if (IsSnapToTickEnabled)
		{
			double num = base.Minimum;
			double num2 = base.Maximum;
			DoubleCollection doubleCollection = null;
			if (GetValueSource(TicksProperty, null, out var hasModifiers) != BaseValueSourceInternal.Default || hasModifiers)
			{
				doubleCollection = Ticks;
			}
			if (doubleCollection != null && doubleCollection.Count > 0)
			{
				for (int i = 0; i < doubleCollection.Count; i++)
				{
					double num3 = doubleCollection[i];
					if (DoubleUtil.AreClose(num3, value))
					{
						return value;
					}
					if (DoubleUtil.LessThan(num3, value) && DoubleUtil.GreaterThan(num3, num))
					{
						num = num3;
					}
					else if (DoubleUtil.GreaterThan(num3, value) && DoubleUtil.LessThan(num3, num2))
					{
						num2 = num3;
					}
				}
			}
			else if (DoubleUtil.GreaterThanZero(TickFrequency))
			{
				num = base.Minimum + Math.Round((value - base.Minimum) / TickFrequency) * TickFrequency;
				num2 = Math.Min(base.Maximum, num + TickFrequency);
			}
			value = (DoubleUtil.GreaterThanOrClose(value, (num + num2) * 0.5) ? num2 : num);
		}
		return value;
	}

	private void MoveToNextTick(double direction)
	{
		if (direction == 0.0)
		{
			return;
		}
		double value = base.Value;
		double num = SnapToTick(Math.Max(base.Minimum, Math.Min(base.Maximum, value + direction)));
		bool flag = direction > 0.0;
		if (num == value && (!flag || value != base.Maximum) && (flag || value != base.Minimum))
		{
			DoubleCollection doubleCollection = null;
			if (GetValueSource(TicksProperty, null, out var hasModifiers) != BaseValueSourceInternal.Default || hasModifiers)
			{
				doubleCollection = Ticks;
			}
			if (doubleCollection != null && doubleCollection.Count > 0)
			{
				for (int i = 0; i < doubleCollection.Count; i++)
				{
					double num2 = doubleCollection[i];
					if ((flag && DoubleUtil.GreaterThan(num2, value) && (DoubleUtil.LessThan(num2, num) || num == value)) || (!flag && DoubleUtil.LessThan(num2, value) && (DoubleUtil.GreaterThan(num2, num) || num == value)))
					{
						num = num2;
					}
				}
			}
			else if (DoubleUtil.GreaterThanZero(TickFrequency))
			{
				double num3 = Math.Round((value - base.Minimum) / TickFrequency);
				num3 = ((!flag) ? (num3 - 1.0) : (num3 + 1.0));
				num = base.Minimum + num3 * TickFrequency;
			}
		}
		if (num != value)
		{
			SetCurrentValueInternal(RangeBase.ValueProperty, num);
		}
	}

	/// <summary>Creates an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for the <see cref="T:System.Windows.Controls.Slider" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Automation.Peers.SliderAutomationPeer" /> for the <see cref="T:System.Windows.Controls.Slider" />.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new SliderAutomationPeer(this);
	}

	private static void _OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left)
		{
			Slider slider = (Slider)sender;
			if (!slider.IsKeyboardFocusWithin)
			{
				e.Handled = slider.Focus() || e.Handled;
			}
		}
	}

	/// <summary>Arranges the content of a <see cref="T:System.Windows.Controls.Slider" /> and determines its <see cref="T:System.Windows.Size" />.</summary>
	/// <returns>The computed <see cref="T:System.Windows.Size" /> of the <see cref="T:System.Windows.Controls.Slider" />.</returns>
	/// <param name="finalSize">The computed <see cref="T:System.Windows.Size" /> that is used to arrange the control.</param>
	protected override Size ArrangeOverride(Size finalSize)
	{
		Size result = base.ArrangeOverride(finalSize);
		UpdateSelectionRangeElementPositionAndSize();
		return result;
	}

	/// <summary>Updates the current position of the <see cref="T:System.Windows.Controls.Slider" /> when the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> property changes.</summary>
	/// <param name="oldValue">The old <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> of the <see cref="T:System.Windows.Controls.Slider" />.</param>
	/// <param name="newValue">The new <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> of the <see cref="T:System.Windows.Controls.Slider" />.</param>
	protected override void OnValueChanged(double oldValue, double newValue)
	{
		base.OnValueChanged(oldValue, newValue);
		UpdateSelectionRangeElementPositionAndSize();
	}

	/// <summary>Builds the visual tree for the <see cref="T:System.Windows.Controls.Slider" /> control.</summary>
	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		SelectionRangeElement = GetTemplateChild("PART_SelectionRange") as FrameworkElement;
		Track = GetTemplateChild("PART_Track") as Track;
		if (_autoToolTip != null)
		{
			_autoToolTip.PlacementTarget = ((Track != null) ? Track.Thumb : null);
		}
	}

	/// <summary>Responds to an <see cref="P:System.Windows.Controls.Slider.IncreaseLarge" /> command.</summary>
	protected virtual void OnIncreaseLarge()
	{
		MoveToNextTick(base.LargeChange);
	}

	/// <summary>Responds to the <see cref="P:System.Windows.Controls.Slider.DecreaseLarge" /> command.</summary>
	protected virtual void OnDecreaseLarge()
	{
		MoveToNextTick(0.0 - base.LargeChange);
	}

	/// <summary>Responds to an <see cref="P:System.Windows.Controls.Slider.IncreaseSmall" /> command.</summary>
	protected virtual void OnIncreaseSmall()
	{
		MoveToNextTick(base.SmallChange);
	}

	/// <summary>Responds to a <see cref="P:System.Windows.Controls.Slider.DecreaseSmall" /> command.</summary>
	protected virtual void OnDecreaseSmall()
	{
		MoveToNextTick(0.0 - base.SmallChange);
	}

	/// <summary>Responds to the <see cref="P:System.Windows.Controls.Slider.MaximizeValue" /> command.</summary>
	protected virtual void OnMaximizeValue()
	{
		SetCurrentValueInternal(RangeBase.ValueProperty, base.Maximum);
	}

	/// <summary>Responds to a <see cref="P:System.Windows.Controls.Slider.MinimizeValue" /> command.</summary>
	protected virtual void OnMinimizeValue()
	{
		SetCurrentValueInternal(RangeBase.ValueProperty, base.Minimum);
	}

	private void UpdateValue(double value)
	{
		double num = SnapToTick(value);
		if (num != base.Value)
		{
			SetCurrentValueInternal(RangeBase.ValueProperty, Math.Max(base.Minimum, Math.Min(base.Maximum, num)));
		}
	}

	private static bool IsValidDoubleValue(object value)
	{
		double d = (double)value;
		if (!double.IsNaN(d))
		{
			return !double.IsInfinity(d);
		}
		return false;
	}
}
