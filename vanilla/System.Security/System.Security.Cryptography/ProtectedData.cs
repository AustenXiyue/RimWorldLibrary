using Mono.Security.Cryptography;

namespace System.Security.Cryptography;

/// <summary>Provides methods for encrypting and decrypting data. This class cannot be inherited.</summary>
public sealed class ProtectedData
{
	private enum DataProtectionImplementation
	{
		Unknown = 0,
		Win32CryptoProtect = 1,
		ManagedProtection = 2,
		Unsupported = int.MinValue
	}

	private static DataProtectionImplementation impl;

	private ProtectedData()
	{
	}

	/// <summary>Encrypts the data in a specified byte array and returns a byte array that contains the encrypted data.</summary>
	/// <returns>A byte array representing the encrypted data.</returns>
	/// <param name="userData">A byte array that contains data to encrypt. </param>
	/// <param name="optionalEntropy">An optional additional byte array used to increase the complexity of the encryption, or null for no additional complexity.</param>
	/// <param name="scope">One of the enumeration values that specifies the scope of encryption. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="userData" /> parameter is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The encryption failed.</exception>
	/// <exception cref="T:System.NotSupportedException">The operating system does not support this method. </exception>
	/// <exception cref="T:System.OutOfMemoryException">The system ran out of memory while encrypting the data.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.DataProtectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ProtectData" />
	/// </PermissionSet>
	public static byte[] Protect(byte[] userData, byte[] optionalEntropy, DataProtectionScope scope)
	{
		if (userData == null)
		{
			throw new ArgumentNullException("userData");
		}
		Check(scope);
		switch (impl)
		{
		case DataProtectionImplementation.ManagedProtection:
			try
			{
				return ManagedProtection.Protect(userData, optionalEntropy, scope);
			}
			catch (Exception inner2)
			{
				throw new CryptographicException(global::Locale.GetText("Data protection failed."), inner2);
			}
		case DataProtectionImplementation.Win32CryptoProtect:
			try
			{
				return NativeDapiProtection.Protect(userData, optionalEntropy, scope);
			}
			catch (Exception inner)
			{
				throw new CryptographicException(global::Locale.GetText("Data protection failed."), inner);
			}
		default:
			throw new PlatformNotSupportedException();
		}
	}

	/// <summary>Decrypts the data in a specified byte array and returns a byte array that contains the decrypted data.</summary>
	/// <returns>A byte array representing the decrypted data.</returns>
	/// <param name="encryptedData">A byte array containing data encrypted using the <see cref="M:System.Security.Cryptography.ProtectedData.Protect(System.Byte[],System.Byte[],System.Security.Cryptography.DataProtectionScope)" /> method. </param>
	/// <param name="optionalEntropy">An optional additional byte array that was used to encrypt the data, or null if the additional byte array was not used.</param>
	/// <param name="scope">One of the enumeration values that specifies the scope of data protection that was used to encrypt the data. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="encryptedData" /> parameter is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The decryption failed.</exception>
	/// <exception cref="T:System.NotSupportedException">The operating system does not support this method. </exception>
	/// <exception cref="T:System.OutOfMemoryException">Out of memory.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.DataProtectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnprotectData" />
	/// </PermissionSet>
	public static byte[] Unprotect(byte[] encryptedData, byte[] optionalEntropy, DataProtectionScope scope)
	{
		if (encryptedData == null)
		{
			throw new ArgumentNullException("encryptedData");
		}
		Check(scope);
		switch (impl)
		{
		case DataProtectionImplementation.ManagedProtection:
			try
			{
				return ManagedProtection.Unprotect(encryptedData, optionalEntropy, scope);
			}
			catch (Exception inner2)
			{
				throw new CryptographicException(global::Locale.GetText("Data unprotection failed."), inner2);
			}
		case DataProtectionImplementation.Win32CryptoProtect:
			try
			{
				return NativeDapiProtection.Unprotect(encryptedData, optionalEntropy, scope);
			}
			catch (Exception inner)
			{
				throw new CryptographicException(global::Locale.GetText("Data unprotection failed."), inner);
			}
		default:
			throw new PlatformNotSupportedException();
		}
	}

	private static void Detect()
	{
		OperatingSystem oSVersion = Environment.OSVersion;
		switch (oSVersion.Platform)
		{
		case PlatformID.Win32NT:
			if (oSVersion.Version.Major < 5)
			{
				impl = DataProtectionImplementation.Unsupported;
			}
			else
			{
				impl = DataProtectionImplementation.Win32CryptoProtect;
			}
			break;
		case PlatformID.Unix:
			impl = DataProtectionImplementation.ManagedProtection;
			break;
		default:
			impl = DataProtectionImplementation.Unsupported;
			break;
		}
	}

	private static void Check(DataProtectionScope scope)
	{
		if (scope < DataProtectionScope.CurrentUser || scope > DataProtectionScope.LocalMachine)
		{
			throw new ArgumentException(global::Locale.GetText("Invalid enum value '{0}' for '{1}'.", scope, "DataProtectionScope"), "scope");
		}
		switch (impl)
		{
		case DataProtectionImplementation.Unknown:
			Detect();
			break;
		case DataProtectionImplementation.Unsupported:
			throw new PlatformNotSupportedException();
		}
	}
}
