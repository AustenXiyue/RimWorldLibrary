using System.Configuration;

namespace System.Net.Configuration;

/// <summary>Represents information used to configure Web proxy scripts. This class cannot be inherited.</summary>
public sealed class WebProxyScriptElement : ConfigurationElement
{
	private static ConfigurationProperty downloadTimeoutProp;

	private static ConfigurationPropertyCollection properties;

	/// <summary>Gets or sets the Web proxy script download timeout using the format hours:minutes:seconds.</summary>
	/// <returns>A <see cref="T:System.TimeSpan" /> object that contains the timeout value. The default download timeout is one minute.</returns>
	[ConfigurationProperty("downloadTimeout", DefaultValue = "00:02:00")]
	public TimeSpan DownloadTimeout
	{
		get
		{
			return (TimeSpan)base[downloadTimeoutProp];
		}
		set
		{
			base[downloadTimeoutProp] = value;
		}
	}

	protected override ConfigurationPropertyCollection Properties => properties;

	static WebProxyScriptElement()
	{
		downloadTimeoutProp = new ConfigurationProperty("downloadTimeout", typeof(TimeSpan), new TimeSpan(0, 0, 2, 0));
		properties = new ConfigurationPropertyCollection();
		properties.Add(downloadTimeoutProp);
	}

	protected override void PostDeserialize()
	{
	}

	/// <summary>Initializes an instance of the <see cref="T:System.Net.Configuration.WebProxyScriptElement" /> class.</summary>
	public WebProxyScriptElement()
	{
	}
}
