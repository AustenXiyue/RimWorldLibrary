namespace System.Windows;

/// <summary>Provides data for the <see cref="E:System.Windows.Interop.HwndSource.AutoResized" /> event raised by <see cref="T:System.Windows.Interop.HwndSource" />.</summary>
public class AutoResizedEventArgs : EventArgs
{
	private Size _size;

	/// <summary>Gets the new size of the window after the auto resize operation.</summary>
	/// <returns>Size of the window after resizing.</returns>
	public Size Size => _size;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.AutoResizedEventArgs" /> class.</summary>
	/// <param name="size">The size to report in the event data.</param>
	public AutoResizedEventArgs(Size size)
	{
		_size = size;
	}
}
