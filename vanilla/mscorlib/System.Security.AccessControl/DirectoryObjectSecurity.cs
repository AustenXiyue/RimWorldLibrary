using System.Security.Principal;

namespace System.Security.AccessControl;

/// <summary>Provides the ability to control access to directory objects without direct manipulation of Access Control Lists (ACLs).</summary>
public abstract class DirectoryObjectSecurity : ObjectSecurity
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.DirectoryObjectSecurity" /> class.</summary>
	protected DirectoryObjectSecurity()
		: base(isContainer: true, isDS: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.DirectoryObjectSecurity" /> class with the specified security descriptor.</summary>
	/// <param name="securityDescriptor">The security descriptor to be associated with the new <see cref="T:System.Security.AccessControl.DirectoryObjectSecurity" />object.</param>
	protected DirectoryObjectSecurity(CommonSecurityDescriptor securityDescriptor)
		: base(securityDescriptor)
	{
	}

	private Exception GetNotImplementedException()
	{
		return new NotImplementedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.AccessRule" /> class with the specified values.</summary>
	/// <returns>The <see cref="T:System.Security.AccessControl.AccessRule" /> object that this method creates.</returns>
	/// <param name="identityReference">The identity to which the access rule applies.  It must be an object that can be cast as a <see cref="T:System.Security.Principal.SecurityIdentifier" />.</param>
	/// <param name="accessMask">The access mask of this rule. The access mask is a 32-bit collection of anonymous bits, the meaning of which is defined by the individual integrators.</param>
	/// <param name="isInherited">true if this rule is inherited from a parent container.</param>
	/// <param name="inheritanceFlags">Specifies the inheritance properties of the access rule.</param>
	/// <param name="propagationFlags">Specifies whether inherited access rules are automatically propagated. The propagation flags are ignored if <paramref name="inheritanceFlags" /> is set to <see cref="F:System.Security.AccessControl.InheritanceFlags.None" />.</param>
	/// <param name="type">Specifies the valid access control type.</param>
	/// <param name="objectType">The identity of the class of objects to which the new access rule applies.</param>
	/// <param name="inheritedObjectType">The identity of the class of child objects which can inherit the new access rule.</param>
	public virtual AccessRule AccessRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type, Guid objectType, Guid inheritedObjectType)
	{
		throw GetNotImplementedException();
	}

	internal override AccessRule InternalAccessRuleFactory(QualifiedAce ace, Type targetType, AccessControlType type)
	{
		ObjectAce objectAce = ace as ObjectAce;
		if (null == objectAce || objectAce.ObjectAceFlags == ObjectAceFlags.None)
		{
			return base.InternalAccessRuleFactory(ace, targetType, type);
		}
		return AccessRuleFactory(ace.SecurityIdentifier.Translate(targetType), ace.AccessMask, ace.IsInherited, ace.InheritanceFlags, ace.PropagationFlags, type, objectAce.ObjectAceType, objectAce.InheritedObjectAceType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.AuditRule" /> class with the specified values.</summary>
	/// <returns>The <see cref="T:System.Security.AccessControl.AuditRule" /> object that this method creates.</returns>
	/// <param name="identityReference">The identity to which the audit rule applies.  It must be an object that can be cast as a <see cref="T:System.Security.Principal.SecurityIdentifier" />.</param>
	/// <param name="accessMask">The access mask of this rule. The access mask is a 32-bit collection of anonymous bits, the meaning of which is defined by the individual integrators.</param>
	/// <param name="isInherited">true if this rule is inherited from a parent container.</param>
	/// <param name="inheritanceFlags">Specifies the inheritance properties of the audit rule.</param>
	/// <param name="propagationFlags">Specifies whether inherited audit rules are automatically propagated. The propagation flags are ignored if <paramref name="inheritanceFlags" /> is set to <see cref="F:System.Security.AccessControl.InheritanceFlags.None" />.</param>
	/// <param name="flags">Specifies the conditions for which the rule is audited.</param>
	/// <param name="objectType">The identity of the class of objects to which the new audit rule applies.</param>
	/// <param name="inheritedObjectType">The identity of the class of child objects which can inherit the new audit rule.</param>
	public virtual AuditRule AuditRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags, Guid objectType, Guid inheritedObjectType)
	{
		throw GetNotImplementedException();
	}

	internal override AuditRule InternalAuditRuleFactory(QualifiedAce ace, Type targetType)
	{
		ObjectAce objectAce = ace as ObjectAce;
		if (null == objectAce || objectAce.ObjectAceFlags == ObjectAceFlags.None)
		{
			return base.InternalAuditRuleFactory(ace, targetType);
		}
		return AuditRuleFactory(ace.SecurityIdentifier.Translate(targetType), ace.AccessMask, ace.IsInherited, ace.InheritanceFlags, ace.PropagationFlags, ace.AuditFlags, objectAce.ObjectAceType, objectAce.InheritedObjectAceType);
	}

	/// <summary>Gets a collection of the access rules associated with the specified security identifier.</summary>
	/// <returns>The collection of access rules associated with the specified <see cref="T:System.Security.Principal.SecurityIdentifier" /> object.</returns>
	/// <param name="includeExplicit">true to include access rules explicitly set for the object.</param>
	/// <param name="includeInherited">true to include inherited access rules.</param>
	/// <param name="targetType">The security identifier for which to retrieve access rules. This must be an object that can be cast as a <see cref="T:System.Security.Principal.SecurityIdentifier" /> object.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	public AuthorizationRuleCollection GetAccessRules(bool includeExplicit, bool includeInherited, Type targetType)
	{
		return InternalGetAccessRules(includeExplicit, includeInherited, targetType);
	}

	/// <summary>Gets a collection of the audit rules associated with the specified security identifier.</summary>
	/// <returns>The collection of audit rules associated with the specified <see cref="T:System.Security.Principal.SecurityIdentifier" /> object.</returns>
	/// <param name="includeExplicit">true to include audit rules explicitly set for the object.</param>
	/// <param name="includeInherited">true to include inherited audit rules.</param>
	/// <param name="targetType">The security identifier for which to retrieve audit rules. This must be an object that can be cast as a <see cref="T:System.Security.Principal.SecurityIdentifier" /> object.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	public AuthorizationRuleCollection GetAuditRules(bool includeExplicit, bool includeInherited, Type targetType)
	{
		return InternalGetAuditRules(includeExplicit, includeInherited, targetType);
	}

	/// <summary>Adds the specified access rule to the Discretionary Access Control List (DACL) associated with this <see cref="T:System.Security.AccessControl.DirectoryObjectSecurity" /> object.</summary>
	/// <param name="rule">The access rule to add.</param>
	protected void AddAccessRule(ObjectAccessRule rule)
	{
		ModifyAccess(AccessControlModification.Add, rule, out var _);
	}

	/// <summary>Removes access rules that contain the same security identifier and access mask as the specified access rule from the Discretionary Access Control List (DACL) associated with this <see cref="T:System.Security.AccessControl.DirectoryObjectSecurity" /> object.</summary>
	/// <returns>true if the access rule was successfully removed; otherwise, false.</returns>
	/// <param name="rule">The access rule to remove.</param>
	protected bool RemoveAccessRule(ObjectAccessRule rule)
	{
		bool modified;
		return ModifyAccess(AccessControlModification.Remove, rule, out modified);
	}

	/// <summary>Removes all access rules that have the same security identifier as the specified access rule from the Discretionary Access Control List (DACL) associated with this <see cref="T:System.Security.AccessControl.DirectoryObjectSecurity" /> object.</summary>
	/// <param name="rule">The access rule to remove.</param>
	protected void RemoveAccessRuleAll(ObjectAccessRule rule)
	{
		ModifyAccess(AccessControlModification.RemoveAll, rule, out var _);
	}

	/// <summary>Removes all access rules that exactly match the specified access rule from the Discretionary Access Control List (DACL) associated with this <see cref="T:System.Security.AccessControl.DirectoryObjectSecurity" /> object.</summary>
	/// <param name="rule">The access rule to remove.</param>
	protected void RemoveAccessRuleSpecific(ObjectAccessRule rule)
	{
		ModifyAccess(AccessControlModification.RemoveSpecific, rule, out var _);
	}

	/// <summary>Removes all access rules in the Discretionary Access Control List (DACL) associated with this <see cref="T:System.Security.AccessControl.DirectoryObjectSecurity" /> object and then adds the specified access rule.</summary>
	/// <param name="rule">The access rule to reset.</param>
	protected void ResetAccessRule(ObjectAccessRule rule)
	{
		ModifyAccess(AccessControlModification.Reset, rule, out var _);
	}

	/// <summary>Removes all access rules that contain the same security identifier and qualifier as the specified access rule in the Discretionary Access Control List (DACL) associated with this <see cref="T:System.Security.AccessControl.DirectoryObjectSecurity" /> object and then adds the specified access rule.</summary>
	/// <param name="rule">The access rule to set.</param>
	protected void SetAccessRule(ObjectAccessRule rule)
	{
		ModifyAccess(AccessControlModification.Set, rule, out var _);
	}

	/// <summary>Applies the specified modification to the Discretionary Access Control List (DACL) associated with this <see cref="T:System.Security.AccessControl.DirectoryObjectSecurity" /> object.</summary>
	/// <returns>true if the DACL is successfully modified; otherwise, false.</returns>
	/// <param name="modification">The modification to apply to the DACL.</param>
	/// <param name="rule">The access rule to modify.</param>
	/// <param name="modified">true if the DACL is successfully modified; otherwise, false.</param>
	protected override bool ModifyAccess(AccessControlModification modification, AccessRule rule, out bool modified)
	{
		if (rule == null)
		{
			throw new ArgumentNullException("rule");
		}
		if (!(rule is ObjectAccessRule objectAccessRule))
		{
			throw new ArgumentException("rule");
		}
		modified = true;
		WriteLock();
		try
		{
			switch (modification)
			{
			case AccessControlModification.Add:
				descriptor.DiscretionaryAcl.AddAccess(objectAccessRule.AccessControlType, ObjectSecurity.SidFromIR(objectAccessRule.IdentityReference), objectAccessRule.AccessMask, objectAccessRule.InheritanceFlags, objectAccessRule.PropagationFlags, objectAccessRule.ObjectFlags, objectAccessRule.ObjectType, objectAccessRule.InheritedObjectType);
				break;
			case AccessControlModification.Set:
				descriptor.DiscretionaryAcl.SetAccess(objectAccessRule.AccessControlType, ObjectSecurity.SidFromIR(objectAccessRule.IdentityReference), objectAccessRule.AccessMask, objectAccessRule.InheritanceFlags, objectAccessRule.PropagationFlags, objectAccessRule.ObjectFlags, objectAccessRule.ObjectType, objectAccessRule.InheritedObjectType);
				break;
			case AccessControlModification.Reset:
				PurgeAccessRules(objectAccessRule.IdentityReference);
				goto case AccessControlModification.Add;
			case AccessControlModification.Remove:
				modified = descriptor.DiscretionaryAcl.RemoveAccess(objectAccessRule.AccessControlType, ObjectSecurity.SidFromIR(objectAccessRule.IdentityReference), rule.AccessMask, objectAccessRule.InheritanceFlags, objectAccessRule.PropagationFlags, objectAccessRule.ObjectFlags, objectAccessRule.ObjectType, objectAccessRule.InheritedObjectType);
				break;
			case AccessControlModification.RemoveAll:
				PurgeAccessRules(objectAccessRule.IdentityReference);
				break;
			case AccessControlModification.RemoveSpecific:
				descriptor.DiscretionaryAcl.RemoveAccessSpecific(objectAccessRule.AccessControlType, ObjectSecurity.SidFromIR(objectAccessRule.IdentityReference), objectAccessRule.AccessMask, objectAccessRule.InheritanceFlags, objectAccessRule.PropagationFlags, objectAccessRule.ObjectFlags, objectAccessRule.ObjectType, objectAccessRule.InheritedObjectType);
				break;
			default:
				throw new ArgumentOutOfRangeException("modification");
			}
			if (modified)
			{
				base.AccessRulesModified = true;
			}
		}
		finally
		{
			WriteUnlock();
		}
		return modified;
	}

	/// <summary>Adds the specified audit rule to the System Access Control List (SACL) associated with this <see cref="T:System.Security.AccessControl.DirectoryObjectSecurity" /> object.</summary>
	/// <param name="rule">The audit rule to add.</param>
	protected void AddAuditRule(ObjectAuditRule rule)
	{
		ModifyAudit(AccessControlModification.Add, rule, out var _);
	}

	/// <summary>Removes audit rules that contain the same security identifier and access mask as the specified audit rule from the System Access Control List (SACL) associated with this <see cref="T:System.Security.AccessControl.CommonObjectSecurity" /> object.</summary>
	/// <returns>true if the audit rule was successfully removed; otherwise, false.</returns>
	/// <param name="rule">The audit rule to remove.</param>
	protected bool RemoveAuditRule(ObjectAuditRule rule)
	{
		bool modified;
		return ModifyAudit(AccessControlModification.Remove, rule, out modified);
	}

	/// <summary>Removes all audit rules that have the same security identifier as the specified audit rule from the System Access Control List (SACL) associated with this <see cref="T:System.Security.AccessControl.DirectoryObjectSecurity" /> object.</summary>
	/// <param name="rule">The audit rule to remove.</param>
	protected void RemoveAuditRuleAll(ObjectAuditRule rule)
	{
		ModifyAudit(AccessControlModification.RemoveAll, rule, out var _);
	}

	/// <summary>Removes all audit rules that exactly match the specified audit rule from the System Access Control List (SACL) associated with this <see cref="T:System.Security.AccessControl.DirectoryObjectSecurity" /> object.</summary>
	/// <param name="rule">The audit rule to remove.</param>
	protected void RemoveAuditRuleSpecific(ObjectAuditRule rule)
	{
		ModifyAudit(AccessControlModification.RemoveSpecific, rule, out var _);
	}

	/// <summary>Removes all audit rules that contain the same security identifier and qualifier as the specified audit rule in the System Access Control List (SACL) associated with this <see cref="T:System.Security.AccessControl.DirectoryObjectSecurity" /> object and then adds the specified audit rule.</summary>
	/// <param name="rule">The audit rule to set.</param>
	protected void SetAuditRule(ObjectAuditRule rule)
	{
		ModifyAudit(AccessControlModification.Set, rule, out var _);
	}

	/// <summary>Applies the specified modification to the System Access Control List (SACL) associated with this <see cref="T:System.Security.AccessControl.DirectoryObjectSecurity" /> object.</summary>
	/// <returns>true if the SACL is successfully modified; otherwise, false.</returns>
	/// <param name="modification">The modification to apply to the SACL.</param>
	/// <param name="rule">The audit rule to modify.</param>
	/// <param name="modified">true if the SACL is successfully modified; otherwise, false.</param>
	protected override bool ModifyAudit(AccessControlModification modification, AuditRule rule, out bool modified)
	{
		if (rule == null)
		{
			throw new ArgumentNullException("rule");
		}
		if (!(rule is ObjectAuditRule objectAuditRule))
		{
			throw new ArgumentException("rule");
		}
		modified = true;
		WriteLock();
		try
		{
			switch (modification)
			{
			case AccessControlModification.Add:
				if (descriptor.SystemAcl == null)
				{
					descriptor.SystemAcl = new SystemAcl(base.IsContainer, base.IsDS, 1);
				}
				descriptor.SystemAcl.AddAudit(objectAuditRule.AuditFlags, ObjectSecurity.SidFromIR(objectAuditRule.IdentityReference), objectAuditRule.AccessMask, objectAuditRule.InheritanceFlags, objectAuditRule.PropagationFlags, objectAuditRule.ObjectFlags, objectAuditRule.ObjectType, objectAuditRule.InheritedObjectType);
				break;
			case AccessControlModification.Set:
				if (descriptor.SystemAcl == null)
				{
					descriptor.SystemAcl = new SystemAcl(base.IsContainer, base.IsDS, 1);
				}
				descriptor.SystemAcl.SetAudit(objectAuditRule.AuditFlags, ObjectSecurity.SidFromIR(objectAuditRule.IdentityReference), objectAuditRule.AccessMask, objectAuditRule.InheritanceFlags, objectAuditRule.PropagationFlags, objectAuditRule.ObjectFlags, objectAuditRule.ObjectType, objectAuditRule.InheritedObjectType);
				break;
			case AccessControlModification.Remove:
				if (descriptor.SystemAcl == null)
				{
					modified = false;
				}
				else
				{
					modified = descriptor.SystemAcl.RemoveAudit(objectAuditRule.AuditFlags, ObjectSecurity.SidFromIR(objectAuditRule.IdentityReference), objectAuditRule.AccessMask, objectAuditRule.InheritanceFlags, objectAuditRule.PropagationFlags, objectAuditRule.ObjectFlags, objectAuditRule.ObjectType, objectAuditRule.InheritedObjectType);
				}
				break;
			case AccessControlModification.RemoveAll:
				PurgeAuditRules(objectAuditRule.IdentityReference);
				break;
			case AccessControlModification.RemoveSpecific:
				if (descriptor.SystemAcl != null)
				{
					descriptor.SystemAcl.RemoveAuditSpecific(objectAuditRule.AuditFlags, ObjectSecurity.SidFromIR(objectAuditRule.IdentityReference), objectAuditRule.AccessMask, objectAuditRule.InheritanceFlags, objectAuditRule.PropagationFlags, objectAuditRule.ObjectFlags, objectAuditRule.ObjectType, objectAuditRule.InheritedObjectType);
				}
				break;
			default:
				throw new ArgumentOutOfRangeException("modification");
			case AccessControlModification.Reset:
				break;
			}
			if (modified)
			{
				base.AuditRulesModified = true;
			}
		}
		finally
		{
			WriteUnlock();
		}
		return modified;
	}
}
