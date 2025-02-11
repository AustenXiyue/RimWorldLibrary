using MS.Internal.PresentationFramework;

namespace System.Windows.Media.Animation;

/// <summary>A class that enables you to associate easing functions with a <see cref="T:System.Windows.Media.Animation.ThicknessAnimationUsingKeyFrames" /> key frame animation.</summary>
public class EasingThicknessKeyFrame : ThicknessKeyFrame
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.EasingThicknessKeyFrame.EasingFunction" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.EasingThicknessKeyFrame.EasingFunction" /> dependency property.</returns>
	public static readonly DependencyProperty EasingFunctionProperty = DependencyProperty.Register("EasingFunction", typeof(IEasingFunction), typeof(EasingThicknessKeyFrame));

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

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.EasingThicknessKeyFrame" /> class. </summary>
	public EasingThicknessKeyFrame()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.EasingThicknessKeyFrame" /> class with the specified <see cref="T:System.Windows.Thickness" /> value. </summary>
	/// <param name="value">The initial <see cref="T:System.Windows.Thickness" /> value.</param>
	public EasingThicknessKeyFrame(Thickness value)
		: this()
	{
		base.Value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.EasingThicknessKeyFrame" /> class with the specified <see cref="T:System.Windows.Thickness" /> value and key time. </summary>
	/// <param name="value">The initial <see cref="T:System.Windows.Thickness" /> value.</param>
	/// <param name="keyTime">The initial key time.</param>
	public EasingThicknessKeyFrame(Thickness value, KeyTime keyTime)
		: this()
	{
		base.Value = value;
		base.KeyTime = keyTime;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.EasingThicknessKeyFrame" /> class with the specified <see cref="T:System.Windows.Thickness" /> value, key time, and easing function. </summary>
	/// <param name="value">The initial <see cref="T:System.Windows.Thickness" /> value.</param>
	/// <param name="keyTime">The initial key time.</param>
	/// <param name="easingFunction">The easing function.</param>
	public EasingThicknessKeyFrame(Thickness value, KeyTime keyTime, IEasingFunction easingFunction)
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
		return new EasingThicknessKeyFrame();
	}

	/// <summary>Interpolates, according to the easing function used, between the previous key frame value and the value of the current key frame, using the supplied progress increment.</summary>
	/// <returns>The output value of this key frame given the specified base value and progress.</returns>
	/// <param name="baseValue"> The value to animate from.</param>
	/// <param name="keyFrameProgress"> A value between 0.0 and 1.0, inclusive, that specifies the percentage of time that has elapsed for this key frame.</param>
	protected override Thickness InterpolateValueCore(Thickness baseValue, double keyFrameProgress)
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
		return AnimatedTypeHelpers.InterpolateThickness(baseValue, base.Value, keyFrameProgress);
	}
}
