using System.Configuration;
using System.Net.Cache;
using System.Xml;

namespace System.Net.Configuration;

/// <summary>Represents the default HTTP cache policy for network resources. This class cannot be inherited.</summary>
public sealed class HttpCachePolicyElement : ConfigurationElement
{
	private static ConfigurationProperty maximumAgeProp;

	private static ConfigurationProperty maximumStaleProp;

	private static ConfigurationProperty minimumFreshProp;

	private static ConfigurationProperty policyLevelProp;

	private static ConfigurationPropertyCollection properties;

	/// <summary>Gets or sets the maximum age permitted for a resource returned from the cache.</summary>
	/// <returns>A <see cref="T:System.TimeSpan" /> value that specifies the maximum age for cached resources specified in the configuration file.</returns>
	[ConfigurationProperty("maximumAge", DefaultValue = "10675199.02:48:05.4775807")]
	public TimeSpan MaximumAge
	{
		get
		{
			return (TimeSpan)base[maximumAgeProp];
		}
		set
		{
			base[maximumAgeProp] = value;
		}
	}

	/// <summary>Gets or sets the maximum staleness value permitted for a resource returned from the cache.</summary>
	/// <returns>A <see cref="T:System.TimeSpan" /> value that is set to the maximum staleness value specified in the configuration file.</returns>
	[ConfigurationProperty("maximumStale", DefaultValue = "-10675199.02:48:05.4775808")]
	public TimeSpan MaximumStale
	{
		get
		{
			return (TimeSpan)base[maximumStaleProp];
		}
		set
		{
			base[maximumStaleProp] = value;
		}
	}

	/// <summary>Gets or sets the minimum freshness permitted for a resource returned from the cache.</summary>
	/// <returns>A <see cref="T:System.TimeSpan" /> value that specifies the minimum freshness specified in the configuration file.</returns>
	[ConfigurationProperty("minimumFresh", DefaultValue = "-10675199.02:48:05.4775808")]
	public TimeSpan MinimumFresh
	{
		get
		{
			return (TimeSpan)base[minimumFreshProp];
		}
		set
		{
			base[minimumFreshProp] = value;
		}
	}

	/// <summary>Gets or sets HTTP caching behavior for the local machine.</summary>
	/// <returns>A <see cref="T:System.Net.Cache.HttpRequestCacheLevel" /> value that specifies the cache behavior.</returns>
	[ConfigurationProperty("policyLevel", DefaultValue = "Default", Options = ConfigurationPropertyOptions.IsRequired)]
	public HttpRequestCacheLevel PolicyLevel
	{
		get
		{
			return (HttpRequestCacheLevel)base[policyLevelProp];
		}
		set
		{
			base[policyLevelProp] = value;
		}
	}

	protected override ConfigurationPropertyCollection Properties => properties;

	static HttpCachePolicyElement()
	{
		maximumAgeProp = new ConfigurationProperty("maximumAge", typeof(TimeSpan), TimeSpan.MaxValue);
		maximumStaleProp = new ConfigurationProperty("maximumStale", typeof(TimeSpan), TimeSpan.MinValue);
		minimumFreshProp = new ConfigurationProperty("minimumFresh", typeof(TimeSpan), TimeSpan.MinValue);
		policyLevelProp = new ConfigurationProperty("policyLevel", typeof(HttpRequestCacheLevel), HttpRequestCacheLevel.Default, ConfigurationPropertyOptions.IsRequired);
		properties = new ConfigurationPropertyCollection();
		properties.Add(maximumAgeProp);
		properties.Add(maximumStaleProp);
		properties.Add(minimumFreshProp);
		properties.Add(policyLevelProp);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Configuration.HttpCachePolicyElement" /> class. </summary>
	public HttpCachePolicyElement()
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
