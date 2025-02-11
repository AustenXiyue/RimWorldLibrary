using System.Globalization;
using System.Security.Principal;
using System.Text;

namespace System.Security.AccessControl;

/// <summary>Represents a security descriptor. A security descriptor includes an owner, a primary group, a Discretionary Access Control List (DACL), and a System Access Control List (SACL).</summary>
public abstract class GenericSecurityDescriptor
{
	/// <summary>Gets the length, in bytes, of the binary representation of the current <see cref="T:System.Security.AccessControl.GenericSecurityDescriptor" /> object. This length should be used before marshaling the ACL into a binary array with the <see cref="M:System.Security.AccessControl.GenericSecurityDescriptor.GetBinaryForm" /> method.</summary>
	/// <returns>The length, in bytes, of the binary representation of the current <see cref="T:System.Security.AccessControl.GenericSecurityDescriptor" /> object.</returns>
	public int BinaryLength
	{
		get
		{
			int num = 20;
			if (Owner != null)
			{
				num += Owner.BinaryLength;
			}
			if (Group != null)
			{
				num += Group.BinaryLength;
			}
			if (DaclPresent && !DaclIsUnmodifiedAefa)
			{
				num += InternalDacl.BinaryLength;
			}
			if (SaclPresent)
			{
				num += InternalSacl.BinaryLength;
			}
			return num;
		}
	}

	/// <summary>Gets values that specify behavior of the <see cref="T:System.Security.AccessControl.GenericSecurityDescriptor" /> object.</summary>
	/// <returns>One or more values of the <see cref="T:System.Security.AccessControl.ControlFlags" /> enumeration combined with a logical OR operation.</returns>
	public abstract ControlFlags ControlFlags { get; }

	/// <summary>Gets or sets the primary group for this <see cref="T:System.Security.AccessControl.GenericSecurityDescriptor" /> object.</summary>
	/// <returns>The primary group for this <see cref="T:System.Security.AccessControl.GenericSecurityDescriptor" /> object.</returns>
	public abstract SecurityIdentifier Group { get; set; }

	/// <summary>Gets or sets the owner of the object associated with this <see cref="T:System.Security.AccessControl.GenericSecurityDescriptor" /> object.</summary>
	/// <returns>The owner of the object associated with this <see cref="T:System.Security.AccessControl.GenericSecurityDescriptor" /> object.</returns>
	public abstract SecurityIdentifier Owner { get; set; }

	/// <summary>Gets the revision level of the <see cref="T:System.Security.AccessControl.GenericSecurityDescriptor" /> object.</summary>
	/// <returns>A byte value that specifies the revision level of the <see cref="T:System.Security.AccessControl.GenericSecurityDescriptor" />.</returns>
	public static byte Revision => 1;

	internal virtual GenericAcl InternalDacl => null;

	internal virtual GenericAcl InternalSacl => null;

	internal virtual byte InternalReservedField => 0;

	internal virtual bool DaclIsUnmodifiedAefa => false;

	private bool DaclPresent
	{
		get
		{
			if (InternalDacl != null)
			{
				return (ControlFlags & ControlFlags.DiscretionaryAclPresent) != 0;
			}
			return false;
		}
	}

