using System;
using System.Security.Permissions;

namespace Microsoft.Win32;

/// <summary>Provides data for the <see cref="E:Microsoft.Win32.SystemEvents.SessionSwitch" /> event.</summary>
[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
public class SessionSwitchEventArgs : EventArgs
{
	private SessionSwitchReason reason;

	/// <summary>Gets an identifier that indicates the type of session change event.</summary>
	/// <returns>A <see cref="T:Microsoft.Win32.SessionSwitchReason" /> indicating the type of the session change event.</returns>
	public SessionSwitchReason Reason => reason;

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.Win32.SessionSwitchEventArgs" /> class using the specified session change event type identifer.</summary>
	/// <param name="reason">A <see cref="T:Microsoft.Win32.SessionSwitchReason" /> that indicates the type of session change event. </param>
	public SessionSwitchEventArgs(SessionSwitchReason reason)
	{
		this.reason = reason;
	}
}
