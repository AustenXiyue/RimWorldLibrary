namespace System.Configuration;

/// <summary>Provides the configuration setting for International Domain Name (IDN) processing in the <see cref="T:System.Uri" /> class.</summary>
public sealed class IdnElement : ConfigurationElement
{
	private static ConfigurationPropertyCollection properties;

	private static ConfigurationProperty enabled_prop;

	internal const UriIdnScope EnabledDefaultValue = UriIdnScope.None;

	/// <summary>Gets or sets the value of the <see cref="T:System.Configuration.IdnElement" /> configuration setting. </summary>
	/// <returns>A <see cref="T:System.UriIdnScope" /> that contains the current configuration setting for IDN processing.</returns>
	[ConfigurationProperty("enabled", DefaultValue = UriIdnScope.None, Options = (ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey))]
	public UriIdnScope Enabled
	{
		get
		{
			return (UriIdnScope)base[enabled_prop];
		}
		set
		{
			base[enabled_prop] = value;
		}
	}

	protected override ConfigurationPropertyCollection Properties => properties;

	static IdnElement()
	{
		enabled_prop = new ConfigurationProperty("enabled", typeof(UriIdnScope), UriIdnScope.None, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
		properties = new ConfigurationPropertyCollection();
		properties.Add(enabled_prop);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.IdnElement" /> class.</summary>
	public IdnElement()
	{
	}

	public override bool Equals(object o)
	{
		if (!(o is IdnElement idnElement))
		{
			return false;
		}
		return idnElement.Enabled == Enabled;
	}

	public override int GetHashCode()
	{
		return (int)(Enabled ^ (UriIdnScope)127);
	}
}
