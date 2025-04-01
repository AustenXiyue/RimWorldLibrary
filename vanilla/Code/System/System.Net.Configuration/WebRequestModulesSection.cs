using System.Configuration;

namespace System.Net.Configuration;

/// <summary>Represents the configuration section for Web request modules. This class cannot be inherited.</summary>
public sealed class WebRequestModulesSection : ConfigurationSection
{
	private static ConfigurationPropertyCollection properties;

	private static ConfigurationProperty webRequestModulesProp;

	protected override ConfigurationPropertyCollection Properties => properties;

	/// <summary>Gets the collection of Web request modules in the section.</summary>
	/// <returns>A <see cref="T:System.Net.Configuration.WebRequestModuleElementCollection" /> containing the registered Web request modules. </returns>
	[ConfigurationProperty("", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
	public WebRequestModuleElementCollection WebRequestModules => (WebRequestModuleElementCollection)base[webRequestModulesProp];

	static WebRequestModulesSection()
	{
		webRequestModulesProp = new ConfigurationProperty("", typeof(WebRequestModuleElementCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
		properties = new ConfigurationPropertyCollection();
		properties.Add(webRequestModulesProp);
	}

	[System.MonoTODO]
	protected override void PostDeserialize()
	{
	}

	[System.MonoTODO]
	protected override void InitializeDefault()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Configuration.WebRequestModulesSection" /> class.</summary>
	public WebRequestModulesSection()
	{
	}
}
