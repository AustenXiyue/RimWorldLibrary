using System.Globalization;
using System.Security.Principal;
using System.Text;
using Unity;

namespace System.Security.AccessControl;

/// <summary>Represents an Access Control Entry (ACE), and is the base class for all other ACE classes.</summary>
public abstract class GenericAce
{
	private AceFlags ace_flags;

	private AceType ace_type;

	/// <summary>Gets or sets the <see cref="T:System.Security.AccessControl.AceFlags" /> associated with this <see cref="T:System.Security.AccessControl.GenericAce" /> object.</summary>
	/// <returns>The <see cref="T:System.Security.AccessControl.AceFlags" /> associated with this <see cref="T:System.Security.AccessControl.GenericAce" /> object.</returns>
	public AceFlags AceFlags
	{
		get
		{
			return ace_flags;
		}
		set
		{
			ace_flags = value;
		}
	}

	/// <summary>Gets the type of this Access Control Entry (ACE).</summary>
	/// <returns>The type of this ACE.</returns>
	public AceType AceType => ace_type;

	/// <summary>Gets the audit information associated with this Access Control Entry (ACE).</summary>
	/// <returns>The audit information associated with this Access Control Entry (ACE).</returns>
	public AuditFlags AuditFlags
	{
		get
		{
			AuditFlags auditFlags = AuditFlags.None;
			if ((ace_flags & AceFlags.SuccessfulAccess) != 0)
			{
				auditFlags |= AuditFlags.Success;
			}
			if ((ace_flags & AceFlags.FailedAccess) != 0)
			{
				auditFlags |= AuditFlags.Failure;
			}
			return auditFlags;
		}
	}

	/// <summary>Gets the length, in bytes, of the binary representation of the current <see cref="T:System.Security.AccessControl.GenericAce" /> object. This length should be used before marshaling the ACL into a binary array with the <see cref="M:System.Security.AccessControl.GenericAce.GetBinaryForm" /> method.</summary>
	/// <returns>The length, in bytes, of the binary representation of the current <see cref="T:System.Security.AccessControl.GenericAce" /> object.</returns>
	public abstract int BinaryLength { get; }

	/// <summary>Gets flags that specify the inheritance properties of this Access Control Entry (ACE).</summary>
	/// <returns>Flags that specify the inheritance properties of this ACE.</returns>
	public InheritanceFlags InheritanceFlags
	{
		get
		{
			InheritanceFlags inheritanceFlags = InheritanceFlags.None;
			if ((ace_flags & AceFlags.ObjectInherit) != 0)
			{
				inheritanceFlags |= InheritanceFlags.ObjectInherit;
			}
			if ((ace_flags & AceFlags.ContainerInherit) != 0)
			{
				inheritanceFlags |= InheritanceFlags.ContainerInherit;
			}
			return inheritanceFlags;
		}
	}

	/// <summary>Gets a Boolean value that specifies whether this Access Control Entry (ACE) is inherited or is set explicitly.</summary>
	/// <returns>true if this ACE is inherited; otherwise, false.</returns>
	public bool IsInherited => (ace_flags & AceFlags.Inherited) != 0;

	/// <summary>Gets flags that specify the inheritance propagation properties of this Access Control Entry (ACE).</summary>
	/// <returns>Flags that specify the inheritance propagation properties of this ACE.</returns>
	public PropagationFlags PropagationFlags
	{
		get
		{
			PropagationFlags propagationFlags = PropagationFlags.None;
			if ((ace_flags & AceFlags.InheritOnly) != 0)
			{
				propagationFlags |= PropagationFlags.InheritOnly;
			}
			if ((ace_flags & AceFlags.NoPropagateInherit) != 0)
			{
				propagationFlags |= PropagationFlags.NoPropagateInherit;
			}
			return propagationFlags;
		}
	}

	internal GenericAce(AceType type, AceFlags flags)
	{
		if ((int)type > 16)
		{
			throw new ArgumentOutOfRangeException("type");
		}
		ace_type = type;
		ace_flags = flags;
	}

	internal GenericAce(byte[] binaryForm, int offset)
	{
		if (binaryForm == null)
		{
			throw new ArgumentNullException("binaryForm");
		}
		if (offset < 0 || offset > binaryForm.Length - 2)
		{
			throw new ArgumentOutOfRangeException("offset", offset, "Offset out of range");
		}
		ace_type = (AceType)binaryForm[offset];
		ace_flags = (AceFlags)binaryForm[offset + 1];
	}

