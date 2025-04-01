using System.ComponentModel;

namespace System.Configuration;

/// <summary>Represents a single, named connection string in the connection strings configuration file section.</summary>
public sealed class ConnectionStringSettings : ConfigurationElement
{
	private static ConfigurationPropertyCollection _properties;

	private static readonly ConfigurationProperty _propConnectionString;

	private static readonly ConfigurationProperty _propName;

	private static readonly ConfigurationProperty _propProviderName;

	protected internal override ConfigurationPropertyCollection Properties => _properties;

	/// <summary>Gets or sets the <see cref="T:System.Configuration.ConnectionStringSettings" /> name.</summary>
	/// <returns>The string value assigned to the <see cref="P:System.Configuration.ConnectionStringSettings.Name" /> property.</returns>
	[ConfigurationProperty("name", DefaultValue = "", Options = (ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey))]
	public string Name
	{
		get
		{
			return (string)base[_propName];
		}
		set
		{
			base[_propName] = value;
		}
	}

	/// <summary>Gets or sets the provider name property.</summary>
	/// <returns>Gets or sets the <see cref="P:System.Configuration.ConnectionStringSettings.ProviderName" /> property.</returns>
	[ConfigurationProperty("providerName", DefaultValue = "System.Data.SqlClient")]
	public string ProviderName
	{
		get
		{
			return (string)base[_propProviderName];
		}
		set
		{
			base[_propProviderName] = value;
		}
	}

	/// <summary>Gets or sets the connection string.</summary>
	/// <returns>The string value assigned to the <see cref="P:System.Configuration.ConnectionStringSettings.ConnectionString" /> property.</returns>
	[ConfigurationProperty("connectionString", DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired)]
	public string ConnectionString
	{
		get
		{
			return (string)base[_propConnectionString];
		}
		set
		{
			base[_propConnectionString] = value;
		}
	}

	static ConnectionStringSettings()
	{
		_properties = new ConfigurationPropertyCollection();
		_propName = new ConfigurationProperty("name", typeof(string), null, TypeDescriptor.GetConverter(typeof(string)), null, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
		_propProviderName = new ConfigurationProperty("providerName", typeof(string), "", ConfigurationPropertyOptions.None);
		_propConnectionString = new ConfigurationProperty("connectionString", typeof(string), "", ConfigurationPropertyOptions.IsRequired);
		_properties.Add(_propName);
		_properties.Add(_propProviderName);
		_properties.Add(_propConnectionString);
	}

	/// <summary>Initializes a new instance of a <see cref="T:System.Configuration.ConnectionStringSettings" /> class.</summary>
	public ConnectionStringSettings()
	{
	}

	/// <summary>Initializes a new instance of a <see cref="T:System.Configuration.ConnectionStringSettings" /> class.</summary>
	/// <param name="name">The name of the connection string.</param>
	/// <param name="connectionString">The connection string.</param>
	public ConnectionStringSettings(string name, string connectionString)
		: this(name, connectionString, "")
	{
	}

	/// <summary>Initializes a new instance of a <see cref="T:System.Configuration.ConnectionStringSettings" /> object.</summary>
	/// <param name="name">The name of the connection string.</param>
	/// <param name="connectionString">The connection string.</param>
	/// <param name="providerName">The name of the provider to use with the connection string.</param>
	public ConnectionStringSettings(string name, string connectionString, string providerName)
	{
		Name = name;
		ConnectionString = connectionString;
		ProviderName = providerName;
	}

	/// <summary>Returns a string representation of the object.</summary>
	/// <returns>A string representation of the object.</returns>
	public override string ToString()
	{
		return ConnectionString;
	}
}
