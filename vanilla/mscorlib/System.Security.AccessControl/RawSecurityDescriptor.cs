using System.Security.Principal;

namespace System.Security.AccessControl;

/// <summary>Represents a security descriptor. A security descriptor includes an owner, a primary group, a Discretionary Access Control List (DACL), and a System Access Control List (SACL).</summary>
public sealed class RawSecurityDescriptor : GenericSecurityDescriptor
{
	private ControlFlags control_flags;

	private SecurityIdentifier owner_sid;

	private SecurityIdentifier group_sid;

	private RawAcl system_acl;

	private RawAcl discretionary_acl;

	private byte resourcemgr_control;

	/// <summary>Gets values that specify behavior of the <see cref="T:System.Security.AccessControl.RawSecurityDescriptor" /> object.</summary>
	/// <returns>One or more values of the <see cref="T:System.Security.AccessControl.ControlFlags" /> enumeration combined with a logical OR operation.</returns>
	public override ControlFlags ControlFlags => control_flags;

	/// <summary>Gets or sets the Discretionary Access Control List (DACL) for this <see cref="T:System.Security.AccessControl.RawSecurityDescriptor" /> object. The DACL contains access rules.</summary>
	/// <returns>The DACL for this <see cref="T:System.Security.AccessControl.RawSecurityDescriptor" /> object.</returns>
	public RawAcl DiscretionaryAcl
	{
		get
		{
			return discretionary_acl;
		}
		set
		{
			discretionary_acl = value;
		}
	}

	/// <summary>Gets or sets the primary group for this <see cref="T:System.Security.AccessControl.RawSecurityDescriptor" /> object.</summary>
	/// <returns>The primary group for this <see cref="T:System.Security.AccessControl.RawSecurityDescriptor" /> object.</returns>
	public override SecurityIdentifier Group
	{
		get
		{
			return group_sid;
		}
		set
		{
			group_sid = value;
		}
	}

	/// <summary>Gets or sets the owner of the object associated with this <see cref="T:System.Security.AccessControl.RawSecurityDescriptor" /> object.</summary>
	/// <returns>The owner of the object associated with this <see cref="T:System.Security.AccessControl.RawSecurityDescriptor" /> object.</returns>
	public override SecurityIdentifier Owner
	{
		get
		{
			return owner_sid;
		}
		set
		{
			owner_sid = value;
		}
	}

	/// <summary>Gets or sets a byte value that represents the resource manager control bits associated with this <see cref="T:System.Security.AccessControl.RawSecurityDescriptor" /> object.</summary>
	/// <returns>A byte value that represents the resource manager control bits associated with this <see cref="T:System.Security.AccessControl.RawSecurityDescriptor" /> object.</returns>
	public byte ResourceManagerControl
	{
		get
		{
			return resourcemgr_control;
		}
		set
		{
			resourcemgr_control = value;
		}
	}

	/// <summary>Gets or sets the System Access Control List (SACL) for this <see cref="T:System.Security.AccessControl.RawSecurityDescriptor" /> object. The SACL contains audit rules.</summary>
	/// <returns>The SACL for this <see cref="T:System.Security.AccessControl.RawSecurityDescriptor" /> object.</returns>
	public RawAcl SystemAcl
	{
		get
		{
			return system_acl;
		}
		set
		{
			system_acl = value;
		}
	}

	internal override GenericAcl InternalDacl => DiscretionaryAcl;

	internal override GenericAcl InternalSacl => SystemAcl;

