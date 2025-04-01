using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;

namespace System.Security.AccessControl;

/// <summary>Provides the ability to control access to objects without direct manipulation of Access Control Lists (ACLs). This class is the abstract base class for the <see cref="T:System.Security.AccessControl.CommonObjectSecurity" /> and <see cref="T:System.Security.AccessControl.DirectoryObjectSecurity" /> classes.</summary>
public abstract class ObjectSecurity
{
	internal CommonSecurityDescriptor descriptor;

	private AccessControlSections sections_modified;

	private ReaderWriterLock rw_lock;

	/// <summary>Gets the <see cref="T:System.Type" /> of the securable object associated with this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object.</summary>
	/// <returns>The type of the securable object associated with this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object.</returns>
	public abstract Type AccessRightType { get; }

	/// <summary>Gets the <see cref="T:System.Type" /> of the object associated with the access rules of this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object. The <see cref="T:System.Type" /> object must be an object that can be cast as a <see cref="T:System.Security.Principal.SecurityIdentifier" /> object.</summary>
	/// <returns>The type of the object associated with the access rules of this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object.</returns>
	public abstract Type AccessRuleType { get; }

	/// <summary>Gets the <see cref="T:System.Type" /> object associated with the audit rules of this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object. The <see cref="T:System.Type" /> object must be an object that can be cast as a <see cref="T:System.Security.Principal.SecurityIdentifier" /> object.</summary>
	/// <returns>The type of the object associated with the audit rules of this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object.</returns>
	public abstract Type AuditRuleType { get; }

	/// <summary>Gets a Boolean value that specifies whether the access rules associated with this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object are in canonical order.</summary>
	/// <returns>true if the access rules are in canonical order; otherwise, false.</returns>
	public bool AreAccessRulesCanonical
	{
		get
		{
			ReadLock();
			try
			{
				return descriptor.IsDiscretionaryAclCanonical;
			}
			finally
			{
				ReadUnlock();
			}
		}
	}

	/// <summary>Gets a Boolean value that specifies whether the Discretionary Access Control List (DACL) associated with this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object is protected.</summary>
	/// <returns>true if the DACL is protected; otherwise, false.</returns>
	public bool AreAccessRulesProtected
	{
		get
		{
			ReadLock();
			try
			{
				return (descriptor.ControlFlags & ControlFlags.DiscretionaryAclProtected) != 0;
			}
			finally
			{
				ReadUnlock();
			}
		}
	}

	/// <summary>Gets a Boolean value that specifies whether the audit rules associated with this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object are in canonical order.</summary>
	/// <returns>true if the audit rules are in canonical order; otherwise, false.</returns>
	public bool AreAuditRulesCanonical
	{
		get
		{
			ReadLock();
			try
			{
				return descriptor.IsSystemAclCanonical;
			}
			finally
			{
				ReadUnlock();
			}
		}
	}

	/// <summary>Gets a Boolean value that specifies whether the System Access Control List (SACL) associated with this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object is protected.</summary>
	/// <returns>true if the SACL is protected; otherwise, false.</returns>
	public bool AreAuditRulesProtected
	{
		get
		{
			ReadLock();
			try
			{
				return (descriptor.ControlFlags & ControlFlags.SystemAclProtected) != 0;
			}
			finally
			{
				ReadUnlock();
			}
		}
	}

	internal AccessControlSections AccessControlSectionsModified
	{
		get
		{
			Reading();
			return sections_modified;
		}
		set
		{
			Writing();
			sections_modified = value;
		}
	}

	/// <summary>Gets or sets a Boolean value that specifies whether the access rules associated with this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object have been modified.</summary>
	/// <returns>true if the access rules associated with this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object have been modified; otherwise, false.</returns>
	protected bool AccessRulesModified
	{
		get
		{
			return AreAccessControlSectionsModified(AccessControlSections.Access);
		}
		set
		{
			SetAccessControlSectionsModified(AccessControlSections.Access, value);
		}
	}

	/// <summary>Gets or sets a Boolean value that specifies whether the audit rules associated with this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object have been modified.</summary>
	/// <returns>true if the audit rules associated with this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object have been modified; otherwise, false.</returns>
	protected bool AuditRulesModified
	{
		get
		{
			return AreAccessControlSectionsModified(AccessControlSections.Audit);
		}
		set
		{
			SetAccessControlSectionsModified(AccessControlSections.Audit, value);
		}
	}

	/// <summary>Gets or sets a Boolean value that specifies whether the group associated with the securable object has been modified. </summary>
	/// <returns>true if the group associated with the securable object has been modified; otherwise, false.</returns>
	protected bool GroupModified
	{
		get
		{
			return AreAccessControlSectionsModified(AccessControlSections.Group);
		}
		set
		{
			SetAccessControlSectionsModified(AccessControlSections.Group, value);
		}
	}

