using System.Runtime.InteropServices;

namespace System.Security.Cryptography;

/// <summary>Represents the base class for the Data Encryption Standard (DES) algorithm from which all <see cref="T:System.Security.Cryptography.DES" /> implementations must derive.</summary>
[ComVisible(true)]
public abstract class DES : SymmetricAlgorithm
{
	private static KeySizes[] s_legalBlockSizes = new KeySizes[1]
	{
		new KeySizes(64, 64, 0)
	};

	private static KeySizes[] s_legalKeySizes = new KeySizes[1]
	{
		new KeySizes(64, 64, 0)
	};

	/// <summary>Gets or sets the secret key for the Data Encryption Standard (<see cref="T:System.Security.Cryptography.DES" />) algorithm.</summary>
	/// <returns>The secret key for the <see cref="T:System.Security.Cryptography.DES" /> algorithm.</returns>
	/// <exception cref="T:System.ArgumentNullException">An attempt was made to set the key to null. </exception>
	/// <exception cref="T:System.ArgumentException">An attempt was made to set a key whose length is not equal to <see cref="F:System.Security.Cryptography.SymmetricAlgorithm.BlockSizeValue" />. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An attempt was made to set a weak key (see <see cref="M:System.Security.Cryptography.DES.IsWeakKey(System.Byte[])" />) or a semi-weak key (see <see cref="M:System.Security.Cryptography.DES.IsSemiWeakKey(System.Byte[])" />). </exception>
	public override byte[] Key
	{
		get
		{
			if (KeyValue == null)
			{
				do
				{
					GenerateKey();
				}
				while (IsWeakKey(KeyValue) || IsSemiWeakKey(KeyValue));
			}
			return (byte[])KeyValue.Clone();
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (!ValidKeySize(value.Length * 8))
			{
				throw new ArgumentException(Environment.GetResourceString("Specified key is not a valid size for this algorithm."));
			}
			if (IsWeakKey(value))
			{
				throw new CryptographicException(Environment.GetResourceString("Specified key is a known weak key for '{0}' and cannot be used."), "DES");
			}
			if (IsSemiWeakKey(value))
			{
				throw new CryptographicException(Environment.GetResourceString("Specified key is a known semi-weak key for '{0}' and cannot be used."), "DES");
			}
			KeyValue = (byte[])value.Clone();
			KeySizeValue = value.Length * 8;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.DES" /> class.</summary>
	protected DES()
	{
		KeySizeValue = 64;
		BlockSizeValue = 64;
		FeedbackSizeValue = BlockSizeValue;
		LegalBlockSizesValue = s_legalBlockSizes;
		LegalKeySizesValue = s_legalKeySizes;
	}

	/// <summary>Creates an instance of a cryptographic object to perform the Data Encryption Standard (<see cref="T:System.Security.Cryptography.DES" />) algorithm.</summary>
	/// <returns>A cryptographic object.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public new static DES Create()
	{
		return Create("System.Security.Cryptography.DES");
	}

	/// <summary>Creates an instance of a cryptographic object to perform the specified implementation of the Data Encryption Standard (<see cref="T:System.Security.Cryptography.DES" />) algorithm.</summary>
	/// <returns>A cryptographic object.</returns>
	/// <param name="algName">The name of the specific implementation of <see cref="T:System.Security.Cryptography.DES" /> to use. </param>
	public new static DES Create(string algName)
	{
		return (DES)CryptoConfig.CreateFromName(algName);
	}

	/// <summary>Determines whether the specified key is weak.</summary>
	/// <returns>true if the key is weak; otherwise, false.</returns>
	/// <param name="rgbKey">The secret key to test for weakness. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The size of the <paramref name="rgbKey" /> parameter is not valid. </exception>
	public static bool IsWeakKey(byte[] rgbKey)
	{
		if (!IsLegalKeySize(rgbKey))
		{
			throw new CryptographicException(Environment.GetResourceString("Specified key is not a valid size for this algorithm."));
		}
		ulong num = QuadWordFromBigEndian(Utils.FixupKeyParity(rgbKey));
		if (num == 72340172838076673L || num == 18374403900871474942uL || num == 2242545357694045710L || num == 16204198716015505905uL)
		{
			return true;
		}
		return false;
	}

	/// <summary>Determines whether the specified key is semi-weak.</summary>
	/// <returns>true if the key is semi-weak; otherwise, false.</returns>
	/// <param name="rgbKey">The secret key to test for semi-weakness. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The size of the <paramref name="rgbKey" /> parameter is not valid. </exception>
	public static bool IsSemiWeakKey(byte[] rgbKey)
	{
		if (!IsLegalKeySize(rgbKey))
		{
			throw new CryptographicException(Environment.GetResourceString("Specified key is not a valid size for this algorithm."));
		}
		ulong num = QuadWordFromBigEndian(Utils.FixupKeyParity(rgbKey));
		if (num == 143554428589179390L || num == 18303189645120372225uL || num == 2296870857142767345L || num == 16149873216566784270uL || num == 135110050437988849L || num == 16141428838415593729uL || num == 2305315235293957886L || num == 18311634023271562766uL || num == 80784550989267214L || num == 2234100979542855169L || num == 16212643094166696446uL || num == 18365959522720284401uL)
		{
			return true;
		}
		return false;
	}

	private static bool IsLegalKeySize(byte[] rgbKey)
	{
		if (rgbKey != null && rgbKey.Length == 8)
		{
			return true;
		}
		return false;
	}

	private static ulong QuadWordFromBigEndian(byte[] block)
	{
		return ((ulong)block[0] << 56) | ((ulong)block[1] << 48) | ((ulong)block[2] << 40) | ((ulong)block[3] << 32) | ((ulong)block[4] << 24) | ((ulong)block[5] << 16) | ((ulong)block[6] << 8) | block[7];
	}
}
