namespace System.Windows.Media.Animation;

/// <summary>Defines a segment of time over which output values are produced. These values are used to animate a target property.</summary>
public abstract class AnimationTimeline : Timeline
{
	/// <summary>Identifies the IsAdditive dependency property.</summary>
	/// <returns>The IsAdditive dependency property identifier. </returns>
	public static readonly DependencyProperty IsAdditiveProperty = DependencyProperty.Register("IsAdditive", typeof(bool), typeof(AnimationTimeline), new PropertyMetadata(false, AnimationTimeline_PropertyChangedFunction));

	/// <summary>Identifies the IsCumulative dependency property.</summary>
	/// <returns>The IsCumulative dependency property identifier.</returns>
	public static readonly DependencyProperty IsCumulativeProperty = DependencyProperty.Register("IsCumulative", typeof(bool), typeof(AnimationTimeline), new PropertyMetadata(false, AnimationTimeline_PropertyChangedFunction));

	/// <summary>When overridden in a derived class, gets the <see cref="T:System.Type" /> of property that can be animated.</summary>
	/// <returns>The type of property that can be animated by this animation.</returns>
	public abstract Type TargetPropertyType { get; }

	/// <summary>Gets a value that indicates whether this animation uses the defaultDestinationValue parameter of the <see cref="M:System.Windows.Media.Animation.AnimationTimeline.GetCurrentValue(System.Object,System.Object,System.Windows.Media.Animation.AnimationClock)" /> method as its destination value.</summary>
	/// <returns>true if the defaultDesintationValue parameter of the <see cref="M:System.Windows.Media.Animation.AnimationTimeline.GetCurrentValue(System.Object,System.Object,System.Windows.Media.Animation.AnimationClock)" /> method is the value of this animation when reaches the end of its simple duration (its clock has a <see cref="P:System.Windows.Media.Animation.Clock.CurrentProgress" /> of 1); otherwise, false. The default implementation always returns false.</returns>
	public virtual bool IsDestinationDefault
	{
		get
		{
			ReadPreamble();
			return false;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.AnimationTimeline" /> class.</summary>
	protected AnimationTimeline()
	{
	}

	private static void AnimationTimeline_PropertyChangedFunction(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((AnimationTimeline)d).PropertyChanged(e.Property);
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Animation.AnimationTimeline" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new AnimationTimeline Clone()
	{
		return (AnimationTimeline)base.Clone();
	}

	/// <summary>Creates a <see cref="T:System.Windows.Media.Animation.Clock" /> for this <see cref="T:System.Windows.Media.Animation.AnimationTimeline" />.</summary>
	/// <returns>A clock for this <see cref="T:System.Windows.Media.Animation.AnimationTimeline" />.</returns>
	protected internal override Clock AllocateClock()
	{
		return new AnimationClock(this);
	}

	/// <summary>Creates a new <see cref="T:System.Windows.Media.Animation.AnimationClock" /> from this <see cref="T:System.Windows.Media.Animation.AnimationTimeline" />. </summary>
	/// <returns>A new clock, created from this <see cref="T:System.Windows.Media.Animation.AnimationTimeline" />.</returns>
	public new AnimationClock CreateClock()
	{
		return (AnimationClock)base.CreateClock();
	}

	/// <summary>Gets the current value of the animation.</summary>
	/// <returns>The value this animation believes should be the current value for the property. </returns>
	/// <param name="defaultOriginValue">The origin value provided to the animation if the animation does not have its own start value. If this animation is the first in a composition chain it will be the base value of the property being animated; otherwise it will be the value returned by the previous animation in the chain.</param>
	/// <param name="defaultDestinationValue">The destination value provided to the animation if the animation does not have its own destination value.</param>
	/// <param name="animationClock">The <see cref="T:System.Windows.Media.Animation.AnimationClock" /> which can generate the <see cref="P:System.Windows.Media.Animation.Clock.CurrentTime" /> or <see cref="P:System.Windows.Media.Animation.Clock.CurrentProgress" /> value to be used by the animation to generate its output value.</param>
	public virtual object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
	{
		ReadPreamble();
		return defaultDestinationValue;
	}

	/// <summary>Returns the length of a single iteration of this <see cref="T:System.Windows.Media.Animation.AnimationTimeline" />. </summary>
	/// <returns>The animation's natural duration. This method always returns a <see cref="T:System.Windows.Duration" /> of 1 second. </returns>
	/// <param name="clock">The clock that was created for this <see cref="T:System.Windows.Media.Animation.AnimationTimeline" />.</param>
	protected override Duration GetNaturalDurationCore(Clock clock)
	{
		return new TimeSpan(0, 0, 1);
	}
}
