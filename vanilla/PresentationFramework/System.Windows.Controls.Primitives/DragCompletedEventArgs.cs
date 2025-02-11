namespace System.Windows.Controls.Primitives;

/// <summary>Provides information about the <see cref="E:System.Windows.Controls.Primitives.Thumb.DragCompleted" /> event that occurs when a user completes a drag operation with the mouse of a <see cref="T:System.Windows.Controls.Primitives.Thumb" /> control. </summary>
public class DragCompletedEventArgs : RoutedEventArgs
{
	private double _horizontalChange;

	private double _verticalChange;

	private bool _wasCanceled;

	/// <summary>Gets the horizontal change in position of the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> after the user drags the control with the mouse. </summary>
	/// <returns>The horizontal difference between the point at which the user pressed the left mouse button and the point at which the user released the button during a drag operation of a <see cref="T:System.Windows.Controls.Primitives.Thumb" /> control. There is no default value.</returns>
	public double HorizontalChange => _horizontalChange;

	/// <summary>Gets the vertical change in position of the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> after the user drags the control with the mouse.</summary>
	/// <returns>The vertical difference between the point at which the user pressed the left mouse button and the point at which the user released the button during a drag operation of a <see cref="T:System.Windows.Controls.Primitives.Thumb" /> control. There is no default value.</returns>
	public double VerticalChange => _verticalChange;

	/// <summary>Gets whether the drag operation for a <see cref="T:System.Windows.Controls.Primitives.Thumb" /> was canceled by a call to the <see cref="M:System.Windows.Controls.Primitives.Thumb.CancelDrag" /> method. </summary>
	/// <returns>true if a drag operation was canceled; otherwise, false.</returns>
	public bool Canceled => _wasCanceled;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.DragCompletedEventArgs" /> class. </summary>
	/// <param name="horizontalChange">The horizontal change in position of the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> control, resulting from the drag operation.</param>
	/// <param name="verticalChange">The vertical change in position of the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> control, resulting from the drag operation.</param>
	/// <param name="canceled">A Boolean value that indicates whether the drag operation was canceled by a call to the <see cref="M:System.Windows.Controls.Primitives.Thumb.CancelDrag" /> method.</param>
	public DragCompletedEventArgs(double horizontalChange, double verticalChange, bool canceled)
	{
		_horizontalChange = horizontalChange;
		_verticalChange = verticalChange;
		_wasCanceled = canceled;
		base.RoutedEvent = Thumb.DragCompletedEvent;
	}

	/// <summary>Converts a method that handles the <see cref="E:System.Windows.Controls.Primitives.Thumb.DragCompleted" /> event to the <see cref="T:System.Windows.Controls.Primitives.DragCompletedEventHandler" /> type.</summary>
	/// <param name="genericHandler">The event handler delegate.</param>
	/// <param name="genericTarget">The <see cref="T:System.Windows.Controls.Primitives.Thumb" /> that uses the handler.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((DragCompletedEventHandler)genericHandler)(genericTarget, this);
	}
}
