using System.Security;

namespace System.Threading;

/// <summary>Provides the functionality to restore the migration, or flow, of the execution context between threads.  </summary>
/// <filterpriority>2</filterpriority>
public struct AsyncFlowControl : IDisposable
{
	private bool useEC;

	private ExecutionContext _ec;

	private Thread _thread;

	[SecurityCritical]
	internal void Setup()
	{
		useEC = true;
		Thread currentThread = Thread.CurrentThread;
		_ec = currentThread.GetMutableExecutionContext();
		_ec.isFlowSuppressed = true;
		_thread = currentThread;
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.Threading.AsyncFlowControl" /> class.</summary>
	public void Dispose()
	{
		Undo();
	}

	/// <summary>Restores the flow of the execution context between threads.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Threading.AsyncFlowControl" /> structure is not used on the thread where it was created.-or-The <see cref="T:System.Threading.AsyncFlowControl" /> structure has already been used to call <see cref="M:System.Threading.AsyncFlowControl.Undo" />.</exception>
	/// <filterpriority>2</filterpriority>
	[SecuritySafeCritical]
	public void Undo()
	{
		if (_thread == null)
		{
			throw new InvalidOperationException(Environment.GetResourceString("AsyncFlowControl object can be used only once to call Undo()."));
		}
		if (_thread != Thread.CurrentThread)
		{
			throw new InvalidOperationException(Environment.GetResourceString("AsyncFlowControl object must be used on the thread where it was created."));
		}
		if (useEC)
		{
			if (Thread.CurrentThread.GetMutableExecutionContext() != _ec)
			{
				throw new InvalidOperationException(Environment.GetResourceString("AsyncFlowControl objects can be used to restore flow only on the Context that had its flow suppressed."));
			}
			ExecutionContext.RestoreFlow();
		}
		_thread = null;
	}

	/// <summary>Gets a hash code for the current <see cref="T:System.Threading.AsyncFlowControl" /> structure.</summary>
	/// <returns>A hash code for the current <see cref="T:System.Threading.AsyncFlowControl" /> structure.</returns>
	public override int GetHashCode()
	{
		if (_thread != null)
		{
			return _thread.GetHashCode();
		}
		return ToString().GetHashCode();
	}

	/// <summary>Determines whether the specified object is equal to the current <see cref="T:System.Threading.AsyncFlowControl" /> structure. </summary>
	/// <returns>true if <paramref name="obj" /> is an <see cref="T:System.Threading.AsyncFlowControl" /> structure and is equal to the current <see cref="T:System.Threading.AsyncFlowControl" /> structure; otherwise, false. </returns>
	/// <param name="obj">An object to compare with the current structure.</param>
	public override bool Equals(object obj)
	{
		if (obj is AsyncFlowControl)
		{
			return Equals((AsyncFlowControl)obj);
		}
		return false;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Threading.AsyncFlowControl" /> structure is equal to the current <see cref="T:System.Threading.AsyncFlowControl" /> structure.</summary>
	/// <returns>true if <paramref name="obj" /> is equal to the current <see cref="T:System.Threading.AsyncFlowControl" /> structure; otherwise, false.</returns>
	/// <param name="obj">An <see cref="T:System.Threading.AsyncFlowControl" /> structure to compare with the current structure.</param>
	public bool Equals(AsyncFlowControl obj)
	{
		if (obj.useEC == useEC && obj._ec == _ec)
		{
			return obj._thread == _thread;
		}
		return false;
	}

	/// <summary>Compares two <see cref="T:System.Threading.AsyncFlowControl" /> structures to determine whether they are equal. </summary>
	/// <returns>true if the two structures are equal; otherwise, false. </returns>
	/// <param name="a">An <see cref="T:System.Threading.AsyncFlowControl" /> structure.</param>
	/// <param name="b">An <see cref="T:System.Threading.AsyncFlowControl" /> structure.</param>
	public static bool operator ==(AsyncFlowControl a, AsyncFlowControl b)
	{
		return a.Equals(b);
	}

	/// <summary>Compares two <see cref="T:System.Threading.AsyncFlowControl" /> structures to determine whether they are not equal. </summary>
	/// <returns>true if the structures are not equal; otherwise, false. </returns>
	/// <param name="a">An <see cref="T:System.Threading.AsyncFlowControl" /> structure.</param>
	/// <param name="b">An <see cref="T:System.Threading.AsyncFlowControl" /> structure.</param>
	public static bool operator !=(AsyncFlowControl a, AsyncFlowControl b)
	{
		return !(a == b);
	}
}
