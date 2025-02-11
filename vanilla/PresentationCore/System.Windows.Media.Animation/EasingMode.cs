namespace System.Windows.Media.Animation;

/// <summary>Defines the modes in which classes derived from <see cref="T:System.Windows.Media.Animation.EasingFunctionBase" /> perform their easing.</summary>
public enum EasingMode
{
	/// <summary>Interpolation follows the mathematical formula associated with the easing function.</summary>
	EaseIn,
	/// <summary>Interpolation follows 100% interpolation minus the output of the formula associated with the easing function.</summary>
	EaseOut,
	/// <summary>Interpolation uses <see cref="F:System.Windows.Media.Animation.EasingMode.EaseIn" /> for the first half of the animation and <see cref="F:System.Windows.Media.Animation.EasingMode.EaseOut" /> for the second half.</summary>
	EaseInOut
}
