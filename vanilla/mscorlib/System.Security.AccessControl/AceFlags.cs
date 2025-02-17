namespace System.Security.AccessControl;

/// <summary>Specifies the inheritance and auditing behavior of an access control entry (ACE).</summary>
[Flags]
public enum AceFlags : byte
{
	/// <summary>No ACE flags are set.</summary>
	None = 0,
	/// <summary>The access mask is propagated onto child leaf objects.</summary>
	ObjectInherit = 1,
	/// <summary>The access mask is propagated to child container objects.</summary>
	ContainerInherit = 2,
	/// <summary>The access checks do not apply to the object; they only apply to its children.</summary>
	NoPropagateInherit = 4,
	/// <summary>The access mask is propagated only to child objects. This includes both container and leaf child objects.</summary>
	InheritOnly = 8,
	/// <summary>A logical OR of <see cref="F:System.Security.AccessControl.AceFlags.ObjectInherit" />, <see cref="F:System.Security.AccessControl.AceFlags.ContainerInherit" />, <see cref="F:System.Security.AccessControl.AceFlags.NoPropagateInherit" />, and <see cref="F:System.Security.AccessControl.AceFlags.InheritOnly" />.</summary>
	InheritanceFlags = 0xF,
	/// <summary>An ACE is inherited from a parent container rather than being explicitly set for an object.</summary>
	Inherited = 0x10,
	/// <summary>Successful access attempts are audited.</summary>
	SuccessfulAccess = 0x40,
	/// <summary>Failed access attempts are audited.</summary>
	FailedAccess = 0x80,
	/// <summary>All access attempts are audited.</summary>
	AuditFlags = 0xC0
}
