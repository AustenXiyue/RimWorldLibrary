using System.Windows.Threading;

namespace System.Windows.Interop;

/// <summary>Defines the interaction between Windows Presentation Foundation (WPF) applications that are hosting interoperation content, and a host supplied progress page. </summary>
public interface IProgressPage
{
	/// <summary>Gets or sets the <see cref="T:System.Uri" /> path to the application deployment manifest.</summary>
	/// <returns>The application deployment manifest path.</returns>
	Uri DeploymentPath { get; set; }

	/// <summary>Gets or sets a reference to a <see cref="T:System.Windows.Threading.DispatcherOperationCallback" /> handler, that can handle the case of a user-initiated Stop command.</summary>
	/// <returns>The callback reference.</returns>
	DispatcherOperationCallback StopCallback { get; set; }

	/// <summary>Gets or sets a reference to a <see cref="T:System.Windows.Threading.DispatcherOperationCallback" /> handler, that can handle the case of a user-initiated Refresh command.</summary>
	/// <returns>The callback reference.</returns>
	DispatcherOperationCallback RefreshCallback { get; set; }

	/// <summary>Gets or sets  the application's name.</summary>
	/// <returns>Name of the application that originates the progress page.</returns>
	string ApplicationName { get; set; }

	/// <summary>Gets or sets the application's publisher.</summary>
	/// <returns>The publisher identifying string.</returns>
	string PublisherName { get; set; }

	/// <summary>Provides upload progress numeric information that can be used to update the progress indicators.</summary>
	/// <param name="bytesDownloaded">Total bytes downloaded thus far.</param>
	/// <param name="bytesTotal">Total bytes that need to be downloaded for the application.</param>
	void UpdateProgress(long bytesDownloaded, long bytesTotal);
}
