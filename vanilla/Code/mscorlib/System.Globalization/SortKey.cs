using System.Runtime.InteropServices;
using Unity;

namespace System.Globalization;

/// <summary>Represents the result of mapping a string to its sort key.</summary>
[Serializable]
[StructLayout(LayoutKind.Sequential)]
[ComVisible(true)]
public class SortKey
{
	private readonly string source;

	private readonly byte[] key;

	private readonly CompareOptions options;

	private readonly int lcid;

	/// <summary>Gets the original string used to create the current <see cref="T:System.Globalization.SortKey" /> object.</summary>
	/// <returns>The original string used to create the current <see cref="T:System.Globalization.SortKey" /> object.</returns>
	public virtual string OriginalString => source;

	/// <summary>Gets the byte array representing the current <see cref="T:System.Globalization.SortKey" /> object.</summary>
	/// <returns>A byte array representing the current <see cref="T:System.Globalization.SortKey" /> object.</returns>
	public virtual byte[] KeyData => key;

	/// <summary>Compares two sort keys.</summary>
	/// <returns>A signed integer that indicates the relationship between <paramref name="sortkey1" /> and <paramref name="sortkey2" />.Value Condition Less than zero <paramref name="sortkey1" /> is less than <paramref name="sortkey2" />. Zero <paramref name="sortkey1" /> is equal to <paramref name="sortkey2" />. Greater than zero <paramref name="sortkey1" /> is greater than <paramref name="sortkey2" />. </returns>
	/// <param name="sortkey1">The first sort key to compare. </param>
	/// <param name="sortkey2">The second sort key to compare. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="sortkey1" /> or <paramref name="sortkey2" /> is null.</exception>
	public static int Compare(SortKey sortkey1, SortKey sortkey2)
	{
		if (sortkey1 == null)
		{
			throw new ArgumentNullException("sortkey1");
		}
		if (sortkey2 == null)
		{
			throw new ArgumentNullException("sortkey2");
		}
		if (sortkey1 == sortkey2 || (object)sortkey1.OriginalString == sortkey2.OriginalString)
		{
			return 0;
		}
		byte[] keyData = sortkey1.KeyData;
		byte[] keyData2 = sortkey2.KeyData;
		int num = ((keyData.Length > keyData2.Length) ? keyData2.Length : keyData.Length);
		for (int i = 0; i < num; i++)
		{
			if (keyData[i] != keyData2[i])
			{
				if (keyData[i] >= keyData2[i])
				{
					return 1;
				}
				return -1;
			}
		}
		if (keyData.Length != keyData2.Length)
		{
			if (keyData.Length >= keyData2.Length)
			{
				return 1;
			}
			return -1;
		}
		return 0;
	}

	internal SortKey(int lcid, string source, CompareOptions opt)
	{
		this.lcid = lcid;
		this.source = source;
		options = opt;
	}

	internal SortKey(int lcid, string source, byte[] buffer, CompareOptions opt, int lv1Length, int lv2Length, int lv3Length, int kanaSmallLength, int markTypeLength, int katakanaLength, int kanaWidthLength, int identLength)
	{
		this.lcid = lcid;
		this.source = source;
		key = buffer;
		options = opt;
	}

	/// <summary>Determines whether the specified object is equal to the current <see cref="T:System.Globalization.SortKey" /> object.</summary>
	/// <returns>true if the <paramref name="value" /> parameter is equal to the current <see cref="T:System.Globalization.SortKey" /> object; otherwise, false. </returns>
	/// <param name="value">The object to compare with the current <see cref="T:System.Globalization.SortKey" /> object. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public override bool Equals(object value)
	{
		if (value is SortKey sortKey && lcid == sortKey.lcid && options == sortKey.options && Compare(this, sortKey) == 0)
		{
			return true;
		}
		return false;
	}

	/// <summary>Serves as a hash function for the current <see cref="T:System.Globalization.SortKey" /> object that is suitable for hashing algorithms and data structures such as a hash table.</summary>
	/// <returns>A hash code for the current <see cref="T:System.Globalization.SortKey" /> object.</returns>
	public override int GetHashCode()
	{
		if (key.Length == 0)
		{
			return 0;
		}
		int num = key[0];
		for (int i = 1; i < key.Length; i++)
		{
			num ^= key[i] << (i & 3);
		}
		return num;
	}

	/// <summary>Returns a string that represents the current <see cref="T:System.Globalization.SortKey" /> object.</summary>
	/// <returns>A string that represents the current <see cref="T:System.Globalization.SortKey" /> object.</returns>
	public override string ToString()
	{
		return string.Concat("SortKey - ", lcid, ", ", options, ", ", source);
	}

	internal SortKey()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
