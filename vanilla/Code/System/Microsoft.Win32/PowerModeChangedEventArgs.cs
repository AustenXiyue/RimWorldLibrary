using System;
using System.Security.Permissions;

namespace Microsoft.Win32;

/// <summary>Provides data for the <see cref="E:Microsoft.Win32.SystemEvents.PowerModeChanged" /> event.</summary>
[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
public class PowerModeChangedEventArgs : EventArgs
{
	private PowerModes mymode;

	/// <summary>Gets an identifier that indicates the type of the power mode event that has occurred.</summary>
	/// <returns>One of the <see cref="T:Microsoft.Win32.PowerModes" /> values.</returns>
	public PowerModes Mode => mymode;

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.Win32.PowerModeChangedEventArgs" /> class using the specified power mode event type.</summary>
	/// <param name="mode">One of the <see cref="T:Microsoft.Win32.PowerModes" /> values that represents the type of power mode event. </param>
	public PowerModeChangedEventArgs(PowerModes mode)
	{
		mymode = mode;
	}
}
