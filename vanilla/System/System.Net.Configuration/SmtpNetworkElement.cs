using System.Configuration;
using Unity;

namespace System.Net.Configuration;

/// <summary>Represents the network element in the SMTP configuration file. This class cannot be inherited.</summary>
public sealed class SmtpNetworkElement : ConfigurationElement
{
	/// <summary>Determines whether or not default user credentials are used to access an SMTP server. The default value is false.</summary>
	/// <returns>true indicates that default user credentials will be used to access the SMTP server; otherwise, false.</returns>
	[ConfigurationProperty("defaultCredentials", DefaultValue = "False")]
	public bool DefaultCredentials
	{
		get
		{
			return (bool)base["defaultCredentials"];
		}
		set
		{
			base["defaultCredentials"] = value;
		}
	}

	/// <summary>Gets or sets the name of the SMTP server.</summary>
	/// <returns>A string that represents the name of the SMTP server to connect to.</returns>
	[ConfigurationProperty("host")]
	public string Host
	{
		get
		{
			return (string)base["host"];
		}
		set
		{
			base["host"] = value;
		}
	}

	/// <summary>Gets or sets the user password to use to connect to an SMTP mail server.</summary>
	/// <returns>A string that represents the password to use to connect to an SMTP mail server.</returns>
	[ConfigurationProperty("password")]
	public string Password
	{
		get
		{
			return (string)base["password"];
		}
		set
		{
			base["password"] = value;
		}
	}

	/// <summary>Gets or sets the port that SMTP clients use to connect to an SMTP mail server. The default value is 25.</summary>
	/// <returns>A string that represents the port to connect to an SMTP mail server.</returns>
	[ConfigurationProperty("port", DefaultValue = "25")]
	public int Port
	{
		get
		{
			return (int)base["port"];
		}
		set
		{
			base["port"] = value;
		}
	}

	/// <summary>Gets or sets the user name to connect to an SMTP mail server.</summary>
	/// <returns>A string that represents the user name to connect to an SMTP mail server.</returns>
	[ConfigurationProperty("userName", DefaultValue = null)]
	public string UserName
	{
		get
		{
			return (string)base["userName"];
		}
		set
		{
			base["userName"] = value;
		}
	}

	/// <summary>Gets or sets the Service Provider Name (SPN) to use for authentication when using extended protection to connect to an SMTP mail server.</summary>
	/// <returns>A string that represents the SPN to use for authentication when using extended protection to connect to an SMTP mail server.</returns>
	[ConfigurationProperty("targetName", DefaultValue = null)]
	public string TargetName
	{
		get
		{
			return (string)base["targetName"];
		}
		set
		{
			base["targetName"] = value;
		}
	}

	/// <summary>Gets or sets whether SSL is used to access an SMTP mail server. The default value is false.</summary>
	/// <returns>true indicates that SSL will be used to access the SMTP mail server; otherwise, false.</returns>
	[ConfigurationProperty("enableSsl", DefaultValue = false)]
	public bool EnableSsl
	{
		get
		{
			return (bool)base["enableSsl"];
		}
		set
		{
			base["enableSsl"] = value;
		}
	}

	protected override ConfigurationPropertyCollection Properties => base.Properties;

	/// <summary>Gets or sets the client domain name used in the initial SMTP protocol request to connect to an SMTP mail server.</summary>
	/// <returns>A string that represents the client domain name used in the initial SMTP protocol request to connect to an SMTP mail server.</returns>
	public string ClientDomain
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	protected override void PostDeserialize()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Configuration.SmtpNetworkElement" /> class.</summary>
	public SmtpNetworkElement()
	{
	}
}
