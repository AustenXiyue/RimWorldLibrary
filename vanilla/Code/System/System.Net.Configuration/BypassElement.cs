using System.Configuration;

namespace System.Net.Configuration;

/// <summary>Represents the address information for resources that are not retrieved using a proxy server. This class cannot be inherited.</summary>
public sealed class BypassElement : ConfigurationElement
{
	private static ConfigurationPropertyCollection properties;

	private static ConfigurationProperty addressProp;

	/// <summary>Gets or sets the addresses of resources that bypass the proxy server.</summary>
	/// <returns>A string that identifies a resource.</returns>
	[ConfigurationProperty("address", Options = (ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey))]
	public string Address
	{
		get
		{
			return (string)base[addressProp];
		}
		set
		{
			base[addressProp] = value;
		}
	}

	protected override ConfigurationPropertyCollection Properties => properties;

	static BypassElement()
	{
		addressProp = new ConfigurationProperty("address", typeof(string), null, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
		properties = new ConfigurationPropertyCollection();
		properties.Add(addressProp);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Configuration.BypassElement" /> class. </summary>
	public BypassElement()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Configuration.BypassElement" /> class with the specified type information.</summary>
	/// <param name="address">A string that identifies the address of a resource.</param>
	public BypassElement(string address)
	{
		Address = address;
	}
}
