using System.ComponentModel;
using System.Windows.Markup;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Animation;

/// <summary>Defines a segment of time. </summary>
[RuntimeNameProperty("Name")]
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public abstract class Timeline : Animatable
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.Timeline.AccelerationRatio" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.Timeline.AccelerationRatio" /> dependency property.</returns>
	public static readonly DependencyProperty AccelerationRatioProperty = DependencyProperty.Register("AccelerationRatio", typeof(double), typeof(Timeline), new PropertyMetadata(0.0, Timeline_PropertyChangedFunction), ValidateAccelerationDecelerationRatio);

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.Timeline.AutoReverse" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.Timeline.AutoReverse" /> dependency property.</returns>
	public static readonly DependencyProperty AutoReverseProperty = DependencyProperty.Register("AutoReverse", typeof(bool), typeof(Timeline), new PropertyMetadata(false, Timeline_PropertyChangedFunction));

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.Timeline.BeginTime" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.Timeline.BeginTime" /> dependency property.</returns>
	public static readonly DependencyProperty BeginTimeProperty = DependencyProperty.Register("BeginTime", typeof(TimeSpan?), typeof(Timeline), new PropertyMetadata(TimeSpan.Zero, Timeline_PropertyChangedFunction));

	/// <summary>Identifies for the <see cref="P:System.Windows.Media.Animation.Timeline.DecelerationRatio" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.Timeline.DecelerationRatio" /> dependency property.</returns>
	public static readonly DependencyProperty DecelerationRatioProperty = DependencyProperty.Register("DecelerationRatio", typeof(double), typeof(Timeline), new PropertyMetadata(0.0, Timeline_PropertyChangedFunction), ValidateAccelerationDecelerationRatio);

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.Timeline.DesiredFrameRate" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.Timeline.DesiredFrameRate" /> attached property.</returns>
	public static readonly DependencyProperty DesiredFrameRateProperty = DependencyProperty.RegisterAttached("DesiredFrameRate", typeof(int?), typeof(Timeline), new PropertyMetadata(null, Timeline_PropertyChangedFunction), ValidateDesiredFrameRate);

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> dependency property.</returns>
	public static readonly DependencyProperty DurationProperty = DependencyProperty.Register("Duration", typeof(Duration), typeof(Timeline), new PropertyMetadata(Duration.Automatic, Timeline_PropertyChangedFunction));

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.Timeline.FillBehavior" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.Timeline.FillBehavior" /> dependency property.</returns>
	public static readonly DependencyProperty FillBehaviorProperty = DependencyProperty.Register("FillBehavior", typeof(FillBehavior), typeof(Timeline), new PropertyMetadata(FillBehavior.HoldEnd, Timeline_PropertyChangedFunction), ValidateFillBehavior);

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.Timeline.Name" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.Timeline.Name" /> dependency property.</returns>
	public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(Timeline), new PropertyMetadata(null, Timeline_PropertyChangedFunction), NameValidationHelper.NameValidationCallback);

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.Timeline.RepeatBehavior" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.Timeline.RepeatBehavior" /> dependency property.</returns>
	public static readonly DependencyProperty RepeatBehaviorProperty = DependencyProperty.Register("RepeatBehavior", typeof(RepeatBehavior), typeof(Timeline), new PropertyMetadata(new RepeatBehavior(1.0), Timeline_PropertyChangedFunction));

	/// <summary>Identifies for the <see cref="P:System.Windows.Media.Animation.Timeline.SpeedRatio" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.Timeline.SpeedRatio" /> dependency property.</returns>
	public static readonly DependencyProperty SpeedRatioProperty = DependencyProperty.Register("SpeedRatio", typeof(double), typeof(Timeline), new PropertyMetadata(1.0, Timeline_PropertyChangedFunction), ValidateSpeedRatio);

	internal static readonly UncommonField<EventHandlersStore> EventHandlersStoreField = new UncommonField<EventHandlersStore>();

	internal static readonly EventPrivateKey CurrentGlobalSpeedInvalidatedKey = new EventPrivateKey();

	internal static readonly EventPrivateKey CurrentStateInvalidatedKey = new EventPrivateKey();

	internal static readonly EventPrivateKey CurrentTimeInvalidatedKey = new EventPrivateKey();

	internal static readonly EventPrivateKey CompletedKey = new EventPrivateKey();

	internal static readonly EventPrivateKey RemoveRequestedKey = new EventPrivateKey();

	/// <summary>Gets or sets a value specifying the percentage of the timeline's <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> spent accelerating the passage of time from zero to its maximum rate.  </summary>
	/// <returns>A value between 0 and 1, inclusive, that specifies the percentage of the timeline's <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> spent accelerating the passage of time from zero to its maximum rate. If the timeline's <see cref="P:System.Windows.Media.Animation.Timeline.DecelerationRatio" />  property is also set, the sum of <see cref="P:System.Windows.Media.Animation.Timeline.AccelerationRatio" /> and <see cref="P:System.Windows.Media.Animation.Timeline.DecelerationRatio" /> must be less than or equal to 1. The default value is 0.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="P:System.Windows.Media.Animation.Timeline.AccelerationRatio" /> is less than 0 or greater than 1.</exception>
	/// <exception cref="T:System.InvalidOperationException">The sum of <see cref="P:System.Windows.Media.Animation.Timeline.AccelerationRatio" /> and <see cref="P:System.Windows.Media.Animation.Timeline.DecelerationRatio" /> exceeds 1.</exception>
	public double AccelerationRatio
	{
		get
		{
			return (double)GetValue(AccelerationRatioProperty);
		}
		set
		{
			SetValue(AccelerationRatioProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the timeline plays in reverse after it completes a forward iteration.  </summary>
	/// <returns>true if the timeline plays in reverse at the end of each iteration; otherwise, false. The default value is false.</returns>
	[DefaultValue(false)]
	public bool AutoReverse
	{
		get
		{
			return (bool)GetValue(AutoReverseProperty);
		}
		set
		{
			SetValue(AutoReverseProperty, value);
		}
	}

	/// <summary>Gets or sets the time at which this <see cref="T:System.Windows.Media.Animation.Timeline" /> should begin.  </summary>
	/// <returns>The time at which this <see cref="T:System.Windows.Media.Animation.Timeline" /> should begin, relative to its parent's <see cref="P:System.Windows.Media.Animation.Timeline.BeginTime" />. If this timeline is a root timeline, the time is relative to its interactive begin time (the moment at which the timeline was triggered). This value may be positive, negative, or null; a null value means the timeline never plays. The default value is zero.</returns>
	public TimeSpan? BeginTime
	{
		get
		{
			return (TimeSpan?)GetValue(BeginTimeProperty);
		}
		set
		{
			SetValue(BeginTimeProperty, value);
		}
	}

	/// <summary>Gets or sets a value specifying the percentage of the timeline's <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> spent decelerating the passage of time from its maximum rate to zero.  </summary>
	/// <returns>A value between 0 and 1, inclusive, that specifies the percentage of the timeline's <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> spent decelerating the passage of time from its maximum rate to zero. If the timeline's <see cref="P:System.Windows.Media.Animation.Timeline.AccelerationRatio" /> property is also set, the sum of <see cref="P:System.Windows.Media.Animation.Timeline.DecelerationRatio" /> and <see cref="P:System.Windows.Media.Animation.Timeline.AccelerationRatio" /> must be less than or equal to 1. The default value is 0.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="P:System.Windows.Media.Animation.Timeline.DecelerationRatio" /> is less than 0 or greater than 1.</exception>
	/// <exception cref="T:System.InvalidOperationException">The sum of <see cref="P:System.Windows.Media.Animation.Timeline.AccelerationRatio" /> and <see cref="P:System.Windows.Media.Animation.Timeline.DecelerationRatio" /> exceeds 1.</exception>
	public double DecelerationRatio
	{
		get
		{
			return (double)GetValue(DecelerationRatioProperty);
		}
		set
		{
			SetValue(DecelerationRatioProperty, value);
		}
	}

	/// <summary>Gets or sets the length of time for which this timeline plays, not counting repetitions.  </summary>
	/// <returns>The timeline's simple duration: the amount of time this timeline takes to complete a single forward iteration. The default value is <see cref="P:System.Windows.Duration.Automatic" />.</returns>
	public Duration Duration
	{
		get
		{
			return (Duration)GetValue(DurationProperty);
		}
		set
		{
			SetValue(DurationProperty, value);
		}
	}

	/// <summary>Gets or sets a value that specifies how the <see cref="T:System.Windows.Media.Animation.Timeline" /> behaves after it reaches the end of its active period.  </summary>
	/// <returns>A value that specifies how the timeline behaves after it reaches the end of its active period but its parent is inside its active or fill period. The default value is <see cref="F:System.Windows.Media.Animation.FillBehavior.HoldEnd" />.</returns>
	public FillBehavior FillBehavior
	{
		get
		{
			return (FillBehavior)GetValue(FillBehaviorProperty);
		}
		set
		{
			SetValue(FillBehaviorProperty, value);
		}
	}

	/// <summary> Gets or sets the name of this <see cref="T:System.Windows.Media.Animation.Timeline" />. </summary>
	/// <returns>The name of this timeline. The default value is null.</returns>
	[DefaultValue(null)]
	[MergableProperty(false)]
	public string Name
	{
		get
		{
			return (string)GetValue(NameProperty);
		}
		set
		{
			SetValue(NameProperty, value);
		}
	}

	/// <summary>Gets or sets the repeating behavior of this timeline.  </summary>
	/// <returns>An iteration <see cref="P:System.Windows.Media.Animation.RepeatBehavior.Count" /> that specifies the number of times the timeline should play, a <see cref="T:System.TimeSpan" /> value that specifies the total the length of this timeline's active period, or the special value <see cref="P:System.Windows.Media.Animation.RepeatBehavior.Forever" />, which specifies that the timeline should repeat indefinitely. The default value is a <see cref="T:System.Windows.Media.Animation.RepeatBehavior" /> with a <see cref="P:System.Windows.Media.Animation.RepeatBehavior.Count" /> of 1, which indicates that the timeline plays once.</returns>
	public RepeatBehavior RepeatBehavior
	{
		get
		{
			return (RepeatBehavior)GetValue(RepeatBehaviorProperty);
		}
		set
		{
			SetValue(RepeatBehaviorProperty, value);
		}
	}

	/// <summary>Gets or sets the rate, relative to its parent, at which time progresses for this <see cref="T:System.Windows.Media.Animation.Timeline" />.  </summary>
	/// <returns>A finite value greater than 0 that describes the rate at which time progresses for this timeline, relative to the speed of the timeline's parent or, if this is a root timeline, the default timeline speed. The default value is 1.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="P:System.Windows.Media.Animation.Timeline.SpeedRatio" /> is less than 0 or is not a finite value.</exception>
	[DefaultValue(1.0)]
	public double SpeedRatio
	{
		get
		{
			return (double)GetValue(SpeedRatioProperty);
		}
		set
		{
			SetValue(SpeedRatioProperty, value);
		}
	}

	internal EventHandlersStore InternalEventHandlersStore => EventHandlersStoreField.GetValue(this);

	/// <summary>Occurs when the <see cref="P:System.Windows.Media.Animation.Clock.CurrentState" /> property of the timeline's <see cref="T:System.Windows.Media.Animation.Clock" /> is updated.</summary>
	public event EventHandler CurrentStateInvalidated
	{
		add
		{
			AddEventHandler(CurrentStateInvalidatedKey, value);
		}
		remove
		{
			RemoveEventHandler(CurrentStateInvalidatedKey, value);
		}
	}

	/// <summary>Occurs when the <see cref="P:System.Windows.Media.Animation.Clock.CurrentTime" /> property of the timeline's <see cref="T:System.Windows.Media.Animation.Clock" /> is updated.</summary>
	public event EventHandler CurrentTimeInvalidated
	{
		add
		{
			AddEventHandler(CurrentTimeInvalidatedKey, value);
		}
		remove
		{
			RemoveEventHandler(CurrentTimeInvalidatedKey, value);
		}
	}

	/// <summary>Occurs when the rate at which time progresses for the timeline's clock changes.</summary>
	public event EventHandler CurrentGlobalSpeedInvalidated
	{
		add
		{
			AddEventHandler(CurrentGlobalSpeedInvalidatedKey, value);
		}
		remove
		{
			RemoveEventHandler(CurrentGlobalSpeedInvalidatedKey, value);
		}
	}

	/// <summary>Occurs when this timeline has completely finished playing: it will no longer enter its active period. </summary>
	public event EventHandler Completed
	{
		add
		{
			AddEventHandler(CompletedKey, value);
		}
		remove
		{
			RemoveEventHandler(CompletedKey, value);
		}
	}

	/// <summary>Occurs when the clock created for this timeline or one of its parent timelines is removed.</summary>
	public event EventHandler RemoveRequested
	{
		add
		{
			AddEventHandler(RemoveRequestedKey, value);
		}
		remove
		{
			RemoveEventHandler(RemoveRequestedKey, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.Timeline" /> class.</summary>
	protected Timeline()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.Timeline" /> class with the specified <see cref="P:System.Windows.Media.Animation.Timeline.BeginTime" />.</summary>
	/// <param name="beginTime">The time at which this <see cref="T:System.Windows.Media.Animation.Timeline" /> should begin. See the <see cref="P:System.Windows.Media.Animation.Timeline.BeginTime" /> property for more information.</param>
	protected Timeline(TimeSpan? beginTime)
		: this()
	{
		BeginTime = beginTime;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.Timeline" /> class with the specified <see cref="P:System.Windows.Media.Animation.Timeline.BeginTime" /> and <see cref="P:System.Windows.Media.Animation.Timeline.Duration" />.</summary>
	/// <param name="beginTime">The time at which this <see cref="T:System.Windows.Media.Animation.Timeline" /> should begin. See the <see cref="P:System.Windows.Media.Animation.Timeline.BeginTime" /> property for more information.</param>
	/// <param name="duration">The length of time for which this timeline plays, not counting repetitions. See the <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> property for more information.</param>
	protected Timeline(TimeSpan? beginTime, Duration duration)
		: this()
	{
		BeginTime = beginTime;
		Duration = duration;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.Timeline" /> class with the specified <see cref="P:System.Windows.Media.Animation.Timeline.BeginTime" />, <see cref="P:System.Windows.Media.Animation.Timeline.Duration" />, and <see cref="P:System.Windows.Media.Animation.Timeline.RepeatBehavior" />.</summary>
	/// <param name="beginTime">The time at which this <see cref="T:System.Windows.Media.Animation.Timeline" /> should begin. See the <see cref="P:System.Windows.Media.Animation.Timeline.BeginTime" /> property for more information.</param>
	/// <param name="duration">The length of time for which this timeline plays, not counting repetitions. See the <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> property for more information.</param>
	/// <param name="repeatBehavior">The repeating behavior of this timeline, either as an iteration <see cref="P:System.Windows.Media.Animation.RepeatBehavior.Count" /> or a repeat <see cref="P:System.Windows.Media.Animation.RepeatBehavior.Duration" />. See the <see cref="P:System.Windows.Media.Animation.Timeline.RepeatBehavior" /> property for more information.</param>
	protected Timeline(TimeSpan? beginTime, Duration duration, RepeatBehavior repeatBehavior)
		: this()
	{
		BeginTime = beginTime;
		Duration = duration;
		RepeatBehavior = repeatBehavior;
	}

	/// <summary>Makes this <see cref="T:System.Windows.Media.Animation.Timeline" /> unmodifiable or determines whether it can be made unmodifiable.</summary>
	/// <returns>If <paramref name="isChecking" /> is true, this method returns true if this instance can be made read-only, or false if it cannot be made read-only. If <paramref name="isChecking" /> is false, this method returns true if this instance is now read-only, or false if it cannot be made read-only, with the side effect of having begun to change the frozen status of this object.</returns>
	/// <param name="isChecking">true to check if this instance can be frozen; false to freeze this instance. </param>
	protected override bool FreezeCore(bool isChecking)
	{
		ValidateTimeline();
		return base.FreezeCore(isChecking);
	}

	/// <summary>Makes this instance a clone of the specified <see cref="T:System.Windows.Media.Animation.Timeline" /> object. </summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.Animation.Timeline" /> instance to clone.</param>
	protected override void GetAsFrozenCore(Freezable sourceFreezable)
	{
		Timeline sourceTimeline = (Timeline)sourceFreezable;
		base.GetAsFrozenCore(sourceFreezable);
		CopyCommon(sourceTimeline);
	}

	/// <summary>Makes this instance a frozen clone of the specified <see cref="T:System.Windows.Media.Animation.Timeline" />. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.Animation.Timeline" /> to copy and freeze.</param>
	protected override void GetCurrentValueAsFrozenCore(Freezable sourceFreezable)
	{
		Timeline sourceTimeline = (Timeline)sourceFreezable;
		base.GetCurrentValueAsFrozenCore(sourceFreezable);
		CopyCommon(sourceTimeline);
	}

	private static void Timeline_PropertyChangedFunction(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Timeline)d).PropertyChanged(e.Property);
	}

	private static bool ValidateAccelerationDecelerationRatio(object value)
	{
		double num = (double)value;
		if (num < 0.0 || num > 1.0 || double.IsNaN(num))
		{
			throw new ArgumentException(SR.Timing_InvalidArgAccelAndDecel, "value");
		}
		return true;
	}

	private static bool ValidateDesiredFrameRate(object value)
	{
		int? num = (int?)value;
		if (num.HasValue)
		{
			return num.Value > 0;
		}
		return true;
	}

	/// <summary>Gets the desired frame rate of the specified <see cref="T:System.Windows.Media.Animation.Timeline" />. </summary>
	/// <returns>The desired frame rate of this timeline. The default value is null.</returns>
	/// <param name="timeline">The timeline from which to retrieve the desired frame rate.</param>
	public static int? GetDesiredFrameRate(Timeline timeline)
	{
		if (timeline == null)
		{
			throw new ArgumentNullException("timeline");
		}
		return (int?)timeline.GetValue(DesiredFrameRateProperty);
	}

	/// <summary>Sets the desired frame rate of the specified <see cref="T:System.Windows.Media.Animation.Timeline" />.</summary>
	/// <param name="timeline">The <see cref="T:System.Windows.Media.Animation.Timeline" /> to which <paramref name="desiredFrameRate" /> is assigned. </param>
	/// <param name="desiredFrameRate">The maximum number of frames this timeline should generate each second, or null if the system should control the number of frames.</param>
	public static void SetDesiredFrameRate(Timeline timeline, int? desiredFrameRate)
	{
		if (timeline == null)
		{
			throw new ArgumentNullException("timeline");
		}
		timeline.SetValue(DesiredFrameRateProperty, desiredFrameRate);
	}

	private static bool ValidateFillBehavior(object value)
	{
		return TimeEnumHelper.IsValidFillBehavior((FillBehavior)value);
	}

	private static bool ValidateSpeedRatio(object value)
	{
		double num = (double)value;
		if (num <= 0.0 || num > double.MaxValue || double.IsNaN(num))
		{
			throw new ArgumentException(SR.Timing_InvalidArgFinitePositive, "value");
		}
		return true;
	}

	/// <summary>Creates a <see cref="T:System.Windows.Media.Animation.Clock" /> for this <see cref="T:System.Windows.Media.Animation.Timeline" />. </summary>
	/// <returns>A clock for this <see cref="T:System.Windows.Media.Animation.Timeline" />.</returns>
	protected internal virtual Clock AllocateClock()
	{
		return new Clock(this);
	}

	/// <summary>Creates a new, controllable <see cref="T:System.Windows.Media.Animation.Clock" /> from this <see cref="T:System.Windows.Media.Animation.Timeline" />. If this <see cref="T:System.Windows.Media.Animation.Timeline" /> has children, a tree of clocks is created with this <see cref="T:System.Windows.Media.Animation.Timeline" /> as the root. </summary>
	/// <returns>A new, controllable <see cref="T:System.Windows.Media.Animation.Clock" /> constructed from this <see cref="T:System.Windows.Media.Animation.Timeline" />. If this <see cref="T:System.Windows.Media.Animation.Timeline" /> is a <see cref="T:System.Windows.Media.Animation.TimelineGroup" /> that contains child timelines, a tree of <see cref="T:System.Windows.Media.Animation.Clock" /> objects is created with a controllable <see cref="T:System.Windows.Media.Animation.Clock" /> created from this <see cref="T:System.Windows.Media.Animation.Timeline" /> as the root.</returns>
	public Clock CreateClock()
	{
		return CreateClock(hasControllableRoot: true);
	}

	/// <summary>Creates a new <see cref="T:System.Windows.Media.Animation.Clock" /> from this <see cref="T:System.Windows.Media.Animation.Timeline" /> and specifies whether the new <see cref="T:System.Windows.Media.Animation.Clock" /> is controllable. If this <see cref="T:System.Windows.Media.Animation.Timeline" /> has children, a tree of clocks is created with this <see cref="T:System.Windows.Media.Animation.Timeline" /> as the root. </summary>
	/// <returns>A new <see cref="T:System.Windows.Media.Animation.Clock" /> constructed from this <see cref="T:System.Windows.Media.Animation.Timeline" />. If this <see cref="T:System.Windows.Media.Animation.Timeline" /> is a <see cref="T:System.Windows.Media.Animation.TimelineGroup" /> that contains child timelines, a tree of <see cref="T:System.Windows.Media.Animation.Clock" /> objects is created with a controllable <see cref="T:System.Windows.Media.Animation.Clock" /> created from this <see cref="T:System.Windows.Media.Animation.Timeline" /> as the root.</returns>
	/// <param name="hasControllableRoot">true if the root <see cref="T:System.Windows.Media.Animation.Clock" /> returned should return a <see cref="T:System.Windows.Media.Animation.ClockController" /> from its <see cref="P:System.Windows.Media.Animation.Clock.Controller" /> property so that the <see cref="T:System.Windows.Media.Animation.Clock" /> tree can be interactively controlled; otherwise, false.</param>
	public Clock CreateClock(bool hasControllableRoot)
	{
		return Clock.BuildClockTreeFromTimeline(this, hasControllableRoot);
	}

	/// <summary>Returns the length of a single iteration of this <see cref="T:System.Windows.Media.Animation.Timeline" />. </summary>
	/// <returns>The length of a single iteration of this <see cref="T:System.Windows.Media.Animation.Timeline" />, or <see cref="P:System.Windows.Duration.Automatic" /> if the natural duration is unknown. </returns>
	/// <param name="clock">The <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Timeline" />.</param>
	protected internal Duration GetNaturalDuration(Clock clock)
	{
		return GetNaturalDurationCore(clock);
	}

	/// <summary>Returns the length of a single iteration of this <see cref="T:System.Windows.Media.Animation.Timeline" />. This method provides the implementation for <see cref="M:System.Windows.Media.Animation.Timeline.GetNaturalDuration(System.Windows.Media.Animation.Clock)" />. </summary>
	/// <returns>The length of a single iteration of this <see cref="T:System.Windows.Media.Animation.Timeline" />, or <see cref="P:System.Windows.Duration.Automatic" /> if the natural duration is unknown.</returns>
	/// <param name="clock">The <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Timeline" />.</param>
	protected virtual Duration GetNaturalDurationCore(Clock clock)
	{
		return Duration.Automatic;
	}

	private void ValidateTimeline()
	{
		if (AccelerationRatio + DecelerationRatio > 1.0)
		{
			throw new InvalidOperationException(SR.Timing_AccelAndDecelGreaterThanOne);
		}
	}

	internal void InternalOnFreezablePropertyChanged(Timeline originalTimeline, Timeline newTimeline)
	{
		OnFreezablePropertyChanged(originalTimeline, newTimeline);
	}

	internal bool InternalFreeze(bool isChecking)
	{
		return Freezable.Freeze(this, isChecking);
	}

	internal void InternalReadPreamble()
	{
		ReadPreamble();
	}

	internal void InternalWritePostscript()
	{
		WritePostscript();
	}

	private void AddEventHandler(EventPrivateKey key, Delegate handler)
	{
		WritePreamble();
		EventHandlersStore eventHandlersStore = EventHandlersStoreField.GetValue(this);
		if (eventHandlersStore == null)
		{
			eventHandlersStore = new EventHandlersStore();
			EventHandlersStoreField.SetValue(this, eventHandlersStore);
		}
		eventHandlersStore.Add(key, handler);
		WritePostscript();
	}

	private void CopyCommon(Timeline sourceTimeline)
	{
		EventHandlersStore value = EventHandlersStoreField.GetValue(sourceTimeline);
		if (value != null)
		{
			EventHandlersStoreField.SetValue(this, new EventHandlersStore(value));
		}
	}

	private void RemoveEventHandler(EventPrivateKey key, Delegate handler)
	{
		WritePreamble();
		EventHandlersStore value = EventHandlersStoreField.GetValue(this);
		if (value != null)
		{
			value.Remove(key, handler);
			if (value.Count == 0)
			{
				EventHandlersStoreField.ClearValue(this);
			}
			WritePostscript();
		}
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Animation.Timeline" />, making deep copies of this object's values. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new Timeline Clone()
	{
		return (Timeline)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Animation.Timeline" /> object, making deep copies of this object's current values. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new Timeline CloneCurrentValue()
	{
		return (Timeline)base.CloneCurrentValue();
	}
}
