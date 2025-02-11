using MS.Internal;

namespace System.Windows.Media.Animation;

/// <summary>Represents an easing function that creates an animation that resembles a spring oscillating back and forth until it comes to rest.  </summary>
public class ElasticEase : EasingFunctionBase
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.ElasticEase.Oscillations" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.ElasticEase.Oscillations" /> dependency property.</returns>
	public static readonly DependencyProperty OscillationsProperty = DependencyProperty.Register("Oscillations", typeof(int), typeof(ElasticEase), new PropertyMetadata(3));

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.ElasticEase.Springiness" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.ElasticEase.Springiness" /> dependency property.</returns>
	public static readonly DependencyProperty SpringinessProperty = DependencyProperty.Register("Springiness", typeof(double), typeof(ElasticEase), new PropertyMetadata(3.0));

	/// <summary>Gets or sets the number of times the target slides back and forth over the animation destination.</summary>
	/// <returns>The number of times the target slides back and forth over the animation destination. This value must be greater than or equal to 0. The default is 3.</returns>
	public int Oscillations
	{
		get
		{
			return (int)GetValue(OscillationsProperty);
		}
		set
		{
			SetValueInternal(OscillationsProperty, value);
		}
	}

	/// <summary>Gets or sets the stiffness of the spring. The smaller the Springiness value is, the stiffer the spring and the faster the elasticity decreases in intensity over each oscillation.</summary>
	/// <returns>A positive number that specifies the stiffness of the spring. The default value is 3.</returns>
	public double Springiness
	{
		get
		{
			return (double)GetValue(SpringinessProperty);
		}
		set
		{
			SetValueInternal(SpringinessProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.ElasticEase" /> class. </summary>
	public ElasticEase()
	{
	}

	/// <summary>Provides the logic portion of the easing function that you can override to produce the <see cref="F:System.Windows.Media.Animation.EasingMode.EaseIn" /> mode of the custom easing function.</summary>
	/// <returns>A double that represents the transformed progress.</returns>
	/// <param name="normalizedTime">Normalized time (progress) of the animation.</param>
	protected override double EaseInCore(double normalizedTime)
	{
		double num = Math.Max(0.0, Oscillations);
		double num2 = Math.Max(0.0, Springiness);
		double num3 = ((!DoubleUtil.IsZero(num2)) ? ((Math.Exp(num2 * normalizedTime) - 1.0) / (Math.Exp(num2) - 1.0)) : normalizedTime);
		return num3 * Math.Sin((Math.PI * 2.0 * num + Math.PI / 2.0) * normalizedTime);
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Freezable" /> derived class. When creating a derived class, you must override this method.</summary>
	/// <returns>The new instance.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new ElasticEase();
	}
}
