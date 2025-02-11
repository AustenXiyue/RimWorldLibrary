namespace System.Windows.Input;

/// <summary>Abstract class that describes input device gestures.</summary>
public abstract class InputGesture
{
	/// <summary>When overridden in a derived class, determines whether the specified <see cref="T:System.Windows.Input.InputGesture" /> matches the input associated with the specified <see cref="T:System.Windows.Input.InputEventArgs" /> object.</summary>
	/// <returns>true if the gesture matches the input; otherwise, false.</returns>
	/// <param name="targetElement">The target of the command.</param>
	/// <param name="inputEventArgs">The input event data to compare this gesture to.</param>
	public abstract bool Matches(object targetElement, InputEventArgs inputEventArgs);

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.InputGesture" /> class.</summary>
	protected InputGesture()
	{
	}
}
