namespace System.Windows.Input;

/// <summary>Provides the position of input that is needed to create a manipulation. </summary>
public interface IManipulator
{
	/// <summary>Gets or sets a unique identifier for the object.</summary>
	/// <returns>A unique identifier for the object.</returns>
	int Id { get; }

	/// <summary>Occurs when the <see cref="T:System.Windows.Input.IManipulator" /> object changes position.</summary>
	event EventHandler Updated;

	/// <summary>Returns the position of the <see cref="T:System.Windows.Input.IManipulator" /> object.</summary>
	/// <returns>The position of the <see cref="T:System.Windows.Input.IManipulator" /> object.</returns>
	/// <param name="relativeTo">The element to use as the frame of reference for calculating the position of the <see cref="T:System.Windows.Input.IManipulator" />.</param>
	Point GetPosition(IInputElement relativeTo);

	/// <summary>Called when the manipulation ends.</summary>
	/// <param name="cancel">true if the manipulation is canceled; otherwise, false.</param>
	void ManipulationEnded(bool cancel);
}