	internal override byte InternalReservedField => ResourceManagerControl;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.RawSecurityDescriptor" /> class from the specified Security Descriptor Definition Language (SDDL) string.</summary>
	/// <param name="sddlForm">The SDDL string from which to create the new <see cref="T:System.Security.AccessControl.RawSecurityDescriptor" /> object.</param>
	public RawSecurityDescriptor(string sddlForm)
	{
		if (sddlForm == null)
		{
			throw new ArgumentNullException("sddlForm");
		}
		ParseSddl(sddlForm.Replace(" ", ""));
		control_flags |= ControlFlags.SelfRelative;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.RawSecurityDescriptor" /> class from the specified array of byte values.</summary>
	/// <param name="binaryForm">The array of byte values from which to create the new <see cref="T:System.Security.AccessControl.RawSecurityDescriptor" /> object.</param>
	/// <param name="offset">The offset in the  <paramref name="binaryForm" /> array at which to begin copying.</param>
	public RawSecurityDescriptor(byte[] binaryForm, int offset)
	{
		if (binaryForm == null)
		{
			throw new ArgumentNullException("binaryForm");
		}
		if (offset < 0 || offset > binaryForm.Length - 20)
		{
			throw new ArgumentOutOfRangeException("offset", offset, "Offset out of range");
		}
		if (binaryForm[offset] != 1)
		{
			throw new ArgumentException("Unrecognized Security Descriptor revision.", "binaryForm");
		}
		resourcemgr_control = binaryForm[offset + 1];
		control_flags = (ControlFlags)ReadUShort(binaryForm, offset + 2);
		int num = ReadInt(binaryForm, offset + 4);
		int num2 = ReadInt(binaryForm, offset + 8);
		int num3 = ReadInt(binaryForm, offset + 12);
		int num4 = ReadInt(binaryForm, offset + 16);
		if (num != 0)
		{
			owner_sid = new SecurityIdentifier(binaryForm, num);
		}
		if (num2 != 0)
		{
			group_sid = new SecurityIdentifier(binaryForm, num2);
		}
		if (num3 != 0)
		{
			system_acl = new RawAcl(binaryForm, num3);
		}
		if (num4 != 0)
		{
			discretionary_acl = new RawAcl(binaryForm, num4);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.RawSecurityDescriptor" /> class with the specified values.</summary>
	/// <param name="flags">Flags that specify behavior of the new <see cref="T:System.Security.AccessControl.RawSecurityDescriptor" /> object.</param>
	/// <param name="owner">The owner for the new <see cref="T:System.Security.AccessControl.RawSecurityDescriptor" /> object.</param>
	/// <param name="group">The primary group for the new <see cref="T:System.Security.AccessControl.RawSecurityDescriptor" /> object.</param>
	/// <param name="systemAcl">The System Access Control List (SACL) for the new <see cref="T:System.Security.AccessControl.RawSecurityDescriptor" /> object.</param>
	/// <param name="discretionaryAcl">The Discretionary Access Control List (DACL) for the new <see cref="T:System.Security.AccessControl.RawSecurityDescriptor" /> object.</param>
	public RawSecurityDescriptor(ControlFlags flags, SecurityIdentifier owner, SecurityIdentifier group, RawAcl systemAcl, RawAcl discretionaryAcl)
	{
		control_flags = flags;
		owner_sid = owner;
		group_sid = group;
		system_acl = systemAcl;
		discretionary_acl = discretionaryAcl;
	}

	/// <summary>Sets the <see cref="P:System.Security.AccessControl.RawSecurityDescriptor.ControlFlags" /> property of this <see cref="T:System.Security.AccessControl.RawSecurityDescriptor" /> object to the specified value.</summary>
	/// <param name="flags">One or more values of the <see cref="T:System.Security.AccessControl.ControlFlags" /> enumeration combined with a logical OR operation.</param>
	public void SetFlags(ControlFlags flags)
	{
		control_flags = flags | ControlFlags.SelfRelative;
	}

	private void ParseSddl(string sddlForm)
	{
		ControlFlags sdFlags = ControlFlags.None;
		int pos = 0;
		while (pos < sddlForm.Length - 2)
		{
			switch (sddlForm.Substring(pos, 2))
			{
			case "O:":
				pos += 2;
				Owner = SecurityIdentifier.ParseSddlForm(sddlForm, ref pos);
				break;
			case "G:":
				pos += 2;
				Group = SecurityIdentifier.ParseSddlForm(sddlForm, ref pos);
				break;
			case "D:":
				pos += 2;
				DiscretionaryAcl = RawAcl.ParseSddlForm(sddlForm, isDacl: true, ref sdFlags, ref pos);
				sdFlags |= ControlFlags.DiscretionaryAclPresent;
				break;
			case "S:":
				pos += 2;
				SystemAcl = RawAcl.ParseSddlForm(sddlForm, isDacl: false, ref sdFlags, ref pos);
				sdFlags |= ControlFlags.SystemAclPresent;
				break;
			default:
				throw new ArgumentException("Invalid SDDL.", "sddlForm");
			}
		}
		if (pos != sddlForm.Length)
		{
			throw new ArgumentException("Invalid SDDL.", "sddlForm");
		}
		SetFlags(sdFlags);
	}

	private ushort ReadUShort(byte[] buffer, int offset)
	{
		return (ushort)(buffer[offset] | (buffer[offset + 1] << 8));
	}

	private int ReadInt(byte[] buffer, int offset)
	{
		return buffer[offset] | (buffer[offset + 1] << 8) | (buffer[offset + 2] << 16) | (buffer[offset + 3] << 24);
	}
}
