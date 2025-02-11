using System.Globalization;
using System.Security.Principal;

namespace System.Security.AccessControl;

/// <summary>Controls access to Directory Services objects. This class represents an Access Control Entry (ACE) associated with a directory object.</summary>
public sealed class ObjectAce : QualifiedAce
{
	private Guid object_ace_type;

	private Guid inherited_object_type;

	private ObjectAceFlags object_ace_flags;

	/// <summary>Gets the length, in bytes, of the binary representation of the current <see cref="T:System.Security.AccessControl.ObjectAce" /> object. This length should be used before marshaling the ACL into a binary array with the <see cref="M:System.Security.AccessControl.ObjectAce.GetBinaryForm" /> method.</summary>
	/// <returns>The length, in bytes, of the binary representation of the current <see cref="T:System.Security.AccessControl.ObjectAce" /> object.</returns>
	public override int BinaryLength
	{
		get
		{
			int num = 12 + base.SecurityIdentifier.BinaryLength + base.OpaqueLength;
			if (ObjectAceTypePresent)
			{
				num += 16;
			}
			if (InheritedObjectAceTypePresent)
			{
				num += 16;
			}
			return num;
		}
	}

	/// <summary>Gets or sets the GUID of the object type that can inherit the Access Control Entry (ACE) that this <see cref="T:System.Security.AccessControl.ObjectAce" /> object represents.</summary>
	/// <returns>The GUID of the object type that can inherit the Access Control Entry (ACE) that this <see cref="T:System.Security.AccessControl.ObjectAce" /> object represents.</returns>
	public Guid InheritedObjectAceType
	{
		get
		{
			return inherited_object_type;
		}
		set
		{
			inherited_object_type = value;
		}
	}

	private bool InheritedObjectAceTypePresent => (ObjectAceFlags & ObjectAceFlags.InheritedObjectAceTypePresent) != 0;

	/// <summary>Gets or sets flags that specify whether the <see cref="P:System.Security.AccessControl.ObjectAce.ObjectAceType" /> and <see cref="P:System.Security.AccessControl.ObjectAce.InheritedObjectAceType" /> properties contain values that identify valid object types.</summary>
	/// <returns>On or more members of the <see cref="T:System.Security.AccessControl.ObjectAceFlags" /> enumeration combined with a logical OR operation.</returns>
	public ObjectAceFlags ObjectAceFlags
	{
		get
		{
			return object_ace_flags;
		}
		set
		{
			object_ace_flags = value;
		}
	}

	/// <summary>Gets or sets the GUID of the object type associated with this <see cref="T:System.Security.AccessControl.ObjectAce" /> object.</summary>
	/// <returns>The GUID of the object type associated with this <see cref="T:System.Security.AccessControl.ObjectAce" /> object.</returns>
	public Guid ObjectAceType
	{
		get
		{
			return object_ace_type;
		}
		set
		{
			object_ace_type = value;
		}
	}

	private bool ObjectAceTypePresent => (ObjectAceFlags & ObjectAceFlags.ObjectAceTypePresent) != 0;

