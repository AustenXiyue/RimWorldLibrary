using MS.Internal.PresentationCore;

namespace System.Windows.Media.Animation;

/// <summary>A class that enables you to associate easing functions with a <see cref="T:System.Windows.Media.Animation.Int16AnimationUsingKeyFrames" /> key frame animation.</summary>
public class EasingInt16KeyFrame : Int16KeyFrame
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.EasingInt16KeyFrame.EasingFunction" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.EasingInt16KeyFrame.EasingFunction" /> dependency property.</returns>
	public static readonly DependencyProperty EasingFunctionProperty = DependencyProperty.Register("EasingFunction", typeof(IEasingFunction), typeof(EasingInt16KeyFrame));

	/// <summary>Gets or sets the easing function applied to the key frame.</summary>
	/// <returns>The easing function applied to the key frame.</returns>
	public IEasingFunction EasingFunction
	{
		get
		{
			return (IEasingFunction)GetValue(EasingFunctionProperty);
		}
		set
		{
			SetValueInternal(EasingFunctionProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.EasingInt16KeyFrame" /> class. </summary>
	public EasingInt16KeyFrame()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.EasingInt16KeyFrame" /> class with the specified <see cref="T:System.Int16" /> value. </summary>
	/// <param name="value">The initial <see cref="T:System.Int16" /> value.</param>
	public EasingInt16KeyFrame(short value)
		: this()
	{
		base.Value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.EasingInt16KeyFrame" /> class with the specified <see cref="T:System.Int16" /> value and key time. </summary>
	/// <param name="value">The initial <see cref="T:System.Int16" /> value.</param>
	/// <param name="keyTime">The initial key time.</param>
	public EasingInt16KeyFrame(short value, KeyTime keyTime)
		: this()
	{
		base.Value = value;
		base.KeyTime = keyTime;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.EasingInt16KeyFrame" /> class with the specified <see cref="T:System.Int16" /> value, key time, and easing function. </summary>
	/// <param name="value">The initial <see cref="T:System.Int16" /> value.</param>
	/// <param name="keyTime">The initial key time.</param>
	/// <param name="easingFunction">The easing function.</param>
	public EasingInt16KeyFrame(short value, KeyTime keyTime, IEasingFunction easingFunction)
		: this()
	{
		base.Value = value;
		base.KeyTime = keyTime;
		EasingFunction = easingFunction;
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Freezable" /> derived class. When creating a derived class, you must override this method.</summary>
	/// <returns>The new instance.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new EasingInt16KeyFrame();
	}

	/// <summary>Interpolates, according to the easing function used, between the previous key frame value and the value of the current key frame, using the supplied progress increment.</summary>
	/// <returns>The output value of this key frame given the specified base value and progress.</returns>
	/// <param name="baseValue"> The value to animate from.</param>
	/// <param name="keyFrameProgress"> A value between 0.0 and 1.0, inclusive, that specifies the percentage of time that has elapsed for this key frame.</param>
	protected override short InterpolateValueCore(short baseValue, double keyFrameProgress)
	{
		IEasingFunction easingFunction = EasingFunction;
		if (easingFunction != null)
		{
			keyFrameProgress = easingFunction.Ease(keyFrameProgress);
		}
		if (keyFrameProgress == 0.0)
		{
			return baseValue;
		}
		if (keyFrameProgress == 1.0)
		{
			return base.Value;
		}
		return AnimatedTypeHelpers.InterpolateInt16(baseValue, base.Value, keyFrameProgress);
	}
}
