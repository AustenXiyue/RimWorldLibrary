using System.Configuration;

namespace System.Net.Configuration;

/// <summary>Represents the configuration section for Web proxy server usage. This class cannot be inherited.</summary>
public sealed class DefaultProxySection : ConfigurationSection
{
	private static ConfigurationPropertyCollection properties;

	private static ConfigurationProperty bypassListProp;

	private static ConfigurationProperty enabledProp;

	private static ConfigurationProperty moduleProp;

	private static ConfigurationProperty proxyProp;

	private static ConfigurationProperty useDefaultCredentialsProp;

	/// <summary>Gets the collection of resources that are not obtained using the Web proxy server.</summary>
	/// <returns>A <see cref="T:System.Net.Configuration.BypassElementCollection" /> that contains the addresses of resources that bypass the Web proxy server. </returns>
	[ConfigurationProperty("bypasslist")]
	public BypassElementCollection BypassList => (BypassElementCollection)base[bypassListProp];

	/// <summary>Gets or sets whether a Web proxy is used.</summary>
	/// <returns>true if a Web proxy will be used; otherwise, false.</returns>
	[ConfigurationProperty("enabled", DefaultValue = "True")]
	public bool Enabled
	{
		get
		{
			return (bool)base[enabledProp];
		}
		set
		{
			base[enabledProp] = value;
		}
	}

	/// <summary>Gets the type information for a custom Web proxy implementation.</summary>
	/// <returns>A <see cref="T:System.Net.Configuration.ModuleElement" />. The type information for a custom Web proxy implementation.</returns>
	[ConfigurationProperty("module")]
	public ModuleElement Module => (ModuleElement)base[moduleProp];

	/// <summary>Gets the URI that identifies the Web proxy server to use.</summary>
	/// <returns>A <see cref="T:System.Net.Configuration.ProxyElement" />. The URI that identifies the Web proxy server.</returns>
	[ConfigurationProperty("proxy")]
	public ProxyElement Proxy => (ProxyElement)base[proxyProp];

	/// <summary>Gets or sets whether default credentials are to be used to access a Web proxy server.</summary>
	/// <returns>true if default credentials are to be used; otherwise, false.</returns>
	[ConfigurationProperty("useDefaultCredentials", DefaultValue = "False")]
	public bool UseDefaultCredentials
	{
		get
		{
			return (bool)base[useDefaultCredentialsProp];
		}
		set
		{
			base[useDefaultCredentialsProp] = value;
		}
	}

	protected override ConfigurationPropertyCollection Properties => properties;

	static DefaultProxySection()
	{
		bypassListProp = new ConfigurationProperty("bypasslist", typeof(BypassElementCollection), null);
		enabledProp = new ConfigurationProperty("enabled", typeof(bool), true);
		moduleProp = new ConfigurationProperty("module", typeof(ModuleElement), null);
		proxyProp = new ConfigurationProperty("proxy", typeof(ProxyElement), null);
		useDefaultCredentialsProp = new ConfigurationProperty("useDefaultCredentials", typeof(bool), false);
		properties = new ConfigurationPropertyCollection();
		properties.Add(bypassListProp);
		properties.Add(enabledProp);
		properties.Add(moduleProp);
		properties.Add(proxyProp);
		properties.Add(useDefaultCredentialsProp);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Configuration.DefaultProxySection" /> class. </summary>
	public DefaultProxySection()
	{
	}

	[System.MonoTODO]
	protected override void PostDeserialize()
	{
	}

	[System.MonoTODO]
	protected override void Reset(ConfigurationElement parentElement)
	{
	}
}