	/// <summary>Gets a Boolean value that specifies whether this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object is a container object.</summary>
	/// <returns>true if the <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object is a container object; otherwise, false.</returns>
	protected bool IsContainer => descriptor.IsContainer;

	/// <summary>Gets a Boolean value that specifies whether this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object is a directory object.</summary>
	/// <returns>true if the <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object is a directory object; otherwise, false.</returns>
	protected bool IsDS => descriptor.IsDS;

	/// <summary>Gets or sets a Boolean value that specifies whether the owner of the securable object has been modified.</summary>
	/// <returns>true if the owner of the securable object has been modified; otherwise, false.</returns>
	protected bool OwnerModified
	{
		get
		{
			return AreAccessControlSectionsModified(AccessControlSections.Owner);
		}
		set
		{
			SetAccessControlSectionsModified(AccessControlSections.Owner, value);
		}
	}

	protected ObjectSecurity()
	{
	}

	protected ObjectSecurity(CommonSecurityDescriptor securityDescriptor)
	{
		if (securityDescriptor == null)
		{
			throw new ArgumentNullException("securityDescriptor");
		}
		descriptor = securityDescriptor;
		rw_lock = new ReaderWriterLock();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.ObjectSecurity" /> class.</summary>
	/// <param name="isContainer">true if the new <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object is a container object.</param>
	/// <param name="isDS">True if the new <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object is a directory object.</param>
	protected ObjectSecurity(bool isContainer, bool isDS)
		: this(new CommonSecurityDescriptor(isContainer, isDS, ControlFlags.None, null, null, null, new DiscretionaryAcl(isContainer, isDS, 0)))
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.AccessRule" /> class with the specified values.</summary>
	/// <returns>The <see cref="T:System.Security.AccessControl.AccessRule" /> object that this method creates.</returns>
	/// <param name="identityReference">The identity to which the access rule applies.  It must be an object that can be cast as a <see cref="T:System.Security.Principal.SecurityIdentifier" />.</param>
	/// <param name="accessMask">The access mask of this rule. The access mask is a 32-bit collection of anonymous bits, the meaning of which is defined by the individual integrators.</param>
	/// <param name="isInherited">true if this rule is inherited from a parent container.</param>
	/// <param name="inheritanceFlags">Specifies the inheritance properties of the access rule.</param>
	/// <param name="propagationFlags">Specifies whether inherited access rules are automatically propagated. The propagation flags are ignored if <paramref name="inheritanceFlags" /> is set to <see cref="F:System.Security.AccessControl.InheritanceFlags.None" />.</param>
	/// <param name="type">Specifies the valid access control type.</param>
	public abstract AccessRule AccessRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type);

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.AuditRule" /> class with the specified values.</summary>
	/// <returns>The <see cref="T:System.Security.AccessControl.AuditRule" /> object that this method creates.</returns>
	/// <param name="identityReference">The identity to which the audit rule applies.  It must be an object that can be cast as a <see cref="T:System.Security.Principal.SecurityIdentifier" />.</param>
	/// <param name="accessMask">The access mask of this rule. The access mask is a 32-bit collection of anonymous bits, the meaning of which is defined by the individual integrators.</param>
	/// <param name="isInherited">true if this rule is inherited from a parent container.</param>
	/// <param name="inheritanceFlags">Specifies the inheritance properties of the audit rule.</param>
	/// <param name="propagationFlags">Specifies whether inherited audit rules are automatically propagated. The propagation flags are ignored if <paramref name="inheritanceFlags" /> is set to <see cref="F:System.Security.AccessControl.InheritanceFlags.None" />.</param>
	/// <param name="flags">Specifies the conditions for which the rule is audited.</param>
	public abstract AuditRule AuditRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags);

	/// <summary>Gets the primary group associated with the specified owner.</summary>
	/// <returns>The primary group associated with the specified owner.</returns>
	/// <param name="targetType">The owner for which to get the primary group. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	public IdentityReference GetGroup(Type targetType)
	{
		ReadLock();
		try
		{
			if (descriptor.Group == null)
			{
				return null;
			}
			return descriptor.Group.Translate(targetType);
		}
		finally
		{
			ReadUnlock();
		}
	}

	/// <summary>Gets the owner associated with the specified primary group.</summary>
	/// <returns>The owner associated with the specified group.</returns>
	/// <param name="targetType">The primary group for which to get the owner.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	public IdentityReference GetOwner(Type targetType)
	{
		ReadLock();
		try
		{
			if (descriptor.Owner == null)
			{
				return null;
			}
			return descriptor.Owner.Translate(targetType);
		}
		finally
		{
			ReadUnlock();
		}
	}

	/// <summary>Returns an array of byte values that represents the security descriptor information for this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object.</summary>
	/// <returns>An array of byte values that represents the security descriptor for this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object. This method returns null if there is no security information in this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object.</returns>
	public byte[] GetSecurityDescriptorBinaryForm()
	{
		ReadLock();
		try
		{
			byte[] array = new byte[descriptor.BinaryLength];
			descriptor.GetBinaryForm(array, 0);
			return array;
		}
		finally
		{
			ReadUnlock();
		}
	}

	/// <summary>Returns the Security Descriptor Definition Language (SDDL) representation of the specified sections of the security descriptor associated with this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object.</summary>
	/// <returns>The SDDL representation of the specified sections of the security descriptor associated with this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object.</returns>
	/// <param name="includeSections">Specifies which sections (access rules, audit rules, primary group, owner) of the security descriptor to get.</param>
	public string GetSecurityDescriptorSddlForm(AccessControlSections includeSections)
	{
		ReadLock();
		try
		{
			return descriptor.GetSddlForm(includeSections);
		}
		finally
		{
			ReadUnlock();
		}
	}

	/// <summary>Returns a Boolean value that specifies whether the security descriptor associated with this  <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object can be converted to the Security Descriptor Definition Language (SDDL) format.</summary>
	/// <returns>true if the security descriptor associated with this  <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object can be converted to the Security Descriptor Definition Language (SDDL) format; otherwise, false.</returns>
	public static bool IsSddlConversionSupported()
	{
		return GenericSecurityDescriptor.IsSddlConversionSupported();
	}

	/// <summary>Applies the specified modification to the Discretionary Access Control List (DACL) associated with this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object.</summary>
	/// <returns>true if the DACL is successfully modified; otherwise, false.</returns>
	/// <param name="modification">The modification to apply to the DACL.</param>
	/// <param name="rule">The access rule to modify.</param>
	/// <param name="modified">true if the DACL is successfully modified; otherwise, false.</param>
	public virtual bool ModifyAccessRule(AccessControlModification modification, AccessRule rule, out bool modified)
	{
		if (rule == null)
		{
			throw new ArgumentNullException("rule");
		}
		if (!AccessRuleType.IsAssignableFrom(rule.GetType()))
		{
			throw new ArgumentException("rule");
		}
		return ModifyAccess(modification, rule, out modified);
	}

	/// <summary>Applies the specified modification to the System Access Control List (SACL) associated with this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object.</summary>
	/// <returns>true if the SACL is successfully modified; otherwise, false.</returns>
	/// <param name="modification">The modification to apply to the SACL.</param>
	/// <param name="rule">The audit rule to modify.</param>
	/// <param name="modified">true if the SACL is successfully modified; otherwise, false.</param>
	public virtual bool ModifyAuditRule(AccessControlModification modification, AuditRule rule, out bool modified)
	{
		if (rule == null)
		{
			throw new ArgumentNullException("rule");
		}
		if (!AuditRuleType.IsAssignableFrom(rule.GetType()))
		{
			throw new ArgumentException("rule");
		}
		return ModifyAudit(modification, rule, out modified);
	}

	/// <summary>Removes all access rules associated with the specified <see cref="T:System.Security.Principal.IdentityReference" />.</summary>
	/// <param name="identity">The <see cref="T:System.Security.Principal.IdentityReference" /> for which to remove all access rules.</param>
	/// <exception cref="T:System.InvalidOperationException">All access rules are not in canonical order.</exception>
	public virtual void PurgeAccessRules(IdentityReference identity)
	{
		if (null == identity)
		{
			throw new ArgumentNullException("identity");
		}
		WriteLock();
		try
		{
			descriptor.PurgeAccessControl(SidFromIR(identity));
		}
		finally
		{
			WriteUnlock();
		}
	}

	/// <summary>Removes all audit rules associated with the specified <see cref="T:System.Security.Principal.IdentityReference" />.</summary>
	/// <param name="identity">The <see cref="T:System.Security.Principal.IdentityReference" /> for which to remove all audit rules.</param>
	/// <exception cref="T:System.InvalidOperationException">All audit rules are not in canonical order.</exception>
	public virtual void PurgeAuditRules(IdentityReference identity)
	{
		if (null == identity)
		{
			throw new ArgumentNullException("identity");
		}
		WriteLock();
		try
		{
			descriptor.PurgeAudit(SidFromIR(identity));
		}
		finally
		{
			WriteUnlock();
		}
	}

	/// <summary>Sets or removes protection of the access rules associated with this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object. Protected access rules cannot be modified by parent objects through inheritance.</summary>
	/// <param name="isProtected">true to protect the access rules associated with this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object from inheritance; false to allow inheritance.</param>
	/// <param name="preserveInheritance">true to preserve inherited access rules; false to remove inherited access rules. This parameter is ignored if <paramref name="isProtected" /> is false.</param>
	/// <exception cref="T:System.InvalidOperationException">This method attempts to remove inherited rules from a non-canonical Discretionary Access Control List (DACL).</exception>
	public void SetAccessRuleProtection(bool isProtected, bool preserveInheritance)
	{
		WriteLock();
		try
		{
			descriptor.SetDiscretionaryAclProtection(isProtected, preserveInheritance);
		}
		finally
		{
			WriteUnlock();
		}
	}

	/// <summary>Sets or removes protection of the audit rules associated with this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object. Protected audit rules cannot be modified by parent objects through inheritance.</summary>
	/// <param name="isProtected">true to protect the audit rules associated with this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object from inheritance; false to allow inheritance.</param>
	/// <param name="preserveInheritance">true to preserve inherited audit rules; false to remove inherited audit rules. This parameter is ignored if <paramref name="isProtected" /> is false.</param>
	/// <exception cref="T:System.InvalidOperationException">This method attempts to remove inherited rules from a non-canonical System Access Control List (SACL).</exception>
	public void SetAuditRuleProtection(bool isProtected, bool preserveInheritance)
	{
		WriteLock();
		try
		{
			descriptor.SetSystemAclProtection(isProtected, preserveInheritance);
		}
		finally
		{
			WriteUnlock();
		}
	}

	/// <summary>Sets the primary group for the security descriptor associated with this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object.</summary>
	/// <param name="identity">The primary group to set.</param>
	public void SetGroup(IdentityReference identity)
	{
		WriteLock();
		try
		{
			descriptor.Group = SidFromIR(identity);
			GroupModified = true;
		}
		finally
		{
			WriteUnlock();
		}
	}

	/// <summary>Sets the owner for the security descriptor associated with this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object.</summary>
	/// <param name="identity">The owner to set.</param>
	public void SetOwner(IdentityReference identity)
	{
		WriteLock();
		try
		{
			descriptor.Owner = SidFromIR(identity);
			OwnerModified = true;
		}
		finally
		{
			WriteUnlock();
		}
	}

	/// <summary>Sets the security descriptor for this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object from the specified array of byte values.</summary>
	/// <param name="binaryForm">The array of bytes from which to set the security descriptor.</param>
	public void SetSecurityDescriptorBinaryForm(byte[] binaryForm)
	{
		SetSecurityDescriptorBinaryForm(binaryForm, AccessControlSections.All);
	}

	/// <summary>Sets the specified sections of the security descriptor for this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object from the specified array of byte values.</summary>
	/// <param name="binaryForm">The array of bytes from which to set the security descriptor.</param>
	/// <param name="includeSections">The sections (access rules, audit rules, owner, primary group) of the security descriptor to set.</param>
	public void SetSecurityDescriptorBinaryForm(byte[] binaryForm, AccessControlSections includeSections)
	{
		CopySddlForm(new CommonSecurityDescriptor(IsContainer, IsDS, binaryForm, 0), includeSections);
	}

	/// <summary>Sets the security descriptor for this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object from the specified Security Descriptor Definition Language (SDDL) string.</summary>
	/// <param name="sddlForm">The SDDL string from which to set the security descriptor.</param>
	public void SetSecurityDescriptorSddlForm(string sddlForm)
	{
		SetSecurityDescriptorSddlForm(sddlForm, AccessControlSections.All);
	}

	/// <summary>Sets the specified sections of the security descriptor for this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object from the specified Security Descriptor Definition Language (SDDL) string.</summary>
	/// <param name="sddlForm">The SDDL string from which to set the security descriptor.</param>
	/// <param name="includeSections">The sections (access rules, audit rules, owner, primary group) of the security descriptor to set.</param>
	public void SetSecurityDescriptorSddlForm(string sddlForm, AccessControlSections includeSections)
	{
		CopySddlForm(new CommonSecurityDescriptor(IsContainer, IsDS, sddlForm), includeSections);
	}

	private void CopySddlForm(CommonSecurityDescriptor sourceDescriptor, AccessControlSections includeSections)
	{
		WriteLock();
		try
		{
			AccessControlSectionsModified |= includeSections;
			if ((includeSections & AccessControlSections.Audit) != 0)
			{
				descriptor.SystemAcl = sourceDescriptor.SystemAcl;
			}
			if ((includeSections & AccessControlSections.Access) != 0)
			{
				descriptor.DiscretionaryAcl = sourceDescriptor.DiscretionaryAcl;
			}
			if ((includeSections & AccessControlSections.Owner) != 0)
			{
				descriptor.Owner = sourceDescriptor.Owner;
			}
			if ((includeSections & AccessControlSections.Group) != 0)
			{
				descriptor.Group = sourceDescriptor.Group;
			}
		}
		finally
		{
			WriteUnlock();
		}
	}

	/// <summary>Applies the specified modification to the Discretionary Access Control List (DACL) associated with this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object.</summary>
	/// <returns>true if the DACL is successfully modified; otherwise, false.</returns>
	/// <param name="modification">The modification to apply to the DACL.</param>
	/// <param name="rule">The access rule to modify.</param>
	/// <param name="modified">true if the DACL is successfully modified; otherwise, false.</param>
	protected abstract bool ModifyAccess(AccessControlModification modification, AccessRule rule, out bool modified);

	/// <summary>Applies the specified modification to the System Access Control List (SACL) associated with this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object.</summary>
	/// <returns>true if the SACL is successfully modified; otherwise, false.</returns>
	/// <param name="modification">The modification to apply to the SACL.</param>
	/// <param name="rule">The audit rule to modify.</param>
	/// <param name="modified">true if the SACL is successfully modified; otherwise, false.</param>
	protected abstract bool ModifyAudit(AccessControlModification modification, AuditRule rule, out bool modified);

	private Exception GetNotImplementedException()
	{
		return new NotImplementedException();
	}

	/// <summary>Saves the specified sections of the security descriptor associated with this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object to permanent storage. We recommend that the values of the <paramref name="includeSections" /> parameters passed to the constructor and persist methods be identical. For more information, see Remarks.</summary>
	/// <param name="handle">The handle used to retrieve the persisted information.</param>
	/// <param name="includeSections">One of the <see cref="T:System.Security.AccessControl.AccessControlSections" /> enumeration values that specifies the sections of the security descriptor (access rules, audit rules, owner, primary group) of the securable object to save.</param>
	protected virtual void Persist(SafeHandle handle, AccessControlSections includeSections)
	{
		throw GetNotImplementedException();
	}

	/// <summary>Saves the specified sections of the security descriptor associated with this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object to permanent storage. We recommend that the values of the <paramref name="includeSections" /> parameters passed to the constructor and persist methods be identical. For more information, see Remarks.</summary>
	/// <param name="name">The name used to retrieve the persisted information.</param>
	/// <param name="includeSections">One of the <see cref="T:System.Security.AccessControl.AccessControlSections" /> enumeration values that specifies the sections of the security descriptor (access rules, audit rules, owner, primary group) of the securable object to save.</param>
	protected virtual void Persist(string name, AccessControlSections includeSections)
	{
		throw GetNotImplementedException();
	}

	/// <summary>Saves the specified sections of the security descriptor associated with this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object to permanent storage. We recommend that the values of the <paramref name="includeSections" /> parameters passed to the constructor and persist methods be identical. For more information, see Remarks.</summary>
	/// <param name="enableOwnershipPrivilege">true to enable the privilege that allows the caller to take ownership of the object.</param>
	/// <param name="name">The name used to retrieve the persisted information.</param>
	/// <param name="includeSections">One of the <see cref="T:System.Security.AccessControl.AccessControlSections" /> enumeration values that specifies the sections of the security descriptor (access rules, audit rules, owner, primary group) of the securable object to save.</param>
	[MonoTODO]
	[HandleProcessCorruptedStateExceptions]
	protected virtual void Persist(bool enableOwnershipPrivilege, string name, AccessControlSections includeSections)
	{
		throw new NotImplementedException();
	}

	private void Reading()
	{
		if (!rw_lock.IsReaderLockHeld && !rw_lock.IsWriterLockHeld)
		{
			throw new InvalidOperationException("Either a read or a write lock must be held.");
		}
	}

	/// <summary>Locks this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object for read access.</summary>
	protected void ReadLock()
	{
		rw_lock.AcquireReaderLock(-1);
	}

	/// <summary>Unlocks this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object for read access.</summary>
	protected void ReadUnlock()
	{
		rw_lock.ReleaseReaderLock();
	}

	private void Writing()
	{
		if (!rw_lock.IsWriterLockHeld)
		{
			throw new InvalidOperationException("Write lock must be held.");
		}
	}

	/// <summary>Locks this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object for write access.</summary>
	protected void WriteLock()
	{
		rw_lock.AcquireWriterLock(-1);
	}

	/// <summary>Unlocks this <see cref="T:System.Security.AccessControl.ObjectSecurity" /> object for write access.</summary>
	protected void WriteUnlock()
	{
		rw_lock.ReleaseWriterLock();
	}

	internal AuthorizationRuleCollection InternalGetAccessRules(bool includeExplicit, bool includeInherited, Type targetType)
	{
		List<AuthorizationRule> list = new List<AuthorizationRule>();
		ReadLock();
		try
		{
			AceEnumerator enumerator = descriptor.DiscretionaryAcl.GetEnumerator();
			while (enumerator.MoveNext())
			{
				QualifiedAce qualifiedAce = enumerator.Current as QualifiedAce;
				if (null == qualifiedAce || (qualifiedAce.IsInherited && !includeInherited) || (!qualifiedAce.IsInherited && !includeExplicit))
				{
					continue;
				}
				AccessControlType type;
				if (qualifiedAce.AceQualifier == AceQualifier.AccessAllowed)
				{
					type = AccessControlType.Allow;
				}
				else
				{
					if (AceQualifier.AccessDenied != qualifiedAce.AceQualifier)
					{
						continue;
					}
					type = AccessControlType.Deny;
				}
				AccessRule item = InternalAccessRuleFactory(qualifiedAce, targetType, type);
				list.Add(item);
			}
		}
		finally
		{
			ReadUnlock();
		}
		return new AuthorizationRuleCollection(list.ToArray());
	}

	internal virtual AccessRule InternalAccessRuleFactory(QualifiedAce ace, Type targetType, AccessControlType type)
	{
		return AccessRuleFactory(ace.SecurityIdentifier.Translate(targetType), ace.AccessMask, ace.IsInherited, ace.InheritanceFlags, ace.PropagationFlags, type);
	}

	internal AuthorizationRuleCollection InternalGetAuditRules(bool includeExplicit, bool includeInherited, Type targetType)
	{
		List<AuthorizationRule> list = new List<AuthorizationRule>();
		ReadLock();
		try
		{
			if (descriptor.SystemAcl != null)
			{
				AceEnumerator enumerator = descriptor.SystemAcl.GetEnumerator();
				while (enumerator.MoveNext())
				{
					QualifiedAce qualifiedAce = enumerator.Current as QualifiedAce;
					if (!(null == qualifiedAce) && (!qualifiedAce.IsInherited || includeInherited) && (qualifiedAce.IsInherited || includeExplicit) && AceQualifier.SystemAudit == qualifiedAce.AceQualifier)
					{
						AuditRule item = InternalAuditRuleFactory(qualifiedAce, targetType);
						list.Add(item);
					}
				}
			}
		}
		finally
		{
			ReadUnlock();
		}
		return new AuthorizationRuleCollection(list.ToArray());
	}

	internal virtual AuditRule InternalAuditRuleFactory(QualifiedAce ace, Type targetType)
	{
		return AuditRuleFactory(ace.SecurityIdentifier.Translate(targetType), ace.AccessMask, ace.IsInherited, ace.InheritanceFlags, ace.PropagationFlags, ace.AuditFlags);
	}

	internal static SecurityIdentifier SidFromIR(IdentityReference identity)
	{
		if (null == identity)
		{
			throw new ArgumentNullException("identity");
		}
		return (SecurityIdentifier)identity.Translate(typeof(SecurityIdentifier));
	}

	private bool AreAccessControlSectionsModified(AccessControlSections mask)
	{
		return (AccessControlSectionsModified & mask) != 0;
	}

	private void SetAccessControlSectionsModified(AccessControlSections mask, bool modified)
	{
		if (modified)
		{
			AccessControlSectionsModified |= mask;
		}
		else
		{
			AccessControlSectionsModified &= ~mask;
		}
	}
}
/// <summary>Provides the ability to control access to objects without direct manipulation of Access Control Lists (ACLs); also grants the ability to type-cast access rights. </summary>
/// <typeparam name="T">The access rights for the object.</typeparam>
public abstract class ObjectSecurity<T> : NativeObjectSecurity where T : struct
{
	/// <summary>Gets the Type of the securable object associated with this ObjectSecurity`1 object.</summary>
	/// <returns>The type of the securable object associated with the current instance.</returns>
	public override Type AccessRightType => typeof(T);

