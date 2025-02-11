using System.ComponentModel;
using System.Threading;
using MS.Internal.WindowsBase;

namespace System.Windows.Threading;

/// <summary>Represents an object that is associated with a <see cref="T:System.Windows.Threading.Dispatcher" />. </summary>
public abstract class DispatcherObject
{
	private Dispatcher _dispatcher;

	private static Dispatcher _sentinelDispatcher;

	/// <summary>Gets the <see cref="T:System.Windows.Threading.Dispatcher" /> this <see cref="T:System.Windows.Threading.DispatcherObject" /> is associated with. </summary>
	/// <returns>The dispatcher.</returns>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public Dispatcher Dispatcher => _dispatcher;

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal void DetachFromDispatcher()
	{
		_dispatcher = null;
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal void MakeSentinel()
	{
		_dispatcher = EnsureSentinelDispatcher();
	}

	private static Dispatcher EnsureSentinelDispatcher()
	{
		if (_sentinelDispatcher == null)
		{
			Dispatcher value = new Dispatcher(isSentinel: true);
			Interlocked.CompareExchange(ref _sentinelDispatcher, value, null);
		}
		return _sentinelDispatcher;
	}

	/// <summary>Determines whether the calling thread has access to this <see cref="T:System.Windows.Threading.DispatcherObject" />.</summary>
	/// <returns>true if the calling thread has access to this object; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool CheckAccess()
	{
		bool result = true;
		Dispatcher dispatcher = _dispatcher;
		if (dispatcher != null)
		{
			result = dispatcher.CheckAccess();
		}
		return result;
	}

	/// <summary>Enforces that the calling thread has access to this <see cref="T:System.Windows.Threading.DispatcherObject" />.</summary>
	/// <exception cref="T:System.InvalidOperationException">the calling thread does not have access to this <see cref="T:System.Windows.Threading.DispatcherObject" />.</exception>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void VerifyAccess()
	{
		_dispatcher?.VerifyAccess();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Threading.DispatcherObject" /> class. </summary>
	protected DispatcherObject()
	{
		_dispatcher = Dispatcher.CurrentDispatcher;
	}
}
