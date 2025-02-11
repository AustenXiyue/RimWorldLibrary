using System.ComponentModel;
using MS.Internal;

namespace System.Windows.Media.Animation;

/// <summary>Defines a segment of time that may contain child <see cref="T:System.Windows.Media.Animation.Timeline" /> objects. These child timelines become active according to their respective <see cref="P:System.Windows.Media.Animation.Timeline.BeginTime" /> properties. Also, child timelines are able to overlap (run in parallel) with each other.</summary>
public class ParallelTimeline : TimelineGroup
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.ParallelTimeline.SlipBehavior" /> dependency property.</summary>
	public static readonly DependencyProperty SlipBehaviorProperty = DependencyProperty.Register("SlipBehavior", typeof(SlipBehavior), typeof(ParallelTimeline), new PropertyMetadata(SlipBehavior.Grow, ParallelTimeline_PropertyChangedFunction), ValidateSlipBehavior);

	/// <summary>Gets or sets a value that specifies how this timeline will behave when one or more of its <see cref="T:System.Windows.Media.Animation.Timeline" /> children slips.  </summary>
	/// <returns>A value that indicates how this timeline will behave when one or more of its <see cref="T:System.Windows.Media.Animation.Timeline" /> children slips. The default value is <see cref="F:System.Windows.Media.Animation.SlipBehavior.Grow" />.</returns>
	[DefaultValue(SlipBehavior.Grow)]
	public SlipBehavior SlipBehavior
	{
		get
		{
			return (SlipBehavior)GetValue(SlipBehaviorProperty);
		}
		set
		{
			SetValue(SlipBehaviorProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.ParallelTimeline" /> class. </summary>
	public ParallelTimeline()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.ParallelTimeline" /> class with the specified <see cref="P:System.Windows.Media.Animation.Timeline.BeginTime" />.</summary>
	/// <param name="beginTime">The <see cref="P:System.Windows.Media.Animation.Timeline.BeginTime" /> for this <see cref="T:System.Windows.Media.Animation.TimelineGroup" />.</param>
	public ParallelTimeline(TimeSpan? beginTime)
		: base(beginTime)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.ParallelTimeline" /> class with the specified <see cref="P:System.Windows.Media.Animation.Timeline.BeginTime" /> and <see cref="P:System.Windows.Media.Animation.Timeline.Duration" />.</summary>
	/// <param name="beginTime">The <see cref="P:System.Windows.Media.Animation.Timeline.BeginTime" /> for this <see cref="T:System.Windows.Media.Animation.TimelineGroup" />.</param>
	/// <param name="duration">The <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> for this <see cref="T:System.Windows.Media.Animation.TimelineGroup" />.</param>
	public ParallelTimeline(TimeSpan? beginTime, Duration duration)
		: base(beginTime, duration)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.ParallelTimeline" /> class with the specified <see cref="P:System.Windows.Media.Animation.Timeline.BeginTime" />, <see cref="P:System.Windows.Media.Animation.Timeline.Duration" />, and <see cref="P:System.Windows.Media.Animation.Timeline.RepeatBehavior" />.</summary>
	/// <param name="beginTime">The <see cref="P:System.Windows.Media.Animation.Timeline.BeginTime" /> for this <see cref="T:System.Windows.Media.Animation.TimelineGroup" />.</param>
	/// <param name="duration">The <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> for this <see cref="T:System.Windows.Media.Animation.TimelineGroup" />.</param>
	/// <param name="repeatBehavior">The <see cref="P:System.Windows.Media.Animation.Timeline.RepeatBehavior" /> for this <see cref="T:System.Windows.Media.Animation.TimelineGroup" />.</param>
	public ParallelTimeline(TimeSpan? beginTime, Duration duration, RepeatBehavior repeatBehavior)
		: base(beginTime, duration, repeatBehavior)
	{
	}

	/// <summary> Return the natural duration (duration of a single iteration) from a specified <see cref="T:System.Windows.Media.Animation.Clock" />. </summary>
	/// <returns>The <see cref="T:System.Windows.Duration" /> quantity representing the natural duration.</returns>
	/// <param name="clock">The <see cref="T:System.Windows.Media.Animation.Clock" /> to return the natural duration from.</param>
	protected override Duration GetNaturalDurationCore(Clock clock)
	{
		Duration duration = TimeSpan.Zero;
		if (clock is ClockGroup { InternalChildren: { } internalChildren })
		{
			bool flag = false;
			for (int i = 0; i < internalChildren.Count; i++)
			{
				Duration endOfActivePeriod = internalChildren[i].EndOfActivePeriod;
				if (endOfActivePeriod == Duration.Forever)
				{
					return Duration.Forever;
				}
				if (endOfActivePeriod == Duration.Automatic)
				{
					flag = true;
				}
				else if (endOfActivePeriod > duration)
				{
					duration = endOfActivePeriod;
				}
			}
			if (flag)
			{
				return Duration.Automatic;
			}
		}
		return duration;
	}

	private static bool ValidateSlipBehavior(object value)
	{
		return TimeEnumHelper.IsValidSlipBehavior((SlipBehavior)value);
	}

	internal static void ParallelTimeline_PropertyChangedFunction(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ParallelTimeline)d).PropertyChanged(e.Property);
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Animation.ParallelTimeline" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new ParallelTimeline Clone()
	{
		return (ParallelTimeline)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Animation.ParallelTimeline" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new ParallelTimeline CloneCurrentValue()
	{
		return (ParallelTimeline)base.CloneCurrentValue();
	}

	/// <summary> Creates a new instance of this <see cref="T:System.Windows.Freezable" />.              </summary>
	/// <returns>The new <see cref="T:System.Windows.Freezable" />.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new ParallelTimeline();
	}
}
