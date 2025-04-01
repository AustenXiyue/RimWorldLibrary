using System;
using System.Security.Permissions;

namespace Microsoft.Win32;

/// <summary>Provides data for the <see cref="E:Microsoft.Win32.SystemEvents.SessionEnded" /> event.</summary>
[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
public class SessionEndedEventArgs : EventArgs
{
	private SessionEndReasons myreason;

	/// <summary>Gets an identifier that indicates how the session ended.</summary>
	/// <returns>One of the <see cref="T:Microsoft.Win32.SessionEndReasons" /> values that indicates how the session ended.</returns>
	public SessionEndReasons Reason => myreason;

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.Win32.SessionEndedEventArgs" /> class.</summary>
	/// <param name="reason">One of the <see cref="T:Microsoft.Win32.SessionEndReasons" /> values indicating how the session ended. </param>
	public SessionEndedEventArgs(SessionEndReasons reason)
	{
		myreason = reason;
	}
}
