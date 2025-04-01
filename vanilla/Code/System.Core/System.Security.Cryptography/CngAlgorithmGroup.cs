using System.Security.Permissions;

namespace System.Security.Cryptography;

/// <summary>Encapsulates the name of an encryption algorithm group. </summary>
[Serializable]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class CngAlgorithmGroup : IEquatable<CngAlgorithmGroup>
{
	private static volatile CngAlgorithmGroup s_dh;

	private static volatile CngAlgorithmGroup s_dsa;

	private static volatile CngAlgorithmGroup s_ecdh;

	private static volatile CngAlgorithmGroup s_ecdsa;

	private static volatile CngAlgorithmGroup s_rsa;

	private string m_algorithmGroup;

	/// <summary>Gets the name of the algorithm group that the current <see cref="T:System.Security.Cryptography.CngAlgorithm" /> object specifies.</summary>
	/// <returns>The embedded algorithm group name.</returns>
	public string AlgorithmGroup => m_algorithmGroup;

	/// <summary>Gets a <see cref="T:System.Security.Cryptography.CngAlgorithmGroup" /> object that specifies the Diffie-Hellman family of algorithms.</summary>
	/// <returns>An object that specifies the Diffie-Hellman family of algorithms.</returns>
	public static CngAlgorithmGroup DiffieHellman
	{
		get
		{
			if (s_dh == null)
			{
				s_dh = new CngAlgorithmGroup("DH");
			}
			return s_dh;
		}
	}

	/// <summary>Gets a <see cref="T:System.Security.Cryptography.CngAlgorithmGroup" /> object that specifies the Digital Signature Algorithm (DSA) family of algorithms.</summary>
	/// <returns>An object that specifies the DSA family of algorithms.</returns>
	public static CngAlgorithmGroup Dsa
	{
		get
		{
			if (s_dsa == null)
			{
				s_dsa = new CngAlgorithmGroup("DSA");
			}
			return s_dsa;
		}
	}

	/// <summary>Gets a <see cref="T:System.Security.Cryptography.CngAlgorithmGroup" /> object that specifies the Elliptic Curve Diffie-Hellman (ECDH) family of algorithms.</summary>
	/// <returns>An object that specifies the ECDH family of algorithms.</returns>
	public static CngAlgorithmGroup ECDiffieHellman
	{
		get
		{
			if (s_ecdh == null)
			{
				s_ecdh = new CngAlgorithmGroup("ECDH");
			}
			return s_ecdh;
		}
	}

	/// <summary>Gets a <see cref="T:System.Security.Cryptography.CngAlgorithmGroup" /> object that specifies the Elliptic Curve Digital Signature Algorithm (ECDSA) family of algorithms.</summary>
	/// <returns>An object that specifies the ECDSA family of algorithms.</returns>
	public static CngAlgorithmGroup ECDsa
	{
		get
		{
			if (s_ecdsa == null)
			{
				s_ecdsa = new CngAlgorithmGroup("ECDSA");
			}
			return s_ecdsa;
		}
	}

	/// <summary>Gets a <see cref="T:System.Security.Cryptography.CngAlgorithmGroup" /> object that specifies the Rivest-Shamir-Adleman (RSA) family of algorithms.</summary>
	/// <returns>An object that specifies the RSA family of algorithms.</returns>
	public static CngAlgorithmGroup Rsa
	{
		get
		{
			if (s_rsa == null)
			{
				s_rsa = new CngAlgorithmGroup("RSA");
			}
			return s_rsa;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.CngAlgorithmGroup" /> class.</summary>
	/// <param name="algorithmGroup">The name of the algorithm group to initialize.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="algorithmGroup" /> parameter is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="algorithmGroup" /> parameter length is 0 (zero).</exception>
	public CngAlgorithmGroup(string algorithmGroup)
	{
		if (algorithmGroup == null)
		{
			throw new ArgumentNullException("algorithmGroup");
		}
		if (algorithmGroup.Length == 0)
		{
			throw new ArgumentException(global::SR.GetString("The algorithm group '{0}' is invalid.", algorithmGroup), "algorithmGroup");
		}
		m_algorithmGroup = algorithmGroup;
	}

	/// <summary>Determines whether two <see cref="T:System.Security.Cryptography.CngAlgorithmGroup" /> objects specify the same algorithm group.</summary>
	/// <returns>true if the two objects specify the same algorithm group; otherwise, false.</returns>
	/// <param name="left">An object that specifies an algorithm group.</param>
	/// <param name="right">A second object, to be compared to the object that is identified by the <paramref name="left" /> parameter.</param>
	public static bool operator ==(CngAlgorithmGroup left, CngAlgorithmGroup right)
	{
		return left?.Equals(right) ?? ((object)right == null);
	}

	/// <summary>Determines whether two <see cref="T:System.Security.Cryptography.CngAlgorithmGroup" /> objects do not specify the same algorithm group.</summary>
	/// <returns>true if the two objects do not specify the same algorithm group; otherwise, false. </returns>
	/// <param name="left">An object that specifies an algorithm group.</param>
	/// <param name="right">A second object, to be compared to the object that is identified by the <paramref name="left" /> parameter.</param>
	public static bool operator !=(CngAlgorithmGroup left, CngAlgorithmGroup right)
	{
		if ((object)left == null)
		{
			return (object)right != null;
		}
		return !left.Equals(right);
	}

	/// <summary>Compares the specified object to the current <see cref="T:System.Security.Cryptography.CngAlgorithmGroup" /> object.</summary>
	/// <returns>true if the <paramref name="obj" /> parameter is a <see cref="T:System.Security.Cryptography.CngAlgorithmGroup" /> that specifies the same algorithm group as the current object; otherwise, false.</returns>
	/// <param name="obj">An object to be compared to the current <see cref="T:System.Security.Cryptography.CngAlgorithmGroup" /> object.</param>
	public override bool Equals(object obj)
	{
		return Equals(obj as CngAlgorithmGroup);
	}

	/// <summary>Compares the specified <see cref="T:System.Security.Cryptography.CngAlgorithmGroup" /> object to the current <see cref="T:System.Security.Cryptography.CngAlgorithmGroup" /> object.</summary>
	/// <returns>true if the <paramref name="other" /> parameter specifies the same algorithm group as the current object; otherwise, false.</returns>
	/// <param name="other">An object to be compared to the current <see cref="T:System.Security.Cryptography.CngAlgorithmGroup" /> object.</param>
	public bool Equals(CngAlgorithmGroup other)
	{
		if ((object)other == null)
		{
			return false;
		}
		return m_algorithmGroup.Equals(other.AlgorithmGroup);
	}

	/// <summary>Generates a hash value for the algorithm group name that is embedded in the current <see cref="T:System.Security.Cryptography.CngAlgorithmGroup" /> object.</summary>
	/// <returns>The hash value of the embedded algorithm group name.</returns>
	public override int GetHashCode()
	{
		return m_algorithmGroup.GetHashCode();
	}

	/// <summary>Gets the name of the algorithm group that the current <see cref="T:System.Security.Cryptography.CngAlgorithm" /> object specifies.</summary>
	/// <returns>The embedded algorithm group name.</returns>
	public override string ToString()
	{
		return m_algorithmGroup;
	}
}
