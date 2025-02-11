namespace System.Windows.Input;

/// <summary>Provides data for the <see cref="E:System.Windows.UIElement.StylusDown" /> event. </summary>
public class StylusDownEventArgs : StylusEventArgs
{
	/// <summary>Gets the number of times the tablet pen was tapped.</summary>
	/// <returns>The number of times the tablet pen was tapped.</returns>
	public int TapCount => base.StylusDeviceImpl.TapCount;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.StylusDownEventArgs" /> class. </summary>
	/// <param name="stylusDevice">Device instance that the event is associated with.</param>
	/// <param name="timestamp">A timestamp used to disambiguate instances of the event.</param>
	public StylusDownEventArgs(StylusDevice stylusDevice, int timestamp)
		: base(stylusDevice, timestamp)
	{
	}

	/// <summary>Invokes event handlers in a type-specific way, which can increase event system efficiency.</summary>
	/// <param name="genericHandler">The generic handler to call in a type-specific way.</param>
	/// <param name="genericTarget">The target to call the handler on.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((StylusDownEventHandler)genericHandler)(genericTarget, this);
	}
}