	/// <summary>Gets the Type of the object associated with the access rules of this ObjectSecurity`1 object. </summary>
	/// <returns>The Type of the object associated with the access rules of the current instance.</returns>
	public override Type AccessRuleType => typeof(AccessRule<T>);

	/// <summary>Gets the Type object associated with the audit rules of this ObjectSecurity`1 object.</summary>
	/// <returns>The Type object associated with the audit rules of the current instance.</returns>
	public override Type AuditRuleType => typeof(AuditRule<T>);

	/// <summary>Initializes a new instance of the ObjectSecurity`1 class.</summary>
	/// <param name="isContainer">true if the new <see cref="T:System.Security.AccessControl.ObjectSecurity`1" /> object is a container object.</param>
	/// <param name="resourceType">The type of resource.</param>
	protected ObjectSecurity(bool isContainer, ResourceType resourceType)
		: base(isContainer, resourceType)
	{
	}

	/// <summary>Initializes a new instance of the ObjectSecurity`1 class.</summary>
	/// <param name="isContainer">true if the new <see cref="T:System.Security.AccessControl.ObjectSecurity`1" /> object is a container object.</param>
	/// <param name="resourceType">The type of resource.</param>
	/// <param name="safeHandle">A handle.</param>
	/// <param name="includeSections">The sections to include.</param>
	protected ObjectSecurity(bool isContainer, ResourceType resourceType, SafeHandle safeHandle, AccessControlSections includeSections)
		: base(isContainer, resourceType, safeHandle, includeSections)
	{
	}

