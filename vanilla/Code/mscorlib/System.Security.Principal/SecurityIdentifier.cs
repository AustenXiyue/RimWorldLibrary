using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Security.Principal;

/// <summary>Represents a security identifier (SID) and provides marshaling and comparison operations for SIDs.</summary>
[ComVisible(false)]
public sealed class SecurityIdentifier : IdentityReference, IComparable<SecurityIdentifier>
{
	private byte[] buffer;

	/// <summary>Returns the maximum size, in bytes, of the binary representation of the security identifier.</summary>
	public static readonly int MaxBinaryLength = 68;

	/// <summary>Returns the minimum size, in bytes, of the binary representation of the security identifier.</summary>
	public static readonly int MinBinaryLength = 8;

	/// <summary>Returns the account domain security identifier (SID) portion from the SID represented by the <see cref="T:System.Security.Principal.SecurityIdentifier" /> object if the SID represents a Windows account SID. If the SID does not represent a Windows account SID, this property returns <see cref="T:System.ArgumentNullException" />.</summary>
	/// <returns>The account domain SID portion from the SID represented by the <see cref="T:System.Security.Principal.SecurityIdentifier" /> object if the SID represents a Windows account SID; otherwise, it returns <see cref="T:System.ArgumentNullException" />.</returns>
	public SecurityIdentifier AccountDomainSid
	{
		get
		{
			if (!Value.StartsWith("S-1-5-21") || buffer[1] < 4)
			{
				return null;
			}
			byte[] array = new byte[24];
			Array.Copy(buffer, 0, array, 0, array.Length);
			array[1] = 4;
			return new SecurityIdentifier(array, 0);
		}
	}

	/// <summary>Returns the length, in bytes, of the security identifier (SID) represented by the <see cref="T:System.Security.Principal.SecurityIdentifier" /> object.</summary>
	/// <returns>The length, in bytes, of the SID represented by the <see cref="T:System.Security.Principal.SecurityIdentifier" /> object.</returns>
	public int BinaryLength => buffer.Length;

