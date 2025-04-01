namespace System.Security;

/// <summary>Identifies the source for the security context.</summary>
public enum SecurityContextSource
{
	/// <summary>The current application domain is the source for the security context.</summary>
	CurrentAppDomain,
	/// <summary>The current assembly is the source for the security context.</summary>
	CurrentAssembly
}
