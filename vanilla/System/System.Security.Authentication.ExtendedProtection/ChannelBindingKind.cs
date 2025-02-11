namespace System.Security.Authentication.ExtendedProtection;

/// <summary>The <see cref="T:System.Security.Authentication.ExtendedProtection.ChannelBindingKind" /> enumeration represents the kinds of channel bindings that can be queried from secure channels.</summary>
public enum ChannelBindingKind
{
	/// <summary>An unknown channel binding type.</summary>
	Unknown = 0,
	/// <summary>A channel binding completely unique to a given channel (a TLS session key, for example).</summary>
	Unique = 25,
	/// <summary>A channel binding unique to a given endpoint (a TLS server certificate, for example).</summary>
	Endpoint = 26
}