	/// <summary>Returns an uppercase Security Descriptor Definition Language (SDDL) string for the security identifier (SID) represented by this <see cref="T:System.Security.Principal.SecurityIdentifier" /> object.</summary>
	/// <returns>An uppercase SDDL string for the SID represented by the <see cref="T:System.Security.Principal.SecurityIdentifier" /> object.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override string Value
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			ulong sidAuthority = GetSidAuthority();
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "S-1-{0}", sidAuthority);
			for (byte b = 0; b < GetSidSubAuthorityCount(); b++)
			{
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "-{0}", GetSidSubAuthority(b));
			}
			return stringBuilder.ToString();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Principal.SecurityIdentifier" /> class by using the specified security identifier (SID) in Security Descriptor Definition Language (SDDL) format.</summary>
	/// <param name="sddlForm">SDDL string for the SID used to create the <see cref="T:System.Security.Principal.SecurityIdentifier" /> object.</param>
	public SecurityIdentifier(string sddlForm)
	{
		if (sddlForm == null)
		{
			throw new ArgumentNullException("sddlForm");
		}
		buffer = ParseSddlForm(sddlForm);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Principal.SecurityIdentifier" /> class by using a specified binary representation of a security identifier (SID).</summary>
	/// <param name="binaryForm">The byte array that represents the SID.</param>
	/// <param name="offset">The byte offset to use as the starting index in <paramref name="binaryForm" />. </param>
	public unsafe SecurityIdentifier(byte[] binaryForm, int offset)
	{
		if (binaryForm == null)
		{
			throw new ArgumentNullException("binaryForm");
		}
		if (offset < 0 || offset > binaryForm.Length - 2)
		{
			throw new ArgumentException("offset");
		}
		fixed (byte* ptr = binaryForm)
		{
			CreateFromBinaryForm((IntPtr)(ptr + offset), binaryForm.Length - offset);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Principal.SecurityIdentifier" /> class by using an integer that represents the binary form of a security identifier (SID).</summary>
	/// <param name="binaryForm">An integer that represents the binary form of a SID.</param>
	public SecurityIdentifier(IntPtr binaryForm)
	{
		CreateFromBinaryForm(binaryForm, int.MaxValue);
	}

	private void CreateFromBinaryForm(IntPtr binaryForm, int length)
	{
		byte num = Marshal.ReadByte(binaryForm, 0);
		int num2 = Marshal.ReadByte(binaryForm, 1);
		if (num != 1 || num2 > 15)
		{
			throw new ArgumentException("Value was invalid.");
		}
		if (length < 8 + num2 * 4)
		{
			throw new ArgumentException("offset");
		}
		buffer = new byte[8 + num2 * 4];
		Marshal.Copy(binaryForm, buffer, 0, buffer.Length);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Principal.SecurityIdentifier" /> class by using the specified well known security identifier (SID) type and domain SID.</summary>
	/// <param name="sidType">One of the enumeration values. This value must not be <see cref="F:System.Security.Principal.WellKnownSidType.LogonIdsSid" />.</param>
	/// <param name="domainSid">The domain SID. This value is required for the following <see cref="T:System.Security.Principal.WellKnownSidType" /> values. This parameter is ignored for any other <see cref="T:System.Security.Principal.WellKnownSidType" /> values.- <see cref="F:System.Security.Principal.WellKnownSidType.AccountAdministratorSid" />- <see cref="F:System.Security.Principal.WellKnownSidType.AccountGuestSid" />- <see cref="F:System.Security.Principal.WellKnownSidType.AccountKrbtgtSid" />- <see cref="F:System.Security.Principal.WellKnownSidType.AccountDomainAdminsSid" />- <see cref="F:System.Security.Principal.WellKnownSidType.AccountDomainUsersSid" />- <see cref="F:System.Security.Principal.WellKnownSidType.AccountDomainGuestsSid" />- <see cref="F:System.Security.Principal.WellKnownSidType.AccountComputersSid" />- <see cref="F:System.Security.Principal.WellKnownSidType.AccountControllersSid" />- <see cref="F:System.Security.Principal.WellKnownSidType.AccountCertAdminsSid" />- <see cref="F:System.Security.Principal.WellKnownSidType.AccountSchemaAdminsSid" />- <see cref="F:System.Security.Principal.WellKnownSidType.AccountEnterpriseAdminsSid" />- <see cref="F:System.Security.Principal.WellKnownSidType.AccountPolicyAdminsSid" />- <see cref="F:System.Security.Principal.WellKnownSidType.AccountRasAndIasServersSid" /></param>
	public SecurityIdentifier(WellKnownSidType sidType, SecurityIdentifier domainSid)
	{
		WellKnownAccount wellKnownAccount = WellKnownAccount.LookupByType(sidType);
		if (wellKnownAccount == null)
		{
			throw new ArgumentException("Unable to convert SID type: " + sidType);
		}
		if (wellKnownAccount.IsAbsolute)
		{
			buffer = ParseSddlForm(wellKnownAccount.Sid);
			return;
		}
		if (domainSid == null)
		{
			throw new ArgumentNullException("domainSid");
		}
		buffer = ParseSddlForm(domainSid.Value + "-" + wellKnownAccount.Rid);
	}

	private ulong GetSidAuthority()
	{
		return ((ulong)buffer[2] << 40) | ((ulong)buffer[3] << 32) | ((ulong)buffer[4] << 24) | ((ulong)buffer[5] << 16) | ((ulong)buffer[6] << 8) | buffer[7];
	}

	private byte GetSidSubAuthorityCount()
	{
		return buffer[1];
	}

	private uint GetSidSubAuthority(byte index)
	{
		int num = 8 + index * 4;
		return (uint)(buffer[num] | (buffer[num + 1] << 8) | (buffer[num + 2] << 16) | (buffer[num + 3] << 24));
	}

	/// <summary>Compares the current <see cref="T:System.Security.Principal.SecurityIdentifier" /> object with the specified <see cref="T:System.Security.Principal.SecurityIdentifier" /> object.</summary>
	/// <returns>A signed number indicating the relative values of this instance and <paramref name="sid" />.Return Value Description Less than zero This instance is less than <paramref name="sid" />. Zero This instance is equal to <paramref name="sid" />. Greater than zero This instance is greater than <paramref name="sid" />. </returns>
	/// <param name="sid">The object to compare with the current object.</param>
	public int CompareTo(SecurityIdentifier sid)
	{
		if (sid == null)
		{
			throw new ArgumentNullException("sid");
		}
		int result;
		if ((result = GetSidAuthority().CompareTo(sid.GetSidAuthority())) != 0)
		{
			return result;
		}
		if ((result = GetSidSubAuthorityCount().CompareTo(sid.GetSidSubAuthorityCount())) != 0)
		{
			return result;
		}
		for (byte b = 0; b < GetSidSubAuthorityCount(); b++)
		{
			if ((result = GetSidSubAuthority(b).CompareTo(sid.GetSidSubAuthority(b))) != 0)
			{
				return result;
			}
		}
		return 0;
	}

	/// <summary>Returns a value that indicates whether this <see cref="T:System.Security.Principal.SecurityIdentifier" /> object is equal to a specified object.</summary>
	/// <returns>true if <paramref name="o" /> is an object with the same underlying type and value as this <see cref="T:System.Security.Principal.SecurityIdentifier" /> object; otherwise, false.</returns>
	/// <param name="o">An object to compare with this <see cref="T:System.Security.Principal.SecurityIdentifier" /> object, or null.</param>
	public override bool Equals(object o)
	{
		return Equals(o as SecurityIdentifier);
	}

	/// <summary>Indicates whether the specified <see cref="T:System.Security.Principal.SecurityIdentifier" /> object is equal to the current <see cref="T:System.Security.Principal.SecurityIdentifier" /> object.</summary>
	/// <returns>true if the value of <paramref name="sid" /> is equal to the value of the current <see cref="T:System.Security.Principal.SecurityIdentifier" /> object.</returns>
	/// <param name="sid">The object to compare with the current object.</param>
	public bool Equals(SecurityIdentifier sid)
	{
		if (sid == null)
		{
			return false;
		}
		return sid.Value == Value;
	}

	/// <summary>Copies the binary representation of the specified security identifier (SID) represented by the <see cref="T:System.Security.Principal.SecurityIdentifier" /> class to a byte array.</summary>
	/// <param name="binaryForm">The byte array to receive the copied SID.</param>
	/// <param name="offset">The byte offset to use as the starting index in <paramref name="binaryForm" />. </param>
	public void GetBinaryForm(byte[] binaryForm, int offset)
	{
		if (binaryForm == null)
		{
			throw new ArgumentNullException("binaryForm");
		}
		if (offset < 0 || offset > binaryForm.Length - buffer.Length)
		{
			throw new ArgumentException("offset");
		}
		Array.Copy(buffer, 0, binaryForm, offset, buffer.Length);
	}

	/// <summary>Serves as a hash function for the current <see cref="T:System.Security.Principal.SecurityIdentifier" /> object. The <see cref="M:System.Security.Principal.SecurityIdentifier.GetHashCode" /> method is suitable for hashing algorithms and data structures like a hash table.</summary>
	/// <returns>A hash value for the current <see cref="T:System.Security.Principal.SecurityIdentifier" /> object.</returns>
	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	/// <summary>Returns a value that indicates whether the security identifier (SID) represented by this <see cref="T:System.Security.Principal.SecurityIdentifier" /> object is a valid Windows account SID.</summary>
	/// <returns>true if the SID represented by this <see cref="T:System.Security.Principal.SecurityIdentifier" /> object is a valid Windows account SID; otherwise, false.</returns>
	public bool IsAccountSid()
	{
		return AccountDomainSid != null;
	}

	/// <summary>Returns a value that indicates whether the security identifier (SID) represented by this <see cref="T:System.Security.Principal.SecurityIdentifier" /> object is from the same domain as the specified SID.</summary>
	/// <returns>true if the SID represented by this <see cref="T:System.Security.Principal.SecurityIdentifier" /> object is in the same domain as the <paramref name="sid" /> SID; otherwise, false.</returns>
	/// <param name="sid">The SID to compare with this <see cref="T:System.Security.Principal.SecurityIdentifier" /> object.</param>
	public bool IsEqualDomainSid(SecurityIdentifier sid)
	{
		SecurityIdentifier accountDomainSid = AccountDomainSid;
		if (accountDomainSid == null)
		{
			return false;
		}
		return accountDomainSid.Equals(sid.AccountDomainSid);
	}

	/// <summary>Returns a value that indicates whether the specified type is a valid translation type for the <see cref="T:System.Security.Principal.SecurityIdentifier" /> class.</summary>
	/// <returns>true if <paramref name="targetType" /> is a valid translation type for the <see cref="T:System.Security.Principal.SecurityIdentifier" /> class; otherwise, false.</returns>
	/// <param name="targetType">The type being queried for validity to serve as a conversion from <see cref="T:System.Security.Principal.SecurityIdentifier" />. The following target types are valid:- <see cref="T:System.Security.Principal.NTAccount" />- <see cref="T:System.Security.Principal.SecurityIdentifier" /></param>
	public override bool IsValidTargetType(Type targetType)
	{
		if (targetType == typeof(SecurityIdentifier))
		{
			return true;
		}
		if (targetType == typeof(NTAccount))
		{
			return true;
		}
		return false;
	}

	/// <summary>Returns a value that indicates whether the <see cref="T:System.Security.Principal.SecurityIdentifier" /> object matches the specified well known security identifier (SID) type. </summary>
	/// <returns>true if <paramref name="type" /> is the SID type for the <see cref="T:System.Security.Principal.SecurityIdentifier" /> object; otherwise, false.</returns>
	/// <param name="type">A value to compare with the <see cref="T:System.Security.Principal.SecurityIdentifier" /> object.</param>
	public bool IsWellKnown(WellKnownSidType type)
	{
		WellKnownAccount wellKnownAccount = WellKnownAccount.LookupByType(type);
		if (wellKnownAccount == null)
		{
			return false;
		}
		string value = Value;
		if (wellKnownAccount.IsAbsolute)
		{
			return value == wellKnownAccount.Sid;
		}
		if (value.StartsWith("S-1-5-21", StringComparison.OrdinalIgnoreCase))
		{
			return value.EndsWith("-" + wellKnownAccount.Rid, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	/// <summary>Returns the security identifier (SID), in Security Descriptor Definition Language (SDDL) format, for the account represented by the <see cref="T:System.Security.Principal.SecurityIdentifier" /> object. An example of the SDDL format is S-1-5-9. </summary>
	/// <returns>The SID, in SDDL format, for the account represented by the <see cref="T:System.Security.Principal.SecurityIdentifier" /> object.</returns>
	public override string ToString()
	{
		return Value;
	}

	/// <summary>Translates the account name represented by the <see cref="T:System.Security.Principal.SecurityIdentifier" /> object into another <see cref="T:System.Security.Principal.IdentityReference" />-derived type.</summary>
	/// <returns>The converted identity.</returns>
	/// <param name="targetType">The target type for the conversion from <see cref="T:System.Security.Principal.SecurityIdentifier" />. The target type must be a type that is considered valid by the <see cref="M:System.Security.Principal.SecurityIdentifier.IsValidTargetType(System.Type)" /> method.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="targetType " />is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="targetType " />is not an <see cref="T:System.Security.Principal.IdentityReference" /> type.</exception>
	/// <exception cref="T:System.Security.Principal.IdentityNotMappedException">Some or all identity references could not be translated.</exception>
	/// <exception cref="T:System.SystemException">A Win32 error code was returned.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	public override IdentityReference Translate(Type targetType)
	{
		if (targetType == typeof(SecurityIdentifier))
		{
			return this;
		}
		if (targetType == typeof(NTAccount))
		{
			WellKnownAccount wellKnownAccount = WellKnownAccount.LookupBySid(Value);
			if (wellKnownAccount == null || wellKnownAccount.Name == null)
			{
				throw new IdentityNotMappedException("Unable to map SID: " + Value);
			}
			return new NTAccount(wellKnownAccount.Name);
		}
		throw new ArgumentException("Unknown type.", "targetType");
	}

	/// <summary>Compares two <see cref="T:System.Security.Principal.SecurityIdentifier" /> objects to determine whether they are equal. They are considered equal if they have the same canonical representation as the one returned by the <see cref="P:System.Security.Principal.SecurityIdentifier.Value" /> property or if they are both null. </summary>
	/// <returns>true if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise, false.</returns>
	/// <param name="left">The left operand to use for the equality comparison. This parameter can be null.</param>
	/// <param name="right">The right operand to use for the equality comparison. This parameter can be null.</param>
	public static bool operator ==(SecurityIdentifier left, SecurityIdentifier right)
	{
		if ((object)left == null)
		{
			return (object)right == null;
		}
		if ((object)right == null)
		{
			return false;
		}
		return left.Value == right.Value;
	}

	/// <summary>Compares two <see cref="T:System.Security.Principal.SecurityIdentifier" /> objects to determine whether they are not equal. They are considered not equal if they have different canonical name representations than the one returned by the <see cref="P:System.Security.Principal.SecurityIdentifier.Value" /> property or if one of the objects is null and the other is not.</summary>
	/// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
	/// <param name="left">The left operand to use for the inequality comparison. This parameter can be null.</param>
	/// <param name="right">The right operand to use for the inequality comparison. This parameter can be null.</param>
	public static bool operator !=(SecurityIdentifier left, SecurityIdentifier right)
	{
		if ((object)left == null)
		{
			return (object)right != null;
		}
		if ((object)right == null)
		{
			return true;
		}
		return left.Value != right.Value;
	}

	internal string GetSddlForm()
	{
		string value = Value;
		WellKnownAccount wellKnownAccount = WellKnownAccount.LookupBySid(value);
		if (wellKnownAccount == null || wellKnownAccount.SddlForm == null)
		{
			return value;
		}
		return wellKnownAccount.SddlForm;
	}

	internal static SecurityIdentifier ParseSddlForm(string sddlForm, ref int pos)
	{
		if (sddlForm.Length - pos < 2)
		{
			throw new ArgumentException("Invalid SDDL string.", "sddlForm");
		}
		string text = sddlForm.Substring(pos, 2).ToUpperInvariant();
		string sddlForm2;
		int num2;
		if (text == "S-")
		{
			int num = pos;
			char c = char.ToUpperInvariant(sddlForm[num]);
			while (true)
			{
				switch (c)
				{
				case '-':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
				case 'S':
				case 'X':
					goto IL_004b;
				default:
					if (c >= 'A' && c <= 'F')
					{
						goto IL_004b;
					}
					break;
				}
				break;
				IL_004b:
				num++;
				c = char.ToUpperInvariant(sddlForm[num]);
			}
			sddlForm2 = sddlForm.Substring(pos, num - pos);
			num2 = num - pos;
		}
		else
		{
			sddlForm2 = text;
			num2 = 2;
		}
		SecurityIdentifier result = new SecurityIdentifier(sddlForm2);
		pos += num2;
		return result;
	}

	private static byte[] ParseSddlForm(string sddlForm)
	{
		string text = sddlForm;
		if (sddlForm.Length == 2)
		{
			WellKnownAccount wellKnownAccount = WellKnownAccount.LookupBySddlForm(sddlForm);
			if (wellKnownAccount == null)
			{
				throw new ArgumentException("Invalid SDDL string - unrecognized account: " + sddlForm, "sddlForm");
			}
			if (!wellKnownAccount.IsAbsolute)
			{
				throw new NotImplementedException("Mono unable to convert account to SID: " + ((wellKnownAccount.Name != null) ? wellKnownAccount.Name : sddlForm));
			}
			text = wellKnownAccount.Sid;
		}
		string[] array = text.ToUpperInvariant().Split('-');
		int num = array.Length - 3;
		if (array.Length < 3 || array[0] != "S" || num > 15)
		{
			throw new ArgumentException("Value was invalid.");
		}
		if (array[1] != "1")
		{
			throw new ArgumentException("Only SIDs with revision 1 are supported");
		}
		byte[] array2 = new byte[8 + num * 4];
		array2[0] = 1;
		array2[1] = (byte)num;
		if (!TryParseAuthority(array[2], out var result))
		{
			throw new ArgumentException("Value was invalid.");
		}
		array2[2] = (byte)((result >> 40) & 0xFF);
		array2[3] = (byte)((result >> 32) & 0xFF);
		array2[4] = (byte)((result >> 24) & 0xFF);
		array2[5] = (byte)((result >> 16) & 0xFF);
		array2[6] = (byte)((result >> 8) & 0xFF);
		array2[7] = (byte)(result & 0xFF);
		for (int i = 0; i < num; i++)
		{
			if (!TryParseSubAuthority(array[i + 3], out var result2))
			{
				throw new ArgumentException("Value was invalid.");
			}
			int num2 = 8 + i * 4;
			array2[num2] = (byte)result2;
			array2[num2 + 1] = (byte)(result2 >> 8);
			array2[num2 + 2] = (byte)(result2 >> 16);
			array2[num2 + 3] = (byte)(result2 >> 24);
		}
		return array2;
	}

	private static bool TryParseAuthority(string s, out ulong result)
	{
		if (s.StartsWith("0X"))
		{
			return ulong.TryParse(s.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result);
		}
		return ulong.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
	}

	private static bool TryParseSubAuthority(string s, out uint result)
	{
		if (s.StartsWith("0X"))
		{
			return uint.TryParse(s.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result);
		}
		return uint.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
	}
}
