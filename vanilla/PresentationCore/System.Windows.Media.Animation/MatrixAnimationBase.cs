namespace System.Windows.Media.Animation;

/// <summary>Abstract class that, when implemented, animates a <see cref="T:System.Windows.Media.Matrix" /> value. </summary>
public abstract class MatrixAnimationBase : AnimationTimeline
{
	/// <summary>Gets the type of value this animation generates.</summary>
	/// <returns>The type of value produced by this animation.</returns>
	public sealed override Type TargetPropertyType
	{
		get
		{
			ReadPreamble();
			return typeof(Matrix);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.MatrixAnimationBase" /> class.</summary>
	protected MatrixAnimationBase()
	{
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Animation.MatrixAnimationBase" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new MatrixAnimationBase Clone()
	{
		return (MatrixAnimationBase)base.Clone();
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
		return GetCurrentValue((Matrix)defaultOriginValue, (Matrix)defaultDestinationValue, animationClock);
	}

	/// <summary>Gets the current value of the animation.</summary>
	/// <returns>The value this animation believes should be the current value for the property.</returns>
	/// <param name="defaultOriginValue">The origin value provided to the animation if the animation does not have its own start value. </param>
	/// <param name="defaultDestinationValue">The destination value provided to the animation if the animation does not have its own destination value.</param>
	/// <param name="animationClock">The <see cref="T:System.Windows.Media.Animation.AnimationClock" /> which can generate the <see cref="P:System.Windows.Media.Animation.Clock.CurrentTime" /> or <see cref="P:System.Windows.Media.Animation.Clock.CurrentProgress" /> value to be used by the animation to generate its output value.</param>
	public Matrix GetCurrentValue(Matrix defaultOriginValue, Matrix defaultDestinationValue, AnimationClock animationClock)
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

	/// <summary>When implemented in a derived class, calculates the current value of the animation.</summary>
	/// <returns>The value this animation believes should be the current value for the property.</returns>
	/// <param name="defaultOriginValue">The suggested origin value, used if the animation does not have its own explicitly set start value. </param>
	/// <param name="defaultDestinationValue">The suggested destination value, used if the animation does not have its own explicitly set end value.</param>
	/// <param name="animationClock">An <see cref="T:System.Windows.Media.Animation.AnimationClock" /> that generates the <see cref="P:System.Windows.Media.Animation.Clock.CurrentTime" /> or <see cref="P:System.Windows.Media.Animation.Clock.CurrentProgress" /> used by the host animation.</param>
	protected abstract Matrix GetCurrentValueCore(Matrix defaultOriginValue, Matrix defaultDestinationValue, AnimationClock animationClock);
}
