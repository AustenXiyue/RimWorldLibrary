namespace System.Configuration;

/// <summary>A configuration element that contains a <see cref="T:System.String" /> name and <see cref="T:System.String" /> value. This class cannot be inherited.</summary>
public sealed class NameValueConfigurationElement : ConfigurationElement
{
	private static ConfigurationPropertyCollection _properties;

	private static readonly ConfigurationProperty _propName;

	private static readonly ConfigurationProperty _propValue;

	/// <summary>Gets the name of the <see cref="T:System.Configuration.NameValueConfigurationElement" /> object.</summary>
	/// <returns>The name of the <see cref="T:System.Configuration.NameValueConfigurationElement" /> object.</returns>
	[ConfigurationProperty("name", DefaultValue = "", Options = ConfigurationPropertyOptions.IsKey)]
	public string Name => (string)base[_propName];

	/// <summary>Gets or sets the value of the <see cref="T:System.Configuration.NameValueConfigurationElement" /> object.</summary>
	/// <returns>The value of the <see cref="T:System.Configuration.NameValueConfigurationElement" /> object.</returns>
	[ConfigurationProperty("value", DefaultValue = "", Options = ConfigurationPropertyOptions.None)]
	public string Value
	{
		get
		{
			return (string)base[_propValue];
		}
		set
		{
			base[_propValue] = value;
		}
	}

	protected internal override ConfigurationPropertyCollection Properties => _properties;

	static NameValueConfigurationElement()
	{
		_properties = new ConfigurationPropertyCollection();
		_propName = new ConfigurationProperty("name", typeof(string), "", ConfigurationPropertyOptions.IsKey);
		_propValue = new ConfigurationProperty("value", typeof(string), "");
		_properties.Add(_propName);
		_properties.Add(_propValue);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.NameValueConfigurationElement" /> class based on supplied parameters.</summary>
	/// <param name="name">The name of the <see cref="T:System.Configuration.NameValueConfigurationElement" /> object.</param>
	/// <param name="value">The value of the <see cref="T:System.Configuration.NameValueConfigurationElement" /> object.</param>
	public NameValueConfigurationElement(string name, string value)
	{
		base[_propName] = name;
		base[_propValue] = value;
	}
}
