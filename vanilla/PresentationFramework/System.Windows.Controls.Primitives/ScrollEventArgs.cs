namespace System.Windows.Controls.Primitives;

/// <summary>Provides data for a <see cref="E:System.Windows.Controls.Primitives.ScrollBar.Scroll" /> event that occurs when the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> of a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> moves. </summary>
public class ScrollEventArgs : RoutedEventArgs
{
	private ScrollEventType _scrollEventType;

	private double _newValue;

	/// <summary>Gets the <see cref="T:System.Windows.Controls.Primitives.ScrollEventType" /> enumeration value that describes the change in the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> position that caused this event.</summary>
	/// <returns>A <see cref="T:System.Windows.Controls.Primitives.ScrollEventType" /> enumeration value that describes the type of <see cref="T:System.Windows.Controls.Primitives.Thumb" /> movement that caused the <see cref="E:System.Windows.Controls.Primitives.ScrollBar.Scroll" /> event.</returns>
	public ScrollEventType ScrollEventType => _scrollEventType;

	/// <summary>Gets a value that represents the new location of the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> in the <see cref="T:System.Windows.Controls.Primitives.ScrollBar" />. </summary>
	/// <returns>The value that corresponds to the new position of the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> in the <see cref="T:System.Windows.Controls.Primitives.ScrollBar" />.</returns>
	public double NewValue => _newValue;

	/// <summary>Initializes an instance of the <see cref="T:System.Windows.Controls.Primitives.ScrollEventArgs" /> class by using the specified <see cref="T:System.Windows.Controls.Primitives.ScrollEventType" /> enumeration value and the new location of the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> control in the <see cref="T:System.Windows.Controls.Primitives.ScrollBar" />. </summary>
	/// <param name="scrollEventType">A <see cref="T:System.Windows.Controls.Primitives.ScrollEventType" /> enumeration value that describes the type of <see cref="T:System.Windows.Controls.Primitives.Thumb" /> movement that caused the event.</param>
	/// <param name="newValue">The value that corresponds to the new location of the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> in the <see cref="T:System.Windows.Controls.Primitives.ScrollBar" />.</param>
	public ScrollEventArgs(ScrollEventType scrollEventType, double newValue)
	{
		_scrollEventType = scrollEventType;
		_newValue = newValue;
		base.RoutedEvent = ScrollBar.ScrollEvent;
	}

	/// <summary>Performs the appropriate type casting to call the type-safe <see cref="T:System.Windows.Controls.Primitives.ScrollEventHandler" /> delegate for the <see cref="E:System.Windows.Controls.Primitives.ScrollBar.Scroll" /> event. </summary>
	/// <param name="genericHandler">The event handler to call.</param>
	/// <param name="genericTarget">The current object along the event's route.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((ScrollEventHandler)genericHandler)(genericTarget, this);
	}
}
