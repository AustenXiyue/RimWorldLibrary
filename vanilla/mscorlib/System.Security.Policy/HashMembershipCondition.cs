using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using Mono.Security.Cryptography;

namespace System.Security.Policy;

/// <summary>Determines whether an assembly belongs to a code group by testing its hash value. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class HashMembershipCondition : IMembershipCondition, ISecurityEncodable, ISecurityPolicyEncodable, IDeserializationCallback, ISerializable
{
	private readonly int version = 1;

	private HashAlgorithm hash_algorithm;

	private byte[] hash_value;

	/// <summary>Gets or sets the hash algorithm to use for the membership condition.</summary>
	/// <returns>The hash algorithm to use for the membership condition.</returns>
	/// <exception cref="T:System.ArgumentNullException">An attempt is made to set <see cref="P:System.Security.Policy.HashMembershipCondition.HashAlgorithm" /> to null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public HashAlgorithm HashAlgorithm
	{
		get
		{
			if (hash_algorithm == null)
			{
				hash_algorithm = new SHA1Managed();
			}
			return hash_algorithm;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("HashAlgorithm");
			}
			hash_algorithm = value;
		}
	}

	/// <summary>Gets or sets the hash value for which the membership condition tests.</summary>
	/// <returns>The hash value for which the membership condition tests.</returns>
	/// <exception cref="T:System.ArgumentNullException">An attempt is made to set <see cref="P:System.Security.Policy.HashMembershipCondition.HashValue" /> to null. </exception>
	public byte[] HashValue
	{
		get
		{
			if (hash_value == null)
			{
				throw new ArgumentException(Locale.GetText("No HashValue available."));
			}
			return (byte[])hash_value.Clone();
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("HashValue");
			}
			hash_value = (byte[])value.Clone();
		}
	}

	internal HashMembershipCondition()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Policy.HashMembershipCondition" /> class with the hash algorithm and hash value that determine membership.</summary>
	/// <param name="hashAlg">The hash algorithm to use to compute the hash value for the assembly. </param>
	/// <param name="value">The hash value for which to test. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="hashAlg" /> parameter is null.-or- The <paramref name="value" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="hashAlg" /> parameter is not a valid hash algorithm. </exception>
	public HashMembershipCondition(HashAlgorithm hashAlg, byte[] value)
	{
		if (hashAlg == null)
		{
			throw new ArgumentNullException("hashAlg");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		hash_algorithm = hashAlg;
		hash_value = (byte[])value.Clone();
	}

	/// <summary>Determines whether the specified evidence satisfies the membership condition.</summary>
	/// <returns>true if the specified evidence satisfies the membership condition; otherwise, false.</returns>
	/// <param name="evidence">The evidence set against which to make the test. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public bool Check(Evidence evidence)
	{
		if (evidence == null)
		{
			return false;
		}
		IEnumerator hostEnumerator = evidence.GetHostEnumerator();
		while (hostEnumerator.MoveNext())
		{
			if (hostEnumerator.Current is Hash hash)
			{
				if (!Compare(hash_value, hash.GenerateHash(hash_algorithm)))
				{
					break;
				}
				return true;
			}
		}
		return false;
	}

	/// <summary>Creates an equivalent copy of the membership condition.</summary>
	/// <returns>A new, identical copy of the current membership condition.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public IMembershipCondition Copy()
	{
		return new HashMembershipCondition(hash_algorithm, hash_value);
	}

	/// <summary>Determines whether the <see cref="P:System.Security.Policy.HashMembershipCondition.HashValue" /> and the <see cref="P:System.Security.Policy.HashMembershipCondition.HashAlgorithm" /> from the specified object are equivalent to the <see cref="P:System.Security.Policy.HashMembershipCondition.HashValue" /> and <see cref="P:System.Security.Policy.HashMembershipCondition.HashAlgorithm" /> contained in the current <see cref="T:System.Security.Policy.HashMembershipCondition" />.</summary>
	/// <returns>true if the <see cref="P:System.Security.Policy.HashMembershipCondition.HashValue" /> and <see cref="P:System.Security.Policy.HashMembershipCondition.HashAlgorithm" /> from the specified object is equivalent to the <see cref="P:System.Security.Policy.HashMembershipCondition.HashValue" /> and <see cref="P:System.Security.Policy.HashMembershipCondition.HashAlgorithm" /> contained in the current <see cref="T:System.Security.Policy.HashMembershipCondition" />; otherwise, false.</returns>
	/// <param name="o">The object to compare to the current <see cref="T:System.Security.Policy.HashMembershipCondition" />. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override bool Equals(object o)
	{
		if (!(o is HashMembershipCondition hashMembershipCondition))
		{
			return false;
		}
		if (hashMembershipCondition.HashAlgorithm == hash_algorithm)
		{
			return Compare(hash_value, hashMembershipCondition.hash_value);
		}
		return false;
	}

	/// <summary>Creates an XML encoding of the security object and its current state.</summary>
	/// <returns>An XML encoding of the security object, including any state information.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public SecurityElement ToXml()
	{
		return ToXml(null);
	}

	/// <summary>Creates an XML encoding of the security object and its current state with the specified <see cref="T:System.Security.Policy.PolicyLevel" />.</summary>
	/// <returns>An XML encoding of the security object, including any state information.</returns>
	/// <param name="level">The policy level context for resolving named permission set references. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public SecurityElement ToXml(PolicyLevel level)
	{
		SecurityElement securityElement = MembershipConditionHelper.Element(typeof(HashMembershipCondition), version);
		securityElement.AddAttribute("HashValue", CryptoConvert.ToHex(HashValue));
		securityElement.AddAttribute("HashAlgorithm", hash_algorithm.GetType().FullName);
		return securityElement;
	}

	/// <summary>Reconstructs a security object with a specified state from an XML encoding.</summary>
	/// <param name="e">The XML encoding to use to reconstruct the security object. </param>
	public void FromXml(SecurityElement e)
	{
		FromXml(e, null);
	}

	/// <summary>Reconstructs a security object with a specified state from an XML encoding.</summary>
	/// <param name="e">The XML encoding to use to reconstruct the security object. </param>
	/// <param name="level">The policy level context, used to resolve named permission set references. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="e" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="e" /> parameter is not a valid membership condition element. </exception>
	public void FromXml(SecurityElement e, PolicyLevel level)
	{
		MembershipConditionHelper.CheckSecurityElement(e, "e", version, version);
		hash_value = CryptoConvert.FromHex(e.Attribute("HashValue"));
		string text = e.Attribute("HashAlgorithm");
		hash_algorithm = ((text == null) ? null : HashAlgorithm.Create(text));
	}

	/// <summary>Gets the hash code for the current membership condition.</summary>
	/// <returns>The hash code for the current membership condition.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override int GetHashCode()
	{
		int num = hash_algorithm.GetType().GetHashCode();
		if (hash_value != null)
		{
			byte[] array = hash_value;
			foreach (byte b in array)
			{
				num ^= b;
			}
		}
		return num;
	}

	/// <summary>Creates and returns a string representation of the membership condition.</summary>
	/// <returns>A string representation of the state of the membership condition.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override string ToString()
	{
		Type type = HashAlgorithm.GetType();
		return $"Hash - {type.FullName} {type.Assembly} = {CryptoConvert.ToHex(HashValue)}";
	}

	private bool Compare(byte[] expected, byte[] actual)
	{
		if (expected.Length != actual.Length)
		{
			return false;
		}
		int num = expected.Length;
		for (int i = 0; i < num; i++)
		{
			if (expected[i] != actual[i])
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Runs when the entire object graph has been deserialized.</summary>
	/// <param name="sender">The object that initiated the callback. The functionality for this parameter is not currently implemented.</param>
	[MonoTODO("fx 2.0")]
	void IDeserializationCallback.OnDeserialization(object sender)
	{
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data needed to serialize the target object.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data. </param>
	/// <param name="context">The destination <see cref="T:System.Runtime.Serialization.StreamingContext" /> for this serialization. </param>
	[MonoTODO("fx 2.0")]
	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
	}
}
