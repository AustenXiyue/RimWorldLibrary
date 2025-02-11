namespace System.Windows.Media.Animation;

/// <summary>Represents an easing function that retracts the motion of an animation slightly before it begins to animate in the path indicated.</summary>
public class BackEase : EasingFunctionBase
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.BackEase.Amplitude" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.BackEase.Amplitude" /> dependency property.</returns>
	public static readonly DependencyProperty AmplitudeProperty = DependencyProperty.Register("Amplitude", typeof(double), typeof(BackEase), new PropertyMetadata(1.0));

	/// <summary>Gets or sets the amplitude of retraction associated with a <see cref="T:System.Windows.Media.Animation.BackEase" /> animation.</summary>
	/// <returns>The amplitude of retraction associated with a <see cref="T:System.Windows.Media.Animation.BackEase" /> animation. This value must be greater than or equal to 0.The default value is 1.</returns>
	public double Amplitude
	{
		get
		{
			return (double)GetValue(AmplitudeProperty);
		}
		set
		{
			SetValueInternal(AmplitudeProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.BackEase" /> class. </summary>
	public BackEase()
	{
	}

	/// <summary>Provides the logic portion of the easing function that you can override to produce the <see cref="F:System.Windows.Media.Animation.EasingMode.EaseIn" /> mode of the custom easing function.</summary>
	/// <returns>A double that represents the transformed progress.</returns>
	/// <param name="normalizedTime">Normalized time (progress) of the animation.</param>
	protected override double EaseInCore(double normalizedTime)
	{
		double num = Math.Max(0.0, Amplitude);
		return Math.Pow(normalizedTime, 3.0) - normalizedTime * num * Math.Sin(Math.PI * normalizedTime);
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Freezable" /> derived class. When creating a derived class, you must override this method.</summary>
	/// <returns>The new instance.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new BackEase();
	}
}
