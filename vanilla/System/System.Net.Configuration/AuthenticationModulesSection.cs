using System.Configuration;

namespace System.Net.Configuration;

/// <summary>Represents the configuration section for authentication modules. This class cannot be inherited.</summary>
public sealed class AuthenticationModulesSection : ConfigurationSection
{
	private static ConfigurationPropertyCollection properties;

	private static ConfigurationProperty authenticationModulesProp;

	protected override ConfigurationPropertyCollection Properties => properties;

	/// <summary>Gets the collection of authentication modules in the section.</summary>
	/// <returns>A <see cref="T:System.Net.Configuration.AuthenticationModuleElementCollection" /> that contains the registered authentication modules. </returns>
	[ConfigurationProperty("", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
	public AuthenticationModuleElementCollection AuthenticationModules => (AuthenticationModuleElementCollection)base[authenticationModulesProp];

	static AuthenticationModulesSection()
	{
		authenticationModulesProp = new ConfigurationProperty("", typeof(AuthenticationModuleElementCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
		properties = new ConfigurationPropertyCollection();
		properties.Add(authenticationModulesProp);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Configuration.AuthenticationModulesSection" /> class. </summary>
	public AuthenticationModulesSection()
	{
	}

	[System.MonoTODO]
	protected override void PostDeserialize()
	{
	}

	[System.MonoTODO]
	protected override void InitializeDefault()
	{
	}
}
