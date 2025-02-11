using MS.Internal;

namespace System.Windows.Media.Animation;

/// <summary>Represents an easing function that creates an animation that accelerates and/or decelerates using an exponential formula.</summary>
public class ExponentialEase : EasingFunctionBase
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.ExponentialEase.Exponent" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.ExponentialEase.Exponent" /> dependency property.</returns>
	public static readonly DependencyProperty ExponentProperty = DependencyProperty.Register("Exponent", typeof(double), typeof(ExponentialEase), new PropertyMetadata(2.0));

	/// <summary>Gets or sets the exponent used to determine the interpolation of the animation.</summary>
	/// <returns>The exponent used to determine the interpolation of the animation. The default is 2.</returns>
	public double Exponent
	{
		get
		{
			return (double)GetValue(ExponentProperty);
		}
		set
		{
			SetValueInternal(ExponentProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.ExponentialEase" /> class. </summary>
	public ExponentialEase()
	{
	}

	/// <summary>Provides the logic portion of the easing function that you can override to produce the <see cref="F:System.Windows.Media.Animation.EasingMode.EaseIn" /> mode of the custom easing function.</summary>
	/// <returns>A double that represents the transformed progress.</returns>
	/// <param name="normalizedTime">Normalized time (progress) of the animation.</param>
	protected override double EaseInCore(double normalizedTime)
	{
		double exponent = Exponent;
		if (DoubleUtil.IsZero(exponent))
		{
			return normalizedTime;
		}
		return (Math.Exp(exponent * normalizedTime) - 1.0) / (Math.Exp(exponent) - 1.0);
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Freezable" /> derived class. When creating a derived class, you must override this method.</summary>
	/// <returns>The new instance.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new ExponentialEase();
	}
}