	/// <summary>Initializes a new instance of the ObjectSecurity`1 class.</summary>
	/// <param name="isContainer">true if the new <see cref="T:System.Security.AccessControl.ObjectSecurity`1" /> object is a container object.</param>
	/// <param name="resourceType">The type of resource.</param>
	/// <param name="name">The name of the securable object with which the new <see cref="T:System.Security.AccessControl.ObjectSecurity`1" /> object is associated.</param>
	/// <param name="includeSections">The sections to include.</param>
	protected ObjectSecurity(bool isContainer, ResourceType resourceType, string name, AccessControlSections includeSections)
		: base(isContainer, resourceType, name, includeSections)
	{
	}

	/// <summary>Initializes a new instance of the ObjectSecurity`1 class.</summary>
	/// <param name="isContainer">true if the new <see cref="T:System.Security.AccessControl.ObjectSecurity`1" /> object is a container object.</param>
	/// <param name="resourceType">The type of resource.</param>
	/// <param name="safeHandle">A handle.</param>
	/// <param name="includeSections">The sections to include.</param>
	/// <param name="exceptionFromErrorCode">A delegate implemented by integrators that provides custom exceptions.</param>
	/// <param name="exceptionContext">An object that contains contextual information about the source or destination of the exception.</param>
	protected ObjectSecurity(bool isContainer, ResourceType resourceType, SafeHandle safeHandle, AccessControlSections includeSections, ExceptionFromErrorCode exceptionFromErrorCode, object exceptionContext)
		: base(isContainer, resourceType, safeHandle, includeSections, exceptionFromErrorCode, exceptionContext)
	{
	}

