using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Security.Permissions;

namespace System.Threading;

/// <summary>Provides the basic functionality for propagating a synchronization context in various synchronization models. </summary>
/// <filterpriority>2</filterpriority>
[SecurityPermission(SecurityAction.InheritanceDemand, Flags = (SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy))]
public class SynchronizationContext
{
	private delegate int WaitDelegate(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout);

	private SynchronizationContextProperties _props;

	private static Type s_cachedPreparedType1;

	private static Type s_cachedPreparedType2;

	private static Type s_cachedPreparedType3;

	private static Type s_cachedPreparedType4;

	private static Type s_cachedPreparedType5;

	/// <summary>Gets the synchronization context for the current thread.</summary>
	/// <returns>A <see cref="T:System.Threading.SynchronizationContext" /> object representing the current synchronization context.</returns>
	/// <filterpriority>1</filterpriority>
	public static SynchronizationContext Current => Thread.CurrentThread.GetExecutionContextReader().SynchronizationContext ?? GetThreadLocalContext();

	internal static SynchronizationContext CurrentNoFlow
	{
		[FriendAccessAllowed]
		get
		{
			return Thread.CurrentThread.GetExecutionContextReader().SynchronizationContextNoFlow ?? GetThreadLocalContext();
		}
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Threading.SynchronizationContext" /> class.</summary>
	public SynchronizationContext()
	{
	}

	/// <summary>Sets notification that wait notification is required and prepares the callback method so it can be called more reliably when a wait occurs.</summary>
	[SecuritySafeCritical]
	protected void SetWaitNotificationRequired()
	{
		Type type = GetType();
		if (s_cachedPreparedType1 != type && s_cachedPreparedType2 != type && s_cachedPreparedType3 != type && s_cachedPreparedType4 != type && s_cachedPreparedType5 != type)
		{
			RuntimeHelpers.PrepareDelegate(new WaitDelegate(Wait));
			if (s_cachedPreparedType1 == null)
			{
				s_cachedPreparedType1 = type;
			}
			else if (s_cachedPreparedType2 == null)
			{
				s_cachedPreparedType2 = type;
			}
			else if (s_cachedPreparedType3 == null)
			{
				s_cachedPreparedType3 = type;
			}
			else if (s_cachedPreparedType4 == null)
			{
				s_cachedPreparedType4 = type;
			}
			else if (s_cachedPreparedType5 == null)
			{
				s_cachedPreparedType5 = type;
			}
		}
		_props |= SynchronizationContextProperties.RequireWaitNotification;
	}

	/// <summary>Determines if wait notification is required.</summary>
	/// <returns>true if wait notification is required; otherwise, false. </returns>
	public bool IsWaitNotificationRequired()
	{
		return (_props & SynchronizationContextProperties.RequireWaitNotification) != 0;
	}

	/// <summary>When overridden in a derived class, dispatches a synchronous message to a synchronization context.</summary>
	/// <param name="d">The <see cref="T:System.Threading.SendOrPostCallback" /> delegate to call.</param>
	/// <param name="state">The object passed to the delegate. </param>
	/// <exception cref="T:System.NotSupportedException">The method was called in a Windows Store app. The implementation of <see cref="T:System.Threading.SynchronizationContext" /> for Windows Store apps does not support the <see cref="M:System.Threading.SynchronizationContext.Send(System.Threading.SendOrPostCallback,System.Object)" /> method. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual void Send(SendOrPostCallback d, object state)
	{
		d(state);
	}

	/// <summary>When overridden in a derived class, dispatches an asynchronous message to a synchronization context.</summary>
	/// <param name="d">The <see cref="T:System.Threading.SendOrPostCallback" /> delegate to call.</param>
	/// <param name="state">The object passed to the delegate.</param>
	/// <filterpriority>2</filterpriority>
	public virtual void Post(SendOrPostCallback d, object state)
	{
		ThreadPool.QueueUserWorkItem(d.Invoke, state);
	}

	/// <summary>When overridden in a derived class, responds to the notification that an operation has started.</summary>
	public virtual void OperationStarted()
	{
	}

	/// <summary>When overridden in a derived class, responds to the notification that an operation has completed.</summary>
	public virtual void OperationCompleted()
	{
	}

	/// <summary>Waits for any or all the elements in the specified array to receive a signal.</summary>
	/// <returns>The array index of the object that satisfied the wait.</returns>
	/// <param name="waitHandles">An array of type <see cref="T:System.IntPtr" /> that contains the native operating system handles.</param>
	/// <param name="waitAll">true to wait for all handles; false to wait for any handle. </param>
	/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" /> (-1) to wait indefinitely.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="waitHandles" /> is null.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	[CLSCompliant(false)]
	[PrePrepareMethod]
	[SecurityCritical]
	public virtual int Wait(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
	{
		if (waitHandles == null)
		{
			throw new ArgumentNullException("waitHandles");
		}
		return WaitHelper(waitHandles, waitAll, millisecondsTimeout);
	}

	/// <summary>Helper function that waits for any or all the elements in the specified array to receive a signal.</summary>
	/// <returns>The array index of the object that satisfied the wait.</returns>
	/// <param name="waitHandles">An array of type <see cref="T:System.IntPtr" /> that contains the native operating system handles.</param>
	/// <param name="waitAll">true to wait for all handles;  false to wait for any handle. </param>
	/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" /> (-1) to wait indefinitely.</param>
	[CLSCompliant(false)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	[PrePrepareMethod]
	[SecurityCritical]
	protected static int WaitHelper(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sets the current synchronization context.</summary>
	/// <param name="syncContext">The <see cref="T:System.Threading.SynchronizationContext" /> object to be set.</param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	[SecurityCritical]
	public static void SetSynchronizationContext(SynchronizationContext syncContext)
	{
		ExecutionContext mutableExecutionContext = Thread.CurrentThread.GetMutableExecutionContext();
		mutableExecutionContext.SynchronizationContext = syncContext;
		mutableExecutionContext.SynchronizationContextNoFlow = syncContext;
	}

	private static SynchronizationContext GetThreadLocalContext()
	{
		return null;
	}

	/// <summary>When overridden in a derived class, creates a copy of the synchronization context.  </summary>
	/// <returns>A new <see cref="T:System.Threading.SynchronizationContext" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual SynchronizationContext CreateCopy()
	{
		return new SynchronizationContext();
	}

	[SecurityCritical]
	private static int InvokeWaitMethodHelper(SynchronizationContext syncContext, IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
	{
		return syncContext.Wait(waitHandles, waitAll, millisecondsTimeout);
	}
}
