using System.IO;
using System.Runtime.InteropServices;
using System.Security.Util;
using System.Text;

namespace System.Security.Cryptography;

/// <summary>Represents the base class from which all implementations of the <see cref="T:System.Security.Cryptography.RSA" /> algorithm inherit.</summary>
[ComVisible(true)]
public abstract class RSA : AsymmetricAlgorithm
{
	public override string KeyExchangeAlgorithm => "RSA";

	public override string SignatureAlgorithm => "RSA";

	/// <summary>Initializes a new instance of <see cref="T:System.Security.Cryptography.RSA" />.</summary>
	protected RSA()
	{
	}

	/// <summary>Creates an instance of the default implementation of the <see cref="T:System.Security.Cryptography.RSA" /> algorithm.</summary>
	/// <returns>A new instance of the default implementation of <see cref="T:System.Security.Cryptography.RSA" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public new static RSA Create()
	{
		return Create("System.Security.Cryptography.RSA");
	}

	/// <summary>Creates an instance of the specified implementation of <see cref="T:System.Security.Cryptography.RSA" />.</summary>
	/// <returns>A new instance of the specified implementation of <see cref="T:System.Security.Cryptography.RSA" />.</returns>
	/// <param name="algName">The name of the implementation of <see cref="T:System.Security.Cryptography.RSA" /> to use. </param>
	public new static RSA Create(string algName)
	{
		return (RSA)CryptoConfig.CreateFromName(algName);
	}

	public virtual byte[] Encrypt(byte[] data, RSAEncryptionPadding padding)
	{
		throw DerivedClassMustOverride();
	}

	public virtual byte[] Decrypt(byte[] data, RSAEncryptionPadding padding)
	{
		throw DerivedClassMustOverride();
	}

	public virtual byte[] SignHash(byte[] hash, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding)
	{
		throw DerivedClassMustOverride();
	}

	public virtual bool VerifyHash(byte[] hash, byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding)
	{
		throw DerivedClassMustOverride();
	}

	protected virtual byte[] HashData(byte[] data, int offset, int count, HashAlgorithmName hashAlgorithm)
	{
		throw DerivedClassMustOverride();
	}

	protected virtual byte[] HashData(Stream data, HashAlgorithmName hashAlgorithm)
	{
		throw DerivedClassMustOverride();
	}

	public byte[] SignData(byte[] data, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return SignData(data, 0, data.Length, hashAlgorithm, padding);
	}

	public virtual byte[] SignData(byte[] data, int offset, int count, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (offset < 0 || offset > data.Length)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (count < 0 || count > data.Length - offset)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (string.IsNullOrEmpty(hashAlgorithm.Name))
		{
			throw HashAlgorithmNameNullOrEmpty();
		}
		if (padding == null)
		{
			throw new ArgumentNullException("padding");
		}
		byte[] hash = HashData(data, offset, count, hashAlgorithm);
		return SignHash(hash, hashAlgorithm, padding);
	}

	public virtual byte[] SignData(Stream data, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (string.IsNullOrEmpty(hashAlgorithm.Name))
		{
			throw HashAlgorithmNameNullOrEmpty();
		}
		if (padding == null)
		{
			throw new ArgumentNullException("padding");
		}
		byte[] hash = HashData(data, hashAlgorithm);
		return SignHash(hash, hashAlgorithm, padding);
	}

	public bool VerifyData(byte[] data, byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return VerifyData(data, 0, data.Length, signature, hashAlgorithm, padding);
	}

	public virtual bool VerifyData(byte[] data, int offset, int count, byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (offset < 0 || offset > data.Length)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (count < 0 || count > data.Length - offset)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (signature == null)
		{
			throw new ArgumentNullException("signature");
		}
		if (string.IsNullOrEmpty(hashAlgorithm.Name))
		{
			throw HashAlgorithmNameNullOrEmpty();
		}
		if (padding == null)
		{
			throw new ArgumentNullException("padding");
		}
		byte[] hash = HashData(data, offset, count, hashAlgorithm);
		return VerifyHash(hash, signature, hashAlgorithm, padding);
	}

	public bool VerifyData(Stream data, byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (signature == null)
		{
			throw new ArgumentNullException("signature");
		}
		if (string.IsNullOrEmpty(hashAlgorithm.Name))
		{
			throw HashAlgorithmNameNullOrEmpty();
		}
		if (padding == null)
		{
			throw new ArgumentNullException("padding");
		}
		byte[] hash = HashData(data, hashAlgorithm);
		return VerifyHash(hash, signature, hashAlgorithm, padding);
	}

	private static Exception DerivedClassMustOverride()
	{
		return new NotImplementedException(Environment.GetResourceString("Derived classes must provide an implementation."));
	}

	internal static Exception HashAlgorithmNameNullOrEmpty()
	{
		return new ArgumentException(Environment.GetResourceString("The hash algorithm name cannot be null or empty."), "hashAlgorithm");
	}

	/// <summary>When overridden in a derived class, decrypts the input data using the private key.</summary>
	/// <returns>The resulting decryption of the <paramref name="rgb" /> parameter in plain text.</returns>
	/// <param name="rgb">The cipher text to be decrypted. </param>
	public virtual byte[] DecryptValue(byte[] rgb)
	{
		throw new NotSupportedException(Environment.GetResourceString("Method is not supported."));
	}

