using System.Runtime.Serialization;

namespace System.Windows.Media.Animation;

/// <summary>The exception that is thrown when an error occurs while animating a property.</summary>
[Serializable]
public sealed class AnimationException : SystemException
{
	[NonSerialized]
	private AnimationClock _clock;

	[NonSerialized]
	private DependencyProperty _property;

	[NonSerialized]
	private IAnimatable _targetElement;

	/// <summary>Gets the clock that generates the animated values. </summary>
	/// <returns>The clock that generates the animated values.</returns>
	public AnimationClock Clock => _clock;

	/// <summary>Gets the animated dependency property.</summary>
	/// <returns>The animated dependency property.</returns>
	public DependencyProperty Property => _property;

	/// <summary>Gets the animated object.</summary>
	/// <returns>The animated object.</returns>
	public IAnimatable Target => _targetElement;

	internal AnimationException(AnimationClock clock, DependencyProperty property, IAnimatable target, string message, Exception innerException)
		: base(message, innerException)
	{
		_clock = clock;
		_property = property;
		_targetElement = target;
	}

	private AnimationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
