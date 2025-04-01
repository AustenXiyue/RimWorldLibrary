using System.IO;
using System.Security.Permissions;

namespace System.Security.Cryptography;

/// <summary>Provides a Cryptography Next Generation (CNG) implementation of the Elliptic Curve Digital Signature Algorithm (ECDSA). </summary>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class ECDsaCng : ECDsa
{
	/// <summary>Gets or sets the hash algorithm to use when signing and verifying data.</summary>
	/// <returns>An object that specifies the hash algorithm.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value is null.</exception>
	public CngAlgorithm HashAlgorithm { get; set; }

	/// <summary>Gets or sets the key to use when signing and verifying data.</summary>
	/// <returns>An object that specifies the key.</returns>
	public CngKey Key
	{
		get
		{
			throw new NotImplementedException();
		}
		private set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.ECDsaCng" /> class with a random key pair.</summary>
	/// <exception cref="T:System.PlatformNotSupportedException">Cryptography Next Generation (CNG) classes are not supported on this system.</exception>
	public ECDsaCng()
		: this(521)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.ECDsaCng" /> class with a random key pair, using the specified key size.</summary>
	/// <param name="keySize">The size of the key. Valid key sizes are 256, 384, and 521 bits.</param>
	/// <exception cref="T:System.PlatformNotSupportedException">Cryptography Next Generation (CNG) classes are not supported on this system.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">
	///   <paramref name="keySize" /> specifies an invalid length. </exception>
	public ECDsaCng(int keySize)
	{
		throw new NotImplementedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.ECDsaCng" /> class by using the specified <see cref="T:System.Security.Cryptography.CngKey" /> object.</summary>
	/// <param name="key">The key that will be used as input to the cryptographic operations performed by the current object.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="key" /> does not specify an Elliptic Curve Digital Signature Algorithm (ECDSA) group.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">Cryptography Next Generation (CNG) classes are not supported on this system.</exception>
	[SecuritySafeCritical]
	public ECDsaCng(CngKey key)
	{
		throw new NotImplementedException();
	}

	public ECDsaCng(ECCurve curve)
	{
		throw new NotImplementedException();
	}

	/// <summary>Generates a signature for the specified hash value.</summary>
	/// <returns>A digital signature for the specified hash value.</returns>
	/// <param name="hash">The hash value of the data to be signed.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="hash" /> is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The key information that is associated with the instance does not have a private key.</exception>
	public override byte[] SignHash(byte[] hash)
	{
		throw new NotImplementedException();
	}

	/// <summary>Verifies the specified digital signature against a specified hash value.</summary>
	/// <returns>true if the signature is valid; otherwise, false.</returns>
	/// <param name="hash">The hash value of the data to be verified.</param>
	/// <param name="signature">The digital signature of the data to be verified against the hash value.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="hash" /> or <paramref name="signature" /> is null.</exception>
	public override bool VerifyHash(byte[] hash, byte[] signature)
	{
		throw new NotImplementedException();
	}

	/// <summary>Deserializes the key information from an XML string by using the specified format.</summary>
	/// <param name="xml">The XML-based key information to be deserialized.</param>
	/// <param name="format">One of the enumeration values that specifies the format of the XML string. The only currently accepted format is <see cref="F:System.Security.Cryptography.ECKeyXmlFormat.Rfc4050" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="xml" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="format" /> specifies an invalid format. The only accepted value is <see cref="F:System.Security.Cryptography.ECKeyXmlFormat.Rfc4050" />.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">All other errors.</exception>
	public void FromXmlString(string xml, ECKeyXmlFormat format)
	{
		throw new NotImplementedException();
	}

	/// <summary>Generates a signature for the specified data.</summary>
	/// <returns>A digital signature for the specified data.</returns>
	/// <param name="data">The message data to be signed.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The key information that is associated with the instance does not have a private key.</exception>
	public byte[] SignData(byte[] data)
	{
		throw new NotImplementedException();
	}

	/// <summary>Generates a signature for the specified data stream, reading to the end of the stream.</summary>
	/// <returns>A digital signature for the specified data stream.</returns>
	/// <param name="data">The data stream to be signed.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The key information that is associated with the instance does not have a private key.</exception>
	public byte[] SignData(Stream data)
	{
		throw new NotImplementedException();
	}

	/// <summary>Generates a digital signature for the specified length of data, beginning at the specified offset. </summary>
	/// <returns>A digital signature for the specified length of data.</returns>
	/// <param name="data">The message data to be signed.</param>
	/// <param name="offset">The location in the string at which to start signing.</param>
	/// <param name="count">The length of the string, in characters, following <paramref name="offset" /> that will be signed.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> or <paramref name="offset" /> caused reading outside the bounds of the data string. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The key information that is associated with the instance does not have a private key.</exception>
	public byte[] SignData(byte[] data, int offset, int count)
	{
		throw new NotImplementedException();
	}

	/// <summary>Serializes the key information to an XML string by using the specified format.</summary>
	/// <returns>A string object that contains the key information, serialized to an XML string according to the requested format.</returns>
	/// <param name="format">One of the enumeration values that specifies the format of the XML string. The only currently accepted format is <see cref="F:System.Security.Cryptography.ECKeyXmlFormat.Rfc4050" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="format" /> specifies an invalid format. The only accepted value is <see cref="F:System.Security.Cryptography.ECKeyXmlFormat.Rfc4050" />.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">All other errors.</exception>
	public string ToXmlString(ECKeyXmlFormat format)
	{
		throw new NotImplementedException();
	}

	/// <summary>Verifies the digital signature of the specified data. </summary>
	/// <returns>true if the signature is valid; otherwise, false.</returns>
	/// <param name="data">The data that was signed.</param>
	/// <param name="signature">The signature to be verified.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> or <paramref name="signature" /> is null.</exception>
	public bool VerifyData(byte[] data, byte[] signature)
	{
		throw new NotImplementedException();
	}

	/// <summary>Verifies the digital signature of the specified data stream, reading to the end of the stream.</summary>
	/// <returns>true if the signature is valid; otherwise, false.</returns>
	/// <param name="data">The data stream that was signed.</param>
	/// <param name="signature">The signature to be verified.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> or <paramref name="signature" /> is null.</exception>
	public bool VerifyData(Stream data, byte[] signature)
	{
		throw new NotImplementedException();
	}

	/// <summary>Verifies a signature for the specified length of data, beginning at the specified offset.</summary>
	/// <returns>true if the signature is valid; otherwise, false.</returns>
	/// <param name="data">The data that was signed.</param>
	/// <param name="offset">The location in the data at which the signed data begins.</param>
	/// <param name="count">The length of the data, in characters, following <paramref name="offset" /> that will be signed.</param>
	/// <param name="signature">The signature to be verified.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="count" /> is less then zero. -or-<paramref name="offset" /> or <paramref name="count" /> is larger than the length of the byte array passed in the <paramref name="data" /> parameter.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> or <paramref name="signature" /> is null.</exception>
	public bool VerifyData(byte[] data, int offset, int count, byte[] signature)
	{
		throw new NotImplementedException();
	}
}
