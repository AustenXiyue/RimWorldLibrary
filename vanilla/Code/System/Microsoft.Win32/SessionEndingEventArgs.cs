using System;
using System.Security.Permissions;

namespace Microsoft.Win32;

/// <summary>Provides data for the <see cref="E:Microsoft.Win32.SystemEvents.SessionEnding" /> event.</summary>
[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
public class SessionEndingEventArgs : EventArgs
{
	private SessionEndReasons myreason;

	private bool mycancel;

	/// <summary>Gets the reason the session is ending.</summary>
	/// <returns>One of the <see cref="T:Microsoft.Win32.SessionEndReasons" /> values that specifies how the session is ending.</returns>
	public SessionEndReasons Reason => myreason;

	/// <summary>Gets or sets a value indicating whether to cancel the user request to end the session.</summary>
	/// <returns>true to cancel the user request to end the session; otherwise, false.</returns>
	public bool Cancel
	{
		get
		{
			return mycancel;
		}
		set
		{
			mycancel = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.Win32.SessionEndingEventArgs" /> class using the specified value indicating the type of session close event that is occurring.</summary>
	/// <param name="reason">One of the <see cref="T:Microsoft.Win32.SessionEndReasons" /> that specifies how the session ends. </param>
	public SessionEndingEventArgs(SessionEndReasons reason)
	{
		myreason = reason;
	}
}
