using System.Collections.Generic;
using Unity;

namespace System.Security.Cryptography;

/// <summary>Provides the base class for data protectors.</summary>
public abstract class DataProtector
{
	/// <summary>Gets the name of the application.</summary>
	/// <returns>The name of the application.</returns>
	protected string ApplicationName
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Specifies whether the hash is prepended to the text array before encryption.</summary>
	/// <returns>Always true.</returns>
	protected virtual bool PrependHashedPurposeToPlaintext
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(bool);
		}
	}

	/// <summary>Gets the primary purpose for the protected data.</summary>
	/// <returns>The primary purpose for the protected data.</returns>
	protected string PrimaryPurpose
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the specific purposes for the protected data.</summary>
	/// <returns>A collection of the specific purposes for the protected data.</returns>
	protected IEnumerable<string> SpecificPurposes
	{
		get
		{
			//IL_0007: Expected O, but got I4
			Unity.ThrowStub.ThrowNotSupportedException();
			return (IEnumerable<string>)0;
		}
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Security.Cryptography.DataProtector" /> class by using the provided application name, primary purpose, and specific purposes.</summary>
	/// <param name="applicationName">The name of the application.</param>
	/// <param name="primaryPurpose">The primary purpose for the protected data. See Remarks for additional important information.</param>
	/// <param name="specificPurposes">The specific purposes for the protected data. See Remarks for additional important information.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="applicationName" /> is an empty string or null.-or-<paramref name="primaryPurpose" /> is an empty string or null.-or-<paramref name="specificPurposes" /> contains an empty string or null.</exception>
	protected DataProtector(string applicationName, string primaryPurpose, string[] specificPurposes)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Creates an instance of a data protector implementation by using the specified class name of the data protector, the application name, the primary purpose, and the specific purposes.</summary>
	/// <returns>A data protector implementation object.</returns>
	/// <param name="providerClass">The class name for the data protector.</param>
	/// <param name="applicationName">The name of the application.</param>
	/// <param name="primaryPurpose">The primary purpose for the protected data.</param>
	/// <param name="specificPurposes">The specific purposes for the protected data.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="providerClass" /> is null.</exception>
	public static DataProtector Create(string providerClass, string applicationName, string primaryPurpose, string[] specificPurposes)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}

	/// <summary>Creates a hash of the property values specified by the constructor.</summary>
	/// <returns>An array of bytes that contain the hash of the <see cref="P:System.Security.Cryptography.DataProtector.ApplicationName" />, <see cref="P:System.Security.Cryptography.DataProtector.PrimaryPurpose" />, and <see cref="P:System.Security.Cryptography.DataProtector.SpecificPurposes" /> properties. </returns>
	protected virtual byte[] GetHashedPurpose()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}

	/// <summary>Determines if re-encryption is required for the specified encrypted data.</summary>
	/// <returns>true if the data must be re-encrypted; otherwise, false.</returns>
	/// <param name="encryptedData">The encrypted data to be evaluated.</param>
	public abstract bool IsReprotectRequired(byte[] encryptedData);

	/// <summary>Protects the specified user data.</summary>
	/// <returns>A byte array that contains the encrypted data.</returns>
	/// <param name="userData">The data to be protected.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="userData" /> is null.</exception>
	public byte[] Protect(byte[] userData)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}

	/// <summary>Specifies the delegate method in the derived class that the <see cref="M:System.Security.Cryptography.DataProtector.Protect(System.Byte[])" /> method in the base class calls back into.</summary>
	/// <returns>A byte array that contains the encrypted data.</returns>
	/// <param name="userData">The data to be encrypted.</param>
	protected abstract byte[] ProviderProtect(byte[] userData);

	/// <summary>Specifies the delegate method in the derived class that the <see cref="M:System.Security.Cryptography.DataProtector.Unprotect(System.Byte[])" /> method in the base class calls back into.</summary>
	/// <returns>The unencrypted data..</returns>
	/// <param name="encryptedData">The data to be unencrypted.</param>
	protected abstract byte[] ProviderUnprotect(byte[] encryptedData);

	/// <summary>Unprotects the specified protected data.</summary>
	/// <returns>A byte array that contains the plain-text data.</returns>
	/// <param name="encryptedData">The encrypted data to be unprotected.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="encryptedData" /> is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">
	///   <paramref name="encryptedData" /> contained an invalid purpose.</exception>
	public byte[] Unprotect(byte[] encryptedData)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}
}
