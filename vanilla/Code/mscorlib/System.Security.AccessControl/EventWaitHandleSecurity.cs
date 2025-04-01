using System.Runtime.InteropServices;
using System.Security.Principal;

namespace System.Security.AccessControl;

/// <summary>Represents the Windows access control security applied to a named system wait handle. This class cannot be inherited.</summary>
public sealed class EventWaitHandleSecurity : NativeObjectSecurity
{
	/// <summary>Gets the enumeration type that the <see cref="T:System.Security.AccessControl.EventWaitHandleSecurity" /> class uses to represent access rights.</summary>
	/// <returns>A <see cref="T:System.Type" /> object representing the <see cref="T:System.Security.AccessControl.EventWaitHandleRights" /> enumeration.</returns>
	public override Type AccessRightType => typeof(EventWaitHandleRights);

	/// <summary>Gets the type that the <see cref="T:System.Security.AccessControl.EventWaitHandleSecurity" /> class uses to represent access rules.</summary>
	/// <returns>A <see cref="T:System.Type" /> object representing the <see cref="T:System.Security.AccessControl.EventWaitHandleAccessRule" /> class.</returns>
	public override Type AccessRuleType => typeof(EventWaitHandleAccessRule);

	/// <summary>Gets the type that the <see cref="T:System.Security.AccessControl.EventWaitHandleSecurity" /> class uses to represent audit rules.</summary>
	/// <returns>A <see cref="T:System.Type" /> object representing the <see cref="T:System.Security.AccessControl.EventWaitHandleAuditRule" /> class.</returns>
	public override Type AuditRuleType => typeof(EventWaitHandleAuditRule);

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.EventWaitHandleSecurity" /> class with default values.</summary>
	/// <exception cref="T:System.NotSupportedException">This class is not supported on Windows 98 or Windows Millennium Edition.</exception>
	public EventWaitHandleSecurity()
		: base(isContainer: false, ResourceType.KernelObject)
	{
	}

	internal EventWaitHandleSecurity(SafeHandle handle, AccessControlSections includeSections)
		: base(isContainer: false, ResourceType.KernelObject, handle, includeSections)
	{
	}

	/// <summary>Creates a new access control rule for the specified user, with the specified access rights, access control, and flags.</summary>
	/// <returns>An <see cref="T:System.Security.AccessControl.EventWaitHandleAccessRule" /> object representing the specified rights for the specified user.</returns>
	/// <param name="identityReference">An <see cref="T:System.Security.Principal.IdentityReference" /> that identifies the user or group the rule applies to.</param>
	/// <param name="accessMask">A bitwise combination of <see cref="T:System.Security.AccessControl.EventWaitHandleRights" /> values specifying the access rights to allow or deny, cast to an integer.</param>
	/// <param name="isInherited">Meaningless for named wait handles, because they have no hierarchy.</param>
	/// <param name="inheritanceFlags">Meaningless for named wait handles, because they have no hierarchy.</param>
	/// <param name="propagationFlags">Meaningless for named wait handles, because they have no hierarchy.</param>
	/// <param name="type">One of the <see cref="T:System.Security.AccessControl.AccessControlType" /> values specifying whether the rights are allowed or denied.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="accessMask" />, <paramref name="inheritanceFlags" />, <paramref name="propagationFlags" />, or <paramref name="type" /> specifies an invalid value.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="identityReference" /> is null. -or-<paramref name="accessMask" /> is zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="identityReference" /> is neither of type <see cref="T:System.Security.Principal.SecurityIdentifier" />, nor of a type such as <see cref="T:System.Security.Principal.NTAccount" /> that can be converted to type <see cref="T:System.Security.Principal.SecurityIdentifier" />.</exception>
	public override AccessRule AccessRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
	{
		return new EventWaitHandleAccessRule(identityReference, (EventWaitHandleRights)accessMask, type);
	}

