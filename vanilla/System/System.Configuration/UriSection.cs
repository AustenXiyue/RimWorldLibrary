using Unity;

namespace System.Configuration;

/// <summary>Represents the Uri section within a configuration file.</summary>
public sealed class UriSection : ConfigurationSection
{
	private static ConfigurationPropertyCollection properties;

	private static ConfigurationProperty idn_prop;

	private static ConfigurationProperty iriParsing_prop;

	/// <summary>Gets an <see cref="T:System.Configuration.IdnElement" /> object that contains the configuration setting for International Domain Name (IDN) processing in the <see cref="T:System.Uri" /> class.</summary>
	/// <returns>The configuration setting for International Domain Name (IDN) processing in the <see cref="T:System.Uri" /> class.</returns>
	[ConfigurationProperty("idn")]
	public IdnElement Idn => (IdnElement)base[idn_prop];

	/// <summary>Gets an <see cref="T:System.Configuration.IriParsingElement" /> object that contains the configuration setting for International Resource Identifiers (IRI) parsing in the <see cref="T:System.Uri" /> class.</summary>
	/// <returns>The configuration setting for International Resource Identifiers (IRI) parsing in the <see cref="T:System.Uri" /> class.</returns>
	[ConfigurationProperty("iriParsing")]
	public IriParsingElement IriParsing => (IriParsingElement)base[iriParsing_prop];

	protected override ConfigurationPropertyCollection Properties => properties;

	/// <summary>Gets a <see cref="T:System.Configuration.SchemeSettingElementCollection" /> object that contains the configuration settings for scheme parsing in the <see cref="T:System.Uri" /> class.</summary>
	/// <returns>The configuration settings for scheme parsing in the <see cref="T:System.Uri" /> class</returns>
	public SchemeSettingElementCollection SchemeSettings
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	static UriSection()
	{
		idn_prop = new ConfigurationProperty("idn", typeof(IdnElement), null);
		iriParsing_prop = new ConfigurationProperty("iriParsing", typeof(IriParsingElement), null);
		properties = new ConfigurationPropertyCollection();
		properties.Add(idn_prop);
		properties.Add(iriParsing_prop);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.UriSection" /> class.</summary>
	public UriSection()
	{
	}
}
