namespace System.Windows.Controls.Primitives;

/// <summary>Provides information about the <see cref="E:System.Windows.Controls.Primitives.Thumb.DragDelta" /> event that occurs one or more times when a user drags a <see cref="T:System.Windows.Controls.Primitives.Thumb" /> control with the mouse.</summary>
public class DragDeltaEventArgs : RoutedEventArgs
{
	private double _horizontalChange;

	private double _verticalChange;

	/// <summary>Gets the horizontal distance that the mouse has moved since the previous <see cref="E:System.Windows.Controls.Primitives.Thumb.DragDelta" /> event when the user drags the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> control with the mouse.</summary>
	/// <returns>A horizontal change in position of the mouse during a drag operation. There is no default value.</returns>
	public double HorizontalChange => _horizontalChange;

	/// <summary>Gets the vertical distance that the mouse has moved since the previous <see cref="E:System.Windows.Controls.Primitives.Thumb.DragDelta" /> event when the user drags the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> with the mouse.</summary>
	/// <returns>A vertical change in position of the mouse during a drag operation. There is no default value.</returns>
	public double VerticalChange => _verticalChange;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.DragDeltaEventArgs" /> class.</summary>
	/// <param name="horizontalChange">The horizontal change in the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> position since the last <see cref="E:System.Windows.Controls.Primitives.Thumb.DragDelta" /> event.</param>
	/// <param name="verticalChange">The vertical change in the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> position since the last <see cref="E:System.Windows.Controls.Primitives.Thumb.DragDelta" /> event.</param>
	public DragDeltaEventArgs(double horizontalChange, double verticalChange)
	{
		_horizontalChange = horizontalChange;
		_verticalChange = verticalChange;
		base.RoutedEvent = Thumb.DragDeltaEvent;
	}

	/// <summary>Converts a method that handles the <see cref="E:System.Windows.Controls.Primitives.Thumb.DragDelta" /> event to the <see cref="T:System.Windows.Controls.Primitives.DragDeltaEventHandler" /> type.</summary>
	/// <param name="genericHandler">The event handler delegate.</param>
	/// <param name="genericTarget">The <see cref="T:System.Windows.Controls.Primitives.Thumb" /> that uses the handler.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((DragDeltaEventHandler)genericHandler)(genericTarget, this);
	}
}