	/// <summary>Initializes a new instance of the ObjectSecurity`1 class.</summary>
	/// <param name="isContainer">true if the new <see cref="T:System.Security.AccessControl.ObjectSecurity`1" /> object is a container object.</param>
	/// <param name="resourceType">The type of resource.</param>
	/// <param name="name">The name of the securable object with which the new <see cref="T:System.Security.AccessControl.ObjectSecurity`1" /> object is associated.</param>
	/// <param name="includeSections">The sections to include. </param>
	/// <param name="exceptionFromErrorCode">A delegate implemented by integrators that provides custom exceptions.</param>
	/// <param name="exceptionContext">An object that contains contextual information about the source or destination of the exception.</param>
	protected ObjectSecurity(bool isContainer, ResourceType resourceType, string name, AccessControlSections includeSections, ExceptionFromErrorCode exceptionFromErrorCode, object exceptionContext)
		: base(isContainer, resourceType, name, includeSections, exceptionFromErrorCode, exceptionContext)
	{
	}

	/// <summary>Initializes a new instance of the ObjectAccessRule class that represents a new access control rule for the associated security object.</summary>
	/// <returns>Represents a new access control rule for the specified user, with the specified access rights, access control, and flags.</returns>
	/// <param name="identityReference">Represents a user account.</param>
	/// <param name="accessMask">The access type.</param>
	/// <param name="isInherited">true if the access rule is inherited; otherwise, false.</param>
	/// <param name="inheritanceFlags">Specifies how to propagate access masks to child objects.</param>
	/// <param name="propagationFlags">Specifies how to propagate Access Control Entries (ACEs) to child objects.</param>
	/// <param name="type">Specifies whether access is allowed or denied.</param>
	public override AccessRule AccessRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
	{
		return new AccessRule<T>(identityReference, accessMask, isInherited, inheritanceFlags, propagationFlags, type);
	}

