using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace System.Threading;

/// <summary>Provides a pool of threads that can be used to execute tasks, post work items, process asynchronous I/O, wait on behalf of other threads, and process timers.</summary>
/// <filterpriority>2</filterpriority>
[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
public static class ThreadPool
{
	/// <summary>Sets the number of requests to the thread pool that can be active concurrently. All requests above that number remain queued until thread pool threads become available.</summary>
	/// <returns>true if the change is successful; otherwise, false.</returns>
	/// <param name="workerThreads">The maximum number of worker threads in the thread pool. </param>
	/// <param name="completionPortThreads">The maximum number of asynchronous I/O threads in the thread pool. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlThread" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	[SecurityPermission(SecurityAction.Demand, ControlThread = true)]
	public static bool SetMaxThreads(int workerThreads, int completionPortThreads)
	{
		return SetMaxThreadsNative(workerThreads, completionPortThreads);
	}

	/// <summary>Retrieves the number of requests to the thread pool that can be active concurrently. All requests above that number remain queued until thread pool threads become available.</summary>
	/// <param name="workerThreads">The maximum number of worker threads in the thread pool. </param>
	/// <param name="completionPortThreads">The maximum number of asynchronous I/O threads in the thread pool. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static void GetMaxThreads(out int workerThreads, out int completionPortThreads)
	{
		GetMaxThreadsNative(out workerThreads, out completionPortThreads);
	}

	/// <summary>Sets the minimum number of threads the thread pool creates on demand, as new requests are made, before switching to an algorithm for managing thread creation and destruction.</summary>
	/// <returns>true if the change is successful; otherwise, false.</returns>
	/// <param name="workerThreads">The minimum number of worker threads that the thread pool creates on demand. </param>
	/// <param name="completionPortThreads">The minimum number of asynchronous I/O threads that the thread pool creates on demand. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlThread" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	[SecurityPermission(SecurityAction.Demand, ControlThread = true)]
	public static bool SetMinThreads(int workerThreads, int completionPortThreads)
	{
		return SetMinThreadsNative(workerThreads, completionPortThreads);
	}

	/// <summary>Retrieves the minimum number of threads the thread pool creates on demand, as new requests are made, before switching to an algorithm for managing thread creation and destruction.</summary>
	/// <param name="workerThreads">When this method returns, contains the minimum number of worker threads that the thread pool creates on demand. </param>
	/// <param name="completionPortThreads">When this method returns, contains the minimum number of asynchronous I/O threads that the thread pool creates on demand. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static void GetMinThreads(out int workerThreads, out int completionPortThreads)
	{
		GetMinThreadsNative(out workerThreads, out completionPortThreads);
	}

	/// <summary>Retrieves the difference between the maximum number of thread pool threads returned by the <see cref="M:System.Threading.ThreadPool.GetMaxThreads(System.Int32@,System.Int32@)" /> method, and the number currently active.</summary>
	/// <param name="workerThreads">The number of available worker threads. </param>
	/// <param name="completionPortThreads">The number of available asynchronous I/O threads. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static void GetAvailableThreads(out int workerThreads, out int completionPortThreads)
	{
		GetAvailableThreadsNative(out workerThreads, out completionPortThreads);
	}

	/// <summary>Registers a delegate to wait for a <see cref="T:System.Threading.WaitHandle" />, specifying a 32-bit unsigned integer for the time-out in milliseconds.</summary>
	/// <returns>The <see cref="T:System.Threading.RegisteredWaitHandle" /> that can be used to cancel the registered wait operation.</returns>
	/// <param name="waitObject">The <see cref="T:System.Threading.WaitHandle" /> to register. Use a <see cref="T:System.Threading.WaitHandle" /> other than <see cref="T:System.Threading.Mutex" />.</param>
	/// <param name="callBack">The <see cref="T:System.Threading.WaitOrTimerCallback" /> delegate to call when the <paramref name="waitObject" /> parameter is signaled. </param>
	/// <param name="state">The object passed to the delegate. </param>
	/// <param name="millisecondsTimeOutInterval">The time-out in milliseconds. If the <paramref name="millisecondsTimeOutInterval" /> parameter is 0 (zero), the function tests the object's state and returns immediately. If <paramref name="millisecondsTimeOutInterval" /> is -1, the function's time-out interval never elapses. </param>
	/// <param name="executeOnlyOnce">true to indicate that the thread will no longer wait on the <paramref name="waitObject" /> parameter after the delegate has been called; false to indicate that the timer is reset every time the wait operation completes until the wait is unregistered. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="millisecondsTimeOutInterval" /> parameter is less than -1. </exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.NoInlining)]
	[SecuritySafeCritical]
	[CLSCompliant(false)]
	public static RegisteredWaitHandle RegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callBack, object state, uint millisecondsTimeOutInterval, bool executeOnlyOnce)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return RegisterWaitForSingleObject(waitObject, callBack, state, millisecondsTimeOutInterval, executeOnlyOnce, ref stackMark, compressStack: true);
	}

	/// <summary>Registers a delegate to wait for a <see cref="T:System.Threading.WaitHandle" />, specifying a 32-bit unsigned integer for the time-out in milliseconds. This method does not propagate the calling stack to the worker thread.</summary>
	/// <returns>The <see cref="T:System.Threading.RegisteredWaitHandle" /> object that can be used to cancel the registered wait operation.</returns>
	/// <param name="waitObject">The <see cref="T:System.Threading.WaitHandle" /> to register. Use a <see cref="T:System.Threading.WaitHandle" /> other than <see cref="T:System.Threading.Mutex" />.</param>
	/// <param name="callBack">The delegate to call when the <paramref name="waitObject" /> parameter is signaled. </param>
	/// <param name="state">The object that is passed to the delegate. </param>
	/// <param name="millisecondsTimeOutInterval">The time-out in milliseconds. If the <paramref name="millisecondsTimeOutInterval" /> parameter is 0 (zero), the function tests the object's state and returns immediately. If <paramref name="millisecondsTimeOutInterval" /> is -1, the function's time-out interval never elapses. </param>
	/// <param name="executeOnlyOnce">true to indicate that the thread will no longer wait on the <paramref name="waitObject" /> parameter after the delegate has been called; false to indicate that the timer is reset every time the wait operation completes until the wait is unregistered. </param>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	[MethodImpl(MethodImplOptions.NoInlining)]
	[SecurityCritical]
	[CLSCompliant(false)]
	public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callBack, object state, uint millisecondsTimeOutInterval, bool executeOnlyOnce)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return RegisterWaitForSingleObject(waitObject, callBack, state, millisecondsTimeOutInterval, executeOnlyOnce, ref stackMark, compressStack: false);
	}

	[SecurityCritical]
	private static RegisteredWaitHandle RegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callBack, object state, uint millisecondsTimeOutInterval, bool executeOnlyOnce, ref StackCrawlMark stackMark, bool compressStack)
	{
		if (waitObject == null)
		{
			throw new ArgumentNullException("waitObject");
		}
		if (callBack == null)
		{
			throw new ArgumentNullException("callBack");
		}
		if (millisecondsTimeOutInterval != uint.MaxValue && millisecondsTimeOutInterval > int.MaxValue)
		{
			throw new NotSupportedException("Timeout is too big. Maximum is Int32.MaxValue");
		}
		RegisteredWaitHandle registeredWaitHandle = new RegisteredWaitHandle(waitObject, callBack, state, new TimeSpan(0, 0, 0, 0, (int)millisecondsTimeOutInterval), executeOnlyOnce);
		if (compressStack)
		{
			QueueUserWorkItem(registeredWaitHandle.Wait, null);
		}
		else
		{
			UnsafeQueueUserWorkItem(registeredWaitHandle.Wait, null);
		}
		return registeredWaitHandle;
	}

	/// <summary>Registers a delegate to wait for a <see cref="T:System.Threading.WaitHandle" />, specifying a 32-bit signed integer for the time-out in milliseconds.</summary>
	/// <returns>The <see cref="T:System.Threading.RegisteredWaitHandle" /> that encapsulates the native handle.</returns>
	/// <param name="waitObject">The <see cref="T:System.Threading.WaitHandle" /> to register. Use a <see cref="T:System.Threading.WaitHandle" /> other than <see cref="T:System.Threading.Mutex" />.</param>
	/// <param name="callBack">The <see cref="T:System.Threading.WaitOrTimerCallback" /> delegate to call when the <paramref name="waitObject" /> parameter is signaled. </param>
	/// <param name="state">The object that is passed to the delegate. </param>
	/// <param name="millisecondsTimeOutInterval">The time-out in milliseconds. If the <paramref name="millisecondsTimeOutInterval" /> parameter is 0 (zero), the function tests the object's state and returns immediately. If <paramref name="millisecondsTimeOutInterval" /> is -1, the function's time-out interval never elapses. </param>
	/// <param name="executeOnlyOnce">true to indicate that the thread will no longer wait on the <paramref name="waitObject" /> parameter after the delegate has been called; false to indicate that the timer is reset every time the wait operation completes until the wait is unregistered. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="millisecondsTimeOutInterval" /> parameter is less than -1. </exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.NoInlining)]
	[SecuritySafeCritical]
	public static RegisteredWaitHandle RegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callBack, object state, int millisecondsTimeOutInterval, bool executeOnlyOnce)
	{
		if (millisecondsTimeOutInterval < -1)
		{
			throw new ArgumentOutOfRangeException("millisecondsTimeOutInterval", Environment.GetResourceString("Number must be either non-negative and less than or equal to Int32.MaxValue or -1."));
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return RegisterWaitForSingleObject(waitObject, callBack, state, (millisecondsTimeOutInterval == -1) ? uint.MaxValue : ((uint)millisecondsTimeOutInterval), executeOnlyOnce, ref stackMark, compressStack: true);
	}

	/// <summary>Registers a delegate to wait for a <see cref="T:System.Threading.WaitHandle" />, using a 32-bit signed integer for the time-out in milliseconds. This method does not propagate the calling stack to the worker thread.</summary>
	/// <returns>The <see cref="T:System.Threading.RegisteredWaitHandle" /> object that can be used to cancel the registered wait operation.</returns>
	/// <param name="waitObject">The <see cref="T:System.Threading.WaitHandle" /> to register. Use a <see cref="T:System.Threading.WaitHandle" /> other than <see cref="T:System.Threading.Mutex" />.</param>
	/// <param name="callBack">The delegate to call when the <paramref name="waitObject" /> parameter is signaled. </param>
	/// <param name="state">The object that is passed to the delegate. </param>
	/// <param name="millisecondsTimeOutInterval">The time-out in milliseconds. If the <paramref name="millisecondsTimeOutInterval" /> parameter is 0 (zero), the function tests the object's state and returns immediately. If <paramref name="millisecondsTimeOutInterval" /> is -1, the function's time-out interval never elapses. </param>
	/// <param name="executeOnlyOnce">true to indicate that the thread will no longer wait on the <paramref name="waitObject" /> parameter after the delegate has been called; false to indicate that the timer is reset every time the wait operation completes until the wait is unregistered. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="millisecondsTimeOutInterval" /> parameter is less than -1. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	[MethodImpl(MethodImplOptions.NoInlining)]
	[SecurityCritical]
	public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callBack, object state, int millisecondsTimeOutInterval, bool executeOnlyOnce)
	{
		if (millisecondsTimeOutInterval < -1)
		{
			throw new ArgumentOutOfRangeException("millisecondsTimeOutInterval", Environment.GetResourceString("Number must be either non-negative and less than or equal to Int32.MaxValue or -1."));
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return RegisterWaitForSingleObject(waitObject, callBack, state, (millisecondsTimeOutInterval == -1) ? uint.MaxValue : ((uint)millisecondsTimeOutInterval), executeOnlyOnce, ref stackMark, compressStack: false);
	}

	/// <summary>Registers a delegate to wait for a <see cref="T:System.Threading.WaitHandle" />, specifying a 64-bit signed integer for the time-out in milliseconds.</summary>
	/// <returns>The <see cref="T:System.Threading.RegisteredWaitHandle" /> that encapsulates the native handle.</returns>
	/// <param name="waitObject">The <see cref="T:System.Threading.WaitHandle" /> to register. Use a <see cref="T:System.Threading.WaitHandle" /> other than <see cref="T:System.Threading.Mutex" />.</param>
	/// <param name="callBack">The <see cref="T:System.Threading.WaitOrTimerCallback" /> delegate to call when the <paramref name="waitObject" /> parameter is signaled. </param>
	/// <param name="state">The object passed to the delegate. </param>
	/// <param name="millisecondsTimeOutInterval">The time-out in milliseconds. If the <paramref name="millisecondsTimeOutInterval" /> parameter is 0 (zero), the function tests the object's state and returns immediately. If <paramref name="millisecondsTimeOutInterval" /> is -1, the function's time-out interval never elapses. </param>
	/// <param name="executeOnlyOnce">true to indicate that the thread will no longer wait on the <paramref name="waitObject" /> parameter after the delegate has been called; false to indicate that the timer is reset every time the wait operation completes until the wait is unregistered. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="millisecondsTimeOutInterval" /> parameter is less than -1. </exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.NoInlining)]
	[SecuritySafeCritical]
	public static RegisteredWaitHandle RegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callBack, object state, long millisecondsTimeOutInterval, bool executeOnlyOnce)
	{
		if (millisecondsTimeOutInterval < -1)
		{
			throw new ArgumentOutOfRangeException("millisecondsTimeOutInterval", Environment.GetResourceString("Number must be either non-negative and less than or equal to Int32.MaxValue or -1."));
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return RegisterWaitForSingleObject(waitObject, callBack, state, (uint)((millisecondsTimeOutInterval == -1) ? uint.MaxValue : millisecondsTimeOutInterval), executeOnlyOnce, ref stackMark, compressStack: true);
	}

	/// <summary>Registers a delegate to wait for a <see cref="T:System.Threading.WaitHandle" />, specifying a 64-bit signed integer for the time-out in milliseconds. This method does not propagate the calling stack to the worker thread.</summary>
	/// <returns>The <see cref="T:System.Threading.RegisteredWaitHandle" /> object that can be used to cancel the registered wait operation.</returns>
	/// <param name="waitObject">The <see cref="T:System.Threading.WaitHandle" /> to register. Use a <see cref="T:System.Threading.WaitHandle" /> other than <see cref="T:System.Threading.Mutex" />.</param>
	/// <param name="callBack">The delegate to call when the <paramref name="waitObject" /> parameter is signaled. </param>
	/// <param name="state">The object that is passed to the delegate. </param>
	/// <param name="millisecondsTimeOutInterval">The time-out in milliseconds. If the <paramref name="millisecondsTimeOutInterval" /> parameter is 0 (zero), the function tests the object's state and returns immediately. If <paramref name="millisecondsTimeOutInterval" /> is -1, the function's time-out interval never elapses. </param>
	/// <param name="executeOnlyOnce">true to indicate that the thread will no longer wait on the <paramref name="waitObject" /> parameter after the delegate has been called; false to indicate that the timer is reset every time the wait operation completes until the wait is unregistered. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="millisecondsTimeOutInterval" /> parameter is less than -1. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	[MethodImpl(MethodImplOptions.NoInlining)]
	[SecurityCritical]
	public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callBack, object state, long millisecondsTimeOutInterval, bool executeOnlyOnce)
	{
		if (millisecondsTimeOutInterval < -1)
		{
			throw new ArgumentOutOfRangeException("millisecondsTimeOutInterval", Environment.GetResourceString("Number must be either non-negative and less than or equal to Int32.MaxValue or -1."));
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return RegisterWaitForSingleObject(waitObject, callBack, state, (uint)((millisecondsTimeOutInterval == -1) ? uint.MaxValue : millisecondsTimeOutInterval), executeOnlyOnce, ref stackMark, compressStack: false);
	}

	/// <summary>Registers a delegate to wait for a <see cref="T:System.Threading.WaitHandle" />, specifying a <see cref="T:System.TimeSpan" /> value for the time-out.</summary>
	/// <returns>The <see cref="T:System.Threading.RegisteredWaitHandle" /> that encapsulates the native handle.</returns>
	/// <param name="waitObject">The <see cref="T:System.Threading.WaitHandle" /> to register. Use a <see cref="T:System.Threading.WaitHandle" /> other than <see cref="T:System.Threading.Mutex" />.</param>
	/// <param name="callBack">The <see cref="T:System.Threading.WaitOrTimerCallback" /> delegate to call when the <paramref name="waitObject" /> parameter is signaled. </param>
	/// <param name="state">The object passed to the delegate. </param>
	/// <param name="timeout">The time-out represented by a <see cref="T:System.TimeSpan" />. If <paramref name="timeout" /> is 0 (zero), the function tests the object's state and returns immediately. If <paramref name="timeout" /> is -1, the function's time-out interval never elapses. </param>
	/// <param name="executeOnlyOnce">true to indicate that the thread will no longer wait on the <paramref name="waitObject" /> parameter after the delegate has been called; false to indicate that the timer is reset every time the wait operation completes until the wait is unregistered. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="timeout" /> parameter is less than -1. </exception>
	/// <exception cref="T:System.NotSupportedException">The <paramref name="timeout" /> parameter is greater than <see cref="F:System.Int32.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.NoInlining)]
	[SecuritySafeCritical]
	public static RegisteredWaitHandle RegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callBack, object state, TimeSpan timeout, bool executeOnlyOnce)
	{
		long num = (long)timeout.TotalMilliseconds;
		if (num < -1)
		{
			throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("Number must be either non-negative and less than or equal to Int32.MaxValue or -1."));
		}
		if (num > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("Argument must be less than or equal to 2^31 - 1 milliseconds."));
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return RegisterWaitForSingleObject(waitObject, callBack, state, (uint)num, executeOnlyOnce, ref stackMark, compressStack: true);
	}

	/// <summary>Registers a delegate to wait for a <see cref="T:System.Threading.WaitHandle" />, specifying a <see cref="T:System.TimeSpan" /> value for the time-out. This method does not propagate the calling stack to the worker thread.</summary>
	/// <returns>The <see cref="T:System.Threading.RegisteredWaitHandle" /> object that can be used to cancel the registered wait operation.</returns>
	/// <param name="waitObject">The <see cref="T:System.Threading.WaitHandle" /> to register. Use a <see cref="T:System.Threading.WaitHandle" /> other than <see cref="T:System.Threading.Mutex" />.</param>
	/// <param name="callBack">The delegate to call when the <paramref name="waitObject" /> parameter is signaled. </param>
	/// <param name="state">The object that is passed to the delegate. </param>
	/// <param name="timeout">The time-out represented by a <see cref="T:System.TimeSpan" />. If <paramref name="timeout" /> is 0 (zero), the function tests the object's state and returns immediately. If <paramref name="timeout" /> is -1, the function's time-out interval never elapses. </param>
	/// <param name="executeOnlyOnce">true to indicate that the thread will no longer wait on the <paramref name="waitObject" /> parameter after the delegate has been called; false to indicate that the timer is reset every time the wait operation completes until the wait is unregistered. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="timeout" /> parameter is less than -1. </exception>
	/// <exception cref="T:System.NotSupportedException">The <paramref name="timeout" /> parameter is greater than <see cref="F:System.Int32.MaxValue" />. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	[MethodImpl(MethodImplOptions.NoInlining)]
	[SecurityCritical]
	public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callBack, object state, TimeSpan timeout, bool executeOnlyOnce)
	{
		long num = (long)timeout.TotalMilliseconds;
		if (num < -1)
		{
			throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("Number must be either non-negative and less than or equal to Int32.MaxValue or -1."));
		}
		if (num > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("Argument must be less than or equal to 2^31 - 1 milliseconds."));
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return RegisterWaitForSingleObject(waitObject, callBack, state, (uint)num, executeOnlyOnce, ref stackMark, compressStack: false);
	}

	/// <summary>Queues a method for execution, and specifies an object containing data to be used by the method. The method executes when a thread pool thread becomes available.</summary>
	/// <returns>true if the method is successfully queued; <see cref="T:System.NotSupportedException" /> is thrown if the work item could not be queued.</returns>
	/// <param name="callBack">A <see cref="T:System.Threading.WaitCallback" /> representing the method to execute. </param>
	/// <param name="state">An object containing data to be used by the method. </param>
	/// <exception cref="T:System.NotSupportedException">The common language runtime (CLR) is hosted, and the host does not support this action.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="callBack" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.NoInlining)]
	[SecuritySafeCritical]
	public static bool QueueUserWorkItem(WaitCallback callBack, object state)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return QueueUserWorkItemHelper(callBack, state, ref stackMark, compressStack: true);
	}

	/// <summary>Queues a method for execution. The method executes when a thread pool thread becomes available.</summary>
	/// <returns>true if the method is successfully queued; <see cref="T:System.NotSupportedException" /> is thrown if the work item could not be queued.</returns>
	/// <param name="callBack">A <see cref="T:System.Threading.WaitCallback" /> that represents the method to be executed. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="callBack" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">The common language runtime (CLR) is hosted, and the host does not support this action.</exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.NoInlining)]
	[SecuritySafeCritical]
	public static bool QueueUserWorkItem(WaitCallback callBack)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return QueueUserWorkItemHelper(callBack, null, ref stackMark, compressStack: true);
	}

	/// <summary>Queues the specified delegate to the thread pool, but does not propagate the calling stack to the worker thread.</summary>
	/// <returns>true if the method succeeds; <see cref="T:System.OutOfMemoryException" /> is thrown if the work item could not be queued.</returns>
	/// <param name="callBack">A <see cref="T:System.Threading.WaitCallback" /> that represents the delegate to invoke when a thread in the thread pool picks up the work item. </param>
	/// <param name="state">The object that is passed to the delegate when serviced from the thread pool. </param>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ApplicationException">An out-of-memory condition was encountered.</exception>
	/// <exception cref="T:System.OutOfMemoryException">The work item could not be queued.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="callBack" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	[MethodImpl(MethodImplOptions.NoInlining)]
	[SecurityCritical]
	public static bool UnsafeQueueUserWorkItem(WaitCallback callBack, object state)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return QueueUserWorkItemHelper(callBack, state, ref stackMark, compressStack: false);
	}

	[SecurityCritical]
	private static bool QueueUserWorkItemHelper(WaitCallback callBack, object state, ref StackCrawlMark stackMark, bool compressStack)
	{
		bool flag = true;
		if (callBack != null)
		{
			EnsureVMInitialized();
			try
			{
			}
			finally
			{
				QueueUserWorkItemCallback callback = new QueueUserWorkItemCallback(callBack, state, compressStack, ref stackMark);
				ThreadPoolGlobals.workQueue.Enqueue(callback, forceGlobal: true);
				flag = true;
			}
			return flag;
		}
		throw new ArgumentNullException("WaitCallback");
	}

	[SecurityCritical]
	internal static void UnsafeQueueCustomWorkItem(IThreadPoolWorkItem workItem, bool forceGlobal)
	{
		EnsureVMInitialized();
		try
		{
		}
		finally
		{
			ThreadPoolGlobals.workQueue.Enqueue(workItem, forceGlobal);
		}
	}

	[SecurityCritical]
	internal static bool TryPopCustomWorkItem(IThreadPoolWorkItem workItem)
	{
		if (!ThreadPoolGlobals.vmTpInitialized)
		{
			return false;
		}
		return ThreadPoolGlobals.workQueue.LocalFindAndPop(workItem);
	}

	[SecurityCritical]
	internal static IEnumerable<IThreadPoolWorkItem> GetQueuedWorkItems()
	{
		return EnumerateQueuedWorkItems(ThreadPoolWorkQueue.allThreadQueues.Current, ThreadPoolGlobals.workQueue.queueTail);
	}

	internal static IEnumerable<IThreadPoolWorkItem> EnumerateQueuedWorkItems(ThreadPoolWorkQueue.WorkStealingQueue[] wsQueues, ThreadPoolWorkQueue.QueueSegment globalQueueTail)
	{
		if (wsQueues != null)
		{
			foreach (ThreadPoolWorkQueue.WorkStealingQueue workStealingQueue in wsQueues)
			{
				if (workStealingQueue == null || workStealingQueue.m_array == null)
				{
					continue;
				}
				IThreadPoolWorkItem[] items = workStealingQueue.m_array;
				foreach (IThreadPoolWorkItem threadPoolWorkItem in items)
				{
					if (threadPoolWorkItem != null)
					{
						yield return threadPoolWorkItem;
					}
				}
			}
		}
		if (globalQueueTail == null)
		{
			yield break;
		}
		for (ThreadPoolWorkQueue.QueueSegment segment = globalQueueTail; segment != null; segment = segment.Next)
		{
			IThreadPoolWorkItem[] items2 = segment.nodes;
			foreach (IThreadPoolWorkItem threadPoolWorkItem2 in items2)
			{
				if (threadPoolWorkItem2 != null)
				{
					yield return threadPoolWorkItem2;
				}
			}
		}
	}

	[SecurityCritical]
	internal static IEnumerable<IThreadPoolWorkItem> GetLocallyQueuedWorkItems()
	{
		return EnumerateQueuedWorkItems(new ThreadPoolWorkQueue.WorkStealingQueue[1] { ThreadPoolWorkQueueThreadLocals.threadLocals.workStealingQueue }, null);
	}

	[SecurityCritical]
	internal static IEnumerable<IThreadPoolWorkItem> GetGloballyQueuedWorkItems()
	{
		return EnumerateQueuedWorkItems(null, ThreadPoolGlobals.workQueue.queueTail);
	}

	private static object[] ToObjectArray(IEnumerable<IThreadPoolWorkItem> workitems)
	{
		int num = 0;
		foreach (IThreadPoolWorkItem workitem in workitems)
		{
			_ = workitem;
			num++;
		}
		object[] array = new object[num];
		num = 0;
		foreach (IThreadPoolWorkItem workitem2 in workitems)
		{
			if (num < array.Length)
			{
				array[num] = workitem2;
			}
			num++;
		}
		return array;
	}

	[SecurityCritical]
	internal static object[] GetQueuedWorkItemsForDebugger()
	{
		return ToObjectArray(GetQueuedWorkItems());
	}

	[SecurityCritical]
	internal static object[] GetGloballyQueuedWorkItemsForDebugger()
	{
		return ToObjectArray(GetGloballyQueuedWorkItems());
	}

	[SecurityCritical]
	internal static object[] GetLocallyQueuedWorkItemsForDebugger()
	{
		return ToObjectArray(GetLocallyQueuedWorkItems());
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	internal static extern bool RequestWorkerThread();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private unsafe static extern bool PostQueuedCompletionStatus(NativeOverlapped* overlapped);

	/// <summary>Queues an overlapped I/O operation for execution.</summary>
	/// <returns>true if the operation was successfully queued to an I/O completion port; otherwise, false.</returns>
	/// <param name="overlapped">The <see cref="T:System.Threading.NativeOverlapped" /> structure to queue.</param>
	[SecurityCritical]
	[CLSCompliant(false)]
	public unsafe static bool UnsafeQueueNativeOverlapped(NativeOverlapped* overlapped)
	{
		return PostQueuedCompletionStatus(overlapped);
	}

	[SecurityCritical]
	private static void EnsureVMInitialized()
	{
		if (!ThreadPoolGlobals.vmTpInitialized)
		{
			InitializeVMTp(ref ThreadPoolGlobals.enableWorkerTracking);
			ThreadPoolGlobals.vmTpInitialized = true;
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private static extern bool SetMinThreadsNative(int workerThreads, int completionPortThreads);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private static extern bool SetMaxThreadsNative(int workerThreads, int completionPortThreads);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private static extern void GetMinThreadsNative(out int workerThreads, out int completionPortThreads);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private static extern void GetMaxThreadsNative(out int workerThreads, out int completionPortThreads);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private static extern void GetAvailableThreadsNative(out int workerThreads, out int completionPortThreads);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	internal static extern bool NotifyWorkItemComplete();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	internal static extern void ReportThreadStatus(bool isWorking);

	[SecuritySafeCritical]
	internal static void NotifyWorkItemProgress()
	{
		if (!ThreadPoolGlobals.vmTpInitialized)
		{
			InitializeVMTp(ref ThreadPoolGlobals.enableWorkerTracking);
		}
		NotifyWorkItemProgressNative();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	internal static extern void NotifyWorkItemProgressNative();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	internal static extern bool IsThreadPoolHosted();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private static extern void InitializeVMTp(ref bool enableWorkerTracking);

	/// <summary>Binds an operating system handle to the <see cref="T:System.Threading.ThreadPool" />.</summary>
	/// <returns>true if the handle is bound; otherwise, false.</returns>
	/// <param name="osHandle">An <see cref="T:System.IntPtr" /> that holds the handle. The handle must have been opened for overlapped I/O on the unmanaged side. </param>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	[Obsolete("ThreadPool.BindHandle(IntPtr) has been deprecated.  Please use ThreadPool.BindHandle(SafeHandle) instead.", false)]
	[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	public static bool BindHandle(IntPtr osHandle)
	{
		return BindIOCompletionCallbackNative(osHandle);
	}

	/// <summary>Binds an operating system handle to the <see cref="T:System.Threading.ThreadPool" />.</summary>
	/// <returns>true if the handle is bound; otherwise, false.</returns>
	/// <param name="osHandle">A <see cref="T:System.Runtime.InteropServices.SafeHandle" />  that holds the operating system handle. The handle must have been opened for overlapped I/O on the unmanaged side.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="osHandle" /> is null. </exception>
	[SecuritySafeCritical]
	[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	public static bool BindHandle(SafeHandle osHandle)
	{
		if (osHandle == null)
		{
			throw new ArgumentNullException("osHandle");
		}
		bool flag = false;
		bool success = false;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			osHandle.DangerousAddRef(ref success);
			return BindIOCompletionCallbackNative(osHandle.DangerousGetHandle());
		}
		finally
		{
			if (success)
			{
				osHandle.DangerousRelease();
			}
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	[SecurityCritical]
	private static extern bool BindIOCompletionCallbackNative(IntPtr fileHandle);
}
