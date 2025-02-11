using System.Windows.Media.Media3D;

namespace System.Windows.Media.Animation;

/// <summary>Abstract class that, when implemented, animates a <see cref="T:System.Windows.Media.Media3D.Rotation3D" /> value.</summary>
public abstract class Rotation3DAnimationBase : AnimationTimeline
{
	/// <summary>Gets the type of value this animation generates.</summary>
	/// <returns>The type of value produced by this animation.</returns>
	public sealed override Type TargetPropertyType
	{
		get
		{
			ReadPreamble();
			return typeof(Rotation3D);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.Rotation3DAnimationBase" /> class.</summary>
	protected Rotation3DAnimationBase()
	{
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Animation.Rotation3DAnimationBase" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Rotation3DAnimationBase Clone()
	{
		return (Rotation3DAnimationBase)base.Clone();
	}

	/// <summary>Gets the current value of the animation.</summary>
	/// <returns>The current value of the animation.</returns>
	/// <param name="defaultOriginValue">The origin value provided to the animation if the animation does not have its own start value. If this animation is the first in a composition chain it will be the base value of the property being animated; otherwise it will be the value returned by the previous animation in the chain.</param>
	/// <param name="defaultDestinationValue">The destination value provided to the animation if the animation does not have its own destination value.</param>
	/// <param name="animationClock">The <see cref="T:System.Windows.Media.Animation.AnimationClock" /> which can generate the <see cref="P:System.Windows.Media.Animation.Clock.CurrentTime" /> or <see cref="P:System.Windows.Media.Animation.Clock.CurrentProgress" /> value to be used by the animation to generate its output value.</param>
	public sealed override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
	{
		return GetCurrentValue((Rotation3D)defaultOriginValue, (Rotation3D)defaultDestinationValue, animationClock);
	}

	/// <summary>Gets the current value of the animation.</summary>
	/// <returns>The current value of the animation.</returns>
	/// <param name="defaultOriginValue">The origin value provided to the animation if the animation does not have its own start value. If this animation is the first in a composition chain it will be the base value of the property being animated; otherwise it will be the value returned by the previous animation in the chain.</param>
	/// <param name="defaultDestinationValue">The destination value provided to the animation if the animation does not have its own destination value.</param>
	/// <param name="animationClock">The <see cref="T:System.Windows.Media.Animation.AnimationClock" /> which can generate the <see cref="P:System.Windows.Media.Animation.Clock.CurrentTime" /> or <see cref="P:System.Windows.Media.Animation.Clock.CurrentProgress" /> value to be used by the animation to generate its output value.</param>
	public Rotation3D GetCurrentValue(Rotation3D defaultOriginValue, Rotation3D defaultDestinationValue, AnimationClock animationClock)
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

	/// <summary>Calculates a value that represents the current value of the property being animated, as determined by the host animation. </summary>
	/// <returns>The calculated value of the property, as determined by the current animation.</returns>
	/// <param name="defaultOriginValue">The suggested origin value, used if the animation does not have its own explicitly set start value.</param>
	/// <param name="defaultDestinationValue">The suggested destination value, used if the animation does not have its own explicitly set end value.</param>
	/// <param name="animationClock">An <see cref="T:System.Windows.Media.Animation.AnimationClock" /> that generates the <see cref="P:System.Windows.Media.Animation.Clock.CurrentTime" /> or <see cref="P:System.Windows.Media.Animation.Clock.CurrentProgress" /> used by the host animation.</param>
	protected abstract Rotation3D GetCurrentValueCore(Rotation3D defaultOriginValue, Rotation3D defaultDestinationValue, AnimationClock animationClock);
}