	/// <summary>Adds the specified access rule to the Discretionary Access Control List (DACL) associated with this ObjectSecurity`1 object.</summary>
	/// <param name="rule">The rule to add.</param>
	public virtual void AddAccessRule(AccessRule<T> rule)
	{
		AddAccessRule((AccessRule)rule);
	}

	/// <summary>Removes access rules that contain the same security identifier and access mask as the specified access rule from the Discretionary Access Control List (DACL) associated with this ObjectSecurity`1 object.</summary>
	/// <returns>Returns true if the access rule was successfully removed; otherwise, false.</returns>
	/// <param name="rule">The rule to remove.</param>
	public virtual bool RemoveAccessRule(AccessRule<T> rule)
	{
		return RemoveAccessRule((AccessRule)rule);
	}

	/// <summary>Removes all access rules that have the same security identifier as the specified access rule from the Discretionary Access Control List (DACL) associated with this ObjectSecurity`1 object.</summary>
	/// <param name="rule">The access rule to remove.</param>
	public virtual void RemoveAccessRuleAll(AccessRule<T> rule)
	{
		RemoveAccessRuleAll((AccessRule)rule);
	}

	/// <summary>Removes all access rules that exactly match the specified access rule from the Discretionary Access Control List (DACL) associated with this ObjectSecurity`1 object</summary>
	/// <param name="rule">The access rule to remove.</param>
	public virtual void RemoveAccessRuleSpecific(AccessRule<T> rule)
	{
		RemoveAccessRuleSpecific((AccessRule)rule);
	}

