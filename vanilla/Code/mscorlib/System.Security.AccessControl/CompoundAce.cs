using System.Security.Principal;

namespace System.Security.AccessControl;

/// <summary>Represents a compound Access Control Entry (ACE).</summary>
public sealed class CompoundAce : KnownAce
{
	private CompoundAceType compound_ace_type;

	/// <summary>Gets the length, in bytes, of the binary representation of the current <see cref="T:System.Security.AccessControl.CompoundAce" /> object. This length should be used before marshaling the ACL into a binary array with the <see cref="M:System.Security.AccessControl.CompoundAce.GetBinaryForm" /> method.</summary>
	/// <returns>The length, in bytes, of the binary representation of the current <see cref="T:System.Security.AccessControl.CompoundAce" /> object.</returns>
	[MonoTODO]
	public override int BinaryLength
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets the type of this <see cref="T:System.Security.AccessControl.CompoundAce" /> object.</summary>
	/// <returns>The type of this <see cref="T:System.Security.AccessControl.CompoundAce" /> object.</returns>
	public CompoundAceType CompoundAceType
	{
		get
		{
			return compound_ace_type;
		}
		set
		{
			compound_ace_type = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.CompoundAce" /> class.</summary>
	/// <param name="flags">Contains flags that specify information about the inheritance, inheritance propagation, and auditing conditions for the new Access Control Entry (ACE).</param>
	/// <param name="accessMask">The access mask for the ACE.</param>
	/// <param name="compoundAceType">A value from the <see cref="T:System.Security.AccessControl.CompoundAceType" /> enumeration.</param>
	/// <param name="sid">The <see cref="T:System.Security.Principal.SecurityIdentifier" /> associated with the new ACE.</param>
	public CompoundAce(AceFlags flags, int accessMask, CompoundAceType compoundAceType, SecurityIdentifier sid)
		: base(AceType.AccessAllowedCompound, flags)
	{
		compound_ace_type = compoundAceType;
		base.AccessMask = accessMask;
		base.SecurityIdentifier = sid;
	}

	/// <summary>Marshals the contents of the <see cref="T:System.Security.AccessControl.CompoundAce" /> object into the specified byte array beginning at the specified offset.</summary>
	/// <param name="binaryForm">The byte array into which the contents of the <see cref="T:System.Security.AccessControl.CompoundAce" /> is marshaled.</param>
	/// <param name="offset">The offset at which to start marshaling.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is negative or too high to allow the entire <see cref="T:System.Security.AccessControl.CompoundAce" /> to be copied into <paramref name="array" />.</exception>
	[MonoTODO]
	public override void GetBinaryForm(byte[] binaryForm, int offset)
	{
		throw new NotImplementedException();
	}

	internal override string GetSddlForm()
	{
		throw new NotImplementedException();
	}
}
