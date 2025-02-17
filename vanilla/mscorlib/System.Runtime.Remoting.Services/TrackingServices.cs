using System.Collections;
using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Services;

/// <summary>Provides a way to register, unregister, and obtain a list of tracking handlers.</summary>
[ComVisible(true)]
public class TrackingServices
{
	private static ArrayList _handlers = new ArrayList();

	/// <summary>Gets an array of the tracking handlers that are currently registered with <see cref="T:System.Runtime.Remoting.Services.TrackingServices" /> in the current <see cref="T:System.AppDomain" />.</summary>
	/// <returns>An array of the tracking handlers that are currently registered with <see cref="T:System.Runtime.Remoting.Services.TrackingServices" /> in the current <see cref="T:System.AppDomain" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static ITrackingHandler[] RegisteredHandlers
	{
		get
		{
			lock (_handlers.SyncRoot)
			{
				if (_handlers.Count == 0)
				{
					return new ITrackingHandler[0];
				}
				return (ITrackingHandler[])_handlers.ToArray(typeof(ITrackingHandler));
			}
		}
	}

	/// <summary>Creates an instance of <see cref="T:System.Runtime.Remoting.Services.TrackingServices" />.</summary>
	public TrackingServices()
	{
	}

	/// <summary>Registers a new tracking handler with the <see cref="T:System.Runtime.Remoting.Services.TrackingServices" />.</summary>
	/// <param name="handler">The tracking handler to register. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="handler" /> is null. </exception>
	/// <exception cref="T:System.Runtime.Remoting.RemotingException">The handler that is indicated in the <paramref name="handler" /> parameter is already registered with <see cref="T:System.Runtime.Remoting.Services.TrackingServices" />. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static void RegisterTrackingHandler(ITrackingHandler handler)
	{
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		lock (_handlers.SyncRoot)
		{
			if (-1 != _handlers.IndexOf(handler))
			{
				throw new RemotingException("handler already registered");
			}
			_handlers.Add(handler);
		}
	}

	/// <summary>Unregisters the specified tracking handler from <see cref="T:System.Runtime.Remoting.Services.TrackingServices" />.</summary>
	/// <param name="handler">The handler to unregister. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="handler" /> is null. </exception>
	/// <exception cref="T:System.Runtime.Remoting.RemotingException">The handler that is indicated in the <paramref name="handler" /> parameter is not registered with <see cref="T:System.Runtime.Remoting.Services.TrackingServices" />. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static void UnregisterTrackingHandler(ITrackingHandler handler)
	{
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		lock (_handlers.SyncRoot)
		{
			int num = _handlers.IndexOf(handler);
			if (num == -1)
			{
				throw new RemotingException("handler is not registered");
			}
			_handlers.RemoveAt(num);
		}
	}

	internal static void NotifyMarshaledObject(object obj, ObjRef or)
	{
		ITrackingHandler[] array;
		lock (_handlers.SyncRoot)
		{
			if (_handlers.Count == 0)
			{
				return;
			}
			array = (ITrackingHandler[])_handlers.ToArray(typeof(ITrackingHandler));
		}
		for (int i = 0; i < array.Length; i++)
		{
			array[i].MarshaledObject(obj, or);
		}
	}

	internal static void NotifyUnmarshaledObject(object obj, ObjRef or)
	{
		ITrackingHandler[] array;
		lock (_handlers.SyncRoot)
		{
			if (_handlers.Count == 0)
			{
				return;
			}
			array = (ITrackingHandler[])_handlers.ToArray(typeof(ITrackingHandler));
		}
		for (int i = 0; i < array.Length; i++)
		{
			array[i].UnmarshaledObject(obj, or);
		}
	}

	internal static void NotifyDisconnectedObject(object obj)
	{
		ITrackingHandler[] array;
		lock (_handlers.SyncRoot)
		{
			if (_handlers.Count == 0)
			{
				return;
			}
			array = (ITrackingHandler[])_handlers.ToArray(typeof(ITrackingHandler));
		}
		for (int i = 0; i < array.Length; i++)
		{
			array[i].DisconnectedObject(obj);
		}
	}
}
