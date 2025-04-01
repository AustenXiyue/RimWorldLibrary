using System.Configuration;

namespace System.Net.Configuration;

/// <summary>Represents the maximum number of connections to a remote computer. This class cannot be inherited.</summary>
public sealed class ConnectionManagementElement : ConfigurationElement
{
	private static ConfigurationPropertyCollection properties;

	private static ConfigurationProperty addressProp;

	private static ConfigurationProperty maxConnectionProp;

	/// <summary>Gets or sets the address for remote computers.</summary>
	/// <returns>A string that contains a regular expression describing an IP address or DNS name.</returns>
	[ConfigurationProperty("address", Options = (ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey))]
	public string Address
	{
		get
		{
			return (string)base[addressProp];
		}
		set
		{
			base[addressProp] = value;
		}
	}

	/// <summary>Gets or sets the maximum number of connections that can be made to a remote computer.</summary>
	/// <returns>An integer that specifies the maximum number of connections.</returns>
	[ConfigurationProperty("maxconnection", DefaultValue = "6", Options = ConfigurationPropertyOptions.IsRequired)]
	public int MaxConnection
	{
		get
		{
			return (int)base[maxConnectionProp];
		}
		set
		{
			base[maxConnectionProp] = value;
		}
	}

	protected override ConfigurationPropertyCollection Properties => properties;

	static ConnectionManagementElement()
	{
		addressProp = new ConfigurationProperty("address", typeof(string), null, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
		maxConnectionProp = new ConfigurationProperty("maxconnection", typeof(int), 1, ConfigurationPropertyOptions.IsRequired);
		properties = new ConfigurationPropertyCollection();
		properties.Add(addressProp);
		properties.Add(maxConnectionProp);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Configuration.ConnectionManagementElement" /> class. </summary>
	public ConnectionManagementElement()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Configuration.ConnectionManagementElement" /> class with the specified address and connection limit information.</summary>
	/// <param name="address">A string that identifies the address of a remote computer.</param>
	/// <param name="maxConnection">An integer that identifies the maximum number of connections allowed to <paramref name="address" /> from the local computer.</param>
	public ConnectionManagementElement(string address, int maxConnection)
	{
		Address = address;
		MaxConnection = maxConnection;
	}
}
