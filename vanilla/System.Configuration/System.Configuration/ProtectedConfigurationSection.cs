using System.Xml;

namespace System.Configuration;

/// <summary>Provides programmatic access to the configProtectedData configuration section. This class cannot be inherited.</summary>
public sealed class ProtectedConfigurationSection : ConfigurationSection
{
	private static ConfigurationProperty defaultProviderProp;

	private static ConfigurationProperty providersProp;

	private static ConfigurationPropertyCollection properties;

	private ProtectedConfigurationProviderCollection providers;

	/// <summary>Gets or sets the name of the default <see cref="T:System.Configuration.ProtectedConfigurationProvider" /> object in the <see cref="P:System.Configuration.ProtectedConfigurationSection.Providers" /> collection property.</summary>
	/// <returns>The name of the default <see cref="T:System.Configuration.ProtectedConfigurationProvider" /> object in the <see cref="P:System.Configuration.ProtectedConfigurationSection.Providers" /> collection property. </returns>
	[ConfigurationProperty("defaultProvider", DefaultValue = "RsaProtectedConfigurationProvider")]
	public string DefaultProvider
	{
		get
		{
			return (string)base[defaultProviderProp];
		}
		set
		{
			base[defaultProviderProp] = value;
		}
	}

	/// <summary>Gets a <see cref="T:System.Configuration.ProviderSettingsCollection" /> collection of all the <see cref="T:System.Configuration.ProtectedConfigurationProvider" /> objects in all participating configuration files.</summary>
	/// <returns>A <see cref="T:System.Configuration.ProviderSettingsCollection" /> collection of all the <see cref="T:System.Configuration.ProtectedConfigurationProvider" /> objects in all participating configuration files. </returns>
	[ConfigurationProperty("providers")]
	public ProviderSettingsCollection Providers => (ProviderSettingsCollection)base[providersProp];

	protected internal override ConfigurationPropertyCollection Properties => properties;

	static ProtectedConfigurationSection()
	{
		defaultProviderProp = new ConfigurationProperty("defaultProvider", typeof(string), "RsaProtectedConfigurationProvider");
		providersProp = new ConfigurationProperty("providers", typeof(ProviderSettingsCollection), null);
		properties = new ConfigurationPropertyCollection();
		properties.Add(defaultProviderProp);
		properties.Add(providersProp);
	}

	internal string EncryptSection(string clearXml, ProtectedConfigurationProvider protectionProvider)
	{
		XmlDocument xmlDocument = new ConfigurationXmlDocument();
		xmlDocument.LoadXml(clearXml);
		return protectionProvider.Encrypt(xmlDocument.DocumentElement).OuterXml;
	}

	internal string DecryptSection(string encryptedXml, ProtectedConfigurationProvider protectionProvider)
	{
		XmlDocument xmlDocument = new ConfigurationXmlDocument();
		xmlDocument.InnerXml = encryptedXml;
		return protectionProvider.Decrypt(xmlDocument.DocumentElement).OuterXml;
	}

	internal ProtectedConfigurationProviderCollection GetAllProviders()
	{
		if (providers == null)
		{
			providers = new ProtectedConfigurationProviderCollection();
			foreach (ProviderSettings provider in Providers)
			{
				providers.Add(InstantiateProvider(provider));
			}
		}
		return providers;
	}

	private ProtectedConfigurationProvider InstantiateProvider(ProviderSettings ps)
	{
		ProtectedConfigurationProvider obj = (Activator.CreateInstance(Type.GetType(ps.Type, throwOnError: true)) as ProtectedConfigurationProvider) ?? throw new Exception("The type specified does not extend ProtectedConfigurationProvider class.");
		obj.Initialize(ps.Name, ps.Parameters);
		return obj;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ProtectedConfigurationSection" /> class using default settings.</summary>
	public ProtectedConfigurationSection()
	{
	}
}
