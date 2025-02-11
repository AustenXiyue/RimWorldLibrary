using System.Runtime.InteropServices;

namespace System.Security.Cryptography;

/// <summary>Represents the base class for Triple Data Encryption Standard algorithms from which all <see cref="T:System.Security.Cryptography.TripleDES" /> implementations must derive.</summary>
[ComVisible(true)]
public abstract class TripleDES : SymmetricAlgorithm
{
	private static KeySizes[] s_legalBlockSizes = new KeySizes[1]
	{
		new KeySizes(64, 64, 0)
	};

	private static KeySizes[] s_legalKeySizes = new KeySizes[1]
	{
		new KeySizes(128, 192, 64)
	};

	/// <summary>Gets or sets the secret key for the <see cref="T:System.Security.Cryptography.TripleDES" /> algorithm.</summary>
	/// <returns>The secret key for the <see cref="T:System.Security.Cryptography.TripleDES" /> algorithm.</returns>
	/// <exception cref="T:System.ArgumentNullException">An attempt was made to set the key to null. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An attempt was made to set a key whose length is invalid.-or- An attempt was made to set a weak key (see <see cref="M:System.Security.Cryptography.TripleDES.IsWeakKey(System.Byte[])" />). </exception>
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
				while (IsWeakKey(KeyValue));
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
				throw new CryptographicException(Environment.GetResourceString("Specified key is not a valid size for this algorithm."));
			}
			if (IsWeakKey(value))
			{
				throw new CryptographicException(Environment.GetResourceString("Specified key is a known weak key for '{0}' and cannot be used."), "TripleDES");
			}
			KeyValue = (byte[])value.Clone();
			KeySizeValue = value.Length * 8;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.TripleDES" /> class.</summary>
	protected TripleDES()
	{
		KeySizeValue = 192;
		BlockSizeValue = 64;
		FeedbackSizeValue = BlockSizeValue;
		LegalBlockSizesValue = s_legalBlockSizes;
		LegalKeySizesValue = s_legalKeySizes;
	}

	/// <summary>Creates an instance of a cryptographic object to perform the <see cref="T:System.Security.Cryptography.TripleDES" /> algorithm.</summary>
	/// <returns>An instance of a cryptographic object.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public new static TripleDES Create()
	{
		return Create("System.Security.Cryptography.TripleDES");
	}

	/// <summary>Creates an instance of a cryptographic object to perform the specified implementation of the <see cref="T:System.Security.Cryptography.TripleDES" /> algorithm.</summary>
	/// <returns>An instance of a cryptographic object.</returns>
	/// <param name="str">The name of the specific implementation of <see cref="T:System.Security.Cryptography.TripleDES" /> to use. </param>
	public new static TripleDES Create(string str)
	{
		return (TripleDES)CryptoConfig.CreateFromName(str);
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
		byte[] array = Utils.FixupKeyParity(rgbKey);
		if (EqualBytes(array, 0, 8, 8))
		{
			return true;
		}
		if (array.Length == 24 && EqualBytes(array, 8, 16, 8))
		{
			return true;
		}
		return false;
	}

	private static bool EqualBytes(byte[] rgbKey, int start1, int start2, int count)
	{
		if (start1 < 0)
		{
			throw new ArgumentOutOfRangeException("start1", Environment.GetResourceString("Non-negative number required."));
		}
		if (start2 < 0)
		{
			throw new ArgumentOutOfRangeException("start2", Environment.GetResourceString("Non-negative number required."));
		}
		if (start1 + count > rgbKey.Length)
		{
			throw new ArgumentException(Environment.GetResourceString("Value was invalid."));
		}
		if (start2 + count > rgbKey.Length)
		{
			throw new ArgumentException(Environment.GetResourceString("Value was invalid."));
		}
		for (int i = 0; i < count; i++)
		{
			if (rgbKey[start1 + i] != rgbKey[start2 + i])
			{
				return false;
			}
		}
		return true;
	}

	private static bool IsLegalKeySize(byte[] rgbKey)
	{
		if (rgbKey != null && (rgbKey.Length == 16 || rgbKey.Length == 24))
		{
			return true;
		}
		return false;
	}
}
