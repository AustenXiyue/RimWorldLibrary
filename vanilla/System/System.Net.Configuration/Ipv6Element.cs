using System.Configuration;

namespace System.Net.Configuration;

/// <summary>Determines whether Internet Protocol version 6 is enabled on the local computer. This class cannot be inherited.</summary>
public sealed class Ipv6Element : ConfigurationElement
{
	private static ConfigurationPropertyCollection properties;

	private static ConfigurationProperty enabledProp;

	/// <summary>Gets or sets a Boolean value that indicates whether Internet Protocol version 6 is enabled on the local computer.</summary>
	/// <returns>true if IPv6 is enabled; otherwise, false.</returns>
	[ConfigurationProperty("enabled", DefaultValue = "False")]
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

	protected override ConfigurationPropertyCollection Properties => properties;

	static Ipv6Element()
	{
		enabledProp = new ConfigurationProperty("enabled", typeof(bool), false);
		properties = new ConfigurationPropertyCollection();
		properties.Add(enabledProp);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Configuration.Ipv6Element" /> class. </summary>
	public Ipv6Element()
	{
	}
}
