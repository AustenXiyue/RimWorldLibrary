using System.Configuration;
using System.Security;
using System.Security.Permissions;

namespace System.Runtime.Serialization.Configuration;

/// <summary>Handles the XML elements used to configure serialization by the <see cref="T:System.Runtime.Serialization.NetDataContractSerializer" />.</summary>
public sealed class NetDataContractSerializerSection : ConfigurationSection
{
	private ConfigurationPropertyCollection properties;

	/// <summary>Gets a value that indicates whether unsafe type forwarding is enabled.</summary>
	/// <returns>true if unsafe type forwarding is enabled; otherwise, false.</returns>
	[ConfigurationProperty("enableUnsafeTypeForwarding", DefaultValue = false)]
	public bool EnableUnsafeTypeForwarding => (bool)base["enableUnsafeTypeForwarding"];

	protected override ConfigurationPropertyCollection Properties
	{
		get
		{
			if (properties == null)
			{
				ConfigurationPropertyCollection configurationPropertyCollection = new ConfigurationPropertyCollection();
				configurationPropertyCollection.Add(new ConfigurationProperty("enableUnsafeTypeForwarding", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
				properties = configurationPropertyCollection;
			}
			return properties;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.Configuration.NetDataContractSerializerSection" /> class.</summary>
	public NetDataContractSerializerSection()
	{
	}

	[SecurityCritical]
	[ConfigurationPermission(SecurityAction.Assert, Unrestricted = true)]
	internal static bool TryUnsafeGetSection(out NetDataContractSerializerSection section)
	{
		section = (NetDataContractSerializerSection)ConfigurationManager.GetSection(ConfigurationStrings.NetDataContractSerializerSectionPath);
		return section != null;
	}
}
