using System.Security.Principal;

namespace System.Security.AccessControl;

/// <summary>Represents a security descriptor. A security descriptor includes an owner, a primary group, a Discretionary Access Control List (DACL), and a System Access Control List (SACL).</summary>
public sealed class CommonSecurityDescriptor : GenericSecurityDescriptor
{
	private bool is_container;

	private bool is_ds;

	private ControlFlags flags;

	private SecurityIdentifier owner;

	private SecurityIdentifier group;

	private SystemAcl system_acl;

	private DiscretionaryAcl discretionary_acl;

	/// <summary>Gets values that specify behavior of the <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object.</summary>
	/// <returns>One or more values of the <see cref="T:System.Security.AccessControl.ControlFlags" /> enumeration combined with a logical OR operation.</returns>
	public override ControlFlags ControlFlags
	{
		get
		{
			ControlFlags controlFlags = flags;
			controlFlags |= ControlFlags.DiscretionaryAclPresent;
			controlFlags |= ControlFlags.SelfRelative;
			if (SystemAcl != null)
			{
				controlFlags |= ControlFlags.SystemAclPresent;
			}
			return controlFlags;
		}
	}

	/// <summary>Gets or sets the discretionary access control list (DACL) for this <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object. The DACL contains access rules.</summary>
	/// <returns>The DACL for this <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object.</returns>
	public DiscretionaryAcl DiscretionaryAcl
	{
		get
		{
			return discretionary_acl;
		}
		set
		{
			if (value == null)
			{
				value = new DiscretionaryAcl(IsContainer, IsDS, 1);
				value.AddAccess(AccessControlType.Allow, new SecurityIdentifier("WD"), -1, IsContainer ? (InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit) : InheritanceFlags.None, PropagationFlags.None);
				value.IsAefa = true;
			}
			CheckAclConsistency(value);
			discretionary_acl = value;
		}
	}

	internal override GenericAcl InternalDacl => DiscretionaryAcl;

	/// <summary>Gets or sets the primary group for this <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object.</summary>
	/// <returns>The primary group for this <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object.</returns>
	public override SecurityIdentifier Group
	{
		get
		{
			return group;
		}
		set
		{
			group = value;
		}
	}

	/// <summary>Gets a Boolean value that specifies whether the object associated with this <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object is a container object.</summary>
	/// <returns>true if the object associated with this <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object is a container object; otherwise, false.</returns>
	public bool IsContainer => is_container;

	/// <summary>Gets a Boolean value that specifies whether the Discretionary Access Control List (DACL) associated with this <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object is in canonical order.</summary>
	/// <returns>true if the DACL associated with this <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object is in canonical order; otherwise, false.</returns>
	public bool IsDiscretionaryAclCanonical => DiscretionaryAcl.IsCanonical;

	/// <summary>Gets a Boolean value that specifies whether the object associated with this <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object is a directory object.</summary>
	/// <returns>true if the object associated with this <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object is a directory object; otherwise, false.</returns>
	public bool IsDS => is_ds;

	/// <summary>Gets a Boolean value that specifies whether the System Access Control List (SACL) associated with this <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object is in canonical order.</summary>
	/// <returns>true if the SACL associated with this <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object is in canonical order; otherwise, false.</returns>
	public bool IsSystemAclCanonical
	{
		get
		{
			if (SystemAcl != null)
			{
				return SystemAcl.IsCanonical;
			}
			return true;
		}
	}

	/// <summary>Gets or sets the owner of the object associated with this <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object.</summary>
	/// <returns>The owner of the object associated with this <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object.</returns>
	public override SecurityIdentifier Owner
	{
		get
		{
			return owner;
		}
		set
		{
			owner = value;
		}
	}

	/// <summary>Gets or sets the System Access Control List (SACL) for this <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object. The SACL contains audit rules.</summary>
	/// <returns>The SACL for this <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object.</returns>
	public SystemAcl SystemAcl
	{
		get
		{
			return system_acl;
		}
		set
		{
			if (value != null)
			{
				CheckAclConsistency(value);
			}
			system_acl = value;
		}
	}

	internal override GenericAcl InternalSacl => SystemAcl;

