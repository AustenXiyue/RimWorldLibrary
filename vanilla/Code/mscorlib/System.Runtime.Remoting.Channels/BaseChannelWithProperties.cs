using System.Collections;
using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Channels;

/// <summary>Provides a base implementation for channels that want to expose a dictionary interface to their properties.</summary>
[ComVisible(true)]
public abstract class BaseChannelWithProperties : BaseChannelObjectWithProperties
{
	/// <summary>Indicates the top channel sink in the channel sink stack.</summary>
	protected IChannelSinkBase SinksWithProperties;

	/// <summary>Gets a <see cref="T:System.Collections.IDictionary" /> of the channel properties associated with the current channel object.</summary>
	/// <returns>A <see cref="T:System.Collections.IDictionary" /> of the channel properties associated with the current channel object.</returns>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	public override IDictionary Properties
	{
		get
		{
			if (SinksWithProperties == null || SinksWithProperties.Properties == null)
			{
				return base.Properties;
			}
			return new AggregateDictionary(new IDictionary[2] { base.Properties, SinksWithProperties.Properties });
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Channels.BaseChannelWithProperties" /> class.</summary>
	protected BaseChannelWithProperties()
	{
	}
}