	/// <summary>When overridden in a derived class, encrypts the input data using the public key.</summary>
	/// <returns>The resulting encryption of the <paramref name="rgb" /> parameter as cipher text.</returns>
	/// <param name="rgb">The plain text to be encrypted. </param>
	public virtual byte[] EncryptValue(byte[] rgb)
	{
		throw new NotSupportedException(Environment.GetResourceString("Method is not supported."));
	}

	/// <summary>Initializes an <see cref="T:System.Security.Cryptography.RSA" /> object from the key information from an XML string.</summary>
	/// <param name="xmlString">The XML string containing <see cref="T:System.Security.Cryptography.RSA" /> key information. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="xmlString" /> parameter is null. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The format of the <paramref name="xmlString" /> parameter is not valid. </exception>
	public override void FromXmlString(string xmlString)
	{
		if (xmlString == null)
		{
			throw new ArgumentNullException("xmlString");
		}
		RSAParameters parameters = default(RSAParameters);
		SecurityElement topElement = new Parser(xmlString).GetTopElement();
		string text = topElement.SearchForTextOfLocalName("Modulus");
		if (text == null)
		{
			throw new CryptographicException(Environment.GetResourceString("Input string does not contain a valid encoding of the '{0}' '{1}' parameter.", "RSA", "Modulus"));
		}
		parameters.Modulus = Convert.FromBase64String(Utils.DiscardWhiteSpaces(text));
		string text2 = topElement.SearchForTextOfLocalName("Exponent");
		if (text2 == null)
		{
			throw new CryptographicException(Environment.GetResourceString("Input string does not contain a valid encoding of the '{0}' '{1}' parameter.", "RSA", "Exponent"));
		}
		parameters.Exponent = Convert.FromBase64String(Utils.DiscardWhiteSpaces(text2));
		string text3 = topElement.SearchForTextOfLocalName("P");
		if (text3 != null)
		{
			parameters.P = Convert.FromBase64String(Utils.DiscardWhiteSpaces(text3));
		}
		string text4 = topElement.SearchForTextOfLocalName("Q");
		if (text4 != null)
		{
			parameters.Q = Convert.FromBase64String(Utils.DiscardWhiteSpaces(text4));
		}
		string text5 = topElement.SearchForTextOfLocalName("DP");
		if (text5 != null)
		{
			parameters.DP = Convert.FromBase64String(Utils.DiscardWhiteSpaces(text5));
		}
		string text6 = topElement.SearchForTextOfLocalName("DQ");
		if (text6 != null)
		{
			parameters.DQ = Convert.FromBase64String(Utils.DiscardWhiteSpaces(text6));
		}
		string text7 = topElement.SearchForTextOfLocalName("InverseQ");
		if (text7 != null)
		{
			parameters.InverseQ = Convert.FromBase64String(Utils.DiscardWhiteSpaces(text7));
		}
		string text8 = topElement.SearchForTextOfLocalName("D");
		if (text8 != null)
		{
			parameters.D = Convert.FromBase64String(Utils.DiscardWhiteSpaces(text8));
		}
		ImportParameters(parameters);
	}

	/// <summary>Creates and returns an XML string containing the key of the current <see cref="T:System.Security.Cryptography.RSA" /> object.</summary>
	/// <returns>An XML string containing the key of the current <see cref="T:System.Security.Cryptography.RSA" /> object.</returns>
	/// <param name="includePrivateParameters">true to include a public and private RSA key; false to include only the public key. </param>
	public override string ToXmlString(bool includePrivateParameters)
	{
		RSAParameters rSAParameters = ExportParameters(includePrivateParameters);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("<RSAKeyValue>");
		stringBuilder.Append("<Modulus>" + Convert.ToBase64String(rSAParameters.Modulus) + "</Modulus>");
		stringBuilder.Append("<Exponent>" + Convert.ToBase64String(rSAParameters.Exponent) + "</Exponent>");
		if (includePrivateParameters)
		{
			stringBuilder.Append("<P>" + Convert.ToBase64String(rSAParameters.P) + "</P>");
			stringBuilder.Append("<Q>" + Convert.ToBase64String(rSAParameters.Q) + "</Q>");
			stringBuilder.Append("<DP>" + Convert.ToBase64String(rSAParameters.DP) + "</DP>");
			stringBuilder.Append("<DQ>" + Convert.ToBase64String(rSAParameters.DQ) + "</DQ>");
			stringBuilder.Append("<InverseQ>" + Convert.ToBase64String(rSAParameters.InverseQ) + "</InverseQ>");
			stringBuilder.Append("<D>" + Convert.ToBase64String(rSAParameters.D) + "</D>");
		}
		stringBuilder.Append("</RSAKeyValue>");
		return stringBuilder.ToString();
	}

	/// <summary>When overridden in a derived class, exports the <see cref="T:System.Security.Cryptography.RSAParameters" />.</summary>
	/// <returns>The parameters for <see cref="T:System.Security.Cryptography.DSA" />.</returns>
	/// <param name="includePrivateParameters">true to include private parameters; otherwise, false. </param>
	public abstract RSAParameters ExportParameters(bool includePrivateParameters);

	/// <summary>When overridden in a derived class, imports the specified <see cref="T:System.Security.Cryptography.RSAParameters" />.</summary>
	/// <param name="parameters">The parameters for <see cref="T:System.Security.Cryptography.RSA" />. </param>
	public abstract void ImportParameters(RSAParameters parameters);
}
