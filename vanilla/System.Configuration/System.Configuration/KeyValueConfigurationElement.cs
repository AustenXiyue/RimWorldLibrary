namespace System.Configuration;

/// <summary>Represents a configuration element that contains a key/value pair. </summary>
public class KeyValueConfigurationElement : ConfigurationElement
{
	private static ConfigurationProperty keyProp;

	private static ConfigurationProperty valueProp;

	private static ConfigurationPropertyCollection properties;

	/// <summary>Gets the key of the <see cref="T:System.Configuration.KeyValueConfigurationElement" /> object.</summary>
	/// <returns>The key of the <see cref="T:System.Configuration.KeyValueConfigurationElement" />.</returns>
	[ConfigurationProperty("key", DefaultValue = "", Options = ConfigurationPropertyOptions.IsKey)]
	public string Key => (string)base[keyProp];

	/// <summary>Gets or sets the value of the <see cref="T:System.Configuration.KeyValueConfigurationElement" /> object.</summary>
	/// <returns>The value of the <see cref="T:System.Configuration.KeyValueConfigurationElement" />.</returns>
	[ConfigurationProperty("value", DefaultValue = "")]
	public string Value
	{
		get
		{
			return (string)base[valueProp];
		}
		set
		{
			base[valueProp] = value;
		}
	}

	/// <summary>Gets the collection of properties. </summary>
	/// <returns>The <see cref="T:System.Configuration.ConfigurationPropertyCollection" /> of properties for the element.</returns>
	protected internal override ConfigurationPropertyCollection Properties => properties;

	static KeyValueConfigurationElement()
	{
		keyProp = new ConfigurationProperty("key", typeof(string), "", ConfigurationPropertyOptions.IsKey);
		valueProp = new ConfigurationProperty("value", typeof(string), "");
		properties = new ConfigurationPropertyCollection();
		properties.Add(keyProp);
		properties.Add(valueProp);
	}

	internal KeyValueConfigurationElement()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.KeyValueConfigurationElement" /> class based on the supplied parameters.</summary>
	/// <param name="key">The key of the <see cref="T:System.Configuration.KeyValueConfigurationElement" />.</param>
	/// <param name="value">The value of the <see cref="T:System.Configuration.KeyValueConfigurationElement" />.</param>
	public KeyValueConfigurationElement(string key, string value)
	{
		base[keyProp] = key;
		base[valueProp] = value;
	}

	/// <summary>Sets the <see cref="T:System.Configuration.KeyValueConfigurationElement" /> object to its initial state.</summary>
	[System.MonoTODO]
	protected internal override void Init()
	{
	}
}
