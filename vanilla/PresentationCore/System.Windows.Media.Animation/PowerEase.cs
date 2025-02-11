namespace System.Windows.Media.Animation;

/// <summary>Represents an easing function that creates an animation that accelerates and/or decelerates using the formula f(t) = tp where p is equal to the <see cref="P:System.Windows.Media.Animation.PowerEase.Power" /> property.</summary>
public class PowerEase : EasingFunctionBase
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.PowerEase.Power" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.PowerEase.Power" /> dependency property.</returns>
	public static readonly DependencyProperty PowerProperty = DependencyProperty.Register("Power", typeof(double), typeof(PowerEase), new PropertyMetadata(2.0));

	/// <summary>Gets or sets the exponential power of the animation interpolation. For example, a value of 7 will create an animation interpolation curve that follows the formula f(t) = t7.</summary>
	/// <returns>The exponential power of the animation interpolation. This value must be greater or equal to 0. The default is 2.</returns>
	public double Power
	{
		get
		{
			return (double)GetValue(PowerProperty);
		}
		set
		{
			SetValueInternal(PowerProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.PowerEase" /> class. </summary>
	public PowerEase()
	{
	}

	/// <summary>Provides the logic portion of the easing function that you can override to produce the <see cref="F:System.Windows.Media.Animation.EasingMode.EaseIn" /> mode of the custom easing function.</summary>
	/// <returns>A double that represents the transformed progress.</returns>
	/// <param name="normalizedTime">Normalized time (progress) of the animation.</param>
	protected override double EaseInCore(double normalizedTime)
	{
		double y = Math.Max(0.0, Power);
		return Math.Pow(normalizedTime, y);
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Freezable" /> derived class. When creating a derived class, you must override this method.</summary>
	/// <returns>The new instance.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new PowerEase();
	}
}
