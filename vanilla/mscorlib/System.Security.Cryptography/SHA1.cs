using System.Runtime.InteropServices;

namespace System.Security.Cryptography;

/// <summary>Computes the <see cref="T:System.Security.Cryptography.SHA1" /> hash for the input data.</summary>
[ComVisible(true)]
public abstract class SHA1 : HashAlgorithm
{
	/// <summary>Initializes a new instance of <see cref="T:System.Security.Cryptography.SHA1" />.</summary>
	/// <exception cref="T:System.InvalidOperationException">The policy on this object is not compliant with the FIPS algorithm.</exception>
	protected SHA1()
	{
		HashSizeValue = 160;
	}

	/// <summary>Creates an instance of the default implementation of <see cref="T:System.Security.Cryptography.SHA1" />.</summary>
	/// <returns>A new instance of <see cref="T:System.Security.Cryptography.SHA1" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public new static SHA1 Create()
	{
		return Create("System.Security.Cryptography.SHA1");
	}

	/// <summary>Creates an instance of the specified implementation of <see cref="T:System.Security.Cryptography.SHA1" />.</summary>
	/// <returns>A new instance of <see cref="T:System.Security.Cryptography.SHA1" /> using the specified implementation.</returns>
	/// <param name="hashName">The name of the specific implementation of <see cref="T:System.Security.Cryptography.SHA1" /> to be used. </param>
	public new static SHA1 Create(string hashName)
	{
		return (SHA1)CryptoConfig.CreateFromName(hashName);
	}
}
