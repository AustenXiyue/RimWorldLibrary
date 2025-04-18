using System.Security.Principal;

namespace System.Security.AccessControl;

/// <summary>Represents a combination of a user's identity and an access mask. An <see cref="T:System.Security.AccessControl.AuditRule" /> object also contains information about how the rule is inherited by child objects, how that inheritance is propagated, and for what conditions it is audited.</summary>
public abstract class AuditRule : AuthorizationRule
{
	private AuditFlags auditFlags;

	/// <summary>Gets the audit flags for this audit rule.</summary>
	/// <returns>A bitwise combination of the enumeration values. This combination specifies the audit conditions for this audit rule.</returns>
	public AuditFlags AuditFlags => auditFlags;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.AuditRule" /> class by using the specified values.</summary>
	/// <param name="identity">The identity to which the audit rule applies. It must be an object that can be cast as a <see cref="T:System.Security.Principal.SecurityIdentifier" />.</param>
	/// <param name="accessMask">The access mask of this rule. The access mask is a 32-bit collection of anonymous bits, the meaning of which is defined by the individual integrators.</param>
	/// <param name="isInherited">true to inherit this rule from a parent container.</param>
	/// <param name="inheritanceFlags">The inheritance properties of the audit rule.</param>
	/// <param name="propagationFlags">Whether inherited audit rules are automatically propagated. The propagation flags are ignored if <paramref name="inheritanceFlags" /> is set to <see cref="F:System.Security.AccessControl.InheritanceFlags.None" />.</param>
	/// <param name="auditFlags">The conditions for which the rule is audited.</param>
	/// <exception cref="T:System.ArgumentException">The value of the <paramref name="identity" /> parameter cannot be cast as a <see cref="T:System.Security.Principal.SecurityIdentifier" />, or the <paramref name="auditFlags" /> parameter contains an invalid value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of the <paramref name="accessMask" /> parameter is zero, or the <paramref name="inheritanceFlags" /> or <paramref name="propagationFlags" /> parameters contain unrecognized flag values.</exception>
	protected AuditRule(IdentityReference identity, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags auditFlags)
		: base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags)
	{
		if (auditFlags != ((AuditFlags.Success | AuditFlags.Failure) & auditFlags))
		{
			throw new ArgumentException("Invalid audit flags.", "auditFlags");
		}
		this.auditFlags = auditFlags;
	}
}
/// <summary>Represents a combination of a user’s identity and an access mask.</summary>
/// <typeparam name="T"></typeparam>
public class AuditRule<T> : AuditRule where T : struct
{
	/// <summary>The rights of the audit rule.</summary>
	/// <returns>Returns <see cref="{0}" />.</returns>
	public T Rights => (T)(object)base.AccessMask;

	/// <summary>Initializes a new instance of the AuditRule’1 class by using the specified values.</summary>
	/// <param name="identity">The identity to which the audit rule applies.</param>
	/// <param name="rights">The rights of the audit rule.</param>
	/// <param name="flags">The properties of the audit rule.</param>
	public AuditRule(string identity, T rights, AuditFlags flags)
		: this((IdentityReference)new NTAccount(identity), rights, flags)
	{
	}

	/// <summary>Initializes a new instance of the AuditRule’1 class by using the specified values.</summary>
	/// <param name="identity">The identity to which this audit rule applies.</param>
	/// <param name="rights">The rights of the audit rule.</param>
	/// <param name="flags">The conditions for which the fule is audited.</param>
	public AuditRule(IdentityReference identity, T rights, AuditFlags flags)
		: this(identity, rights, InheritanceFlags.None, PropagationFlags.None, flags)
	{
	}

	/// <summary>Initializes a new instance of the AuditRule’1 class by using the specified values.</summary>
	/// <param name="identity">The identity to which the audit rule applies.</param>
	/// <param name="rights">The rights of the audit rule.</param>
	/// <param name="inheritanceFlags">The inheritance properties of the audit rule.</param>
	/// <param name="propagationFlags">Whether inherited audit rules are automatically propagated.</param>
	/// <param name="flags">The conditions for which the rule is audited.</param>
	public AuditRule(string identity, T rights, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
		: this((IdentityReference)new NTAccount(identity), rights, inheritanceFlags, propagationFlags, flags)
	{
	}

	/// <summary>Initializes a new instance of the AuditRule’1 class by using the specified values.</summary>
	/// <param name="identity">The identity to which the audit rule applies. </param>
	/// <param name="rights">The rights of the audit rule.</param>
	/// <param name="inheritanceFlags">The inheritance properties of the audit rule.</param>
	/// <param name="propagationFlags">Whether inherited audit rules are automatically propagated.</param>
	/// <param name="flags">The conditions for which the rule is audited.</param>
	public AuditRule(IdentityReference identity, T rights, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
		: this(identity, (int)(object)rights, isInherited: false, inheritanceFlags, propagationFlags, flags)
	{
	}

	internal AuditRule(IdentityReference identity, int rights, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
		: base(identity, rights, isInherited, inheritanceFlags, propagationFlags, flags)
	{
	}
}
