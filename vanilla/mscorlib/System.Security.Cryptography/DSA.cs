using System.IO;
using System.Runtime.InteropServices;
using System.Security.Util;
using System.Text;

namespace System.Security.Cryptography;

/// <summary>Represents the abstract base class from which all implementations of the Digital Signature Algorithm (<see cref="T:System.Security.Cryptography.DSA" />) must inherit.</summary>
[ComVisible(true)]
public abstract class DSA : AsymmetricAlgorithm
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.DSA" /> class. </summary>
	protected DSA()
	{
	}

	/// <summary>Creates the default cryptographic object used to perform the asymmetric algorithm.</summary>
	/// <returns>A cryptographic object used to perform the asymmetric algorithm.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public new static DSA Create()
	{
		return Create("System.Security.Cryptography.DSA");
	}

	/// <summary>Creates the specified cryptographic object used to perform the asymmetric algorithm.</summary>
	/// <returns>A cryptographic object used to perform the asymmetric algorithm.</returns>
	/// <param name="algName">The name of the specific implementation of <see cref="T:System.Security.Cryptography.DSA" /> to use. </param>
	public new static DSA Create(string algName)
	{
		return (DSA)CryptoConfig.CreateFromName(algName);
	}

	/// <summary>When overridden in a derived class, creates the <see cref="T:System.Security.Cryptography.DSA" /> signature for the specified data.</summary>
	/// <returns>The digital signature for the specified data.</returns>
	/// <param name="rgbHash">The data to be signed. </param>
	public abstract byte[] CreateSignature(byte[] rgbHash);

	/// <summary>When overridden in a derived class, verifies the <see cref="T:System.Security.Cryptography.DSA" /> signature for the specified data.</summary>
	/// <returns>true if <paramref name="rgbSignature" /> matches the signature computed using the specified hash algorithm and key on <paramref name="rgbHash" />; otherwise, false.</returns>
	/// <param name="rgbHash">The hash of the data signed with <paramref name="rgbSignature" />. </param>
	/// <param name="rgbSignature">The signature to be verified for <paramref name="rgbData" />. </param>
	public abstract bool VerifySignature(byte[] rgbHash, byte[] rgbSignature);

	protected virtual byte[] HashData(byte[] data, int offset, int count, HashAlgorithmName hashAlgorithm)
	{
		throw DerivedClassMustOverride();
	}

	protected virtual byte[] HashData(Stream data, HashAlgorithmName hashAlgorithm)
	{
		throw DerivedClassMustOverride();
	}

	public byte[] SignData(byte[] data, HashAlgorithmName hashAlgorithm)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return SignData(data, 0, data.Length, hashAlgorithm);
	}

	public virtual byte[] SignData(byte[] data, int offset, int count, HashAlgorithmName hashAlgorithm)
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
		byte[] rgbHash = HashData(data, offset, count, hashAlgorithm);
		return CreateSignature(rgbHash);
	}

	public virtual byte[] SignData(Stream data, HashAlgorithmName hashAlgorithm)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (string.IsNullOrEmpty(hashAlgorithm.Name))
		{
			throw HashAlgorithmNameNullOrEmpty();
		}
		byte[] rgbHash = HashData(data, hashAlgorithm);
		return CreateSignature(rgbHash);
	}

	public bool VerifyData(byte[] data, byte[] signature, HashAlgorithmName hashAlgorithm)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return VerifyData(data, 0, data.Length, signature, hashAlgorithm);
	}

	public virtual bool VerifyData(byte[] data, int offset, int count, byte[] signature, HashAlgorithmName hashAlgorithm)
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
		byte[] rgbHash = HashData(data, offset, count, hashAlgorithm);
		return VerifySignature(rgbHash, signature);
	}

	public virtual bool VerifyData(Stream data, byte[] signature, HashAlgorithmName hashAlgorithm)
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
		byte[] rgbHash = HashData(data, hashAlgorithm);
		return VerifySignature(rgbHash, signature);
	}

	/// <summary>Reconstructs a <see cref="T:System.Security.Cryptography.DSA" /> object from an XML string.</summary>
	/// <param name="xmlString">The XML string to use to reconstruct the <see cref="T:System.Security.Cryptography.DSA" /> object. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="xmlString" /> parameter is null. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The format of the <paramref name="xmlString" /> parameter is not valid. </exception>
	public override void FromXmlString(string xmlString)
	{
		if (xmlString == null)
		{
			throw new ArgumentNullException("xmlString");
		}
		DSAParameters parameters = default(DSAParameters);
		SecurityElement topElement = new Parser(xmlString).GetTopElement();
		string text = topElement.SearchForTextOfLocalName("P");
		if (text == null)
		{
			throw new CryptographicException(Environment.GetResourceString("Input string does not contain a valid encoding of the '{0}' '{1}' parameter.", "DSA", "P"));
		}
		parameters.P = Convert.FromBase64String(Utils.DiscardWhiteSpaces(text));
		string text2 = topElement.SearchForTextOfLocalName("Q");
		if (text2 == null)
		{
			throw new CryptographicException(Environment.GetResourceString("Input string does not contain a valid encoding of the '{0}' '{1}' parameter.", "DSA", "Q"));
		}
		parameters.Q = Convert.FromBase64String(Utils.DiscardWhiteSpaces(text2));
		string text3 = topElement.SearchForTextOfLocalName("G");
		if (text3 == null)
		{
			throw new CryptographicException(Environment.GetResourceString("Input string does not contain a valid encoding of the '{0}' '{1}' parameter.", "DSA", "G"));
		}
		parameters.G = Convert.FromBase64String(Utils.DiscardWhiteSpaces(text3));
		string text4 = topElement.SearchForTextOfLocalName("Y");
		if (text4 == null)
		{
			throw new CryptographicException(Environment.GetResourceString("Input string does not contain a valid encoding of the '{0}' '{1}' parameter.", "DSA", "Y"));
		}
		parameters.Y = Convert.FromBase64String(Utils.DiscardWhiteSpaces(text4));
		string text5 = topElement.SearchForTextOfLocalName("J");
		if (text5 != null)
		{
			parameters.J = Convert.FromBase64String(Utils.DiscardWhiteSpaces(text5));
		}
		string text6 = topElement.SearchForTextOfLocalName("X");
		if (text6 != null)
		{
			parameters.X = Convert.FromBase64String(Utils.DiscardWhiteSpaces(text6));
		}
		string text7 = topElement.SearchForTextOfLocalName("Seed");
		string text8 = topElement.SearchForTextOfLocalName("PgenCounter");
		if (text7 != null && text8 != null)
		{
			parameters.Seed = Convert.FromBase64String(Utils.DiscardWhiteSpaces(text7));
			parameters.Counter = Utils.ConvertByteArrayToInt(Convert.FromBase64String(Utils.DiscardWhiteSpaces(text8)));
		}
		else if (text7 != null || text8 != null)
		{
			if (text7 == null)
			{
				throw new CryptographicException(Environment.GetResourceString("Input string does not contain a valid encoding of the '{0}' '{1}' parameter.", "DSA", "Seed"));
			}
			throw new CryptographicException(Environment.GetResourceString("Input string does not contain a valid encoding of the '{0}' '{1}' parameter.", "DSA", "PgenCounter"));
		}
		ImportParameters(parameters);
	}

	/// <summary>Creates and returns an XML string representation of the current <see cref="T:System.Security.Cryptography.DSA" /> object.</summary>
	/// <returns>An XML string encoding of the current <see cref="T:System.Security.Cryptography.DSA" /> object.</returns>
	/// <param name="includePrivateParameters">true to include private parameters; otherwise, false. </param>
	public override string ToXmlString(bool includePrivateParameters)
	{
		DSAParameters dSAParameters = ExportParameters(includePrivateParameters);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("<DSAKeyValue>");
		stringBuilder.Append("<P>" + Convert.ToBase64String(dSAParameters.P) + "</P>");
		stringBuilder.Append("<Q>" + Convert.ToBase64String(dSAParameters.Q) + "</Q>");
		stringBuilder.Append("<G>" + Convert.ToBase64String(dSAParameters.G) + "</G>");
		stringBuilder.Append("<Y>" + Convert.ToBase64String(dSAParameters.Y) + "</Y>");
		if (dSAParameters.J != null)
		{
			stringBuilder.Append("<J>" + Convert.ToBase64String(dSAParameters.J) + "</J>");
		}
		if (dSAParameters.Seed != null)
		{
			stringBuilder.Append("<Seed>" + Convert.ToBase64String(dSAParameters.Seed) + "</Seed>");
			stringBuilder.Append("<PgenCounter>" + Convert.ToBase64String(Utils.ConvertIntToByteArray(dSAParameters.Counter)) + "</PgenCounter>");
		}
		if (includePrivateParameters)
		{
			stringBuilder.Append("<X>" + Convert.ToBase64String(dSAParameters.X) + "</X>");
		}
		stringBuilder.Append("</DSAKeyValue>");
		return stringBuilder.ToString();
	}

	/// <summary>When overridden in a derived class, exports the <see cref="T:System.Security.Cryptography.DSAParameters" />.</summary>
	/// <returns>The parameters for <see cref="T:System.Security.Cryptography.DSA" />.</returns>
	/// <param name="includePrivateParameters">true to include private parameters; otherwise, false. </param>
	public abstract DSAParameters ExportParameters(bool includePrivateParameters);

	/// <summary>When overridden in a derived class, imports the specified <see cref="T:System.Security.Cryptography.DSAParameters" />.</summary>
	/// <param name="parameters">The parameters for <see cref="T:System.Security.Cryptography.DSA" />. </param>
	public abstract void ImportParameters(DSAParameters parameters);

	private static Exception DerivedClassMustOverride()
	{
		return new NotImplementedException(Environment.GetResourceString("Derived classes must provide an implementation."));
	}

	internal static Exception HashAlgorithmNameNullOrEmpty()
	{
		return new ArgumentException(Environment.GetResourceString("The hash algorithm name cannot be null or empty."), "hashAlgorithm");
	}
}
