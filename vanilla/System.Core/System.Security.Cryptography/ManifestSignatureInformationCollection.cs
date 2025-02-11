using System.Collections.ObjectModel;
using System.Security.Permissions;
using Unity;

namespace System.Security.Cryptography;

/// <summary>Represents a read-only collection of <see cref="T:System.Security.Cryptography.ManifestSignatureInformation" /> objects.  </summary>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class ManifestSignatureInformationCollection : ReadOnlyCollection<ManifestSignatureInformation>
{
	internal ManifestSignatureInformationCollection()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
