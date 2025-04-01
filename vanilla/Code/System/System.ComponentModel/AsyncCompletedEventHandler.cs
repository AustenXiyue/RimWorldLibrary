using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Represents the method that will handle the MethodNameCompleted event of an asynchronous operation.</summary>
/// <param name="sender">The source of the event. </param>
/// <param name="e">An <see cref="T:System.ComponentModel.AsyncCompletedEventArgs" /> that contains the event data. </param>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public delegate void AsyncCompletedEventHandler(object sender, AsyncCompletedEventArgs e);
