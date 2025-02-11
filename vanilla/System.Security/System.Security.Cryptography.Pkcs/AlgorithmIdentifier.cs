namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.AlgorithmIdentifier" /> class defines the algorithm used for a cryptographic operation.</summary>
public sealed class AlgorithmIdentifier
{
	private Oid _oid;

	private int _length;

	private byte[] _params;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.AlgorithmIdentifier.KeyLength" /> property sets or retrieves the key length, in bits. This property is not used for algorithms that use a fixed key length.</summary>
	/// <returns>An int value that represents the key length, in bits.</returns>
	public int KeyLength
	{
		get
		{
			return _length;
		}
		set
		{
			_length = value;
		}
	}

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.AlgorithmIdentifier.Oid" /> property sets or retrieves the <see cref="T:System.Security.Cryptography.Oid" />  object that specifies the object identifier for the algorithm.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.Oid" /> object that represents the algorithm.</returns>
	public Oid Oid
	{
		get
		{
			return _oid;
		}
		set
		{
			_oid = value;
		}
	}

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.AlgorithmIdentifier.Parameters" /> property sets or retrieves any parameters required by the algorithm.</summary>
	/// <returns>An array of byte values that specifies any parameters required by the algorithm.</returns>
	public byte[] Parameters
	{
		get
		{
			return _params;
		}
		set
		{
			_params = value;
		}
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.AlgorithmIdentifier.#ctor" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.AlgorithmIdentifier" /> class by using a set of default parameters. </summary>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	public AlgorithmIdentifier()
	{
		_oid = new Oid("1.2.840.113549.3.7", "3des");
		_params = new byte[0];
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.AlgorithmIdentifier.#ctor(System.Security.Cryptography.Oid)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.AlgorithmIdentifier" /> class with the specified algorithm identifier.</summary>
	/// <param name="oid">An object identifier for the algorithm.</param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	public AlgorithmIdentifier(Oid oid)
	{
		_oid = oid;
		_params = new byte[0];
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.AlgorithmIdentifier.#ctor(System.Security.Cryptography.Oid,System.Int32)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.AlgorithmIdentifier" /> class with the specified algorithm identifier and key length.</summary>
	/// <param name="oid">An object identifier for the algorithm.</param>
	/// <param name="keyLength">The length, in bits, of the key.</param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	public AlgorithmIdentifier(Oid oid, int keyLength)
	{
		_oid = oid;
		_length = keyLength;
		_params = new byte[0];
	}
}
