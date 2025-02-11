using System.Configuration;
using System.Net.Mail;

namespace System.Net.Configuration;

/// <summary>Represents the SMTP section in the System.Net configuration file.</summary>
public sealed class SmtpSection : ConfigurationSection
{
	/// <summary>Gets or sets the Simple Mail Transport Protocol (SMTP) delivery method. The default delivery method is <see cref="F:System.Net.Mail.SmtpDeliveryMethod.Network" />.</summary>
	/// <returns>A string that represents the SMTP delivery method.</returns>
	[ConfigurationProperty("deliveryMethod", DefaultValue = "Network")]
	public SmtpDeliveryMethod DeliveryMethod
	{
		get
		{
			return (SmtpDeliveryMethod)base["deliveryMethod"];
		}
		set
		{
			base["deliveryMethod"] = value;
		}
	}

	/// <summary>Gets or sets the delivery format to use for sending outgoing e-mail using the Simple Mail Transport Protocol (SMTP).</summary>
	/// <returns>Returns <see cref="T:System.Net.Mail.SmtpDeliveryFormat" />.The delivery format to use for sending outgoing e-mail using SMTP.</returns>
	[ConfigurationProperty("deliveryFormat", DefaultValue = SmtpDeliveryFormat.SevenBit)]
	public SmtpDeliveryFormat DeliveryFormat
	{
		get
		{
			return (SmtpDeliveryFormat)base["deliveryFormat"];
		}
		set
		{
			base["deliveryFormat"] = value;
		}
	}

	/// <summary>Gets or sets the default value that indicates who the email message is from.</summary>
	/// <returns>A string that represents the default value indicating who a mail message is from.</returns>
	[ConfigurationProperty("from")]
	public string From
	{
		get
		{
			return (string)base["from"];
		}
		set
		{
			base["from"] = value;
		}
	}

	/// <summary>Gets the configuration element that controls the network settings used by the Simple Mail Transport Protocol (SMTP). file.<see cref="T:System.Net.Configuration.SmtpNetworkElement" />.</summary>
	/// <returns>A <see cref="T:System.Net.Configuration.SmtpNetworkElement" /> object.The configuration element that controls the network settings used by SMTP.</returns>
	[ConfigurationProperty("network")]
	public SmtpNetworkElement Network => (SmtpNetworkElement)base["network"];

	/// <summary>Gets the pickup directory that will be used by the SMPT client.</summary>
	/// <returns>A <see cref="T:System.Net.Configuration.SmtpSpecifiedPickupDirectoryElement" /> object that specifies the pickup directory folder.</returns>
	[ConfigurationProperty("specifiedPickupDirectory")]
	public SmtpSpecifiedPickupDirectoryElement SpecifiedPickupDirectory => (SmtpSpecifiedPickupDirectoryElement)base["specifiedPickupDirectory"];

	protected override ConfigurationPropertyCollection Properties => base.Properties;

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Configuration.SmtpSection" /> class.</summary>
	public SmtpSection()
	{
	}
}
