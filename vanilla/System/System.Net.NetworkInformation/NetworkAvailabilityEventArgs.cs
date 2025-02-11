using Unity;

namespace System.Net.NetworkInformation;

/// <summary>Provides data for the <see cref="E:System.Net.NetworkInformation.NetworkChange.NetworkAvailabilityChanged" /> event.</summary>
public class NetworkAvailabilityEventArgs : EventArgs
{
	private bool isAvailable;

	/// <summary>Gets the current status of the network connection.</summary>
	/// <returns>true if the network is available; otherwise, false.</returns>
	public bool IsAvailable => isAvailable;

	internal NetworkAvailabilityEventArgs(bool isAvailable)
	{
		this.isAvailable = isAvailable;
	}

	internal NetworkAvailabilityEventArgs()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
