using System.Configuration;

namespace System.Net.Configuration;

/// <summary>Represents the maximum length for response headers. This class cannot be inherited.</summary>
public sealed class HttpWebRequestElement : ConfigurationElement
{
	private static ConfigurationProperty maximumErrorResponseLengthProp;

	private static ConfigurationProperty maximumResponseHeadersLengthProp;

	private static ConfigurationProperty maximumUnauthorizedUploadLengthProp;

	private static ConfigurationProperty useUnsafeHeaderParsingProp;

	private static ConfigurationPropertyCollection properties;

	/// <summary>Gets or sets the maximum allowed length of an error response.</summary>
	/// <returns>A 32-bit signed integer containing the maximum length in kilobytes (1024 bytes) of the error response. The default value is 64.</returns>
	[ConfigurationProperty("maximumErrorResponseLength", DefaultValue = "64")]
	public int MaximumErrorResponseLength
	{
		get
		{
			return (int)base[maximumErrorResponseLengthProp];
		}
		set
		{
			base[maximumErrorResponseLengthProp] = value;
		}
	}

	/// <summary>Gets or sets the maximum allowed length of the response headers.</summary>
	/// <returns>A 32-bit signed integer containing the maximum length in kilobytes (1024 bytes) of the response headers. The default value is 64.</returns>
	[ConfigurationProperty("maximumResponseHeadersLength", DefaultValue = "64")]
	public int MaximumResponseHeadersLength
	{
		get
		{
			return (int)base[maximumResponseHeadersLengthProp];
		}
		set
		{
			base[maximumResponseHeadersLengthProp] = value;
		}
	}

	/// <summary>Gets or sets the maximum length of an upload in response to an unauthorized error code.</summary>
	/// <returns>A 32-bit signed integer containing the maximum length (in multiple of 1,024 byte units) of an upload in response to an unauthorized error code. A value of -1 indicates that no size limit will be imposed on the upload. Setting the <see cref="P:System.Net.Configuration.HttpWebRequestElement.MaximumUnauthorizedUploadLength" /> property to any other value will only send the request body if it is smaller than the number of bytes specified. So a value of 1 would indicate to only send the request body if it is smaller than 1,024 bytes. The default value for this property is -1.</returns>
	[ConfigurationProperty("maximumUnauthorizedUploadLength", DefaultValue = "-1")]
	public int MaximumUnauthorizedUploadLength
	{
		get
		{
			return (int)base[maximumUnauthorizedUploadLengthProp];
		}
		set
		{
			base[maximumUnauthorizedUploadLengthProp] = value;
		}
	}

	/// <summary>Setting this property ignores validation errors that occur during HTTP parsing.</summary>
	/// <returns>Boolean that indicates whether this property has been set. </returns>
	[ConfigurationProperty("useUnsafeHeaderParsing", DefaultValue = "False")]
	public bool UseUnsafeHeaderParsing
	{
		get
		{
			return (bool)base[useUnsafeHeaderParsingProp];
		}
		set
		{
			base[useUnsafeHeaderParsingProp] = value;
		}
	}

	protected override ConfigurationPropertyCollection Properties => properties;

	static HttpWebRequestElement()
	{
		maximumErrorResponseLengthProp = new ConfigurationProperty("maximumErrorResponseLength", typeof(int), 64);
		maximumResponseHeadersLengthProp = new ConfigurationProperty("maximumResponseHeadersLength", typeof(int), 64);
		maximumUnauthorizedUploadLengthProp = new ConfigurationProperty("maximumUnauthorizedUploadLength", typeof(int), -1);
		useUnsafeHeaderParsingProp = new ConfigurationProperty("useUnsafeHeaderParsing", typeof(bool), false);
		properties = new ConfigurationPropertyCollection();
		properties.Add(maximumErrorResponseLengthProp);
		properties.Add(maximumResponseHeadersLengthProp);
		properties.Add(maximumUnauthorizedUploadLengthProp);
		properties.Add(useUnsafeHeaderParsingProp);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Configuration.HttpWebRequestElement" /> class. </summary>
	public HttpWebRequestElement()
	{
	}

	[System.MonoTODO]
	protected override void PostDeserialize()
	{
		base.PostDeserialize();
	}
}
