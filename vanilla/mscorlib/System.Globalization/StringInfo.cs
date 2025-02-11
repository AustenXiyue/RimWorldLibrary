using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Globalization;

/// <summary>Provides functionality to split a string into text elements and to iterate through those text elements.</summary>
[Serializable]
[ComVisible(true)]
public class StringInfo
{
	[OptionalField(VersionAdded = 2)]
	private string m_str;

	[NonSerialized]
	private int[] m_indexes;

	private int[] Indexes
	{
		get
		{
			if (m_indexes == null && 0 < String.Length)
			{
				m_indexes = ParseCombiningCharacters(String);
			}
			return m_indexes;
		}
	}

	/// <summary>Gets or sets the value of the current <see cref="T:System.Globalization.StringInfo" /> object.</summary>
	/// <returns>The string that is the value of the current <see cref="T:System.Globalization.StringInfo" /> object.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value in a set operation is null.</exception>
	public string String
	{
		get
		{
			return m_str;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("String", Environment.GetResourceString("String reference not set to an instance of a String."));
			}
			m_str = value;
			m_indexes = null;
		}
	}

	/// <summary>Gets the number of text elements in the current <see cref="T:System.Globalization.StringInfo" /> object.</summary>
	/// <returns>The number of base characters, surrogate pairs, and combining character sequences in this <see cref="T:System.Globalization.StringInfo" /> object.</returns>
	public int LengthInTextElements
	{
		get
		{
			if (Indexes == null)
			{
				return 0;
			}
			return Indexes.Length;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Globalization.StringInfo" /> class. </summary>
	public StringInfo()
		: this("")
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Globalization.StringInfo" /> class to a specified string.</summary>
	/// <param name="value">A string to initialize this <see cref="T:System.Globalization.StringInfo" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public StringInfo(string value)
	{
		String = value;
	}

	[OnDeserializing]
	private void OnDeserializing(StreamingContext ctx)
	{
		m_str = string.Empty;
	}

	[OnDeserialized]
	private void OnDeserialized(StreamingContext ctx)
	{
		if (m_str.Length == 0)
		{
			m_indexes = null;
		}
	}

	/// <summary>Indicates whether the current <see cref="T:System.Globalization.StringInfo" /> object is equal to a specified object.</summary>
	/// <returns>true if the <paramref name="value" /> parameter is a <see cref="T:System.Globalization.StringInfo" /> object and its <see cref="P:System.Globalization.StringInfo.String" /> property equals the <see cref="P:System.Globalization.StringInfo.String" /> property of this <see cref="T:System.Globalization.StringInfo" /> object; otherwise, false.</returns>
	/// <param name="value">An object.</param>
	[ComVisible(false)]
	public override bool Equals(object value)
	{
		if (value is StringInfo stringInfo)
		{
			return m_str.Equals(stringInfo.m_str);
		}
		return false;
	}

	/// <summary>Calculates a hash code for the value of the current <see cref="T:System.Globalization.StringInfo" /> object.</summary>
	/// <returns>A 32-bit signed integer hash code based on the string value of this <see cref="T:System.Globalization.StringInfo" /> object.</returns>
	[ComVisible(false)]
	public override int GetHashCode()
	{
		return m_str.GetHashCode();
	}

	/// <summary>Retrieves a substring of text elements from the current <see cref="T:System.Globalization.StringInfo" /> object starting from a specified text element and continuing through the last text element.</summary>
	/// <returns>A substring of text elements in this <see cref="T:System.Globalization.StringInfo" /> object, starting from the text element index specified by the <paramref name="startingTextElement" /> parameter and continuing through the last text element in this object.</returns>
	/// <param name="startingTextElement">The zero-based index of a text element in this <see cref="T:System.Globalization.StringInfo" /> object.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startingTextElement" /> is less than zero.-or-The string that is the value of the current <see cref="T:System.Globalization.StringInfo" /> object is the empty string ("").</exception>
	public string SubstringByTextElements(int startingTextElement)
	{
		if (Indexes == null)
		{
			if (startingTextElement < 0)
			{
				throw new ArgumentOutOfRangeException("startingTextElement", Environment.GetResourceString("Positive number required."));
			}
			throw new ArgumentOutOfRangeException("startingTextElement", Environment.GetResourceString("Specified argument was out of the range of valid values."));
		}
		return SubstringByTextElements(startingTextElement, Indexes.Length - startingTextElement);
	}

	/// <summary>Retrieves a substring of text elements from the current <see cref="T:System.Globalization.StringInfo" /> object starting from a specified text element and continuing through the specified number of text elements.</summary>
	/// <returns>A substring of text elements in this <see cref="T:System.Globalization.StringInfo" /> object. The substring consists of the number of text elements specified by the <paramref name="lengthInTextElements" /> parameter and starts from the text element index specified by the <paramref name="startingTextElement" /> parameter.</returns>
	/// <param name="startingTextElement">The zero-based index of a text element in this <see cref="T:System.Globalization.StringInfo" /> object.</param>
	/// <param name="lengthInTextElements">The number of text elements to retrieve.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startingTextElement" /> is less than zero.-or-<paramref name="startingTextElement" /> is greater than or equal to the length of the string that is the value of the current <see cref="T:System.Globalization.StringInfo" /> object.-or-<paramref name="lengthInTextElements" /> is less than zero.-or-The string that is the value of the current <see cref="T:System.Globalization.StringInfo" /> object is the empty string ("").-or-<paramref name="startingTextElement" /> + <paramref name="lengthInTextElements" /> specify an index that is greater than the number of text elements in this <see cref="T:System.Globalization.StringInfo" /> object.</exception>
	public string SubstringByTextElements(int startingTextElement, int lengthInTextElements)
	{
		if (startingTextElement < 0)
		{
			throw new ArgumentOutOfRangeException("startingTextElement", Environment.GetResourceString("Positive number required."));
		}
		if (String.Length == 0 || startingTextElement >= Indexes.Length)
		{
			throw new ArgumentOutOfRangeException("startingTextElement", Environment.GetResourceString("Specified argument was out of the range of valid values."));
		}
		if (lengthInTextElements < 0)
		{
			throw new ArgumentOutOfRangeException("lengthInTextElements", Environment.GetResourceString("Positive number required."));
		}
		if (startingTextElement > Indexes.Length - lengthInTextElements)
		{
			throw new ArgumentOutOfRangeException("lengthInTextElements", Environment.GetResourceString("Specified argument was out of the range of valid values."));
		}
		int num = Indexes[startingTextElement];
		if (startingTextElement + lengthInTextElements == Indexes.Length)
		{
			return String.Substring(num);
		}
		return String.Substring(num, Indexes[lengthInTextElements + startingTextElement] - num);
	}

	/// <summary>Gets the first text element in a specified string.</summary>
	/// <returns>A string containing the first text element in the specified string.</returns>
	/// <param name="str">The string from which to get the text element. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="str" /> is null. </exception>
	public static string GetNextTextElement(string str)
	{
		return GetNextTextElement(str, 0);
	}

	internal static int GetCurrentTextElementLen(string str, int index, int len, ref UnicodeCategory ucCurrent, ref int currentCharCount)
	{
		if (index + currentCharCount == len)
		{
			return currentCharCount;
		}
		UnicodeCategory unicodeCategory = CharUnicodeInfo.InternalGetUnicodeCategory(str, index + currentCharCount, out var charLength);
		if (CharUnicodeInfo.IsCombiningCategory(unicodeCategory) && !CharUnicodeInfo.IsCombiningCategory(ucCurrent) && ucCurrent != UnicodeCategory.Format && ucCurrent != UnicodeCategory.Control && ucCurrent != UnicodeCategory.OtherNotAssigned && ucCurrent != UnicodeCategory.Surrogate)
		{
			int num = index;
			for (index += currentCharCount + charLength; index < len; index += charLength)
			{
				unicodeCategory = CharUnicodeInfo.InternalGetUnicodeCategory(str, index, out charLength);
				if (!CharUnicodeInfo.IsCombiningCategory(unicodeCategory))
				{
					ucCurrent = unicodeCategory;
					currentCharCount = charLength;
					break;
				}
			}
			return index - num;
		}
		int result = currentCharCount;
		ucCurrent = unicodeCategory;
		currentCharCount = charLength;
		return result;
	}

	/// <summary>Gets the text element at the specified index of the specified string.</summary>
	/// <returns>A string containing the text element at the specified index of the specified string.</returns>
	/// <param name="str">The string from which to get the text element. </param>
	/// <param name="index">The zero-based index at which the text element starts. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="str" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is outside the range of valid indexes for <paramref name="str" />. </exception>
	public static string GetNextTextElement(string str, int index)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		int length = str.Length;
		if (index < 0 || index >= length)
		{
			if (index == length)
			{
				return string.Empty;
			}
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		int charLength;
		UnicodeCategory ucCurrent = CharUnicodeInfo.InternalGetUnicodeCategory(str, index, out charLength);
		return str.Substring(index, GetCurrentTextElementLen(str, index, length, ref ucCurrent, ref charLength));
	}

	/// <summary>Returns an enumerator that iterates through the text elements of the entire string.</summary>
	/// <returns>A <see cref="T:System.Globalization.TextElementEnumerator" /> for the entire string.</returns>
	/// <param name="str">The string to iterate through. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="str" /> is null. </exception>
	public static TextElementEnumerator GetTextElementEnumerator(string str)
	{
		return GetTextElementEnumerator(str, 0);
	}

	/// <summary>Returns an enumerator that iterates through the text elements of the string, starting at the specified index.</summary>
	/// <returns>A <see cref="T:System.Globalization.TextElementEnumerator" /> for the string starting at <paramref name="index" />.</returns>
	/// <param name="str">The string to iterate through. </param>
	/// <param name="index">The zero-based index at which to start iterating. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="str" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is outside the range of valid indexes for <paramref name="str" />. </exception>
	public static TextElementEnumerator GetTextElementEnumerator(string str, int index)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		int length = str.Length;
		if (index < 0 || index > length)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		return new TextElementEnumerator(str, index, length);
	}

	/// <summary>Returns the indexes of each base character, high surrogate, or control character within the specified string.</summary>
	/// <returns>An array of integers that contains the zero-based indexes of each base character, high surrogate, or control character within the specified string.</returns>
	/// <param name="str">The string to search. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="str" /> is null. </exception>
	public static int[] ParseCombiningCharacters(string str)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		int length = str.Length;
		int[] array = new int[length];
		if (length == 0)
		{
			return array;
		}
		int num = 0;
		int i = 0;
		int charLength;
		for (UnicodeCategory ucCurrent = CharUnicodeInfo.InternalGetUnicodeCategory(str, 0, out charLength); i < length; i += GetCurrentTextElementLen(str, i, length, ref ucCurrent, ref charLength))
		{
			array[num++] = i;
		}
		if (num < length)
		{
			int[] array2 = new int[num];
			Array.Copy(array, array2, num);
			return array2;
		}
		return array;
	}
}
