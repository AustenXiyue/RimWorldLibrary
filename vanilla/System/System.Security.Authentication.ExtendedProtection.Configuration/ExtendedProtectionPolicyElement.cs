using System.Configuration;

namespace System.Security.Authentication.ExtendedProtection.Configuration;

/// <summary>The <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ExtendedProtectionPolicyElement" /> class represents a configuration element for an <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy" />.</summary>
[System.MonoTODO]
public sealed class ExtendedProtectionPolicyElement : ConfigurationElement
{
	private static ConfigurationPropertyCollection properties;

	private static ConfigurationProperty custom_service_names;

	private static ConfigurationProperty policy_enforcement;

	private static ConfigurationProperty protection_scenario;

	/// <summary>Gets or sets the custom Service Provider Name (SPN) list used to match against a client's SPN for this configuration policy element. </summary>
	/// <returns>Returns a <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElementCollection" /> that includes the custom SPN list used to match against a client's SPN.</returns>
	[ConfigurationProperty("customServiceNames")]
	public ServiceNameElementCollection CustomServiceNames => (ServiceNameElementCollection)base[custom_service_names];

	/// <summary>Gets or sets the policy enforcement value for this configuration policy element.</summary>
	/// <returns>Returns a <see cref="T:System.Security.Authentication.ExtendedProtection.PolicyEnforcement" /> value that indicates when the extended protection policy should be enforced.</returns>
	[ConfigurationProperty("policyEnforcement")]
	public PolicyEnforcement PolicyEnforcement
	{
		get
		{
			return (PolicyEnforcement)base[policy_enforcement];
		}
		set
		{
			base[policy_enforcement] = value;
		}
	}

	/// <summary>Gets or sets the kind of protection enforced by the extended protection policy for this configuration policy element.</summary>
	/// <returns>A <see cref="T:System.Security.Authentication.ExtendedProtection.ProtectionScenario" /> value that indicates the kind of protection enforced by the policy.</returns>
	[ConfigurationProperty("protectionScenario", DefaultValue = ProtectionScenario.TransportSelected)]
	public ProtectionScenario ProtectionScenario
	{
		get
		{
			return (ProtectionScenario)base[protection_scenario];
		}
		set
		{
			base[protection_scenario] = value;
		}
	}

	protected override ConfigurationPropertyCollection Properties => properties;

	static ExtendedProtectionPolicyElement()
	{
		properties = new ConfigurationPropertyCollection();
		Type typeFromHandle = typeof(ExtendedProtectionPolicyElement);
		custom_service_names = ConfigUtil.BuildProperty(typeFromHandle, "CustomServiceNames");
		policy_enforcement = ConfigUtil.BuildProperty(typeFromHandle, "PolicyEnforcement");
		protection_scenario = ConfigUtil.BuildProperty(typeFromHandle, "ProtectionScenario");
		ConfigurationProperty[] array = new ConfigurationProperty[3] { custom_service_names, policy_enforcement, protection_scenario };
		foreach (ConfigurationProperty property in array)
		{
			properties.Add(property);
		}
	}

	/// <summary>The <see cref="M:System.Security.Authentication.ExtendedProtection.Configuration.ExtendedProtectionPolicyElement.BuildPolicy" /> method builds a new <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy" /> instance based on the properties set on the <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ExtendedProtectionPolicyElement" /> class. </summary>
	/// <returns>A new <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy" /> instance that represents the extended protection policy created.</returns>
	public ExtendedProtectionPolicy BuildPolicy()
	{
		throw new NotImplementedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ExtendedProtectionPolicyElement" /> class.</summary>
	public ExtendedProtectionPolicyElement()
	{
	}
}