	/// <summary>Creates a deep copy of this Access Control Entry (ACE).</summary>
	/// <returns>The <see cref="T:System.Security.AccessControl.GenericAce" /> object that this method creates.</returns>
	public GenericAce Copy()
	{
		byte[] binaryForm = new byte[BinaryLength];
		GetBinaryForm(binaryForm, 0);
		return CreateFromBinaryForm(binaryForm, 0);
	}

	/// <summary>Creates a <see cref="T:System.Security.AccessControl.GenericAce" /> object from the specified binary data.</summary>
	/// <returns>The <see cref="T:System.Security.AccessControl.GenericAce" /> object this method creates.</returns>
	/// <param name="binaryForm">The binary data from which to create the new <see cref="T:System.Security.AccessControl.GenericAce" /> object.</param>
	/// <param name="offset">The offset at which to begin unmarshaling.</param>
	public static GenericAce CreateFromBinaryForm(byte[] binaryForm, int offset)
	{
		if (binaryForm == null)
		{
			throw new ArgumentNullException("binaryForm");
		}
		if (offset < 0 || offset > binaryForm.Length - 1)
		{
			throw new ArgumentOutOfRangeException("offset", offset, "Offset out of range");
		}
		if (IsObjectType((AceType)binaryForm[offset]))
		{
			return new ObjectAce(binaryForm, offset);
		}
		return new CommonAce(binaryForm, offset);
	}

	/// <summary>Determines whether the specified <see cref="T:System.Security.AccessControl.GenericAce" /> object is equal to the current <see cref="T:System.Security.AccessControl.GenericAce" /> object.</summary>
	/// <returns>true if the specified <see cref="T:System.Security.AccessControl.GenericAce" /> object is equal to the current <see cref="T:System.Security.AccessControl.GenericAce" /> object; otherwise, false.</returns>
	/// <param name="o">The <see cref="T:System.Security.AccessControl.GenericAce" /> object to compare to the current <see cref="T:System.Security.AccessControl.GenericAce" /> object.</param>
	public sealed override bool Equals(object o)
	{
		return this == o as GenericAce;
	}

	/// <summary>Marshals the contents of the <see cref="T:System.Security.AccessControl.GenericAce" /> object into the specified byte array beginning at the specified offset.</summary>
	/// <param name="binaryForm">The byte array into which the contents of the <see cref="T:System.Security.AccessControl.GenericAce" /> is marshaled.</param>
	/// <param name="offset">The offset at which to start marshaling.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is negative or too high to allow the entire <see cref="T:System.Security.AccessControl.GenericAcl" /> to be copied into <paramref name="array" />.</exception>
	public abstract void GetBinaryForm(byte[] binaryForm, int offset);

