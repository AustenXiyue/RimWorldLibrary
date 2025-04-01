namespace System.Xml;

/// <summary>Implements a single-threaded <see cref="T:System.Xml.XmlNameTable" />.</summary>
public class NameTable : XmlNameTable
{
	private class Entry
	{
		internal string str;

		internal int hashCode;

		internal Entry next;

		internal Entry(string str, int hashCode, Entry next)
		{
			this.str = str;
			this.hashCode = hashCode;
			this.next = next;
		}
	}

	private Entry[] entries;

	private int count;

	private int mask;

	private int hashCodeRandomizer;

	/// <summary>Initializes a new instance of the NameTable class.</summary>
	public NameTable()
	{
		mask = 31;
		entries = new Entry[mask + 1];
		hashCodeRandomizer = Environment.TickCount;
	}

	/// <summary>Atomizes the specified string and adds it to the NameTable.</summary>
	/// <returns>The atomized string or the existing string if it already exists in the NameTable.</returns>
	/// <param name="key">The string to add. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null. </exception>
	public override string Add(string key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		int length = key.Length;
		if (length == 0)
		{
			return string.Empty;
		}
		int num = length + hashCodeRandomizer;
		for (int i = 0; i < key.Length; i++)
		{
			num += (num << 7) ^ key[i];
		}
		num -= num >> 17;
		num -= num >> 11;
		num -= num >> 5;
		for (Entry entry = entries[num & mask]; entry != null; entry = entry.next)
		{
			if (entry.hashCode == num && entry.str.Equals(key))
			{
				return entry.str;
			}
		}
		return AddEntry(key, num);
	}

	/// <summary>Atomizes the specified string and adds it to the NameTable.</summary>
	/// <returns>The atomized string or the existing string if one already exists in the NameTable. If <paramref name="len" /> is zero, String.Empty is returned.</returns>
	/// <param name="key">The character array containing the string to add. </param>
	/// <param name="start">The zero-based index into the array specifying the first character of the string. </param>
	/// <param name="len">The number of characters in the string. </param>
	/// <exception cref="T:System.IndexOutOfRangeException">0 &gt; <paramref name="start" />-or- <paramref name="start" /> &gt;= <paramref name="key" />.Length -or- <paramref name="len" /> &gt;= <paramref name="key" />.Length The above conditions do not cause an exception to be thrown if <paramref name="len" /> =0. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="len" /> &lt; 0. </exception>
	public override string Add(char[] key, int start, int len)
	{
		if (len == 0)
		{
			return string.Empty;
		}
		int num = len + hashCodeRandomizer;
		num += (num << 7) ^ key[start];
		int num2 = start + len;
		for (int i = start + 1; i < num2; i++)
		{
			num += (num << 7) ^ key[i];
		}
		num -= num >> 17;
		num -= num >> 11;
		num -= num >> 5;
		for (Entry entry = entries[num & mask]; entry != null; entry = entry.next)
		{
			if (entry.hashCode == num && TextEquals(entry.str, key, start, len))
			{
				return entry.str;
			}
		}
		return AddEntry(new string(key, start, len), num);
	}

	/// <summary>Gets the atomized string with the specified value.</summary>
	/// <returns>The atomized string object or null if the string has not already been atomized.</returns>
	/// <param name="value">The name to find. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	public override string Get(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value.Length == 0)
		{
			return string.Empty;
		}
		int num = value.Length + hashCodeRandomizer;
		for (int i = 0; i < value.Length; i++)
		{
			num += (num << 7) ^ value[i];
		}
		num -= num >> 17;
		num -= num >> 11;
		num -= num >> 5;
		for (Entry entry = entries[num & mask]; entry != null; entry = entry.next)
		{
			if (entry.hashCode == num && entry.str.Equals(value))
			{
				return entry.str;
			}
		}
		return null;
	}

	/// <summary>Gets the atomized string containing the same characters as the specified range of characters in the given array.</summary>
	/// <returns>The atomized string or null if the string has not already been atomized. If <paramref name="len" /> is zero, String.Empty is returned.</returns>
	/// <param name="key">The character array containing the name to find. </param>
	/// <param name="start">The zero-based index into the array specifying the first character of the name. </param>
	/// <param name="len">The number of characters in the name. </param>
	/// <exception cref="T:System.IndexOutOfRangeException">0 &gt; <paramref name="start" />-or- <paramref name="start" /> &gt;= <paramref name="key" />.Length -or- <paramref name="len" /> &gt;= <paramref name="key" />.Length The above conditions do not cause an exception to be thrown if <paramref name="len" /> =0. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="len" /> &lt; 0. </exception>
	public override string Get(char[] key, int start, int len)
	{
		if (len == 0)
		{
			return string.Empty;
		}
		int num = len + hashCodeRandomizer;
		num += (num << 7) ^ key[start];
		int num2 = start + len;
		for (int i = start + 1; i < num2; i++)
		{
			num += (num << 7) ^ key[i];
		}
		num -= num >> 17;
		num -= num >> 11;
		num -= num >> 5;
		for (Entry entry = entries[num & mask]; entry != null; entry = entry.next)
		{
			if (entry.hashCode == num && TextEquals(entry.str, key, start, len))
			{
				return entry.str;
			}
		}
		return null;
	}

	private string AddEntry(string str, int hashCode)
	{
		int num = hashCode & mask;
		Entry entry = new Entry(str, hashCode, entries[num]);
		entries[num] = entry;
		if (count++ == mask)
		{
			Grow();
		}
		return entry.str;
	}

	private void Grow()
	{
		int num = mask * 2 + 1;
		Entry[] array = entries;
		Entry[] array2 = new Entry[num + 1];
		for (int i = 0; i < array.Length; i++)
		{
			Entry entry = array[i];
			while (entry != null)
			{
				int num2 = entry.hashCode & num;
				Entry next = entry.next;
				entry.next = array2[num2];
				array2[num2] = entry;
				entry = next;
			}
		}
		entries = array2;
		mask = num;
	}

	private static bool TextEquals(string str1, char[] str2, int str2Start, int str2Length)
	{
		if (str1.Length != str2Length)
		{
			return false;
		}
		for (int i = 0; i < str1.Length; i++)
		{
			if (str1[i] != str2[str2Start + i])
			{
				return false;
			}
		}
		return true;
	}
}
