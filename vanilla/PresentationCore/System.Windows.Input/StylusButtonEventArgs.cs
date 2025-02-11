namespace System.Windows.Input;

/// <summary>Provides data for the <see cref="E:System.Windows.UIElement.StylusButtonDown" /> and <see cref="E:System.Windows.UIElement.StylusButtonUp" /> events. </summary>
public class StylusButtonEventArgs : StylusEventArgs
{
	private StylusButton _button;

	/// <summary>Gets the <see cref="T:System.Windows.Input.StylusButton" /> that raises this event.</summary>
	/// <returns>The <see cref="T:System.Windows.Input.StylusButton" /> that raises this event.</returns>
	public StylusButton StylusButton => _button;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.StylusButtonEventArgs" /> class. </summary>
	/// <param name="stylusDevice">The <see cref="T:System.Windows.Input.StylusDevice" /> to associate with this event.</param>
	/// <param name="timestamp">The time when the event occurs.</param>
	/// <param name="button">The <see cref="T:System.Windows.Input.StylusButton" /> that raises the event.</param>
	public StylusButtonEventArgs(StylusDevice stylusDevice, int timestamp, StylusButton button)
		: base(stylusDevice, timestamp)
	{
		_button = button;
	}

	/// <summary>Invokes event handlers in a type-specific way, which can increase event system efficiency.</summary>
	/// <param name="genericHandler">The generic handler to call in a type-specific way.</param>
	/// <param name="genericTarget">The target to call the handler on.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((StylusButtonEventHandler)genericHandler)(genericTarget, this);
	}
}
