namespace System.Net.NetworkInformation;

/// <summary>Provides configuration and statistical information for a network interface.</summary>
public abstract class NetworkInterface
{
	/// <summary>Gets the index of the IPv4 loopback interface.</summary>
	/// <returns>A <see cref="T:System.Int32" /> that contains the index for the IPv4 loopback interface.</returns>
	/// <exception cref="T:System.Net.NetworkInformation.NetworkInformationException">This property is not valid on computers running only Ipv6.</exception>
	public static int LoopbackInterfaceIndex => SystemNetworkInterface.InternalLoopbackInterfaceIndex;

	/// <summary>Gets the index of the IPv6 loopback interface.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.The index for the IPv6 loopback interface.</returns>
	public static int IPv6LoopbackInterfaceIndex => SystemNetworkInterface.InternalIPv6LoopbackInterfaceIndex;

	/// <summary>Gets the identifier of the network adapter.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the identifier.</returns>
	public virtual string Id
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets the name of the network adapter.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the adapter name.</returns>
	public virtual string Name
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets the description of the interface.</summary>
	/// <returns>A <see cref="T:System.String" /> that describes this interface.</returns>
	public virtual string Description
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets the current operational state of the network connection.</summary>
	/// <returns>One of the <see cref="T:System.Net.NetworkInformation.OperationalStatus" /> values.</returns>
	public virtual OperationalStatus OperationalStatus
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets the speed of the network interface.</summary>
	/// <returns>A <see cref="T:System.Int64" /> value that specifies the speed in bits per second.</returns>
	public virtual long Speed
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets a <see cref="T:System.Boolean" /> value that indicates whether the network interface is set to only receive data packets.</summary>
	/// <returns>true if the interface only receives network traffic; otherwise, false.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">This property is not valid on computers running operating systems earlier than Windows XP. </exception>
	public virtual bool IsReceiveOnly
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets a <see cref="T:System.Boolean" /> value that indicates whether the network interface is enabled to receive multicast packets.</summary>
	/// <returns>true if the interface receives multicast packets; otherwise, false.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">This property is not valid on computers running operating systems earlier than Windows XP. </exception>
	public virtual bool SupportsMulticast
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets the interface type.</summary>
	/// <returns>An <see cref="T:System.Net.NetworkInformation.NetworkInterfaceType" /> value that specifies the network interface type.</returns>
	public virtual NetworkInterfaceType NetworkInterfaceType
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Returns objects that describe the network interfaces on the local computer.</summary>
	/// <returns>A <see cref="T:System.Net.NetworkInformation.NetworkInterface" /> array that contains objects that describe the available network interfaces, or an empty array if no interfaces are detected.</returns>
	/// <exception cref="T:System.Net.NetworkInformation.NetworkInformationException">A Windows system function call failed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Net.NetworkInformation.NetworkInformationPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Access="Read" />
	/// </PermissionSet>
	public static NetworkInterface[] GetAllNetworkInterfaces()
	{
		return SystemNetworkInterface.GetNetworkInterfaces();
	}

	/// <summary>Indicates whether any network connection is available.</summary>
	/// <returns>true if a network connection is available; otherwise, false.</returns>
	public static bool GetIsNetworkAvailable()
	{
		return SystemNetworkInterface.InternalGetIsNetworkAvailable();
	}

	/// <summary>Returns an object that describes the configuration of this network interface.</summary>
	/// <returns>An <see cref="T:System.Net.NetworkInformation.IPInterfaceProperties" /> object that describes this network interface.</returns>
	public virtual IPInterfaceProperties GetIPProperties()
	{
		throw new NotImplementedException();
	}

	/// <summary>Gets the IPv4 statistics for this <see cref="T:System.Net.NetworkInformation.NetworkInterface" /> instance.</summary>
	/// <returns>An <see cref="T:System.Net.NetworkInformation.IPv4InterfaceStatistics" /> object.</returns>
	public virtual IPv4InterfaceStatistics GetIPv4Statistics()
	{
		throw new NotImplementedException();
	}

	/// <summary>Gets the IP statistics for this <see cref="T:System.Net.NetworkInformation.NetworkInterface" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Net.NetworkInformation.IPInterfaceStatistics" />.The IP statistics.</returns>
	public virtual IPInterfaceStatistics GetIPStatistics()
	{
		throw new NotImplementedException();
	}

	/// <summary>Returns the Media Access Control (MAC) or physical address for this adapter.</summary>
	/// <returns>A <see cref="T:System.Net.NetworkInformation.PhysicalAddress" /> object that contains the physical address.</returns>
	public virtual PhysicalAddress GetPhysicalAddress()
	{
		throw new NotImplementedException();
	}

	/// <summary>Gets a <see cref="T:System.Boolean" /> value that indicates whether the interface supports the specified protocol.</summary>
	/// <returns>true if the specified protocol is supported; otherwise, false.</returns>
	/// <param name="networkInterfaceComponent">A <see cref="T:System.Net.NetworkInformation.NetworkInterfaceComponent" /> value.</param>
	public virtual bool Supports(NetworkInterfaceComponent networkInterfaceComponent)
	{
		throw new NotImplementedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.NetworkInformation.NetworkInterface" /> class.</summary>
	protected NetworkInterface()
	{
	}
}
