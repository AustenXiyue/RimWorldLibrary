using System.ComponentModel;
using Unity;

namespace System.Net.NetworkInformation;

/// <summary>Allows applications to receive notification when the Internet Protocol (IP) address of a network interface, also called a network card or adapter, changes.</summary>
public sealed class NetworkChange
{
	private static INetworkChange networkChange;

	/// <summary>Occurs when the IP address of a network interface changes.</summary>
	public static event NetworkAddressChangedEventHandler NetworkAddressChanged
	{
		add
		{
			lock (typeof(INetworkChange))
			{
				MaybeCreate();
				if (networkChange != null)
				{
					networkChange.NetworkAddressChanged += value;
				}
			}
		}
		remove
		{
			lock (typeof(INetworkChange))
			{
				if (networkChange != null)
				{
					networkChange.NetworkAddressChanged -= value;
					MaybeDispose();
				}
			}
		}
	}

	/// <summary>Occurs when the availability of the network changes.</summary>
	public static event NetworkAvailabilityChangedEventHandler NetworkAvailabilityChanged
	{
		add
		{
			lock (typeof(INetworkChange))
			{
				MaybeCreate();
				if (networkChange != null)
				{
					networkChange.NetworkAvailabilityChanged += value;
				}
			}
		}
		remove
		{
			lock (typeof(INetworkChange))
			{
				if (networkChange != null)
				{
					networkChange.NetworkAvailabilityChanged -= value;
					MaybeDispose();
				}
			}
		}
	}

	private static void MaybeCreate()
	{
		if (networkChange != null)
		{
			return;
		}
		try
		{
			networkChange = new MacNetworkChange();
		}
		catch
		{
			networkChange = new LinuxNetworkChange();
		}
	}

	private static void MaybeDispose()
	{
		if (networkChange != null && networkChange.HasRegisteredEvents)
		{
			networkChange.Dispose();
			networkChange = null;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.NetworkInformation.NetworkChange" /> class.</summary>
	public NetworkChange()
	{
	}

	/// <summary>Registers a network change instance to receive network change events.</summary>
	/// <param name="nc">The instance to register. </param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
	public static void RegisterNetworkChange(NetworkChange nc)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
