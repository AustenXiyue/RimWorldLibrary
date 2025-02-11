using System.Configuration;
using System.Net.Security;
using Unity;

namespace System.Net.Configuration;

/// <summary>Represents the default settings used to create connections to a remote computer. This class cannot be inherited.</summary>
public sealed class ServicePointManagerElement : ConfigurationElement
{
	private static ConfigurationPropertyCollection properties;

	private static ConfigurationProperty checkCertificateNameProp;

	private static ConfigurationProperty checkCertificateRevocationListProp;

	private static ConfigurationProperty dnsRefreshTimeoutProp;

	private static ConfigurationProperty enableDnsRoundRobinProp;

	private static ConfigurationProperty expect100ContinueProp;

	private static ConfigurationProperty useNagleAlgorithmProp;

	/// <summary>Gets or sets a Boolean value that controls checking host name information in an X509 certificate.</summary>
	/// <returns>true to specify host name checking; otherwise, false. </returns>
	[ConfigurationProperty("checkCertificateName", DefaultValue = "True")]
	public bool CheckCertificateName
	{
		get
		{
			return (bool)base[checkCertificateNameProp];
		}
		set
		{
			base[checkCertificateNameProp] = value;
		}
	}

	/// <summary>Gets or sets a Boolean value that indicates whether the certificate is checked against the certificate authority revocation list.</summary>
	/// <returns>true if the certificate revocation list is checked; otherwise, false.The default value is false.</returns>
	[ConfigurationProperty("checkCertificateRevocationList", DefaultValue = "False")]
	public bool CheckCertificateRevocationList
	{
		get
		{
			return (bool)base[checkCertificateRevocationListProp];
		}
		set
		{
			base[checkCertificateRevocationListProp] = value;
		}
	}

	/// <summary>Gets or sets the amount of time after which address information is refreshed.</summary>
	/// <returns>A <see cref="T:System.TimeSpan" /> that specifies when addresses are resolved using DNS.</returns>
	[ConfigurationProperty("dnsRefreshTimeout", DefaultValue = "120000")]
	public int DnsRefreshTimeout
	{
		get
		{
			return (int)base[dnsRefreshTimeoutProp];
		}
		set
		{
			base[dnsRefreshTimeoutProp] = value;
		}
	}

	/// <summary>Gets or sets a Boolean value that controls using different IP addresses on connections to the same server.</summary>
	/// <returns>true to enable DNS round-robin behavior; otherwise, false.</returns>
	[ConfigurationProperty("enableDnsRoundRobin", DefaultValue = "False")]
	public bool EnableDnsRoundRobin
	{
		get
		{
			return (bool)base[enableDnsRoundRobinProp];
		}
		set
		{
			base[enableDnsRoundRobinProp] = value;
		}
	}

	/// <summary>Gets or sets a Boolean value that determines whether 100-Continue behavior is used.</summary>
	/// <returns>true to expect 100-Continue responses for POST requests; otherwise, false. The default value is true.</returns>
	[ConfigurationProperty("expect100Continue", DefaultValue = "True")]
	public bool Expect100Continue
	{
		get
		{
			return (bool)base[expect100ContinueProp];
		}
		set
		{
			base[expect100ContinueProp] = value;
		}
	}

	/// <summary>Gets or sets a Boolean value that determines whether the Nagle algorithm is used.</summary>
	/// <returns>true to use the Nagle algorithm; otherwise, false. The default value is true.</returns>
	[ConfigurationProperty("useNagleAlgorithm", DefaultValue = "True")]
	public bool UseNagleAlgorithm
	{
		get
		{
			return (bool)base[useNagleAlgorithmProp];
		}
		set
		{
			base[useNagleAlgorithmProp] = value;
		}
	}

	protected override ConfigurationPropertyCollection Properties => properties;

	/// <summary>Gets or sets the <see cref="T:System.Net.Security.EncryptionPolicy" /> to use.</summary>
	/// <returns>The encryption policy to use for a <see cref="T:System.Net.ServicePointManager" /> instance.</returns>
	public EncryptionPolicy EncryptionPolicy
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(EncryptionPolicy);
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	static ServicePointManagerElement()
	{
		checkCertificateNameProp = new ConfigurationProperty("checkCertificateName", typeof(bool), true);
		checkCertificateRevocationListProp = new ConfigurationProperty("checkCertificateRevocationList", typeof(bool), false);
		dnsRefreshTimeoutProp = new ConfigurationProperty("dnsRefreshTimeout", typeof(int), 120000);
		enableDnsRoundRobinProp = new ConfigurationProperty("enableDnsRoundRobin", typeof(bool), false);
		expect100ContinueProp = new ConfigurationProperty("expect100Continue", typeof(bool), true);
		useNagleAlgorithmProp = new ConfigurationProperty("useNagleAlgorithm", typeof(bool), true);
		properties = new ConfigurationPropertyCollection();
		properties.Add(checkCertificateNameProp);
		properties.Add(checkCertificateRevocationListProp);
		properties.Add(dnsRefreshTimeoutProp);
		properties.Add(enableDnsRoundRobinProp);
		properties.Add(expect100ContinueProp);
		properties.Add(useNagleAlgorithmProp);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Configuration.ServicePointManagerElement" /> class. </summary>
	public ServicePointManagerElement()
	{
	}

	[System.MonoTODO]
	protected override void PostDeserialize()
	{
	}
}