	/// <summary>Searches for a matching access control rule with which the new rule can be merged. If none are found, adds the new rule.</summary>
	/// <param name="rule">The access control rule to add.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rule" /> is null.</exception>
	public void AddAccessRule(EventWaitHandleAccessRule rule)
	{
		AddAccessRule((AccessRule)rule);
	}

	/// <summary>Searches for an access control rule with the same user and <see cref="T:System.Security.AccessControl.AccessControlType" /> (allow or deny) as the specified access rule, and with compatible inheritance and propagation flags; if such a rule is found, the rights contained in the specified access rule are removed from it.</summary>
	/// <returns>true if a compatible rule is found; otherwise, false.</returns>
	/// <param name="rule">An <see cref="T:System.Security.AccessControl.EventWaitHandleAccessRule" /> that specifies the user and <see cref="T:System.Security.AccessControl.AccessControlType" /> to search for, and a set of inheritance and propagation flags that a matching rule, if found, must be compatible with. Specifies the rights to remove from the compatible rule, if found.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rule" /> is null.</exception>
	public bool RemoveAccessRule(EventWaitHandleAccessRule rule)
	{
		return RemoveAccessRule((AccessRule)rule);
	}

	/// <summary>Searches for all access control rules with the same user and <see cref="T:System.Security.AccessControl.AccessControlType" /> (allow or deny) as the specified rule and, if found, removes them.</summary>
	/// <param name="rule">An <see cref="T:System.Security.AccessControl.EventWaitHandleAccessRule" /> that specifies the user and <see cref="T:System.Security.AccessControl.AccessControlType" /> to search for. Any rights specified by this rule are ignored.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rule" /> is null.</exception>
	public void RemoveAccessRuleAll(EventWaitHandleAccessRule rule)
	{
		RemoveAccessRuleAll((AccessRule)rule);
	}

	/// <summary>Searches for an access control rule that exactly matches the specified rule and, if found, removes it.</summary>
	/// <param name="rule">The <see cref="T:System.Security.AccessControl.EventWaitHandleAccessRule" /> to remove.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rule" /> is null.</exception>
	public void RemoveAccessRuleSpecific(EventWaitHandleAccessRule rule)
	{
		RemoveAccessRuleSpecific((AccessRule)rule);
	}

	/// <summary>Removes all access control rules with the same user as the specified rule, regardless of <see cref="T:System.Security.AccessControl.AccessControlType" />, and then adds the specified rule.</summary>
	/// <param name="rule">The <see cref="T:System.Security.AccessControl.EventWaitHandleAccessRule" /> to add. The user specified by this rule determines the rules to remove before this rule is added.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rule" /> is null.</exception>
	public void ResetAccessRule(EventWaitHandleAccessRule rule)
	{
		ResetAccessRule((AccessRule)rule);
	}

	/// <summary>Removes all access control rules with the same user and <see cref="T:System.Security.AccessControl.AccessControlType" /> (allow or deny) as the specified rule, and then adds the specified rule.</summary>
	/// <param name="rule">The <see cref="T:System.Security.AccessControl.EventWaitHandleAccessRule" /> to add. The user and <see cref="T:System.Security.AccessControl.AccessControlType" /> of this rule determine the rules to remove before this rule is added.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rule" /> is null.</exception>
	public void SetAccessRule(EventWaitHandleAccessRule rule)
	{
		SetAccessRule((AccessRule)rule);
	}

