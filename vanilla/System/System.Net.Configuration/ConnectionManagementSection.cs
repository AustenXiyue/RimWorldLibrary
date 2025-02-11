using System.Configuration;

namespace System.Net.Configuration;

/// <summary>Represents the configuration section for connection management. This class cannot be inherited.</summary>
public sealed class ConnectionManagementSection : ConfigurationSection
{
	private static ConfigurationProperty connectionManagementProp;

	private static ConfigurationPropertyCollection properties;

	/// <summary>Gets the collection of connection management objects in the section.</summary>
	/// <returns>A <see cref="T:System.Net.Configuration.ConnectionManagementElementCollection" /> that contains the connection management information for the local computer. </returns>
	[ConfigurationProperty("", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
	public ConnectionManagementElementCollection ConnectionManagement => (ConnectionManagementElementCollection)base[connectionManagementProp];

	protected override ConfigurationPropertyCollection Properties => properties;

	static ConnectionManagementSection()
	{
		connectionManagementProp = new ConfigurationProperty("ConnectionManagement", typeof(ConnectionManagementElementCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
		properties = new ConfigurationPropertyCollection();
		properties.Add(connectionManagementProp);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Configuration.ConnectionManagementSection" /> class. </summary>
	public ConnectionManagementSection()
	{
	}
}
