namespace System.Windows.Media.Animation;

/// <summary>Represents an easing function that creates an animation that accelerates and/or decelerates using a circular function.</summary>
public class CircleEase : EasingFunctionBase
{
	/// <summary>Provides the logic portion of the easing function that you can override to produce the <see cref="F:System.Windows.Media.Animation.EasingMode.EaseIn" /> mode of the custom easing function.</summary>
	/// <returns>A double that represents the transformed progress.</returns>
	/// <param name="normalizedTime"> Normalized time (progress) of the animation.</param>
	protected override double EaseInCore(double normalizedTime)
	{
		normalizedTime = Math.Max(0.0, Math.Min(1.0, normalizedTime));
		return 1.0 - Math.Sqrt(1.0 - normalizedTime * normalizedTime);
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Freezable" /> derived class. When creating a derived class, you must override this method.</summary>
	/// <returns>The new instance.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new CircleEase();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.CircleEase" /> class. </summary>
	public CircleEase()
	{
	}
}
