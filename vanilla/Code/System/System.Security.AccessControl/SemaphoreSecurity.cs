using System.Runtime.InteropServices;
using System.Security.Principal;

namespace System.Security.AccessControl;

/// <summary>Represents the Windows access control security for a named semaphore. This class cannot be inherited.</summary>
[ComVisible(false)]
public sealed class SemaphoreSecurity : NativeObjectSecurity
{
	/// <summary>Gets the enumeration that the <see cref="T:System.Security.AccessControl.SemaphoreSecurity" /> class uses to represent access rights.</summary>
	/// <returns>A <see cref="T:System.Type" /> object representing the <see cref="T:System.Security.AccessControl.SemaphoreRights" /> enumeration.</returns>
	public override Type AccessRightType => typeof(SemaphoreRights);

	/// <summary>Gets the type that the <see cref="T:System.Security.AccessControl.SemaphoreSecurity" /> class uses to represent access rules.</summary>
	/// <returns>A <see cref="T:System.Type" /> object representing the <see cref="T:System.Security.AccessControl.SemaphoreAccessRule" /> class.</returns>
	public override Type AccessRuleType => typeof(SemaphoreAccessRule);

	/// <summary>Gets the type that the <see cref="T:System.Security.AccessControl.SemaphoreSecurity" /> class uses to represent audit rules.</summary>
	/// <returns>A <see cref="T:System.Type" /> object representing the <see cref="T:System.Security.AccessControl.SemaphoreAuditRule" /> class.</returns>
	public override Type AuditRuleType => typeof(SemaphoreAuditRule);

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.SemaphoreSecurity" /> class with default values.</summary>
	/// <exception cref="T:System.NotSupportedException">This class is not supported on Windows 98 or Windows Millennium Edition.</exception>
	public SemaphoreSecurity()
		: base(isContainer: false, ResourceType.KernelObject)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.SemaphoreSecurity" /> class with the specified sections of the access control security rules from the system semaphore with the specified name.</summary>
	/// <param name="name">The name of the system semaphore whose access control security rules are to be retrieved.</param>
	/// <param name="includeSections">A combination of <see cref="T:System.Security.AccessControl.AccessControlSections" /> flags specifying the sections to retrieve.</param>
	/// <exception cref="T:System.NotSupportedException">This class is not supported on Windows 98 or Windows Millennium Edition.</exception>
	public SemaphoreSecurity(string name, AccessControlSections includeSections)
		: base(isContainer: false, ResourceType.KernelObject, name, includeSections)
	{
	}

	internal SemaphoreSecurity(SafeHandle handle, AccessControlSections includeSections)
		: base(isContainer: false, ResourceType.KernelObject, handle, includeSections)
	{
	}

	/// <summary>Creates a new access control rule for the specified user, with the specified access rights, access control, and flags.</summary>
	/// <returns>A <see cref="T:System.Security.AccessControl.SemaphoreAccessRule" /> object representing the specified rights for the specified user.</returns>
	/// <param name="identityReference">An <see cref="T:System.Security.Principal.IdentityReference" /> that identifies the user or group the rule applies to.</param>
	/// <param name="accessMask">A bitwise combination of <see cref="T:System.Security.AccessControl.SemaphoreRights" /> values specifying the access rights to allow or deny, cast to an integer.</param>
	/// <param name="isInherited">Meaningless for named semaphores, because they have no hierarchy.</param>
	/// <param name="inheritanceFlags">Meaningless for named semaphores, because they have no hierarchy.</param>
	/// <param name="propagationFlags">Meaningless for named semaphores, because they have no hierarchy.</param>
	/// <param name="type">One of the <see cref="T:System.Security.AccessControl.AccessControlType" /> values specifying whether the rights are allowed or denied.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="accessMask" />, <paramref name="inheritanceFlags" />, <paramref name="propagationFlags" />, or <paramref name="type" /> specifies an invalid value.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="identityReference" /> is null. -or-<paramref name="accessMask" /> is zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="identityReference" /> is neither of type <see cref="T:System.Security.Principal.SecurityIdentifier" />, nor of a type such as <see cref="T:System.Security.Principal.NTAccount" /> that can be converted to type <see cref="T:System.Security.Principal.SecurityIdentifier" />.</exception>
	public override AccessRule AccessRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
	{
		return new SemaphoreAccessRule(identityReference, (SemaphoreRights)accessMask, type);
	}

