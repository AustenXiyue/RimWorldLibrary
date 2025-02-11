using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security.Permissions;

namespace System.Threading.Tasks;

/// <summary>Provides support for creating and scheduling <see cref="T:System.Threading.Tasks.Task`1" /> objects.</summary>
/// <typeparam name="TResult">The type of the results that are available through the <see cref="T:System.Threading.Tasks.Task`1" /> objects that are associated with the methods in this class.</typeparam>
[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
public class TaskFactory<TResult>
{
	private sealed class FromAsyncTrimPromise<TInstance> : Task<TResult> where TInstance : class
	{
		internal static readonly AsyncCallback s_completeFromAsyncResult = CompleteFromAsyncResult;

		private TInstance m_thisRef;

		private Func<TInstance, IAsyncResult, TResult> m_endMethod;

		internal FromAsyncTrimPromise(TInstance thisRef, Func<TInstance, IAsyncResult, TResult> endMethod)
		{
			m_thisRef = thisRef;
			m_endMethod = endMethod;
		}

		internal static void CompleteFromAsyncResult(IAsyncResult asyncResult)
		{
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			if (!(asyncResult.AsyncState is FromAsyncTrimPromise<TInstance> { m_thisRef: var thisRef, m_endMethod: var endMethod } fromAsyncTrimPromise))
			{
				throw new ArgumentException(Environment.GetResourceString("Either the IAsyncResult object did not come from the corresponding async method on this type, or the End method was called multiple times with the same IAsyncResult."), "asyncResult");
			}
			fromAsyncTrimPromise.m_thisRef = null;
			fromAsyncTrimPromise.m_endMethod = null;
			if (endMethod == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Either the IAsyncResult object did not come from the corresponding async method on this type, or the End method was called multiple times with the same IAsyncResult."), "asyncResult");
			}
			if (!asyncResult.CompletedSynchronously)
			{
				fromAsyncTrimPromise.Complete(thisRef, endMethod, asyncResult, requiresSynchronization: true);
			}
		}

		internal void Complete(TInstance thisRef, Func<TInstance, IAsyncResult, TResult> endMethod, IAsyncResult asyncResult, bool requiresSynchronization)
		{
			try
			{
				TResult result = endMethod(thisRef, asyncResult);
				if (requiresSynchronization)
				{
					TrySetResult(result);
				}
				else
				{
					DangerousSetResult(result);
				}
			}
			catch (OperationCanceledException ex)
			{
				TrySetCanceled(ex.CancellationToken, ex);
			}
			catch (Exception exceptionObject)
			{
				TrySetException(exceptionObject);
			}
		}
	}

	private CancellationToken m_defaultCancellationToken;

	private TaskScheduler m_defaultScheduler;

	private TaskCreationOptions m_defaultCreationOptions;

	private TaskContinuationOptions m_defaultContinuationOptions;

	private TaskScheduler DefaultScheduler
	{
		get
		{
			if (m_defaultScheduler == null)
			{
				return TaskScheduler.Current;
			}
			return m_defaultScheduler;
		}
	}

	/// <summary>Gets the default cancellation token for this task factory.</summary>
	/// <returns>The default cancellation token for this task factory.</returns>
	public CancellationToken CancellationToken => m_defaultCancellationToken;

	/// <summary>Gets the task scheduler for this task factory.</summary>
	/// <returns>The task scheduler for this task factory.</returns>
	public TaskScheduler Scheduler => m_defaultScheduler;

	/// <summary>Gets the <see cref="T:System.Threading.Tasks.TaskCreationOptions" /> enumeration value for this task factory.</summary>
	/// <returns>One of the enumeration values that specifies the default creation options for this task factory.</returns>
	public TaskCreationOptions CreationOptions => m_defaultCreationOptions;

	/// <summary>Gets the <see cref="T:System.Threading.Tasks.TaskContinuationOptions" /> enumeration value for this task factory.</summary>
	/// <returns>One of the enumeration values that specifies the default continuation options for this task factory.</returns>
	public TaskContinuationOptions ContinuationOptions => m_defaultContinuationOptions;

	private TaskScheduler GetDefaultScheduler(Task currTask)
	{
		if (m_defaultScheduler != null)
		{
			return m_defaultScheduler;
		}
		if (currTask != null && (currTask.CreationOptions & TaskCreationOptions.HideScheduler) == 0)
		{
			return currTask.ExecutingTaskScheduler;
		}
		return TaskScheduler.Default;
	}

	/// <summary>Initializes a <see cref="T:System.Threading.Tasks.TaskFactory`1" /> instance with the default configuration.</summary>
	public TaskFactory()
		: this(default(CancellationToken), TaskCreationOptions.None, TaskContinuationOptions.None, (TaskScheduler)null)
	{
	}

	/// <summary>Initializes a <see cref="T:System.Threading.Tasks.TaskFactory`1" /> instance with the default configuration.</summary>
	/// <param name="cancellationToken">The default cancellation token that will be assigned to tasks created by this <see cref="T:System.Threading.Tasks.TaskFactory" /> unless another cancellation token is explicitly specified when calling the factory methods.</param>
	public TaskFactory(CancellationToken cancellationToken)
		: this(cancellationToken, TaskCreationOptions.None, TaskContinuationOptions.None, (TaskScheduler)null)
	{
	}

	/// <summary>Initializes a <see cref="T:System.Threading.Tasks.TaskFactory`1" /> instance with the specified configuration.</summary>
	/// <param name="scheduler">The scheduler to use to schedule any tasks created with this <see cref="T:System.Threading.Tasks.TaskFactory`1" />. A null value indicates that the current <see cref="T:System.Threading.Tasks.TaskScheduler" /> should be used.</param>
	public TaskFactory(TaskScheduler scheduler)
		: this(default(CancellationToken), TaskCreationOptions.None, TaskContinuationOptions.None, scheduler)
	{
	}

	/// <summary>Initializes a <see cref="T:System.Threading.Tasks.TaskFactory`1" /> instance with the specified configuration.</summary>
	/// <param name="creationOptions">The default options to use when creating tasks with this <see cref="T:System.Threading.Tasks.TaskFactory`1" />.</param>
	/// <param name="continuationOptions">The default options to use when creating continuation tasks with this <see cref="T:System.Threading.Tasks.TaskFactory`1" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="creationOptions" /> or <paramref name="continuationOptions" /> specifies an invalid value.</exception>
	public TaskFactory(TaskCreationOptions creationOptions, TaskContinuationOptions continuationOptions)
		: this(default(CancellationToken), creationOptions, continuationOptions, (TaskScheduler)null)
	{
	}

	/// <summary>Initializes a <see cref="T:System.Threading.Tasks.TaskFactory`1" /> instance with the specified configuration.</summary>
	/// <param name="cancellationToken">The default cancellation token that will be assigned to tasks created by this <see cref="T:System.Threading.Tasks.TaskFactory" /> unless another cancellation token is explicitly specified when calling the factory methods.</param>
	/// <param name="creationOptions">The default options to use when creating tasks with this <see cref="T:System.Threading.Tasks.TaskFactory`1" />.</param>
	/// <param name="continuationOptions">The default options to use when creating continuation tasks with this <see cref="T:System.Threading.Tasks.TaskFactory`1" />.</param>
	/// <param name="scheduler">The default scheduler to use to schedule any tasks created with this <see cref="T:System.Threading.Tasks.TaskFactory`1" />. A null value indicates that <see cref="P:System.Threading.Tasks.TaskScheduler.Current" /> should be used.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="creationOptions" /> or <paramref name="continuationOptions" /> specifies an invalid value.</exception>
	public TaskFactory(CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		TaskFactory.CheckMultiTaskContinuationOptions(continuationOptions);
		TaskFactory.CheckCreationOptions(creationOptions);
		m_defaultCancellationToken = cancellationToken;
		m_defaultScheduler = scheduler;
		m_defaultCreationOptions = creationOptions;
		m_defaultContinuationOptions = continuationOptions;
	}

	/// <summary>Creates and starts a task.</summary>
	/// <returns>The started task.</returns>
	/// <param name="function">A function delegate that returns the future result to be available through the task.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="function" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> StartNew(Func<TResult> function)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		Task internalCurrent = Task.InternalCurrent;
		return Task<TResult>.StartNew(internalCurrent, function, m_defaultCancellationToken, m_defaultCreationOptions, InternalTaskOptions.None, GetDefaultScheduler(internalCurrent), ref stackMark);
	}

	/// <summary>Creates and starts a task.</summary>
	/// <returns>The started task.</returns>
	/// <param name="function">A function delegate that returns the future result to be available through the task.</param>
	/// <param name="cancellationToken">The cancellation token that will be assigned to the new task.</param>
	/// <exception cref="T:System.ObjectDisposedException">The cancellation token source that created<paramref name="cancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="function" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> StartNew(Func<TResult> function, CancellationToken cancellationToken)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		Task internalCurrent = Task.InternalCurrent;
		return Task<TResult>.StartNew(internalCurrent, function, cancellationToken, m_defaultCreationOptions, InternalTaskOptions.None, GetDefaultScheduler(internalCurrent), ref stackMark);
	}

	/// <summary>Creates and starts a task.</summary>
	/// <returns>The started <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="function">A function delegate that returns the future result to be available through the task.</param>
	/// <param name="creationOptions">One of the enumeration values that controls the behavior of the created task.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="function" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="creationOptions" /> parameter specifies an invalid value.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> StartNew(Func<TResult> function, TaskCreationOptions creationOptions)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		Task internalCurrent = Task.InternalCurrent;
		return Task<TResult>.StartNew(internalCurrent, function, m_defaultCancellationToken, creationOptions, InternalTaskOptions.None, GetDefaultScheduler(internalCurrent), ref stackMark);
	}

	/// <summary>Creates and starts a task.</summary>
	/// <returns>The started task.</returns>
	/// <param name="function">A function delegate that returns the future result to be available through the task.</param>
	/// <param name="cancellationToken">The cancellation token that will be assigned to the new task.</param>
	/// <param name="creationOptions">One of the enumeration values that controls the behavior of the created task.</param>
	/// <param name="scheduler">The task scheduler that is used to schedule the created task.</param>
	/// <exception cref="T:System.ObjectDisposedException">The cancellation token source that created<paramref name="cancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="function" /> argument is null.-or-The <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="creationOptions" /> parameter specifies an invalid value.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> StartNew(Func<TResult> function, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return Task<TResult>.StartNew(Task.InternalCurrentIfAttached(creationOptions), function, cancellationToken, creationOptions, InternalTaskOptions.None, scheduler, ref stackMark);
	}

	/// <summary>Creates and starts a task.</summary>
	/// <returns>The started task.</returns>
	/// <param name="function">A function delegate that returns the future result to be available through the task.</param>
	/// <param name="state">An object that contains data to be used by the <paramref name="function" /> delegate.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="function" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> StartNew(Func<object, TResult> function, object state)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		Task internalCurrent = Task.InternalCurrent;
		return Task<TResult>.StartNew(internalCurrent, function, state, m_defaultCancellationToken, m_defaultCreationOptions, InternalTaskOptions.None, GetDefaultScheduler(internalCurrent), ref stackMark);
	}

	/// <summary>Creates and starts a task.</summary>
	/// <returns>The started task.</returns>
	/// <param name="function">A function delegate that returns the future result to be available through the task.</param>
	/// <param name="state">An object that contains data to be used by the <paramref name="function" /> delegate.</param>
	/// <param name="cancellationToken">The cancellation token that will be assigned to the new task.</param>
	/// <exception cref="T:System.ObjectDisposedException">The cancellation token source that created<paramref name="cancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="function" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> StartNew(Func<object, TResult> function, object state, CancellationToken cancellationToken)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		Task internalCurrent = Task.InternalCurrent;
		return Task<TResult>.StartNew(internalCurrent, function, state, cancellationToken, m_defaultCreationOptions, InternalTaskOptions.None, GetDefaultScheduler(internalCurrent), ref stackMark);
	}

	/// <summary>Creates and starts a task.</summary>
	/// <returns>The started task.</returns>
	/// <param name="function">A function delegate that returns the future result to be available through the task.</param>
	/// <param name="state">An object that contains data to be used by the <paramref name="function" /> delegate.</param>
	/// <param name="creationOptions">One of the enumeration values that controls the behavior of the created task.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="function" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="creationOptions" /> parameter specifies an invalid value.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> StartNew(Func<object, TResult> function, object state, TaskCreationOptions creationOptions)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		Task internalCurrent = Task.InternalCurrent;
		return Task<TResult>.StartNew(internalCurrent, function, state, m_defaultCancellationToken, creationOptions, InternalTaskOptions.None, GetDefaultScheduler(internalCurrent), ref stackMark);
	}

	/// <summary>Creates and starts a task.</summary>
	/// <returns>The started task.</returns>
	/// <param name="function">A function delegate that returns the future result to be available through the task.</param>
	/// <param name="state">An object that contains data to be used by the <paramref name="function" /> delegate.</param>
	/// <param name="cancellationToken">The cancellation token that will be assigned to the new task.</param>
	/// <param name="creationOptions">One of the enumeration values that controls the behavior of the created task.</param>
	/// <param name="scheduler">The task scheduler that is used to schedule the created task.</param>
	/// <exception cref="T:System.ObjectDisposedException">The cancellation token source that created<paramref name="cancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="function" /> argument is null.-or-The <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="creationOptions" /> parameter specifies an invalid value.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> StartNew(Func<object, TResult> function, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return Task<TResult>.StartNew(Task.InternalCurrentIfAttached(creationOptions), function, state, cancellationToken, creationOptions, InternalTaskOptions.None, scheduler, ref stackMark);
	}

	private static void FromAsyncCoreLogic(IAsyncResult iar, Func<IAsyncResult, TResult> endFunction, Action<IAsyncResult> endAction, Task<TResult> promise, bool requiresSynchronization)
	{
		Exception ex = null;
		OperationCanceledException ex2 = null;
		TResult result = default(TResult);
		try
		{
			if (endFunction != null)
			{
				result = endFunction(iar);
			}
			else
			{
				endAction(iar);
			}
		}
		catch (OperationCanceledException ex3)
		{
			ex2 = ex3;
		}
		catch (Exception ex4)
		{
			ex = ex4;
		}
		finally
		{
			if (ex2 != null)
			{
				promise.TrySetCanceled(ex2.CancellationToken, ex2);
			}
			else if (ex != null)
			{
				if (promise.TrySetException(ex) && ex is ThreadAbortException)
				{
					promise.m_contingentProperties.m_exceptionsHolder.MarkAsHandled(calledFromFinalizer: false);
				}
			}
			else
			{
				if (AsyncCausalityTracer.LoggingOn)
				{
					AsyncCausalityTracer.TraceOperationCompletion(CausalityTraceLevel.Required, promise.Id, AsyncCausalityStatus.Completed);
				}
				if (Task.s_asyncDebuggingEnabled)
				{
					Task.RemoveFromActiveTasks(promise.Id);
				}
				if (requiresSynchronization)
				{
					promise.TrySetResult(result);
				}
				else
				{
					promise.DangerousSetResult(result);
				}
			}
		}
	}

	/// <summary>Creates a task that executes an end method function when a specified <see cref="T:System.IAsyncResult" /> completes.</summary>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous operation.</returns>
	/// <param name="asyncResult">The <see cref="T:System.IAsyncResult" /> whose completion should trigger the processing of the <paramref name="endMethod" />.</param>
	/// <param name="endMethod">The function delegate that processes the completed <paramref name="asyncResult" />.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="asyncResult" /> argument is null.-or-The <paramref name="endMethod" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> FromAsync(IAsyncResult asyncResult, Func<IAsyncResult, TResult> endMethod)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return FromAsyncImpl(asyncResult, endMethod, null, m_defaultCreationOptions, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a task that executes an end method function when a specified <see cref="T:System.IAsyncResult" /> completes.</summary>
	/// <returns>A task that represents the asynchronous operation.</returns>
	/// <param name="asyncResult">The <see cref="T:System.IAsyncResult" /> whose completion should trigger the processing of the <paramref name="endMethod" />.</param>
	/// <param name="endMethod">The function delegate that processes the completed <paramref name="asyncResult" />.</param>
	/// <param name="creationOptions">One of the enumeration values that controls the behavior of the created task.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="asyncResult" /> argument is null.-or-The <paramref name="endMethod" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="creationOptions" /> argument specifies an invalid value.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> FromAsync(IAsyncResult asyncResult, Func<IAsyncResult, TResult> endMethod, TaskCreationOptions creationOptions)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return FromAsyncImpl(asyncResult, endMethod, null, creationOptions, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a task that executes an end method function when a specified <see cref="T:System.IAsyncResult" /> completes.</summary>
	/// <returns>The created task that represents the asynchronous operation.</returns>
	/// <param name="asyncResult">The <see cref="T:System.IAsyncResult" /> whose completion should trigger the processing of the <paramref name="endMethod" />.</param>
	/// <param name="endMethod">The function delegate that processes the completed <paramref name="asyncResult" />.</param>
	/// <param name="creationOptions">One of the enumeration values that controls the behavior of the created task.</param>
	/// <param name="scheduler">The task scheduler that is used to schedule the task that executes the end method.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="asyncResult" /> argument is null.-or-The <paramref name="endMethod" /> argument is null.-or-The <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="creationOptions" /> parameter specifies an invalid value.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> FromAsync(IAsyncResult asyncResult, Func<IAsyncResult, TResult> endMethod, TaskCreationOptions creationOptions, TaskScheduler scheduler)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return FromAsyncImpl(asyncResult, endMethod, null, creationOptions, scheduler, ref stackMark);
	}

	internal static Task<TResult> FromAsyncImpl(IAsyncResult asyncResult, Func<IAsyncResult, TResult> endFunction, Action<IAsyncResult> endAction, TaskCreationOptions creationOptions, TaskScheduler scheduler, ref StackCrawlMark stackMark)
	{
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		if (endFunction == null && endAction == null)
		{
			throw new ArgumentNullException("endMethod");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		TaskFactory.CheckFromAsyncOptions(creationOptions, hasBeginMethod: false);
		Task<TResult> promise = new Task<TResult>((object)null, creationOptions);
		if (AsyncCausalityTracer.LoggingOn)
		{
			AsyncCausalityTracer.TraceOperationCreation(CausalityTraceLevel.Required, promise.Id, "TaskFactory.FromAsync", 0uL);
		}
		if (Task.s_asyncDebuggingEnabled)
		{
			Task.AddToActiveTasks(promise);
		}
		Task t = new Task(delegate
		{
			FromAsyncCoreLogic(asyncResult, endFunction, endAction, promise, requiresSynchronization: true);
		}, null, null, default(CancellationToken), TaskCreationOptions.None, InternalTaskOptions.None, null, ref stackMark);
		if (AsyncCausalityTracer.LoggingOn)
		{
			AsyncCausalityTracer.TraceOperationCreation(CausalityTraceLevel.Verbose, t.Id, "TaskFactory.FromAsync Callback", 0uL);
		}
		if (Task.s_asyncDebuggingEnabled)
		{
			Task.AddToActiveTasks(t);
		}
		if (asyncResult.IsCompleted)
		{
			try
			{
				t.InternalRunSynchronously(scheduler, waitForCompletion: false);
			}
			catch (Exception exceptionObject)
			{
				promise.TrySetException(exceptionObject);
			}
		}
		else
		{
			ThreadPool.RegisterWaitForSingleObject(asyncResult.AsyncWaitHandle, delegate
			{
				try
				{
					t.InternalRunSynchronously(scheduler, waitForCompletion: false);
				}
				catch (Exception exceptionObject2)
				{
					promise.TrySetException(exceptionObject2);
				}
			}, null, -1, executeOnlyOnce: true);
		}
		return promise;
	}

	/// <summary>Creates a task that represents a pair of begin and end methods that conform to the Asynchronous Programming Model pattern.</summary>
	/// <returns>The created task that represents the asynchronous operation.</returns>
	/// <param name="beginMethod">The delegate that begins the asynchronous operation.</param>
	/// <param name="endMethod">The delegate that ends the asynchronous operation.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="beginMethod" /> delegate.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="beginMethod" /> argument is null.-or-The <paramref name="endMethod" /> argument is null.</exception>
	public Task<TResult> FromAsync(Func<AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, object state)
	{
		return FromAsyncImpl(beginMethod, endMethod, null, state, m_defaultCreationOptions);
	}

	/// <summary>Creates a task that represents a pair of begin and end methods that conform to the Asynchronous Programming Model pattern.</summary>
	/// <returns>The created <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous operation.</returns>
	/// <param name="beginMethod">The delegate that begins the asynchronous operation.</param>
	/// <param name="endMethod">The delegate that ends the asynchronous operation.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="creationOptions">One of the enumeration values that controls the behavior of the created task.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="beginMethod" /> argument is null.-or-The <paramref name="endMethod" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="creationOptions" /> argument specifies an invalid value.</exception>
	public Task<TResult> FromAsync(Func<AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, object state, TaskCreationOptions creationOptions)
	{
		return FromAsyncImpl(beginMethod, endMethod, null, state, creationOptions);
	}

	internal static Task<TResult> FromAsyncImpl(Func<AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endFunction, Action<IAsyncResult> endAction, object state, TaskCreationOptions creationOptions)
	{
		if (beginMethod == null)
		{
			throw new ArgumentNullException("beginMethod");
		}
		if (endFunction == null && endAction == null)
		{
			throw new ArgumentNullException("endMethod");
		}
		TaskFactory.CheckFromAsyncOptions(creationOptions, hasBeginMethod: true);
		Task<TResult> promise = new Task<TResult>(state, creationOptions);
		if (AsyncCausalityTracer.LoggingOn)
		{
			AsyncCausalityTracer.TraceOperationCreation(CausalityTraceLevel.Required, promise.Id, "TaskFactory.FromAsync: " + beginMethod.Method.Name, 0uL);
		}
		if (Task.s_asyncDebuggingEnabled)
		{
			Task.AddToActiveTasks(promise);
		}
		try
		{
			if (BinaryCompatibility.TargetsAtLeast_Desktop_V4_5)
			{
				AtomicBoolean invoked = new AtomicBoolean();
				IAsyncResult asyncResult = beginMethod(delegate(IAsyncResult iar)
				{
					if (invoked.TryRelaxedSet())
					{
						FromAsyncCoreLogic(iar, endFunction, endAction, promise, requiresSynchronization: true);
					}
				}, state);
				if (asyncResult != null && asyncResult.CompletedSynchronously && invoked.TryRelaxedSet())
				{
					FromAsyncCoreLogic(asyncResult, endFunction, endAction, promise, requiresSynchronization: false);
				}
			}
			else
			{
				beginMethod(delegate(IAsyncResult iar)
				{
					FromAsyncCoreLogic(iar, endFunction, endAction, promise, requiresSynchronization: true);
				}, state);
			}
		}
		catch
		{
			if (AsyncCausalityTracer.LoggingOn)
			{
				AsyncCausalityTracer.TraceOperationCompletion(CausalityTraceLevel.Required, promise.Id, AsyncCausalityStatus.Error);
			}
			if (Task.s_asyncDebuggingEnabled)
			{
				Task.RemoveFromActiveTasks(promise.Id);
			}
			promise.TrySetResult(default(TResult));
			throw;
		}
		return promise;
	}

	/// <summary>Creates a task that represents a pair of begin and end methods that conform to the Asynchronous Programming Model pattern.</summary>
	/// <returns>The created task that represents the asynchronous operation.</returns>
	/// <param name="beginMethod">The delegate that begins the asynchronous operation.</param>
	/// <param name="endMethod">The delegate that ends the asynchronous operation.</param>
	/// <param name="arg1">The first argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="beginMethod" /> delegate.</param>
	/// <typeparam name="TArg1">The type of the first argument passed to the <paramref name="beginMethod" /> delegate.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="beginMethod" /> argument is null.-or-The <paramref name="endMethod" /> argument is null.</exception>
	public Task<TResult> FromAsync<TArg1>(Func<TArg1, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg1 arg1, object state)
	{
		return FromAsyncImpl(beginMethod, endMethod, null, arg1, state, m_defaultCreationOptions);
	}

	/// <summary>Creates a task that represents a pair of begin and end methods that conform to the Asynchronous Programming Model pattern.</summary>
	/// <returns>The created task that represents the asynchronous operation.</returns>
	/// <param name="beginMethod">The delegate that begins the asynchronous operation.</param>
	/// <param name="endMethod">The delegate that ends the asynchronous operation.</param>
	/// <param name="arg1">The first argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="creationOptions">One of the enumeration values that controls the behavior of the created task.</param>
	/// <typeparam name="TArg1">The type of the first argument passed to the <paramref name="beginMethod" /> delegate.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="beginMethod" /> argument is null.-or-The <paramref name="endMethod" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="creationOptions" /> parameter specifies an invalid value.</exception>
	public Task<TResult> FromAsync<TArg1>(Func<TArg1, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg1 arg1, object state, TaskCreationOptions creationOptions)
	{
		return FromAsyncImpl(beginMethod, endMethod, null, arg1, state, creationOptions);
	}

	internal static Task<TResult> FromAsyncImpl<TArg1>(Func<TArg1, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endFunction, Action<IAsyncResult> endAction, TArg1 arg1, object state, TaskCreationOptions creationOptions)
	{
		if (beginMethod == null)
		{
			throw new ArgumentNullException("beginMethod");
		}
		if (endFunction == null && endAction == null)
		{
			throw new ArgumentNullException("endFunction");
		}
		TaskFactory.CheckFromAsyncOptions(creationOptions, hasBeginMethod: true);
		Task<TResult> promise = new Task<TResult>(state, creationOptions);
		if (AsyncCausalityTracer.LoggingOn)
		{
			AsyncCausalityTracer.TraceOperationCreation(CausalityTraceLevel.Required, promise.Id, "TaskFactory.FromAsync: " + beginMethod.Method.Name, 0uL);
		}
		if (Task.s_asyncDebuggingEnabled)
		{
			Task.AddToActiveTasks(promise);
		}
		try
		{
			if (BinaryCompatibility.TargetsAtLeast_Desktop_V4_5)
			{
				AtomicBoolean invoked = new AtomicBoolean();
				IAsyncResult asyncResult = beginMethod(arg1, delegate(IAsyncResult iar)
				{
					if (invoked.TryRelaxedSet())
					{
						FromAsyncCoreLogic(iar, endFunction, endAction, promise, requiresSynchronization: true);
					}
				}, state);
				if (asyncResult != null && asyncResult.CompletedSynchronously && invoked.TryRelaxedSet())
				{
					FromAsyncCoreLogic(asyncResult, endFunction, endAction, promise, requiresSynchronization: false);
				}
			}
			else
			{
				beginMethod(arg1, delegate(IAsyncResult iar)
				{
					FromAsyncCoreLogic(iar, endFunction, endAction, promise, requiresSynchronization: true);
				}, state);
			}
		}
		catch
		{
			if (AsyncCausalityTracer.LoggingOn)
			{
				AsyncCausalityTracer.TraceOperationCompletion(CausalityTraceLevel.Required, promise.Id, AsyncCausalityStatus.Error);
			}
			if (Task.s_asyncDebuggingEnabled)
			{
				Task.RemoveFromActiveTasks(promise.Id);
			}
			promise.TrySetResult(default(TResult));
			throw;
		}
		return promise;
	}

	/// <summary>Creates a task that represents a pair of begin and end methods that conform to the Asynchronous Programming Model pattern.</summary>
	/// <returns>The created task that represents the asynchronous operation.</returns>
	/// <param name="beginMethod">The delegate that begins the asynchronous operation.</param>
	/// <param name="endMethod">The delegate that ends the asynchronous operation.</param>
	/// <param name="arg1">The first argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="arg2">The second argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="beginMethod" /> delegate.</param>
	/// <typeparam name="TArg1">The type of the second argument passed to <paramref name="beginMethod" /> delegate.</typeparam>
	/// <typeparam name="TArg2">The type of the first argument passed to the <paramref name="beginMethod" /> delegate.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="beginMethod" /> argument is null.-or-The <paramref name="endMethod" /> argument is null.</exception>
	public Task<TResult> FromAsync<TArg1, TArg2>(Func<TArg1, TArg2, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg1 arg1, TArg2 arg2, object state)
	{
		return FromAsyncImpl(beginMethod, endMethod, null, arg1, arg2, state, m_defaultCreationOptions);
	}

	/// <summary>Creates a task that represents a pair of begin and end methods that conform to the Asynchronous Programming Model pattern.</summary>
	/// <returns>The created task that represents the asynchronous operation.</returns>
	/// <param name="beginMethod">The delegate that begins the asynchronous operation.</param>
	/// <param name="endMethod">The delegate that ends the asynchronous operation.</param>
	/// <param name="arg1">The first argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="arg2">The second argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="creationOptions">An object that controls the behavior of the created <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <typeparam name="TArg1">The type of the second argument passed to <paramref name="beginMethod" /> delegate.</typeparam>
	/// <typeparam name="TArg2">The type of the first argument passed to the <paramref name="beginMethod" /> delegate.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="beginMethod" /> argument is null.-or-The <paramref name="endMethod" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="creationOptions" /> parameter specifies an invalid value.</exception>
	public Task<TResult> FromAsync<TArg1, TArg2>(Func<TArg1, TArg2, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg1 arg1, TArg2 arg2, object state, TaskCreationOptions creationOptions)
	{
		return FromAsyncImpl(beginMethod, endMethod, null, arg1, arg2, state, creationOptions);
	}

	internal static Task<TResult> FromAsyncImpl<TArg1, TArg2>(Func<TArg1, TArg2, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endFunction, Action<IAsyncResult> endAction, TArg1 arg1, TArg2 arg2, object state, TaskCreationOptions creationOptions)
	{
		if (beginMethod == null)
		{
			throw new ArgumentNullException("beginMethod");
		}
		if (endFunction == null && endAction == null)
		{
			throw new ArgumentNullException("endMethod");
		}
		TaskFactory.CheckFromAsyncOptions(creationOptions, hasBeginMethod: true);
		Task<TResult> promise = new Task<TResult>(state, creationOptions);
		if (AsyncCausalityTracer.LoggingOn)
		{
			AsyncCausalityTracer.TraceOperationCreation(CausalityTraceLevel.Required, promise.Id, "TaskFactory.FromAsync: " + beginMethod.Method.Name, 0uL);
		}
		if (Task.s_asyncDebuggingEnabled)
		{
			Task.AddToActiveTasks(promise);
		}
		try
		{
			if (BinaryCompatibility.TargetsAtLeast_Desktop_V4_5)
			{
				AtomicBoolean invoked = new AtomicBoolean();
				IAsyncResult asyncResult = beginMethod(arg1, arg2, delegate(IAsyncResult iar)
				{
					if (invoked.TryRelaxedSet())
					{
						FromAsyncCoreLogic(iar, endFunction, endAction, promise, requiresSynchronization: true);
					}
				}, state);
				if (asyncResult != null && asyncResult.CompletedSynchronously && invoked.TryRelaxedSet())
				{
					FromAsyncCoreLogic(asyncResult, endFunction, endAction, promise, requiresSynchronization: false);
				}
			}
			else
			{
				beginMethod(arg1, arg2, delegate(IAsyncResult iar)
				{
					FromAsyncCoreLogic(iar, endFunction, endAction, promise, requiresSynchronization: true);
				}, state);
			}
		}
		catch
		{
			if (AsyncCausalityTracer.LoggingOn)
			{
				AsyncCausalityTracer.TraceOperationCompletion(CausalityTraceLevel.Required, promise.Id, AsyncCausalityStatus.Error);
			}
			if (Task.s_asyncDebuggingEnabled)
			{
				Task.RemoveFromActiveTasks(promise.Id);
			}
			promise.TrySetResult(default(TResult));
			throw;
		}
		return promise;
	}

	/// <summary>Creates a task that represents a pair of begin and end methods that conform to the Asynchronous Programming Model pattern.</summary>
	/// <returns>The created task that represents the asynchronous operation.</returns>
	/// <param name="beginMethod">The delegate that begins the asynchronous operation.</param>
	/// <param name="endMethod">The delegate that ends the asynchronous operation.</param>
	/// <param name="arg1">The first argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="arg2">The second argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="arg3">The third argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="beginMethod" /> delegate.</param>
	/// <typeparam name="TArg1">The type of the second argument passed to <paramref name="beginMethod" /> delegate.</typeparam>
	/// <typeparam name="TArg2">The type of the third argument passed to <paramref name="beginMethod" /> delegate.</typeparam>
	/// <typeparam name="TArg3">The type of the first argument passed to the <paramref name="beginMethod" /> delegate.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="beginMethod" /> argument is null.-or-The <paramref name="endMethod" /> argument is null.</exception>
	public Task<TResult> FromAsync<TArg1, TArg2, TArg3>(Func<TArg1, TArg2, TArg3, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg1 arg1, TArg2 arg2, TArg3 arg3, object state)
	{
		return FromAsyncImpl(beginMethod, endMethod, null, arg1, arg2, arg3, state, m_defaultCreationOptions);
	}

	/// <summary>Creates a task that represents a pair of begin and end methods that conform to the Asynchronous Programming Model pattern.</summary>
	/// <returns>The created task that represents the asynchronous operation.</returns>
	/// <param name="beginMethod">The delegate that begins the asynchronous operation.</param>
	/// <param name="endMethod">The delegate that ends the asynchronous operation.</param>
	/// <param name="arg1">The first argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="arg2">The second argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="arg3">The third argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="creationOptions">An object that controls the behavior of the created task.</param>
	/// <typeparam name="TArg1">The type of the second argument passed to <paramref name="beginMethod" /> delegate.</typeparam>
	/// <typeparam name="TArg2">The type of the third argument passed to <paramref name="beginMethod" /> delegate.</typeparam>
	/// <typeparam name="TArg3">The type of the first argument passed to the <paramref name="beginMethod" /> delegate.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="beginMethod" /> argument is null.-or-The <paramref name="endMethod" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="creationOptions" /> parameter specifies an invalid value.</exception>
	public Task<TResult> FromAsync<TArg1, TArg2, TArg3>(Func<TArg1, TArg2, TArg3, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg1 arg1, TArg2 arg2, TArg3 arg3, object state, TaskCreationOptions creationOptions)
	{
		return FromAsyncImpl(beginMethod, endMethod, null, arg1, arg2, arg3, state, creationOptions);
	}

	internal static Task<TResult> FromAsyncImpl<TArg1, TArg2, TArg3>(Func<TArg1, TArg2, TArg3, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endFunction, Action<IAsyncResult> endAction, TArg1 arg1, TArg2 arg2, TArg3 arg3, object state, TaskCreationOptions creationOptions)
	{
		if (beginMethod == null)
		{
			throw new ArgumentNullException("beginMethod");
		}
		if (endFunction == null && endAction == null)
		{
			throw new ArgumentNullException("endMethod");
		}
		TaskFactory.CheckFromAsyncOptions(creationOptions, hasBeginMethod: true);
		Task<TResult> promise = new Task<TResult>(state, creationOptions);
		if (AsyncCausalityTracer.LoggingOn)
		{
			AsyncCausalityTracer.TraceOperationCreation(CausalityTraceLevel.Required, promise.Id, "TaskFactory.FromAsync: " + beginMethod.Method.Name, 0uL);
		}
		if (Task.s_asyncDebuggingEnabled)
		{
			Task.AddToActiveTasks(promise);
		}
		try
		{
			if (BinaryCompatibility.TargetsAtLeast_Desktop_V4_5)
			{
				AtomicBoolean invoked = new AtomicBoolean();
				IAsyncResult asyncResult = beginMethod(arg1, arg2, arg3, delegate(IAsyncResult iar)
				{
					if (invoked.TryRelaxedSet())
					{
						FromAsyncCoreLogic(iar, endFunction, endAction, promise, requiresSynchronization: true);
					}
				}, state);
				if (asyncResult != null && asyncResult.CompletedSynchronously && invoked.TryRelaxedSet())
				{
					FromAsyncCoreLogic(asyncResult, endFunction, endAction, promise, requiresSynchronization: false);
				}
			}
			else
			{
				beginMethod(arg1, arg2, arg3, delegate(IAsyncResult iar)
				{
					FromAsyncCoreLogic(iar, endFunction, endAction, promise, requiresSynchronization: true);
				}, state);
			}
		}
		catch
		{
			if (AsyncCausalityTracer.LoggingOn)
			{
				AsyncCausalityTracer.TraceOperationCompletion(CausalityTraceLevel.Required, promise.Id, AsyncCausalityStatus.Error);
			}
			if (Task.s_asyncDebuggingEnabled)
			{
				Task.RemoveFromActiveTasks(promise.Id);
			}
			promise.TrySetResult(default(TResult));
			throw;
		}
		return promise;
	}

	internal static Task<TResult> FromAsyncTrim<TInstance, TArgs>(TInstance thisRef, TArgs args, Func<TInstance, TArgs, AsyncCallback, object, IAsyncResult> beginMethod, Func<TInstance, IAsyncResult, TResult> endMethod) where TInstance : class
	{
		FromAsyncTrimPromise<TInstance> fromAsyncTrimPromise = new FromAsyncTrimPromise<TInstance>(thisRef, endMethod);
		IAsyncResult asyncResult = beginMethod(thisRef, args, FromAsyncTrimPromise<TInstance>.s_completeFromAsyncResult, fromAsyncTrimPromise);
		if (asyncResult != null && asyncResult.CompletedSynchronously)
		{
			fromAsyncTrimPromise.Complete(thisRef, endMethod, asyncResult, requiresSynchronization: false);
		}
		return fromAsyncTrimPromise;
	}

	private static Task<TResult> CreateCanceledTask(TaskContinuationOptions continuationOptions, CancellationToken ct)
	{
		Task.CreationOptionsFromContinuationOptions(continuationOptions, out var creationOptions, out var _);
		return new Task<TResult>(canceled: true, default(TResult), creationOptions, ct);
	}

	/// <summary>Creates a continuation task that will be started upon the completion of a set of provided tasks.</summary>
	/// <returns>The new continuation task.</returns>
	/// <param name="tasks">The array of tasks from which to continue.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when all tasks in the <paramref name="tasks" /> array have completed.</param>
	/// <exception cref="T:System.ObjectDisposedException">One of the elements in the <paramref name="tasks" /> array has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="tasks" /> array is null.-or-The <paramref name="continuationFunction" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contains a null value or is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAll(Task[] tasks, Func<Task[], TResult> continuationFunction)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWhenAllImpl(tasks, continuationFunction, null, m_defaultContinuationOptions, m_defaultCancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation task that will be started upon the completion of a set of provided tasks.</summary>
	/// <returns>The new continuation task.</returns>
	/// <param name="tasks">The array of tasks from which to continue.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when all tasks in the <paramref name="tasks" /> array have completed.</param>
	/// <param name="cancellationToken">The cancellation token that will be assigned to the new continuation task.</param>
	/// <exception cref="T:System.ObjectDisposedException">One of the elements in the <paramref name="tasks" /> array has been disposed.-or-The <see cref="T:System.Threading.CancellationTokenSource" /> that created<paramref name=" cancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> array is null.-or-<paramref name="continuationFunction" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contains a null value or is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAll(Task[] tasks, Func<Task[], TResult> continuationFunction, CancellationToken cancellationToken)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWhenAllImpl(tasks, continuationFunction, null, m_defaultContinuationOptions, cancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation task that will be started upon the completion of a set of provided Tasks.</summary>
	/// <returns>The new continuation task.</returns>
	/// <param name="tasks">The array of tasks from which to continue.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when all tasks in the <paramref name="tasks" /> array have completed.</param>
	/// <param name="continuationOptions">One of the enumeration values that controls the behavior of the created continuation task. The NotOn* or OnlyOn* values are not valid.</param>
	/// <exception cref="T:System.ObjectDisposedException">One of the elements in the <paramref name="tasks" /> array has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> array is null.-or-The <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contains a null value or is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAll(Task[] tasks, Func<Task[], TResult> continuationFunction, TaskContinuationOptions continuationOptions)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWhenAllImpl(tasks, continuationFunction, null, continuationOptions, m_defaultCancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation task that will be started upon the completion of a set of provided Tasks.</summary>
	/// <returns>The new continuation task.</returns>
	/// <param name="tasks">The array of tasks from which to continue.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when all tasks in the <paramref name="tasks" /> array have completed.</param>
	/// <param name="cancellationToken">The cancellation token that will be assigned to the new continuation task.</param>
	/// <param name="continuationOptions">One of the enumeration values that controls the behavior of the created continuation task. The NotOn* or OnlyOn* values are not valid.</param>
	/// <param name="scheduler">The scheduler that is used to schedule the created continuation task.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> array is null.-or-The <paramref name="continuationFunction" /> argument is null.-or-The <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contains a null value or is empty.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="continuationOptions" /> specifies an invalid value.</exception>
	/// <exception cref="T:System.ObjectDisposedException">One of the elements in the <paramref name="tasks" /> array has been disposed.-or-The <see cref="T:System.Threading.CancellationTokenSource" /> that created<paramref name=" cancellationToken" /> has already been disposed.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAll(Task[] tasks, Func<Task[], TResult> continuationFunction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWhenAllImpl(tasks, continuationFunction, null, continuationOptions, cancellationToken, scheduler, ref stackMark);
	}

	/// <summary>Creates a continuation task that will be started upon the completion of a set of provided tasks.</summary>
	/// <returns>The new continuation task.</returns>
	/// <param name="tasks">The array of tasks from which to continue.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when all tasks in the <paramref name="tasks" /> array have completed.</param>
	/// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks" />.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">One of the elements in the <paramref name="tasks" /> array has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> array is null.-or-The <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contains a null value or is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAll<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>[], TResult> continuationFunction)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWhenAllImpl(tasks, continuationFunction, null, m_defaultContinuationOptions, m_defaultCancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation task that will be started upon the completion of a set of provided tasks.</summary>
	/// <returns>The new continuation task.</returns>
	/// <param name="tasks">The array of tasks from which to continue.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when all tasks in the <paramref name="tasks" /> array have completed.</param>
	/// <param name="cancellationToken">The cancellation token that will be assigned to the new continuation task.</param>
	/// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks" />.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">One of the elements in the <paramref name="tasks" /> array has been disposed.-or-The <see cref="T:System.Threading.CancellationTokenSource" /> that created<paramref name=" cancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> array is null.-or-The <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contains a null value or is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAll<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>[], TResult> continuationFunction, CancellationToken cancellationToken)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWhenAllImpl(tasks, continuationFunction, null, m_defaultContinuationOptions, cancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation task that will be started upon the completion of a set of provided tasks.</summary>
	/// <returns>The new continuation task.</returns>
	/// <param name="tasks">The array of tasks from which to continue.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when all tasks in the <paramref name="tasks" /> array have completed.</param>
	/// <param name="continuationOptions">One of the enumeration values that controls the behavior of the created continuation task. The NotOn* or OnlyOn* values are not valid.</param>
	/// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks" />.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">One of the elements in the <paramref name="tasks" /> array has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> array is null.-or-The <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contains a null value or is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAll<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>[], TResult> continuationFunction, TaskContinuationOptions continuationOptions)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWhenAllImpl(tasks, continuationFunction, null, continuationOptions, m_defaultCancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation task that will be started upon the completion of a set of provided tasks.</summary>
	/// <returns>The new continuation task.</returns>
	/// <param name="tasks">The array of tasks from which to continue.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when all tasks in the <paramref name="tasks" /> array have completed.</param>
	/// <param name="cancellationToken">The cancellation token that will be assigned to the new continuation task.</param>
	/// <param name="continuationOptions">One of the enumeration values that controls the behavior of the created continuation task. The NotOn* or OnlyOn* values are not valid.</param>
	/// <param name="scheduler">The scheduler that is used to schedule the created continuation task.</param>
	/// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> array is null.-or-The <paramref name="continuationFunction" /> argument is null.-or-The <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contains a null value or is empty.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value.</exception>
	/// <exception cref="T:System.ObjectDisposedException">One of the elements in the <paramref name="tasks" /> array has been disposed.-or-The <see cref="T:System.Threading.CancellationTokenSource" /> that created<paramref name=" cancellationToken" /> has already been disposed.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAll<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>[], TResult> continuationFunction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWhenAllImpl(tasks, continuationFunction, null, continuationOptions, cancellationToken, scheduler, ref stackMark);
	}

	internal static Task<TResult> ContinueWhenAllImpl<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>[], TResult> continuationFunction, Action<Task<TAntecedentResult>[]> continuationAction, TaskContinuationOptions continuationOptions, CancellationToken cancellationToken, TaskScheduler scheduler, ref StackCrawlMark stackMark)
	{
		TaskFactory.CheckMultiTaskContinuationOptions(continuationOptions);
		if (tasks == null)
		{
			throw new ArgumentNullException("tasks");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		Task<TAntecedentResult>[] tasksCopy = TaskFactory.CheckMultiContinuationTasksAndCopy(tasks);
		if (cancellationToken.IsCancellationRequested && (continuationOptions & TaskContinuationOptions.LazyCancellation) == 0)
		{
			return CreateCanceledTask(continuationOptions, cancellationToken);
		}
		Task<Task<TAntecedentResult>[]> task = TaskFactory.CommonCWAllLogic(tasksCopy);
		if (continuationFunction != null)
		{
			return task.ContinueWith(GenericDelegateCache<TAntecedentResult, TResult>.CWAllFuncDelegate, continuationFunction, scheduler, cancellationToken, continuationOptions, ref stackMark);
		}
		return task.ContinueWith(GenericDelegateCache<TAntecedentResult, TResult>.CWAllActionDelegate, continuationAction, scheduler, cancellationToken, continuationOptions, ref stackMark);
	}

	internal static Task<TResult> ContinueWhenAllImpl(Task[] tasks, Func<Task[], TResult> continuationFunction, Action<Task[]> continuationAction, TaskContinuationOptions continuationOptions, CancellationToken cancellationToken, TaskScheduler scheduler, ref StackCrawlMark stackMark)
	{
		TaskFactory.CheckMultiTaskContinuationOptions(continuationOptions);
		if (tasks == null)
		{
			throw new ArgumentNullException("tasks");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		Task[] tasksCopy = TaskFactory.CheckMultiContinuationTasksAndCopy(tasks);
		if (cancellationToken.IsCancellationRequested && (continuationOptions & TaskContinuationOptions.LazyCancellation) == 0)
		{
			return CreateCanceledTask(continuationOptions, cancellationToken);
		}
		Task<Task[]> task = TaskFactory.CommonCWAllLogic(tasksCopy);
		if (continuationFunction != null)
		{
			return task.ContinueWith(delegate(Task<Task[]> completedTasks, object state)
			{
				completedTasks.NotifyDebuggerOfWaitCompletionIfNecessary();
				return ((Func<Task[], TResult>)state)(completedTasks.Result);
			}, continuationFunction, scheduler, cancellationToken, continuationOptions, ref stackMark);
		}
		return task.ContinueWith(delegate(Task<Task[]> completedTasks, object state)
		{
			completedTasks.NotifyDebuggerOfWaitCompletionIfNecessary();
			((Action<Task[]>)state)(completedTasks.Result);
			return default(TResult);
		}, continuationAction, scheduler, cancellationToken, continuationOptions, ref stackMark);
	}

	/// <summary>Creates a continuation task that will be started upon the completion of any task in the provided set. </summary>
	/// <returns>The new continuation task.</returns>
	/// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when one task in the <paramref name="tasks" /> array completes.</param>
	/// <exception cref="T:System.ObjectDisposedException">One of the elements in the <paramref name="tasks" /> array has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> array is null.-or-The <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contains a null value or is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAny(Task[] tasks, Func<Task, TResult> continuationFunction)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWhenAnyImpl(tasks, continuationFunction, null, m_defaultContinuationOptions, m_defaultCancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation task that will be started upon the completion of any task in the provided set.</summary>
	/// <returns>The new continuation task.</returns>
	/// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when one task in the <paramref name="tasks" /> array completes.</param>
	/// <param name="cancellationToken">The cancellation token that will be assigned to the new continuation task.</param>
	/// <exception cref="T:System.ObjectDisposedException">One of the elements in the <paramref name="tasks" /> array has been disposed.-or-The <see cref="T:System.Threading.CancellationTokenSource" /> that created<paramref name=" cancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> array is null.-or-The <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contains a null value.-or-The <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAny(Task[] tasks, Func<Task, TResult> continuationFunction, CancellationToken cancellationToken)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWhenAnyImpl(tasks, continuationFunction, null, m_defaultContinuationOptions, cancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation task that will be started upon the completion of any task in the provided set.</summary>
	/// <returns>The new continuation task.</returns>
	/// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when one task in the <paramref name="tasks" /> array completes.</param>
	/// <param name="continuationOptions">One of the enumeration values that controls the behavior of the created continuation task. The NotOn* or OnlyOn* values are not valid.</param>
	/// <exception cref="T:System.ObjectDisposedException">One of the elements in the <paramref name="tasks" /> array has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> array is null.-or-The <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid enumeration value.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contains a null value.-or-The <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAny(Task[] tasks, Func<Task, TResult> continuationFunction, TaskContinuationOptions continuationOptions)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWhenAnyImpl(tasks, continuationFunction, null, continuationOptions, m_defaultCancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation task that will be started upon the completion of any task in the provided set.</summary>
	/// <returns>The new continuation task.</returns>
	/// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when one task in the <paramref name="tasks" /> array completes.</param>
	/// <param name="cancellationToken">The cancellation token that will be assigned to the new continuation task.</param>
	/// <param name="continuationOptions">One of the enumeration values that controls the behavior of the created continuation task. The NotOn* or OnlyOn* values are not valid.</param>
	/// <param name="scheduler">The task scheduler that is used to schedule the created continuation task.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> array is null.-or-The <paramref name="continuationFunction" /> argument is null.-or-The <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contains a null value.-or-The <paramref name="tasks" /> array is empty.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid <see cref="T:System.Threading.Tasks.TaskContinuationOptions" /> value.</exception>
	/// <exception cref="T:System.ObjectDisposedException">One of the elements in the <paramref name="tasks" /> array has been disposed.-or-The <see cref="T:System.Threading.CancellationTokenSource" /> that created<paramref name=" cancellationToken" /> has already been disposed. </exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAny(Task[] tasks, Func<Task, TResult> continuationFunction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWhenAnyImpl(tasks, continuationFunction, null, continuationOptions, cancellationToken, scheduler, ref stackMark);
	}

	/// <summary>Creates a continuation task that will be started upon the completion of any task in the provided set.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when one task in the <paramref name="tasks" /> array completes.</param>
	/// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks" />.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">One of the elements in the <paramref name="tasks" /> array has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> array is null.-or-The <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contains a null value.-or-The <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAny<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>, TResult> continuationFunction)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWhenAnyImpl(tasks, continuationFunction, null, m_defaultContinuationOptions, m_defaultCancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation task that will be started upon the completion of any task in the provided set.</summary>
	/// <returns>The new continuation task.</returns>
	/// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when one task in the <paramref name="tasks" /> array completes.</param>
	/// <param name="cancellationToken">The cancellation token that will be assigned to the new continuation task.</param>
	/// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks" />.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">One of the elements in the <paramref name="tasks" /> array has been disposed.-or-The <see cref="T:System.Threading.CancellationTokenSource" /> that created<paramref name=" cancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> array is null.-or-The <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contains a null value.-or-The <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAny<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>, TResult> continuationFunction, CancellationToken cancellationToken)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWhenAnyImpl(tasks, continuationFunction, null, m_defaultContinuationOptions, cancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation task that will be started upon the completion of any task in the provided set.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when one task in the <paramref name="tasks" /> array completes.</param>
	/// <param name="continuationOptions">One of the enumeration values that controls the behavior of the created continuation task. The NotOn* or OnlyOn* values are not valid.</param>
	/// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks" />.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">One of the elements in the <paramref name="tasks" /> array has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> array is null.-or-The <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid enumeration value.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contains a null value.-or-The <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAny<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>, TResult> continuationFunction, TaskContinuationOptions continuationOptions)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWhenAnyImpl(tasks, continuationFunction, null, continuationOptions, m_defaultCancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation task that will be started upon the completion of any task in the provided set.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when one task in the <paramref name="tasks" /> array completes.</param>
	/// <param name="cancellationToken">The cancellation token that will be assigned to the new continuation task.</param>
	/// <param name="continuationOptions">One of the enumeration values that controls the behavior of the created continuation task. The NotOn* or OnlyOn* values are not valid.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> that is used to schedule the created continuation <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> array is null.-or-The <paramref name="continuationFunction" /> argument is null.-or-The <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contains a null value.-or-The <paramref name="tasks" /> array is empty.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid TaskContinuationOptions value.</exception>
	/// <exception cref="T:System.ObjectDisposedException">One of the elements in the <paramref name="tasks" /> array has been disposed.-or-The <see cref="T:System.Threading.CancellationTokenSource" /> that created<paramref name=" cancellationToken" /> has already been disposed. </exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAny<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>, TResult> continuationFunction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWhenAnyImpl(tasks, continuationFunction, null, continuationOptions, cancellationToken, scheduler, ref stackMark);
	}

	internal static Task<TResult> ContinueWhenAnyImpl(Task[] tasks, Func<Task, TResult> continuationFunction, Action<Task> continuationAction, TaskContinuationOptions continuationOptions, CancellationToken cancellationToken, TaskScheduler scheduler, ref StackCrawlMark stackMark)
	{
		TaskFactory.CheckMultiTaskContinuationOptions(continuationOptions);
		if (tasks == null)
		{
			throw new ArgumentNullException("tasks");
		}
		if (tasks.Length == 0)
		{
			throw new ArgumentException(Environment.GetResourceString("The tasks argument contains no tasks."), "tasks");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		Task<Task> task = TaskFactory.CommonCWAnyLogic(tasks);
		if (cancellationToken.IsCancellationRequested && (continuationOptions & TaskContinuationOptions.LazyCancellation) == 0)
		{
			return CreateCanceledTask(continuationOptions, cancellationToken);
		}
		if (continuationFunction != null)
		{
			return task.ContinueWith((Task<Task> completedTask, object state) => ((Func<Task, TResult>)state)(completedTask.Result), continuationFunction, scheduler, cancellationToken, continuationOptions, ref stackMark);
		}
		return task.ContinueWith(delegate(Task<Task> completedTask, object state)
		{
			((Action<Task>)state)(completedTask.Result);
			return default(TResult);
		}, continuationAction, scheduler, cancellationToken, continuationOptions, ref stackMark);
	}

	internal static Task<TResult> ContinueWhenAnyImpl<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>, TResult> continuationFunction, Action<Task<TAntecedentResult>> continuationAction, TaskContinuationOptions continuationOptions, CancellationToken cancellationToken, TaskScheduler scheduler, ref StackCrawlMark stackMark)
	{
		TaskFactory.CheckMultiTaskContinuationOptions(continuationOptions);
		if (tasks == null)
		{
			throw new ArgumentNullException("tasks");
		}
		if (tasks.Length == 0)
		{
			throw new ArgumentException(Environment.GetResourceString("The tasks argument contains no tasks."), "tasks");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		Task<Task> task = TaskFactory.CommonCWAnyLogic(tasks);
		if (cancellationToken.IsCancellationRequested && (continuationOptions & TaskContinuationOptions.LazyCancellation) == 0)
		{
			return CreateCanceledTask(continuationOptions, cancellationToken);
		}
		if (continuationFunction != null)
		{
			return task.ContinueWith(GenericDelegateCache<TAntecedentResult, TResult>.CWAnyFuncDelegate, continuationFunction, scheduler, cancellationToken, continuationOptions, ref stackMark);
		}
		return task.ContinueWith(GenericDelegateCache<TAntecedentResult, TResult>.CWAnyActionDelegate, continuationAction, scheduler, cancellationToken, continuationOptions, ref stackMark);
	}
}
/// <summary>Provides support for creating and scheduling <see cref="T:System.Threading.Tasks.Task" /> objects.</summary>
[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
public class TaskFactory
{
	private sealed class CompleteOnCountdownPromise : Task<Task[]>, ITaskCompletionAction
	{
		private readonly Task[] _tasks;

		private int _count;

		internal override bool ShouldNotifyDebuggerOfWaitCompletion
		{
			get
			{
				if (base.ShouldNotifyDebuggerOfWaitCompletion)
				{
					return Task.AnyTaskRequiresNotifyDebuggerOfWaitCompletion(_tasks);
				}
				return false;
			}
		}

		internal CompleteOnCountdownPromise(Task[] tasksCopy)
		{
			_tasks = tasksCopy;
			_count = tasksCopy.Length;
			if (AsyncCausalityTracer.LoggingOn)
			{
				AsyncCausalityTracer.TraceOperationCreation(CausalityTraceLevel.Required, base.Id, "TaskFactory.ContinueWhenAll", 0uL);
			}
			if (Task.s_asyncDebuggingEnabled)
			{
				Task.AddToActiveTasks(this);
			}
		}

		public void Invoke(Task completingTask)
		{
			if (AsyncCausalityTracer.LoggingOn)
			{
				AsyncCausalityTracer.TraceOperationRelation(CausalityTraceLevel.Important, base.Id, CausalityRelation.Join);
			}
			if (completingTask.IsWaitNotificationEnabled)
			{
				SetNotificationForWaitCompletion(enabled: true);
			}
			if (Interlocked.Decrement(ref _count) == 0)
			{
				if (AsyncCausalityTracer.LoggingOn)
				{
					AsyncCausalityTracer.TraceOperationCompletion(CausalityTraceLevel.Required, base.Id, AsyncCausalityStatus.Completed);
				}
				if (Task.s_asyncDebuggingEnabled)
				{
					Task.RemoveFromActiveTasks(base.Id);
				}
				TrySetResult(_tasks);
			}
		}
	}

	private sealed class CompleteOnCountdownPromise<T> : Task<Task<T>[]>, ITaskCompletionAction
	{
		private readonly Task<T>[] _tasks;

		private int _count;

		internal override bool ShouldNotifyDebuggerOfWaitCompletion
		{
			get
			{
				if (base.ShouldNotifyDebuggerOfWaitCompletion)
				{
					return Task.AnyTaskRequiresNotifyDebuggerOfWaitCompletion(_tasks);
				}
				return false;
			}
		}

		internal CompleteOnCountdownPromise(Task<T>[] tasksCopy)
		{
			_tasks = tasksCopy;
			_count = tasksCopy.Length;
			if (AsyncCausalityTracer.LoggingOn)
			{
				AsyncCausalityTracer.TraceOperationCreation(CausalityTraceLevel.Required, base.Id, "TaskFactory.ContinueWhenAll<>", 0uL);
			}
			if (Task.s_asyncDebuggingEnabled)
			{
				Task.AddToActiveTasks(this);
			}
		}

		public void Invoke(Task completingTask)
		{
			if (AsyncCausalityTracer.LoggingOn)
			{
				AsyncCausalityTracer.TraceOperationRelation(CausalityTraceLevel.Important, base.Id, CausalityRelation.Join);
			}
			if (completingTask.IsWaitNotificationEnabled)
			{
				SetNotificationForWaitCompletion(enabled: true);
			}
			if (Interlocked.Decrement(ref _count) == 0)
			{
				if (AsyncCausalityTracer.LoggingOn)
				{
					AsyncCausalityTracer.TraceOperationCompletion(CausalityTraceLevel.Required, base.Id, AsyncCausalityStatus.Completed);
				}
				if (Task.s_asyncDebuggingEnabled)
				{
					Task.RemoveFromActiveTasks(base.Id);
				}
				TrySetResult(_tasks);
			}
		}
	}

	internal sealed class CompleteOnInvokePromise : Task<Task>, ITaskCompletionAction
	{
		private IList<Task> _tasks;

		private int m_firstTaskAlreadyCompleted;

		public CompleteOnInvokePromise(IList<Task> tasks)
		{
			_tasks = tasks;
			if (AsyncCausalityTracer.LoggingOn)
			{
				AsyncCausalityTracer.TraceOperationCreation(CausalityTraceLevel.Required, base.Id, "TaskFactory.ContinueWhenAny", 0uL);
			}
			if (Task.s_asyncDebuggingEnabled)
			{
				Task.AddToActiveTasks(this);
			}
		}

		public void Invoke(Task completingTask)
		{
			if (Interlocked.CompareExchange(ref m_firstTaskAlreadyCompleted, 1, 0) != 0)
			{
				return;
			}
			if (AsyncCausalityTracer.LoggingOn)
			{
				AsyncCausalityTracer.TraceOperationRelation(CausalityTraceLevel.Important, base.Id, CausalityRelation.Choice);
				AsyncCausalityTracer.TraceOperationCompletion(CausalityTraceLevel.Required, base.Id, AsyncCausalityStatus.Completed);
			}
			if (Task.s_asyncDebuggingEnabled)
			{
				Task.RemoveFromActiveTasks(base.Id);
			}
			TrySetResult(completingTask);
			IList<Task> tasks = _tasks;
			int count = tasks.Count;
			for (int i = 0; i < count; i++)
			{
				Task task = tasks[i];
				if (task != null && !task.IsCompleted)
				{
					task.RemoveContinuation(this);
				}
			}
			_tasks = null;
		}
	}

	private CancellationToken m_defaultCancellationToken;

	private TaskScheduler m_defaultScheduler;

	private TaskCreationOptions m_defaultCreationOptions;

	private TaskContinuationOptions m_defaultContinuationOptions;

	private TaskScheduler DefaultScheduler
	{
		get
		{
			if (m_defaultScheduler == null)
			{
				return TaskScheduler.Current;
			}
			return m_defaultScheduler;
		}
	}

	/// <summary>Gets the default cancellation token for this task factory.</summary>
	/// <returns>The default task cancellation token for this task factory.</returns>
	public CancellationToken CancellationToken => m_defaultCancellationToken;

	/// <summary>Gets the default task scheduler for this task factory.</summary>
	/// <returns>The default task scheduler for this task factory.</returns>
	public TaskScheduler Scheduler => m_defaultScheduler;

	/// <summary>Gets the default task creation options for this task factory.</summary>
	/// <returns>The default task creation options for this task factory.</returns>
	public TaskCreationOptions CreationOptions => m_defaultCreationOptions;

	/// <summary>Gets the default task continuation options for this task factory.</summary>
	/// <returns>The default task continuation options for this task factory.</returns>
	public TaskContinuationOptions ContinuationOptions => m_defaultContinuationOptions;

	private TaskScheduler GetDefaultScheduler(Task currTask)
	{
		if (m_defaultScheduler != null)
		{
			return m_defaultScheduler;
		}
		if (currTask != null && (currTask.CreationOptions & TaskCreationOptions.HideScheduler) == 0)
		{
			return currTask.ExecutingTaskScheduler;
		}
		return TaskScheduler.Default;
	}

	/// <summary>Initializes a <see cref="T:System.Threading.Tasks.TaskFactory" /> instance with the default configuration.</summary>
	public TaskFactory()
		: this(default(CancellationToken), TaskCreationOptions.None, TaskContinuationOptions.None, null)
	{
	}

	/// <summary>Initializes a <see cref="T:System.Threading.Tasks.TaskFactory" /> instance with the specified configuration.</summary>
	/// <param name="cancellationToken">The <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> that will be assigned to tasks created by this <see cref="T:System.Threading.Tasks.TaskFactory" /> unless another CancellationToken is explicitly specified while calling the factory methods.</param>
	public TaskFactory(CancellationToken cancellationToken)
		: this(cancellationToken, TaskCreationOptions.None, TaskContinuationOptions.None, null)
	{
	}

	/// <summary>Initializes a <see cref="T:System.Threading.Tasks.TaskFactory" /> instance with the specified configuration.</summary>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to use to schedule any tasks created with this TaskFactory. A null value indicates that the current TaskScheduler should be used.</param>
	public TaskFactory(TaskScheduler scheduler)
		: this(default(CancellationToken), TaskCreationOptions.None, TaskContinuationOptions.None, scheduler)
	{
	}

	/// <summary>Initializes a <see cref="T:System.Threading.Tasks.TaskFactory" /> instance with the specified configuration.</summary>
	/// <param name="creationOptions">The default <see cref="T:System.Threading.Tasks.TaskCreationOptions" /> to use when creating tasks with this TaskFactory.</param>
	/// <param name="continuationOptions">The default <see cref="T:System.Threading.Tasks.TaskContinuationOptions" /> to use when creating continuation tasks with this TaskFactory.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="creationOptions" /> argument or the <paramref name="continuationOptions" /> argument specifies an invalid value. The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. For more information, see the Remarks for <see cref="M:System.Threading.Tasks.TaskFactory.FromAsync(System.Func{System.AsyncCallback,System.Object,System.IAsyncResult},System.Action{System.IAsyncResult},System.Object,System.Threading.Tasks.TaskCreationOptions)" /></exception>
	public TaskFactory(TaskCreationOptions creationOptions, TaskContinuationOptions continuationOptions)
		: this(default(CancellationToken), creationOptions, continuationOptions, null)
	{
	}

	/// <summary>Initializes a <see cref="T:System.Threading.Tasks.TaskFactory" /> instance with the specified configuration.</summary>
	/// <param name="cancellationToken">The default <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> that will be assigned to tasks created by this <see cref="T:System.Threading.Tasks.TaskFactory" /> unless another CancellationToken is explicitly specified while calling the factory methods.</param>
	/// <param name="creationOptions">The default <see cref="T:System.Threading.Tasks.TaskCreationOptions" /> to use when creating tasks with this TaskFactory.</param>
	/// <param name="continuationOptions">The default <see cref="T:System.Threading.Tasks.TaskContinuationOptions" /> to use when creating continuation tasks with this TaskFactory.</param>
	/// <param name="scheduler">The default <see cref="T:System.Threading.Tasks.TaskScheduler" /> to use to schedule any Tasks created with this TaskFactory. A null value indicates that TaskScheduler.Current should be used.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="creationOptions" /> argument or the <paramref name="continuationOptions" /> argument specifies an invalid value. The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. For more information, see the Remarks for <see cref="M:System.Threading.Tasks.TaskFactory.FromAsync(System.Func{System.AsyncCallback,System.Object,System.IAsyncResult},System.Action{System.IAsyncResult},System.Object,System.Threading.Tasks.TaskCreationOptions)" /></exception>
	public TaskFactory(CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		CheckMultiTaskContinuationOptions(continuationOptions);
		CheckCreationOptions(creationOptions);
		m_defaultCancellationToken = cancellationToken;
		m_defaultScheduler = scheduler;
		m_defaultCreationOptions = creationOptions;
		m_defaultContinuationOptions = continuationOptions;
	}

	internal static void CheckCreationOptions(TaskCreationOptions creationOptions)
	{
		if ((creationOptions & ~(TaskCreationOptions.PreferFairness | TaskCreationOptions.LongRunning | TaskCreationOptions.AttachedToParent | TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler | TaskCreationOptions.RunContinuationsAsynchronously)) != 0)
		{
			throw new ArgumentOutOfRangeException("creationOptions");
		}
	}

	/// <summary>Creates and starts a task.</summary>
	/// <returns>The started task.</returns>
	/// <param name="action">The action delegate to execute asynchronously.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="action" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task StartNew(Action action)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		Task internalCurrent = Task.InternalCurrent;
		return Task.InternalStartNew(internalCurrent, action, null, m_defaultCancellationToken, GetDefaultScheduler(internalCurrent), m_defaultCreationOptions, InternalTaskOptions.None, ref stackMark);
	}

	/// <summary>Creates and starts a <see cref="T:System.Threading.Tasks.Task" />.</summary>
	/// <returns>The started <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="action">The action delegate to execute asynchronously.</param>
	/// <param name="cancellationToken">The <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> that will be assigned to the new task.</param>
	/// <exception cref="T:System.ObjectDisposedException">The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="action" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task StartNew(Action action, CancellationToken cancellationToken)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		Task internalCurrent = Task.InternalCurrent;
		return Task.InternalStartNew(internalCurrent, action, null, cancellationToken, GetDefaultScheduler(internalCurrent), m_defaultCreationOptions, InternalTaskOptions.None, ref stackMark);
	}

	/// <summary>Creates and starts a <see cref="T:System.Threading.Tasks.Task" />.</summary>
	/// <returns>The started <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="action">The action delegate to execute asynchronously.</param>
	/// <param name="creationOptions">A TaskCreationOptions value that controls the behavior of the created <see cref="T:System.Threading.Tasks.Task" /></param>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="action" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task StartNew(Action action, TaskCreationOptions creationOptions)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		Task internalCurrent = Task.InternalCurrent;
		return Task.InternalStartNew(internalCurrent, action, null, m_defaultCancellationToken, GetDefaultScheduler(internalCurrent), creationOptions, InternalTaskOptions.None, ref stackMark);
	}

	/// <summary>Creates and starts a <see cref="T:System.Threading.Tasks.Task" />.</summary>
	/// <returns>The started <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="action">The action delegate to execute asynchronously.</param>
	/// <param name="cancellationToken">The <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> that will be assigned to the new <see cref="T:System.Threading.Tasks.Task" /></param>
	/// <param name="creationOptions">A TaskCreationOptions value that controls the behavior of the created <see cref="T:System.Threading.Tasks.Task" /></param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> that is used to schedule the created <see cref="T:System.Threading.Tasks.Task" />.</param>
	/// <exception cref="T:System.ObjectDisposedException">The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="action" /> argument is null.-or-The exception that is thrown when the <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. For more information, see the Remarks for <see cref="M:System.Threading.Tasks.TaskFactory.FromAsync(System.Func{System.AsyncCallback,System.Object,System.IAsyncResult},System.Action{System.IAsyncResult},System.Object,System.Threading.Tasks.TaskCreationOptions)" /></exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task StartNew(Action action, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return Task.InternalStartNew(Task.InternalCurrentIfAttached(creationOptions), action, null, cancellationToken, scheduler, creationOptions, InternalTaskOptions.None, ref stackMark);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal Task StartNew(Action action, CancellationToken cancellationToken, TaskCreationOptions creationOptions, InternalTaskOptions internalOptions, TaskScheduler scheduler)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return Task.InternalStartNew(Task.InternalCurrentIfAttached(creationOptions), action, null, cancellationToken, scheduler, creationOptions, internalOptions, ref stackMark);
	}

	/// <summary>Creates and starts a <see cref="T:System.Threading.Tasks.Task" />.</summary>
	/// <returns>The started <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="action">The action delegate to execute asynchronously.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="action" /> delegate.</param>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="action" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task StartNew(Action<object> action, object state)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		Task internalCurrent = Task.InternalCurrent;
		return Task.InternalStartNew(internalCurrent, action, state, m_defaultCancellationToken, GetDefaultScheduler(internalCurrent), m_defaultCreationOptions, InternalTaskOptions.None, ref stackMark);
	}

	/// <summary>Creates and starts a <see cref="T:System.Threading.Tasks.Task" />.</summary>
	/// <returns>The started <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="action">The action delegate to execute asynchronously.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="action" /> delegate.</param>
	/// <param name="cancellationToken">The <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> that will be assigned to the new <see cref="T:System.Threading.Tasks.Task" /></param>
	/// <exception cref="T:System.ObjectDisposedException">The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="action" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task StartNew(Action<object> action, object state, CancellationToken cancellationToken)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		Task internalCurrent = Task.InternalCurrent;
		return Task.InternalStartNew(internalCurrent, action, state, cancellationToken, GetDefaultScheduler(internalCurrent), m_defaultCreationOptions, InternalTaskOptions.None, ref stackMark);
	}

	/// <summary>Creates and starts a <see cref="T:System.Threading.Tasks.Task" />.</summary>
	/// <returns>The started <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="action">The action delegate to execute asynchronously.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="action" /> delegate.</param>
	/// <param name="creationOptions">A TaskCreationOptions value that controls the behavior of the created <see cref="T:System.Threading.Tasks.Task" /></param>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="action" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task StartNew(Action<object> action, object state, TaskCreationOptions creationOptions)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		Task internalCurrent = Task.InternalCurrent;
		return Task.InternalStartNew(internalCurrent, action, state, m_defaultCancellationToken, GetDefaultScheduler(internalCurrent), creationOptions, InternalTaskOptions.None, ref stackMark);
	}

	/// <summary>Creates and starts a <see cref="T:System.Threading.Tasks.Task" />.</summary>
	/// <returns>The started <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="action">The action delegate to execute asynchronously.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="action" /> delegate.</param>
	/// <param name="cancellationToken">The <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> that will be assigned to the new task.</param>
	/// <param name="creationOptions">A TaskCreationOptions value that controls the behavior of the created <see cref="T:System.Threading.Tasks.Task" /></param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> that is used to schedule the created <see cref="T:System.Threading.Tasks.Task" />.</param>
	/// <exception cref="T:System.ObjectDisposedException">The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="action" /> argument is null.-or-The exception that is thrown when the <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. For more information, see the Remarks for <see cref="M:System.Threading.Tasks.TaskFactory.FromAsync(System.Func{System.AsyncCallback,System.Object,System.IAsyncResult},System.Action{System.IAsyncResult},System.Object,System.Threading.Tasks.TaskCreationOptions)" /></exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task StartNew(Action<object> action, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return Task.InternalStartNew(Task.InternalCurrentIfAttached(creationOptions), action, state, cancellationToken, scheduler, creationOptions, InternalTaskOptions.None, ref stackMark);
	}

	/// <summary>Creates and starts a <see cref="T:System.Threading.Tasks.Task`1" />.</summary>
	/// <returns>The started <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="function">A function delegate that returns the future result to be available through the <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <typeparam name="TResult">The type of the result available through the <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="function" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> StartNew<TResult>(Func<TResult> function)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		Task internalCurrent = Task.InternalCurrent;
		return Task<TResult>.StartNew(internalCurrent, function, m_defaultCancellationToken, m_defaultCreationOptions, InternalTaskOptions.None, GetDefaultScheduler(internalCurrent), ref stackMark);
	}

	/// <summary>Creates and starts a <see cref="T:System.Threading.Tasks.Task`1" />.</summary>
	/// <returns>The started <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="function">A function delegate that returns the future result to be available through the <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <param name="cancellationToken">The <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> that will be assigned to the new <see cref="T:System.Threading.Tasks.Task" /></param>
	/// <typeparam name="TResult">The type of the result available through the <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="function" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> StartNew<TResult>(Func<TResult> function, CancellationToken cancellationToken)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		Task internalCurrent = Task.InternalCurrent;
		return Task<TResult>.StartNew(internalCurrent, function, cancellationToken, m_defaultCreationOptions, InternalTaskOptions.None, GetDefaultScheduler(internalCurrent), ref stackMark);
	}

	/// <summary>Creates and starts a <see cref="T:System.Threading.Tasks.Task`1" />.</summary>
	/// <returns>The started <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="function">A function delegate that returns the future result to be available through the <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <param name="creationOptions">A TaskCreationOptions value that controls the behavior of the created <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <typeparam name="TResult">The type of the result available through the <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="function" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. For more information, see the Remarks for <see cref="M:System.Threading.Tasks.TaskFactory.FromAsync(System.Func{System.AsyncCallback,System.Object,System.IAsyncResult},System.Action{System.IAsyncResult},System.Object,System.Threading.Tasks.TaskCreationOptions)" /></exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> StartNew<TResult>(Func<TResult> function, TaskCreationOptions creationOptions)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		Task internalCurrent = Task.InternalCurrent;
		return Task<TResult>.StartNew(internalCurrent, function, m_defaultCancellationToken, creationOptions, InternalTaskOptions.None, GetDefaultScheduler(internalCurrent), ref stackMark);
	}

	/// <summary>Creates and starts a <see cref="T:System.Threading.Tasks.Task`1" />.</summary>
	/// <returns>The started <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="function">A function delegate that returns the future result to be available through the <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <param name="cancellationToken">The <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> that will be assigned to the new task.</param>
	/// <param name="creationOptions">A TaskCreationOptions value that controls the behavior of the created <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> that is used to schedule the created <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <typeparam name="TResult">The type of the result available through the <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="function" /> argument is null.-or-The exception that is thrown when the <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. For more information, see the Remarks for <see cref="M:System.Threading.Tasks.TaskFactory.FromAsync(System.Func{System.AsyncCallback,System.Object,System.IAsyncResult},System.Action{System.IAsyncResult},System.Object,System.Threading.Tasks.TaskCreationOptions)" /></exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> StartNew<TResult>(Func<TResult> function, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return Task<TResult>.StartNew(Task.InternalCurrentIfAttached(creationOptions), function, cancellationToken, creationOptions, InternalTaskOptions.None, scheduler, ref stackMark);
	}

	/// <summary>Creates and starts a <see cref="T:System.Threading.Tasks.Task`1" />.</summary>
	/// <returns>The started <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="function">A function delegate that returns the future result to be available through the <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="function" /> delegate.</param>
	/// <typeparam name="TResult">The type of the result available through the <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="function" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> StartNew<TResult>(Func<object, TResult> function, object state)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		Task internalCurrent = Task.InternalCurrent;
		return Task<TResult>.StartNew(internalCurrent, function, state, m_defaultCancellationToken, m_defaultCreationOptions, InternalTaskOptions.None, GetDefaultScheduler(internalCurrent), ref stackMark);
	}

	/// <summary>Creates and starts a <see cref="T:System.Threading.Tasks.Task`1" />.</summary>
	/// <returns>The started <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="function">A function delegate that returns the future result to be available through the <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="function" /> delegate.</param>
	/// <param name="cancellationToken">The <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> that will be assigned to the new <see cref="T:System.Threading.Tasks.Task" /></param>
	/// <typeparam name="TResult">The type of the result available through the <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="function" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> StartNew<TResult>(Func<object, TResult> function, object state, CancellationToken cancellationToken)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		Task internalCurrent = Task.InternalCurrent;
		return Task<TResult>.StartNew(internalCurrent, function, state, cancellationToken, m_defaultCreationOptions, InternalTaskOptions.None, GetDefaultScheduler(internalCurrent), ref stackMark);
	}

	/// <summary>Creates and starts a <see cref="T:System.Threading.Tasks.Task`1" />.</summary>
	/// <returns>The started <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="function">A function delegate that returns the future result to be available through the <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="function" /> delegate.</param>
	/// <param name="creationOptions">A TaskCreationOptions value that controls the behavior of the created <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <typeparam name="TResult">The type of the result available through the <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="function" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. For more information, see the Remarks for <see cref="M:System.Threading.Tasks.TaskFactory.FromAsync(System.Func{System.AsyncCallback,System.Object,System.IAsyncResult},System.Action{System.IAsyncResult},System.Object,System.Threading.Tasks.TaskCreationOptions)" /></exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> StartNew<TResult>(Func<object, TResult> function, object state, TaskCreationOptions creationOptions)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		Task internalCurrent = Task.InternalCurrent;
		return Task<TResult>.StartNew(internalCurrent, function, state, m_defaultCancellationToken, creationOptions, InternalTaskOptions.None, GetDefaultScheduler(internalCurrent), ref stackMark);
	}

	/// <summary>Creates and starts a <see cref="T:System.Threading.Tasks.Task`1" />.</summary>
	/// <returns>The started <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="function">A function delegate that returns the future result to be available through the <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="function" /> delegate.</param>
	/// <param name="cancellationToken">The <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> that will be assigned to the new task.</param>
	/// <param name="creationOptions">A TaskCreationOptions value that controls the behavior of the created <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> that is used to schedule the created <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <typeparam name="TResult">The type of the result available through the <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="function" /> argument is null.-or-The exception that is thrown when the <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. For more information, see the Remarks for <see cref="M:System.Threading.Tasks.TaskFactory.FromAsync(System.Func{System.AsyncCallback,System.Object,System.IAsyncResult},System.Action{System.IAsyncResult},System.Object,System.Threading.Tasks.TaskCreationOptions)" /></exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> StartNew<TResult>(Func<object, TResult> function, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return Task<TResult>.StartNew(Task.InternalCurrentIfAttached(creationOptions), function, state, cancellationToken, creationOptions, InternalTaskOptions.None, scheduler, ref stackMark);
	}

	/// <summary>Creates a <see cref="T:System.Threading.Tasks.Task" /> that executes an end method action when a specified <see cref="T:System.IAsyncResult" /> completes.</summary>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <param name="asyncResult">The IAsyncResult whose completion should trigger the processing of the <paramref name="endMethod" />.</param>
	/// <param name="endMethod">The action delegate that processes the completed <paramref name="asyncResult" />.</param>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="asyncResult" /> argument is null.-or-The exception that is thrown when the <paramref name="endMethod" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task FromAsync(IAsyncResult asyncResult, Action<IAsyncResult> endMethod)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return FromAsync(asyncResult, endMethod, m_defaultCreationOptions, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a <see cref="T:System.Threading.Tasks.Task" /> that executes an end method action when a specified <see cref="T:System.IAsyncResult" /> completes.</summary>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <param name="asyncResult">The IAsyncResult whose completion should trigger the processing of the <paramref name="endMethod" />.</param>
	/// <param name="endMethod">The action delegate that processes the completed <paramref name="asyncResult" />.</param>
	/// <param name="creationOptions">The TaskCreationOptions value that controls the behavior of the created <see cref="T:System.Threading.Tasks.Task" />.</param>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="asyncResult" /> argument is null.-or-The exception that is thrown when the <paramref name="endMethod" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. For more information, see the Remarks for <see cref="M:System.Threading.Tasks.TaskFactory.FromAsync(System.Func{System.AsyncCallback,System.Object,System.IAsyncResult},System.Action{System.IAsyncResult},System.Object,System.Threading.Tasks.TaskCreationOptions)" /></exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task FromAsync(IAsyncResult asyncResult, Action<IAsyncResult> endMethod, TaskCreationOptions creationOptions)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return FromAsync(asyncResult, endMethod, creationOptions, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a <see cref="T:System.Threading.Tasks.Task" /> that executes an end method action when a specified <see cref="T:System.IAsyncResult" /> completes.</summary>
	/// <returns>The created <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <param name="asyncResult">The IAsyncResult whose completion should trigger the processing of the <paramref name="endMethod" />.</param>
	/// <param name="endMethod">The action delegate that processes the completed <paramref name="asyncResult" />.</param>
	/// <param name="creationOptions">The TaskCreationOptions value that controls the behavior of the created <see cref="T:System.Threading.Tasks.Task" />.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> that is used to schedule the task that executes the end method.</param>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="asyncResult" /> argument is null.-or-The exception that is thrown when the <paramref name="endMethod" /> argument is null.-or-The exception that is thrown when the <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. For more information, see the Remarks for <see cref="M:System.Threading.Tasks.TaskFactory.FromAsync(System.Func{System.AsyncCallback,System.Object,System.IAsyncResult},System.Action{System.IAsyncResult},System.Object,System.Threading.Tasks.TaskCreationOptions)" /></exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task FromAsync(IAsyncResult asyncResult, Action<IAsyncResult> endMethod, TaskCreationOptions creationOptions, TaskScheduler scheduler)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return FromAsync(asyncResult, endMethod, creationOptions, scheduler, ref stackMark);
	}

	private Task FromAsync(IAsyncResult asyncResult, Action<IAsyncResult> endMethod, TaskCreationOptions creationOptions, TaskScheduler scheduler, ref StackCrawlMark stackMark)
	{
		return TaskFactory<VoidTaskResult>.FromAsyncImpl(asyncResult, null, endMethod, creationOptions, scheduler, ref stackMark);
	}

	/// <summary>Creates a <see cref="T:System.Threading.Tasks.Task" /> that represents a pair of begin and end methods that conform to the Asynchronous Programming Model pattern.</summary>
	/// <returns>The created <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <param name="beginMethod">The delegate that begins the asynchronous operation.</param>
	/// <param name="endMethod">The delegate that ends the asynchronous operation.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="beginMethod" /> delegate.</param>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="beginMethod" /> argument is null.-or-The exception that is thrown when the <paramref name="endMethod" /> argument is null.</exception>
	public Task FromAsync(Func<AsyncCallback, object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod, object state)
	{
		return FromAsync(beginMethod, endMethod, state, m_defaultCreationOptions);
	}

	/// <summary>Creates a <see cref="T:System.Threading.Tasks.Task" /> that represents a pair of begin and end methods that conform to the Asynchronous Programming Model pattern.</summary>
	/// <returns>The created <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <param name="beginMethod">The delegate that begins the asynchronous operation.</param>
	/// <param name="endMethod">The delegate that ends the asynchronous operation.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="creationOptions">The TaskCreationOptions value that controls the behavior of the created <see cref="T:System.Threading.Tasks.Task" />.</param>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="beginMethod" /> argument is null.-or-The exception that is thrown when the <paramref name="endMethod" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value.</exception>
	public Task FromAsync(Func<AsyncCallback, object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod, object state, TaskCreationOptions creationOptions)
	{
		return TaskFactory<VoidTaskResult>.FromAsyncImpl(beginMethod, null, endMethod, state, creationOptions);
	}

	/// <summary>Creates a <see cref="T:System.Threading.Tasks.Task" /> that represents a pair of begin and end methods that conform to the Asynchronous Programming Model pattern.</summary>
	/// <returns>The created <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <param name="beginMethod">The delegate that begins the asynchronous operation.</param>
	/// <param name="endMethod">The delegate that ends the asynchronous operation.</param>
	/// <param name="arg1">The first argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="beginMethod" /> delegate.</param>
	/// <typeparam name="TArg1">The type of the first argument passed to the <paramref name="beginMethod" /> delegate.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="beginMethod" /> argument is null.-or-The exception that is thrown when the <paramref name="endMethod" /> argument is null.</exception>
	public Task FromAsync<TArg1>(Func<TArg1, AsyncCallback, object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod, TArg1 arg1, object state)
	{
		return FromAsync(beginMethod, endMethod, arg1, state, m_defaultCreationOptions);
	}

	/// <summary>Creates a <see cref="T:System.Threading.Tasks.Task" /> that represents a pair of begin and end methods that conform to the Asynchronous Programming Model pattern.</summary>
	/// <returns>The created <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <param name="beginMethod">The delegate that begins the asynchronous operation.</param>
	/// <param name="endMethod">The delegate that ends the asynchronous operation.</param>
	/// <param name="arg1">The first argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="creationOptions">The TaskCreationOptions value that controls the behavior of the created <see cref="T:System.Threading.Tasks.Task" />.</param>
	/// <typeparam name="TArg1">The type of the first argument passed to the <paramref name="beginMethod" /> delegate.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="beginMethod" /> argument is null.-or-The exception that is thrown when the <paramref name="endMethod" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. For more information, see the Remarks for <see cref="M:System.Threading.Tasks.TaskFactory.FromAsync(System.Func{System.AsyncCallback,System.Object,System.IAsyncResult},System.Action{System.IAsyncResult},System.Object,System.Threading.Tasks.TaskCreationOptions)" /></exception>
	public Task FromAsync<TArg1>(Func<TArg1, AsyncCallback, object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod, TArg1 arg1, object state, TaskCreationOptions creationOptions)
	{
		return TaskFactory<VoidTaskResult>.FromAsyncImpl(beginMethod, null, endMethod, arg1, state, creationOptions);
	}

	/// <summary>Creates a <see cref="T:System.Threading.Tasks.Task" /> that represents a pair of begin and end methods that conform to the Asynchronous Programming Model pattern.</summary>
	/// <returns>The created <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <param name="beginMethod">The delegate that begins the asynchronous operation.</param>
	/// <param name="endMethod">The delegate that ends the asynchronous operation.</param>
	/// <param name="arg1">The first argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="arg2">The second argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="beginMethod" /> delegate.</param>
	/// <typeparam name="TArg1">The type of the second argument passed to <paramref name="beginMethod" /> delegate.</typeparam>
	/// <typeparam name="TArg2">The type of the first argument passed to the <paramref name="beginMethod" /> delegate.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="beginMethod" /> argument is null.-or-The exception that is thrown when the <paramref name="endMethod" /> argument is null.</exception>
	public Task FromAsync<TArg1, TArg2>(Func<TArg1, TArg2, AsyncCallback, object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod, TArg1 arg1, TArg2 arg2, object state)
	{
		return FromAsync(beginMethod, endMethod, arg1, arg2, state, m_defaultCreationOptions);
	}

	/// <summary>Creates a <see cref="T:System.Threading.Tasks.Task" /> that represents a pair of begin and end methods that conform to the Asynchronous Programming Model pattern.</summary>
	/// <returns>The created <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <param name="beginMethod">The delegate that begins the asynchronous operation.</param>
	/// <param name="endMethod">The delegate that ends the asynchronous operation.</param>
	/// <param name="arg1">The first argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="arg2">The second argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="creationOptions">The TaskCreationOptions value that controls the behavior of the created <see cref="T:System.Threading.Tasks.Task" />.</param>
	/// <typeparam name="TArg1">The type of the second argument passed to <paramref name="beginMethod" /> delegate.</typeparam>
	/// <typeparam name="TArg2">The type of the first argument passed to the <paramref name="beginMethod" /> delegate.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="beginMethod" /> argument is null.-or-The exception that is thrown when the <paramref name="endMethod" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. For more information, see the Remarks for <see cref="M:System.Threading.Tasks.TaskFactory.FromAsync(System.Func{System.AsyncCallback,System.Object,System.IAsyncResult},System.Action{System.IAsyncResult},System.Object,System.Threading.Tasks.TaskCreationOptions)" /></exception>
	public Task FromAsync<TArg1, TArg2>(Func<TArg1, TArg2, AsyncCallback, object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod, TArg1 arg1, TArg2 arg2, object state, TaskCreationOptions creationOptions)
	{
		return TaskFactory<VoidTaskResult>.FromAsyncImpl(beginMethod, null, endMethod, arg1, arg2, state, creationOptions);
	}

	/// <summary>Creates a <see cref="T:System.Threading.Tasks.Task" /> that represents a pair of begin and end methods that conform to the Asynchronous Programming Model pattern.</summary>
	/// <returns>The created <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <param name="beginMethod">The delegate that begins the asynchronous operation.</param>
	/// <param name="endMethod">The delegate that ends the asynchronous operation.</param>
	/// <param name="arg1">The first argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="arg2">The second argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="arg3">The third argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="beginMethod" /> delegate.</param>
	/// <typeparam name="TArg1">The type of the second argument passed to <paramref name="beginMethod" /> delegate.</typeparam>
	/// <typeparam name="TArg2">The type of the third argument passed to <paramref name="beginMethod" /> delegate.</typeparam>
	/// <typeparam name="TArg3">The type of the first argument passed to the <paramref name="beginMethod" /> delegate.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="beginMethod" /> argument is null.-or-The exception that is thrown when the <paramref name="endMethod" /> argument is null.</exception>
	public Task FromAsync<TArg1, TArg2, TArg3>(Func<TArg1, TArg2, TArg3, AsyncCallback, object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod, TArg1 arg1, TArg2 arg2, TArg3 arg3, object state)
	{
		return FromAsync(beginMethod, endMethod, arg1, arg2, arg3, state, m_defaultCreationOptions);
	}

	/// <summary>Creates a <see cref="T:System.Threading.Tasks.Task" /> that represents a pair of begin and end methods that conform to the Asynchronous Programming Model pattern.</summary>
	/// <returns>The created <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <param name="beginMethod">The delegate that begins the asynchronous operation.</param>
	/// <param name="endMethod">The delegate that ends the asynchronous operation.</param>
	/// <param name="arg1">The first argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="arg2">The second argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="arg3">The third argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="creationOptions">The TaskCreationOptions value that controls the behavior of the created <see cref="T:System.Threading.Tasks.Task" />.</param>
	/// <typeparam name="TArg1">The type of the second argument passed to <paramref name="beginMethod" /> delegate.</typeparam>
	/// <typeparam name="TArg2">The type of the third argument passed to <paramref name="beginMethod" /> delegate.</typeparam>
	/// <typeparam name="TArg3">The type of the first argument passed to the <paramref name="beginMethod" /> delegate.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="beginMethod" /> argument is null.-or-The exception that is thrown when the <paramref name="endMethod" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. For more information, see the Remarks for <see cref="M:System.Threading.Tasks.TaskFactory.FromAsync(System.Func{System.AsyncCallback,System.Object,System.IAsyncResult},System.Action{System.IAsyncResult},System.Object,System.Threading.Tasks.TaskCreationOptions)" /></exception>
	public Task FromAsync<TArg1, TArg2, TArg3>(Func<TArg1, TArg2, TArg3, AsyncCallback, object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod, TArg1 arg1, TArg2 arg2, TArg3 arg3, object state, TaskCreationOptions creationOptions)
	{
		return TaskFactory<VoidTaskResult>.FromAsyncImpl(beginMethod, null, endMethod, arg1, arg2, arg3, state, creationOptions);
	}

	/// <summary>Creates a <see cref="T:System.Threading.Tasks.Task`1" /> that executes an end method function when a specified <see cref="T:System.IAsyncResult" /> completes.</summary>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous operation.</returns>
	/// <param name="asyncResult">The IAsyncResult whose completion should trigger the processing of the <paramref name="endMethod" />.</param>
	/// <param name="endMethod">The function delegate that processes the completed <paramref name="asyncResult" />.</param>
	/// <typeparam name="TResult">The type of the result available through the <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="asyncResult" /> argument is null.-or-The exception that is thrown when the <paramref name="endMethod" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> FromAsync<TResult>(IAsyncResult asyncResult, Func<IAsyncResult, TResult> endMethod)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<TResult>.FromAsyncImpl(asyncResult, endMethod, null, m_defaultCreationOptions, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a <see cref="T:System.Threading.Tasks.Task`1" /> that executes an end method function when a specified <see cref="T:System.IAsyncResult" /> completes.</summary>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous operation.</returns>
	/// <param name="asyncResult">The IAsyncResult whose completion should trigger the processing of the <paramref name="endMethod" />.</param>
	/// <param name="endMethod">The function delegate that processes the completed <paramref name="asyncResult" />.</param>
	/// <param name="creationOptions">The TaskCreationOptions value that controls the behavior of the created <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <typeparam name="TResult">The type of the result available through the <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="asyncResult" /> argument is null.-or-The exception that is thrown when the <paramref name="endMethod" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. For more information, see the Remarks for <see cref="M:System.Threading.Tasks.TaskFactory.FromAsync(System.Func{System.AsyncCallback,System.Object,System.IAsyncResult},System.Action{System.IAsyncResult},System.Object,System.Threading.Tasks.TaskCreationOptions)" /></exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> FromAsync<TResult>(IAsyncResult asyncResult, Func<IAsyncResult, TResult> endMethod, TaskCreationOptions creationOptions)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<TResult>.FromAsyncImpl(asyncResult, endMethod, null, creationOptions, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a <see cref="T:System.Threading.Tasks.Task`1" /> that executes an end method function when a specified <see cref="T:System.IAsyncResult" /> completes.</summary>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous operation.</returns>
	/// <param name="asyncResult">The IAsyncResult whose completion should trigger the processing of the <paramref name="endMethod" />.</param>
	/// <param name="endMethod">The function delegate that processes the completed <paramref name="asyncResult" />.</param>
	/// <param name="creationOptions">The TaskCreationOptions value that controls the behavior of the created <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> that is used to schedule the task that executes the end method.</param>
	/// <typeparam name="TResult">The type of the result available through the <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="asyncResult" /> argument is null.-or-The exception that is thrown when the <paramref name="endMethod" /> argument is null.-or-The exception that is thrown when the <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. For more information, see the Remarks for <see cref="M:System.Threading.Tasks.TaskFactory.FromAsync(System.Func{System.AsyncCallback,System.Object,System.IAsyncResult},System.Action{System.IAsyncResult},System.Object,System.Threading.Tasks.TaskCreationOptions)" /></exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> FromAsync<TResult>(IAsyncResult asyncResult, Func<IAsyncResult, TResult> endMethod, TaskCreationOptions creationOptions, TaskScheduler scheduler)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<TResult>.FromAsyncImpl(asyncResult, endMethod, null, creationOptions, scheduler, ref stackMark);
	}

	/// <summary>Creates a <see cref="T:System.Threading.Tasks.Task`1" /> that represents a pair of begin and end methods that conform to the Asynchronous Programming Model pattern.</summary>
	/// <returns>The created <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous operation.</returns>
	/// <param name="beginMethod">The delegate that begins the asynchronous operation.</param>
	/// <param name="endMethod">The delegate that ends the asynchronous operation.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="beginMethod" /> delegate.</param>
	/// <typeparam name="TResult">The type of the result available through the <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="beginMethod" /> argument is null.-or-The exception that is thrown when the <paramref name="endMethod" /> argument is null.</exception>
	public Task<TResult> FromAsync<TResult>(Func<AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, object state)
	{
		return TaskFactory<TResult>.FromAsyncImpl(beginMethod, endMethod, null, state, m_defaultCreationOptions);
	}

	/// <summary>Creates a <see cref="T:System.Threading.Tasks.Task`1" /> that represents a pair of begin and end methods that conform to the Asynchronous Programming Model pattern.</summary>
	/// <returns>The created <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous operation.</returns>
	/// <param name="beginMethod">The delegate that begins the asynchronous operation.</param>
	/// <param name="endMethod">The delegate that ends the asynchronous operation.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="creationOptions">The TaskCreationOptions value that controls the behavior of the created <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <typeparam name="TResult">The type of the result available through the <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="beginMethod" /> argument is null.-or-The exception that is thrown when the <paramref name="endMethod" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. For more information, see the Remarks for <see cref="M:System.Threading.Tasks.TaskFactory.FromAsync(System.Func{System.AsyncCallback,System.Object,System.IAsyncResult},System.Action{System.IAsyncResult},System.Object,System.Threading.Tasks.TaskCreationOptions)" /></exception>
	public Task<TResult> FromAsync<TResult>(Func<AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, object state, TaskCreationOptions creationOptions)
	{
		return TaskFactory<TResult>.FromAsyncImpl(beginMethod, endMethod, null, state, creationOptions);
	}

	/// <summary>Creates a <see cref="T:System.Threading.Tasks.Task`1" /> that represents a pair of begin and end methods that conform to the Asynchronous Programming Model pattern.</summary>
	/// <returns>The created <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous operation.</returns>
	/// <param name="beginMethod">The delegate that begins the asynchronous operation.</param>
	/// <param name="endMethod">The delegate that ends the asynchronous operation.</param>
	/// <param name="arg1">The first argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="beginMethod" /> delegate.</param>
	/// <typeparam name="TArg1">The type of the first argument passed to the <paramref name="beginMethod" /> delegate.</typeparam>
	/// <typeparam name="TResult">The type of the result available through the <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="beginMethod" /> argument is null.-or-The exception that is thrown when the <paramref name="endMethod" /> argument is null.</exception>
	public Task<TResult> FromAsync<TArg1, TResult>(Func<TArg1, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg1 arg1, object state)
	{
		return TaskFactory<TResult>.FromAsyncImpl(beginMethod, endMethod, null, arg1, state, m_defaultCreationOptions);
	}

	/// <summary>Creates a <see cref="T:System.Threading.Tasks.Task`1" /> that represents a pair of begin and end methods that conform to the Asynchronous Programming Model pattern.</summary>
	/// <returns>The created <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous operation.</returns>
	/// <param name="beginMethod">The delegate that begins the asynchronous operation.</param>
	/// <param name="endMethod">The delegate that ends the asynchronous operation.</param>
	/// <param name="arg1">The first argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="creationOptions">The TaskCreationOptions value that controls the behavior of the created <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <typeparam name="TArg1">The type of the first argument passed to the <paramref name="beginMethod" /> delegate.</typeparam>
	/// <typeparam name="TResult">The type of the result available through the <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="beginMethod" /> argument is null.-or-The exception that is thrown when the <paramref name="endMethod" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. For more information, see the Remarks for <see cref="M:System.Threading.Tasks.TaskFactory.FromAsync(System.Func{System.AsyncCallback,System.Object,System.IAsyncResult},System.Action{System.IAsyncResult},System.Object,System.Threading.Tasks.TaskCreationOptions)" /></exception>
	public Task<TResult> FromAsync<TArg1, TResult>(Func<TArg1, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg1 arg1, object state, TaskCreationOptions creationOptions)
	{
		return TaskFactory<TResult>.FromAsyncImpl(beginMethod, endMethod, null, arg1, state, creationOptions);
	}

	/// <summary>Creates a <see cref="T:System.Threading.Tasks.Task`1" /> that represents a pair of begin and end methods that conform to the Asynchronous Programming Model pattern.</summary>
	/// <returns>The created <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous operation.</returns>
	/// <param name="beginMethod">The delegate that begins the asynchronous operation.</param>
	/// <param name="endMethod">The delegate that ends the asynchronous operation.</param>
	/// <param name="arg1">The first argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="arg2">The second argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="beginMethod" /> delegate.</param>
	/// <typeparam name="TArg1">The type of the second argument passed to <paramref name="beginMethod" /> delegate.</typeparam>
	/// <typeparam name="TArg2">The type of the first argument passed to the <paramref name="beginMethod" /> delegate.</typeparam>
	/// <typeparam name="TResult">The type of the result available through the <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="beginMethod" /> argument is null.-or-The exception that is thrown when the <paramref name="endMethod" /> argument is null.</exception>
	public Task<TResult> FromAsync<TArg1, TArg2, TResult>(Func<TArg1, TArg2, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg1 arg1, TArg2 arg2, object state)
	{
		return TaskFactory<TResult>.FromAsyncImpl(beginMethod, endMethod, null, arg1, arg2, state, m_defaultCreationOptions);
	}

	/// <summary>Creates a <see cref="T:System.Threading.Tasks.Task`1" /> that represents a pair of begin and end methods that conform to the Asynchronous Programming Model pattern.</summary>
	/// <returns>The created <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous operation.</returns>
	/// <param name="beginMethod">The delegate that begins the asynchronous operation.</param>
	/// <param name="endMethod">The delegate that ends the asynchronous operation.</param>
	/// <param name="arg1">The first argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="arg2">The second argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="creationOptions">The TaskCreationOptions value that controls the behavior of the created <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <typeparam name="TArg1">The type of the second argument passed to <paramref name="beginMethod" /> delegate.</typeparam>
	/// <typeparam name="TArg2">The type of the first argument passed to the <paramref name="beginMethod" /> delegate.</typeparam>
	/// <typeparam name="TResult">The type of the result available through the <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="beginMethod" /> argument is null.-or-The exception that is thrown when the <paramref name="endMethod" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. For more information, see the Remarks for <see cref="M:System.Threading.Tasks.TaskFactory.FromAsync(System.Func{System.AsyncCallback,System.Object,System.IAsyncResult},System.Action{System.IAsyncResult},System.Object,System.Threading.Tasks.TaskCreationOptions)" /></exception>
	public Task<TResult> FromAsync<TArg1, TArg2, TResult>(Func<TArg1, TArg2, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg1 arg1, TArg2 arg2, object state, TaskCreationOptions creationOptions)
	{
		return TaskFactory<TResult>.FromAsyncImpl(beginMethod, endMethod, null, arg1, arg2, state, creationOptions);
	}

	/// <summary>Creates a <see cref="T:System.Threading.Tasks.Task`1" /> that represents a pair of begin and end methods that conform to the Asynchronous Programming Model pattern.</summary>
	/// <returns>The created <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous operation.</returns>
	/// <param name="beginMethod">The delegate that begins the asynchronous operation.</param>
	/// <param name="endMethod">The delegate that ends the asynchronous operation.</param>
	/// <param name="arg1">The first argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="arg2">The second argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="arg3">The third argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="beginMethod" /> delegate.</param>
	/// <typeparam name="TArg1">The type of the second argument passed to <paramref name="beginMethod" /> delegate.</typeparam>
	/// <typeparam name="TArg2">The type of the third argument passed to <paramref name="beginMethod" /> delegate.</typeparam>
	/// <typeparam name="TArg3">The type of the first argument passed to the <paramref name="beginMethod" /> delegate.</typeparam>
	/// <typeparam name="TResult">The type of the result available through the <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="beginMethod" /> argument is null.-or-The exception that is thrown when the <paramref name="endMethod" /> argument is null.</exception>
	public Task<TResult> FromAsync<TArg1, TArg2, TArg3, TResult>(Func<TArg1, TArg2, TArg3, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg1 arg1, TArg2 arg2, TArg3 arg3, object state)
	{
		return TaskFactory<TResult>.FromAsyncImpl(beginMethod, endMethod, null, arg1, arg2, arg3, state, m_defaultCreationOptions);
	}

	/// <summary>Creates a <see cref="T:System.Threading.Tasks.Task`1" /> that represents a pair of begin and end methods that conform to the Asynchronous Programming Model pattern.</summary>
	/// <returns>The created <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous operation.</returns>
	/// <param name="beginMethod">The delegate that begins the asynchronous operation.</param>
	/// <param name="endMethod">The delegate that ends the asynchronous operation.</param>
	/// <param name="arg1">The first argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="arg2">The second argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="arg3">The third argument passed to the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="state">An object containing data to be used by the <paramref name="beginMethod" /> delegate.</param>
	/// <param name="creationOptions">The TaskCreationOptions value that controls the behavior of the created <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <typeparam name="TArg1">The type of the second argument passed to <paramref name="beginMethod" /> delegate.</typeparam>
	/// <typeparam name="TArg2">The type of the third argument passed to <paramref name="beginMethod" /> delegate.</typeparam>
	/// <typeparam name="TArg3">The type of the first argument passed to the <paramref name="beginMethod" /> delegate.</typeparam>
	/// <typeparam name="TResult">The type of the result available through the <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="beginMethod" /> argument is null.-or-The exception that is thrown when the <paramref name="endMethod" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. For more information, see the Remarks for <see cref="M:System.Threading.Tasks.TaskFactory.FromAsync(System.Func{System.AsyncCallback,System.Object,System.IAsyncResult},System.Action{System.IAsyncResult},System.Object,System.Threading.Tasks.TaskCreationOptions)" /></exception>
	public Task<TResult> FromAsync<TArg1, TArg2, TArg3, TResult>(Func<TArg1, TArg2, TArg3, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg1 arg1, TArg2 arg2, TArg3 arg3, object state, TaskCreationOptions creationOptions)
	{
		return TaskFactory<TResult>.FromAsyncImpl(beginMethod, endMethod, null, arg1, arg2, arg3, state, creationOptions);
	}

	internal static void CheckFromAsyncOptions(TaskCreationOptions creationOptions, bool hasBeginMethod)
	{
		if (hasBeginMethod)
		{
			if ((creationOptions & TaskCreationOptions.LongRunning) != 0)
			{
				throw new ArgumentOutOfRangeException("creationOptions", Environment.GetResourceString("It is invalid to specify TaskCreationOptions.LongRunning in calls to FromAsync."));
			}
			if ((creationOptions & TaskCreationOptions.PreferFairness) != 0)
			{
				throw new ArgumentOutOfRangeException("creationOptions", Environment.GetResourceString("It is invalid to specify TaskCreationOptions.PreferFairness in calls to FromAsync."));
			}
		}
		if ((creationOptions & ~(TaskCreationOptions.PreferFairness | TaskCreationOptions.LongRunning | TaskCreationOptions.AttachedToParent | TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler)) != 0)
		{
			throw new ArgumentOutOfRangeException("creationOptions");
		}
	}

	internal static Task<Task[]> CommonCWAllLogic(Task[] tasksCopy)
	{
		CompleteOnCountdownPromise completeOnCountdownPromise = new CompleteOnCountdownPromise(tasksCopy);
		for (int i = 0; i < tasksCopy.Length; i++)
		{
			if (tasksCopy[i].IsCompleted)
			{
				completeOnCountdownPromise.Invoke(tasksCopy[i]);
			}
			else
			{
				tasksCopy[i].AddCompletionAction(completeOnCountdownPromise);
			}
		}
		return completeOnCountdownPromise;
	}

	internal static Task<Task<T>[]> CommonCWAllLogic<T>(Task<T>[] tasksCopy)
	{
		CompleteOnCountdownPromise<T> completeOnCountdownPromise = new CompleteOnCountdownPromise<T>(tasksCopy);
		for (int i = 0; i < tasksCopy.Length; i++)
		{
			if (tasksCopy[i].IsCompleted)
			{
				completeOnCountdownPromise.Invoke(tasksCopy[i]);
			}
			else
			{
				tasksCopy[i].AddCompletionAction(completeOnCountdownPromise);
			}
		}
		return completeOnCountdownPromise;
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task" /> that will be started upon the completion of a set of provided Tasks.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue.</param>
	/// <param name="continuationAction">The action delegate to execute when all tasks in the <paramref name="tasks" /> array have completed.</param>
	/// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when one of the elements in the <paramref name="tasks" /> array has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationAction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWhenAll(Task[] tasks, Action<Task[]> continuationAction)
	{
		if (continuationAction == null)
		{
			throw new ArgumentNullException("continuationAction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<VoidTaskResult>.ContinueWhenAllImpl(tasks, null, continuationAction, m_defaultContinuationOptions, m_defaultCancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task" /> that will be started upon the completion of a set of provided Tasks.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue.</param>
	/// <param name="continuationAction">The action delegate to execute when all tasks in the <paramref name="tasks" /> array have completed.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
	/// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when one of the elements in the <paramref name="tasks" /> array has been disposed.-or-The <see cref="T:System.Threading.CancellationTokenSource" /> that created<paramref name=" cancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationAction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWhenAll(Task[] tasks, Action<Task[]> continuationAction, CancellationToken cancellationToken)
	{
		if (continuationAction == null)
		{
			throw new ArgumentNullException("continuationAction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<VoidTaskResult>.ContinueWhenAllImpl(tasks, null, continuationAction, m_defaultContinuationOptions, cancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task" /> that will be started upon the completion of a set of provided Tasks.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue.</param>
	/// <param name="continuationAction">The action delegate to execute when all tasks in the <paramref name="tasks" /> array have completed.</param>
	/// <param name="continuationOptions">The <see cref="T:System.Threading.Tasks.TaskContinuationOptions" /> value that controls the behavior of the created continuation <see cref="T:System.Threading.Tasks.Task" />.</param>
	/// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when one of the elements in the <paramref name="tasks" /> array has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationAction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="continuationOptions" /> argument specifies an invalid TaskContinuationOptions value. The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. For more information, see the Remarks for <see cref="M:System.Threading.Tasks.TaskFactory.FromAsync(System.Func{System.AsyncCallback,System.Object,System.IAsyncResult},System.Action{System.IAsyncResult},System.Object,System.Threading.Tasks.TaskCreationOptions)" /></exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWhenAll(Task[] tasks, Action<Task[]> continuationAction, TaskContinuationOptions continuationOptions)
	{
		if (continuationAction == null)
		{
			throw new ArgumentNullException("continuationAction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<VoidTaskResult>.ContinueWhenAllImpl(tasks, null, continuationAction, continuationOptions, m_defaultCancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task" /> that will be started upon the completion of a set of provided Tasks.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue.</param>
	/// <param name="continuationAction">The action delegate to execute when all tasks in the <paramref name="tasks" /> array have completed.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
	/// <param name="continuationOptions">The <see cref="T:System.Threading.Tasks.TaskContinuationOptions" /> value that controls the behavior of the created continuation <see cref="T:System.Threading.Tasks.Task" />.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> that is used to schedule the created continuation <see cref="T:System.Threading.Tasks.Task" />.</param>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationAction" /> argument is null.-or-The exception that is thrown when the <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWhenAll(Task[] tasks, Action<Task[]> continuationAction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		if (continuationAction == null)
		{
			throw new ArgumentNullException("continuationAction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<VoidTaskResult>.ContinueWhenAllImpl(tasks, null, continuationAction, continuationOptions, cancellationToken, scheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task" /> that will be started upon the completion of a set of provided Tasks.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue.</param>
	/// <param name="continuationAction">The action delegate to execute when all tasks in the <paramref name="tasks" /> array have completed.</param>
	/// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks" />.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when one of the elements in the <paramref name="tasks" /> array has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationAction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWhenAll<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Action<Task<TAntecedentResult>[]> continuationAction)
	{
		if (continuationAction == null)
		{
			throw new ArgumentNullException("continuationAction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<VoidTaskResult>.ContinueWhenAllImpl(tasks, null, continuationAction, m_defaultContinuationOptions, m_defaultCancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task" /> that will be started upon the completion of a set of provided Tasks.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue.</param>
	/// <param name="continuationAction">The action delegate to execute when all tasks in the <paramref name="tasks" /> array have completed.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
	/// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks" />.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when one of the elements in the <paramref name="tasks" /> array has been disposed.-or-The <see cref="T:System.Threading.CancellationTokenSource" /> that created<paramref name=" cancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationAction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWhenAll<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Action<Task<TAntecedentResult>[]> continuationAction, CancellationToken cancellationToken)
	{
		if (continuationAction == null)
		{
			throw new ArgumentNullException("continuationAction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<VoidTaskResult>.ContinueWhenAllImpl(tasks, null, continuationAction, m_defaultContinuationOptions, cancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task" /> that will be started upon the completion of a set of provided Tasks.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue.</param>
	/// <param name="continuationAction">The action delegate to execute when all tasks in the <paramref name="tasks" /> array have completed.</param>
	/// <param name="continuationOptions">The <see cref="T:System.Threading.Tasks.TaskContinuationOptions" /> value that controls the behavior of the created continuation <see cref="T:System.Threading.Tasks.Task" />.</param>
	/// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks" />.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when one of the elements in the <paramref name="tasks" /> array has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationAction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="continuationOptions" /> argument specifies an invalid TaskContinuationOptions value. The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. For more information, see the Remarks for <see cref="M:System.Threading.Tasks.TaskFactory.FromAsync(System.Func{System.AsyncCallback,System.Object,System.IAsyncResult},System.Action{System.IAsyncResult},System.Object,System.Threading.Tasks.TaskCreationOptions)" /></exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWhenAll<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Action<Task<TAntecedentResult>[]> continuationAction, TaskContinuationOptions continuationOptions)
	{
		if (continuationAction == null)
		{
			throw new ArgumentNullException("continuationAction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<VoidTaskResult>.ContinueWhenAllImpl(tasks, null, continuationAction, continuationOptions, m_defaultCancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task" /> that will be started upon the completion of a set of provided Tasks.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue.</param>
	/// <param name="continuationAction">The action delegate to execute when all tasks in the <paramref name="tasks" /> array have completed.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
	/// <param name="continuationOptions">The <see cref="T:System.Threading.Tasks.TaskContinuationOptions" /> value that controls the behavior of the created continuation <see cref="T:System.Threading.Tasks.Task" />.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> that is used to schedule the created continuation <see cref="T:System.Threading.Tasks.Task" />.</param>
	/// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationAction" /> argument is null.-or-The exception that is thrown when the <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWhenAll<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Action<Task<TAntecedentResult>[]> continuationAction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		if (continuationAction == null)
		{
			throw new ArgumentNullException("continuationAction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<VoidTaskResult>.ContinueWhenAllImpl(tasks, null, continuationAction, continuationOptions, cancellationToken, scheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task" /> that will be started upon the completion of a set of provided Tasks.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when all tasks in the <paramref name="tasks" /> array have completed.</param>
	/// <typeparam name="TResult">The type of the result that is returned by the <paramref name="continuationFunction" /> delegate and associated with the created <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when one of the elements in the <paramref name="tasks" /> array has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAll<TResult>(Task[] tasks, Func<Task[], TResult> continuationFunction)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<TResult>.ContinueWhenAllImpl(tasks, continuationFunction, null, m_defaultContinuationOptions, m_defaultCancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task" /> that will be started upon the completion of a set of provided Tasks.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when all tasks in the <paramref name="tasks" /> array have completed.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
	/// <typeparam name="TResult">The type of the result that is returned by the <paramref name="continuationFunction" /> delegate and associated with the created <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when one of the elements in the <paramref name="tasks" /> array has been disposed.-or-The <see cref="T:System.Threading.CancellationTokenSource" /> that created<paramref name=" cancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAll<TResult>(Task[] tasks, Func<Task[], TResult> continuationFunction, CancellationToken cancellationToken)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<TResult>.ContinueWhenAllImpl(tasks, continuationFunction, null, m_defaultContinuationOptions, cancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task`1" /> that will be started upon the completion of a set of provided Tasks.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when all tasks in the <paramref name="tasks" /> array have completed.</param>
	/// <param name="continuationOptions">The <see cref="T:System.Threading.Tasks.TaskContinuationOptions" /> value that controls the behavior of the created continuation <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <typeparam name="TResult">The type of the result that is returned by the <paramref name="continuationFunction" /> delegate and associated with the created <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when one of the elements in the <paramref name="tasks" /> array has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="continuationOptions" /> argument specifies an invalid TaskContinuationOptions value. The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. For more information, see the Remarks for <see cref="M:System.Threading.Tasks.TaskFactory.FromAsync(System.Func{System.AsyncCallback,System.Object,System.IAsyncResult},System.Action{System.IAsyncResult},System.Object,System.Threading.Tasks.TaskCreationOptions)" /></exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAll<TResult>(Task[] tasks, Func<Task[], TResult> continuationFunction, TaskContinuationOptions continuationOptions)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<TResult>.ContinueWhenAllImpl(tasks, continuationFunction, null, continuationOptions, m_defaultCancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task`1" /> that will be started upon the completion of a set of provided Tasks.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when all tasks in the <paramref name="tasks" /> array have completed.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
	/// <param name="continuationOptions">The <see cref="T:System.Threading.Tasks.TaskContinuationOptions" /> value that controls the behavior of the created continuation <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> that is used to schedule the created continuation <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <typeparam name="TResult">The type of the result that is returned by the <paramref name="continuationFunction" /> delegate and associated with the created <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationFunction" /> argument is null.-or-The exception that is thrown when the <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAll<TResult>(Task[] tasks, Func<Task[], TResult> continuationFunction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<TResult>.ContinueWhenAllImpl(tasks, continuationFunction, null, continuationOptions, cancellationToken, scheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task`1" /> that will be started upon the completion of a set of provided Tasks.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when all tasks in the <paramref name="tasks" /> array have completed.</param>
	/// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks" />.</typeparam>
	/// <typeparam name="TResult">The type of the result that is returned by the <paramref name="continuationFunction" /> delegate and associated with the created <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when one of the elements in the <paramref name="tasks" /> array has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAll<TAntecedentResult, TResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>[], TResult> continuationFunction)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<TResult>.ContinueWhenAllImpl(tasks, continuationFunction, null, m_defaultContinuationOptions, m_defaultCancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task`1" /> that will be started upon the completion of a set of provided Tasks.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when all tasks in the <paramref name="tasks" /> array have completed.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
	/// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks" />.</typeparam>
	/// <typeparam name="TResult">The type of the result that is returned by the <paramref name="continuationFunction" /> delegate and associated with the created <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when one of the elements in the <paramref name="tasks" /> array has been disposed.-or-The <see cref="T:System.Threading.CancellationTokenSource" /> that created<paramref name=" cancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAll<TAntecedentResult, TResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>[], TResult> continuationFunction, CancellationToken cancellationToken)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<TResult>.ContinueWhenAllImpl(tasks, continuationFunction, null, m_defaultContinuationOptions, cancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task`1" /> that will be started upon the completion of a set of provided Tasks.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when all tasks in the <paramref name="tasks" /> array have completed.</param>
	/// <param name="continuationOptions">The <see cref="T:System.Threading.Tasks.TaskContinuationOptions" /> value that controls the behavior of the created continuation <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks" />.</typeparam>
	/// <typeparam name="TResult">The type of the result that is returned by the <paramref name="continuationFunction" /> delegate and associated with the created <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when one of the elements in the <paramref name="tasks" /> array has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="continuationOptions" /> argument specifies an invalid TaskContinuationOptions value.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAll<TAntecedentResult, TResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>[], TResult> continuationFunction, TaskContinuationOptions continuationOptions)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<TResult>.ContinueWhenAllImpl(tasks, continuationFunction, null, continuationOptions, m_defaultCancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task`1" /> that will be started upon the completion of a set of provided Tasks.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when all tasks in the <paramref name="tasks" /> array have completed.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
	/// <param name="continuationOptions">The <see cref="T:System.Threading.Tasks.TaskContinuationOptions" /> value that controls the behavior of the created continuation <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> that is used to schedule the created continuation <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks" />.</typeparam>
	/// <typeparam name="TResult">The type of the result that is returned by the <paramref name="continuationFunction" /> delegate and associated with the created <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationFunction" /> argument is null.-or-The exception that is thrown when the <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="continuationOptions" /> argument specifies an invalid TaskContinuationOptions value. The exception that is thrown when the <paramref name="creationOptions" /> argument specifies an invalid TaskCreationOptions value. For more information, see the Remarks for <see cref="M:System.Threading.Tasks.TaskFactory.FromAsync(System.Func{System.AsyncCallback,System.Object,System.IAsyncResult},System.Action{System.IAsyncResult},System.Object,System.Threading.Tasks.TaskCreationOptions)" /></exception>
	/// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when one of the elements in the <paramref name="tasks" /> array has been disposed.-or-The <see cref="T:System.Threading.CancellationTokenSource" /> that created<paramref name=" cancellationToken" /> has already been disposed.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAll<TAntecedentResult, TResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>[], TResult> continuationFunction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<TResult>.ContinueWhenAllImpl(tasks, continuationFunction, null, continuationOptions, cancellationToken, scheduler, ref stackMark);
	}

	internal static Task<Task> CommonCWAnyLogic(IList<Task> tasks)
	{
		CompleteOnInvokePromise completeOnInvokePromise = new CompleteOnInvokePromise(tasks);
		bool flag = false;
		int count = tasks.Count;
		for (int i = 0; i < count; i++)
		{
			Task task = tasks[i];
			if (task == null)
			{
				throw new ArgumentException(Environment.GetResourceString("The tasks argument included a null value."), "tasks");
			}
			if (flag)
			{
				continue;
			}
			if (completeOnInvokePromise.IsCompleted)
			{
				flag = true;
				continue;
			}
			if (task.IsCompleted)
			{
				completeOnInvokePromise.Invoke(task);
				flag = true;
				continue;
			}
			task.AddCompletionAction(completeOnInvokePromise);
			if (completeOnInvokePromise.IsCompleted)
			{
				task.RemoveContinuation(completeOnInvokePromise);
			}
		}
		return completeOnInvokePromise;
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task" /> that will be started upon the completion of any Task in the provided set.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
	/// <param name="continuationAction">The action delegate to execute when one task in the <paramref name="tasks" /> array completes.</param>
	/// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when one of the elements in the <paramref name="tasks" /> array has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationAction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWhenAny(Task[] tasks, Action<Task> continuationAction)
	{
		if (continuationAction == null)
		{
			throw new ArgumentNullException("continuationAction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<VoidTaskResult>.ContinueWhenAnyImpl(tasks, null, continuationAction, m_defaultContinuationOptions, m_defaultCancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task" /> that will be started upon the completion of any Task in the provided set.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
	/// <param name="continuationAction">The action delegate to execute when one task in the <paramref name="tasks" /> array completes.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
	/// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when one of the elements in the <paramref name="tasks" /> array has been disposed.-or-The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationAction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWhenAny(Task[] tasks, Action<Task> continuationAction, CancellationToken cancellationToken)
	{
		if (continuationAction == null)
		{
			throw new ArgumentNullException("continuationAction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<VoidTaskResult>.ContinueWhenAnyImpl(tasks, null, continuationAction, m_defaultContinuationOptions, cancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task" /> that will be started upon the completion of any Task in the provided set.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
	/// <param name="continuationAction">The action delegate to execute when one task in the <paramref name="tasks" /> array completes.</param>
	/// <param name="continuationOptions">The <see cref="T:System.Threading.Tasks.TaskContinuationOptions" /> value that controls the behavior of the created continuation <see cref="T:System.Threading.Tasks.Task" />.</param>
	/// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when one of the elements in the <paramref name="tasks" /> array has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationAction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="continuationOptions" /> argument specifies an invalid TaskContinuationOptions value.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWhenAny(Task[] tasks, Action<Task> continuationAction, TaskContinuationOptions continuationOptions)
	{
		if (continuationAction == null)
		{
			throw new ArgumentNullException("continuationAction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<VoidTaskResult>.ContinueWhenAnyImpl(tasks, null, continuationAction, continuationOptions, m_defaultCancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task" /> that will be started upon the completion of any Task in the provided set.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
	/// <param name="continuationAction">The action delegate to execute when one task in the <paramref name="tasks" /> array completes.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
	/// <param name="continuationOptions">The <see cref="T:System.Threading.Tasks.TaskContinuationOptions" /> value that controls the behavior of the created continuation <see cref="T:System.Threading.Tasks.Task" />.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> that is used to schedule the created continuation <see cref="T:System.Threading.Tasks.Task" />.</param>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationAction" /> argument is null.-or-The exception that is thrown when the <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWhenAny(Task[] tasks, Action<Task> continuationAction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		if (continuationAction == null)
		{
			throw new ArgumentNullException("continuationAction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<VoidTaskResult>.ContinueWhenAnyImpl(tasks, null, continuationAction, continuationOptions, cancellationToken, scheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task`1" /> that will be started upon the completion of any Task in the provided set.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when one task in the <paramref name="tasks" /> array completes.</param>
	/// <typeparam name="TResult">The type of the result that is returned by the <paramref name="continuationFunction" /> delegate and associated with the created <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when one of the elements in the <paramref name="tasks" /> array has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAny<TResult>(Task[] tasks, Func<Task, TResult> continuationFunction)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<TResult>.ContinueWhenAnyImpl(tasks, continuationFunction, null, m_defaultContinuationOptions, m_defaultCancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task`1" /> that will be started upon the completion of any Task in the provided set.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when one task in the <paramref name="tasks" /> array completes.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
	/// <typeparam name="TResult">The type of the result that is returned by the <paramref name="continuationFunction" /> delegate and associated with the created <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when one of the elements in the <paramref name="tasks" /> array has been disposed.-or-The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAny<TResult>(Task[] tasks, Func<Task, TResult> continuationFunction, CancellationToken cancellationToken)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<TResult>.ContinueWhenAnyImpl(tasks, continuationFunction, null, m_defaultContinuationOptions, cancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task`1" /> that will be started upon the completion of any Task in the provided set.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when one task in the <paramref name="tasks" /> array completes.</param>
	/// <param name="continuationOptions">The <see cref="T:System.Threading.Tasks.TaskContinuationOptions" /> value that controls the behavior of the created continuation <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <typeparam name="TResult">The type of the result that is returned by the <paramref name="continuationFunction" /> delegate and associated with the created <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when one of the elements in the <paramref name="tasks" /> array has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="continuationOptions" /> argument specifies an invalid TaskContinuationOptions value.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAny<TResult>(Task[] tasks, Func<Task, TResult> continuationFunction, TaskContinuationOptions continuationOptions)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<TResult>.ContinueWhenAnyImpl(tasks, continuationFunction, null, continuationOptions, m_defaultCancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task`1" /> that will be started upon the completion of any Task in the provided set.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when one task in the <paramref name="tasks" /> array completes.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
	/// <param name="continuationOptions">The <see cref="T:System.Threading.Tasks.TaskContinuationOptions" /> value that controls the behavior of the created continuation <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> that is used to schedule the created continuation <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <typeparam name="TResult">The type of the result that is returned by the <paramref name="continuationFunction" /> delegate and associated with the created <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationFunction" /> argument is null.-or-The exception that is thrown when the <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAny<TResult>(Task[] tasks, Func<Task, TResult> continuationFunction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<TResult>.ContinueWhenAnyImpl(tasks, continuationFunction, null, continuationOptions, cancellationToken, scheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task`1" /> that will be started upon the completion of any Task in the provided set.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when one task in the <paramref name="tasks" /> array completes.</param>
	/// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks" />.</typeparam>
	/// <typeparam name="TResult">The type of the result that is returned by the <paramref name="continuationFunction" /> delegate and associated with the created <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when one of the elements in the <paramref name="tasks" /> array has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAny<TAntecedentResult, TResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>, TResult> continuationFunction)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		return TaskFactory<TResult>.ContinueWhenAnyImpl(tasks, continuationFunction, null, m_defaultContinuationOptions, m_defaultCancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task`1" /> that will be started upon the completion of any Task in the provided set.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when one task in the <paramref name="tasks" /> array completes.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
	/// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks" />.</typeparam>
	/// <typeparam name="TResult">The type of the result that is returned by the <paramref name="continuationFunction" /> delegate and associated with the created <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when one of the elements in the <paramref name="tasks" /> array has been disposed.-or-The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAny<TAntecedentResult, TResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>, TResult> continuationFunction, CancellationToken cancellationToken)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<TResult>.ContinueWhenAnyImpl(tasks, continuationFunction, null, m_defaultContinuationOptions, cancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task`1" /> that will be started upon the completion of any Task in the provided set.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when one task in the <paramref name="tasks" /> array completes.</param>
	/// <param name="continuationOptions">The <see cref="T:System.Threading.Tasks.TaskContinuationOptions" /> value that controls the behavior of the created continuation <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks" />.</typeparam>
	/// <typeparam name="TResult">The type of the result that is returned by the <paramref name="continuationFunction" /> delegate and associated with the created <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when one of the elements in the <paramref name="tasks" /> array has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="continuationOptions" /> argument specifies an invalid TaskContinuationOptions value.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAny<TAntecedentResult, TResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>, TResult> continuationFunction, TaskContinuationOptions continuationOptions)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<TResult>.ContinueWhenAnyImpl(tasks, continuationFunction, null, continuationOptions, m_defaultCancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task`1" /> that will be started upon the completion of any Task in the provided set.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
	/// <param name="continuationFunction">The function delegate to execute asynchronously when one task in the <paramref name="tasks" /> array completes.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
	/// <param name="continuationOptions">The <see cref="T:System.Threading.Tasks.TaskContinuationOptions" /> value that controls the behavior of the created continuation <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> that is used to schedule the created continuation <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks" />.</typeparam>
	/// <typeparam name="TResult">The type of the result that is returned by the <paramref name="continuationFunction" /> delegate and associated with the created <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationFunction" /> argument is null.-or-The exception that is thrown when the <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWhenAny<TAntecedentResult, TResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>, TResult> continuationFunction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<TResult>.ContinueWhenAnyImpl(tasks, continuationFunction, null, continuationOptions, cancellationToken, scheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task" /> that will be started upon the completion of any Task in the provided set.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
	/// <param name="continuationAction">The action delegate to execute when one task in the <paramref name="tasks" /> array completes.</param>
	/// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks" />.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when one of the elements in the <paramref name="tasks" /> array has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationAction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWhenAny<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Action<Task<TAntecedentResult>> continuationAction)
	{
		if (continuationAction == null)
		{
			throw new ArgumentNullException("continuationAction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<VoidTaskResult>.ContinueWhenAnyImpl(tasks, null, continuationAction, m_defaultContinuationOptions, m_defaultCancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task" /> that will be started upon the completion of any Task in the provided set.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
	/// <param name="continuationAction">The action delegate to execute when one task in the <paramref name="tasks" /> array completes.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
	/// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks" />.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when one of the elements in the <paramref name="tasks" /> array has been disposed.-or-The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationAction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWhenAny<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Action<Task<TAntecedentResult>> continuationAction, CancellationToken cancellationToken)
	{
		if (continuationAction == null)
		{
			throw new ArgumentNullException("continuationAction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<VoidTaskResult>.ContinueWhenAnyImpl(tasks, null, continuationAction, m_defaultContinuationOptions, cancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task" /> that will be started upon the completion of any Task in the provided set.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
	/// <param name="continuationAction">The action delegate to execute when one task in the <paramref name="tasks" /> array completes.</param>
	/// <param name="continuationOptions">The <see cref="T:System.Threading.Tasks.TaskContinuationOptions" /> value that controls the behavior of the created continuation <see cref="T:System.Threading.Tasks.Task" />.</param>
	/// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks" />.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The exception that is thrown when one of the elements in the <paramref name="tasks" /> array has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationAction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the <paramref name="continuationOptions" /> argument specifies an invalid TaskContinuationOptions value.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWhenAny<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Action<Task<TAntecedentResult>> continuationAction, TaskContinuationOptions continuationOptions)
	{
		if (continuationAction == null)
		{
			throw new ArgumentNullException("continuationAction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<VoidTaskResult>.ContinueWhenAnyImpl(tasks, null, continuationAction, continuationOptions, m_defaultCancellationToken, DefaultScheduler, ref stackMark);
	}

	/// <summary>Creates a continuation <see cref="T:System.Threading.Tasks.Task" /> that will be started upon the completion of any Task in the provided set.</summary>
	/// <returns>The new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="tasks">The array of tasks from which to continue when one task completes.</param>
	/// <param name="continuationAction">The action delegate to execute when one task in the <paramref name="tasks" /> array completes.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
	/// <param name="continuationOptions">The <see cref="T:System.Threading.Tasks.TaskContinuationOptions" /> value that controls the behavior of the created continuation <see cref="T:System.Threading.Tasks.Task" />.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> that is used to schedule the created continuation <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <typeparam name="TAntecedentResult">The type of the result of the antecedent <paramref name="tasks" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="tasks" /> array is null.-or-The exception that is thrown when the <paramref name="continuationAction" /> argument is null.-or-The exception that is thrown when the <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The exception that is thrown when the <paramref name="tasks" /> array contains a null value.-or-The exception that is thrown when the <paramref name="tasks" /> array is empty.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWhenAny<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Action<Task<TAntecedentResult>> continuationAction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		if (continuationAction == null)
		{
			throw new ArgumentNullException("continuationAction");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TaskFactory<VoidTaskResult>.ContinueWhenAnyImpl(tasks, null, continuationAction, continuationOptions, cancellationToken, scheduler, ref stackMark);
	}

	internal static Task[] CheckMultiContinuationTasksAndCopy(Task[] tasks)
	{
		if (tasks == null)
		{
			throw new ArgumentNullException("tasks");
		}
		if (tasks.Length == 0)
		{
			throw new ArgumentException(Environment.GetResourceString("The tasks argument contains no tasks."), "tasks");
		}
		Task[] array = new Task[tasks.Length];
		for (int i = 0; i < tasks.Length; i++)
		{
			array[i] = tasks[i];
			if (array[i] == null)
			{
				throw new ArgumentException(Environment.GetResourceString("The tasks argument included a null value."), "tasks");
			}
		}
		return array;
	}

	internal static Task<TResult>[] CheckMultiContinuationTasksAndCopy<TResult>(Task<TResult>[] tasks)
	{
		if (tasks == null)
		{
			throw new ArgumentNullException("tasks");
		}
		if (tasks.Length == 0)
		{
			throw new ArgumentException(Environment.GetResourceString("The tasks argument contains no tasks."), "tasks");
		}
		Task<TResult>[] array = new Task<TResult>[tasks.Length];
		for (int i = 0; i < tasks.Length; i++)
		{
			array[i] = tasks[i];
			if (array[i] == null)
			{
				throw new ArgumentException(Environment.GetResourceString("The tasks argument included a null value."), "tasks");
			}
		}
		return array;
	}

	internal static void CheckMultiTaskContinuationOptions(TaskContinuationOptions continuationOptions)
	{
		if ((continuationOptions & (TaskContinuationOptions.LongRunning | TaskContinuationOptions.ExecuteSynchronously)) == (TaskContinuationOptions.LongRunning | TaskContinuationOptions.ExecuteSynchronously))
		{
			throw new ArgumentOutOfRangeException("continuationOptions", Environment.GetResourceString("The specified TaskContinuationOptions combined LongRunning and ExecuteSynchronously.  Synchronous continuations should not be long running."));
		}
		if ((continuationOptions & ~(TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.PreferFairness | TaskContinuationOptions.LongRunning | TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.DenyChildAttach | TaskContinuationOptions.HideScheduler | TaskContinuationOptions.LazyCancellation | TaskContinuationOptions.NotOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously)) != 0)
		{
			throw new ArgumentOutOfRangeException("continuationOptions");
		}
		if ((continuationOptions & (TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.NotOnRanToCompletion)) != 0)
		{
			throw new ArgumentOutOfRangeException("continuationOptions", Environment.GetResourceString("It is invalid to exclude specific continuation kinds for continuations off of multiple tasks."));
		}
	}
}
