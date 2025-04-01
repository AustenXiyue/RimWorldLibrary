namespace System.Diagnostics.Eventing.Reader;

/// <summary>Defines values for the type of authentication used during a Remote Procedure Call (RPC) login to a server. This login occurs when you create a <see cref="T:System.Diagnostics.Eventing.Reader.EventLogSession" /> object that specifies a connection to a remote computer.</summary>
/// <filterpriority>2</filterpriority>
public enum SessionAuthentication
{
	/// <summary>Use the default authentication method during RPC login. The default authentication is equivalent to Negotiate.</summary>
	Default = 0,
	/// <summary>Use Kerberos authentication during RPC login. </summary>
	Kerberos = 2,
	/// <summary>Use the Negotiate authentication method during RPC login. This allows the client application to select the most appropriate authentication method (NTLM or Kerberos) for the situation. </summary>
	Negotiate = 1,
	/// <summary>Use Windows NT LAN Manager (NTLM) authentication during RPC login.</summary>
	Ntlm = 3
}
