using System.Security.Principal;

namespace System.Security.AccessControl;

/// <summary>Determines access to securable objects. The derived classes <see cref="T:System.Security.AccessControl.AccessRule" /> and <see cref="T:System.Security.AccessControl.AuditRule" /> offer specializations for access and audit functionality.</summary>
public abstract class AuthorizationRule
{
	private IdentityReference identity;

	private int accessMask;

	private bool isInherited;

	private InheritanceFlags inheritanceFlags;

	private PropagationFlags propagationFlags;

	/// <summary>Gets the <see cref="T:System.Security.Principal.IdentityReference" /> to which this rule applies.</summary>
	/// <returns>The <see cref="T:System.Security.Principal.IdentityReference" /> to which this rule applies.</returns>
	public IdentityReference IdentityReference => identity;

	/// <summary>Gets the value of flags that determine how this rule is inherited by child objects.</summary>
	/// <returns>A bitwise combination of the enumeration values.</returns>
	public InheritanceFlags InheritanceFlags => inheritanceFlags;

	/// <summary>Gets a value indicating whether this rule is explicitly set or is inherited from a parent container object.</summary>
	/// <returns>true if this rule is not explicitly set but is instead inherited from a parent container.</returns>
	public bool IsInherited => isInherited;

	/// <summary>Gets the value of the propagation flags, which determine how inheritance of this rule is propagated to child objects. This property is significant only when the value of the <see cref="T:System.Security.AccessControl.InheritanceFlags" /> enumeration is not <see cref="F:System.Security.AccessControl.InheritanceFlags.None" />.</summary>
	/// <returns>A bitwise combination of the enumeration values.</returns>
	public PropagationFlags PropagationFlags => propagationFlags;

	/// <summary>Gets the access mask for this rule.</summary>
	/// <returns>The access mask for this rule.</returns>
	protected internal int AccessMask => accessMask;

	internal AuthorizationRule()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AuthorizationControl.AccessRule" /> class by using the specified values.</summary>
	/// <param name="identity">The identity to which the access rule applies.  This parameter must be an object that can be cast as a <see cref="T:System.Security.Principal.SecurityIdentifier" />.</param>
	/// <param name="accessMask">The access mask of this rule. The access mask is a 32-bit collection of anonymous bits, the meaning of which is defined by the individual integrators.</param>
	/// <param name="isInherited">true to inherit this rule from a parent container.</param>
	/// <param name="inheritanceFlags">The inheritance properties of the access rule.</param>
	/// <param name="propagationFlags">Whether inherited access rules are automatically propagated. The propagation flags are ignored if <paramref name="inheritanceFlags" /> is set to <see cref="F:System.Security.AccessControl.InheritanceFlags.None" />.</param>
	/// <exception cref="T:System.ArgumentException">The value of the <paramref name="identity" /> parameter cannot be cast as a <see cref="T:System.Security.Principal.SecurityIdentifier" />.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of the <paramref name="accessMask" /> parameter is zero, or the <paramref name="inheritanceFlags" /> or <paramref name="propagationFlags" /> parameters contain unrecognized flag values.</exception>
	protected internal AuthorizationRule(IdentityReference identity, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags)
	{
		if (null == identity)
		{
			throw new ArgumentNullException("identity");
		}
		if (!(identity is SecurityIdentifier) && !(identity is NTAccount))
		{
			throw new ArgumentException("identity");
		}
		if (accessMask == 0)
		{
			throw new ArgumentException("accessMask");
		}
		if ((inheritanceFlags & ~(InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit)) != 0)
		{
			throw new ArgumentOutOfRangeException();
		}
		if ((propagationFlags & ~(PropagationFlags.NoPropagateInherit | PropagationFlags.InheritOnly)) != 0)
		{
			throw new ArgumentOutOfRangeException();
		}
		this.identity = identity;
		this.accessMask = accessMask;
		this.isInherited = isInherited;
		this.inheritanceFlags = inheritanceFlags;
		this.propagationFlags = propagationFlags;
	}
}
