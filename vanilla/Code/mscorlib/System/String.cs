using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace System;

/// <summary>Represents text as a series of Unicode characters.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
public sealed class String : IComparable, ICloneable, IConvertible, IEnumerable, IComparable<string>, IEnumerable<char>, IEquatable<string>
{
	[NonSerialized]
	private int m_stringLength;

	[NonSerialized]
	private char m_firstChar;

	private const int TrimHead = 0;

	private const int TrimTail = 1;

	private const int TrimBoth = 2;

	/// <summary>Represents the empty string. This field is read-only.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly string Empty;

	private const int charPtrAlignConst = 1;

	private const int alignConst = 3;

	internal char FirstChar => m_firstChar;

	/// <summary>Gets the <see cref="T:System.Char" /> object at a specified position in the current <see cref="T:System.String" /> object.</summary>
	/// <returns>The object at position <paramref name="index" />.</returns>
	/// <param name="index">A position in the current string. </param>
	/// <exception cref="T:System.IndexOutOfRangeException">
	///   <paramref name="index" /> is greater than or equal to the length of this object or less than zero. </exception>
	/// <filterpriority>2</filterpriority>
	[IndexerName("Chars")]
	public unsafe char this[int index]
	{
		get
		{
			if (index < 0 || index >= m_stringLength)
			{
				throw new IndexOutOfRangeException();
			}
			fixed (char* firstChar = &m_firstChar)
			{
				return firstChar[index];
			}
		}
	}

	/// <summary>Gets the number of characters in the current <see cref="T:System.String" /> object.</summary>
	/// <returns>The number of characters in the current string.</returns>
	/// <filterpriority>1</filterpriority>
	public int Length => m_stringLength;

