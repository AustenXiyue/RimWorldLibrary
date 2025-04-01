using System.Security.Permissions;

namespace System.Security.Policy;

/// <summary>Provides a base class from which all objects to be used as evidence must derive. </summary>
[Serializable]
[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
public abstract class EvidenceBase
{
	/// <summary>Creates a new object that is a complete copy of the current instance.</summary>
	/// <returns>A duplicate copy of this evidence object.</returns>
	[SecurityPermission(SecurityAction.Assert, SerializationFormatter = true)]
	public virtual EvidenceBase Clone()
	{
		throw new NotImplementedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Policy.EvidenceBase" /> class. </summary>
	/// <exception cref="T:System.InvalidOperationException">An object to be used as evidence is not serializable.</exception>
	protected EvidenceBase()
	{
	}
}
