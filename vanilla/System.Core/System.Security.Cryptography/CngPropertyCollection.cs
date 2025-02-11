using System.Collections.ObjectModel;
using System.Security.Permissions;

namespace System.Security.Cryptography;

/// <summary>Provides a strongly typed collection of Cryptography Next Generation (CNG) properties.</summary>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class CngPropertyCollection : Collection<CngProperty>
{
	/// <summary>Initializes a new <see cref="T:System.Security.Cryptography.CngPropertyCollection" /> object.</summary>
	public CngPropertyCollection()
	{
	}
}
