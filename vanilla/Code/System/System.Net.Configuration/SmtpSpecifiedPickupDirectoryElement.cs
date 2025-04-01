using System.Configuration;

namespace System.Net.Configuration;

/// <summary>Represents an SMTP pickup directory configuration element.</summary>
public sealed class SmtpSpecifiedPickupDirectoryElement : ConfigurationElement
{
	private static ConfigurationProperty pickupDirectoryLocationProp;

	private static ConfigurationPropertyCollection properties;

	/// <summary>Gets or sets the folder where applications save mail messages to be processed by the SMTP server.</summary>
	/// <returns>A string that specifies the pickup directory for e-mail messages.</returns>
	[ConfigurationProperty("pickupDirectoryLocation")]
	public string PickupDirectoryLocation
	{
		get
		{
			return (string)base[pickupDirectoryLocationProp];
		}
		set
		{
			base[pickupDirectoryLocationProp] = value;
		}
	}

	protected override ConfigurationPropertyCollection Properties => properties;

	static SmtpSpecifiedPickupDirectoryElement()
	{
		pickupDirectoryLocationProp = new ConfigurationProperty("pickupDirectoryLocation", typeof(string));
		properties = new ConfigurationPropertyCollection();
		properties.Add(pickupDirectoryLocationProp);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Configuration.SmtpSpecifiedPickupDirectoryElement" /> class.</summary>
	public SmtpSpecifiedPickupDirectoryElement()
	{
	}
}
