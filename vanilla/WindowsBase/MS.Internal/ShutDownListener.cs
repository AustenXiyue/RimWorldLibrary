using System;
using System.Threading;
using System.Windows.Threading;
using MS.Internal.WindowsBase;

namespace MS.Internal;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal abstract class ShutDownListener : WeakReference
{
	[Flags]
	private enum PrivateFlags : ushort
	{
		DomainUnload = 1,
		ProcessExit = 2,
		DispatcherShutdown = 4,
		Static = 0x4000,
		Listening = 0x8000
	}

	private PrivateFlags _flags;

	private WeakReference _dispatcherWR;

	private int _inShutDown;

	internal ShutDownListener(object target)
		: this(target, ShutDownEvents.All)
	{
	}

	internal ShutDownListener(object target, ShutDownEvents events)
		: base(target)
	{
		_flags = (PrivateFlags)(events | (ShutDownEvents)32768);
		if (target == null)
		{
			_flags |= PrivateFlags.Static;
		}
		if ((_flags & PrivateFlags.DomainUnload) != 0)
		{
			AppDomain.CurrentDomain.DomainUnload += HandleShutDown;
		}
		if ((_flags & PrivateFlags.ProcessExit) != 0)
		{
			AppDomain.CurrentDomain.ProcessExit += HandleShutDown;
		}
		if ((_flags & PrivateFlags.DispatcherShutdown) != 0)
		{
			Dispatcher currentDispatcher = Dispatcher.CurrentDispatcher;
			currentDispatcher.ShutdownFinished += HandleShutDown;
			_dispatcherWR = new WeakReference(currentDispatcher);
		}
	}

	internal abstract void OnShutDown(object target, object sender, EventArgs e);

	internal void StopListening()
	{
		if ((_flags & PrivateFlags.Listening) == 0)
		{
			return;
		}
		_flags &= ~PrivateFlags.Listening;
		if ((_flags & PrivateFlags.DomainUnload) != 0)
		{
			AppDomain.CurrentDomain.DomainUnload -= HandleShutDown;
		}
		if ((_flags & PrivateFlags.ProcessExit) != 0)
		{
			AppDomain.CurrentDomain.ProcessExit -= HandleShutDown;
		}
		if ((_flags & PrivateFlags.DispatcherShutdown) != 0)
		{
			Dispatcher dispatcher = (Dispatcher)_dispatcherWR.Target;
			if (dispatcher != null)
			{
				dispatcher.ShutdownFinished -= HandleShutDown;
			}
			_dispatcherWR = null;
		}
	}

	private void HandleShutDown(object sender, EventArgs e)
	{
		if (Interlocked.Exchange(ref _inShutDown, 1) == 0)
		{
			StopListening();
			object target = Target;
			if (target != null || (_flags & PrivateFlags.Static) != 0)
			{
				OnShutDown(target, sender, e);
			}
		}
	}
}
