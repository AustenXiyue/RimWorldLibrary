using System.Security.Permissions;
using System.Threading;
using Unity;

namespace System.ComponentModel;

/// <summary>Tracks the lifetime of an asynchronous operation.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public sealed class AsyncOperation
{
	private SynchronizationContext syncContext;

	private object userSuppliedState;

	private bool alreadyCompleted;

	/// <summary>Gets or sets an object used to uniquely identify an asynchronous operation.</summary>
	/// <returns>The state object passed to the asynchronous method invocation.</returns>
	public object UserSuppliedState => userSuppliedState;

	/// <summary>Gets the <see cref="T:System.Threading.SynchronizationContext" /> object that was passed to the constructor.</summary>
	/// <returns>The <see cref="T:System.Threading.SynchronizationContext" /> object that was passed to the constructor.</returns>
	public SynchronizationContext SynchronizationContext => syncContext;

	private AsyncOperation(object userSuppliedState, SynchronizationContext syncContext)
	{
		this.userSuppliedState = userSuppliedState;
		this.syncContext = syncContext;
		alreadyCompleted = false;
		this.syncContext.OperationStarted();
	}

	~AsyncOperation()
	{
		if (!alreadyCompleted && syncContext != null)
		{
			syncContext.OperationCompleted();
		}
	}

	/// <summary>Invokes a delegate on the thread or context appropriate for the application model.</summary>
	/// <param name="d">A <see cref="T:System.Threading.SendOrPostCallback" /> object that wraps the delegate to be called when the operation ends. </param>
	/// <param name="arg">An argument for the delegate contained in the <paramref name="d" /> parameter. </param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="M:System.ComponentModel.AsyncOperation.PostOperationCompleted(System.Threading.SendOrPostCallback,System.Object)" /> method has been called previously for this task. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="d" /> is null. </exception>
	public void Post(SendOrPostCallback d, object arg)
	{
		VerifyNotCompleted();
		VerifyDelegateNotNull(d);
		syncContext.Post(d, arg);
	}

	/// <summary>Ends the lifetime of an asynchronous operation.</summary>
	/// <param name="d">A <see cref="T:System.Threading.SendOrPostCallback" /> object that wraps the delegate to be called when the operation ends. </param>
	/// <param name="arg">An argument for the delegate contained in the <paramref name="d" /> parameter. </param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.ComponentModel.AsyncOperation.OperationCompleted" /> has been called previously for this task. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="d" /> is null. </exception>
	public void PostOperationCompleted(SendOrPostCallback d, object arg)
	{
		Post(d, arg);
		OperationCompletedCore();
	}

	/// <summary>Ends the lifetime of an asynchronous operation.</summary>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.ComponentModel.AsyncOperation.OperationCompleted" /> has been called previously for this task. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void OperationCompleted()
	{
		VerifyNotCompleted();
		OperationCompletedCore();
	}

	private void OperationCompletedCore()
	{
		try
		{
			syncContext.OperationCompleted();
		}
		finally
		{
			alreadyCompleted = true;
			GC.SuppressFinalize(this);
		}
	}

	private void VerifyNotCompleted()
	{
		if (alreadyCompleted)
		{
			throw new InvalidOperationException(global::SR.GetString("This operation has already had OperationCompleted called on it and further calls are illegal."));
		}
	}

	private void VerifyDelegateNotNull(SendOrPostCallback d)
	{
		if (d == null)
		{
			throw new ArgumentNullException(global::SR.GetString("A non-null SendOrPostCallback must be supplied."), "d");
		}
	}

	internal static AsyncOperation CreateOperation(object userSuppliedState, SynchronizationContext syncContext)
	{
		return new AsyncOperation(userSuppliedState, syncContext);
	}

	internal AsyncOperation()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
