using System.Runtime.InteropServices;

namespace System.Security.Cryptography;

/// <summary>Determines the set of valid key sizes for the symmetric cryptographic algorithms.</summary>
[ComVisible(true)]
public sealed class KeySizes
{
	private int m_minSize;

	private int m_maxSize;

	private int m_skipSize;

	/// <summary>Specifies the minimum key size in bits.</summary>
	/// <returns>The minimum key size in bits.</returns>
	public int MinSize => m_minSize;

	/// <summary>Specifies the maximum key size in bits.</summary>
	/// <returns>The maximum key size in bits.</returns>
	public int MaxSize => m_maxSize;

	/// <summary>Specifies the interval between valid key sizes in bits.</summary>
	/// <returns>The interval between valid key sizes in bits.</returns>
	public int SkipSize => m_skipSize;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.KeySizes" /> class with the specified key values.</summary>
	/// <param name="minSize">The minimum valid key size. </param>
	/// <param name="maxSize">The maximum valid key size. </param>
	/// <param name="skipSize">The interval between valid key sizes. </param>
	public KeySizes(int minSize, int maxSize, int skipSize)
	{
		m_minSize = minSize;
		m_maxSize = maxSize;
		m_skipSize = skipSize;
	}

	internal bool IsLegal(int keySize)
	{
		int num = keySize - MinSize;
		bool flag = num >= 0 && keySize <= MaxSize;
		if (SkipSize != 0)
		{
			if (flag)
			{
				return num % SkipSize == 0;
			}
			return false;
		}
		return flag;
	}

	internal static bool IsLegalKeySize(KeySizes[] legalKeys, int size)
	{
		for (int i = 0; i < legalKeys.Length; i++)
		{
			if (legalKeys[i].IsLegal(size))
			{
				return true;
			}
		}
		return false;
	}
}
