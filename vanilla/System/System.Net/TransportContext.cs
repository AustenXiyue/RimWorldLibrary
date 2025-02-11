using System.Collections.Generic;
using System.Security.Authentication.ExtendedProtection;

namespace System.Net;

/// <summary>The <see cref="T:System.Net.TransportContext" /> class provides additional context about the underlying transport layer.</summary>
public abstract class TransportContext
{
	/// <summary>Retrieves the requested channel binding. </summary>
	/// <returns>The requested <see cref="T:System.Security.Authentication.ExtendedProtection.ChannelBinding" />, or null if the channel binding is not supported by the current transport or by the operating system.</returns>
	/// <param name="kind">The type of channel binding to retrieve.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="kind" /> is must be <see cref="F:System.Security.Authentication.ExtendedProtection.ChannelBindingKind.Endpoint" /> for use with the <see cref="T:System.Net.TransportContext" /> retrieved from the <see cref="P:System.Net.HttpListenerRequest.TransportContext" /> property.</exception>
	public abstract ChannelBinding GetChannelBinding(ChannelBindingKind kind);

	public virtual IEnumerable<TokenBinding> GetTlsTokenBindings()
	{
		throw new NotSupportedException();
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Net.TransportContext" /> class</summary>
	protected TransportContext()
	{
	}
}
