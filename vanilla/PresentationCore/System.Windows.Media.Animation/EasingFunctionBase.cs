namespace System.Windows.Media.Animation;

/// <summary>Provides the base class for all the easing functions. </summary>
public abstract class EasingFunctionBase : Freezable, IEasingFunction
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.EasingFunctionBase.EasingMode" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.EasingFunctionBase.EasingMode" /> dependency property.</returns>
	public static readonly DependencyProperty EasingModeProperty = DependencyProperty.Register("EasingMode", typeof(EasingMode), typeof(EasingFunctionBase), new PropertyMetadata(EasingMode.EaseOut));

	/// <summary>Gets or sets a value that specifies how the animation interpolates. </summary>
	/// <returns>One of the enumeration values that specifies how the animation interpolates. The default is <see cref="F:System.Windows.Media.Animation.EasingMode.EaseOut" />.</returns>
	public EasingMode EasingMode
	{
		get
		{
			return (EasingMode)GetValue(EasingModeProperty);
		}
		set
		{
			SetValueInternal(EasingModeProperty, value);
		}
	}

	/// <summary>Transforms normalized time to control the pace of an animation.</summary>
	/// <returns>A double that represents the transformed progress.</returns>
	/// <param name="normalizedTime">Normalized time (progress) of the animation, which is a value from 0 through 1.</param>
	public double Ease(double normalizedTime)
	{
		switch (EasingMode)
		{
		case EasingMode.EaseIn:
			return EaseInCore(normalizedTime);
		case EasingMode.EaseOut:
			return 1.0 - EaseInCore(1.0 - normalizedTime);
		default:
			if (!(normalizedTime < 0.5))
			{
				return (1.0 - EaseInCore((1.0 - normalizedTime) * 2.0)) * 0.5 + 0.5;
			}
			return EaseInCore(normalizedTime * 2.0) * 0.5;
		}
	}

	/// <summary>Provides the logic portion of the easing function that you can override to produce the <see cref="F:System.Windows.Media.Animation.EasingMode.EaseIn" /> mode of the custom easing function.  </summary>
	/// <returns>A double that represents the transformed progress.</returns>
	/// <param name="normalizedTime">Normalized time (progress) of the animation, which is a value from 0 through 1.</param>
	protected abstract double EaseInCore(double normalizedTime);

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.EasingFunctionBase" /> class. </summary>
	protected EasingFunctionBase()
	{
	}
}