	/// <summary>Serves as a hash function for the <see cref="T:System.Security.AccessControl.GenericAce" /> class. The  <see cref="M:System.Security.AccessControl.GenericAce.GetHashCode" /> method is suitable for use in hashing algorithms and data structures like a hash table.</summary>
	/// <returns>A hash code for the current <see cref="T:System.Security.AccessControl.GenericAce" /> object.</returns>
	public sealed override int GetHashCode()
	{
		byte[] array = new byte[BinaryLength];
		GetBinaryForm(array, 0);
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			num = (num << 3) | ((num >> 29) & 7);
			num ^= array[i] & 0xFF;
		}
		return num;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Security.AccessControl.GenericAce" /> objects are considered equal.</summary>
	/// <returns>true if the two <see cref="T:System.Security.AccessControl.GenericAce" /> objects are equal; otherwise, false.</returns>
	/// <param name="left">The first <see cref="T:System.Security.AccessControl.GenericAce" /> object to compare.</param>
	/// <param name="right">The second <see cref="T:System.Security.AccessControl.GenericAce" /> to compare.</param>
	public static bool operator ==(GenericAce left, GenericAce right)
	{
		if ((object)left == null)
		{
			return (object)right == null;
		}
		if ((object)right == null)
		{
			return false;
		}
		int binaryLength = left.BinaryLength;
		int binaryLength2 = right.BinaryLength;
		if (binaryLength != binaryLength2)
		{
			return false;
		}
		byte[] array = new byte[binaryLength];
		byte[] array2 = new byte[binaryLength2];
		left.GetBinaryForm(array, 0);
		right.GetBinaryForm(array2, 0);
		for (int i = 0; i < binaryLength; i++)
		{
			if (array[i] != array2[i])
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Security.AccessControl.GenericAce" /> objects are considered unequal.</summary>
	/// <returns>true if the two <see cref="T:System.Security.AccessControl.GenericAce" /> objects are unequal; otherwise, false.</returns>
	/// <param name="left">The first <see cref="T:System.Security.AccessControl.GenericAce" /> object to compare.</param>
	/// <param name="right">The second <see cref="T:System.Security.AccessControl.GenericAce" /> to compare.</param>
	public static bool operator !=(GenericAce left, GenericAce right)
	{
		if ((object)left == null)
		{
			return (object)right != null;
		}
		if ((object)right == null)
		{
			return true;
		}
		int binaryLength = left.BinaryLength;
		int binaryLength2 = right.BinaryLength;
		if (binaryLength != binaryLength2)
		{
			return true;
		}
		byte[] array = new byte[binaryLength];
		byte[] array2 = new byte[binaryLength2];
		left.GetBinaryForm(array, 0);
		right.GetBinaryForm(array2, 0);
		for (int i = 0; i < binaryLength; i++)
		{
			if (array[i] != array2[i])
			{
				return true;
			}
		}
		return false;
	}

	internal abstract string GetSddlForm();

	internal static GenericAce CreateFromSddlForm(string sddlForm, ref int pos)
	{
		if (sddlForm[pos] != '(')
		{
			throw new ArgumentException("Invalid SDDL string.", "sddlForm");
		}
		int num = sddlForm.IndexOf(')', pos);
		if (num < 0)
		{
			throw new ArgumentException("Invalid SDDL string.", "sddlForm");
		}
		int length = num - (pos + 1);
		string[] array = sddlForm.Substring(pos + 1, length).ToUpperInvariant().Split(';');
		if (array.Length != 6)
		{
			throw new ArgumentException("Invalid SDDL string.", "sddlForm");
		}
		ObjectAceFlags objectAceFlags = ObjectAceFlags.None;
		AceType aceType = ParseSddlAceType(array[0]);
		AceFlags flags = ParseSddlAceFlags(array[1]);
		int accessMask = ParseSddlAccessRights(array[2]);
		Guid objType = Guid.Empty;
		if (!string.IsNullOrEmpty(array[3]))
		{
			objType = new Guid(array[3]);
			objectAceFlags |= ObjectAceFlags.ObjectAceTypePresent;
		}
		Guid inheritedType = Guid.Empty;
		if (!string.IsNullOrEmpty(array[4]))
		{
			inheritedType = new Guid(array[4]);
			objectAceFlags |= ObjectAceFlags.InheritedObjectAceTypePresent;
		}
		SecurityIdentifier sid = new SecurityIdentifier(array[5]);
		if (aceType == AceType.AccessAllowedCallback || aceType == AceType.AccessDeniedCallback)
		{
			throw new NotImplementedException("Conditional ACEs not supported");
		}
		pos = num + 1;
		if (IsObjectType(aceType))
		{
			return new ObjectAce(aceType, flags, accessMask, sid, objectAceFlags, objType, inheritedType, null);
		}
		if (objectAceFlags != 0)
		{
			throw new ArgumentException("Invalid SDDL string.", "sddlForm");
		}
		return new CommonAce(aceType, flags, accessMask, sid, null);
	}

	private static bool IsObjectType(AceType type)
	{
		if (type != AceType.AccessAllowedCallbackObject && type != AceType.AccessAllowedObject && type != AceType.AccessDeniedCallbackObject && type != AceType.AccessDeniedObject && type != AceType.SystemAlarmCallbackObject && type != AceType.SystemAlarmObject && type != AceType.SystemAuditCallbackObject)
		{
			return type == AceType.SystemAuditObject;
		}
		return true;
	}

	internal static string GetSddlAceType(AceType type)
	{
		return type switch
		{
			AceType.AccessAllowed => "A", 
			AceType.AccessDenied => "D", 
			AceType.AccessAllowedObject => "OA", 
			AceType.AccessDeniedObject => "OD", 
			AceType.SystemAudit => "AU", 
			AceType.SystemAlarm => "AL", 
			AceType.SystemAuditObject => "OU", 
			AceType.SystemAlarmObject => "OL", 
			AceType.AccessAllowedCallback => "XA", 
			AceType.AccessDeniedCallback => "XD", 
			_ => throw new ArgumentException("Unable to convert to SDDL ACE type: " + type, "type"), 
		};
	}

	private static AceType ParseSddlAceType(string type)
	{
		return type switch
		{
			"A" => AceType.AccessAllowed, 
			"D" => AceType.AccessDenied, 
			"OA" => AceType.AccessAllowedObject, 
			"OD" => AceType.AccessDeniedObject, 
			"AU" => AceType.SystemAudit, 
			"AL" => AceType.SystemAlarm, 
			"OU" => AceType.SystemAuditObject, 
			"OL" => AceType.SystemAlarmObject, 
			"XA" => AceType.AccessAllowedCallback, 
			"XD" => AceType.AccessDeniedCallback, 
			_ => throw new ArgumentException("Unable to convert SDDL to ACE type: " + type, "type"), 
		};
	}

	internal static string GetSddlAceFlags(AceFlags flags)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if ((flags & AceFlags.ObjectInherit) != 0)
		{
			stringBuilder.Append("OI");
		}
		if ((flags & AceFlags.ContainerInherit) != 0)
		{
			stringBuilder.Append("CI");
		}
		if ((flags & AceFlags.NoPropagateInherit) != 0)
		{
			stringBuilder.Append("NP");
		}
		if ((flags & AceFlags.InheritOnly) != 0)
		{
			stringBuilder.Append("IO");
		}
		if ((flags & AceFlags.Inherited) != 0)
		{
			stringBuilder.Append("ID");
		}
		if ((flags & AceFlags.SuccessfulAccess) != 0)
		{
			stringBuilder.Append("SA");
		}
		if ((flags & AceFlags.FailedAccess) != 0)
		{
			stringBuilder.Append("FA");
		}
		return stringBuilder.ToString();
	}

	private static AceFlags ParseSddlAceFlags(string flags)
	{
		AceFlags aceFlags = AceFlags.None;
		int i;
		for (i = 0; i < flags.Length - 1; i += 2)
		{
			aceFlags = flags.Substring(i, 2) switch
			{
				"CI" => aceFlags | AceFlags.ContainerInherit, 
				"OI" => aceFlags | AceFlags.ObjectInherit, 
				"NP" => aceFlags | AceFlags.NoPropagateInherit, 
				"IO" => aceFlags | AceFlags.InheritOnly, 
				"ID" => aceFlags | AceFlags.Inherited, 
				"SA" => aceFlags | AceFlags.SuccessfulAccess, 
				"FA" => aceFlags | AceFlags.FailedAccess, 
				_ => throw new ArgumentException("Invalid SDDL string.", "flags"), 
			};
		}
		if (i != flags.Length)
		{
			throw new ArgumentException("Invalid SDDL string.", "flags");
		}
		return aceFlags;
	}

	private static int ParseSddlAccessRights(string accessMask)
	{
		if (accessMask.StartsWith("0X"))
		{
			return int.Parse(accessMask.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
		}
		if (char.IsDigit(accessMask, 0))
		{
			return int.Parse(accessMask, NumberStyles.Integer, CultureInfo.InvariantCulture);
		}
		return ParseSddlAliasRights(accessMask);
	}

	private static int ParseSddlAliasRights(string accessMask)
	{
		int num = 0;
		int i;
		for (i = 0; i < accessMask.Length - 1; i += 2)
		{
			SddlAccessRight sddlAccessRight = SddlAccessRight.LookupByName(accessMask.Substring(i, 2));
			if (sddlAccessRight == null)
			{
				throw new ArgumentException("Invalid SDDL string.", "accessMask");
			}
			num |= sddlAccessRight.Value;
		}
		if (i != accessMask.Length)
		{
			throw new ArgumentException("Invalid SDDL string.", "accessMask");
		}
		return num;
	}

	internal static ushort ReadUShort(byte[] buffer, int offset)
	{
		return (ushort)(buffer[offset] | (buffer[offset + 1] << 8));
	}

	internal static int ReadInt(byte[] buffer, int offset)
	{
		return buffer[offset] | (buffer[offset + 1] << 8) | (buffer[offset + 2] << 16) | (buffer[offset + 3] << 24);
	}

	internal static void WriteInt(int val, byte[] buffer, int offset)
	{
		buffer[offset] = (byte)val;
		buffer[offset + 1] = (byte)(val >> 8);
		buffer[offset + 2] = (byte)(val >> 16);
		buffer[offset + 3] = (byte)(val >> 24);
	}

	internal static void WriteUShort(ushort val, byte[] buffer, int offset)
	{
		buffer[offset] = (byte)val;
		buffer[offset + 1] = (byte)(val >> 8);
	}

	internal GenericAce()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