	internal override bool DaclIsUnmodifiedAefa => DiscretionaryAcl.IsAefa;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> class from the specified <see cref="T:System.Security.AccessControl.RawSecurityDescriptor" /> object.</summary>
	/// <param name="isContainer">true if the new security descriptor is associated with a container object.</param>
	/// <param name="isDS">true if the new security descriptor is associated with a directory object.</param>
	/// <param name="rawSecurityDescriptor">The <see cref="T:System.Security.AccessControl.RawSecurityDescriptor" /> object from which to create the new <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object.</param>
	public CommonSecurityDescriptor(bool isContainer, bool isDS, RawSecurityDescriptor rawSecurityDescriptor)
	{
		Init(isContainer, isDS, rawSecurityDescriptor);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> class from the specified Security Descriptor Definition Language (SDDL) string.</summary>
	/// <param name="isContainer">true if the new security descriptor is associated with a container object.</param>
	/// <param name="isDS">true if the new security descriptor is associated with a directory object.</param>
	/// <param name="sddlForm">The SDDL string from which to create the new <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object.</param>
	public CommonSecurityDescriptor(bool isContainer, bool isDS, string sddlForm)
	{
		Init(isContainer, isDS, new RawSecurityDescriptor(sddlForm));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> class from the specified array of byte values.</summary>
	/// <param name="isContainer">true if the new security descriptor is associated with a container object.</param>
	/// <param name="isDS">true if the new security descriptor is associated with a directory object.</param>
	/// <param name="binaryForm">The array of byte values from which to create the new <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object.</param>
	/// <param name="offset">The offset in the <paramref name="binaryForm" /> array at which to begin copying.</param>
	public CommonSecurityDescriptor(bool isContainer, bool isDS, byte[] binaryForm, int offset)
	{
		Init(isContainer, isDS, new RawSecurityDescriptor(binaryForm, offset));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> class from the specified information.</summary>
	/// <param name="isContainer">true if the new security descriptor is associated with a container object.</param>
	/// <param name="isDS">true if the new security descriptor is associated with a directory object.</param>
	/// <param name="flags">Flags that specify behavior of the new <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object.</param>
	/// <param name="owner">The owner for the new <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object.</param>
	/// <param name="group">The primary group for the new <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object.</param>
	/// <param name="systemAcl">The System Access Control List (SACL) for the new <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object.</param>
	/// <param name="discretionaryAcl">The Discretionary Access Control List (DACL) for the new <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object.</param>
	public CommonSecurityDescriptor(bool isContainer, bool isDS, ControlFlags flags, SecurityIdentifier owner, SecurityIdentifier group, SystemAcl systemAcl, DiscretionaryAcl discretionaryAcl)
	{
		Init(isContainer, isDS, flags, owner, group, systemAcl, discretionaryAcl);
	}

	private void Init(bool isContainer, bool isDS, RawSecurityDescriptor rawSecurityDescriptor)
	{
		if (rawSecurityDescriptor == null)
		{
			throw new ArgumentNullException("rawSecurityDescriptor");
		}
		SystemAcl systemAcl = null;
		if (rawSecurityDescriptor.SystemAcl != null)
		{
			systemAcl = new SystemAcl(isContainer, isDS, rawSecurityDescriptor.SystemAcl);
		}
		DiscretionaryAcl discretionaryAcl = null;
		if (rawSecurityDescriptor.DiscretionaryAcl != null)
		{
			discretionaryAcl = new DiscretionaryAcl(isContainer, isDS, rawSecurityDescriptor.DiscretionaryAcl);
		}
		Init(isContainer, isDS, rawSecurityDescriptor.ControlFlags, rawSecurityDescriptor.Owner, rawSecurityDescriptor.Group, systemAcl, discretionaryAcl);
	}

	private void Init(bool isContainer, bool isDS, ControlFlags flags, SecurityIdentifier owner, SecurityIdentifier group, SystemAcl systemAcl, DiscretionaryAcl discretionaryAcl)
	{
		this.flags = flags & ~ControlFlags.SystemAclPresent;
		is_container = isContainer;
		is_ds = isDS;
		Owner = owner;
		Group = group;
		SystemAcl = systemAcl;
		DiscretionaryAcl = discretionaryAcl;
	}

	/// <summary>Removes all access rules for the specified security identifier from the Discretionary Access Control List (DACL) associated with this <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object.</summary>
	/// <param name="sid">The security identifier for which to remove access rules.</param>
	public void PurgeAccessControl(SecurityIdentifier sid)
	{
		DiscretionaryAcl.Purge(sid);
	}

	/// <summary>Removes all audit rules for the specified security identifier from the System Access Control List (SACL) associated with this <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object.</summary>
	/// <param name="sid">The security identifier for which to remove audit rules.</param>
	public void PurgeAudit(SecurityIdentifier sid)
	{
		if (SystemAcl != null)
		{
			SystemAcl.Purge(sid);
		}
	}

	/// <summary>Sets the inheritance protection for the Discretionary Access Control List (DACL) associated with this <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object. DACLs that are protected do not inherit access rules from parent containers.</summary>
	/// <param name="isProtected">true to protect the DACL from inheritance.</param>
	/// <param name="preserveInheritance">true to keep inherited access rules in the DACL; false to remove inherited access rules from the DACL.</param>
	public void SetDiscretionaryAclProtection(bool isProtected, bool preserveInheritance)
	{
		DiscretionaryAcl.IsAefa = false;
		if (!isProtected)
		{
			flags &= ~ControlFlags.DiscretionaryAclProtected;
			return;
		}
		flags |= ControlFlags.DiscretionaryAclProtected;
		if (!preserveInheritance)
		{
			DiscretionaryAcl.RemoveInheritedAces();
		}
	}

	/// <summary>Sets the inheritance protection for the System Access Control List (SACL) associated with this <see cref="T:System.Security.AccessControl.CommonSecurityDescriptor" /> object. SACLs that are protected do not inherit audit rules from parent containers.</summary>
	/// <param name="isProtected">true to protect the SACL from inheritance.</param>
	/// <param name="preserveInheritance">true to keep inherited audit rules in the SACL; false to remove inherited audit rules from the SACL.</param>
	public void SetSystemAclProtection(bool isProtected, bool preserveInheritance)
	{
		if (!isProtected)
		{
			flags &= ~ControlFlags.SystemAclProtected;
			return;
		}
		flags |= ControlFlags.SystemAclProtected;
		if (!preserveInheritance && SystemAcl != null)
		{
			SystemAcl.RemoveInheritedAces();
		}
	}

	public void AddDiscretionaryAcl(byte revision, int trusted)
	{
		DiscretionaryAcl = new DiscretionaryAcl(IsContainer, IsDS, revision, trusted);
		flags |= ControlFlags.DiscretionaryAclPresent;
	}

	public void AddSystemAcl(byte revision, int trusted)
	{
		SystemAcl = new SystemAcl(IsContainer, IsDS, revision, trusted);
		flags |= ControlFlags.SystemAclPresent;
	}

	private void CheckAclConsistency(CommonAcl acl)
	{
		if (IsContainer != acl.IsContainer)
		{
			throw new ArgumentException("IsContainer must match between descriptor and ACL.");
		}
		if (IsDS != acl.IsDS)
		{
			throw new ArgumentException("IsDS must match between descriptor and ACL.");
		}
	}
}