	private bool SaclPresent
	{
		get
		{
			if (InternalSacl != null)
			{
				return (ControlFlags & ControlFlags.SystemAclPresent) != 0;
			}
			return false;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.GenericSecurity" /> class.</summary>
	protected GenericSecurityDescriptor()
	{
	}

	/// <summary>Returns an array of byte values that represents the information contained in this <see cref="T:System.Security.AccessControl.GenericSecurityDescriptor" /> object.</summary>
	/// <param name="binaryForm">The byte array into which the contents of the <see cref="T:System.Security.AccessControl.GenericSecurityDescriptor" /> is marshaled.</param>
	/// <param name="offset">The offset at which to start marshaling.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is negative or too high to allow the entire <see cref="T:System.Security.AccessControl.GenericSecurityDescriptor" /> to be copied into <paramref name="array" />.</exception>
	public void GetBinaryForm(byte[] binaryForm, int offset)
	{
		if (binaryForm == null)
		{
			throw new ArgumentNullException("binaryForm");
		}
		int binaryLength = BinaryLength;
		if (offset < 0 || offset > binaryForm.Length - binaryLength)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		ControlFlags controlFlags = ControlFlags;
		if (DaclIsUnmodifiedAefa)
		{
			controlFlags &= ~ControlFlags.DiscretionaryAclPresent;
		}
		binaryForm[offset] = Revision;
		binaryForm[offset + 1] = InternalReservedField;
		WriteUShort((ushort)controlFlags, binaryForm, offset + 2);
		int num = 20;
		if (Owner != null)
		{
			WriteInt(num, binaryForm, offset + 4);
			Owner.GetBinaryForm(binaryForm, offset + num);
			num += Owner.BinaryLength;
		}
		else
		{
			WriteInt(0, binaryForm, offset + 4);
		}
		if (Group != null)
		{
			WriteInt(num, binaryForm, offset + 8);
			Group.GetBinaryForm(binaryForm, offset + num);
			num += Group.BinaryLength;
		}
		else
		{
			WriteInt(0, binaryForm, offset + 8);
		}
		GenericAcl internalSacl = InternalSacl;
		if (SaclPresent)
		{
			WriteInt(num, binaryForm, offset + 12);
			internalSacl.GetBinaryForm(binaryForm, offset + num);
			num += InternalSacl.BinaryLength;
		}
		else
		{
			WriteInt(0, binaryForm, offset + 12);
		}
		GenericAcl internalDacl = InternalDacl;
		if (DaclPresent && !DaclIsUnmodifiedAefa)
		{
			WriteInt(num, binaryForm, offset + 16);
			internalDacl.GetBinaryForm(binaryForm, offset + num);
			num += InternalDacl.BinaryLength;
		}
		else
		{
			WriteInt(0, binaryForm, offset + 16);
		}
	}

	/// <summary>Returns the Security Descriptor Definition Language (SDDL) representation of the specified sections of the security descriptor that this <see cref="T:System.Security.AccessControl.GenericSecurityDescriptor" /> object represents.</summary>
	/// <returns>The SDDL representation of the specified sections of the security descriptor associated with this <see cref="T:System.Security.AccessControl.GenericSecurityDescriptor" /> object.</returns>
	/// <param name="includeSections">Specifies which sections (access rules, audit rules, primary group, owner) of the security descriptor to get.</param>
	public string GetSddlForm(AccessControlSections includeSections)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if ((includeSections & AccessControlSections.Owner) != 0 && Owner != null)
		{
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "O:{0}", Owner.GetSddlForm());
		}
		if ((includeSections & AccessControlSections.Group) != 0 && Group != null)
		{
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "G:{0}", Group.GetSddlForm());
		}
		if ((includeSections & AccessControlSections.Access) != 0 && DaclPresent && !DaclIsUnmodifiedAefa)
		{
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "D:{0}", InternalDacl.GetSddlForm(ControlFlags, isDacl: true));
		}
		if ((includeSections & AccessControlSections.Audit) != 0 && SaclPresent)
		{
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "S:{0}", InternalSacl.GetSddlForm(ControlFlags, isDacl: false));
		}
		return stringBuilder.ToString();
	}

	/// <summary>Returns a boolean value that specifies whether the security descriptor associated with this  <see cref="T:System.Security.AccessControl.GenericSecurityDescriptor" /> object can be converted to the Security Descriptor Definition Language (SDDL) format.</summary>
	/// <returns>true if the security descriptor associated with this  <see cref="T:System.Security.AccessControl.GenericSecurityDescriptor" /> object can be converted to the Security Descriptor Definition Language (SDDL) format; otherwise, false.</returns>
	public static bool IsSddlConversionSupported()
	{
		return true;
	}

	private void WriteUShort(ushort val, byte[] buffer, int offset)
	{
		buffer[offset] = (byte)val;
		buffer[offset + 1] = (byte)(val >> 8);
	}

	private void WriteInt(int val, byte[] buffer, int offset)
	{
		buffer[offset] = (byte)val;
		buffer[offset + 1] = (byte)(val >> 8);
		buffer[offset + 2] = (byte)(val >> 16);
		buffer[offset + 3] = (byte)(val >> 24);
	}
}
