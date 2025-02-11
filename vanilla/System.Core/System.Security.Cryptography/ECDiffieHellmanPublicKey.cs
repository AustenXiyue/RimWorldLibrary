using System.Security.Permissions;

namespace System.Security.Cryptography;

/// <summary>Provides an abstract base class from which all <see cref="T:System.Security.Cryptography.ECDiffieHellmanCngPublicKey" /> implementations must inherit. </summary>
[Serializable]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public abstract class ECDiffieHellmanPublicKey : IDisposable
{
	private byte[] m_keyBlob;

	protected ECDiffieHellmanPublicKey()
	{
		m_keyBlob = new byte[0];
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.ECDiffieHellmanPublicKey" /> class.</summary>
	/// <param name="keyBlob">A byte array that represents an <see cref="T:System.Security.Cryptography.ECDiffieHellmanPublicKey" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="keyBlob" /> is null.</exception>
	protected ECDiffieHellmanPublicKey(byte[] keyBlob)
	{
		if (keyBlob == null)
		{
			throw new ArgumentNullException("keyBlob");
		}
		m_keyBlob = keyBlob.Clone() as byte[];
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.Security.Cryptography.ECDiffieHellman" /> class.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Security.Cryptography.ECDiffieHellman" /> class and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected virtual void Dispose(bool disposing)
	{
	}

	/// <summary>Serializes the <see cref="T:System.Security.Cryptography.ECDiffieHellmanPublicKey" /> key BLOB to a byte array.</summary>
	/// <returns>A byte array that contains the serialized Elliptic Curve Diffie-Hellman (ECDH) public key.</returns>
	public virtual byte[] ToByteArray()
	{
		return m_keyBlob.Clone() as byte[];
	}

	/// <summary>Serializes the <see cref="T:System.Security.Cryptography.ECDiffieHellmanPublicKey" /> public key to an XML string.</summary>
	/// <returns>An XML string that contains the serialized Elliptic Curve Diffie-Hellman (ECDH) public key.</returns>
	public virtual string ToXmlString()
	{
		throw new NotImplementedException(global::SR.GetString("Method not supported. Derived class must override."));
	}

	public virtual ECParameters ExportParameters()
	{
		throw new NotSupportedException(global::SR.GetString("Method not supported. Derived class must override."));
	}

	public virtual ECParameters ExportExplicitParameters()
	{
		throw new NotSupportedException(global::SR.GetString("Method not supported. Derived class must override."));
	}
}
