using System.Collections.Generic;

namespace System.Windows.Diagnostics;

public static class BindingDiagnostics
{
	private static List<BindingFailedEventArgs> s_pendingEvents;

	private static readonly object s_pendingEventsLock;

	private const int MaxPendingEvents = 2000;

	internal static bool IsEnabled { get; private set; }

	private static event EventHandler<BindingFailedEventArgs> s_bindingFailed;

	public static event EventHandler<BindingFailedEventArgs> BindingFailed
	{
		add
		{
			if (IsEnabled)
			{
				s_bindingFailed += value;
				FlushPendingBindingFailedEvents();
			}
		}
		remove
		{
			s_bindingFailed -= value;
		}
	}

	static BindingDiagnostics()
	{
		IsEnabled = VisualDiagnostics.IsEnabled && VisualDiagnostics.IsEnvironmentVariableSet(null, "ENABLE_XAML_DIAGNOSTICS_SOURCE_INFO");
		if (IsEnabled)
		{
			s_pendingEvents = new List<BindingFailedEventArgs>();
			s_pendingEventsLock = new object();
		}
	}

	private static void FlushPendingBindingFailedEvents()
	{
		if (s_pendingEvents == null)
		{
			return;
		}
		List<BindingFailedEventArgs> list = null;
		lock (s_pendingEventsLock)
		{
			list = s_pendingEvents;
			s_pendingEvents = null;
		}
		if (list == null)
		{
			return;
		}
		foreach (BindingFailedEventArgs item in list)
		{
			BindingDiagnostics.s_bindingFailed?.Invoke(null, item);
		}
	}

	internal static void NotifyBindingFailed(BindingFailedEventArgs args)
	{
		if (!IsEnabled)
		{
			return;
		}
		if (s_pendingEvents != null)
		{
			lock (s_pendingEventsLock)
			{
				if (s_pendingEvents != null)
				{
					if (s_pendingEvents.Count < 2000)
					{
						s_pendingEvents.Add(args);
					}
					return;
				}
			}
		}
		BindingDiagnostics.s_bindingFailed?.Invoke(null, args);
	}
}