	/// <summary>Concatenates all the elements of a string array, using the specified separator between each element. </summary>
	/// <returns>A string that consists of the elements in <paramref name="value" /> delimited by the <paramref name="separator" /> string. If <paramref name="value" /> is an empty array, the method returns <see cref="F:System.String.Empty" />.</returns>
	/// <param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="value" /> has more than one element.</param>
	/// <param name="value">An array that contains the elements to concatenate. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public static string Join(string separator, params string[] value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return Join(separator, value, 0, value.Length);
	}

	/// <summary>Concatenates the elements of an object array, using the specified separator between each element.</summary>
	/// <returns>A string that consists of the elements of <paramref name="values" /> delimited by the <paramref name="separator" /> string. If <paramref name="values" /> is an empty array, the method returns <see cref="F:System.String.Empty" />.</returns>
	/// <param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="values" /> has more than one element.</param>
	/// <param name="values">An array that contains the elements to concatenate.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="values" /> is null. </exception>
	[ComVisible(false)]
	public static string Join(string separator, params object[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length == 0 || values[0] == null)
		{
			return Empty;
		}
		if ((object)separator == null)
		{
			separator = Empty;
		}
		StringBuilder stringBuilder = StringBuilderCache.Acquire();
		string text = values[0].ToString();
		if ((object)text != null)
		{
			stringBuilder.Append(text);
		}
		for (int i = 1; i < values.Length; i++)
		{
			stringBuilder.Append(separator);
			if (values[i] != null)
			{
				text = values[i].ToString();
				if ((object)text != null)
				{
					stringBuilder.Append(text);
				}
			}
		}
		return StringBuilderCache.GetStringAndRelease(stringBuilder);
	}

	/// <summary>Concatenates the members of a collection, using the specified separator between each member.</summary>
	/// <returns>A string that consists of the members of <paramref name="values" /> delimited by the <paramref name="separator" /> string. If <paramref name="values" /> has no members, the method returns <see cref="F:System.String.Empty" />.</returns>
	/// <param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="values" /> has more than one element.</param>
	/// <param name="values">A collection that contains the objects to concatenate.</param>
	/// <typeparam name="T">The type of the members of <paramref name="values" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="values" /> is null. </exception>
	[ComVisible(false)]
	public static string Join<T>(string separator, IEnumerable<T> values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if ((object)separator == null)
		{
			separator = Empty;
		}
		using IEnumerator<T> enumerator = values.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return Empty;
		}
		StringBuilder stringBuilder = StringBuilderCache.Acquire();
		if (enumerator.Current != null)
		{
			string text = enumerator.Current.ToString();
			if ((object)text != null)
			{
				stringBuilder.Append(text);
			}
		}
		while (enumerator.MoveNext())
		{
			stringBuilder.Append(separator);
			if (enumerator.Current != null)
			{
				string text2 = enumerator.Current.ToString();
				if ((object)text2 != null)
				{
					stringBuilder.Append(text2);
				}
			}
		}
		return StringBuilderCache.GetStringAndRelease(stringBuilder);
	}

	/// <summary>Concatenates the members of a constructed <see cref="T:System.Collections.Generic.IEnumerable`1" /> collection of type <see cref="T:System.String" />, using the specified separator between each member.</summary>
	/// <returns>A string that consists of the members of <paramref name="values" /> delimited by the <paramref name="separator" /> string. If <paramref name="values" /> has no members, the method returns <see cref="F:System.String.Empty" />.</returns>
	/// <param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="values" /> has more than one element.</param>
	/// <param name="values">A collection that contains the strings to concatenate.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="values" /> is null. </exception>
	[ComVisible(false)]
	public static string Join(string separator, IEnumerable<string> values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if ((object)separator == null)
		{
			separator = Empty;
		}
		using IEnumerator<string> enumerator = values.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return Empty;
		}
		StringBuilder stringBuilder = StringBuilderCache.Acquire();
		if ((object)enumerator.Current != null)
		{
			stringBuilder.Append(enumerator.Current);
		}
		while (enumerator.MoveNext())
		{
			stringBuilder.Append(separator);
			if ((object)enumerator.Current != null)
			{
				stringBuilder.Append(enumerator.Current);
			}
		}
		return StringBuilderCache.GetStringAndRelease(stringBuilder);
	}

	/// <summary>Concatenates the specified elements of a string array, using the specified separator between each element. </summary>
	/// <returns>A string that consists of the strings in <paramref name="value" /> delimited by the <paramref name="separator" /> string. -or-<see cref="F:System.String.Empty" /> if <paramref name="count" /> is zero, <paramref name="value" /> has no elements, or <paramref name="separator" /> and all the elements of <paramref name="value" /> are <see cref="F:System.String.Empty" />.</returns>
	/// <param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="value" /> has more than one element.</param>
	/// <param name="value">An array that contains the elements to concatenate. </param>
	/// <param name="startIndex">The first element in <paramref name="value" /> to use. </param>
	/// <param name="count">The number of elements of <paramref name="value" /> to use. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> or <paramref name="count" /> is less than 0.-or- <paramref name="startIndex" /> plus <paramref name="count" /> is greater than the number of elements in <paramref name="value" />. </exception>
	/// <exception cref="T:System.OutOfMemoryException">Out of memory.</exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe static string Join(string separator, string[] value, int startIndex, int count)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (startIndex < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("StartIndex cannot be less than zero."));
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Count cannot be less than zero."));
		}
		if (startIndex > value.Length - count)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Index and count must refer to a location within the buffer."));
		}
		if ((object)separator == null)
		{
			separator = Empty;
		}
		if (count == 0)
		{
			return Empty;
		}
		int num = 0;
		int num2 = startIndex + count - 1;
		for (int i = startIndex; i <= num2; i++)
		{
			if ((object)value[i] != null)
			{
				num += value[i].Length;
			}
		}
		num += (count - 1) * separator.Length;
		if (num < 0 || num + 1 < 0)
		{
			throw new OutOfMemoryException();
		}
		if (num == 0)
		{
			return Empty;
		}
		string text = FastAllocateString(num);
		fixed (char* firstChar = &text.m_firstChar)
		{
			UnSafeCharBuffer unSafeCharBuffer = new UnSafeCharBuffer(firstChar, num);
			unSafeCharBuffer.AppendString(value[startIndex]);
			for (int j = startIndex + 1; j <= num2; j++)
			{
				unSafeCharBuffer.AppendString(separator);
				unSafeCharBuffer.AppendString(value[j]);
			}
		}
		return text;
	}

	[SecuritySafeCritical]
	private unsafe static int CompareOrdinalIgnoreCaseHelper(string strA, string strB)
	{
		int num = Math.Min(strA.Length, strB.Length);
		fixed (char* firstChar = &strA.m_firstChar)
		{
			fixed (char* firstChar2 = &strB.m_firstChar)
			{
				char* ptr = firstChar;
				char* ptr2 = firstChar2;
				while (num != 0)
				{
					int num2 = *ptr;
					int num3 = *ptr2;
					if ((uint)(num2 - 97) <= 25u)
					{
						num2 -= 32;
					}
					if ((uint)(num3 - 97) <= 25u)
					{
						num3 -= 32;
					}
					if (num2 != num3)
					{
						return num2 - num3;
					}
					ptr++;
					ptr2++;
					num--;
				}
				return strA.Length - strB.Length;
			}
		}
	}

	[SecuritySafeCritical]
	internal unsafe static string SmallCharToUpper(string strIn)
	{
		int length = strIn.Length;
		string text = FastAllocateString(length);
		fixed (char* firstChar = &strIn.m_firstChar)
		{
			fixed (char* firstChar2 = &text.m_firstChar)
			{
				for (int i = 0; i < length; i++)
				{
					int num = firstChar[i];
					if ((uint)(num - 97) <= 25u)
					{
						num -= 32;
					}
					firstChar2[i] = (char)num;
				}
			}
		}
		return text;
	}

	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	[SecuritySafeCritical]
	private unsafe static bool EqualsHelper(string strA, string strB)
	{
		int num = strA.Length;
		fixed (char* firstChar = &strA.m_firstChar)
		{
			fixed (char* firstChar2 = &strB.m_firstChar)
			{
				char* ptr = firstChar;
				char* ptr2 = firstChar2;
				if (Environment.Is64BitProcess)
				{
					while (num >= 12)
					{
						if (*(long*)ptr != *(long*)ptr2)
						{
							return false;
						}
						if (*(long*)(ptr + 4) != *(long*)(ptr2 + 4))
						{
							return false;
						}
						if (*(long*)(ptr + 8) != *(long*)(ptr2 + 8))
						{
							return false;
						}
						ptr += 12;
						ptr2 += 12;
						num -= 12;
					}
				}
				else
				{
					while (num >= 10)
					{
						if (*(int*)ptr != *(int*)ptr2)
						{
							return false;
						}
						if (*(int*)(ptr + 2) != *(int*)(ptr2 + 2))
						{
							return false;
						}
						if (*(int*)(ptr + 4) != *(int*)(ptr2 + 4))
						{
							return false;
						}
						if (*(int*)(ptr + 6) != *(int*)(ptr2 + 6))
						{
							return false;
						}
						if (*(int*)(ptr + 8) != *(int*)(ptr2 + 8))
						{
							return false;
						}
						ptr += 10;
						ptr2 += 10;
						num -= 10;
					}
				}
				while (num > 0 && *(int*)ptr == *(int*)ptr2)
				{
					ptr += 2;
					ptr2 += 2;
					num -= 2;
				}
				return num <= 0;
			}
		}
	}

	[SecuritySafeCritical]
	private unsafe static int CompareOrdinalHelper(string strA, string strB)
	{
		int num = Math.Min(strA.Length, strB.Length);
		int num2 = -1;
		fixed (char* firstChar = &strA.m_firstChar)
		{
			fixed (char* firstChar2 = &strB.m_firstChar)
			{
				char* ptr = firstChar;
				char* ptr2 = firstChar2;
				while (num >= 10)
				{
					if (*(int*)ptr != *(int*)ptr2)
					{
						num2 = 0;
						break;
					}
					if (*(int*)(ptr + 2) != *(int*)(ptr2 + 2))
					{
						num2 = 2;
						break;
					}
					if (*(int*)(ptr + 4) != *(int*)(ptr2 + 4))
					{
						num2 = 4;
						break;
					}
					if (*(int*)(ptr + 6) != *(int*)(ptr2 + 6))
					{
						num2 = 6;
						break;
					}
					if (*(int*)(ptr + 8) != *(int*)(ptr2 + 8))
					{
						num2 = 8;
						break;
					}
					ptr += 10;
					ptr2 += 10;
					num -= 10;
				}
				if (num2 != -1)
				{
					ptr += num2;
					ptr2 += num2;
					int result;
					if ((result = *ptr - *ptr2) != 0)
					{
						return result;
					}
					return ptr[1] - ptr2[1];
				}
				while (num > 0 && *(int*)ptr == *(int*)ptr2)
				{
					ptr += 2;
					ptr2 += 2;
					num -= 2;
				}
				if (num > 0)
				{
					int result2;
					if ((result2 = *ptr - *ptr2) != 0)
					{
						return result2;
					}
					return ptr[1] - ptr2[1];
				}
				return strA.Length - strB.Length;
			}
		}
	}

	/// <summary>Determines whether this instance and a specified object, which must also be a <see cref="T:System.String" /> object, have the same value.</summary>
	/// <returns>true if <paramref name="obj" /> is a <see cref="T:System.String" /> and its value is the same as this instance; otherwise, false.</returns>
	/// <param name="obj">The string to compare to this instance. </param>
	/// <filterpriority>2</filterpriority>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	public override bool Equals(object obj)
	{
		if ((object)this == null)
		{
			throw new NullReferenceException();
		}
		if (!(obj is string text))
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (Length != text.Length)
		{
			return false;
		}
		return EqualsHelper(this, text);
	}

	/// <summary>Determines whether this instance and another specified <see cref="T:System.String" /> object have the same value.</summary>
	/// <returns>true if the value of the <paramref name="value" /> parameter is the same as this instance; otherwise, false.</returns>
	/// <param name="value">The string to compare to this instance. </param>
	/// <filterpriority>2</filterpriority>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	public bool Equals(string value)
	{
		if ((object)this == null)
		{
			throw new NullReferenceException();
		}
		if ((object)value == null)
		{
			return false;
		}
		if ((object)this == value)
		{
			return true;
		}
		if (Length != value.Length)
		{
			return false;
		}
		return EqualsHelper(this, value);
	}

	/// <summary>Determines whether this string and a specified <see cref="T:System.String" /> object have the same value. A parameter specifies the culture, case, and sort rules used in the comparison.</summary>
	/// <returns>true if the value of the <paramref name="value" /> parameter is the same as this string; otherwise, false.</returns>
	/// <param name="value">The string to compare to this instance.</param>
	/// <param name="comparisonType">One of the enumeration values that specifies how the strings will be compared. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="comparisonType" /> is not a <see cref="T:System.StringComparison" /> value. </exception>
	/// <filterpriority>2</filterpriority>
	[SecuritySafeCritical]
	public bool Equals(string value, StringComparison comparisonType)
	{
		if (comparisonType < StringComparison.CurrentCulture || comparisonType > StringComparison.OrdinalIgnoreCase)
		{
			throw new ArgumentException(Environment.GetResourceString("The string comparison type passed in is currently not supported."), "comparisonType");
		}
		if ((object)this == value)
		{
			return true;
		}
		if ((object)value == null)
		{
			return false;
		}
		switch (comparisonType)
		{
		case StringComparison.CurrentCulture:
			return CultureInfo.CurrentCulture.CompareInfo.Compare(this, value, CompareOptions.None) == 0;
		case StringComparison.CurrentCultureIgnoreCase:
			return CultureInfo.CurrentCulture.CompareInfo.Compare(this, value, CompareOptions.IgnoreCase) == 0;
		case StringComparison.InvariantCulture:
			return CultureInfo.InvariantCulture.CompareInfo.Compare(this, value, CompareOptions.None) == 0;
		case StringComparison.InvariantCultureIgnoreCase:
			return CultureInfo.InvariantCulture.CompareInfo.Compare(this, value, CompareOptions.IgnoreCase) == 0;
		case StringComparison.Ordinal:
			if (Length != value.Length)
			{
				return false;
			}
			return EqualsHelper(this, value);
		case StringComparison.OrdinalIgnoreCase:
			if (Length != value.Length)
			{
				return false;
			}
			if (IsAscii() && value.IsAscii())
			{
				return CompareOrdinalIgnoreCaseHelper(this, value) == 0;
			}
			return TextInfo.CompareOrdinalIgnoreCase(this, value) == 0;
		default:
			throw new ArgumentException(Environment.GetResourceString("The string comparison type passed in is currently not supported."), "comparisonType");
		}
	}

	/// <summary>Determines whether two specified <see cref="T:System.String" /> objects have the same value.</summary>
	/// <returns>true if the value of <paramref name="a" /> is the same as the value of <paramref name="b" />; otherwise, false. If both <paramref name="a" /> and <paramref name="b" /> are null, the method returns true.</returns>
	/// <param name="a">The first string to compare, or null. </param>
	/// <param name="b">The second string to compare, or null. </param>
	/// <filterpriority>1</filterpriority>
	public static bool Equals(string a, string b)
	{
		if ((object)a == b)
		{
			return true;
		}
		if ((object)a == null || (object)b == null)
		{
			return false;
		}
		if (a.Length != b.Length)
		{
			return false;
		}
		return EqualsHelper(a, b);
	}

	/// <summary>Determines whether two specified <see cref="T:System.String" /> objects have the same value. A parameter specifies the culture, case, and sort rules used in the comparison.</summary>
	/// <returns>true if the value of the <paramref name="a" /> parameter is equal to the value of the <paramref name="b" /> parameter; otherwise, false.</returns>
	/// <param name="a">The first string to compare, or null. </param>
	/// <param name="b">The second string to compare, or null. </param>
	/// <param name="comparisonType">One of the enumeration values that specifies the rules for the comparison. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="comparisonType" /> is not a <see cref="T:System.StringComparison" /> value. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static bool Equals(string a, string b, StringComparison comparisonType)
	{
		if (comparisonType < StringComparison.CurrentCulture || comparisonType > StringComparison.OrdinalIgnoreCase)
		{
			throw new ArgumentException(Environment.GetResourceString("The string comparison type passed in is currently not supported."), "comparisonType");
		}
		if ((object)a == b)
		{
			return true;
		}
		if ((object)a == null || (object)b == null)
		{
			return false;
		}
		switch (comparisonType)
		{
		case StringComparison.CurrentCulture:
			return CultureInfo.CurrentCulture.CompareInfo.Compare(a, b, CompareOptions.None) == 0;
		case StringComparison.CurrentCultureIgnoreCase:
			return CultureInfo.CurrentCulture.CompareInfo.Compare(a, b, CompareOptions.IgnoreCase) == 0;
		case StringComparison.InvariantCulture:
			return CultureInfo.InvariantCulture.CompareInfo.Compare(a, b, CompareOptions.None) == 0;
		case StringComparison.InvariantCultureIgnoreCase:
			return CultureInfo.InvariantCulture.CompareInfo.Compare(a, b, CompareOptions.IgnoreCase) == 0;
		case StringComparison.Ordinal:
			if (a.Length != b.Length)
			{
				return false;
			}
			return EqualsHelper(a, b);
		case StringComparison.OrdinalIgnoreCase:
			if (a.Length != b.Length)
			{
				return false;
			}
			if (a.IsAscii() && b.IsAscii())
			{
				return CompareOrdinalIgnoreCaseHelper(a, b) == 0;
			}
			return TextInfo.CompareOrdinalIgnoreCase(a, b) == 0;
		default:
			throw new ArgumentException(Environment.GetResourceString("The string comparison type passed in is currently not supported."), "comparisonType");
		}
	}

	/// <summary>Determines whether two specified strings have the same value.</summary>
	/// <returns>true if the value of <paramref name="a" /> is the same as the value of <paramref name="b" />; otherwise, false.</returns>
	/// <param name="a">The first string to compare, or null. </param>
	/// <param name="b">The second string to compare, or null. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator ==(string a, string b)
	{
		return Equals(a, b);
	}

	/// <summary>Determines whether two specified strings have different values.</summary>
	/// <returns>true if the value of <paramref name="a" /> is different from the value of <paramref name="b" />; otherwise, false.</returns>
	/// <param name="a">The first string to compare, or null. </param>
	/// <param name="b">The second string to compare, or null. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator !=(string a, string b)
	{
		return !Equals(a, b);
	}

	/// <summary>Copies a specified number of characters from a specified position in this instance to a specified position in an array of Unicode characters.</summary>
	/// <param name="sourceIndex">The index of the first character in this instance to copy. </param>
	/// <param name="destination">An array of Unicode characters to which characters in this instance are copied. </param>
	/// <param name="destinationIndex">The index in <paramref name="destination" /> at which the copy operation begins. </param>
	/// <param name="count">The number of characters in this instance to copy to <paramref name="destination" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destination" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="sourceIndex" />, <paramref name="destinationIndex" />, or <paramref name="count" /> is negative -or- <paramref name="count" /> is greater than the length of the substring from <paramref name="startIndex" /> to the end of this instance -or- <paramref name="count" /> is greater than the length of the subarray from <paramref name="destinationIndex" /> to the end of <paramref name="destination" /></exception>
	/// <filterpriority>2</filterpriority>
	[SecuritySafeCritical]
	public unsafe void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Count cannot be less than zero."));
		}
		if (sourceIndex < 0)
		{
			throw new ArgumentOutOfRangeException("sourceIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (count > Length - sourceIndex)
		{
			throw new ArgumentOutOfRangeException("sourceIndex", Environment.GetResourceString("Index and count must refer to a location within the string."));
		}
		if (destinationIndex > destination.Length - count || destinationIndex < 0)
		{
			throw new ArgumentOutOfRangeException("destinationIndex", Environment.GetResourceString("Index and count must refer to a location within the string."));
		}
		if (count <= 0)
		{
			return;
		}
		fixed (char* firstChar = &m_firstChar)
		{
			fixed (char* ptr = destination)
			{
				wstrcpy(ptr + destinationIndex, firstChar + sourceIndex, count);
			}
		}
	}

	/// <summary>Copies the characters in this instance to a Unicode character array.</summary>
	/// <returns>A Unicode character array whose elements are the individual characters of this instance. If this instance is an empty string, the returned array is empty and has a zero length.</returns>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe char[] ToCharArray()
	{
		int length = Length;
		char[] array = new char[length];
		if (length > 0)
		{
			fixed (char* firstChar = &m_firstChar)
			{
				fixed (char* dmem = array)
				{
					wstrcpy(dmem, firstChar, length);
				}
			}
		}
		return array;
	}

	/// <summary>Copies the characters in a specified substring in this instance to a Unicode character array.</summary>
	/// <returns>A Unicode character array whose elements are the <paramref name="length" /> number of characters in this instance starting from character position <paramref name="startIndex" />.</returns>
	/// <param name="startIndex">The starting position of a substring in this instance. </param>
	/// <param name="length">The length of the substring in this instance. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> or <paramref name="length" /> is less than zero.-or- <paramref name="startIndex" /> plus <paramref name="length" /> is greater than the length of this instance. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe char[] ToCharArray(int startIndex, int length)
	{
		if (startIndex < 0 || startIndex > Length || startIndex > Length - length)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		char[] array = new char[length];
		if (length > 0)
		{
			fixed (char* firstChar = &m_firstChar)
			{
				fixed (char* dmem = array)
				{
					wstrcpy(dmem, firstChar + startIndex, length);
				}
			}
		}
		return array;
	}

	/// <summary>Indicates whether the specified string is null or an <see cref="F:System.String.Empty" /> string.</summary>
	/// <returns>true if the <paramref name="value" /> parameter is null or an empty string (""); otherwise, false.</returns>
	/// <param name="value">The string to test. </param>
	/// <filterpriority>1</filterpriority>
	public static bool IsNullOrEmpty(string value)
	{
		if ((object)value != null)
		{
			return value.Length == 0;
		}
		return true;
	}

	/// <summary>Indicates whether a specified string is null, empty, or consists only of white-space characters.</summary>
	/// <returns>true if the <paramref name="value" /> parameter is null or <see cref="F:System.String.Empty" />, or if <paramref name="value" /> consists exclusively of white-space characters. </returns>
	/// <param name="value">The string to test.</param>
	public static bool IsNullOrWhiteSpace(string value)
	{
		if ((object)value == null)
		{
			return true;
		}
		for (int i = 0; i < value.Length; i++)
		{
			if (!char.IsWhiteSpace(value[i]))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Returns the hash code for this string.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <filterpriority>2</filterpriority>
	[SecuritySafeCritical]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	public unsafe override int GetHashCode()
	{
		fixed (char* ptr = this)
		{
			int num = 5381;
			int num2 = num;
			char* ptr2 = ptr;
			int num3;
			while ((num3 = *ptr2) != 0)
			{
				num = ((num << 5) + num) ^ num3;
				num3 = ptr2[1];
				if (num3 == 0)
				{
					break;
				}
				num2 = ((num2 << 5) + num2) ^ num3;
				ptr2 += 2;
			}
			return num + num2 * 1566083941;
		}
	}

	[SecuritySafeCritical]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	internal unsafe int GetLegacyNonRandomizedHashCode()
	{
		fixed (char* ptr = this)
		{
			int num = 5381;
			int num2 = num;
			char* ptr2 = ptr;
			int num3;
			while ((num3 = *ptr2) != 0)
			{
				num = ((num << 5) + num) ^ num3;
				num3 = ptr2[1];
				if (num3 == 0)
				{
					break;
				}
				num2 = ((num2 << 5) + num2) ^ num3;
				ptr2 += 2;
			}
			return num + num2 * 1566083941;
		}
	}

	/// <summary>Returns a string array that contains the substrings in this instance that are delimited by elements of a specified Unicode character array.</summary>
	/// <returns>An array whose elements contain the substrings in this instance that are delimited by one or more characters in <paramref name="separator" />. For more information, see the Remarks section.</returns>
	/// <param name="separator">An array of Unicode characters that delimit the substrings in this instance, an empty array that contains no delimiters, or null. </param>
	/// <filterpriority>1</filterpriority>
	public string[] Split(params char[] separator)
	{
		return SplitInternal(separator, int.MaxValue, StringSplitOptions.None);
	}

	/// <summary>Returns a string array that contains the substrings in this instance that are delimited by elements of a specified Unicode character array. A parameter specifies the maximum number of substrings to return.</summary>
	/// <returns>An array whose elements contain the substrings in this instance that are delimited by one or more characters in <paramref name="separator" />. For more information, see the Remarks section.</returns>
	/// <param name="separator">An array of Unicode characters that delimit the substrings in this instance, an empty array that contains no delimiters, or null. </param>
	/// <param name="count">The maximum number of substrings to return. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is negative. </exception>
	/// <filterpriority>1</filterpriority>
	public string[] Split(char[] separator, int count)
	{
		return SplitInternal(separator, count, StringSplitOptions.None);
	}

	/// <summary>Returns a string array that contains the substrings in this string that are delimited by elements of a specified Unicode character array. A parameter specifies whether to return empty array elements.</summary>
	/// <returns>An array whose elements contain the substrings in this string that are delimited by one or more characters in <paramref name="separator" />. For more information, see the Remarks section.</returns>
	/// <param name="separator">An array of Unicode characters that delimit the substrings in this string, an empty array that contains no delimiters, or null. </param>
	/// <param name="options">
	///   <see cref="F:System.StringSplitOptions.RemoveEmptyEntries" /> to omit empty array elements from the array returned; or <see cref="F:System.StringSplitOptions.None" /> to include empty array elements in the array returned. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> is not one of the <see cref="T:System.StringSplitOptions" /> values.</exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(false)]
	public string[] Split(char[] separator, StringSplitOptions options)
	{
		return SplitInternal(separator, int.MaxValue, options);
	}

	/// <summary>Returns a string array that contains the substrings in this string that are delimited by elements of a specified Unicode character array. Parameters specify the maximum number of substrings to return and whether to return empty array elements.</summary>
	/// <returns>An array whose elements contain the substrings in this string that are delimited by one or more characters in <paramref name="separator" />. For more information, see the Remarks section.</returns>
	/// <param name="separator">An array of Unicode characters that delimit the substrings in this string, an empty array that contains no delimiters, or null. </param>
	/// <param name="count">The maximum number of substrings to return. </param>
	/// <param name="options">
	///   <see cref="F:System.StringSplitOptions.RemoveEmptyEntries" /> to omit empty array elements from the array returned; or <see cref="F:System.StringSplitOptions.None" /> to include empty array elements in the array returned. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is negative. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> is not one of the <see cref="T:System.StringSplitOptions" /> values.</exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(false)]
	public string[] Split(char[] separator, int count, StringSplitOptions options)
	{
		return SplitInternal(separator, count, options);
	}

	[ComVisible(false)]
	internal string[] SplitInternal(char[] separator, int count, StringSplitOptions options)
	{
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Count cannot be less than zero."));
		}
		if (options < StringSplitOptions.None || options > StringSplitOptions.RemoveEmptyEntries)
		{
			throw new ArgumentException(Environment.GetResourceString("Illegal enum value: {0}.", options));
		}
		bool flag = options == StringSplitOptions.RemoveEmptyEntries;
		if (count == 0 || (flag && Length == 0))
		{
			return new string[0];
		}
		int[] sepList = new int[Length];
		int num = MakeSeparatorList(separator, ref sepList);
		if (num == 0 || count == 1)
		{
			return new string[1] { this };
		}
		if (flag)
		{
			return InternalSplitOmitEmptyEntries(sepList, null, num, count);
		}
		return InternalSplitKeepEmptyEntries(sepList, null, num, count);
	}

	/// <summary>Returns a string array that contains the substrings in this string that are delimited by elements of a specified string array. A parameter specifies whether to return empty array elements.</summary>
	/// <returns>An array whose elements contain the substrings in this string that are delimited by one or more strings in <paramref name="separator" />. For more information, see the Remarks section.</returns>
	/// <param name="separator">An array of strings that delimit the substrings in this string, an empty array that contains no delimiters, or null. </param>
	/// <param name="options">
	///   <see cref="F:System.StringSplitOptions.RemoveEmptyEntries" /> to omit empty array elements from the array returned; or <see cref="F:System.StringSplitOptions.None" /> to include empty array elements in the array returned. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> is not one of the <see cref="T:System.StringSplitOptions" /> values.</exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(false)]
	public string[] Split(string[] separator, StringSplitOptions options)
	{
		return Split(separator, int.MaxValue, options);
	}

	/// <summary>Returns a string array that contains the substrings in this string that are delimited by elements of a specified string array. Parameters specify the maximum number of substrings to return and whether to return empty array elements.</summary>
	/// <returns>An array whose elements contain the substrings in this string that are delimited by one or more strings in <paramref name="separator" />. For more information, see the Remarks section.</returns>
	/// <param name="separator">An array of strings that delimit the substrings in this string, an empty array that contains no delimiters, or null. </param>
	/// <param name="count">The maximum number of substrings to return. </param>
	/// <param name="options">
	///   <see cref="F:System.StringSplitOptions.RemoveEmptyEntries" /> to omit empty array elements from the array returned; or <see cref="F:System.StringSplitOptions.None" /> to include empty array elements in the array returned. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is negative. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> is not one of the <see cref="T:System.StringSplitOptions" /> values.</exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(false)]
	public string[] Split(string[] separator, int count, StringSplitOptions options)
	{
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Count cannot be less than zero."));
		}
		if (options < StringSplitOptions.None || options > StringSplitOptions.RemoveEmptyEntries)
		{
			throw new ArgumentException(Environment.GetResourceString("Illegal enum value: {0}.", (int)options));
		}
		bool flag = options == StringSplitOptions.RemoveEmptyEntries;
		if (separator == null || separator.Length == 0)
		{
			return SplitInternal(null, count, options);
		}
		if (count == 0 || (flag && Length == 0))
		{
			return new string[0];
		}
		int[] sepList = new int[Length];
		int[] lengthList = new int[Length];
		int num = MakeSeparatorList(separator, ref sepList, ref lengthList);
		if (num == 0 || count == 1)
		{
			return new string[1] { this };
		}
		if (flag)
		{
			return InternalSplitOmitEmptyEntries(sepList, lengthList, num, count);
		}
		return InternalSplitKeepEmptyEntries(sepList, lengthList, num, count);
	}

	private string[] InternalSplitKeepEmptyEntries(int[] sepList, int[] lengthList, int numReplaces, int count)
	{
		int num = 0;
		int num2 = 0;
		count--;
		int num3 = ((numReplaces < count) ? numReplaces : count);
		string[] array = new string[num3 + 1];
		for (int i = 0; i < num3; i++)
		{
			if (num >= Length)
			{
				break;
			}
			array[num2++] = Substring(num, sepList[i] - num);
			num = sepList[i] + ((lengthList == null) ? 1 : lengthList[i]);
		}
		if (num < Length && num3 >= 0)
		{
			array[num2] = Substring(num);
		}
		else if (num2 == num3)
		{
			array[num2] = Empty;
		}
		return array;
	}

	private string[] InternalSplitOmitEmptyEntries(int[] sepList, int[] lengthList, int numReplaces, int count)
	{
		int num = ((numReplaces < count) ? (numReplaces + 1) : count);
		string[] array = new string[num];
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < numReplaces; i++)
		{
			if (num2 >= Length)
			{
				break;
			}
			if (sepList[i] - num2 > 0)
			{
				array[num3++] = Substring(num2, sepList[i] - num2);
			}
			num2 = sepList[i] + ((lengthList == null) ? 1 : lengthList[i]);
			if (num3 == count - 1)
			{
				while (i < numReplaces - 1 && num2 == sepList[++i])
				{
					num2 += ((lengthList == null) ? 1 : lengthList[i]);
				}
				break;
			}
		}
		if (num2 < Length)
		{
			array[num3++] = Substring(num2);
		}
		string[] array2 = array;
		if (num3 != num)
		{
			array2 = new string[num3];
			for (int j = 0; j < num3; j++)
			{
				array2[j] = array[j];
			}
		}
		return array2;
	}

	[SecuritySafeCritical]
	private unsafe int MakeSeparatorList(char[] separator, ref int[] sepList)
	{
		int num = 0;
		if (separator == null || separator.Length == 0)
		{
			fixed (char* firstChar = &m_firstChar)
			{
				for (int i = 0; i < Length; i++)
				{
					if (num >= sepList.Length)
					{
						break;
					}
					if (char.IsWhiteSpace(firstChar[i]))
					{
						sepList[num++] = i;
					}
				}
			}
		}
		else
		{
			int num2 = sepList.Length;
			int num3 = separator.Length;
			fixed (char* firstChar2 = &m_firstChar)
			{
				fixed (char* ptr = separator)
				{
					for (int j = 0; j < Length; j++)
					{
						if (num >= num2)
						{
							break;
						}
						char* ptr2 = ptr;
						int num4 = 0;
						while (num4 < num3)
						{
							if (firstChar2[j] == *ptr2)
							{
								sepList[num++] = j;
								break;
							}
							num4++;
							ptr2++;
						}
					}
				}
			}
		}
		return num;
	}

	[SecuritySafeCritical]
	private unsafe int MakeSeparatorList(string[] separators, ref int[] sepList, ref int[] lengthList)
	{
		int num = 0;
		int num2 = sepList.Length;
		_ = separators.Length;
		fixed (char* firstChar = &m_firstChar)
		{
			for (int i = 0; i < Length; i++)
			{
				if (num >= num2)
				{
					break;
				}
				foreach (string text in separators)
				{
					if (!IsNullOrEmpty(text))
					{
						int length = text.Length;
						if (firstChar[i] == text[0] && length <= Length - i && (length == 1 || CompareOrdinal(this, i, text, 0, length) == 0))
						{
							sepList[num] = i;
							lengthList[num] = length;
							num++;
							i += length - 1;
							break;
						}
					}
				}
			}
		}
		return num;
	}

	/// <summary>Retrieves a substring from this instance. The substring starts at a specified character position and continues to the end of the string.</summary>
	/// <returns>A string that is equivalent to the substring that begins at <paramref name="startIndex" /> in this instance, or <see cref="F:System.String.Empty" /> if <paramref name="startIndex" /> is equal to the length of this instance.</returns>
	/// <param name="startIndex">The zero-based starting character position of a substring in this instance. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is less than zero or greater than the length of this instance. </exception>
	/// <filterpriority>1</filterpriority>
	public string Substring(int startIndex)
	{
		return Substring(startIndex, Length - startIndex);
	}

	/// <summary>Retrieves a substring from this instance. The substring starts at a specified character position and has a specified length.</summary>
	/// <returns>A string that is equivalent to the substring of length <paramref name="length" /> that begins at <paramref name="startIndex" /> in this instance, or <see cref="F:System.String.Empty" /> if <paramref name="startIndex" /> is equal to the length of this instance and <paramref name="length" /> is zero.</returns>
	/// <param name="startIndex">The zero-based starting character position of a substring in this instance. </param>
	/// <param name="length">The number of characters in the substring. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> plus <paramref name="length" /> indicates a position not within this instance.-or- <paramref name="startIndex" /> or <paramref name="length" /> is less than zero. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public string Substring(int startIndex, int length)
	{
		if (startIndex < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("StartIndex cannot be less than zero."));
		}
		if (startIndex > Length)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("startIndex cannot be larger than length of string."));
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("Length cannot be less than zero."));
		}
		if (startIndex > Length - length)
		{
			throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("Index and length must refer to a location within the string."));
		}
		if (length == 0)
		{
			return Empty;
		}
		if (startIndex == 0 && length == Length)
		{
			return this;
		}
		return InternalSubString(startIndex, length);
	}

	[SecurityCritical]
	private unsafe string InternalSubString(int startIndex, int length)
	{
		string text = FastAllocateString(length);
		fixed (char* firstChar = &text.m_firstChar)
		{
			fixed (char* firstChar2 = &m_firstChar)
			{
				wstrcpy(firstChar, firstChar2 + startIndex, length);
			}
		}
		return text;
	}

	/// <summary>Removes all leading and trailing occurrences of a set of characters specified in an array from the current <see cref="T:System.String" /> object.</summary>
	/// <returns>The string that remains after all occurrences of the characters in the <paramref name="trimChars" /> parameter are removed from the start and end of the current string. If <paramref name="trimChars" /> is null or an empty array, white-space characters are removed instead.</returns>
	/// <param name="trimChars">An array of Unicode characters to remove, or null. </param>
	/// <filterpriority>1</filterpriority>
	public string Trim(params char[] trimChars)
	{
		if (trimChars == null || trimChars.Length == 0)
		{
			return TrimHelper(2);
		}
		return TrimHelper(trimChars, 2);
	}

	/// <summary>Removes all leading occurrences of a set of characters specified in an array from the current <see cref="T:System.String" /> object.</summary>
	/// <returns>The string that remains after all occurrences of characters in the <paramref name="trimChars" /> parameter are removed from the start of the current string. If <paramref name="trimChars" /> is null or an empty array, white-space characters are removed instead.</returns>
	/// <param name="trimChars">An array of Unicode characters to remove, or null. </param>
	/// <filterpriority>2</filterpriority>
	public string TrimStart(params char[] trimChars)
	{
		if (trimChars == null || trimChars.Length == 0)
		{
			return TrimHelper(0);
		}
		return TrimHelper(trimChars, 0);
	}

	/// <summary>Removes all trailing occurrences of a set of characters specified in an array from the current <see cref="T:System.String" /> object.</summary>
	/// <returns>The string that remains after all occurrences of the characters in the <paramref name="trimChars" /> parameter are removed from the end of the current string. If <paramref name="trimChars" /> is null or an empty array, Unicode white-space characters are removed instead.</returns>
	/// <param name="trimChars">An array of Unicode characters to remove, or null. </param>
	/// <filterpriority>2</filterpriority>
	public string TrimEnd(params char[] trimChars)
	{
		if (trimChars == null || trimChars.Length == 0)
		{
			return TrimHelper(1);
		}
		return TrimHelper(trimChars, 1);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by a specified pointer to an array of Unicode characters.</summary>
	/// <param name="value">A pointer to a null-terminated array of Unicode characters. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The current process does not have read access to all the addressed characters.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> specifies an array that contains an invalid Unicode character, or <paramref name="value" /> specifies an address less than 64000.</exception>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[CLSCompliant(false)]
	[SecurityCritical]
	public unsafe extern String(char* value);

	/// <summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by a specified pointer to an array of Unicode characters, a starting character position within that array, and a length.</summary>
	/// <param name="value">A pointer to an array of Unicode characters. </param>
	/// <param name="startIndex">The starting position within <paramref name="value" />. </param>
	/// <param name="length">The number of characters within <paramref name="value" /> to use. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> or <paramref name="length" /> is less than zero, <paramref name="value" /> + <paramref name="startIndex" /> cause a pointer overflow, or the current process does not have read access to all the addressed characters.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> specifies an array that contains an invalid Unicode character, or <paramref name="value" /> + <paramref name="startIndex" /> specifies an address less than 64000.</exception>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	[CLSCompliant(false)]
	public unsafe extern String(char* value, int startIndex, int length);

	/// <summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by a pointer to an array of 8-bit signed integers.</summary>
	/// <param name="value">A pointer to a null-terminated array of 8-bit signed integers. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">A new instance of <see cref="T:System.String" /> could not be initialized using <paramref name="value" />, assuming <paramref name="value" /> is encoded in ANSI. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The length of the new string to initialize, which is determined by the null termination character of <paramref name="value" />, is too large to allocate. </exception>
	/// <exception cref="T:System.AccessViolationException">
	///   <paramref name="value" /> specifies an invalid address.</exception>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[CLSCompliant(false)]
	[SecurityCritical]
	public unsafe extern String(sbyte* value);

	/// <summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by a specified pointer to an array of 8-bit signed integers, a starting position within that array, and a length.</summary>
	/// <param name="value">A pointer to an array of 8-bit signed integers. </param>
	/// <param name="startIndex">The starting position within <paramref name="value" />. </param>
	/// <param name="length">The number of characters within <paramref name="value" /> to use. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> or <paramref name="length" /> is less than zero. -or-The address specified by <paramref name="value" /> + <paramref name="startIndex" /> is too large for the current platform; that is, the address calculation overflowed. -or-The length of the new string to initialize is too large to allocate.</exception>
	/// <exception cref="T:System.ArgumentException">The address specified by <paramref name="value" /> + <paramref name="startIndex" /> is less than 64K.-or- A new instance of <see cref="T:System.String" /> could not be initialized using <paramref name="value" />, assuming <paramref name="value" /> is encoded in ANSI. </exception>
	/// <exception cref="T:System.AccessViolationException">
	///   <paramref name="value" />, <paramref name="startIndex" />, and <paramref name="length" /> collectively specify an invalid address.</exception>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	[CLSCompliant(false)]
	public unsafe extern String(sbyte* value, int startIndex, int length);

	/// <summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by a specified pointer to an array of 8-bit signed integers, a starting position within that array, a length, and an <see cref="T:System.Text.Encoding" /> object.</summary>
	/// <param name="value">A pointer to an array of 8-bit signed integers. </param>
	/// <param name="startIndex">The starting position within <paramref name="value" />. </param>
	/// <param name="length">The number of characters within <paramref name="value" /> to use. </param>
	/// <param name="enc">An object that specifies how the array referenced by <paramref name="value" /> is encoded. If <paramref name="enc" /> is null, ANSI encoding is assumed.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> or <paramref name="length" /> is less than zero. -or-The address specified by <paramref name="value" /> + <paramref name="startIndex" /> is too large for the current platform; that is, the address calculation overflowed. -or-The length of the new string to initialize is too large to allocate.</exception>
	/// <exception cref="T:System.ArgumentException">The address specified by <paramref name="value" /> + <paramref name="startIndex" /> is less than 64K.-or- A new instance of <see cref="T:System.String" /> could not be initialized using <paramref name="value" />, assuming <paramref name="value" /> is encoded as specified by <paramref name="enc" />. </exception>
	/// <exception cref="T:System.AccessViolationException">
	///   <paramref name="value" />, <paramref name="startIndex" />, and <paramref name="length" /> collectively specify an invalid address.</exception>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[CLSCompliant(false)]
	[SecurityCritical]
	public unsafe extern String(sbyte* value, int startIndex, int length, Encoding enc);

	[SecurityCritical]
	internal unsafe static string CreateStringFromEncoding(byte* bytes, int byteLength, Encoding encoding)
	{
		int charCount = encoding.GetCharCount(bytes, byteLength, null);
		if (charCount == 0)
		{
			return Empty;
		}
		string text = FastAllocateString(charCount);
		fixed (char* firstChar = &text.m_firstChar)
		{
			encoding.GetChars(bytes, byteLength, firstChar, charCount, null);
		}
		return text;
	}

	internal unsafe int GetBytesFromEncoding(byte* pbNativeBuffer, int cbNativeBuffer, Encoding encoding)
	{
		fixed (char* firstChar = &m_firstChar)
		{
			return encoding.GetBytes(firstChar, m_stringLength, pbNativeBuffer, cbNativeBuffer);
		}
	}

	/// <summary>Indicates whether this string is in Unicode normalization form C.</summary>
	/// <returns>true if this string is in normalization form C; otherwise, false.</returns>
	/// <exception cref="T:System.ArgumentException">The current instance contains invalid Unicode characters.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public bool IsNormalized()
	{
		return IsNormalized(NormalizationForm.FormC);
	}

	/// <summary>Indicates whether this string is in the specified Unicode normalization form.</summary>
	/// <returns>true if this string is in the normalization form specified by the <paramref name="normalizationForm" /> parameter; otherwise, false.</returns>
	/// <param name="normalizationForm">A Unicode normalization form. </param>
	/// <exception cref="T:System.ArgumentException">The current instance contains invalid Unicode characters.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	public bool IsNormalized(NormalizationForm normalizationForm)
	{
		if (IsFastSort() && (normalizationForm == NormalizationForm.FormC || normalizationForm == NormalizationForm.FormKC || normalizationForm == NormalizationForm.FormD || normalizationForm == NormalizationForm.FormKD))
		{
			return true;
		}
		return Normalization.IsNormalized(this, normalizationForm);
	}

	/// <summary>Returns a new string whose textual value is the same as this string, but whose binary representation is in Unicode normalization form C.</summary>
	/// <returns>A new, normalized string whose textual value is the same as this string, but whose binary representation is in normalization form C.</returns>
	/// <exception cref="T:System.ArgumentException">The current instance contains invalid Unicode characters.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public string Normalize()
	{
		return Normalize(NormalizationForm.FormC);
	}

	/// <summary>Returns a new string whose textual value is the same as this string, but whose binary representation is in the specified Unicode normalization form.</summary>
	/// <returns>A new string whose textual value is the same as this string, but whose binary representation is in the normalization form specified by the <paramref name="normalizationForm" /> parameter.</returns>
	/// <param name="normalizationForm">A Unicode normalization form. </param>
	/// <exception cref="T:System.ArgumentException">The current instance contains invalid Unicode characters.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	public string Normalize(NormalizationForm normalizationForm)
	{
		if (IsAscii() && (normalizationForm == NormalizationForm.FormC || normalizationForm == NormalizationForm.FormKC || normalizationForm == NormalizationForm.FormD || normalizationForm == NormalizationForm.FormKD))
		{
			return this;
		}
		return Normalization.Normalize(this, normalizationForm);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	internal static extern string FastAllocateString(int length);

	[SecuritySafeCritical]
	private unsafe static void FillStringChecked(string dest, int destPos, string src)
	{
		if (src.Length > dest.Length - destPos)
		{
			throw new IndexOutOfRangeException();
		}
		fixed (char* firstChar = &dest.m_firstChar)
		{
			fixed (char* firstChar2 = &src.m_firstChar)
			{
				wstrcpy(firstChar + destPos, firstChar2, src.Length);
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by an array of Unicode characters, a starting character position within that array, and a length.</summary>
	/// <param name="value">An array of Unicode characters. </param>
	/// <param name="startIndex">The starting position within <paramref name="value" />. </param>
	/// <param name="length">The number of characters within <paramref name="value" /> to use. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> or <paramref name="length" /> is less than zero.-or- The sum of <paramref name="startIndex" /> and <paramref name="length" /> is greater than the number of elements in <paramref name="value" />. </exception>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecuritySafeCritical]
	public extern String(char[] value, int startIndex, int length);

	/// <summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by an array of Unicode characters.</summary>
	/// <param name="value">An array of Unicode characters. </param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecuritySafeCritical]
	public extern String(char[] value);

	[SecurityCritical]
	internal unsafe static void wstrcpy(char* dmem, char* smem, int charCount)
	{
		Buffer.Memcpy((byte*)dmem, (byte*)smem, charCount * 2);
	}

	[SecuritySafeCritical]
	private unsafe string CtorCharArray(char[] value)
	{
		if (value != null && value.Length != 0)
		{
			string text = FastAllocateString(value.Length);
			fixed (char* dmem = text)
			{
				fixed (char* smem = value)
				{
					wstrcpy(dmem, smem, value.Length);
				}
			}
			return text;
		}
		return Empty;
	}

	[SecuritySafeCritical]
	private unsafe string CtorCharArrayStartLength(char[] value, int startIndex, int length)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (startIndex < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("StartIndex cannot be less than zero."));
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("Length cannot be less than zero."));
		}
		if (startIndex > value.Length - length)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (length > 0)
		{
			string text = FastAllocateString(length);
			fixed (char* dmem = text)
			{
				fixed (char* ptr = value)
				{
					wstrcpy(dmem, ptr + startIndex, length);
				}
			}
			return text;
		}
		return Empty;
	}

	[SecuritySafeCritical]
	private unsafe string CtorCharCount(char c, int count)
	{
		if (count > 0)
		{
			string text = FastAllocateString(count);
			if (c != 0)
			{
				fixed (char* ptr = text)
				{
					char* ptr2 = ptr;
					while (((int)ptr2 & 3) != 0 && count > 0)
					{
						*(ptr2++) = c;
						count--;
					}
					uint num = ((uint)c << 16) | c;
					if (count >= 4)
					{
						count -= 4;
						do
						{
							*(uint*)ptr2 = num;
							*(uint*)(ptr2 + 2) = num;
							ptr2 += 4;
							count -= 4;
						}
						while (count >= 0);
					}
					if ((count & 2) != 0)
					{
						*(uint*)ptr2 = num;
						ptr2 += 2;
					}
					if ((count & 1) != 0)
					{
						*ptr2 = c;
					}
				}
			}
			return text;
		}
		if (count == 0)
		{
			return Empty;
		}
		throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("'{0}' must be non-negative.", "count"));
	}

	[SecurityCritical]
	private unsafe static int wcslen(char* ptr)
	{
		char* ptr2;
		for (ptr2 = ptr; ((int)ptr2 & 3) != 0 && *ptr2 != 0; ptr2++)
		{
		}
		if (*ptr2 != 0)
		{
			for (; (*ptr2 & ptr2[1]) != 0 || (*ptr2 != 0 && ptr2[1] != 0); ptr2 += 2)
			{
			}
		}
		for (; *ptr2 != 0; ptr2++)
		{
		}
		return (int)(ptr2 - ptr);
	}

	[SecurityCritical]
	private unsafe string CtorCharPtr(char* ptr)
	{
		if (ptr == null)
		{
			return Empty;
		}
		try
		{
			int num = wcslen(ptr);
			if (num == 0)
			{
				return Empty;
			}
			string text = FastAllocateString(num);
			fixed (char* dmem = text)
			{
				wstrcpy(dmem, ptr, num);
			}
			return text;
		}
		catch (NullReferenceException)
		{
			throw new ArgumentOutOfRangeException("ptr", Environment.GetResourceString("Pointer startIndex and length do not refer to a valid string."));
		}
	}

	[SecurityCritical]
	private unsafe string CtorCharPtrStartLength(char* ptr, int startIndex, int length)
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("Length cannot be less than zero."));
		}
		if (startIndex < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("StartIndex cannot be less than zero."));
		}
		char* ptr2 = ptr + startIndex;
		if (ptr2 < ptr)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Pointer startIndex and length do not refer to a valid string."));
		}
		if (length == 0)
		{
			return Empty;
		}
		string text = FastAllocateString(length);
		try
		{
			fixed (char* dmem = text)
			{
				wstrcpy(dmem, ptr2, length);
			}
			return text;
		}
		catch (NullReferenceException)
		{
			throw new ArgumentOutOfRangeException("ptr", Environment.GetResourceString("Pointer startIndex and length do not refer to a valid string."));
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by a specified Unicode character repeated a specified number of times.</summary>
	/// <param name="c">A Unicode character. </param>
	/// <param name="count">The number of times <paramref name="c" /> occurs. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is less than zero. </exception>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecuritySafeCritical]
	public extern String(char c, int count);

	/// <summary>Compares two specified <see cref="T:System.String" /> objects and returns an integer that indicates their relative position in the sort order.</summary>
	/// <returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.Value Condition Less than zero <paramref name="strA" /> is less than <paramref name="strB" />. Zero <paramref name="strA" /> equals <paramref name="strB" />. Greater than zero <paramref name="strA" /> is greater than <paramref name="strB" />. </returns>
	/// <param name="strA">The first string to compare. </param>
	/// <param name="strB">The second string to compare. </param>
	/// <filterpriority>1</filterpriority>
	public static int Compare(string strA, string strB)
	{
		return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.None);
	}

	/// <summary>Compares two specified <see cref="T:System.String" /> objects, ignoring or honoring their case, and returns an integer that indicates their relative position in the sort order.</summary>
	/// <returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.Value Condition Less than zero <paramref name="strA" /> is less than <paramref name="strB" />. Zero <paramref name="strA" /> equals <paramref name="strB" />. Greater than zero <paramref name="strA" /> is greater than <paramref name="strB" />. </returns>
	/// <param name="strA">The first string to compare. </param>
	/// <param name="strB">The second string to compare. </param>
	/// <param name="ignoreCase">true to ignore case during the comparison; otherwise, false.</param>
	/// <filterpriority>1</filterpriority>
	public static int Compare(string strA, string strB, bool ignoreCase)
	{
		if (ignoreCase)
		{
			return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreCase);
		}
		return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.None);
	}

	/// <summary>Compares two specified <see cref="T:System.String" /> objects using the specified rules, and returns an integer that indicates their relative position in the sort order.</summary>
	/// <returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.Value Condition Less than zero <paramref name="strA" /> is less than <paramref name="strB" />. Zero <paramref name="strA" /> equals <paramref name="strB" />. Greater than zero <paramref name="strA" /> is greater than <paramref name="strB" />. </returns>
	/// <param name="strA">The first string to compare.</param>
	/// <param name="strB">The second string to compare. </param>
	/// <param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="comparisonType" /> is not a <see cref="T:System.StringComparison" /> value. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <see cref="T:System.StringComparison" /> is not supported.</exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static int Compare(string strA, string strB, StringComparison comparisonType)
	{
		if ((uint)(comparisonType - 0) > 5u)
		{
			throw new ArgumentException(Environment.GetResourceString("The string comparison type passed in is currently not supported."), "comparisonType");
		}
		if ((object)strA == strB)
		{
			return 0;
		}
		if ((object)strA == null)
		{
			return -1;
		}
		if ((object)strB == null)
		{
			return 1;
		}
		switch (comparisonType)
		{
		case StringComparison.CurrentCulture:
			return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.None);
		case StringComparison.CurrentCultureIgnoreCase:
			return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreCase);
		case StringComparison.InvariantCulture:
			return CultureInfo.InvariantCulture.CompareInfo.Compare(strA, strB, CompareOptions.None);
		case StringComparison.InvariantCultureIgnoreCase:
			return CultureInfo.InvariantCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreCase);
		case StringComparison.Ordinal:
			if (strA.m_firstChar - strB.m_firstChar != 0)
			{
				return strA.m_firstChar - strB.m_firstChar;
			}
			return CompareOrdinalHelper(strA, strB);
		case StringComparison.OrdinalIgnoreCase:
			if (strA.IsAscii() && strB.IsAscii())
			{
				return CompareOrdinalIgnoreCaseHelper(strA, strB);
			}
			return TextInfo.CompareOrdinalIgnoreCase(strA, strB);
		default:
			throw new NotSupportedException(Environment.GetResourceString("The string comparison type passed in is currently not supported."));
		}
	}

	/// <summary>Compares two specified <see cref="T:System.String" /> objects using the specified comparison options and culture-specific information to influence the comparison, and returns an integer that indicates the relationship of the two strings to each other in the sort order.</summary>
	/// <returns>A 32-bit signed integer that indicates the lexical relationship between <paramref name="strA" /> and <paramref name="strB" />, as shown in the following tableValueConditionLess than zero<paramref name="strA" /> is less than <paramref name="strB" />.Zero<paramref name="strA" /> equals <paramref name="strB" />.Greater than zero<paramref name="strA" /> is greater than <paramref name="strB" />.</returns>
	/// <param name="strA">The first string to compare.  </param>
	/// <param name="strB">The second string to compare.</param>
	/// <param name="culture">The culture that supplies culture-specific comparison information.</param>
	/// <param name="options">Options to use when performing the comparison (such as ignoring case or symbols).  </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> is not a <see cref="T:System.Globalization.CompareOptions" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="culture" /> is null.</exception>
	public static int Compare(string strA, string strB, CultureInfo culture, CompareOptions options)
	{
		if (culture == null)
		{
			throw new ArgumentNullException("culture");
		}
		return culture.CompareInfo.Compare(strA, strB, options);
	}

	/// <summary>Compares two specified <see cref="T:System.String" /> objects, ignoring or honoring their case, and using culture-specific information to influence the comparison, and returns an integer that indicates their relative position in the sort order.</summary>
	/// <returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.Value Condition Less than zero <paramref name="strA" /> is less than <paramref name="strB" />. Zero <paramref name="strA" /> equals <paramref name="strB" />. Greater than zero <paramref name="strA" /> is greater than <paramref name="strB" />. </returns>
	/// <param name="strA">The first string to compare. </param>
	/// <param name="strB">The second string to compare. </param>
	/// <param name="ignoreCase">true to ignore case during the comparison; otherwise, false. </param>
	/// <param name="culture">An object that supplies culture-specific comparison information. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="culture" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public static int Compare(string strA, string strB, bool ignoreCase, CultureInfo culture)
	{
		if (culture == null)
		{
			throw new ArgumentNullException("culture");
		}
		if (ignoreCase)
		{
			return culture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreCase);
		}
		return culture.CompareInfo.Compare(strA, strB, CompareOptions.None);
	}

	/// <summary>Compares substrings of two specified <see cref="T:System.String" /> objects and returns an integer that indicates their relative position in the sort order.</summary>
	/// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.Value Condition Less than zero The substring in <paramref name="strA" /> is less than the substring in <paramref name="strB" />. Zero The substrings are equal, or <paramref name="length" /> is zero. Greater than zero The substring in <paramref name="strA" /> is greater than the substring in <paramref name="strB" />. </returns>
	/// <param name="strA">The first string to use in the comparison. </param>
	/// <param name="indexA">The position of the substring within <paramref name="strA" />. </param>
	/// <param name="strB">The second string to use in the comparison. </param>
	/// <param name="indexB">The position of the substring within <paramref name="strB" />. </param>
	/// <param name="length">The maximum number of characters in the substrings to compare. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="indexA" /> is greater than <paramref name="strA" />.<see cref="P:System.String.Length" />.-or- <paramref name="indexB" /> is greater than <paramref name="strB" />.<see cref="P:System.String.Length" />.-or- <paramref name="indexA" />, <paramref name="indexB" />, or <paramref name="length" /> is negative. -or-Either <paramref name="indexA" /> or <paramref name="indexB" /> is null, and <paramref name="length" /> is greater than zero.</exception>
	/// <filterpriority>1</filterpriority>
	public static int Compare(string strA, int indexA, string strB, int indexB, int length)
	{
		int num = length;
		int num2 = length;
		if ((object)strA != null && strA.Length - indexA < num)
		{
			num = strA.Length - indexA;
		}
		if ((object)strB != null && strB.Length - indexB < num2)
		{
			num2 = strB.Length - indexB;
		}
		return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.None);
	}

	/// <summary>Compares substrings of two specified <see cref="T:System.String" /> objects, ignoring or honoring their case, and returns an integer that indicates their relative position in the sort order.</summary>
	/// <returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.ValueCondition Less than zero The substring in <paramref name="strA" /> is less than the substring in <paramref name="strB" />. Zero The substrings are equal, or <paramref name="length" /> is zero. Greater than zero The substring in <paramref name="strA" /> is greater than the substring in <paramref name="strB" />. </returns>
	/// <param name="strA">The first string to use in the comparison. </param>
	/// <param name="indexA">The position of the substring within <paramref name="strA" />. </param>
	/// <param name="strB">The second string to use in the comparison. </param>
	/// <param name="indexB">The position of the substring within <paramref name="strB" />. </param>
	/// <param name="length">The maximum number of characters in the substrings to compare. </param>
	/// <param name="ignoreCase">true to ignore case during the comparison; otherwise, false.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="indexA" /> is greater than <paramref name="strA" />.<see cref="P:System.String.Length" />.-or- <paramref name="indexB" /> is greater than <paramref name="strB" />.<see cref="P:System.String.Length" />.-or- <paramref name="indexA" />, <paramref name="indexB" />, or <paramref name="length" /> is negative. -or-Either <paramref name="indexA" /> or <paramref name="indexB" /> is null, and <paramref name="length" /> is greater than zero.</exception>
	/// <filterpriority>1</filterpriority>
	public static int Compare(string strA, int indexA, string strB, int indexB, int length, bool ignoreCase)
	{
		int num = length;
		int num2 = length;
		if ((object)strA != null && strA.Length - indexA < num)
		{
			num = strA.Length - indexA;
		}
		if ((object)strB != null && strB.Length - indexB < num2)
		{
			num2 = strB.Length - indexB;
		}
		if (ignoreCase)
		{
			return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.IgnoreCase);
		}
		return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.None);
	}

	/// <summary>Compares substrings of two specified <see cref="T:System.String" /> objects, ignoring or honoring their case and using culture-specific information to influence the comparison, and returns an integer that indicates their relative position in the sort order.</summary>
	/// <returns>An integer that indicates the lexical relationship between the two comparands.Value Condition Less than zero The substring in <paramref name="strA" /> is less than the substring in <paramref name="strB" />. Zero The substrings are equal, or <paramref name="length" /> is zero. Greater than zero The substring in <paramref name="strA" /> is greater than the substring in <paramref name="strB" />. </returns>
	/// <param name="strA">The first string to use in the comparison. </param>
	/// <param name="indexA">The position of the substring within <paramref name="strA" />. </param>
	/// <param name="strB">The second string to use in the comparison. </param>
	/// <param name="indexB">The position of the substring within <paramref name="strB" />. </param>
	/// <param name="length">The maximum number of characters in the substrings to compare. </param>
	/// <param name="ignoreCase">true to ignore case during the comparison; otherwise, false. </param>
	/// <param name="culture">An object that supplies culture-specific comparison information. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="indexA" /> is greater than <paramref name="strA" />.<see cref="P:System.String.Length" />.-or- <paramref name="indexB" /> is greater than <paramref name="strB" />.<see cref="P:System.String.Length" />.-or- <paramref name="indexA" />, <paramref name="indexB" />, or <paramref name="length" /> is negative. -or-Either <paramref name="strA" /> or <paramref name="strB" /> is null, and <paramref name="length" /> is greater than zero.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="culture" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public static int Compare(string strA, int indexA, string strB, int indexB, int length, bool ignoreCase, CultureInfo culture)
	{
		if (culture == null)
		{
			throw new ArgumentNullException("culture");
		}
		int num = length;
		int num2 = length;
		if ((object)strA != null && strA.Length - indexA < num)
		{
			num = strA.Length - indexA;
		}
		if ((object)strB != null && strB.Length - indexB < num2)
		{
			num2 = strB.Length - indexB;
		}
		if (ignoreCase)
		{
			return culture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.IgnoreCase);
		}
		return culture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.None);
	}

	/// <summary>Compares substrings of two specified <see cref="T:System.String" /> objects using the specified comparison options and culture-specific information to influence the comparison, and returns an integer that indicates the relationship of the two substrings to each other in the sort order.</summary>
	/// <returns>An integer that indicates the lexical relationship between the two substrings, as shown in the following table.ValueConditionLess than zeroThe substring in <paramref name="strA" /> is less than the substring in <paramref name="strB" />.ZeroThe substrings are equal or <paramref name="length" /> is zero.Greater than zeroThe substring in <paramref name="strA" /> is greater than the substring in <paramref name="strB" />.</returns>
	/// <param name="strA">The first string to use in the comparison.   </param>
	/// <param name="indexA">The starting position of the substring within <paramref name="strA" />.</param>
	/// <param name="strB">The second string to use in the comparison.</param>
	/// <param name="indexB">The starting position of the substring within <paramref name="strB" />.</param>
	/// <param name="length">The maximum number of characters in the substrings to compare.</param>
	/// <param name="culture">An object that supplies culture-specific comparison information.</param>
	/// <param name="options">Options to use when performing the comparison (such as ignoring case or symbols).  </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> is not a <see cref="T:System.Globalization.CompareOptions" /> value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="indexA" /> is greater than <paramref name="strA" />.Length.-or-<paramref name="indexB" /> is greater than <paramref name="strB" />.Length.-or-<paramref name="indexA" />, <paramref name="indexB" />, or <paramref name="length" /> is negative.-or-Either <paramref name="strA" /> or <paramref name="strB" /> is null, and <paramref name="length" /> is greater than zero.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="culture" /> is null.</exception>
	public static int Compare(string strA, int indexA, string strB, int indexB, int length, CultureInfo culture, CompareOptions options)
	{
		if (culture == null)
		{
			throw new ArgumentNullException("culture");
		}
		int num = length;
		int num2 = length;
		if ((object)strA != null && strA.Length - indexA < num)
		{
			num = strA.Length - indexA;
		}
		if ((object)strB != null && strB.Length - indexB < num2)
		{
			num2 = strB.Length - indexB;
		}
		return culture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, options);
	}

	/// <summary>Compares substrings of two specified <see cref="T:System.String" /> objects using the specified rules, and returns an integer that indicates their relative position in the sort order. </summary>
	/// <returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.Value Condition Less than zero The substring in the <paramref name="strA" /> parameter is less than the substring in the <paramref name="strB" /> parameter.Zero The substrings are equal, or the <paramref name="length" /> parameter is zero. Greater than zero The substring in <paramref name="strA" /> is greater than the substring in <paramref name="strB" />. </returns>
	/// <param name="strA">The first string to use in the comparison. </param>
	/// <param name="indexA">The position of the substring within <paramref name="strA" />. </param>
	/// <param name="strB">The second string to use in the comparison.</param>
	/// <param name="indexB">The position of the substring within <paramref name="strB" />. </param>
	/// <param name="length">The maximum number of characters in the substrings to compare. </param>
	/// <param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="indexA" /> is greater than <paramref name="strA" />.<see cref="P:System.String.Length" />.-or- <paramref name="indexB" /> is greater than <paramref name="strB" />.<see cref="P:System.String.Length" />.-or- <paramref name="indexA" />, <paramref name="indexB" />, or <paramref name="length" /> is negative. -or-Either <paramref name="indexA" /> or <paramref name="indexB" /> is null, and <paramref name="length" /> is greater than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="comparisonType" /> is not a <see cref="T:System.StringComparison" /> value. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static int Compare(string strA, int indexA, string strB, int indexB, int length, StringComparison comparisonType)
	{
		if (comparisonType < StringComparison.CurrentCulture || comparisonType > StringComparison.OrdinalIgnoreCase)
		{
			throw new ArgumentException(Environment.GetResourceString("The string comparison type passed in is currently not supported."), "comparisonType");
		}
		if ((object)strA == null || (object)strB == null)
		{
			if ((object)strA == strB)
			{
				return 0;
			}
			if ((object)strA != null)
			{
				return 1;
			}
			return -1;
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("Length cannot be less than zero."));
		}
		if (indexA < 0)
		{
			throw new ArgumentOutOfRangeException("indexA", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (indexB < 0)
		{
			throw new ArgumentOutOfRangeException("indexB", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (strA.Length - indexA < 0)
		{
			throw new ArgumentOutOfRangeException("indexA", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (strB.Length - indexB < 0)
		{
			throw new ArgumentOutOfRangeException("indexB", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (length == 0 || (strA == strB && indexA == indexB))
		{
			return 0;
		}
		int num = length;
		int num2 = length;
		if ((object)strA != null && strA.Length - indexA < num)
		{
			num = strA.Length - indexA;
		}
		if ((object)strB != null && strB.Length - indexB < num2)
		{
			num2 = strB.Length - indexB;
		}
		return comparisonType switch
		{
			StringComparison.CurrentCulture => CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.None), 
			StringComparison.CurrentCultureIgnoreCase => CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.IgnoreCase), 
			StringComparison.InvariantCulture => CultureInfo.InvariantCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.None), 
			StringComparison.InvariantCultureIgnoreCase => CultureInfo.InvariantCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.IgnoreCase), 
			StringComparison.Ordinal => nativeCompareOrdinalEx(strA, indexA, strB, indexB, length), 
			StringComparison.OrdinalIgnoreCase => TextInfo.CompareOrdinalIgnoreCaseEx(strA, indexA, strB, indexB, num, num2), 
			_ => throw new ArgumentException(Environment.GetResourceString("The string comparison type passed in is currently not supported.")), 
		};
	}

	/// <summary>Compares this instance with a specified <see cref="T:System.Object" /> and indicates whether this instance precedes, follows, or appears in the same position in the sort order as the specified <see cref="T:System.Object" />.</summary>
	/// <returns>A 32-bit signed integer that indicates whether this instance precedes, follows, or appears in the same position in the sort order as the <paramref name="value" /> parameter.Value Condition Less than zero This instance precedes <paramref name="value" />. Zero This instance has the same position in the sort order as <paramref name="value" />. Greater than zero This instance follows <paramref name="value" />.-or- <paramref name="value" /> is null. </returns>
	/// <param name="value">An object that evaluates to a <see cref="T:System.String" />. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is not a <see cref="T:System.String" />. </exception>
	/// <filterpriority>2</filterpriority>
	public int CompareTo(object value)
	{
		if (value == null)
		{
			return 1;
		}
		if (!(value is string))
		{
			throw new ArgumentException(Environment.GetResourceString("Object must be of type String."));
		}
		return Compare(this, (string)value, StringComparison.CurrentCulture);
	}

	/// <summary>Compares this instance with a specified <see cref="T:System.String" /> object and indicates whether this instance precedes, follows, or appears in the same position in the sort order as the specified <see cref="T:System.String" />.</summary>
	/// <returns>A 32-bit signed integer that indicates whether this instance precedes, follows, or appears in the same position in the sort order as the <paramref name="value" /> parameter.Value Condition Less than zero This instance precedes <paramref name="strB" />. Zero This instance has the same position in the sort order as <paramref name="strB" />. Greater than zero This instance follows <paramref name="strB" />.-or- <paramref name="strB" /> is null. </returns>
	/// <param name="strB">The string to compare with this instance. </param>
	/// <filterpriority>2</filterpriority>
	public int CompareTo(string strB)
	{
		if ((object)strB == null)
		{
			return 1;
		}
		return CultureInfo.CurrentCulture.CompareInfo.Compare(this, strB, CompareOptions.None);
	}

	/// <summary>Compares two specified <see cref="T:System.String" /> objects by evaluating the numeric values of the corresponding <see cref="T:System.Char" /> objects in each string.</summary>
	/// <returns>An integer that indicates the lexical relationship between the two comparands.ValueCondition Less than zero <paramref name="strA" /> is less than <paramref name="strB" />. Zero <paramref name="strA" /> and <paramref name="strB" /> are equal. Greater than zero <paramref name="strA" /> is greater than <paramref name="strB" />. </returns>
	/// <param name="strA">The first string to compare. </param>
	/// <param name="strB">The second string to compare. </param>
	/// <filterpriority>2</filterpriority>
	public static int CompareOrdinal(string strA, string strB)
	{
		if ((object)strA == strB)
		{
			return 0;
		}
		if ((object)strA == null)
		{
			return -1;
		}
		if ((object)strB == null)
		{
			return 1;
		}
		if (strA.m_firstChar - strB.m_firstChar != 0)
		{
			return strA.m_firstChar - strB.m_firstChar;
		}
		return CompareOrdinalHelper(strA, strB);
	}

	/// <summary>Compares substrings of two specified <see cref="T:System.String" /> objects by evaluating the numeric values of the corresponding <see cref="T:System.Char" /> objects in each substring. </summary>
	/// <returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.ValueCondition Less than zero The substring in <paramref name="strA" /> is less than the substring in <paramref name="strB" />. Zero The substrings are equal, or <paramref name="length" /> is zero. Greater than zero The substring in <paramref name="strA" /> is greater than the substring in <paramref name="strB" />. </returns>
	/// <param name="strA">The first string to use in the comparison. </param>
	/// <param name="indexA">The starting index of the substring in <paramref name="strA" />. </param>
	/// <param name="strB">The second string to use in the comparison. </param>
	/// <param name="indexB">The starting index of the substring in <paramref name="strB" />. </param>
	/// <param name="length">The maximum number of characters in the substrings to compare. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="strA" /> is not null and <paramref name="indexA" /> is greater than <paramref name="strA" />.<see cref="P:System.String.Length" />.-or- <paramref name="strB" /> is not null and<paramref name="indexB" /> is greater than <paramref name="strB" />.<see cref="P:System.String.Length" />.-or- <paramref name="indexA" />, <paramref name="indexB" />, or <paramref name="length" /> is negative. </exception>
	/// <filterpriority>2</filterpriority>
	[SecuritySafeCritical]
	public static int CompareOrdinal(string strA, int indexA, string strB, int indexB, int length)
	{
		if ((object)strA == null || (object)strB == null)
		{
			if ((object)strA == strB)
			{
				return 0;
			}
			if ((object)strA != null)
			{
				return 1;
			}
			return -1;
		}
		return nativeCompareOrdinalEx(strA, indexA, strB, indexB, length);
	}

	/// <summary>Returns a value indicating whether the specified <see cref="T:System.String" /> object occurs within this string.</summary>
	/// <returns>true if the <paramref name="value" /> parameter occurs within this string, or if <paramref name="value" /> is the empty string (""); otherwise, false.</returns>
	/// <param name="value">The string to seek. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public bool Contains(string value)
	{
		return IndexOf(value, StringComparison.Ordinal) >= 0;
	}

	/// <summary>Determines whether the end of this string instance matches the specified string.</summary>
	/// <returns>true if <paramref name="value" /> matches the end of this instance; otherwise, false.</returns>
	/// <param name="value">The string to compare to the substring at the end of this instance. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public bool EndsWith(string value)
	{
		return EndsWith(value, StringComparison.CurrentCulture);
	}

	/// <summary>Determines whether the end of this string instance matches the specified string when compared using the specified comparison option.</summary>
	/// <returns>true if the <paramref name="value" /> parameter matches the end of this string; otherwise, false.</returns>
	/// <param name="value">The string to compare to the substring at the end of this instance. </param>
	/// <param name="comparisonType">One of the enumeration values that determines how this string and <paramref name="value" /> are compared. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="comparisonType" /> is not a <see cref="T:System.StringComparison" /> value.</exception>
	[SecuritySafeCritical]
	[ComVisible(false)]
	public bool EndsWith(string value, StringComparison comparisonType)
	{
		if ((object)value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (comparisonType < StringComparison.CurrentCulture || comparisonType > StringComparison.OrdinalIgnoreCase)
		{
			throw new ArgumentException(Environment.GetResourceString("The string comparison type passed in is currently not supported."), "comparisonType");
		}
		if ((object)this == value)
		{
			return true;
		}
		if (value.Length == 0)
		{
			return true;
		}
		switch (comparisonType)
		{
		case StringComparison.CurrentCulture:
			return CultureInfo.CurrentCulture.CompareInfo.IsSuffix(this, value, CompareOptions.None);
		case StringComparison.CurrentCultureIgnoreCase:
			return CultureInfo.CurrentCulture.CompareInfo.IsSuffix(this, value, CompareOptions.IgnoreCase);
		case StringComparison.InvariantCulture:
			return CultureInfo.InvariantCulture.CompareInfo.IsSuffix(this, value, CompareOptions.None);
		case StringComparison.InvariantCultureIgnoreCase:
			return CultureInfo.InvariantCulture.CompareInfo.IsSuffix(this, value, CompareOptions.IgnoreCase);
		case StringComparison.Ordinal:
			if (Length >= value.Length)
			{
				return nativeCompareOrdinalEx(this, Length - value.Length, value, 0, value.Length) == 0;
			}
			return false;
		case StringComparison.OrdinalIgnoreCase:
			if (Length >= value.Length)
			{
				return TextInfo.CompareOrdinalIgnoreCaseEx(this, Length - value.Length, value, 0, value.Length, value.Length) == 0;
			}
			return false;
		default:
			throw new ArgumentException(Environment.GetResourceString("The string comparison type passed in is currently not supported."), "comparisonType");
		}
	}

	/// <summary>Determines whether the end of this string instance matches the specified string when compared using the specified culture.</summary>
	/// <returns>true if the <paramref name="value" /> parameter matches the end of this string; otherwise, false.</returns>
	/// <param name="value">The string to compare to the substring at the end of this instance. </param>
	/// <param name="ignoreCase">true to ignore case during the comparison; otherwise, false.</param>
	/// <param name="culture">Cultural information that determines how this instance and <paramref name="value" /> are compared. If <paramref name="culture" /> is null, the current culture is used.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public bool EndsWith(string value, bool ignoreCase, CultureInfo culture)
	{
		if ((object)value == null)
		{
			throw new ArgumentNullException("value");
		}
		if ((object)this == value)
		{
			return true;
		}
		CultureInfo cultureInfo = ((culture != null) ? culture : CultureInfo.CurrentCulture);
		return cultureInfo.CompareInfo.IsSuffix(this, value, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
	}

	internal bool EndsWith(char value)
	{
		int length = Length;
		if (length != 0 && this[length - 1] == value)
		{
			return true;
		}
		return false;
	}

	/// <summary>Reports the zero-based index of the first occurrence of the specified Unicode character in this string.</summary>
	/// <returns>The zero-based index position of <paramref name="value" /> if that character is found, or -1 if it is not.</returns>
	/// <param name="value">A Unicode character to seek. </param>
	/// <filterpriority>1</filterpriority>
	public int IndexOf(char value)
	{
		return IndexOf(value, 0, Length);
	}

	/// <summary>Reports the zero-based index of the first occurrence of the specified Unicode character in this string. The search starts at a specified character position.</summary>
	/// <returns>The zero-based index position of <paramref name="value" /> if that character is found, or -1 if it is not.</returns>
	/// <param name="value">A Unicode character to seek. </param>
	/// <param name="startIndex">The search starting position. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is less than 0 (zero) or greater than the length of the string. </exception>
	/// <filterpriority>1</filterpriority>
	public int IndexOf(char value, int startIndex)
	{
		return IndexOf(value, startIndex, Length - startIndex);
	}

	/// <summary>Reports the zero-based index of the first occurrence in this instance of any character in a specified array of Unicode characters.</summary>
	/// <returns>The zero-based index position of the first occurrence in this instance where any character in <paramref name="anyOf" /> was found; -1 if no character in <paramref name="anyOf" /> was found.</returns>
	/// <param name="anyOf">A Unicode character array containing one or more characters to seek. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="anyOf" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	public int IndexOfAny(char[] anyOf)
	{
		return IndexOfAny(anyOf, 0, Length);
	}

	/// <summary>Reports the zero-based index of the first occurrence in this instance of any character in a specified array of Unicode characters. The search starts at a specified character position.</summary>
	/// <returns>The zero-based index position of the first occurrence in this instance where any character in <paramref name="anyOf" /> was found; -1 if no character in <paramref name="anyOf" /> was found.</returns>
	/// <param name="anyOf">A Unicode character array containing one or more characters to seek. </param>
	/// <param name="startIndex">The search starting position. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="anyOf" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is negative.-or- <paramref name="startIndex" /> is greater than the number of characters in this instance. </exception>
	/// <filterpriority>2</filterpriority>
	public int IndexOfAny(char[] anyOf, int startIndex)
	{
		return IndexOfAny(anyOf, startIndex, Length - startIndex);
	}

	/// <summary>Reports the zero-based index of the first occurrence of the specified string in this instance.</summary>
	/// <returns>The zero-based index position of <paramref name="value" /> if that string is found, or -1 if it is not. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is 0.</returns>
	/// <param name="value">The string to seek. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public int IndexOf(string value)
	{
		return IndexOf(value, StringComparison.CurrentCulture);
	}

	/// <summary>Reports the zero-based index of the first occurrence of the specified string in this instance. The search starts at a specified character position.</summary>
	/// <returns>The zero-based index position of <paramref name="value" /> if that string is found, or -1 if it is not. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is <paramref name="startIndex" />.</returns>
	/// <param name="value">The string to seek. </param>
	/// <param name="startIndex">The search starting position. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is less than 0 (zero) or greater than the length of this string.</exception>
	/// <filterpriority>1</filterpriority>
	public int IndexOf(string value, int startIndex)
	{
		return IndexOf(value, startIndex, StringComparison.CurrentCulture);
	}

	/// <summary>Reports the zero-based index of the first occurrence of the specified string in this instance. The search starts at a specified character position and examines a specified number of character positions.</summary>
	/// <returns>The zero-based index position of <paramref name="value" /> if that string is found, or -1 if it is not. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is <paramref name="startIndex" />.</returns>
	/// <param name="value">The string to seek. </param>
	/// <param name="startIndex">The search starting position. </param>
	/// <param name="count">The number of character positions to examine. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> or <paramref name="startIndex" /> is negative.-or- <paramref name="startIndex" /> is greater than the length of this string.-or-<paramref name="count" /> is greater than the length of this string minus <paramref name="startIndex" />. </exception>
	/// <filterpriority>1</filterpriority>
	public int IndexOf(string value, int startIndex, int count)
	{
		if (startIndex < 0 || startIndex > Length)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (count < 0 || count > Length - startIndex)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Count must be positive and count must refer to a location within the string/array/collection."));
		}
		return IndexOf(value, startIndex, count, StringComparison.CurrentCulture);
	}

	/// <summary>Reports the zero-based index of the first occurrence of the specified string in the current <see cref="T:System.String" /> object. A parameter specifies the type of search to use for the specified string.</summary>
	/// <returns>The index position of the <paramref name="value" /> parameter if that string is found, or -1 if it is not. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is 0.</returns>
	/// <param name="value">The string to seek. </param>
	/// <param name="comparisonType">One of the enumeration values that specifies the rules for the search. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="comparisonType" /> is not a valid <see cref="T:System.StringComparison" /> value.</exception>
	public int IndexOf(string value, StringComparison comparisonType)
	{
		return IndexOf(value, 0, Length, comparisonType);
	}

	/// <summary>Reports the zero-based index of the first occurrence of the specified string in the current <see cref="T:System.String" /> object. Parameters specify the starting search position in the current string and the type of search to use for the specified string.</summary>
	/// <returns>The zero-based index position of the <paramref name="value" /> parameter if that string is found, or -1 if it is not. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is <paramref name="startIndex" />.</returns>
	/// <param name="value">The string to seek. </param>
	/// <param name="startIndex">The search starting position. </param>
	/// <param name="comparisonType">One of the enumeration values that specifies the rules for the search. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is less than 0 (zero) or greater than the length of this string. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="comparisonType" /> is not a valid <see cref="T:System.StringComparison" /> value.</exception>
	public int IndexOf(string value, int startIndex, StringComparison comparisonType)
	{
		return IndexOf(value, startIndex, Length - startIndex, comparisonType);
	}

	/// <summary>Reports the zero-based index of the first occurrence of the specified string in the current <see cref="T:System.String" /> object. Parameters specify the starting search position in the current string, the number of characters in the current string to search, and the type of search to use for the specified string.</summary>
	/// <returns>The zero-based index position of the <paramref name="value" /> parameter if that string is found, or -1 if it is not. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is <paramref name="startIndex" />.</returns>
	/// <param name="value">The string to seek. </param>
	/// <param name="startIndex">The search starting position. </param>
	/// <param name="count">The number of character positions to examine. </param>
	/// <param name="comparisonType">One of the enumeration values that specifies the rules for the search. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> or <paramref name="startIndex" /> is negative.-or- <paramref name="startIndex" /> is greater than the length of this instance.-or-<paramref name="count" /> is greater than the length of this string minus <paramref name="startIndex" />.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="comparisonType" /> is not a valid <see cref="T:System.StringComparison" /> value.</exception>
	[SecuritySafeCritical]
	public int IndexOf(string value, int startIndex, int count, StringComparison comparisonType)
	{
		if ((object)value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (startIndex < 0 || startIndex > Length)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (count < 0 || startIndex > Length - count)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Count must be positive and count must refer to a location within the string/array/collection."));
		}
		switch (comparisonType)
		{
		case StringComparison.CurrentCulture:
			return CultureInfo.CurrentCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.None);
		case StringComparison.CurrentCultureIgnoreCase:
			return CultureInfo.CurrentCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.IgnoreCase);
		case StringComparison.InvariantCulture:
			return CultureInfo.InvariantCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.None);
		case StringComparison.InvariantCultureIgnoreCase:
			return CultureInfo.InvariantCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.IgnoreCase);
		case StringComparison.Ordinal:
			return CultureInfo.InvariantCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.Ordinal);
		case StringComparison.OrdinalIgnoreCase:
			if (value.IsAscii() && IsAscii())
			{
				return CultureInfo.InvariantCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.IgnoreCase);
			}
			return TextInfo.IndexOfStringOrdinalIgnoreCase(this, value, startIndex, count);
		default:
			throw new ArgumentException(Environment.GetResourceString("The string comparison type passed in is currently not supported."), "comparisonType");
		}
	}

	/// <summary>Reports the zero-based index position of the last occurrence of a specified Unicode character within this instance.</summary>
	/// <returns>The zero-based index position of <paramref name="value" /> if that character is found, or -1 if it is not.</returns>
	/// <param name="value">The Unicode character to seek. </param>
	/// <filterpriority>1</filterpriority>
	public int LastIndexOf(char value)
	{
		return LastIndexOf(value, Length - 1, Length);
	}

	/// <summary>Reports the zero-based index position of the last occurrence of a specified Unicode character within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string.</summary>
	/// <returns>The zero-based index position of <paramref name="value" /> if that character is found, or -1 if it is not found or if the current instance equals <see cref="F:System.String.Empty" />.</returns>
	/// <param name="value">The Unicode character to seek. </param>
	/// <param name="startIndex">The starting position of the search. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is less than zero or greater than or equal to the length of this instance.</exception>
	/// <filterpriority>1</filterpriority>
	public int LastIndexOf(char value, int startIndex)
	{
		return LastIndexOf(value, startIndex, startIndex + 1);
	}

	/// <summary>Reports the zero-based index position of the last occurrence in this instance of one or more characters specified in a Unicode array.</summary>
	/// <returns>The index position of the last occurrence in this instance where any character in <paramref name="anyOf" /> was found; -1 if no character in <paramref name="anyOf" /> was found.</returns>
	/// <param name="anyOf">A Unicode character array containing one or more characters to seek. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="anyOf" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	public int LastIndexOfAny(char[] anyOf)
	{
		return LastIndexOfAny(anyOf, Length - 1, Length);
	}

	/// <summary>Reports the zero-based index position of the last occurrence in this instance of one or more characters specified in a Unicode array. The search starts at a specified character position and proceeds backward toward the beginning of the string.</summary>
	/// <returns>The index position of the last occurrence in this instance where any character in <paramref name="anyOf" /> was found; -1 if no character in <paramref name="anyOf" /> was found or if the current instance equals <see cref="F:System.String.Empty" />.</returns>
	/// <param name="anyOf">A Unicode character array containing one or more characters to seek. </param>
	/// <param name="startIndex">The search starting position. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="anyOf" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> specifies a position that is not within this instance. </exception>
	/// <filterpriority>2</filterpriority>
	public int LastIndexOfAny(char[] anyOf, int startIndex)
	{
		return LastIndexOfAny(anyOf, startIndex, startIndex + 1);
	}

	/// <summary>Reports the zero-based index position of the last occurrence of a specified string within this instance.</summary>
	/// <returns>The zero-based index position of <paramref name="value" /> if that string is found, or -1 if it is not. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is the last index position in this instance.</returns>
	/// <param name="value">The string to seek. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public int LastIndexOf(string value)
	{
		return LastIndexOf(value, Length - 1, Length, StringComparison.CurrentCulture);
	}

	/// <summary>Reports the zero-based index position of the last occurrence of a specified string within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string.</summary>
	/// <returns>The zero-based index position of <paramref name="value" /> if that string is found, or -1 if it is not found or if the current instance equals <see cref="F:System.String.Empty" />. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is the smaller of <paramref name="startIndex" /> and the last index position in this instance.</returns>
	/// <param name="value">The string to seek. </param>
	/// <param name="startIndex">The search starting position. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is less than zero or greater than the length of the current instance. -or-The current instance equals <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is greater than zero.</exception>
	/// <filterpriority>1</filterpriority>
	public int LastIndexOf(string value, int startIndex)
	{
		return LastIndexOf(value, startIndex, startIndex + 1, StringComparison.CurrentCulture);
	}

	/// <summary>Reports the zero-based index position of the last occurrence of a specified string within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string for a specified number of character positions.</summary>
	/// <returns>The zero-based index position of <paramref name="value" /> if that string is found, or -1 if it is not found or if the current instance equals <see cref="F:System.String.Empty" />. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is the smaller of <paramref name="startIndex" /> and the last index position in this instance.</returns>
	/// <param name="value">The string to seek. </param>
	/// <param name="startIndex">The search starting position. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
	/// <param name="count">The number of character positions to examine. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is negative.-or-The current instance does not equal <see cref="F:System.String.Empty" />, and  <paramref name="startIndex" /> is negative.-or- The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is greater than the length of this instance.-or-The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> - <paramref name="count" /> + 1 specifies a position that is not within this instance. </exception>
	/// <filterpriority>1</filterpriority>
	public int LastIndexOf(string value, int startIndex, int count)
	{
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Count must be positive and count must refer to a location within the string/array/collection."));
		}
		return LastIndexOf(value, startIndex, count, StringComparison.CurrentCulture);
	}

	/// <summary>Reports the zero-based index of the last occurrence of a specified string within the current <see cref="T:System.String" /> object. A parameter specifies the type of search to use for the specified string.</summary>
	/// <returns>The index position of the <paramref name="value" /> parameter if that string is found, or -1 if it is not. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is the last index position in this instance.</returns>
	/// <param name="value">The string to seek. </param>
	/// <param name="comparisonType">One of the enumeration values that specifies the rules for the search. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="comparisonType" /> is not a valid <see cref="T:System.StringComparison" /> value.</exception>
	public int LastIndexOf(string value, StringComparison comparisonType)
	{
		return LastIndexOf(value, Length - 1, Length, comparisonType);
	}

	/// <summary>Reports the zero-based index of the last occurrence of a specified string within the current <see cref="T:System.String" /> object. The search starts at a specified character position and proceeds backward toward the beginning of the string. A parameter specifies the type of comparison to perform when searching for the specified string.</summary>
	/// <returns>The index position of the <paramref name="value" /> parameter if that string is found, or -1 if it is not found or if the current instance equals <see cref="F:System.String.Empty" />. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is the smaller of <paramref name="startIndex" /> and the last index position in this instance.</returns>
	/// <param name="value">The string to seek. </param>
	/// <param name="startIndex">The search starting position. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
	/// <param name="comparisonType">One of the enumeration values that specifies the rules for the search. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is less than zero or greater than the length of the current instance. -or-The current instance equals <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is greater than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="comparisonType" /> is not a valid <see cref="T:System.StringComparison" /> value.</exception>
	public int LastIndexOf(string value, int startIndex, StringComparison comparisonType)
	{
		return LastIndexOf(value, startIndex, startIndex + 1, comparisonType);
	}

	/// <summary>Reports the zero-based index position of the last occurrence of a specified string within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string for the specified number of character positions. A parameter specifies the type of comparison to perform when searching for the specified string.</summary>
	/// <returns>The index position of the <paramref name="value" /> parameter if that string is found, or -1 if it is not found or if the current instance equals <see cref="F:System.String.Empty" />. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is the smaller of <paramref name="startIndex" /> and the last index position in this instance.</returns>
	/// <param name="value">The string to seek. </param>
	/// <param name="startIndex">The search starting position. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
	/// <param name="count">The number of character positions to examine. </param>
	/// <param name="comparisonType">One of the enumeration values that specifies the rules for the search. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is negative.-or-The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is negative.-or- The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is greater than the length of this instance.-or-The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> + 1 - <paramref name="count" /> specifies a position that is not within this instance. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="comparisonType" /> is not a valid <see cref="T:System.StringComparison" /> value.</exception>
	[SecuritySafeCritical]
	public int LastIndexOf(string value, int startIndex, int count, StringComparison comparisonType)
	{
		if ((object)value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (Length == 0 && (startIndex == -1 || startIndex == 0))
		{
			if (value.Length != 0)
			{
				return -1;
			}
			return 0;
		}
		if (startIndex < 0 || startIndex > Length)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (startIndex == Length)
		{
			startIndex--;
			if (count > 0)
			{
				count--;
			}
			if (value.Length == 0 && count >= 0 && startIndex - count + 1 >= 0)
			{
				return startIndex;
			}
		}
		if (count < 0 || startIndex - count + 1 < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Count must be positive and count must refer to a location within the string/array/collection."));
		}
		switch (comparisonType)
		{
		case StringComparison.CurrentCulture:
			return CultureInfo.CurrentCulture.CompareInfo.LastIndexOf(this, value, startIndex, count, CompareOptions.None);
		case StringComparison.CurrentCultureIgnoreCase:
			return CultureInfo.CurrentCulture.CompareInfo.LastIndexOf(this, value, startIndex, count, CompareOptions.IgnoreCase);
		case StringComparison.InvariantCulture:
			return CultureInfo.InvariantCulture.CompareInfo.LastIndexOf(this, value, startIndex, count, CompareOptions.None);
		case StringComparison.InvariantCultureIgnoreCase:
			return CultureInfo.InvariantCulture.CompareInfo.LastIndexOf(this, value, startIndex, count, CompareOptions.IgnoreCase);
		case StringComparison.Ordinal:
			return CultureInfo.InvariantCulture.CompareInfo.LastIndexOf(this, value, startIndex, count, CompareOptions.Ordinal);
		case StringComparison.OrdinalIgnoreCase:
			if (value.IsAscii() && IsAscii())
			{
				return CultureInfo.InvariantCulture.CompareInfo.LastIndexOf(this, value, startIndex, count, CompareOptions.IgnoreCase);
			}
			return TextInfo.LastIndexOfStringOrdinalIgnoreCase(this, value, startIndex, count);
		default:
			throw new ArgumentException(Environment.GetResourceString("The string comparison type passed in is currently not supported."), "comparisonType");
		}
	}

	/// <summary>Returns a new string that right-aligns the characters in this instance by padding them with spaces on the left, for a specified total length.</summary>
	/// <returns>A new string that is equivalent to this instance, but right-aligned and padded on the left with as many spaces as needed to create a length of <paramref name="totalWidth" />. However, if <paramref name="totalWidth" /> is less than the length of this instance, the method returns a reference to the existing instance. If <paramref name="totalWidth" /> is equal to the length of this instance, the method returns a new string that is identical to this instance.</returns>
	/// <param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="totalWidth" /> is less than zero. </exception>
	/// <filterpriority>2</filterpriority>
	public string PadLeft(int totalWidth)
	{
		return PadHelper(totalWidth, ' ', isRightPadded: false);
	}

	/// <summary>Returns a new string that right-aligns the characters in this instance by padding them on the left with a specified Unicode character, for a specified total length.</summary>
	/// <returns>A new string that is equivalent to this instance, but right-aligned and padded on the left with as many <paramref name="paddingChar" /> characters as needed to create a length of <paramref name="totalWidth" />. However, if <paramref name="totalWidth" /> is less than the length of this instance, the method returns a reference to the existing instance. If <paramref name="totalWidth" /> is equal to the length of this instance, the method returns a new string that is identical to this instance.</returns>
	/// <param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters. </param>
	/// <param name="paddingChar">A Unicode padding character. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="totalWidth" /> is less than zero. </exception>
	/// <filterpriority>2</filterpriority>
	public string PadLeft(int totalWidth, char paddingChar)
	{
		return PadHelper(totalWidth, paddingChar, isRightPadded: false);
	}

	/// <summary>Returns a new string that left-aligns the characters in this string by padding them with spaces on the right, for a specified total length.</summary>
	/// <returns>A new string that is equivalent to this instance, but left-aligned and padded on the right with as many spaces as needed to create a length of <paramref name="totalWidth" />. However, if <paramref name="totalWidth" /> is less than the length of this instance, the method returns a reference to the existing instance. If <paramref name="totalWidth" /> is equal to the length of this instance, the method returns a new string that is identical to this instance.</returns>
	/// <param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="totalWidth" /> is less than zero. </exception>
	/// <filterpriority>2</filterpriority>
	public string PadRight(int totalWidth)
	{
		return PadHelper(totalWidth, ' ', isRightPadded: true);
	}

	/// <summary>Returns a new string that left-aligns the characters in this string by padding them on the right with a specified Unicode character, for a specified total length.</summary>
	/// <returns>A new string that is equivalent to this instance, but left-aligned and padded on the right with as many <paramref name="paddingChar" /> characters as needed to create a length of <paramref name="totalWidth" />.  However, if <paramref name="totalWidth" /> is less than the length of this instance, the method returns a reference to the existing instance. If <paramref name="totalWidth" /> is equal to the length of this instance, the method returns a new string that is identical to this instance.</returns>
	/// <param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters. </param>
	/// <param name="paddingChar">A Unicode padding character. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="totalWidth" /> is less than zero. </exception>
	/// <filterpriority>2</filterpriority>
	public string PadRight(int totalWidth, char paddingChar)
	{
		return PadHelper(totalWidth, paddingChar, isRightPadded: true);
	}

	/// <summary>Determines whether the beginning of this string instance matches the specified string.</summary>
	/// <returns>true if <paramref name="value" /> matches the beginning of this string; otherwise, false.</returns>
	/// <param name="value">The string to compare. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public bool StartsWith(string value)
	{
		if ((object)value == null)
		{
			throw new ArgumentNullException("value");
		}
		return StartsWith(value, StringComparison.CurrentCulture);
	}

	/// <summary>Determines whether the beginning of this string instance matches the specified string when compared using the specified comparison option.</summary>
	/// <returns>true if this instance begins with <paramref name="value" />; otherwise, false.</returns>
	/// <param name="value">The string to compare. </param>
	/// <param name="comparisonType">One of the enumeration values that determines how this string and <paramref name="value" /> are compared. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="comparisonType" /> is not a <see cref="T:System.StringComparison" /> value.</exception>
	[SecuritySafeCritical]
	[ComVisible(false)]
	public bool StartsWith(string value, StringComparison comparisonType)
	{
		if ((object)value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (comparisonType < StringComparison.CurrentCulture || comparisonType > StringComparison.OrdinalIgnoreCase)
		{
			throw new ArgumentException(Environment.GetResourceString("The string comparison type passed in is currently not supported."), "comparisonType");
		}
		if ((object)this == value)
		{
			return true;
		}
		if (value.Length == 0)
		{
			return true;
		}
		switch (comparisonType)
		{
		case StringComparison.CurrentCulture:
			return CultureInfo.CurrentCulture.CompareInfo.IsPrefix(this, value, CompareOptions.None);
		case StringComparison.CurrentCultureIgnoreCase:
			return CultureInfo.CurrentCulture.CompareInfo.IsPrefix(this, value, CompareOptions.IgnoreCase);
		case StringComparison.InvariantCulture:
			return CultureInfo.InvariantCulture.CompareInfo.IsPrefix(this, value, CompareOptions.None);
		case StringComparison.InvariantCultureIgnoreCase:
			return CultureInfo.InvariantCulture.CompareInfo.IsPrefix(this, value, CompareOptions.IgnoreCase);
		case StringComparison.Ordinal:
			if (Length < value.Length)
			{
				return false;
			}
			return nativeCompareOrdinalEx(this, 0, value, 0, value.Length) == 0;
		case StringComparison.OrdinalIgnoreCase:
			if (Length < value.Length)
			{
				return false;
			}
			return TextInfo.CompareOrdinalIgnoreCaseEx(this, 0, value, 0, value.Length, value.Length) == 0;
		default:
			throw new ArgumentException(Environment.GetResourceString("The string comparison type passed in is currently not supported."), "comparisonType");
		}
	}

	/// <summary>Determines whether the beginning of this string instance matches the specified string when compared using the specified culture.</summary>
	/// <returns>true if the <paramref name="value" /> parameter matches the beginning of this string; otherwise, false.</returns>
	/// <param name="value">The string to compare. </param>
	/// <param name="ignoreCase">true to ignore case during the comparison; otherwise, false.</param>
	/// <param name="culture">Cultural information that determines how this string and <paramref name="value" /> are compared. If <paramref name="culture" /> is null, the current culture is used.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public bool StartsWith(string value, bool ignoreCase, CultureInfo culture)
	{
		if ((object)value == null)
		{
			throw new ArgumentNullException("value");
		}
		if ((object)this == value)
		{
			return true;
		}
		CultureInfo cultureInfo = ((culture != null) ? culture : CultureInfo.CurrentCulture);
		return cultureInfo.CompareInfo.IsPrefix(this, value, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
	}

	/// <summary>Returns a copy of this string converted to lowercase.</summary>
	/// <returns>A string in lowercase.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public string ToLower()
	{
		return ToLower(CultureInfo.CurrentCulture);
	}

	/// <summary>Returns a copy of this string converted to lowercase, using the casing rules of the specified culture.</summary>
	/// <returns>The lowercase equivalent of the current string.</returns>
	/// <param name="culture">An object that supplies culture-specific casing rules. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="culture" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public string ToLower(CultureInfo culture)
	{
		if (culture == null)
		{
			throw new ArgumentNullException("culture");
		}
		return culture.TextInfo.ToLower(this);
	}

	/// <summary>Returns a copy of this <see cref="T:System.String" /> object converted to lowercase using the casing rules of the invariant culture.</summary>
	/// <returns>The lowercase equivalent of the current string.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public string ToLowerInvariant()
	{
		return ToLower(CultureInfo.InvariantCulture);
	}

	/// <summary>Returns a copy of this string converted to uppercase.</summary>
	/// <returns>The uppercase equivalent of the current string.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public string ToUpper()
	{
		return ToUpper(CultureInfo.CurrentCulture);
	}

	/// <summary>Returns a copy of this string converted to uppercase, using the casing rules of the specified culture.</summary>
	/// <returns>The uppercase equivalent of the current string.</returns>
	/// <param name="culture">An object that supplies culture-specific casing rules. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="culture" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public string ToUpper(CultureInfo culture)
	{
		if (culture == null)
		{
			throw new ArgumentNullException("culture");
		}
		return culture.TextInfo.ToUpper(this);
	}

	/// <summary>Returns a copy of this <see cref="T:System.String" /> object converted to uppercase using the casing rules of the invariant culture.</summary>
	/// <returns>The uppercase equivalent of the current string.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public string ToUpperInvariant()
	{
		return ToUpper(CultureInfo.InvariantCulture);
	}

	/// <summary>Returns this instance of <see cref="T:System.String" />; no actual conversion is performed.</summary>
	/// <returns>The current string.</returns>
	/// <filterpriority>1</filterpriority>
	public override string ToString()
	{
		return this;
	}

	/// <summary>Returns this instance of <see cref="T:System.String" />; no actual conversion is performed.</summary>
	/// <returns>The current string.</returns>
	/// <param name="provider">(Reserved) An object that supplies culture-specific formatting information. </param>
	/// <filterpriority>1</filterpriority>
	public string ToString(IFormatProvider provider)
	{
		return this;
	}

	/// <summary>Returns a reference to this instance of <see cref="T:System.String" />.</summary>
	/// <returns>This instance of <see cref="T:System.String" />.</returns>
	/// <filterpriority>2</filterpriority>
	public object Clone()
	{
		return this;
	}

	private static bool IsBOMWhitespace(char c)
	{
		return false;
	}

	/// <summary>Removes all leading and trailing white-space characters from the current <see cref="T:System.String" /> object.</summary>
	/// <returns>The string that remains after all white-space characters are removed from the start and end of the current string.</returns>
	/// <filterpriority>1</filterpriority>
	public string Trim()
	{
		return TrimHelper(2);
	}

	[SecuritySafeCritical]
	private string TrimHelper(int trimType)
	{
		int num = Length - 1;
		int i = 0;
		if (trimType != 1)
		{
			for (i = 0; i < Length && (char.IsWhiteSpace(this[i]) || IsBOMWhitespace(this[i])); i++)
			{
			}
		}
		if (trimType != 0)
		{
			num = Length - 1;
			while (num >= i && (char.IsWhiteSpace(this[num]) || IsBOMWhitespace(this[i])))
			{
				num--;
			}
		}
		return CreateTrimmedString(i, num);
	}

	[SecuritySafeCritical]
	private string TrimHelper(char[] trimChars, int trimType)
	{
		int num = Length - 1;
		int i = 0;
		if (trimType != 1)
		{
			for (i = 0; i < Length; i++)
			{
				int num2 = 0;
				char c = this[i];
				for (num2 = 0; num2 < trimChars.Length && trimChars[num2] != c; num2++)
				{
				}
				if (num2 == trimChars.Length)
				{
					break;
				}
			}
		}
		if (trimType != 0)
		{
			for (num = Length - 1; num >= i; num--)
			{
				int num3 = 0;
				char c2 = this[num];
				for (num3 = 0; num3 < trimChars.Length && trimChars[num3] != c2; num3++)
				{
				}
				if (num3 == trimChars.Length)
				{
					break;
				}
			}
		}
		return CreateTrimmedString(i, num);
	}

	[SecurityCritical]
	private string CreateTrimmedString(int start, int end)
	{
		int num = end - start + 1;
		if (num == Length)
		{
			return this;
		}
		if (num == 0)
		{
			return Empty;
		}
		return InternalSubString(start, num);
	}

	/// <summary>Returns a new string in which a specified string is inserted at a specified index position in this instance. </summary>
	/// <returns>A new string that is equivalent to this instance, but with <paramref name="value" /> inserted at position <paramref name="startIndex" />.</returns>
	/// <param name="startIndex">The zero-based index position of the insertion. </param>
	/// <param name="value">The string to insert. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is negative or greater than the length of this instance. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe string Insert(int startIndex, string value)
	{
		if ((object)value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (startIndex < 0 || startIndex > Length)
		{
			throw new ArgumentOutOfRangeException("startIndex");
		}
		int length = Length;
		int length2 = value.Length;
		int num = length + length2;
		if (num == 0)
		{
			return Empty;
		}
		string text = FastAllocateString(num);
		fixed (char* firstChar2 = &m_firstChar)
		{
			fixed (char* firstChar3 = &value.m_firstChar)
			{
				fixed (char* firstChar = &text.m_firstChar)
				{
					wstrcpy(firstChar, firstChar2, startIndex);
					wstrcpy(firstChar + startIndex, firstChar3, length2);
					wstrcpy(firstChar + startIndex + length2, firstChar2 + startIndex, length - startIndex);
				}
			}
		}
		return text;
	}

	/// <summary>Returns a new string in which all occurrences of a specified Unicode character in this instance are replaced with another specified Unicode character.</summary>
	/// <returns>A string that is equivalent to this instance except that all instances of <paramref name="oldChar" /> are replaced with <paramref name="newChar" />. If <paramref name="oldChar" /> is not found in the current instance, the method returns the current instance unchanged. </returns>
	/// <param name="oldChar">The Unicode character to be replaced. </param>
	/// <param name="newChar">The Unicode character to replace all occurrences of <paramref name="oldChar" />. </param>
	/// <filterpriority>1</filterpriority>
	public string Replace(char oldChar, char newChar)
	{
		return ReplaceInternal(oldChar, newChar);
	}

	/// <summary>Returns a new string in which all occurrences of a specified string in the current instance are replaced with another specified string.</summary>
	/// <returns>A string that is equivalent to the current string except that all instances of <paramref name="oldValue" /> are replaced with <paramref name="newValue" />. If <paramref name="oldValue" /> is not found in the current instance, the method returns the current instance unchanged. </returns>
	/// <param name="oldValue">The string to be replaced. </param>
	/// <param name="newValue">The string to replace all occurrences of <paramref name="oldValue" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="oldValue" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="oldValue" /> is the empty string (""). </exception>
	/// <filterpriority>1</filterpriority>
	public string Replace(string oldValue, string newValue)
	{
		if ((object)oldValue == null)
		{
			throw new ArgumentNullException("oldValue");
		}
		return ReplaceInternal(oldValue, newValue);
	}

	/// <summary>Returns a new string in which a specified number of characters in the current this instance beginning at a specified position have been deleted.</summary>
	/// <returns>A new string that is equivalent to this instance except for the removed characters.</returns>
	/// <param name="startIndex">The zero-based position to begin deleting characters. </param>
	/// <param name="count">The number of characters to delete. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Either <paramref name="startIndex" /> or <paramref name="count" /> is less than zero.-or- <paramref name="startIndex" /> plus <paramref name="count" /> specify a position outside this instance. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe string Remove(int startIndex, int count)
	{
		if (startIndex < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("StartIndex cannot be less than zero."));
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Count cannot be less than zero."));
		}
		if (count > Length - startIndex)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Index and count must refer to a location within the string."));
		}
		int num = Length - count;
		if (num == 0)
		{
			return Empty;
		}
		string text = FastAllocateString(num);
		fixed (char* firstChar2 = &m_firstChar)
		{
			fixed (char* firstChar = &text.m_firstChar)
			{
				wstrcpy(firstChar, firstChar2, startIndex);
				wstrcpy(firstChar + startIndex, firstChar2 + startIndex + count, num - startIndex);
			}
		}
		return text;
	}

	/// <summary>Returns a new string in which all the characters in the current instance, beginning at a specified position and continuing through the last position, have been deleted.</summary>
	/// <returns>A new string that is equivalent to this string except for the removed characters.</returns>
	/// <param name="startIndex">The zero-based position to begin deleting characters. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is less than zero.-or- <paramref name="startIndex" /> specifies a position that is not within this string. </exception>
	/// <filterpriority>1</filterpriority>
	public string Remove(int startIndex)
	{
		if (startIndex < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("StartIndex cannot be less than zero."));
		}
		if (startIndex >= Length)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("startIndex must be less than length of string."));
		}
		return Substring(0, startIndex);
	}

	/// <summary>Replaces one or more format items in a specified string with the string representation of a specified object.</summary>
	/// <returns>A copy of <paramref name="format" /> in which any format items are replaced by the string representation of <paramref name="arg0" />.</returns>
	/// <param name="format">A composite format string. </param>
	/// <param name="arg0">The object to format. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null. </exception>
	/// <exception cref="T:System.FormatException">The format item in <paramref name="format" /> is invalid.-or- The index of a format item is not zero. </exception>
	/// <filterpriority>1</filterpriority>
	public static string Format(string format, object arg0)
	{
		return FormatHelper(null, format, new ParamsArray(arg0));
	}

	/// <summary>Replaces the format items in a specified string with the string representation of two specified objects.</summary>
	/// <returns>A copy of <paramref name="format" /> in which format items are replaced by the string representations of <paramref name="arg0" /> and <paramref name="arg1" />.</returns>
	/// <param name="format">A composite format string. </param>
	/// <param name="arg0">The first object to format. </param>
	/// <param name="arg1">The second object to format. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is invalid.-or- The index of a format item is not zero or one. </exception>
	/// <filterpriority>1</filterpriority>
	public static string Format(string format, object arg0, object arg1)
	{
		return FormatHelper(null, format, new ParamsArray(arg0, arg1));
	}

	/// <summary>Replaces the format items in a specified string with the string representation of three specified objects.</summary>
	/// <returns>A copy of <paramref name="format" /> in which the format items have been replaced by the string representations of <paramref name="arg0" />, <paramref name="arg1" />, and <paramref name="arg2" />.</returns>
	/// <param name="format">A composite format string.</param>
	/// <param name="arg0">The first object to format. </param>
	/// <param name="arg1">The second object to format. </param>
	/// <param name="arg2">The third object to format. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is invalid.-or- The index of a format item is less than zero, or greater than two. </exception>
	/// <filterpriority>1</filterpriority>
	public static string Format(string format, object arg0, object arg1, object arg2)
	{
		return FormatHelper(null, format, new ParamsArray(arg0, arg1, arg2));
	}

	/// <summary>Replaces the format item in a specified string with the string representation of a corresponding object in a specified array.</summary>
	/// <returns>A copy of <paramref name="format" /> in which the format items have been replaced by the string representation of the corresponding objects in <paramref name="args" />.</returns>
	/// <param name="format">A composite format string.</param>
	/// <param name="args">An object array that contains zero or more objects to format. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> or <paramref name="args" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is invalid.-or- The index of a format item is less than zero, or greater than or equal to the length of the <paramref name="args" /> array. </exception>
	/// <filterpriority>1</filterpriority>
	public static string Format(string format, params object[] args)
	{
		if (args == null)
		{
			throw new ArgumentNullException(((object)format == null) ? "format" : "args");
		}
		return FormatHelper(null, format, new ParamsArray(args));
	}

	public static string Format(IFormatProvider provider, string format, object arg0)
	{
		return FormatHelper(provider, format, new ParamsArray(arg0));
	}

	public static string Format(IFormatProvider provider, string format, object arg0, object arg1)
	{
		return FormatHelper(provider, format, new ParamsArray(arg0, arg1));
	}

	public static string Format(IFormatProvider provider, string format, object arg0, object arg1, object arg2)
	{
		return FormatHelper(provider, format, new ParamsArray(arg0, arg1, arg2));
	}

	/// <summary>Replaces the format items in a specified string with the string representations of corresponding objects in a specified array. A parameter supplies culture-specific formatting information.</summary>
	/// <returns>A copy of <paramref name="format" /> in which the format items have been replaced by the string representation of the corresponding objects in <paramref name="args" />.</returns>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <param name="format">A composite format string. </param>
	/// <param name="args">An object array that contains zero or more objects to format. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> or <paramref name="args" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is invalid.-or- The index of a format item is less than zero, or greater than or equal to the length of the <paramref name="args" /> array. </exception>
	/// <filterpriority>1</filterpriority>
	public static string Format(IFormatProvider provider, string format, params object[] args)
	{
		if (args == null)
		{
			throw new ArgumentNullException(((object)format == null) ? "format" : "args");
		}
		return FormatHelper(provider, format, new ParamsArray(args));
	}

	private static string FormatHelper(IFormatProvider provider, string format, ParamsArray args)
	{
		if ((object)format == null)
		{
			throw new ArgumentNullException("format");
		}
		return StringBuilderCache.GetStringAndRelease(StringBuilderCache.Acquire(format.Length + args.Length * 8).AppendFormatHelper(provider, format, args));
	}

	/// <summary>Creates a new instance of <see cref="T:System.String" /> with the same value as a specified <see cref="T:System.String" />.</summary>
	/// <returns>A new string with the same value as <paramref name="str" />.</returns>
	/// <param name="str">The string to copy. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="str" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe static string Copy(string str)
	{
		if ((object)str == null)
		{
			throw new ArgumentNullException("str");
		}
		int length = str.Length;
		string text = FastAllocateString(length);
		fixed (char* firstChar = &text.m_firstChar)
		{
			fixed (char* firstChar2 = &str.m_firstChar)
			{
				wstrcpy(firstChar, firstChar2, length);
			}
		}
		return text;
	}

	/// <summary>Creates the string  representation of a specified object.</summary>
	/// <returns>The string representation of the value of <paramref name="arg0" />, or <see cref="F:System.String.Empty" /> if <paramref name="arg0" /> is null.</returns>
	/// <param name="arg0">The object to represent, or null. </param>
	/// <filterpriority>1</filterpriority>
	public static string Concat(object arg0)
	{
		if (arg0 == null)
		{
			return Empty;
		}
		return arg0.ToString();
	}

	/// <summary>Concatenates the string representations of two specified objects.</summary>
	/// <returns>The concatenated string representations of the values of <paramref name="arg0" /> and <paramref name="arg1" />.</returns>
	/// <param name="arg0">The first object to concatenate. </param>
	/// <param name="arg1">The second object to concatenate. </param>
	/// <filterpriority>1</filterpriority>
	public static string Concat(object arg0, object arg1)
	{
		if (arg0 == null)
		{
			arg0 = Empty;
		}
		if (arg1 == null)
		{
			arg1 = Empty;
		}
		return arg0.ToString() + arg1.ToString();
	}

	/// <summary>Concatenates the string representations of three specified objects.</summary>
	/// <returns>The concatenated string representations of the values of <paramref name="arg0" />, <paramref name="arg1" />, and <paramref name="arg2" />.</returns>
	/// <param name="arg0">The first object to concatenate. </param>
	/// <param name="arg1">The second object to concatenate. </param>
	/// <param name="arg2">The third object to concatenate. </param>
	/// <filterpriority>1</filterpriority>
	public static string Concat(object arg0, object arg1, object arg2)
	{
		if (arg0 == null)
		{
			arg0 = Empty;
		}
		if (arg1 == null)
		{
			arg1 = Empty;
		}
		if (arg2 == null)
		{
			arg2 = Empty;
		}
		return arg0.ToString() + arg1.ToString() + arg2.ToString();
	}

	[CLSCompliant(false)]
	public static string Concat(object arg0, object arg1, object arg2, object arg3, __arglist)
	{
		ArgIterator argIterator = new ArgIterator(__arglist);
		int num = argIterator.GetRemainingCount() + 4;
		object[] array = new object[num];
		array[0] = arg0;
		array[1] = arg1;
		array[2] = arg2;
		array[3] = arg3;
		for (int i = 4; i < num; i++)
		{
			array[i] = TypedReference.ToObject(argIterator.GetNextArg());
		}
		return Concat(array);
	}

	/// <summary>Concatenates the string representations of the elements in a specified <see cref="T:System.Object" /> array.</summary>
	/// <returns>The concatenated string representations of the values of the elements in <paramref name="args" />.</returns>
	/// <param name="args">An object array that contains the elements to concatenate. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="args" /> is null. </exception>
	/// <exception cref="T:System.OutOfMemoryException">Out of memory.</exception>
	/// <filterpriority>1</filterpriority>
	public static string Concat(params object[] args)
	{
		if (args == null)
		{
			throw new ArgumentNullException("args");
		}
		string[] array = new string[args.Length];
		int num = 0;
		for (int i = 0; i < args.Length; i++)
		{
			object obj = args[i];
			array[i] = ((obj == null) ? Empty : obj.ToString());
			if ((object)array[i] == null)
			{
				array[i] = Empty;
			}
			num += array[i].Length;
			if (num < 0)
			{
				throw new OutOfMemoryException();
			}
		}
		return ConcatArray(array, num);
	}

	/// <summary>Concatenates the members of an <see cref="T:System.Collections.Generic.IEnumerable`1" /> implementation.</summary>
	/// <returns>The concatenated members in <paramref name="values" />.</returns>
	/// <param name="values">A collection object that implements the <see cref="T:System.Collections.Generic.IEnumerable`1" /> interface.</param>
	/// <typeparam name="T">The type of the members of <paramref name="values" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="values" /> is null. </exception>
	[ComVisible(false)]
	public static string Concat<T>(IEnumerable<T> values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		StringBuilder stringBuilder = StringBuilderCache.Acquire();
		using (IEnumerator<T> enumerator = values.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current != null)
				{
					string text = enumerator.Current.ToString();
					if ((object)text != null)
					{
						stringBuilder.Append(text);
					}
				}
			}
		}
		return StringBuilderCache.GetStringAndRelease(stringBuilder);
	}

	/// <summary>Concatenates the members of a constructed <see cref="T:System.Collections.Generic.IEnumerable`1" /> collection of type <see cref="T:System.String" />.</summary>
	/// <returns>The concatenated strings in <paramref name="values" />.</returns>
	/// <param name="values">A collection object that implements <see cref="T:System.Collections.Generic.IEnumerable`1" /> and whose generic type argument is <see cref="T:System.String" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="values" /> is null. </exception>
	[ComVisible(false)]
	public static string Concat(IEnumerable<string> values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		StringBuilder stringBuilder = StringBuilderCache.Acquire();
		using (IEnumerator<string> enumerator = values.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if ((object)enumerator.Current != null)
				{
					stringBuilder.Append(enumerator.Current);
				}
			}
		}
		return StringBuilderCache.GetStringAndRelease(stringBuilder);
	}

	/// <summary>Concatenates two specified instances of <see cref="T:System.String" />.</summary>
	/// <returns>The concatenation of <paramref name="str0" /> and <paramref name="str1" />.</returns>
	/// <param name="str0">The first string to concatenate. </param>
	/// <param name="str1">The second string to concatenate. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static string Concat(string str0, string str1)
	{
		if (IsNullOrEmpty(str0))
		{
			if (IsNullOrEmpty(str1))
			{
				return Empty;
			}
			return str1;
		}
		if (IsNullOrEmpty(str1))
		{
			return str0;
		}
		int length = str0.Length;
		string text = FastAllocateString(length + str1.Length);
		FillStringChecked(text, 0, str0);
		FillStringChecked(text, length, str1);
		return text;
	}

	/// <summary>Concatenates three specified instances of <see cref="T:System.String" />.</summary>
	/// <returns>The concatenation of <paramref name="str0" />, <paramref name="str1" />, and <paramref name="str2" />.</returns>
	/// <param name="str0">The first string to concatenate. </param>
	/// <param name="str1">The second string to concatenate. </param>
	/// <param name="str2">The third string to concatenate. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static string Concat(string str0, string str1, string str2)
	{
		if ((object)str0 == null && (object)str1 == null && (object)str2 == null)
		{
			return Empty;
		}
		if ((object)str0 == null)
		{
			str0 = Empty;
		}
		if ((object)str1 == null)
		{
			str1 = Empty;
		}
		if ((object)str2 == null)
		{
			str2 = Empty;
		}
		string text = FastAllocateString(str0.Length + str1.Length + str2.Length);
		FillStringChecked(text, 0, str0);
		FillStringChecked(text, str0.Length, str1);
		FillStringChecked(text, str0.Length + str1.Length, str2);
		return text;
	}

	/// <summary>Concatenates four specified instances of <see cref="T:System.String" />.</summary>
	/// <returns>The concatenation of <paramref name="str0" />, <paramref name="str1" />, <paramref name="str2" />, and <paramref name="str3" />.</returns>
	/// <param name="str0">The first string to concatenate. </param>
	/// <param name="str1">The second string to concatenate. </param>
	/// <param name="str2">The third string to concatenate. </param>
	/// <param name="str3">The fourth string to concatenate. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static string Concat(string str0, string str1, string str2, string str3)
	{
		if ((object)str0 == null && (object)str1 == null && (object)str2 == null && (object)str3 == null)
		{
			return Empty;
		}
		if ((object)str0 == null)
		{
			str0 = Empty;
		}
		if ((object)str1 == null)
		{
			str1 = Empty;
		}
		if ((object)str2 == null)
		{
			str2 = Empty;
		}
		if ((object)str3 == null)
		{
			str3 = Empty;
		}
		string text = FastAllocateString(str0.Length + str1.Length + str2.Length + str3.Length);
		FillStringChecked(text, 0, str0);
		FillStringChecked(text, str0.Length, str1);
		FillStringChecked(text, str0.Length + str1.Length, str2);
		FillStringChecked(text, str0.Length + str1.Length + str2.Length, str3);
		return text;
	}

	[SecuritySafeCritical]
	private static string ConcatArray(string[] values, int totalLength)
	{
		string text = FastAllocateString(totalLength);
		int num = 0;
		for (int i = 0; i < values.Length; i++)
		{
			FillStringChecked(text, num, values[i]);
			num += values[i].Length;
		}
		return text;
	}

	/// <summary>Concatenates the elements of a specified <see cref="T:System.String" /> array.</summary>
	/// <returns>The concatenated elements of <paramref name="values" />.</returns>
	/// <param name="values">An array of string instances. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="values" /> is null. </exception>
	/// <exception cref="T:System.OutOfMemoryException">Out of memory.</exception>
	/// <filterpriority>1</filterpriority>
	public static string Concat(params string[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		int num = 0;
		string[] array = new string[values.Length];
		for (int i = 0; i < values.Length; i++)
		{
			string text = values[i];
			array[i] = (((object)text == null) ? Empty : text);
			num += array[i].Length;
			if (num < 0)
			{
				throw new OutOfMemoryException();
			}
		}
		return ConcatArray(array, num);
	}

	/// <summary>Retrieves the system's reference to the specified <see cref="T:System.String" />.</summary>
	/// <returns>The system's reference to <paramref name="str" />, if it is interned; otherwise, a new reference to a string with the value of <paramref name="str" />.</returns>
	/// <param name="str">A string to search for in the intern pool. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="str" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	[SecuritySafeCritical]
	public static string Intern(string str)
	{
		if ((object)str == null)
		{
			throw new ArgumentNullException("str");
		}
		return InternalIntern(str);
	}

	/// <summary>Retrieves a reference to a specified <see cref="T:System.String" />.</summary>
	/// <returns>A reference to <paramref name="str" /> if it is in the common language runtime intern pool; otherwise, null.</returns>
	/// <param name="str">The string to search for in the intern pool. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="str" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	[SecuritySafeCritical]
	public static string IsInterned(string str)
	{
		if ((object)str == null)
		{
			throw new ArgumentNullException("str");
		}
		return InternalIsInterned(str);
	}

	/// <summary>Returns the <see cref="T:System.TypeCode" /> for class <see cref="T:System.String" />.</summary>
	/// <returns>The enumerated constant, <see cref="F:System.TypeCode.String" />.</returns>
	/// <filterpriority>2</filterpriority>
	public TypeCode GetTypeCode()
	{
		return TypeCode.String;
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToBoolean(System.IFormatProvider)" />. </summary>
	/// <returns>true if the value of the current string is <see cref="F:System.Boolean.TrueString" />; false if the value of the current string is <see cref="F:System.Boolean.FalseString" />.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	/// <exception cref="T:System.FormatException">The value of the current string is not <see cref="F:System.Boolean.TrueString" /> or <see cref="F:System.Boolean.FalseString" />.</exception>
	bool IConvertible.ToBoolean(IFormatProvider provider)
	{
		return Convert.ToBoolean(this, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToChar(System.IFormatProvider)" />.</summary>
	/// <returns>The character at index 0 in the current <see cref="T:System.String" /> object.</returns>
	/// <param name="provider">An object that provides culture-specific formatting information. </param>
	char IConvertible.ToChar(IFormatProvider provider)
	{
		return Convert.ToChar(this, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToSByte(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.String" /> object.</returns>
	/// <param name="provider">An object that provides culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">The value of the current <see cref="T:System.String" /> object cannot be parsed. </exception>
	/// <exception cref="T:System.OverflowException">The value of the current <see cref="T:System.String" /> object is a number greater than <see cref="F:System.SByte.MaxValue" /> or less than <see cref="F:System.SByte.MinValue" />. </exception>
	sbyte IConvertible.ToSByte(IFormatProvider provider)
	{
		return Convert.ToSByte(this, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToByte(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.String" /> object.</returns>
	/// <param name="provider">An object that provides culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">The value of the current <see cref="T:System.String" /> object cannot be parsed. </exception>
	/// <exception cref="T:System.OverflowException">The value of the current <see cref="T:System.String" /> object is a number greater than <see cref="F:System.Byte.MaxValue" /> or less than <see cref="F:System.Byte.MinValue" />. </exception>
	byte IConvertible.ToByte(IFormatProvider provider)
	{
		return Convert.ToByte(this, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToInt16(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.String" /> object.</returns>
	/// <param name="provider">An object that provides culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">The value of the current <see cref="T:System.String" /> object cannot be parsed. </exception>
	/// <exception cref="T:System.OverflowException">The value of the current <see cref="T:System.String" /> object is a number greater than <see cref="F:System.Int16.MaxValue" /> or less than <see cref="F:System.Int16.MinValue" />.</exception>
	short IConvertible.ToInt16(IFormatProvider provider)
	{
		return Convert.ToInt16(this, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToUInt16(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.String" /> object.</returns>
	/// <param name="provider">An object that provides culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">The value of the current <see cref="T:System.String" /> object cannot be parsed. </exception>
	/// <exception cref="T:System.OverflowException">The value of the current <see cref="T:System.String" /> object is a number greater than <see cref="F:System.UInt16.MaxValue" /> or less than <see cref="F:System.UInt16.MinValue" />.</exception>
	ushort IConvertible.ToUInt16(IFormatProvider provider)
	{
		return Convert.ToUInt16(this, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToInt32(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.String" /> object.</returns>
	/// <param name="provider">An object that provides culture-specific formatting information. </param>
	int IConvertible.ToInt32(IFormatProvider provider)
	{
		return Convert.ToInt32(this, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToUInt32(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.String" /> object.</returns>
	/// <param name="provider">An object that provides culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">The value of the current <see cref="T:System.String" /> object cannot be parsed. </exception>
	/// <exception cref="T:System.OverflowException">The value of the current <see cref="T:System.String" /> object is a number greater <see cref="F:System.UInt32.MaxValue" /> or less than <see cref="F:System.UInt32.MinValue" /></exception>
	uint IConvertible.ToUInt32(IFormatProvider provider)
	{
		return Convert.ToUInt32(this, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToInt64(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.String" /> object.</returns>
	/// <param name="provider">An object that provides culture-specific formatting information. </param>
	long IConvertible.ToInt64(IFormatProvider provider)
	{
		return Convert.ToInt64(this, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToUInt64(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.String" /> object.</returns>
	/// <param name="provider">An object that provides culture-specific formatting information. </param>
	ulong IConvertible.ToUInt64(IFormatProvider provider)
	{
		return Convert.ToUInt64(this, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToSingle(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.String" /> object.</returns>
	/// <param name="provider">An object that provides culture-specific formatting information. </param>
	float IConvertible.ToSingle(IFormatProvider provider)
	{
		return Convert.ToSingle(this, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToDouble(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.String" /> object.</returns>
	/// <param name="provider">An object that provides culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">The value of the current <see cref="T:System.String" /> object cannot be parsed. </exception>
	/// <exception cref="T:System.OverflowException">The value of the current <see cref="T:System.String" /> object is a number less than <see cref="F:System.Double.MinValue" /> or greater than <see cref="F:System.Double.MaxValue" />. </exception>
	double IConvertible.ToDouble(IFormatProvider provider)
	{
		return Convert.ToDouble(this, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToDecimal(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.String" /> object.</returns>
	/// <param name="provider">An object that provides culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">The value of the current <see cref="T:System.String" /> object cannot be parsed. </exception>
	/// <exception cref="T:System.OverflowException">The value of the current <see cref="T:System.String" /> object is a number less than <see cref="F:System.Decimal.MinValue" /> or than <see cref="F:System.Decimal.MaxValue" /> greater. </exception>
	decimal IConvertible.ToDecimal(IFormatProvider provider)
	{
		return Convert.ToDecimal(this, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToDateTime(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.String" /> object.</returns>
	/// <param name="provider">An object that provides culture-specific formatting information. </param>
	DateTime IConvertible.ToDateTime(IFormatProvider provider)
	{
		return Convert.ToDateTime(this, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToType(System.Type,System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.String" /> object.</returns>
	/// <param name="type">The type of the returned object. </param>
	/// <param name="provider">An object that provides culture-specific formatting information.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null.</exception>
	/// <exception cref="T:System.InvalidCastException">The value of the current <see cref="T:System.String" /> object cannot be converted to the type specified by the <paramref name="type" /> parameter. </exception>
	object IConvertible.ToType(Type type, IFormatProvider provider)
	{
		return Convert.DefaultToType(this, type, provider);
	}

	/// <summary>Retrieves an object that can iterate through the individual characters in this string.</summary>
	/// <returns>An enumerator object.</returns>
	/// <filterpriority>2</filterpriority>
	public CharEnumerator GetEnumerator()
	{
		return new CharEnumerator(this);
	}

	IEnumerator<char> IEnumerable<char>.GetEnumerator()
	{
		return new CharEnumerator(this);
	}

	/// <summary>Returns an enumerator that iterates through the current <see cref="T:System.String" /> object. </summary>
	/// <returns>An enumerator that can be used to iterate through the current string.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new CharEnumerator(this);
	}

	[SecurityCritical]
	internal unsafe static void InternalCopy(string src, IntPtr dest, int len)
	{
		if (len != 0)
		{
			fixed (char* firstChar = &src.m_firstChar)
			{
				byte* src2 = (byte*)firstChar;
				byte* dest2 = (byte*)(void*)dest;
				Buffer.Memcpy(dest2, src2, len);
			}
		}
	}

	internal unsafe static int CompareOrdinalUnchecked(string strA, int indexA, int lenA, string strB, int indexB, int lenB)
	{
		if ((object)strA == null)
		{
			if ((object)strB != null)
			{
				return -1;
			}
			return 0;
		}
		if ((object)strB == null)
		{
			return 1;
		}
		int num = Math.Min(lenA, strA.m_stringLength - indexA);
		int num2 = Math.Min(lenB, strB.m_stringLength - indexB);
		if (num == num2 && indexA == indexB && (object)strA == strB)
		{
			return 0;
		}
		fixed (char* ptr = strA)
		{
			fixed (char* ptr4 = strB)
			{
				char* ptr2 = ptr + indexA;
				char* ptr3 = ptr2 + Math.Min(num, num2);
				char* ptr5 = ptr4 + indexB;
				while (ptr2 < ptr3)
				{
					if (*ptr2 != *ptr5)
					{
						return *ptr2 - *ptr5;
					}
					ptr2++;
					ptr5++;
				}
				return num - num2;
			}
		}
	}

	/// <summary>Reports the zero-based index of the first occurrence of the specified character in this instance. The search starts at a specified character position and examines a specified number of character positions.</summary>
	/// <returns>The zero-based index position of <paramref name="value" /> if that character is found, or -1 if it is not.</returns>
	/// <param name="value">A Unicode character to seek. </param>
	/// <param name="startIndex">The search starting position. </param>
	/// <param name="count">The number of character positions to examine. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> or <paramref name="startIndex" /> is negative.-or- <paramref name="startIndex" /> is greater than the length of this string.-or-<paramref name="count" /> is greater than the length of this string minus <paramref name="startIndex" />. </exception>
	/// <filterpriority>1</filterpriority>
	public int IndexOf(char value, int startIndex, int count)
	{
		if (startIndex < 0 || startIndex > m_stringLength)
		{
			throw new ArgumentOutOfRangeException("startIndex", "Cannot be negative and must be< 0");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "< 0");
		}
		if (startIndex > m_stringLength - count)
		{
			throw new ArgumentOutOfRangeException("count", "startIndex + count > this.m_stringLength");
		}
		if ((startIndex == 0 && m_stringLength == 0) || startIndex == m_stringLength || count == 0)
		{
			return -1;
		}
		return IndexOfUnchecked(value, startIndex, count);
	}

	internal unsafe int IndexOfUnchecked(char value, int startIndex, int count)
	{
		fixed (char* firstChar = &m_firstChar)
		{
			char* ptr = firstChar + startIndex;
			char* ptr2;
			for (ptr2 = ptr + (count >> 3 << 3); ptr != ptr2; ptr += 8)
			{
				if (*ptr == value)
				{
					return (int)(ptr - firstChar);
				}
				if (ptr[1] == value)
				{
					return (int)(ptr - firstChar + 1);
				}
				if (ptr[2] == value)
				{
					return (int)(ptr - firstChar + 2);
				}
				if (ptr[3] == value)
				{
					return (int)(ptr - firstChar + 3);
				}
				if (ptr[4] == value)
				{
					return (int)(ptr - firstChar + 4);
				}
				if (ptr[5] == value)
				{
					return (int)(ptr - firstChar + 5);
				}
				if (ptr[6] == value)
				{
					return (int)(ptr - firstChar + 6);
				}
				if (ptr[7] == value)
				{
					return (int)(ptr - firstChar + 7);
				}
			}
			for (ptr2 += count & 7; ptr != ptr2; ptr++)
			{
				if (*ptr == value)
				{
					return (int)(ptr - firstChar);
				}
			}
			return -1;
		}
	}

	internal unsafe int IndexOfUnchecked(string value, int startIndex, int count)
	{
		int length = value.Length;
		if (count < length)
		{
			return -1;
		}
		if (length <= 1)
		{
			if (length == 1)
			{
				return IndexOfUnchecked(value[0], startIndex, count);
			}
			return startIndex;
		}
		fixed (char* firstChar = &m_firstChar)
		{
			fixed (char* ptr3 = value)
			{
				char* ptr = firstChar + startIndex;
				for (char* ptr2 = ptr + count - length + 1; ptr != ptr2; ptr++)
				{
					if (*ptr != *ptr3)
					{
						continue;
					}
					int num = 1;
					while (true)
					{
						if (num < length)
						{
							if (ptr[num] != ptr3[num])
							{
								break;
							}
							num++;
							continue;
						}
						return (int)(ptr - firstChar);
					}
				}
			}
		}
		return -1;
	}

	/// <summary>Reports the zero-based index of the first occurrence in this instance of any character in a specified array of Unicode characters. The search starts at a specified character position and examines a specified number of character positions.</summary>
	/// <returns>The zero-based index position of the first occurrence in this instance where any character in <paramref name="anyOf" /> was found; -1 if no character in <paramref name="anyOf" /> was found.</returns>
	/// <param name="anyOf">A Unicode character array containing one or more characters to seek. </param>
	/// <param name="startIndex">The search starting position. </param>
	/// <param name="count">The number of character positions to examine. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="anyOf" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> or <paramref name="startIndex" /> is negative.-or- <paramref name="count" /> + <paramref name="startIndex" /> is greater than the number of characters in this instance. </exception>
	/// <filterpriority>2</filterpriority>
	public int IndexOfAny(char[] anyOf, int startIndex, int count)
	{
		if (anyOf == null)
		{
			throw new ArgumentNullException();
		}
		if (startIndex < 0 || startIndex > m_stringLength)
		{
			throw new ArgumentOutOfRangeException();
		}
		if (count < 0 || startIndex > m_stringLength - count)
		{
			throw new ArgumentOutOfRangeException("count", "Count cannot be negative, and startIndex + count must be less than m_stringLength of the string.");
		}
		return IndexOfAnyUnchecked(anyOf, startIndex, count);
	}

	private unsafe int IndexOfAnyUnchecked(char[] anyOf, int startIndex, int count)
	{
		if (anyOf.Length == 0)
		{
			return -1;
		}
		if (anyOf.Length == 1)
		{
			return IndexOfUnchecked(anyOf[0], startIndex, count);
		}
		fixed (char* ptr = anyOf)
		{
			int num = *ptr;
			int num2 = *ptr;
			char* ptr2 = ptr + anyOf.Length;
			char* ptr3 = ptr;
			while (++ptr3 != ptr2)
			{
				if (*ptr3 > num)
				{
					num = *ptr3;
				}
				else if (*ptr3 < num2)
				{
					num2 = *ptr3;
				}
			}
			fixed (char* firstChar = &m_firstChar)
			{
				char* ptr4 = firstChar + startIndex;
				char* ptr5 = ptr4 + count;
				while (ptr4 != ptr5)
				{
					if (*ptr4 > num || *ptr4 < num2)
					{
						ptr4++;
						continue;
					}
					if (*ptr4 == *ptr)
					{
						return (int)(ptr4 - firstChar);
					}
					ptr3 = ptr;
					while (++ptr3 != ptr2)
					{
						if (*ptr4 == *ptr3)
						{
							return (int)(ptr4 - firstChar);
						}
					}
					ptr4++;
				}
			}
		}
		return -1;
	}

	/// <summary>Reports the zero-based index position of the last occurrence of the specified Unicode character in a substring within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string for a specified number of character positions.</summary>
	/// <returns>The zero-based index position of <paramref name="value" /> if that character is found, or -1 if it is not found or if the current instance equals <see cref="F:System.String.Empty" />.</returns>
	/// <param name="value">The Unicode character to seek. </param>
	/// <param name="startIndex">The starting position of the search. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
	/// <param name="count">The number of character positions to examine. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is less than zero or greater than or equal to the length of this instance.-or-The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> - <paramref name="count" /> + 1 is less than zero.</exception>
	/// <filterpriority>1</filterpriority>
	public int LastIndexOf(char value, int startIndex, int count)
	{
		if (m_stringLength == 0)
		{
			return -1;
		}
		if (startIndex < 0 || startIndex >= Length)
		{
			throw new ArgumentOutOfRangeException("startIndex", "< 0 || >= this.Length");
		}
		if (count < 0 || count > Length)
		{
			throw new ArgumentOutOfRangeException("count", "< 0 || > this.Length");
		}
		if (startIndex - count + 1 < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex - count + 1 < 0");
		}
		return LastIndexOfUnchecked(value, startIndex, count);
	}

	internal unsafe int LastIndexOfUnchecked(char value, int startIndex, int count)
	{
		fixed (char* firstChar = &m_firstChar)
		{
			char* ptr = firstChar + startIndex;
			char* ptr2 = ptr - (count >> 3 << 3);
			while (ptr != ptr2)
			{
				if (*ptr == value)
				{
					return (int)(ptr - firstChar);
				}
				if (ptr[-1] == value)
				{
					return (int)(ptr - firstChar) - 1;
				}
				if (ptr[-2] == value)
				{
					return (int)(ptr - firstChar) - 2;
				}
				if (ptr[-3] == value)
				{
					return (int)(ptr - firstChar) - 3;
				}
				if (ptr[-4] == value)
				{
					return (int)(ptr - firstChar) - 4;
				}
				if (ptr[-5] == value)
				{
					return (int)(ptr - firstChar) - 5;
				}
				if (ptr[-6] == value)
				{
					return (int)(ptr - firstChar) - 6;
				}
				if (ptr[-7] == value)
				{
					return (int)(ptr - firstChar) - 7;
				}
				ptr -= 8;
			}
			ptr2 -= count & 7;
			while (ptr != ptr2)
			{
				if (*ptr == value)
				{
					return (int)(ptr - firstChar);
				}
				ptr--;
			}
			return -1;
		}
	}

	/// <summary>Reports the zero-based index position of the last occurrence in this instance of one or more characters specified in a Unicode array. The search starts at a specified character position and proceeds backward toward the beginning of the string for a specified number of character positions.</summary>
	/// <returns>The index position of the last occurrence in this instance where any character in <paramref name="anyOf" /> was found; -1 if no character in <paramref name="anyOf" /> was found or if the current instance equals <see cref="F:System.String.Empty" />.</returns>
	/// <param name="anyOf">A Unicode character array containing one or more characters to seek. </param>
	/// <param name="startIndex">The search starting position. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
	/// <param name="count">The number of character positions to examine. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="anyOf" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="count" /> or <paramref name="startIndex" /> is negative.-or- The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> minus <paramref name="count" /> + 1 is less than zero. </exception>
	/// <filterpriority>2</filterpriority>
	public int LastIndexOfAny(char[] anyOf, int startIndex, int count)
	{
		if (anyOf == null)
		{
			throw new ArgumentNullException();
		}
		if (m_stringLength == 0)
		{
			return -1;
		}
		if (startIndex < 0 || startIndex >= Length)
		{
			throw new ArgumentOutOfRangeException("startIndex", "< 0 || > this.Length");
		}
		if (count < 0 || count > Length)
		{
			throw new ArgumentOutOfRangeException("count", "< 0 || > this.Length");
		}
		if (startIndex - count + 1 < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex - count + 1 < 0");
		}
		if (m_stringLength == 0)
		{
			return -1;
		}
		return LastIndexOfAnyUnchecked(anyOf, startIndex, count);
	}

	private unsafe int LastIndexOfAnyUnchecked(char[] anyOf, int startIndex, int count)
	{
		if (anyOf.Length == 1)
		{
			return LastIndexOfUnchecked(anyOf[0], startIndex, count);
		}
		fixed (char* firstChar = &m_firstChar)
		{
			fixed (char* ptr3 = anyOf)
			{
				char* ptr = firstChar + startIndex;
				char* ptr2 = ptr - count;
				char* ptr4 = ptr3 + anyOf.Length;
				while (ptr != ptr2)
				{
					for (char* ptr5 = ptr3; ptr5 != ptr4; ptr5++)
					{
						if (*ptr5 == *ptr)
						{
							return (int)(ptr - firstChar);
						}
					}
					ptr--;
				}
				return -1;
			}
		}
	}

	internal static int nativeCompareOrdinalEx(string strA, int indexA, string strB, int indexB, int count)
	{
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Count cannot be less than zero."));
		}
		if (indexA < 0 || indexA > strA.Length)
		{
			throw new ArgumentOutOfRangeException("indexA", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (indexB < 0 || indexB > strB.Length)
		{
			throw new ArgumentOutOfRangeException("indexB", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		return CompareOrdinalUnchecked(strA, indexA, count, strB, indexB, count);
	}

	private unsafe string ReplaceInternal(char oldChar, char newChar)
	{
		if (m_stringLength == 0 || oldChar == newChar)
		{
			return this;
		}
		int num = IndexOfUnchecked(oldChar, 0, m_stringLength);
		if (num == -1)
		{
			return this;
		}
		if (num < 4)
		{
			num = 0;
		}
		string text = FastAllocateString(m_stringLength);
		fixed (char* ptr = text)
		{
			fixed (char* firstChar = &m_firstChar)
			{
				if (num != 0)
				{
					CharCopy(ptr, firstChar, num);
				}
				char* ptr2 = ptr + m_stringLength;
				char* ptr3 = ptr + num;
				char* ptr4 = firstChar + num;
				for (; ptr3 != ptr2; ptr3++)
				{
					if (*ptr4 == oldChar)
					{
						*ptr3 = newChar;
					}
					else
					{
						*ptr3 = *ptr4;
					}
					ptr4++;
				}
			}
		}
		return text;
	}

	internal string ReplaceInternal(string oldValue, string newValue)
	{
		if ((object)oldValue == null)
		{
			throw new ArgumentNullException("oldValue");
		}
		if (oldValue.Length == 0)
		{
			throw new ArgumentException("oldValue is the empty string.");
		}
		if (Length == 0)
		{
			return this;
		}
		if ((object)newValue == null)
		{
			newValue = Empty;
		}
		return ReplaceUnchecked(oldValue, newValue);
	}

	private unsafe string ReplaceUnchecked(string oldValue, string newValue)
	{
		if (oldValue.m_stringLength > m_stringLength)
		{
			return this;
		}
		if (oldValue.m_stringLength == 1 && newValue.m_stringLength == 1)
		{
			return Replace(oldValue[0], newValue[0]);
		}
		int* ptr = stackalloc int[200];
		fixed (char* firstChar = &m_firstChar)
		{
			fixed (char* src = newValue)
			{
				int num = 0;
				int num2 = 0;
				while (num < m_stringLength)
				{
					int num3 = IndexOfUnchecked(oldValue, num, m_stringLength - num);
					if (num3 < 0)
					{
						break;
					}
					if (num2 < 200)
					{
						ptr[num2++] = num3;
						num = num3 + oldValue.m_stringLength;
						continue;
					}
					return ReplaceFallback(oldValue, newValue, 200);
				}
				if (num2 == 0)
				{
					return this;
				}
				int num4 = 0;
				try
				{
					num4 = checked(m_stringLength + (newValue.m_stringLength - oldValue.m_stringLength) * num2);
				}
				catch (OverflowException)
				{
					throw new OutOfMemoryException();
				}
				string text = FastAllocateString(num4);
				int num5 = 0;
				int num6 = 0;
				fixed (char* ptr2 = text)
				{
					for (int i = 0; i < num2; i++)
					{
						int num7 = ptr[i] - num6;
						CharCopy(ptr2 + num5, firstChar + num6, num7);
						num5 += num7;
						num6 = ptr[i] + oldValue.m_stringLength;
						CharCopy(ptr2 + num5, src, newValue.m_stringLength);
						num5 += newValue.m_stringLength;
					}
					CharCopy(ptr2 + num5, firstChar + num6, m_stringLength - num6);
				}
				return text;
			}
		}
	}

	private string ReplaceFallback(string oldValue, string newValue, int testedCount)
	{
		StringBuilder stringBuilder = new StringBuilder(m_stringLength + (newValue.m_stringLength - oldValue.m_stringLength) * testedCount);
		int num = 0;
		while (num < m_stringLength)
		{
			int num2 = IndexOfUnchecked(oldValue, num, m_stringLength - num);
			if (num2 < 0)
			{
				stringBuilder.Append(InternalSubString(num, m_stringLength - num));
				break;
			}
			stringBuilder.Append(InternalSubString(num, num2 - num));
			stringBuilder.Append(newValue);
			num = num2 + oldValue.m_stringLength;
		}
		return stringBuilder.ToString();
	}

	private unsafe string PadHelper(int totalWidth, char paddingChar, bool isRightPadded)
	{
		if (totalWidth < 0)
		{
			throw new ArgumentOutOfRangeException("totalWidth", "Non-negative number required");
		}
		if (totalWidth <= m_stringLength)
		{
			return this;
		}
		string text = FastAllocateString(totalWidth);
		fixed (char* ptr = text)
		{
			fixed (char* firstChar = &m_firstChar)
			{
				if (isRightPadded)
				{
					CharCopy(ptr, firstChar, m_stringLength);
					char* ptr2 = ptr + totalWidth;
					char* ptr3 = ptr + m_stringLength;
					while (ptr3 < ptr2)
					{
						*(ptr3++) = paddingChar;
					}
				}
				else
				{
					char* ptr4 = ptr;
					char* ptr5 = ptr4 + totalWidth - m_stringLength;
					while (ptr4 < ptr5)
					{
						*(ptr4++) = paddingChar;
					}
					CharCopy(ptr4, firstChar, m_stringLength);
				}
			}
		}
		return text;
	}

	internal bool StartsWithOrdinalUnchecked(string value)
	{
		if (m_stringLength >= value.m_stringLength)
		{
			return CompareOrdinalUnchecked(this, 0, value.m_stringLength, value, 0, value.m_stringLength) == 0;
		}
		return false;
	}

	internal unsafe bool IsAscii()
	{
		fixed (char* firstChar = &m_firstChar)
		{
			char* ptr = firstChar + m_stringLength;
			for (char* ptr2 = firstChar; ptr2 != ptr; ptr2++)
			{
				if (*ptr2 >= '\u0080')
				{
					return false;
				}
			}
		}
		return true;
	}

	internal bool IsFastSort()
	{
		return false;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern string InternalIsInterned(string str);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern string InternalIntern(string str);

	internal unsafe static void CharCopy(char* dest, char* src, int count)
	{
		if ((((int)dest | (int)src) & 3) != 0)
		{
			if (((int)dest & 2) != 0 && ((int)src & 2) != 0 && count > 0)
			{
				*dest = *src;
				dest++;
				src++;
				count--;
			}
			if ((((int)dest | (int)src) & 2) != 0)
			{
				Buffer.memcpy2((byte*)dest, (byte*)src, count * 2);
				return;
			}
		}
		Buffer.memcpy4((byte*)dest, (byte*)src, count * 2);
	}

	private unsafe static void memset(byte* dest, int val, int len)
	{
		if (len < 8)
		{
			while (len != 0)
			{
				*dest = (byte)val;
				dest++;
				len--;
			}
			return;
		}
		if (val != 0)
		{
			val |= val << 8;
			val |= val << 16;
		}
		int num = (int)dest & 3;
		if (num != 0)
		{
			num = 4 - num;
			len -= num;
			do
			{
				*dest = (byte)val;
				dest++;
				num--;
			}
			while (num != 0);
		}
		while (len >= 16)
		{
			*(int*)dest = val;
			*(int*)(dest + 4) = val;
			*(int*)(dest + (nint)2 * (nint)4) = val;
			*(int*)(dest + (nint)3 * (nint)4) = val;
			dest += 16;
			len -= 16;
		}
		while (len >= 4)
		{
			*(int*)dest = val;
			dest += 4;
			len -= 4;
		}
		while (len > 0)
		{
			*dest = (byte)val;
			dest++;
			len--;
		}
	}

	private unsafe static void memcpy(byte* dest, byte* src, int size)
	{
		Buffer.Memcpy(dest, src, size);
	}

	internal unsafe static void bzero(byte* dest, int len)
	{
		memset(dest, 0, len);
	}

	internal unsafe static void bzero_aligned_1(byte* dest, int len)
	{
		*dest = 0;
	}

	internal unsafe static void bzero_aligned_2(byte* dest, int len)
	{
		*(short*)dest = 0;
	}

	internal unsafe static void bzero_aligned_4(byte* dest, int len)
	{
		*(int*)dest = 0;
	}

	internal unsafe static void bzero_aligned_8(byte* dest, int len)
	{
		*(long*)dest = 0L;
	}

	internal unsafe static void memcpy_aligned_1(byte* dest, byte* src, int size)
	{
		*dest = *src;
	}

	internal unsafe static void memcpy_aligned_2(byte* dest, byte* src, int size)
	{
		*(short*)dest = *(short*)src;
	}

	internal unsafe static void memcpy_aligned_4(byte* dest, byte* src, int size)
	{
		*(int*)dest = *(int*)src;
	}

	internal unsafe static void memcpy_aligned_8(byte* dest, byte* src, int size)
	{
		*(long*)dest = *(long*)src;
	}

	private unsafe string CreateString(sbyte* value)
	{
		if (value == null)
		{
			return Empty;
		}
		byte* ptr = (byte*)value;
		int num = 0;
		try
		{
			while (*(ptr++) != 0)
			{
				num++;
			}
		}
		catch (NullReferenceException)
		{
			throw new ArgumentOutOfRangeException("ptr", "Value does not refer to a valid string.");
		}
		return CreateString(value, 0, num, null);
	}

	private unsafe string CreateString(sbyte* value, int startIndex, int length)
	{
		return CreateString(value, startIndex, length, null);
	}

	private unsafe string CreateString(char* value)
	{
		return CtorCharPtr(value);
	}

	private unsafe string CreateString(char* value, int startIndex, int length)
	{
		return CtorCharPtrStartLength(value, startIndex, length);
	}

	private string CreateString(char[] val, int startIndex, int length)
	{
		return CtorCharArrayStartLength(val, startIndex, length);
	}

	private string CreateString(char[] val)
	{
		return CtorCharArray(val);
	}

	private unsafe string CreateString(char c, int count)
	{
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (count == 0)
		{
			return Empty;
		}
		string text = FastAllocateString(count);
		fixed (char* ptr = text)
		{
			char* ptr2 = ptr;
			for (char* ptr3 = ptr2 + count; ptr2 < ptr3; ptr2++)
			{
				*ptr2 = c;
			}
		}
		return text;
	}

	private unsafe string CreateString(sbyte* value, int startIndex, int length, Encoding enc)
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", "Non-negative number required.");
		}
		if (startIndex < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex", "Non-negative number required.");
		}
		if (value + startIndex < value)
		{
			throw new ArgumentOutOfRangeException("startIndex", "Value, startIndex and length do not refer to a valid string.");
		}
		if (enc == null)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (length == 0)
			{
				return Empty;
			}
			enc = Encoding.Default;
		}
		byte[] array = new byte[length];
		if (length != 0)
		{
			fixed (byte* dest = array)
			{
				try
				{
					if (value == null)
					{
						throw new ArgumentOutOfRangeException("ptr", "Value, startIndex and length do not refer to a valid string.");
					}
					memcpy(dest, (byte*)(value + startIndex), length);
				}
				catch (NullReferenceException)
				{
					throw new ArgumentOutOfRangeException("ptr", "Value, startIndex and length do not refer to a valid string.");
				}
			}
		}
		return enc.GetString(array);
	}
}
