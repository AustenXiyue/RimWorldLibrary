using System.Runtime.InteropServices;
using System.Text;

namespace System.Security.Permissions;

/// <summary>Represents the public key information (called a blob) for a strong name. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class StrongNamePublicKeyBlob
{
	internal byte[] pubkey;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.StrongNamePublicKeyBlob" /> class with raw bytes of the public key blob.</summary>
	/// <param name="publicKey">The array of bytes representing the raw public key data. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="publicKey" /> parameter is null. </exception>
	public StrongNamePublicKeyBlob(byte[] publicKey)
	{
		if (publicKey == null)
		{
			throw new ArgumentNullException("publicKey");
		}
		pubkey = publicKey;
	}

	internal static StrongNamePublicKeyBlob FromString(string s)
	{
		if (s == null || s.Length == 0)
		{
			return null;
		}
		byte[] array = new byte[s.Length / 2];
		int num = 0;
		int num2 = 0;
		while (num < s.Length)
		{
			byte b = CharToByte(s[num]);
			byte b2 = CharToByte(s[num + 1]);
			array[num2] = Convert.ToByte(b * 16 + b2);
			num += 2;
			num2++;
		}
		return new StrongNamePublicKeyBlob(array);
	}

	private static byte CharToByte(char c)
	{
		char c2 = char.ToLowerInvariant(c);
		if (char.IsDigit(c2))
		{
			return (byte)(c2 - 48);
		}
		return (byte)(c2 - 97 + 10);
	}

	/// <summary>Gets or sets a value indicating whether the current public key blob is equal to the specified public key blob.</summary>
	/// <returns>true if the public key blob of the current object is equal to the public key blob of the <paramref name="obj" /> parameter; otherwise, false.</returns>
	/// <param name="obj">An object containing a public key blob. </param>
	public override bool Equals(object obj)
	{
		if (!(obj is StrongNamePublicKeyBlob strongNamePublicKeyBlob))
		{
			return false;
		}
		bool flag = pubkey.Length == strongNamePublicKeyBlob.pubkey.Length;
		if (flag)
		{
			for (int i = 0; i < pubkey.Length; i++)
			{
				if (pubkey[i] != strongNamePublicKeyBlob.pubkey[i])
				{
					return false;
				}
			}
		}
		return flag;
	}

	/// <summary>Returns a hash code based on the public key.</summary>
	/// <returns>The hash code based on the public key.</returns>
	public override int GetHashCode()
	{
		int num = 0;
		int num2 = 0;
		int num3 = Math.Min(pubkey.Length, 4);
		while (num2 < num3)
		{
			num = (num << 8) + pubkey[num2++];
		}
		return num;
	}

	/// <summary>Creates and returns a string representation of the public key blob.</summary>
	/// <returns>A hexadecimal version of the public key blob.</returns>
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < pubkey.Length; i++)
		{
			stringBuilder.Append(pubkey[i].ToString("X2"));
		}
		return stringBuilder.ToString();
	}
}
