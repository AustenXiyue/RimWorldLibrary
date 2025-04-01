using System.Configuration;
using System.Net.Sockets;
using Unity;

namespace System.Net.Configuration;

/// <summary>Represents information used to configure <see cref="T:System.Net.Sockets.Socket" /> objects. This class cannot be inherited.</summary>
public sealed class SocketElement : ConfigurationElement
{
	private static ConfigurationPropertyCollection properties;

	private static ConfigurationProperty alwaysUseCompletionPortsForAcceptProp;

	private static ConfigurationProperty alwaysUseCompletionPortsForConnectProp;

	/// <summary>Gets or sets a Boolean value that specifies whether completion ports are used when accepting connections.</summary>
	/// <returns>true to use completion ports; otherwise, false.</returns>
	[ConfigurationProperty("alwaysUseCompletionPortsForAccept", DefaultValue = "False")]
	public bool AlwaysUseCompletionPortsForAccept
	{
		get
		{
			return (bool)base[alwaysUseCompletionPortsForAcceptProp];
		}
		set
		{
			base[alwaysUseCompletionPortsForAcceptProp] = value;
		}
	}

	/// <summary>Gets or sets a Boolean value that specifies whether completion ports are used when making connections.</summary>
	/// <returns>true to use completion ports; otherwise, false.</returns>
	[ConfigurationProperty("alwaysUseCompletionPortsForConnect", DefaultValue = "False")]
	public bool AlwaysUseCompletionPortsForConnect
	{
		get
		{
			return (bool)base[alwaysUseCompletionPortsForConnectProp];
		}
		set
		{
			base[alwaysUseCompletionPortsForConnectProp] = value;
		}
	}

	protected override ConfigurationPropertyCollection Properties => properties;

	/// <summary>Gets or sets a value that specifies the default <see cref="T:System.Net.Sockets.IPProtectionLevel" /> to use for a socket.</summary>
	/// <returns>The value of the <see cref="T:System.Net.Sockets.IPProtectionLevel" /> for the current instance.</returns>
	public IPProtectionLevel IPProtectionLevel
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(IPProtectionLevel);
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Configuration.SocketElement" /> class. </summary>
	public SocketElement()
	{
		alwaysUseCompletionPortsForAcceptProp = new ConfigurationProperty("alwaysUseCompletionPortsForAccept", typeof(bool), false);
		alwaysUseCompletionPortsForConnectProp = new ConfigurationProperty("alwaysUseCompletionPortsForConnect", typeof(bool), false);
		properties = new ConfigurationPropertyCollection();
		properties.Add(alwaysUseCompletionPortsForAcceptProp);
		properties.Add(alwaysUseCompletionPortsForConnectProp);
	}

	[System.MonoTODO]
	protected override void PostDeserialize()
	{
	}
}
