namespace System.Windows.Media.Animation;

/// <summary>Defines the basic functionality of an easing function.</summary>
public interface IEasingFunction
{
	/// <summary>Transforms normalized time to control the pace of an animation.</summary>
	/// <returns>The transformed progress.</returns>
	/// <param name="normalizedTime">Normalized time (progress) of the animation.</param>
	double Ease(double normalizedTime);
}