	/// <summary>Searches for a matching rule with which the new rule can be merged. If none are found, adds the new rule.</summary>
	/// <param name="rule">The access control rule to add.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rule" /> is null.</exception>
	public void AddAccessRule(SemaphoreAccessRule rule)
	{
		AddAccessRule((AccessRule)rule);
	}

	/// <summary>Searches for an access control rule with the same user and <see cref="T:System.Security.AccessControl.AccessControlType" /> (allow or deny) as the specified rule, and with compatible inheritance and propagation flags; if such a rule is found, the rights contained in the specified access rule are removed from it.</summary>
	/// <returns>true if a compatible rule is found; otherwise false.</returns>
	/// <param name="rule">A <see cref="T:System.Security.AccessControl.SemaphoreAccessRule" /> that specifies the user and <see cref="T:System.Security.AccessControl.AccessControlType" /> to search for, and a set of inheritance and propagation flags that a matching rule, if found, must be compatible with. Specifies the rights to remove from the compatible rule, if found.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rule" /> is null.</exception>
	public bool RemoveAccessRule(SemaphoreAccessRule rule)
	{
		return RemoveAccessRule((AccessRule)rule);
	}

	/// <summary>Searches for all access control rules with the same user and <see cref="T:System.Security.AccessControl.AccessControlType" /> (allow or deny) as the specified rule and, if found, removes them.</summary>
	/// <param name="rule">A <see cref="T:System.Security.AccessControl.SemaphoreAccessRule" /> that specifies the user and <see cref="T:System.Security.AccessControl.AccessControlType" /> to search for. Any rights specified by this rule are ignored.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rule" /> is null.</exception>
	public void RemoveAccessRuleAll(SemaphoreAccessRule rule)
	{
		RemoveAccessRuleAll((AccessRule)rule);
	}

	/// <summary>Searches for an access control rule that exactly matches the specified rule and, if found, removes it.</summary>
	/// <param name="rule">The <see cref="T:System.Security.AccessControl.SemaphoreAccessRule" /> to remove.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rule" /> is null.</exception>
	public void RemoveAccessRuleSpecific(SemaphoreAccessRule rule)
	{
		RemoveAccessRuleSpecific((AccessRule)rule);
	}

	/// <summary>Removes all access control rules with the same user as the specified rule, regardless of <see cref="T:System.Security.AccessControl.AccessControlType" />, and then adds the specified rule.</summary>
	/// <param name="rule">The <see cref="T:System.Security.AccessControl.SemaphoreAccessRule" /> to add. The user specified by this rule determines the rules to remove before this rule is added.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rule" /> is null.</exception>
	public void ResetAccessRule(SemaphoreAccessRule rule)
	{
		ResetAccessRule((AccessRule)rule);
	}

	/// <summary>Removes all access control rules with the same user and <see cref="T:System.Security.AccessControl.AccessControlType" /> (allow or deny) as the specified rule, and then adds the specified rule.</summary>
	/// <param name="rule">The <see cref="T:System.Security.AccessControl.SemaphoreAccessRule" /> to add. The user and <see cref="T:System.Security.AccessControl.AccessControlType" /> of this rule determine the rules to remove before this rule is added.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rule" /> is null.</exception>
	public void SetAccessRule(SemaphoreAccessRule rule)
	{
		SetAccessRule((AccessRule)rule);
	}

