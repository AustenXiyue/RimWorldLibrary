using System.Configuration;
using System.Net.Cache;
using System.Xml;

namespace System.Net.Configuration;

/// <summary>Represents the default FTP cache policy for network resources. This class cannot be inherited.</summary>
public sealed class FtpCachePolicyElement : ConfigurationElement
{
	private static ConfigurationProperty policyLevelProp;

	private static ConfigurationPropertyCollection properties;

	/// <summary>Gets or sets FTP caching behavior for the local machine.</summary>
	/// <returns>A <see cref="T:System.Net.Cache.RequestCacheLevel" /> value that specifies the cache behavior.</returns>
	[ConfigurationProperty("policyLevel", DefaultValue = "Default")]
	public RequestCacheLevel PolicyLevel
	{
		get
		{
			return (RequestCacheLevel)base[policyLevelProp];
		}
		set
		{
			base[policyLevelProp] = value;
		}
	}

	protected override ConfigurationPropertyCollection Properties => properties;

	static FtpCachePolicyElement()
	{
		policyLevelProp = new ConfigurationProperty("policyLevel", typeof(RequestCacheLevel), RequestCacheLevel.Default);
		properties = new ConfigurationPropertyCollection();
		properties.Add(policyLevelProp);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Configuration.FtpCachePolicyElement" /> class.</summary>
	public FtpCachePolicyElement()
	{
	}

	[System.MonoTODO]
	protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
	{
		throw new NotImplementedException();
	}

	[System.MonoTODO]
	protected override void Reset(ConfigurationElement parentElement)
	{
		throw new NotImplementedException();
	}
}