	/// <summary>Creates a new audit rule, specifying the user the rule applies to, the access rights to audit, and the outcome that triggers the audit rule.</summary>
	/// <returns>An <see cref="T:System.Security.AccessControl.EventWaitHandleAuditRule" /> object representing the specified audit rule for the specified user. The return type of the method is the base class, <see cref="T:System.Security.AccessControl.AuditRule" />, but the return value can be cast safely to the derived class.</returns>
	/// <param name="identityReference">An <see cref="T:System.Security.Principal.IdentityReference" /> that identifies the user or group the rule applies to.</param>
	/// <param name="accessMask">A bitwise combination of <see cref="T:System.Security.AccessControl.EventWaitHandleRights" /> values specifying the access rights to audit, cast to an integer.</param>
	/// <param name="isInherited">Meaningless for named wait handles, because they have no hierarchy.</param>
	/// <param name="inheritanceFlags">Meaningless for named wait handles, because they have no hierarchy.</param>
	/// <param name="propagationFlags">Meaningless for named wait handles, because they have no hierarchy.</param>
	/// <param name="flags">A bitwise combination of <see cref="T:System.Security.AccessControl.AuditFlags" /> values specifying whether to audit successful access, failed access, or both.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="accessMask" />, <paramref name="inheritanceFlags" />, <paramref name="propagationFlags" />, or <paramref name="flags" /> specifies an invalid value.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="identityReference" /> is null. -or-<paramref name="accessMask" /> is zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="identityReference" /> is neither of type <see cref="T:System.Security.Principal.SecurityIdentifier" />, nor of a type such as <see cref="T:System.Security.Principal.NTAccount" /> that can be converted to type <see cref="T:System.Security.Principal.SecurityIdentifier" />.</exception>
	public override AuditRule AuditRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
	{
		return new EventWaitHandleAuditRule(identityReference, (EventWaitHandleRights)accessMask, flags);
	}

	/// <summary>Searches for an audit rule with which the new rule can be merged. If none are found, adds the new rule.</summary>
	/// <param name="rule">The audit rule to add. The user specified by this rule determines the search.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rule" /> is null.</exception>
	public void AddAuditRule(EventWaitHandleAuditRule rule)
	{
		AddAuditRule((AuditRule)rule);
	}

	/// <summary>Searches for an audit rule with the same user as the specified rule, and with compatible inheritance and propagation flags; if a compatible rule is found, the rights contained in the specified rule are removed from it.</summary>
	/// <returns>true if a compatible rule is found; otherwise, false.</returns>
	/// <param name="rule">An <see cref="T:System.Security.AccessControl.EventWaitHandleAuditRule" /> that specifies the user to search for and a set of inheritance and propagation flags that a matching rule, if found, must be compatible with. Specifies the rights to remove from the compatible rule, if found.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rule" /> is null.</exception>
	public bool RemoveAuditRule(EventWaitHandleAuditRule rule)
	{
		return RemoveAuditRule((AuditRule)rule);
	}

	/// <summary>Searches for all audit rules with the same user as the specified rule and, if found, removes them.</summary>
	/// <param name="rule">An <see cref="T:System.Security.AccessControl.EventWaitHandleAuditRule" /> that specifies the user to search for. Any rights specified by this rule are ignored.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rule" /> is null.</exception>
	public void RemoveAuditRuleAll(EventWaitHandleAuditRule rule)
	{
		RemoveAuditRuleAll((AuditRule)rule);
	}

	/// <summary>Searches for an audit rule that exactly matches the specified rule and, if found, removes it.</summary>
	/// <param name="rule">The <see cref="T:System.Security.AccessControl.EventWaitHandleAuditRule" /> to remove.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rule" /> is null.</exception>
	public void RemoveAuditRuleSpecific(EventWaitHandleAuditRule rule)
	{
		RemoveAuditRuleSpecific((AuditRule)rule);
	}

	/// <summary>Removes all audit rules with the same user as the specified rule, regardless of the <see cref="T:System.Security.AccessControl.AuditFlags" /> value, and then adds the specified rule.</summary>
	/// <param name="rule">The <see cref="T:System.Security.AccessControl.EventWaitHandleAuditRule" /> to add. The user specified by this rule determines the rules to remove before this rule is added.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rule" /> is null.</exception>
	public void SetAuditRule(EventWaitHandleAuditRule rule)
	{
		SetAuditRule((AuditRule)rule);
	}

	internal void Persist(SafeHandle handle)
	{
		PersistModifications(handle);
	}
}
