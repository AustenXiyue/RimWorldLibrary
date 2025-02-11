using System.Reflection;
using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Provides data for the MethodNameCompleted event.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class AsyncCompletedEventArgs : EventArgs
{
	private readonly Exception error;

	private readonly bool cancelled;

	private readonly object userState;

	/// <summary>Gets a value indicating whether an asynchronous operation has been canceled.</summary>
	/// <returns>true if the background operation has been canceled; otherwise false. The default is false.</returns>
	[SRDescription("True if operation was cancelled.")]
	public bool Cancelled => cancelled;

	/// <summary>Gets a value indicating which error occurred during an asynchronous operation.</summary>
	/// <returns>An <see cref="T:System.Exception" /> instance, if an error occurred during an asynchronous operation; otherwise null.</returns>
	[SRDescription("Exception that occurred during operation.  Null if no error.")]
	public Exception Error => error;

	/// <summary>Gets the unique identifier for the asynchronous task.</summary>
	/// <returns>An object reference that uniquely identifies the asynchronous task; otherwise, null if no value has been set.</returns>
	[SRDescription("User-supplied state to identify operation.")]
	public object UserState => userState;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.AsyncCompletedEventArgs" /> class.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
	public AsyncCompletedEventArgs()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.AsyncCompletedEventArgs" /> class. </summary>
	/// <param name="error">Any error that occurred during the asynchronous operation.</param>
	/// <param name="cancelled">A value indicating whether the asynchronous operation was canceled.</param>
	/// <param name="userState">The optional user-supplied state object passed to the <see cref="M:System.ComponentModel.BackgroundWorker.RunWorkerAsync(System.Object)" /> method.</param>
	public AsyncCompletedEventArgs(Exception error, bool cancelled, object userState)
	{
		this.error = error;
		this.cancelled = cancelled;
		this.userState = userState;
	}

	/// <summary>Raises a user-supplied exception if an asynchronous operation failed.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.ComponentModel.AsyncCompletedEventArgs.Cancelled" /> property is true. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">The <see cref="P:System.ComponentModel.AsyncCompletedEventArgs.Error" /> property has been set by the asynchronous operation. The <see cref="P:System.Exception.InnerException" /> property holds a reference to <see cref="P:System.ComponentModel.AsyncCompletedEventArgs.Error" />. </exception>
	protected void RaiseExceptionIfNecessary()
	{
		if (Error != null)
		{
			throw new TargetInvocationException(global::SR.GetString("An exception occurred during the operation, making the result invalid.  Check InnerException for exception details."), Error);
		}
		if (Cancelled)
		{
			throw new InvalidOperationException(global::SR.GetString("Operation has been cancelled."));
		}
	}
}
