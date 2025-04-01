namespace System.Configuration;

/// <summary>Provides programmatic access to the connection strings configuration-file section. </summary>
public sealed class ConnectionStringsSection : ConfigurationSection
{
	private static readonly ConfigurationProperty _propConnectionStrings;

	private static ConfigurationPropertyCollection _properties;

	/// <summary>Gets a <see cref="T:System.Configuration.ConnectionStringSettingsCollection" /> collection of <see cref="T:System.Configuration.ConnectionStringSettings" /> objects.</summary>
	/// <returns>A <see cref="T:System.Configuration.ConnectionStringSettingsCollection" /> collection of <see cref="T:System.Configuration.ConnectionStringSettings" /> objects.</returns>
	[ConfigurationProperty("", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
	public ConnectionStringSettingsCollection ConnectionStrings => (ConnectionStringSettingsCollection)base[_propConnectionStrings];

	protected internal override ConfigurationPropertyCollection Properties => _properties;

	static ConnectionStringsSection()
	{
		_propConnectionStrings = new ConfigurationProperty("", typeof(ConnectionStringSettingsCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
		_properties = new ConfigurationPropertyCollection();
		_properties.Add(_propConnectionStrings);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConnectionStringsSection" /> class.</summary>
	public ConnectionStringsSection()
	{
	}

	protected internal override object GetRuntimeObject()
	{
		return base.GetRuntimeObject();
	}
}
