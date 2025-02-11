using System.Configuration;
using System.Net.Cache;
using System.Xml;

namespace System.Net.Configuration;

/// <summary>Represents the configuration section for cache behavior. This class cannot be inherited.</summary>
public sealed class RequestCachingSection : ConfigurationSection
{
	private static ConfigurationPropertyCollection properties;

	private static ConfigurationProperty defaultFtpCachePolicyProp;

	private static ConfigurationProperty defaultHttpCachePolicyProp;

	private static ConfigurationProperty defaultPolicyLevelProp;

	private static ConfigurationProperty disableAllCachingProp;

	private static ConfigurationProperty isPrivateCacheProp;

	private static ConfigurationProperty unspecifiedMaximumAgeProp;

	/// <summary>Gets the default FTP caching behavior for the local computer.</summary>
	/// <returns>A <see cref="T:System.Net.Configuration.FtpCachePolicyElement" /> that defines the default cache policy.</returns>
	[ConfigurationProperty("defaultFtpCachePolicy")]
	public FtpCachePolicyElement DefaultFtpCachePolicy => (FtpCachePolicyElement)base[defaultFtpCachePolicyProp];

	/// <summary>Gets the default caching behavior for the local computer.</summary>
	/// <returns>A <see cref="T:System.Net.Configuration.HttpCachePolicyElement" /> that defines the default cache policy.</returns>
	[ConfigurationProperty("defaultHttpCachePolicy")]
	public HttpCachePolicyElement DefaultHttpCachePolicy => (HttpCachePolicyElement)base[defaultHttpCachePolicyProp];

	/// <summary>Gets or sets the default cache policy level.</summary>
	/// <returns>A <see cref="T:System.Net.Cache.RequestCacheLevel" /> enumeration value.</returns>
	[ConfigurationProperty("defaultPolicyLevel", DefaultValue = "BypassCache")]
	public RequestCacheLevel DefaultPolicyLevel
	{
		get
		{
			return (RequestCacheLevel)base[defaultPolicyLevelProp];
		}
		set
		{
			base[defaultPolicyLevelProp] = value;
		}
	}

	/// <summary>Gets or sets a Boolean value that enables caching on the local computer.</summary>
	/// <returns>true if caching is disabled on the local computer; otherwise, false.</returns>
	[ConfigurationProperty("disableAllCaching", DefaultValue = "False")]
	public bool DisableAllCaching
	{
		get
		{
			return (bool)base[disableAllCachingProp];
		}
		set
		{
			base[disableAllCachingProp] = value;
		}
	}

	/// <summary>Gets or sets a Boolean value that indicates whether the local computer cache is private.</summary>
	/// <returns>true if the cache provides user isolation; otherwise, false.</returns>
	[ConfigurationProperty("isPrivateCache", DefaultValue = "True")]
	public bool IsPrivateCache
	{
		get
		{
			return (bool)base[isPrivateCacheProp];
		}
		set
		{
			base[isPrivateCacheProp] = value;
		}
	}

	/// <summary>Gets or sets a value used as the maximum age for cached resources that do not have expiration information.</summary>
	/// <returns>A <see cref="T:System.TimeSpan" /> that provides a default maximum age for cached resources.</returns>
	[ConfigurationProperty("unspecifiedMaximumAge", DefaultValue = "1.00:00:00")]
	public TimeSpan UnspecifiedMaximumAge
	{
		get
		{
			return (TimeSpan)base[unspecifiedMaximumAgeProp];
		}
		set
		{
			base[unspecifiedMaximumAgeProp] = value;
		}
	}

	protected override ConfigurationPropertyCollection Properties => properties;

	static RequestCachingSection()
	{
		defaultFtpCachePolicyProp = new ConfigurationProperty("defaultFtpCachePolicy", typeof(FtpCachePolicyElement));
		defaultHttpCachePolicyProp = new ConfigurationProperty("defaultHttpCachePolicy", typeof(HttpCachePolicyElement));
		defaultPolicyLevelProp = new ConfigurationProperty("defaultPolicyLevel", typeof(RequestCacheLevel), RequestCacheLevel.BypassCache);
		disableAllCachingProp = new ConfigurationProperty("disableAllCaching", typeof(bool), false);
		isPrivateCacheProp = new ConfigurationProperty("isPrivateCache", typeof(bool), true);
		unspecifiedMaximumAgeProp = new ConfigurationProperty("unspecifiedMaximumAge", typeof(TimeSpan), new TimeSpan(1, 0, 0, 0));
		properties = new ConfigurationPropertyCollection();
		properties.Add(defaultFtpCachePolicyProp);
		properties.Add(defaultHttpCachePolicyProp);
		properties.Add(defaultPolicyLevelProp);
		properties.Add(disableAllCachingProp);
		properties.Add(isPrivateCacheProp);
		properties.Add(unspecifiedMaximumAgeProp);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Configuration.RequestCachingSection" /> class. </summary>
	public RequestCachingSection()
	{
	}

	[System.MonoTODO]
	protected override void PostDeserialize()
	{
		base.PostDeserialize();
	}

	[System.MonoTODO]
	protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
	{
		base.DeserializeElement(reader, serializeCollectionKey);
	}
}
