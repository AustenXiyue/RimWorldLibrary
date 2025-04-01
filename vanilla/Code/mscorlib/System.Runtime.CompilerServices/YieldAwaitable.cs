using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices;

/// <summary>Provides the context for waiting when asynchronously switching into a target environment.</summary>
[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct YieldAwaitable
{
	/// <summary>Provides an awaiter for switching into a target environment.</summary>
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
	public struct YieldAwaiter : ICriticalNotifyCompletion, INotifyCompletion
	{
		private static readonly WaitCallback s_waitCallbackRunAction = RunAction;

		private static readonly SendOrPostCallback s_sendOrPostCallbackRunAction = RunAction;

		/// <summary>Gets a value that indicates whether a yield is not required.</summary>
		/// <returns>Always false, which indicates that a yield is always required for <see cref="T:System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter" />.</returns>
		public bool IsCompleted => false;

		/// <summary>Sets the continuation to invoke.</summary>
		/// <param name="continuation">The action to invoke asynchronously.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="continuation" /> is null.</exception>
		[SecuritySafeCritical]
		public void OnCompleted(Action continuation)
		{
			QueueContinuation(continuation, flowContext: true);
		}

		/// <summary>Posts the <paramref name="continuation" /> back to the current context.</summary>
		/// <param name="continuation">The action to invoke asynchronously.</param>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuation" /> argument is null.</exception>
		[SecurityCritical]
		public void UnsafeOnCompleted(Action continuation)
		{
			QueueContinuation(continuation, flowContext: false);
		}

		[SecurityCritical]
		private static void QueueContinuation(Action continuation, bool flowContext)
		{
			if (continuation == null)
			{
				throw new ArgumentNullException("continuation");
			}
			SynchronizationContext currentNoFlow = SynchronizationContext.CurrentNoFlow;
			if (currentNoFlow != null && currentNoFlow.GetType() != typeof(SynchronizationContext))
			{
				currentNoFlow.Post(s_sendOrPostCallbackRunAction, continuation);
				return;
			}
			TaskScheduler current = TaskScheduler.Current;
			if (current == TaskScheduler.Default)
			{
				if (flowContext)
				{
					ThreadPool.QueueUserWorkItem(s_waitCallbackRunAction, continuation);
				}
				else
				{
					ThreadPool.UnsafeQueueUserWorkItem(s_waitCallbackRunAction, continuation);
				}
			}
			else
			{
				Task.Factory.StartNew(continuation, default(CancellationToken), TaskCreationOptions.PreferFairness, current);
			}
		}

		private static void RunAction(object state)
		{
			((Action)state)();
		}

		/// <summary>Ends the await operation.</summary>
		public void GetResult()
		{
		}
	}

	/// <summary>Retrieves a <see cref="T:System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter" /> object  for this instance of the class.</summary>
	/// <returns>The object that is used to monitor the completion of an asynchronous operation.</returns>
	public YieldAwaiter GetAwaiter()
	{
		return default(YieldAwaiter);
	}
}
