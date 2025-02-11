using System.Runtime.InteropServices;

namespace System.Security.Cryptography;

/// <summary>Contains information about the properties of a digital signature.</summary>
[ComVisible(true)]
public class SignatureDescription
{
	private string _strKey;

	private string _strDigest;

	private string _strFormatter;

	private string _strDeformatter;

	/// <summary>Gets or sets the key algorithm for the signature description.</summary>
	/// <returns>The key algorithm for the signature description.</returns>
	public string KeyAlgorithm
	{
		get
		{
			return _strKey;
		}
		set
		{
			_strKey = value;
		}
	}

	/// <summary>Gets or sets the digest algorithm for the signature description.</summary>
	/// <returns>The digest algorithm for the signature description.</returns>
	public string DigestAlgorithm
	{
		get
		{
			return _strDigest;
		}
		set
		{
			_strDigest = value;
		}
	}

	/// <summary>Gets or sets the formatter algorithm for the signature description.</summary>
	/// <returns>The formatter algorithm for the signature description.</returns>
	public string FormatterAlgorithm
	{
		get
		{
			return _strFormatter;
		}
		set
		{
			_strFormatter = value;
		}
	}

	/// <summary>Gets or sets the deformatter algorithm for the signature description.</summary>
	/// <returns>The deformatter algorithm for the signature description.</returns>
	public string DeformatterAlgorithm
	{
		get
		{
			return _strDeformatter;
		}
		set
		{
			_strDeformatter = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.SignatureDescription" /> class.</summary>
	public SignatureDescription()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.SignatureDescription" /> class from the specified <see cref="T:System.Security.SecurityElement" />.</summary>
	/// <param name="el">The <see cref="T:System.Security.SecurityElement" /> from which to get the algorithms for the signature description. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="el" /> parameter is null.</exception>
	public SignatureDescription(SecurityElement el)
	{
		if (el == null)
		{
			throw new ArgumentNullException("el");
		}
		_strKey = el.SearchForTextOfTag("Key");
		_strDigest = el.SearchForTextOfTag("Digest");
		_strFormatter = el.SearchForTextOfTag("Formatter");
		_strDeformatter = el.SearchForTextOfTag("Deformatter");
	}

	/// <summary>Creates an <see cref="T:System.Security.Cryptography.AsymmetricSignatureDeformatter" /> instance with the specified key using the <see cref="P:System.Security.Cryptography.SignatureDescription.DeformatterAlgorithm" /> property.</summary>
	/// <returns>The newly created <see cref="T:System.Security.Cryptography.AsymmetricSignatureDeformatter" /> instance.</returns>
	/// <param name="key">The key to use in the <see cref="T:System.Security.Cryptography.AsymmetricSignatureDeformatter" />. </param>
	public virtual AsymmetricSignatureDeformatter CreateDeformatter(AsymmetricAlgorithm key)
	{
		AsymmetricSignatureDeformatter obj = (AsymmetricSignatureDeformatter)CryptoConfig.CreateFromName(_strDeformatter);
		obj.SetKey(key);
		return obj;
	}

	/// <summary>Creates an <see cref="T:System.Security.Cryptography.AsymmetricSignatureFormatter" /> instance with the specified key using the <see cref="P:System.Security.Cryptography.SignatureDescription.FormatterAlgorithm" /> property.</summary>
	/// <returns>The newly created <see cref="T:System.Security.Cryptography.AsymmetricSignatureFormatter" /> instance.</returns>
	/// <param name="key">The key to use in the <see cref="T:System.Security.Cryptography.AsymmetricSignatureFormatter" />. </param>
	public virtual AsymmetricSignatureFormatter CreateFormatter(AsymmetricAlgorithm key)
	{
		AsymmetricSignatureFormatter obj = (AsymmetricSignatureFormatter)CryptoConfig.CreateFromName(_strFormatter);
		obj.SetKey(key);
		return obj;
	}

	/// <summary>Creates a <see cref="T:System.Security.Cryptography.HashAlgorithm" /> instance using the <see cref="P:System.Security.Cryptography.SignatureDescription.DigestAlgorithm" /> property.</summary>
	/// <returns>The newly created <see cref="T:System.Security.Cryptography.HashAlgorithm" /> instance.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public virtual HashAlgorithm CreateDigest()
	{
		return (HashAlgorithm)CryptoConfig.CreateFromName(_strDigest);
	}
}