	/// <summary>Creates a new audit rule, specifying the user the rule applies to, the access rights to audit, and the outcome that triggers the audit rule.</summary>
	/// <returns>A <see cref="T:System.Security.AccessControl.SemaphoreAuditRule" /> object representing the specified audit rule for the specified user. The return type of the method is the base class, <see cref="T:System.Security.AccessControl.AuditRule" />, but the return value can be cast safely to the derived class.</returns>
	/// <param name="identityReference">An <see cref="T:System.Security.Principal.IdentityReference" /> that identifies the user or group the rule applies to.</param>
	/// <param name="accessMask">A bitwise combination of <see cref="T:System.Security.AccessControl.SemaphoreRights" /> values specifying the access rights to audit, cast to an integer.</param>
	/// <param name="isInherited">Meaningless for named wait handles, because they have no hierarchy.</param>
	/// <param name="inheritanceFlags">Meaningless for named wait handles, because they have no hierarchy.</param>
	/// <param name="propagationFlags">Meaningless for named wait handles, because they have no hierarchy.</param>
	/// <param name="flags">A bitwise combination of <see cref="T:System.Security.AccessControl.AuditFlags" /> values that specify whether to audit successful access, failed access, or both.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="accessMask" />, <paramref name="inheritanceFlags" />, <paramref name="propagationFlags" />, or <paramref name="flags" /> specifies an invalid value.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="identityReference" /> is null. -or-<paramref name="accessMask" /> is zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="identityReference" /> is neither of type <see cref="T:System.Security.Principal.SecurityIdentifier" />, nor of a type such as <see cref="T:System.Security.Principal.NTAccount" /> that can be converted to type <see cref="T:System.Security.Principal.SecurityIdentifier" />.</exception>
	public override AuditRule AuditRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
	{
		return new SemaphoreAuditRule(identityReference, (SemaphoreRights)accessMask, flags);
	}

	/// <summary>Searches for an audit rule with which the new rule can be merged. If none are found, adds the new rule.</summary>
	/// <param name="rule">The audit rule to add. The user specified by this rule determines the search.</param>
	public void AddAuditRule(SemaphoreAuditRule rule)
	{
		AddAuditRule((AuditRule)rule);
	}

	/// <summary>Searches for an audit control rule with the same user as the specified rule, and with compatible inheritance and propagation flags; if a compatible rule is found, the rights contained in the specified rule are removed from it.</summary>
	/// <returns>true if a compatible rule is found; otherwise, false.</returns>
	/// <param name="rule">A <see cref="T:System.Security.AccessControl.SemaphoreAuditRule" /> that specifies the user to search for, and a set of inheritance and propagation flags that a matching rule, if found, must be compatible with. Specifies the rights to remove from the compatible rule, if found.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rule" /> is null.</exception>
	public bool RemoveAuditRule(SemaphoreAuditRule rule)
	{
		return RemoveAuditRule((AuditRule)rule);
	}

	/// <summary>Searches for all audit rules with the same user as the specified rule and, if found, removes them.</summary>
	/// <param name="rule">A <see cref="T:System.Security.AccessControl.SemaphoreAuditRule" /> that specifies the user to search for. Any rights specified by this rule are ignored.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rule" /> is null.</exception>
	public void RemoveAuditRuleAll(SemaphoreAuditRule rule)
	{
		RemoveAuditRuleAll((AuditRule)rule);
	}

	/// <summary>Searches for an audit rule that exactly matches the specified rule and, if found, removes it.</summary>
	/// <param name="rule">The <see cref="T:System.Security.AccessControl.SemaphoreAuditRule" /> to remove.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rule" /> is null.</exception>
	public void RemoveAuditRuleSpecific(SemaphoreAuditRule rule)
	{
		RemoveAuditRuleSpecific((AuditRule)rule);
	}

	/// <summary>Removes all audit rules with the same user as the specified rule, regardless of the <see cref="T:System.Security.AccessControl.AuditFlags" /> value, and then adds the specified rule.</summary>
	/// <param name="rule">The <see cref="T:System.Security.AccessControl.SemaphoreAuditRule" /> to add. The user specified by this rule determines the rules to remove before this rule is added.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rule" /> is null.</exception>
	public void SetAuditRule(SemaphoreAuditRule rule)
	{
		SetAuditRule((AuditRule)rule);
	}

	internal void Persist(SafeHandle handle)
	{
		WriteLock();
		try
		{
			Persist(handle, (AccessControlSections)((base.AccessRulesModified ? 2 : 0) | (base.AuditRulesModified ? 1 : 0) | (base.OwnerModified ? 4 : 0) | (base.GroupModified ? 8 : 0)), null);
		}
		finally
		{
			WriteUnlock();
		}
	}
}
