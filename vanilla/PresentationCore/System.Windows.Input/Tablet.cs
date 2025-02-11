namespace System.Windows.Input;

/// <summary>Provides access to static methods that return the tablet devices attached to the system. </summary>
public static class Tablet
{
	/// <summary>Gets the current <see cref="T:System.Windows.Input.TabletDevice" />.</summary>
	/// <returns>The current <see cref="T:System.Windows.Input.TabletDevice" />.</returns>
	public static TabletDevice CurrentTabletDevice => Stylus.CurrentStylusDevice?.TabletDevice;

	/// <summary>Gets the <see cref="T:System.Windows.Input.TabletDeviceCollection" /> associated with the system.</summary>
	/// <returns>The <see cref="T:System.Windows.Input.TabletDeviceCollection" /> associated with the Tablet PC.</returns>
	public static TabletDeviceCollection TabletDevices => StylusLogic.CurrentStylusLogic?.TabletDevices ?? TabletDeviceCollection.EmptyTabletDeviceCollection;
}