	/// <summary>Initiates a new instance of the <see cref="T:System.Security.AccessControl.ObjectAce" /> class.</summary>
	/// <param name="aceFlags">The inheritance, inheritance propagation, and auditing conditions for the new Access Control Entry (ACE).</param>
	/// <param name="qualifier">The use of the new ACE.</param>
	/// <param name="accessMask">The access mask for the ACE.</param>
	/// <param name="sid">The <see cref="T:System.Security.Principal.SecurityIdentifier" /> associated with the new ACE.</param>
	/// <param name="flags">Whether the <paramref name="type" /> and <paramref name="inheritedType" /> parameters contain valid object GUIDs.</param>
	/// <param name="type">A GUID that identifies the object type to which the new ACE applies.</param>
	/// <param name="inheritedType">A GUID that identifies the object type that can inherit the new ACE.</param>
	/// <param name="isCallback">true if the new ACE is a callback type ACE.</param>
	/// <param name="opaque">Opaque data associated with the new ACE. This is allowed only for callback ACE types. The length of this array must not be greater than the return value of the <see cref="M:System.Security.AccessControl.ObjectAceMaxOpaqueLength" /> method.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The qualifier parameter contains an invalid value or the length of the value of the opaque parameter is greater than the return value of the <see cref="M:System.Security.AccessControl.ObjectAceMaxOpaqueLength" /> method.</exception>
	public ObjectAce(AceFlags aceFlags, AceQualifier qualifier, int accessMask, SecurityIdentifier sid, ObjectAceFlags flags, Guid type, Guid inheritedType, bool isCallback, byte[] opaque)
		: base(ConvertType(qualifier, isCallback), aceFlags, opaque)
	{
		base.AccessMask = accessMask;
		base.SecurityIdentifier = sid;
		ObjectAceFlags = flags;
		ObjectAceType = type;
		InheritedObjectAceType = inheritedType;
	}

	internal ObjectAce(AceType type, AceFlags flags, int accessMask, SecurityIdentifier sid, ObjectAceFlags objFlags, Guid objType, Guid inheritedType, byte[] opaque)
		: base(type, flags, opaque)
	{
		base.AccessMask = accessMask;
		base.SecurityIdentifier = sid;
		ObjectAceFlags = objFlags;
		ObjectAceType = objType;
		InheritedObjectAceType = inheritedType;
	}

	internal ObjectAce(byte[] binaryForm, int offset)
		: base(binaryForm, offset)
	{
		int num = GenericAce.ReadUShort(binaryForm, offset + 2);
		int num2 = 12 + SecurityIdentifier.MinBinaryLength;
		if (offset > binaryForm.Length - num)
		{
			throw new ArgumentException("Invalid ACE - truncated", "binaryForm");
		}
		if (num < num2)
		{
			throw new ArgumentException("Invalid ACE", "binaryForm");
		}
		base.AccessMask = GenericAce.ReadInt(binaryForm, offset + 4);
		ObjectAceFlags = (ObjectAceFlags)GenericAce.ReadInt(binaryForm, offset + 8);
		if (ObjectAceTypePresent)
		{
			num2 += 16;
		}
		if (InheritedObjectAceTypePresent)
		{
			num2 += 16;
		}
		if (num < num2)
		{
			throw new ArgumentException("Invalid ACE", "binaryForm");
		}
		int num3 = 12;
		if (ObjectAceTypePresent)
		{
			ObjectAceType = ReadGuid(binaryForm, offset + num3);
			num3 += 16;
		}
		if (InheritedObjectAceTypePresent)
		{
			InheritedObjectAceType = ReadGuid(binaryForm, offset + num3);
			num3 += 16;
		}
		base.SecurityIdentifier = new SecurityIdentifier(binaryForm, offset + num3);
		num3 += base.SecurityIdentifier.BinaryLength;
		int num4 = num - num3;
		if (num4 > 0)
		{
			byte[] destinationArray = new byte[num4];
			Array.Copy(binaryForm, offset + num3, destinationArray, 0, num4);
			SetOpaque(destinationArray);
		}
	}

