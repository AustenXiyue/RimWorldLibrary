using System;
using System.Collections.Generic;

namespace MS.Internal;

internal static class AppDomainShutdownMonitor
{
	private static readonly HashSet<WeakReference<IAppDomainShutdownListener>> _listeners;

	private static bool _shuttingDown;

	static AppDomainShutdownMonitor()
	{
		_listeners = new HashSet<WeakReference<IAppDomainShutdownListener>>();
		AppDomain.CurrentDomain.DomainUnload += OnShutdown;
		AppDomain.CurrentDomain.ProcessExit += OnShutdown;
	}

	public static void Add(WeakReference<IAppDomainShutdownListener> listener)
	{
		lock (_listeners)
		{
			if (!_shuttingDown)
			{
				_listeners.Add(listener);
			}
		}
	}

	public static void Remove(WeakReference<IAppDomainShutdownListener> listener)
	{
		lock (_listeners)
		{
			if (!_shuttingDown)
			{
				_listeners.Remove(listener);
			}
		}
	}

	private static void OnShutdown(object sender, EventArgs e)
	{
		lock (_listeners)
		{
			_shuttingDown = true;
		}
		foreach (WeakReference<IAppDomainShutdownListener> listener in _listeners)
		{
			if (listener.TryGetTarget(out var target))
			{
				target.NotifyShutdown();
			}
		}
	}
}
