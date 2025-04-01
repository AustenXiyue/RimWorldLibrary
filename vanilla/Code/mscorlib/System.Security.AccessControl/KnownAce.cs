using System.Globalization;
using System.Security.Principal;
using System.Text;
using Unity;

namespace System.Security.AccessControl;

/// <summary>Encapsulates all Access Control Entry (ACE) types currently defined by Microsoft Corporation. All <see cref="T:System.Security.AccessControl.KnownAce" /> objects contain a 32-bit access mask and a <see cref="T:System.Security.Principal.SecurityIdentifier" /> object.</summary>
public abstract class KnownAce : GenericAce
{
	private int access_mask;

	private SecurityIdentifier identifier;

	/// <summary>Gets or sets the access mask for this <see cref="T:System.Security.AccessControl.KnownAce" /> object.</summary>
	/// <returns>The access mask for this <see cref="T:System.Security.AccessControl.KnownAce" /> object.</returns>
	public int AccessMask
	{
		get
		{
			return access_mask;
		}
		set
		{
			access_mask = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Security.Principal.SecurityIdentifier" /> object associated with this <see cref="T:System.Security.AccessControl.KnownAce" /> object.</summary>
	/// <returns>The <see cref="T:System.Security.Principal.SecurityIdentifier" /> object associated with this <see cref="T:System.Security.AccessControl.KnownAce" /> object.</returns>
	public SecurityIdentifier SecurityIdentifier
	{
		get
		{
			return identifier;
		}
		set
		{
			identifier = value;
		}
	}

	internal KnownAce(AceType type, AceFlags flags)
		: base(type, flags)
	{
	}

	internal KnownAce(byte[] binaryForm, int offset)
		: base(binaryForm, offset)
	{
	}

	internal static string GetSddlAccessRights(int accessMask)
	{
		string sddlAliasRights = GetSddlAliasRights(accessMask);
		if (!string.IsNullOrEmpty(sddlAliasRights))
		{
			return sddlAliasRights;
		}
		return string.Format(CultureInfo.InvariantCulture, "0x{0:x}", accessMask);
	}

	private static string GetSddlAliasRights(int accessMask)
	{
		SddlAccessRight[] array = SddlAccessRight.Decompose(accessMask);
		if (array == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		SddlAccessRight[] array2 = array;
		foreach (SddlAccessRight sddlAccessRight in array2)
		{
			stringBuilder.Append(sddlAccessRight.Name);
		}
		return stringBuilder.ToString();
	}

	internal KnownAce()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
