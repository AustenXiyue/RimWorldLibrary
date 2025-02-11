using System.Security.Permissions;

namespace System.Security.Cryptography;

/// <summary>Specifies a key BLOB format for use with Microsoft Cryptography Next Generation (CNG) objects. </summary>
[Serializable]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class CngKeyBlobFormat : IEquatable<CngKeyBlobFormat>
{
	private static volatile CngKeyBlobFormat s_eccPrivate;

	private static volatile CngKeyBlobFormat s_eccPublic;

	private static volatile CngKeyBlobFormat s_eccFullPrivate;

	private static volatile CngKeyBlobFormat s_eccFullPublic;

	private static volatile CngKeyBlobFormat s_genericPrivate;

	private static volatile CngKeyBlobFormat s_genericPublic;

	private static volatile CngKeyBlobFormat s_opaqueTransport;

	private static volatile CngKeyBlobFormat s_pkcs8Private;

	private string m_format;

	/// <summary>Gets the name of the key BLOB format that the current <see cref="T:System.Security.Cryptography.CngKeyBlobFormat" /> object specifies.</summary>
	/// <returns>The embedded key BLOB format name.</returns>
	public string Format => m_format;

	/// <summary>Gets a <see cref="T:System.Security.Cryptography.CngKeyBlobFormat" /> object that specifies a private key BLOB for an elliptic curve cryptography (ECC) key.</summary>
	/// <returns>An object that specifies an ECC private key BLOB.</returns>
	public static CngKeyBlobFormat EccPrivateBlob
	{
		get
		{
			if (s_eccPrivate == null)
			{
				s_eccPrivate = new CngKeyBlobFormat("ECCPRIVATEBLOB");
			}
			return s_eccPrivate;
		}
	}

	/// <summary>Gets a <see cref="T:System.Security.Cryptography.CngKeyBlobFormat" /> object that specifies a public key BLOB for an elliptic curve cryptography (ECC) key.</summary>
	/// <returns>An object that specifies an ECC public key BLOB.</returns>
	public static CngKeyBlobFormat EccPublicBlob
	{
		get
		{
			if (s_eccPublic == null)
			{
				s_eccPublic = new CngKeyBlobFormat("ECCPUBLICBLOB");
			}
			return s_eccPublic;
		}
	}

	public static CngKeyBlobFormat EccFullPrivateBlob
	{
		get
		{
			if (s_eccFullPrivate == null)
			{
				s_eccFullPrivate = new CngKeyBlobFormat("ECCFULLPRIVATEBLOB");
			}
			return s_eccFullPrivate;
		}
	}

	public static CngKeyBlobFormat EccFullPublicBlob
	{
		get
		{
			if (s_eccFullPublic == null)
			{
				s_eccFullPublic = new CngKeyBlobFormat("ECCFULLPUBLICBLOB");
			}
			return s_eccFullPublic;
		}
	}

	/// <summary>Gets a <see cref="T:System.Security.Cryptography.CngKeyBlobFormat" /> object that specifies a generic private key BLOB.</summary>
	/// <returns>An object that specifies a generic private key BLOB.</returns>
	public static CngKeyBlobFormat GenericPrivateBlob
	{
		get
		{
			if (s_genericPrivate == null)
			{
				s_genericPrivate = new CngKeyBlobFormat("PRIVATEBLOB");
			}
			return s_genericPrivate;
		}
	}

	/// <summary>Gets a <see cref="T:System.Security.Cryptography.CngKeyBlobFormat" /> object that specifies a generic public key BLOB.</summary>
	/// <returns>An object that specifies a generic public key BLOB.</returns>
	public static CngKeyBlobFormat GenericPublicBlob
	{
		get
		{
			if (s_genericPublic == null)
			{
				s_genericPublic = new CngKeyBlobFormat("PUBLICBLOB");
			}
			return s_genericPublic;
		}
	}

	/// <summary>Gets a <see cref="T:System.Security.Cryptography.CngKeyBlobFormat" /> object that specifies an opaque transport key BLOB.</summary>
	/// <returns>An object that specifies an opaque transport key BLOB.</returns>
	public static CngKeyBlobFormat OpaqueTransportBlob
	{
		get
		{
			if (s_opaqueTransport == null)
			{
				s_opaqueTransport = new CngKeyBlobFormat("OpaqueTransport");
			}
			return s_opaqueTransport;
		}
	}

	/// <summary>Gets a <see cref="T:System.Security.Cryptography.CngKeyBlobFormat" /> object that specifies a Private Key Information Syntax Standard (PKCS #8) key BLOB.</summary>
	/// <returns>An object that specifies a PKCS #8 private key BLOB.</returns>
	public static CngKeyBlobFormat Pkcs8PrivateBlob
	{
		get
		{
			if (s_pkcs8Private == null)
			{
				s_pkcs8Private = new CngKeyBlobFormat("PKCS8_PRIVATEKEY");
			}
			return s_pkcs8Private;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.CngKeyBlobFormat" /> class by using the specified format.</summary>
	/// <param name="format">The key BLOB format to initialize.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="format" /> parameter is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="format" /> parameter length is 0 (zero).</exception>
	public CngKeyBlobFormat(string format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (format.Length == 0)
		{
			throw new ArgumentException(global::SR.GetString("The key blob format '{0}' is invalid.", format), "format");
		}
		m_format = format;
	}

	/// <summary>Determines whether two <see cref="T:System.Security.Cryptography.CngKeyBlobFormat" /> objects specify the same key BLOB format.</summary>
	/// <returns>true if the two objects specify the same key BLOB format; otherwise, false.</returns>
	/// <param name="left">An object that specifies a key BLOB format.</param>
	/// <param name="right">A second object, to be compared to the object identified by the <paramref name="left" /> parameter.</param>
	public static bool operator ==(CngKeyBlobFormat left, CngKeyBlobFormat right)
	{
		return left?.Equals(right) ?? ((object)right == null);
	}

	/// <summary>Determines whether two <see cref="T:System.Security.Cryptography.CngKeyBlobFormat" /> objects do not specify the same key BLOB format.</summary>
	/// <returns>true if the two objects do not specify the same key BLOB format; otherwise, false.</returns>
	/// <param name="left">An object that specifies a key BLOB format.</param>
	/// <param name="right">A second object, to be compared to the object identified by the <paramref name="left" /> parameter.</param>
	public static bool operator !=(CngKeyBlobFormat left, CngKeyBlobFormat right)
	{
		if ((object)left == null)
		{
			return (object)right != null;
		}
		return !left.Equals(right);
	}

	/// <summary>Compares the specified object to the current <see cref="T:System.Security.Cryptography.CngKeyBlobFormat" /> object.</summary>
	/// <returns>true if the <paramref name="obj" /> parameter is a <see cref="T:System.Security.Cryptography.CngKeyBlobFormat" /> object that specifies the same key BLOB format as the current object; otherwise, false.</returns>
	/// <param name="obj">An object to be compared to the current <see cref="T:System.Security.Cryptography.CngKeyBlobFormat" /> object.</param>
	public override bool Equals(object obj)
	{
		return Equals(obj as CngKeyBlobFormat);
	}

	/// <summary>Compares the specified <see cref="T:System.Security.Cryptography.CngKeyBlobFormat" /> object to the current <see cref="T:System.Security.Cryptography.CngKeyBlobFormat" /> object.</summary>
	/// <returns>true if the <paramref name="other" /> parameter specifies the same key BLOB format as the current object; otherwise, false.</returns>
	/// <param name="other">An object to be compared to the current <see cref="T:System.Security.Cryptography.CngKeyBlobFormat" /> object.</param>
	public bool Equals(CngKeyBlobFormat other)
	{
		if ((object)other == null)
		{
			return false;
		}
		return m_format.Equals(other.Format);
	}

	/// <summary>Generates a hash value for the embedded key BLOB format in the current <see cref="T:System.Security.Cryptography.CngKeyBlobFormat" /> object.</summary>
	/// <returns>The hash value of the embedded key BLOB format. </returns>
	public override int GetHashCode()
	{
		return m_format.GetHashCode();
	}

	/// <summary>Gets the name of the key BLOB format that the current <see cref="T:System.Security.Cryptography.CngKeyBlobFormat" /> object specifies.</summary>
	/// <returns>The embedded key BLOB format name.</returns>
	public override string ToString()
	{
		return m_format;
	}
}
