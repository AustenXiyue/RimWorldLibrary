namespace System.Windows.Media.Animation;

/// <summary>Maintains the run-time state of an <see cref="T:System.Windows.Media.Animation.AnimationTimeline" /> and processes its output values. </summary>
public class AnimationClock : Clock
{
	/// <summary>Gets the <see cref="T:System.Windows.Media.Animation.AnimationTimeline" /> that describes this clock's behavior.</summary>
	/// <returns>The animation that describes this clock's behavior.</returns>
	public new AnimationTimeline Timeline => (AnimationTimeline)base.Timeline;

	internal override bool NeedsTicksWhenActive => true;

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.AnimationClock" /> class. </summary>
	/// <param name="animation">The animation that describes this clock's output values and timing behaviors.</param>
	protected internal AnimationClock(AnimationTimeline animation)
		: base(animation)
	{
	}

	/// <summary>Gets the current output value of the <see cref="T:System.Windows.Media.Animation.AnimationClock" />.</summary>
	/// <returns>The current value of this <see cref="T:System.Windows.Media.Animation.AnimationClock" />. </returns>
	/// <param name="defaultOriginValue">The origin value provided to the clock if its animation does not have its own start value. If this clock is the first in a composition chain it will be the base value of the property being animated; otherwise it will be the value returned by the previous clock in the chain</param>
	/// <param name="defaultDestinationValue">The destination value provided to the clock if its animation does not have its own destination value. If this clock is the first in a composition chain it will be the base value of the property being animated; otherwise it will be the value returned by the previous clock in the chain </param>
	public object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue)
	{
		return ((AnimationTimeline)base.Timeline).GetCurrentValue(defaultOriginValue, defaultDestinationValue, this);
	}
}
