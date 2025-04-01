namespace System.Configuration;

/// <summary>Represents a group of user-scoped application settings in a configuration file.</summary>
/// <filterpriority>2</filterpriority>
public sealed class ClientSettingsSection : ConfigurationSection
{
	private static ConfigurationPropertyCollection properties;

	private static ConfigurationProperty settings_prop;

	/// <summary>Gets the collection of client settings for the section.</summary>
	/// <returns>A <see cref="T:System.Configuration.SettingElementCollection" /> containing all the client settings found in the current configuration section.</returns>
	/// <filterpriority>2</filterpriority>
	[ConfigurationProperty("", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
	public SettingElementCollection Settings => (SettingElementCollection)base[settings_prop];

	protected override ConfigurationPropertyCollection Properties => properties;

	static ClientSettingsSection()
	{
		settings_prop = new ConfigurationProperty("", typeof(SettingElementCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
		properties = new ConfigurationPropertyCollection();
		properties.Add(settings_prop);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ClientSettingsSection" /> class.</summary>
	public ClientSettingsSection()
	{
	}
}
