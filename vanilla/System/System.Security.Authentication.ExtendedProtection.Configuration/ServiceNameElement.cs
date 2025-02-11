using System.Configuration;

namespace System.Security.Authentication.ExtendedProtection.Configuration;

/// <summary>The <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement" /> class represents a configuration element for a service name used in a <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElementCollection" />.</summary>
public sealed class ServiceNameElement : ConfigurationElement
{
	private static ConfigurationPropertyCollection properties;

	private static ConfigurationProperty name;

	/// <summary>Gets or sets the Service Provider Name (SPN) for this <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement" /> instance.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the representation of SPN for this <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement" /> instance.</returns>
	[ConfigurationProperty("name")]
	public string Name
	{
		get
		{
			return (string)base[name];
		}
		set
		{
			base[name] = value;
		}
	}

	protected override ConfigurationPropertyCollection Properties => properties;

	static ServiceNameElement()
	{
		properties = new ConfigurationPropertyCollection();
		name = ConfigUtil.BuildProperty(typeof(ServiceNameElement), "Name");
		properties.Add(name);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement" /> class.</summary>
	public ServiceNameElement()
	{
	}
}