	/// <summary>Marshals the contents of the <see cref="T:System.Security.AccessControl.ObjectAce" /> object into the specified byte array beginning at the specified offset.</summary>
	/// <param name="binaryForm">The byte array into which the contents of the <see cref="T:System.Security.AccessControl.ObjectAce" /> is marshaled.</param>
	/// <param name="offset">The offset at which to start marshaling.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is negative or too high to allow the entire <see cref="T:System.Security.AccessControl.ObjectAce" /> to be copied into <paramref name="array" />.</exception>
	public override void GetBinaryForm(byte[] binaryForm, int offset)
	{
		int binaryLength = BinaryLength;
		binaryForm[offset++] = (byte)base.AceType;
		binaryForm[offset++] = (byte)base.AceFlags;
		GenericAce.WriteUShort((ushort)binaryLength, binaryForm, offset);
		offset += 2;
		GenericAce.WriteInt(base.AccessMask, binaryForm, offset);
		offset += 4;
		GenericAce.WriteInt((int)ObjectAceFlags, binaryForm, offset);
		offset += 4;
		if ((ObjectAceFlags & ObjectAceFlags.ObjectAceTypePresent) != 0)
		{
			WriteGuid(ObjectAceType, binaryForm, offset);
			offset += 16;
		}
		if ((ObjectAceFlags & ObjectAceFlags.InheritedObjectAceTypePresent) != 0)
		{
			WriteGuid(InheritedObjectAceType, binaryForm, offset);
			offset += 16;
		}
		base.SecurityIdentifier.GetBinaryForm(binaryForm, offset);
		offset += base.SecurityIdentifier.BinaryLength;
		byte[] array = GetOpaque();
		if (array != null)
		{
			Array.Copy(array, 0, binaryForm, offset, array.Length);
			offset += array.Length;
		}
	}

	/// <summary>Returns the maximum allowed length, in bytes, of an opaque data BLOB for callback Access Control Entries (ACEs).</summary>
	/// <returns>The maximum allowed length, in bytes, of an opaque data BLOB for callback Access Control Entries (ACEs).</returns>
	/// <param name="isCallback">True if the <see cref="T:System.Security.AccessControl.ObjectAce" /> is a callback ACE type.</param>
	public static int MaxOpaqueLength(bool isCallback)
	{
		return 65423;
	}

	internal override string GetSddlForm()
	{
		if (base.OpaqueLength != 0)
		{
			throw new NotImplementedException("Unable to convert conditional ACEs to SDDL");
		}
		string text = "";
		if ((ObjectAceFlags & ObjectAceFlags.ObjectAceTypePresent) != 0)
		{
			text = object_ace_type.ToString("D");
		}
		string text2 = "";
		if ((ObjectAceFlags & ObjectAceFlags.InheritedObjectAceTypePresent) != 0)
		{
			text2 = inherited_object_type.ToString("D");
		}
		return string.Format(CultureInfo.InvariantCulture, "({0};{1};{2};{3};{4};{5})", GenericAce.GetSddlAceType(base.AceType), GenericAce.GetSddlAceFlags(base.AceFlags), KnownAce.GetSddlAccessRights(base.AccessMask), text, text2, base.SecurityIdentifier.GetSddlForm());
	}

	private static AceType ConvertType(AceQualifier qualifier, bool isCallback)
	{
		switch (qualifier)
		{
		case AceQualifier.AccessAllowed:
			if (isCallback)
			{
				return AceType.AccessAllowedCallbackObject;
			}
			return AceType.AccessAllowedObject;
		case AceQualifier.AccessDenied:
			if (isCallback)
			{
				return AceType.AccessDeniedCallbackObject;
			}
			return AceType.AccessDeniedObject;
		case AceQualifier.SystemAlarm:
			if (isCallback)
			{
				return AceType.SystemAlarmCallbackObject;
			}
			return AceType.SystemAlarmObject;
		case AceQualifier.SystemAudit:
			if (isCallback)
			{
				return AceType.SystemAuditCallbackObject;
			}
			return AceType.SystemAuditObject;
		default:
			throw new ArgumentException("Unrecognized ACE qualifier: " + qualifier, "qualifier");
		}
	}

	private void WriteGuid(Guid val, byte[] buffer, int offset)
	{
		Array.Copy(val.ToByteArray(), 0, buffer, offset, 16);
	}

	private Guid ReadGuid(byte[] buffer, int offset)
	{
		byte[] array = new byte[16];
		Array.Copy(buffer, offset, array, 0, 16);
		return new Guid(array);
	}
}
