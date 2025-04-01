using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Provides data for the <see cref="E:System.ComponentModel.BackgroundWorker.ProgressChanged" /> event.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class ProgressChangedEventArgs : EventArgs
{
	private readonly int progressPercentage;

	private readonly object userState;

	/// <summary>Gets the asynchronous task progress percentage.</summary>
	/// <returns>A percentage value indicating the asynchronous task progress.</returns>
	[SRDescription("Percentage progress made in operation.")]
	public int ProgressPercentage => progressPercentage;

	/// <summary>Gets a unique user state.</summary>
	/// <returns>A unique <see cref="T:System.Object" /> indicating the user state.</returns>
	[SRDescription("User-supplied state to identify operation.")]
	public object UserState => userState;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.ProgressChangedEventArgs" /> class.</summary>
	/// <param name="progressPercentage">The percentage of an asynchronous task that has been completed.</param>
	/// <param name="userState">A unique user state.</param>
	public ProgressChangedEventArgs(int progressPercentage, object userState)
	{
		this.progressPercentage = progressPercentage;
		this.userState = userState;
	}
}
