using System;
using System.Security.Permissions;

namespace Microsoft.Win32;

/// <summary>Provides data for the <see cref="E:Microsoft.Win32.SystemEvents.UserPreferenceChanging" /> event.</summary>
[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
public class UserPreferenceChangingEventArgs : EventArgs
{
	private UserPreferenceCategory mycategory;

	/// <summary>Gets the category of user preferences that is changing.</summary>
	/// <returns>One of the <see cref="T:Microsoft.Win32.UserPreferenceCategory" /> values that indicates the category of user preferences that is changing.</returns>
	public UserPreferenceCategory Category => mycategory;

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.Win32.UserPreferenceChangingEventArgs" /> class using the specified user preference category identifier.</summary>
	/// <param name="category">One of the <see cref="T:Microsoft.Win32.UserPreferenceCategory" /> values that indicate the user preference category that is changing. </param>
	public UserPreferenceChangingEventArgs(UserPreferenceCategory category)
	{
		mycategory = category;
	}
}
