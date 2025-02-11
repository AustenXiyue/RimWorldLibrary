using MS.Internal;

namespace System.Windows.Media.Animation;

/// <summary>Represents an easing function that creates an animated bouncing effect.</summary>
public class BounceEase : EasingFunctionBase
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.BounceEase.Bounces" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.BounceEase.Bounces" /> dependency property.</returns>
	public static readonly DependencyProperty BouncesProperty = DependencyProperty.Register("Bounces", typeof(int), typeof(BounceEase), new PropertyMetadata(3));

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.BounceEase.Bounciness" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.BounceEase.Bounciness" /> dependency property.</returns>
	public static readonly DependencyProperty BouncinessProperty = DependencyProperty.Register("Bounciness", typeof(double), typeof(BounceEase), new PropertyMetadata(2.0));

	/// <summary>Gets or sets the number of bounces. </summary>
	/// <returns>The number of bounces. The value must be greater than or equal to zero. Negative values resolve to zero. The default is 3. </returns>
	public int Bounces
	{
		get
		{
			return (int)GetValue(BouncesProperty);
		}
		set
		{
			SetValueInternal(BouncesProperty, value);
		}
	}

	/// <summary>Gets or sets a value that specifies how bouncy the bounce animation is. Low values of this property result in bounces with little lose of height between bounces (more bouncy) while high values result in dampened bounces (less bouncy). </summary>
	/// <returns>The value that specifies how bouncy the bounce animation is. This value must be positive. The default value is 2.</returns>
	public double Bounciness
	{
		get
		{
			return (double)GetValue(BouncinessProperty);
		}
		set
		{
			SetValueInternal(BouncinessProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.BounceEase" /> class. </summary>
	public BounceEase()
	{
	}

	/// <summary>Provides the logic portion of the easing function that you can override to produce the <see cref="F:System.Windows.Media.Animation.EasingMode.EaseIn" /> mode of the custom easing function.</summary>
	/// <returns>A double that represents the transformed progress.</returns>
	/// <param name="normalizedTime">Normalized time (progress) of the animation.</param>
	protected override double EaseInCore(double normalizedTime)
	{
		double num = Math.Max(0.0, Bounces);
		double num2 = Bounciness;
		if (num2 < 1.0 || DoubleUtil.IsOne(num2))
		{
			num2 = 1.001;
		}
		double num3 = Math.Pow(num2, num);
		double num4 = 1.0 - num2;
		double num5 = (1.0 - num3) / num4 + num3 * 0.5;
		double num6 = Math.Floor(Math.Log((0.0 - normalizedTime * num5) * (1.0 - num2) + 1.0, num2));
		double y = num6 + 1.0;
		double num7 = (1.0 - Math.Pow(num2, num6)) / (num4 * num5);
		double num8 = (1.0 - Math.Pow(num2, y)) / (num4 * num5);
		double num9 = (num7 + num8) * 0.5;
		double num10 = normalizedTime - num9;
		double num11 = num9 - num7;
		return (0.0 - Math.Pow(1.0 / num2, num - num6)) / (num11 * num11) * (num10 - num11) * (num10 + num11);
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Freezable" /> derived class. When creating a derived class, you must override this method.</summary>
	/// <returns>The new instance.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new BounceEase();
	}
}