	/// <summary>Removes all access rules in the Discretionary Access Control List (DACL) associated with this ObjectSecurity`1 object and then adds the specified access rule.</summary>
	/// <param name="rule">The access rule to reset.</param>
	public virtual void ResetAccessRule(AccessRule<T> rule)
	{
		ResetAccessRule((AccessRule)rule);
	}

	/// <summary>Removes all access rules that contain the same security identifier and qualifier as the specified access rule in the Discretionary Access Control List (DACL) associated with this ObjectSecurity`1 object and then adds the specified access rule.</summary>
	/// <param name="rule">The access rule to set.</param>
	public virtual void SetAccessRule(AccessRule<T> rule)
	{
		SetAccessRule((AccessRule)rule);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.AuditRule" /> class representing the specified audit rule for the specified user.</summary>
	/// <returns>Returns the specified audit rule for the specified user.</returns>
	/// <param name="identityReference">Represents a user account. </param>
	/// <param name="accessMask">An integer that specifies an access type.</param>
	/// <param name="isInherited">true if the access rule is inherited; otherwise, false.</param>
	/// <param name="inheritanceFlags">Specifies how to propagate access masks to child objects.</param>
	/// <param name="propagationFlags">Specifies how to propagate Access Control Entries (ACEs) to child objects.</param>
	/// <param name="flags">Describes the type of auditing to perform.</param>
	public override AuditRule AuditRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
	{
		return new AuditRule<T>(identityReference, accessMask, isInherited, inheritanceFlags, propagationFlags, flags);
	}

	/// <summary>Adds the specified audit rule to the System Access Control List (SACL) associated with this ObjectSecurity`1 object.</summary>
	/// <param name="rule">The audit rule to add.</param>
	public virtual void AddAuditRule(AuditRule<T> rule)
	{
		AddAuditRule((AuditRule)rule);
	}

	/// <summary>Removes audit rules that contain the same security identifier and access mask as the specified audit rule from the System Access Control List (SACL) associated with this ObjectSecurity`1 object.</summary>
	/// <returns>Returns true if the object was removed; otherwise, false.</returns>
	/// <param name="rule">The audit rule to remove</param>
	public virtual bool RemoveAuditRule(AuditRule<T> rule)
	{
		return RemoveAuditRule((AuditRule)rule);
	}

	/// <summary>Removes all audit rules that have the same security identifier as the specified audit rule from the System Access Control List (SACL) associated with this ObjectSecurity`1 object.</summary>
	/// <param name="rule">The audit rule to remove.</param>
	public virtual void RemoveAuditRuleAll(AuditRule<T> rule)
	{
		RemoveAuditRuleAll((AuditRule)rule);
	}

	/// <summary>Removes all audit rules that exactly match the specified audit rule from the System Access Control List (SACL) associated with this ObjectSecurity`1 object</summary>
	/// <param name="rule">The audit rule to remove.</param>
	public virtual void RemoveAuditRuleSpecific(AuditRule<T> rule)
	{
		RemoveAuditRuleSpecific((AuditRule)rule);
	}

	/// <summary>Removes all audit rules that contain the same security identifier and qualifier as the specified audit rule in the System Access Control List (SACL) associated with this ObjectSecurity`1 object and then adds the specified audit rule.</summary>
	/// <param name="rule">The audit rule to set.</param>
	public virtual void SetAuditRule(AuditRule<T> rule)
	{
		SetAuditRule((AuditRule)rule);
	}

	/// <summary>Saves the security descriptor associated with this ObjectSecurity`1 object to permanent storage, using the specified handle.</summary>
	/// <param name="handle">The handle of the securable object with which this ObjectSecurity`1 object is associated.</param>
	protected void Persist(SafeHandle handle)
	{
		WriteLock();
		try
		{
			Persist(handle, base.AccessControlSectionsModified);
		}
		finally
		{
			WriteUnlock();
		}
	}

	/// <summary>Saves the security descriptor associated with this ObjectSecurity`1 object to permanent storage, using the specified name.</summary>
	/// <param name="name">The name of the securable object with which this ObjectSecurity`1 object is associated.</param>
	protected void Persist(string name)
	{
		WriteLock();
		try
		{
			Persist(name, base.AccessControlSectionsModified);
		}
		finally
		{
			WriteUnlock();
		}
	}
}
