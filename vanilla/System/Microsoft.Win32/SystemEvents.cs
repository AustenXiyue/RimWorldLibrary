using System;
using System.Collections;
using System.ComponentModel;
using System.Security.Permissions;
using System.Timers;

namespace Microsoft.Win32;

/// <summary>Provides access to system event notifications. This class cannot be inherited.</summary>
[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
public sealed class SystemEvents
{
	private static Hashtable TimerStore = new Hashtable();

	/// <summary>Occurs when the user changes the display settings.</summary>
	/// <exception cref="T:System.InvalidOperationException">System event notifications are not supported under the current context. Server processes, for example, might not support global system event notifications.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.ExternalException">The attempt to create a system events window thread did not succeed.</exception>
	[System.MonoTODO]
	public static event EventHandler DisplaySettingsChanged
	{
		add
		{
		}
		remove
		{
		}
	}

	/// <summary>Occurs when the display settings are changing.</summary>
	/// <exception cref="T:System.InvalidOperationException">System event notifications are not supported under the current context. Server processes, for example, might not support global system event notifications.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.ExternalException">The attempt to create a system events window thread did not succeed.</exception>
	[System.MonoTODO("Currently does nothing on Mono")]
	public static event EventHandler DisplaySettingsChanging
	{
		add
		{
		}
		remove
		{
		}
	}

	/// <summary>Occurs before the thread that listens for system events is terminated.</summary>
	/// <exception cref="T:System.InvalidOperationException">System event notifications are not supported under the current context. Server processes, for example, might not support global system event notifications.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.ExternalException">The attempt to create a system events window thread did not succeed.</exception>
	[System.MonoTODO("Currently does nothing on Mono")]
	public static event EventHandler EventsThreadShutdown
	{
		add
		{
		}
		remove
		{
		}
	}

	/// <summary>Occurs when the user adds fonts to or removes fonts from the system.</summary>
	/// <exception cref="T:System.InvalidOperationException">System event notifications are not supported under the current context. Server processes, for example, might not support global system event notifications.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.ExternalException">The attempt to create a system events window thread did not succeed.</exception>
	[System.MonoTODO("Currently does nothing on Mono")]
	public static event EventHandler InstalledFontsChanged
	{
		add
		{
		}
		remove
		{
		}
	}

	/// <summary>Occurs when the system is running out of available RAM.</summary>
	/// <exception cref="T:System.InvalidOperationException">System event notifications are not supported under the current context. Server processes, for example, might not support global system event notifications.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.ExternalException">The attempt to create a system events window thread did not succeed.</exception>
	[System.MonoTODO("Currently does nothing on Mono")]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("")]
	public static event EventHandler LowMemory
	{
		add
		{
		}
		remove
		{
		}
	}

	/// <summary>Occurs when the user switches to an application that uses a different palette.</summary>
	/// <exception cref="T:System.InvalidOperationException">System event notifications are not supported under the current context. Server processes, for example, might not support global system event notifications.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.ExternalException">The attempt to create a system events window thread did not succeed.</exception>
	[System.MonoTODO("Currently does nothing on Mono")]
	public static event EventHandler PaletteChanged
	{
		add
		{
		}
		remove
		{
		}
	}

	/// <summary>Occurs when the user suspends or resumes the system.</summary>
	/// <exception cref="T:System.InvalidOperationException">System event notifications are not supported under the current context. Server processes, for example, might not support global system event notifications.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.ExternalException">The attempt to create a system events window thread did not succeed.</exception>
	[System.MonoTODO("Currently does nothing on Mono")]
	public static event PowerModeChangedEventHandler PowerModeChanged
	{
		add
		{
		}
		remove
		{
		}
	}

	/// <summary>Occurs when the user is logging off or shutting down the system.</summary>
	/// <exception cref="T:System.InvalidOperationException">System event notifications are not supported under the current context. Server processes, for example, might not support global system event notifications.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.ExternalException">The attempt to create a system events window thread did not succeed.</exception>
	[System.MonoTODO("Currently does nothing on Mono")]
	public static event SessionEndedEventHandler SessionEnded
	{
		add
		{
		}
		remove
		{
		}
	}

	/// <summary>Occurs when the user is trying to log off or shut down the system.</summary>
	/// <exception cref="T:System.InvalidOperationException">System event notifications are not supported under the current context. Server processes, for example, might not support global system event notifications.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.ExternalException">The attempt to create a system events window thread did not succeed.</exception>
	[System.MonoTODO("Currently does nothing on Mono")]
	public static event SessionEndingEventHandler SessionEnding
	{
		add
		{
		}
		remove
		{
		}
	}

	/// <summary>Occurs when the currently logged-in user has changed.</summary>
	/// <exception cref="T:System.InvalidOperationException">System event notifications are not supported under the current context. Server processes, for example, might not support global system event notifications.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.ExternalException">The attempt to create a system events window thread did not succeed.</exception>
	[System.MonoTODO("Currently does nothing on Mono")]
	public static event SessionSwitchEventHandler SessionSwitch
	{
		add
		{
		}
		remove
		{
		}
	}

	/// <summary>Occurs when the user changes the time on the system clock.</summary>
	/// <exception cref="T:System.InvalidOperationException">System event notifications are not supported under the current context. Server processes, for example, might not support global system event notifications.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.ExternalException">The attempt to create a system events window thread did not succeed.</exception>
	[System.MonoTODO("Currently does nothing on Mono")]
	public static event EventHandler TimeChanged
	{
		add
		{
		}
		remove
		{
		}
	}

	/// <summary>Occurs when a windows timer interval has expired.</summary>
	/// <exception cref="T:System.InvalidOperationException">System event notifications are not supported under the current context. Server processes, for example, might not support global system event notifications.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.ExternalException">The attempt to create a system events window thread did not succeed.</exception>
	public static event TimerElapsedEventHandler TimerElapsed;

	/// <summary>Occurs when a user preference has changed.</summary>
	/// <exception cref="T:System.InvalidOperationException">System event notifications are not supported under the current context. Server processes, for example, might not support global system event notifications.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.ExternalException">The attempt to create a system events window thread did not succeed.</exception>
	[System.MonoTODO("Currently does nothing on Mono")]
	public static event UserPreferenceChangedEventHandler UserPreferenceChanged
	{
		add
		{
		}
		remove
		{
		}
	}

	/// <summary>Occurs when a user preference is changing.</summary>
	/// <exception cref="T:System.InvalidOperationException">System event notifications are not supported under the current context. Server processes, for example, might not support global system event notifications.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.ExternalException">The attempt to create a system events window thread did not succeed.</exception>
	[System.MonoTODO("Currently does nothing on Mono")]
	public static event UserPreferenceChangingEventHandler UserPreferenceChanging
	{
		add
		{
		}
		remove
		{
		}
	}

	private SystemEvents()
	{
	}

	/// <summary>Creates a new window timer associated with the system events window.</summary>
	/// <returns>The ID of the new timer.</returns>
	/// <param name="interval">Specifies the interval between timer notifications, in milliseconds.</param>
	/// <exception cref="T:System.ArgumentException">The interval is less than or equal to zero. </exception>
	/// <exception cref="T:System.InvalidOperationException">System event notifications are not supported under the current context. Server processes, for example, might not support global system event notifications.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.ExternalException">The attempt to create a system events window thread did not succeed, or the attempt to create the timer did not succeed.</exception>
	public static IntPtr CreateTimer(int interval)
	{
		int hashCode = Guid.NewGuid().GetHashCode();
		Timer timer = new Timer(interval);
		timer.Elapsed += InternalTimerElapsed;
		TimerStore.Add(hashCode, timer);
		return new IntPtr(hashCode);
	}

	/// <summary>Terminates the timer specified by the given id.</summary>
	/// <param name="timerId">The ID of the timer to terminate. </param>
	/// <exception cref="T:System.InvalidOperationException">System event notifications are not supported under the current context. Server processes, for example, might not support global system event notifications.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.ExternalException">The attempt to create a system events window thread did not succeed, or the attempt to terminate the timer did not succeed. </exception>
	public static void KillTimer(IntPtr timerId)
	{
		Timer obj = (Timer)TimerStore[timerId.GetHashCode()];
		obj.Stop();
		obj.Elapsed -= InternalTimerElapsed;
		obj.Dispose();
		TimerStore.Remove(timerId.GetHashCode());
	}

	private static void InternalTimerElapsed(object e, ElapsedEventArgs args)
	{
		if (SystemEvents.TimerElapsed != null)
		{
			SystemEvents.TimerElapsed(null, new TimerElapsedEventArgs(IntPtr.Zero));
		}
	}

	/// <summary>Invokes the specified delegate using the thread that listens for system events.</summary>
	/// <param name="method">A delegate to invoke using the thread that listens for system events. </param>
	/// <exception cref="T:System.InvalidOperationException">System event notifications are not supported under the current context. Server processes, for example, might not support global system event notifications.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.ExternalException">The attempt to create a system events window thread did not succeed.</exception>
	[System.MonoTODO]
	public static void InvokeOnEventsThread(Delegate method)
	{
		throw new NotImplementedException();
	}
}
