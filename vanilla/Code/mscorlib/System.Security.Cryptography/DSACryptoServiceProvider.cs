using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Mono.Security.Cryptography;

namespace System.Security.Cryptography;

/// <summary>Defines a wrapper object to access the cryptographic service provider (CSP) implementation of the <see cref="T:System.Security.Cryptography.DSA" /> algorithm. This class cannot be inherited. </summary>
[ComVisible(true)]
public sealed class DSACryptoServiceProvider : DSA, ICspAsymmetricAlgorithm
{
	private const int PROV_DSS_DH = 13;

	private KeyPairPersistence store;

	private bool persistKey;

	private bool persisted;

	private bool privateKeyExportable = true;

	private bool m_disposed;

	private DSAManaged dsa;

	private static bool useMachineKeyStore;

	/// <summary>Gets the name of the key exchange algorithm.</summary>
	/// <returns>The name of the key exchange algorithm.</returns>
	public override string KeyExchangeAlgorithm => null;

	/// <summary>Gets the size of the key used by the asymmetric algorithm in bits.</summary>
	/// <returns>The size of the key used by the asymmetric algorithm.</returns>
	public override int KeySize => dsa.KeySize;

	/// <summary>Gets or sets a value indicating whether the key should be persisted in the cryptographic service provider (CSP).</summary>
	/// <returns>true if the key should be persisted in the CSP; otherwise, false.</returns>
	public bool PersistKeyInCsp
	{
		get
		{
			return persistKey;
		}
		set
		{
			persistKey = value;
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Security.Cryptography.DSACryptoServiceProvider" /> object contains only a public key.</summary>
	/// <returns>true if the <see cref="T:System.Security.Cryptography.DSACryptoServiceProvider" /> object contains only a public key; otherwise, false.</returns>
	[ComVisible(false)]
	public bool PublicOnly => dsa.PublicOnly;

	/// <summary>Gets the name of the signature algorithm.</summary>
	/// <returns>The name of the signature algorithm.</returns>
	public override string SignatureAlgorithm => "http://www.w3.org/2000/09/xmldsig#dsa-sha1";

	/// <summary>Gets or sets a value indicating whether the key should be persisted in the computer's key store instead of the user profile store.</summary>
	/// <returns>true if the key should be persisted in the computer key store; otherwise, false.</returns>
	public static bool UseMachineKeyStore
	{
		get
		{
			return useMachineKeyStore;
		}
		set
		{
			useMachineKeyStore = value;
		}
	}

	/// <summary>Gets a <see cref="T:System.Security.Cryptography.CspKeyContainerInfo" /> object that describes additional information about a cryptographic key pair.  </summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.CspKeyContainerInfo" /> object that describes additional information about a cryptographic key pair.</returns>
	[MonoTODO("call into KeyPairPersistence to get details")]
	[ComVisible(false)]
	public CspKeyContainerInfo CspKeyContainerInfo
	{
		[SecuritySafeCritical]
		get
		{
			return null;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.DSACryptoServiceProvider" /> class.</summary>
	public DSACryptoServiceProvider()
		: this(1024)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.DSACryptoServiceProvider" /> class with the specified parameters for the cryptographic service provider (CSP).</summary>
	/// <param name="parameters">The parameters for the CSP. </param>
	public DSACryptoServiceProvider(CspParameters parameters)
		: this(1024, parameters)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.DSACryptoServiceProvider" /> class with the specified key size.</summary>
	/// <param name="dwKeySize">The size of the key for the asymmetric algorithm in bits. </param>
	public DSACryptoServiceProvider(int dwKeySize)
	{
		Common(dwKeySize, parameters: false);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.DSACryptoServiceProvider" /> class with the specified key size and parameters for the cryptographic service provider (CSP).</summary>
	/// <param name="dwKeySize">The size of the key for the cryptographic algorithm in bits. </param>
	/// <param name="parameters">The parameters for the CSP. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The CSP cannot be acquired.-or- The key cannot be created. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="dwKeySize" /> is out of range.</exception>
	public DSACryptoServiceProvider(int dwKeySize, CspParameters parameters)
	{
		bool flag = parameters != null;
		Common(dwKeySize, flag);
		if (flag)
		{
			Common(parameters);
		}
	}

	private void Common(int dwKeySize, bool parameters)
	{
		LegalKeySizesValue = new KeySizes[1];
		LegalKeySizesValue[0] = new KeySizes(512, 1024, 64);
		KeySize = dwKeySize;
		dsa = new DSAManaged(dwKeySize);
		dsa.KeyGenerated += OnKeyGenerated;
		persistKey = parameters;
		if (!parameters)
		{
			CspParameters cspParameters = new CspParameters(13);
			if (useMachineKeyStore)
			{
				cspParameters.Flags |= CspProviderFlags.UseMachineKeyStore;
			}
			store = new KeyPairPersistence(cspParameters);
		}
	}

	private void Common(CspParameters parameters)
	{
		store = new KeyPairPersistence(parameters);
		store.Load();
		if (store.KeyValue != null)
		{
			persisted = true;
			FromXmlString(store.KeyValue);
		}
		privateKeyExportable = (parameters.Flags & CspProviderFlags.UseNonExportableKey) == 0;
	}

	~DSACryptoServiceProvider()
	{
		Dispose(disposing: false);
	}

	/// <summary>Exports the <see cref="T:System.Security.Cryptography.DSAParameters" />.</summary>
	/// <returns>The parameters for <see cref="T:System.Security.Cryptography.DSA" />.</returns>
	/// <param name="includePrivateParameters">true to include private parameters; otherwise, false. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The key cannot be exported. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override DSAParameters ExportParameters(bool includePrivateParameters)
	{
		if (includePrivateParameters && !privateKeyExportable)
		{
			throw new CryptographicException(Locale.GetText("Cannot export private key"));
		}
		return dsa.ExportParameters(includePrivateParameters);
	}

	/// <summary>Imports the specified <see cref="T:System.Security.Cryptography.DSAParameters" />.</summary>
	/// <param name="parameters">The parameters for <see cref="T:System.Security.Cryptography.DSA" />. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.-or- The <paramref name="parameters" /> parameter has missing fields. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override void ImportParameters(DSAParameters parameters)
	{
		dsa.ImportParameters(parameters);
	}

	/// <summary>Creates the <see cref="T:System.Security.Cryptography.DSA" /> signature for the specified data.</summary>
	/// <returns>The digital signature for the specified data.</returns>
	/// <param name="rgbHash">The data to be signed. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override byte[] CreateSignature(byte[] rgbHash)
	{
		return dsa.CreateSignature(rgbHash);
	}

	/// <summary>Computes the hash value of the specified byte array and signs the resulting hash value.</summary>
	/// <returns>The <see cref="T:System.Security.Cryptography.DSA" /> signature for the specified data.</returns>
	/// <param name="buffer">The input data for which to compute the hash. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public byte[] SignData(byte[] buffer)
	{
		byte[] rgbHash = SHA1.Create().ComputeHash(buffer);
		return dsa.CreateSignature(rgbHash);
	}

	/// <summary>Signs a byte array from the specified start point to the specified end point.</summary>
	/// <returns>The <see cref="T:System.Security.Cryptography.DSA" /> signature for the specified data.</returns>
	/// <param name="buffer">The input data to sign. </param>
	/// <param name="offset">The offset into the array from which to begin using data. </param>
	/// <param name="count">The number of bytes in the array to use as data. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public byte[] SignData(byte[] buffer, int offset, int count)
	{
		byte[] rgbHash = SHA1.Create().ComputeHash(buffer, offset, count);
		return dsa.CreateSignature(rgbHash);
	}

	/// <summary>Computes the hash value of the specified input stream and signs the resulting hash value.</summary>
	/// <returns>The <see cref="T:System.Security.Cryptography.DSA" /> signature for the specified data.</returns>
	/// <param name="inputStream">The input data for which to compute the hash. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public byte[] SignData(Stream inputStream)
	{
		byte[] rgbHash = SHA1.Create().ComputeHash(inputStream);
		return dsa.CreateSignature(rgbHash);
	}

	/// <summary>Computes the signature for the specified hash value by encrypting it with the private key.</summary>
	/// <returns>The <see cref="T:System.Security.Cryptography.DSA" /> signature for the specified hash value.</returns>
	/// <param name="rgbHash">The hash value of the data to be signed. </param>
	/// <param name="str">The name of the hash algorithm used to create the hash value of the data. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="rgbHash" /> parameter is null. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.-or- There is no private key. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public byte[] SignHash(byte[] rgbHash, string str)
	{
		if (string.Compare(str, "SHA1", ignoreCase: true, CultureInfo.InvariantCulture) != 0)
		{
			throw new CryptographicException(Locale.GetText("Only SHA1 is supported."));
		}
		return dsa.CreateSignature(rgbHash);
	}

	/// <summary>Verifies the specified signature data by comparing it to the signature computed for the specified data.</summary>
	/// <returns>true if the signature verifies as valid; otherwise, false.</returns>
	/// <param name="rgbData">The data that was signed. </param>
	/// <param name="rgbSignature">The signature data to be verified. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public bool VerifyData(byte[] rgbData, byte[] rgbSignature)
	{
		byte[] rgbHash = SHA1.Create().ComputeHash(rgbData);
		return dsa.VerifySignature(rgbHash, rgbSignature);
	}

	/// <summary>Verifies the specified signature data by comparing it to the signature computed for the specified hash value.</summary>
	/// <returns>true if the signature verifies as valid; otherwise, false.</returns>
	/// <param name="rgbHash">The hash value of the data to be signed. </param>
	/// <param name="str">The name of the hash algorithm used to create the hash value of the data. </param>
	/// <param name="rgbSignature">The signature data to be verified. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="rgbHash" /> parameter is null.-or- The <paramref name="rgbSignature" /> parameter is null. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.-or- The signature cannot be verified. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public bool VerifyHash(byte[] rgbHash, string str, byte[] rgbSignature)
	{
		if (str == null)
		{
			str = "SHA1";
		}
		if (string.Compare(str, "SHA1", ignoreCase: true, CultureInfo.InvariantCulture) != 0)
		{
			throw new CryptographicException(Locale.GetText("Only SHA1 is supported."));
		}
		return dsa.VerifySignature(rgbHash, rgbSignature);
	}

	/// <summary>Verifies the <see cref="T:System.Security.Cryptography.DSA" /> signature for the specified data.</summary>
	/// <returns>true if <paramref name="rgbSignature" /> matches the signature computed using the specified hash algorithm and key on <paramref name="rgbHash" />; otherwise, false.</returns>
	/// <param name="rgbHash">The data signed with <paramref name="rgbSignature" />. </param>
	/// <param name="rgbSignature">The signature to be verified for <paramref name="rgbData" />. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override bool VerifySignature(byte[] rgbHash, byte[] rgbSignature)
	{
		return dsa.VerifySignature(rgbHash, rgbSignature);
	}

	protected override byte[] HashData(byte[] data, int offset, int count, HashAlgorithmName hashAlgorithm)
	{
		if (hashAlgorithm != HashAlgorithmName.SHA1)
		{
			throw new CryptographicException(Environment.GetResourceString("'{0}' is not a known hash algorithm.", hashAlgorithm.Name));
		}
		return HashAlgorithm.Create(hashAlgorithm.Name).ComputeHash(data, offset, count);
	}

	protected override byte[] HashData(Stream data, HashAlgorithmName hashAlgorithm)
	{
		if (hashAlgorithm != HashAlgorithmName.SHA1)
		{
			throw new CryptographicException(Environment.GetResourceString("'{0}' is not a known hash algorithm.", hashAlgorithm.Name));
		}
		return HashAlgorithm.Create(hashAlgorithm.Name).ComputeHash(data);
	}

	protected override void Dispose(bool disposing)
	{
		if (!m_disposed)
		{
			if (persisted && !persistKey)
			{
				store.Remove();
			}
			if (dsa != null)
			{
				dsa.Clear();
			}
			m_disposed = true;
		}
	}

	private void OnKeyGenerated(object sender, EventArgs e)
	{
		if (persistKey && !persisted)
		{
			store.KeyValue = ToXmlString(!dsa.PublicOnly);
			store.Save();
			persisted = true;
		}
	}

	/// <summary>Exports a blob containing the key information associated with a <see cref="T:System.Security.Cryptography.DSACryptoServiceProvider" /> object.  </summary>
	/// <returns>A byte array containing the key information associated with a <see cref="T:System.Security.Cryptography.DSACryptoServiceProvider" /> object.</returns>
	/// <param name="includePrivateParameters">true to include the private key; otherwise, false.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[ComVisible(false)]
	[SecuritySafeCritical]
	public byte[] ExportCspBlob(bool includePrivateParameters)
	{
		byte[] array = null;
		if (includePrivateParameters)
		{
			return CryptoConvert.ToCapiPrivateKeyBlob(this);
		}
		return CryptoConvert.ToCapiPublicKeyBlob(this);
	}

	/// <summary>Imports a blob that represents DSA key information.</summary>
	/// <param name="keyBlob">A byte array that represents a DSA key blob.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	[ComVisible(false)]
	public void ImportCspBlob(byte[] keyBlob)
	{
		if (keyBlob == null)
		{
			throw new ArgumentNullException("keyBlob");
		}
		DSA dSA = CryptoConvert.FromCapiKeyBlobDSA(keyBlob);
		if (dSA is DSACryptoServiceProvider)
		{
			DSAParameters parameters = dSA.ExportParameters(!(dSA as DSACryptoServiceProvider).PublicOnly);
			ImportParameters(parameters);
			return;
		}
		try
		{
			DSAParameters parameters2 = dSA.ExportParameters(includePrivateParameters: true);
			ImportParameters(parameters2);
		}
		catch
		{
			DSAParameters parameters3 = dSA.ExportParameters(includePrivateParameters: false);
			ImportParameters(parameters3);
		}
	}
}
