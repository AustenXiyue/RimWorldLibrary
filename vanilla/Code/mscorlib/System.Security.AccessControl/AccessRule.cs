using System.Security.Principal;

namespace System.Security.AccessControl;

/// <summary>Represents a combination of a user's identity, an access mask, and an access control type (allow or deny). An <see cref="T:System.Security.AccessControl.AccessRule" /> object also contains information about the how the rule is inherited by child objects and how that inheritance is propagated.</summary>
public abstract class AccessRule : AuthorizationRule
{
	private AccessControlType type;

	/// <summary>Gets the <see cref="T:System.Security.AccessControl.AccessControlType" /> value associated with this <see cref="T:System.Security.AccessControl.AccessRule" /> object.</summary>
	/// <returns>The <see cref="T:System.Security.AccessControl.AccessControlType" /> value associated with this <see cref="T:System.Security.AccessControl.AccessRule" /> object.</returns>
	public AccessControlType AccessControlType => type;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.AccessRule" /> class by using the specified values.</summary>
	/// <param name="identity">The identity to which the access rule applies. This parameter must be an object that can be cast as a <see cref="T:System.Security.Principal.SecurityIdentifier" />.</param>
	/// <param name="accessMask">The access mask of this rule. The access mask is a 32-bit collection of anonymous bits, the meaning of which is defined by the individual integrators.</param>
	/// <param name="isInherited">true if this rule is inherited from a parent container.</param>
	/// <param name="inheritanceFlags">The inheritance properties of the access rule.</param>
	/// <param name="propagationFlags">Whether inherited access rules are automatically propagated. The propagation flags are ignored if <paramref name="inheritanceFlags" /> is set to <see cref="F:System.Security.AccessControl.InheritanceFlags.None" />.</param>
	/// <param name="type">The valid access control type.</param>
	/// <exception cref="T:System.ArgumentException">The value of the <paramref name="identity" /> parameter cannot be cast as a <see cref="T:System.Security.Principal.SecurityIdentifier" />, or the <paramref name="type" /> parameter contains an invalid value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of the <paramref name="accessMask" /> parameter is zero, or the <paramref name="inheritanceFlags" /> or <paramref name="propagationFlags" /> parameters contain unrecognized flag values.</exception>
	protected AccessRule(IdentityReference identity, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
		: base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags)
	{
		if (type < AccessControlType.Allow || type > AccessControlType.Deny)
		{
			throw new ArgumentException("Invalid access control type.", "type");
		}
		this.type = type;
	}
}
/// <summary>Represents a combination of a user's identity, an access mask, and an access control type (allow or deny). An AccessRule`1 object also contains information about the how the rule is inherited by child objects and how that inheritance is propagated.</summary>
/// <typeparam name="T">The access rights type for the access rule.</typeparam>
public class AccessRule<T> : AccessRule where T : struct
{
	/// <summary>Gets the rights of the current instance.</summary>
	/// <returns>The rights, cast as type &lt;T&gt;, of the current instance.</returns>
	public T Rights => (T)(object)base.AccessMask;

	/// <summary>Initializes a new instance of the AccessRule’1 class by using the specified values.</summary>
	/// <param name="identity">The identity to which the access rule applies.</param>
	/// <param name="rights">The rights of the access rule.</param>
	/// <param name="type">The valid access control type.</param>
	public AccessRule(string identity, T rights, AccessControlType type)
		: this((IdentityReference)new NTAccount(identity), rights, type)
	{
	}

	/// <summary>Initializes a new instance of the AccessRule’1 class by using the specified values.</summary>
	/// <param name="identity">The identity to which the access rule applies.</param>
	/// <param name="rights">The rights of the access rule.</param>
	/// <param name="type">The valid access control type.</param>
	public AccessRule(IdentityReference identity, T rights, AccessControlType type)
		: this(identity, rights, InheritanceFlags.None, PropagationFlags.None, type)
	{
	}

	/// <summary>Initializes a new instance of the AccessRule’1 class by using the specified values.</summary>
	/// <param name="identity">The identity to which the access rule applies.</param>
	/// <param name="rights">The rights of the access rule.</param>
	/// <param name="inheritanceFlags">The inheritance properties of the access rule.</param>
	/// <param name="propagationFlags">Whether inherited access rules are automatically propagated. The propagation flags are ignored if <paramref name="inheritanceFlags" /> is set to <see cref="F:System.Security.AccessControl.InheritanceFlags.None" />.</param>
	/// <param name="type">The valid access control type.</param>
	public AccessRule(string identity, T rights, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
		: this((IdentityReference)new NTAccount(identity), rights, inheritanceFlags, propagationFlags, type)
	{
	}

	/// <summary>Initializes a new instance of the AccessRule’1 class by using the specified values.</summary>
	/// <param name="identity">The identity to which the access rule applies.</param>
	/// <param name="rights">The rights of the access rule.</param>
	/// <param name="inheritanceFlags">The inheritance properties of the access rule.</param>
	/// <param name="propagationFlags">Whether inherited access rules are automatically propagated. The propagation flags are ignored if <paramref name="inheritanceFlags" /> is set to <see cref="F:System.Security.AccessControl.InheritanceFlags.None" />.</param>
	/// <param name="type">The valid access control type.</param>
	public AccessRule(IdentityReference identity, T rights, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
		: this(identity, (int)(object)rights, isInherited: false, inheritanceFlags, propagationFlags, type)
	{
	}

	internal AccessRule(IdentityReference identity, int rights, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
		: base(identity, rights, isInherited, inheritanceFlags, propagationFlags, type)
	{
	}
}
