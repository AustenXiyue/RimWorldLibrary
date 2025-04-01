using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace System.Runtime.Remoting.Messaging;

/// <summary>Encapsulates the results of an asynchronous operation on a delegate.</summary>
[StructLayout(LayoutKind.Sequential)]
[ComVisible(true)]
public class AsyncResult : IAsyncResult, IMessageSink, IThreadPoolWorkItem
{
	private object async_state;

	private WaitHandle handle;

	private object async_delegate;

	private IntPtr data;

	private object object_data;

	private bool sync_completed;

	private bool completed;

	private bool endinvoke_called;

	private object async_callback;

	private ExecutionContext current;

	private ExecutionContext original;

	private long add_time;

	private MonoMethodMessage call_message;

	private IMessageCtrl message_ctrl;

	private IMessage reply_message;

	private WaitCallback orig_cb;

	internal static ContextCallback ccb = WaitCallback_Context;

	/// <summary>Gets the object provided as the last parameter of a BeginInvoke method call.</summary>
	/// <returns>The object provided as the last parameter of a BeginInvoke method call.</returns>
	public virtual object AsyncState => async_state;

	/// <summary>Gets a <see cref="T:System.Threading.WaitHandle" /> that encapsulates Win32 synchronization handles, and allows the implementation of various synchronization schemes.</summary>
	/// <returns>A <see cref="T:System.Threading.WaitHandle" /> that encapsulates Win32 synchronization handles, and allows the implementation of various synchronization schemes.</returns>
	public virtual WaitHandle AsyncWaitHandle
	{
		get
		{
			lock (this)
			{
				if (handle == null)
				{
					handle = new ManualResetEvent(completed);
				}
				return handle;
			}
		}
	}

	/// <summary>Gets a value indicating whether the BeginInvoke call completed synchronously.</summary>
	/// <returns>true if the BeginInvoke call completed synchronously; otherwise, false.</returns>
	public virtual bool CompletedSynchronously => sync_completed;

	/// <summary>Gets a value indicating whether the server has completed the call.</summary>
	/// <returns>true after the server has completed the call; otherwise, false.</returns>
	public virtual bool IsCompleted => completed;

	/// <summary>Gets or sets a value indicating whether EndInvoke has been called on the current <see cref="T:System.Runtime.Remoting.Messaging.AsyncResult" />.</summary>
	/// <returns>true if EndInvoke has been called on the current <see cref="T:System.Runtime.Remoting.Messaging.AsyncResult" />; otherwise, false.</returns>
	public bool EndInvokeCalled
	{
		get
		{
			return endinvoke_called;
		}
		set
		{
			endinvoke_called = value;
		}
	}

	/// <summary>Gets the delegate object on which the asynchronous call was invoked.</summary>
	/// <returns>The delegate object on which the asynchronous call was invoked.</returns>
	public virtual object AsyncDelegate => async_delegate;

	/// <summary>Gets the next message sink in the sink chain.</summary>
	/// <returns>An <see cref="T:System.Runtime.Remoting.Messaging.IMessageSink" /> interface that represents the next message sink in the sink chain.</returns>
	public IMessageSink NextSink
	{
		[SecurityCritical]
		get
		{
			return null;
		}
	}

	internal MonoMethodMessage CallMessage
	{
		get
		{
			return call_message;
		}
		set
		{
			call_message = value;
		}
	}

	internal AsyncResult()
	{
	}

	internal AsyncResult(WaitCallback cb, object state, bool capture_context)
	{
		orig_cb = cb;
		if (capture_context)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMe;
			current = ExecutionContext.Capture(ref stackMark, ExecutionContext.CaptureOptions.IgnoreSyncCtx | ExecutionContext.CaptureOptions.OptimizeDefaultCase);
			cb = delegate
			{
				ExecutionContext.Run(current, ccb, this, preserveSyncCtx: true);
			};
		}
		async_state = state;
		async_delegate = cb;
	}

	private static void WaitCallback_Context(object state)
	{
		AsyncResult asyncResult = (AsyncResult)state;
		asyncResult.orig_cb(asyncResult.async_state);
	}

	/// <summary>Implements the <see cref="T:System.Runtime.Remoting.Messaging.IMessageSink" /> interface.</summary>
	/// <returns>No value is returned.</returns>
	/// <param name="msg">The request <see cref="T:System.Runtime.Remoting.Messaging.IMessage" /> interface. </param>
	/// <param name="replySink">The response <see cref="T:System.Runtime.Remoting.Messaging.IMessageSink" /> interface. </param>
	[SecurityCritical]
	public virtual IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
	{
		throw new NotSupportedException();
	}

	/// <summary>Gets the response message for the asynchronous call.</summary>
	/// <returns>A remoting message that should represent a response to a method call on a remote object.</returns>
	public virtual IMessage GetReplyMessage()
	{
		return reply_message;
	}

	/// <summary>Sets an <see cref="T:System.Runtime.Remoting.Messaging.IMessageCtrl" /> for the current remote method call, which provides a way to control asynchronous messages after they have been dispatched.</summary>
	/// <param name="mc">The <see cref="T:System.Runtime.Remoting.Messaging.IMessageCtrl" /> for the current remote method call. </param>
	public virtual void SetMessageCtrl(IMessageCtrl mc)
	{
		message_ctrl = mc;
	}

	internal void SetCompletedSynchronously(bool completed)
	{
		sync_completed = completed;
	}

	internal IMessage EndInvoke()
	{
		lock (this)
		{
			if (completed)
			{
				return reply_message;
			}
		}
		AsyncWaitHandle.WaitOne();
		return reply_message;
	}

	/// <summary>Synchronously processes a response message returned by a method call on a remote object.</summary>
	/// <returns>Returns null.</returns>
	/// <param name="msg">A response message to a method call on a remote object.</param>
	[SecurityCritical]
	public virtual IMessage SyncProcessMessage(IMessage msg)
	{
		reply_message = msg;
		lock (this)
		{
			completed = true;
			if (handle != null)
			{
				((ManualResetEvent)AsyncWaitHandle).Set();
			}
		}
		if (async_callback != null)
		{
			((AsyncCallback)async_callback)(this);
		}
		return null;
	}

	void IThreadPoolWorkItem.ExecuteWorkItem()
	{
		Invoke();
	}

	void IThreadPoolWorkItem.MarkAborted(ThreadAbortException tae)
	{
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal extern object Invoke();
}
