namespace System.Configuration;

/// <summary>Represents a group of configuration elements that configure the providers for the &lt;configProtectedData&gt; configuration section.</summary>
public class ProtectedProviderSettings : ConfigurationElement
{
	private static ConfigurationProperty providersProp;

	private static ConfigurationPropertyCollection properties;

	/// <summary>Gets a <see cref="T:System.Configuration.ConfigurationPropertyCollection" /> collection that represents the properties of the providers for the protected configuration data.</summary>
	/// <returns>A <see cref="T:System.Configuration.ConfigurationPropertyCollection" /> that represents the properties of the providers for the protected configuration data.</returns>
	protected internal override ConfigurationPropertyCollection Properties => properties;

	/// <summary>Gets a collection of <see cref="T:System.Configuration.ProviderSettings" /> objects that represent the properties of the providers for the protected configuration data.</summary>
	/// <returns>A collection of <see cref="T:System.Configuration.ProviderSettings" /> objects that represent the properties of the providers for the protected configuration data.</returns>
	[ConfigurationProperty("", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
	public ProviderSettingsCollection Providers => (ProviderSettingsCollection)base[providersProp];

	static ProtectedProviderSettings()
	{
		providersProp = new ConfigurationProperty("", typeof(ProviderSettingsCollection), null, null, null, ConfigurationPropertyOptions.IsDefaultCollection);
		properties = new ConfigurationPropertyCollection();
		properties.Add(providersProp);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ProtectedProviderSettings" /> class. </summary>
	public ProtectedProviderSettings()
	{
	}
}
