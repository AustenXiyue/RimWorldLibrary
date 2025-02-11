namespace System.Windows.Controls.Primitives;

/// <summary>Provides information about the <see cref="E:System.Windows.Controls.Primitives.Thumb.DragStarted" /> event that occurs when a user drags a <see cref="T:System.Windows.Controls.Primitives.Thumb" /> control with the mouse..</summary>
public class DragStartedEventArgs : RoutedEventArgs
{
	private double _horizontalOffset;

	private double _verticalOffset;

	/// <summary>Gets the horizontal offset of the mouse click relative to the screen coordinates of the <see cref="T:System.Windows.Controls.Primitives.Thumb" />.</summary>
	/// <returns>The horizontal offset of the mouse click with respect to the upper-left corner of the bounding box of the <see cref="T:System.Windows.Controls.Primitives.Thumb" />. There is no default value.</returns>
	public double HorizontalOffset => _horizontalOffset;

	/// <summary>Gets the vertical offset of the mouse click relative to the screen coordinates of the <see cref="T:System.Windows.Controls.Primitives.Thumb" />.</summary>
	/// <returns>The horizontal offset of the mouse click with respect to the upper-left corner of the bounding box of the <see cref="T:System.Windows.Controls.Primitives.Thumb" />. There is no default value.</returns>
	public double VerticalOffset => _verticalOffset;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.DragStartedEventArgs" /> class.</summary>
	/// <param name="horizontalOffset">The horizontal offset of the mouse click with respect to the screen coordinates of the <see cref="T:System.Windows.Controls.Primitives.Thumb" />.</param>
	/// <param name="verticalOffset">The vertical offset of the mouse click with respect to the screen coordinates of the <see cref="T:System.Windows.Controls.Primitives.Thumb" />.</param>
	public DragStartedEventArgs(double horizontalOffset, double verticalOffset)
	{
		_horizontalOffset = horizontalOffset;
		_verticalOffset = verticalOffset;
		base.RoutedEvent = Thumb.DragStartedEvent;
	}

	/// <summary>Converts a method that handles the <see cref="E:System.Windows.Controls.Primitives.Thumb.DragStarted" /> event to the <see cref="T:System.Windows.Controls.Primitives.DragStartedEventHandler" /> type.</summary>
	/// <param name="genericHandler">The event handler delegate.</param>
	/// <param name="genericTarget">The <see cref="T:System.Windows.Controls.Primitives.Thumb" /> that uses the handler.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((DragStartedEventHandler)genericHandler)(genericTarget, this);
	}
}
