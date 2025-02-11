using System.Windows.Media.Media3D;

namespace System.Windows.Media.Animation;

/// <summary>Abstract class that, when implemented, animates a <see cref="T:System.Windows.Media.Media3D.Quaternion" /> value.</summary>
public abstract class QuaternionAnimationBase : AnimationTimeline
{
	/// <summary>Gets the type of value this animation generates.</summary>
	/// <returns>The type of value produced by this animation.</returns>
	public sealed override Type TargetPropertyType
	{
		get
		{
			ReadPreamble();
			return typeof(Quaternion);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.QuaternionAnimationBase" /> class.</summary>
	protected QuaternionAnimationBase()
	{
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Animation.QuaternionAnimationBase" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new QuaternionAnimationBase Clone()
	{
		return (QuaternionAnimationBase)base.Clone();
	}

	/// <summary>Gets the current value of the animation.</summary>
	/// <returns>The current value of the animation.</returns>
	/// <param name="defaultOriginValue">The origin value provided to the animation if the animation does not have its own start value. </param>
	/// <param name="defaultDestinationValue">The destination value provided to the animation if the animation does not have its own destination value.</param>
	/// <param name="animationClock">The <see cref="T:System.Windows.Media.Animation.AnimationClock" /> which can generate the <see cref="P:System.Windows.Media.Animation.Clock.CurrentTime" /> or <see cref="P:System.Windows.Media.Animation.Clock.CurrentProgress" /> value to be used by the animation to generate its output value.</param>
	public sealed override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
	{
		if (defaultOriginValue == null)
		{
			throw new ArgumentNullException("defaultOriginValue");
		}
		if (defaultDestinationValue == null)
		{
			throw new ArgumentNullException("defaultDestinationValue");
		}
		return GetCurrentValue((Quaternion)defaultOriginValue, (Quaternion)defaultDestinationValue, animationClock);
	}

	/// <summary>Gets the current value of the animation.</summary>
	/// <returns>The current value of the animation.</returns>
	/// <param name="defaultOriginValue">The origin value provided to the animation if the animation does not have its own start value. </param>
	/// <param name="defaultDestinationValue">The destination value provided to the animation if the animation does not have its own destination value.</param>
	/// <param name="animationClock">The <see cref="T:System.Windows.Media.Animation.AnimationClock" /> which can generate the <see cref="P:System.Windows.Media.Animation.Clock.CurrentTime" /> or <see cref="P:System.Windows.Media.Animation.Clock.CurrentProgress" /> value to be used by the animation to generate its output value.</param>
	public Quaternion GetCurrentValue(Quaternion defaultOriginValue, Quaternion defaultDestinationValue, AnimationClock animationClock)
	{
		ReadPreamble();
		if (animationClock == null)
		{
			throw new ArgumentNullException("animationClock");
		}
		if (animationClock.CurrentState == ClockState.Stopped)
		{
			return defaultDestinationValue;
		}
		return GetCurrentValueCore(defaultOriginValue, defaultDestinationValue, animationClock);
	}

	/// <summary>Gets the current value of the animation.</summary>
	/// <returns>The current value of the animation.</returns>
	/// <param name="defaultOriginValue">The origin value provided to the animation if the animation does not have its own start value. </param>
	/// <param name="defaultDestinationValue">The destination value provided to the animation if the animation does not have its own destination value.</param>
	/// <param name="animationClock">The <see cref="T:System.Windows.Media.Animation.AnimationClock" /> which can generate the <see cref="P:System.Windows.Media.Animation.Clock.CurrentTime" /> or <see cref="P:System.Windows.Media.Animation.Clock.CurrentProgress" /> value to be used by the animation to generate its output value.</param>
	protected abstract Quaternion GetCurrentValueCore(Quaternion defaultOriginValue, Quaternion defaultDestinationValue, AnimationClock animationClock);
}
