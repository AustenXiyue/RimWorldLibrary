using System.Globalization;
using System.Runtime.InteropServices;

namespace System;

/// <summary>Represents a character as a UTF-16 code unit.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
public struct Char : IComparable, IConvertible, IComparable<char>, IEquatable<char>
{
	internal char m_value;

	/// <summary>Represents the largest possible value of a <see cref="T:System.Char" />. This field is constant.</summary>
	/// <filterpriority>1</filterpriority>
	public const char MaxValue = '\uffff';

	/// <summary>Represents the smallest possible value of a <see cref="T:System.Char" />. This field is constant.</summary>
	/// <filterpriority>1</filterpriority>
	public const char MinValue = '\0';

	private static readonly byte[] categoryForLatin1 = new byte[256]
	{
		14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
		14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
		14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
		14, 14, 11, 24, 24, 24, 26, 24, 24, 24,
		20, 21, 24, 25, 24, 19, 24, 24, 8, 8,
		8, 8, 8, 8, 8, 8, 8, 8, 24, 24,
		25, 25, 25, 24, 24, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 20, 24, 21, 27, 18, 27, 1, 1, 1,
		1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
		1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
		1, 1, 1, 20, 25, 21, 25, 14, 14, 14,
		14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
		14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
		14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
		11, 24, 26, 26, 26, 26, 28, 28, 27, 28,
		1, 22, 25, 19, 28, 27, 28, 25, 10, 10,
		27, 1, 28, 24, 27, 10, 1, 23, 10, 10,
		10, 24, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 25, 0, 0, 0, 0,
		0, 0, 0, 1, 1, 1, 1, 1, 1, 1,
		1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
		1, 1, 1, 1, 1, 1, 1, 25, 1, 1,
		1, 1, 1, 1, 1, 1
	};

	internal const int UNICODE_PLANE00_END = 65535;

	internal const int UNICODE_PLANE01_START = 65536;

	internal const int UNICODE_PLANE16_END = 1114111;

	internal const int HIGH_SURROGATE_START = 55296;

	internal const int LOW_SURROGATE_END = 57343;

	private static bool IsLatin1(char ch)
	{
		return ch <= 'ÿ';
	}

	private static bool IsAscii(char ch)
	{
		return ch <= '\u007f';
	}

	private static UnicodeCategory GetLatin1UnicodeCategory(char ch)
	{
		return (UnicodeCategory)categoryForLatin1[(uint)ch];
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <filterpriority>2</filterpriority>
	public override int GetHashCode()
	{
		return (int)(this | ((uint)this << 16));
	}

	/// <summary>Returns a value that indicates whether this instance is equal to a specified object.</summary>
	/// <returns>true if <paramref name="obj" /> is an instance of <see cref="T:System.Char" /> and equals the value of this instance; otherwise, false.</returns>
	/// <param name="obj">An object to compare with this instance or null. </param>
	/// <filterpriority>2</filterpriority>
	public override bool Equals(object obj)
	{
		if (!(obj is char))
		{
			return false;
		}
		return this == (char)obj;
	}

	/// <summary>Returns a value that indicates whether this instance is equal to the specified <see cref="T:System.Char" /> object.</summary>
	/// <returns>true if the <paramref name="obj" /> parameter equals the value of this instance; otherwise, false.</returns>
	/// <param name="obj">An object to compare to this instance. </param>
	/// <filterpriority>2</filterpriority>
	public bool Equals(char obj)
	{
		return this == obj;
	}

	/// <summary>Compares this instance to a specified object and indicates whether this instance precedes, follows, or appears in the same position in the sort order as the specified <see cref="T:System.Object" />.</summary>
	/// <returns>A signed number indicating the position of this instance in the sort order in relation to the <paramref name="value" /> parameter.Return Value Description Less than zero This instance precedes <paramref name="value" />. Zero This instance has the same position in the sort order as <paramref name="value" />. Greater than zero This instance follows <paramref name="value" />.-or- <paramref name="value" /> is null. </returns>
	/// <param name="value">An object to compare this instance to, or null. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is not a <see cref="T:System.Char" /> object. </exception>
	/// <filterpriority>2</filterpriority>
	public int CompareTo(object value)
	{
		if (value == null)
		{
			return 1;
		}
		if (!(value is char))
		{
			throw new ArgumentException(Environment.GetResourceString("Object must be of type Char."));
		}
		return this - (char)value;
	}

	/// <summary>Compares this instance to a specified <see cref="T:System.Char" /> object and indicates whether this instance precedes, follows, or appears in the same position in the sort order as the specified <see cref="T:System.Char" /> object.</summary>
	/// <returns>A signed number indicating the position of this instance in the sort order in relation to the <paramref name="value" /> parameter.Return Value Description Less than zero This instance precedes <paramref name="value" />. Zero This instance has the same position in the sort order as <paramref name="value" />. Greater than zero This instance follows <paramref name="value" />. </returns>
	/// <param name="value">A <see cref="T:System.Char" /> object to compare. </param>
	/// <filterpriority>2</filterpriority>
	public int CompareTo(char value)
	{
		return this - value;
	}

	/// <summary>Converts the value of this instance to its equivalent string representation.</summary>
	/// <returns>The string representation of the value of this instance.</returns>
	/// <filterpriority>1</filterpriority>
	public override string ToString()
	{
		return ToString(this);
	}

	/// <summary>Converts the value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
	/// <returns>The string representation of the value of this instance as specified by <paramref name="provider" />.</returns>
	/// <param name="provider">(Reserved) An object that supplies culture-specific formatting information. </param>
	/// <filterpriority>1</filterpriority>
	public string ToString(IFormatProvider provider)
	{
		return ToString(this);
	}

	/// <summary>Converts the specified Unicode character to its equivalent string representation.</summary>
	/// <returns>The string representation of the value of <paramref name="c" />.</returns>
	/// <param name="c">The Unicode character to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static string ToString(char c)
	{
		return new string(c, 1);
	}

	/// <summary>Converts the value of the specified string to its equivalent Unicode character.</summary>
	/// <returns>A Unicode character equivalent to the sole character in <paramref name="s" />.</returns>
	/// <param name="s">A string that contains a single character, or null. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.FormatException">The length of <paramref name="s" /> is not 1. </exception>
	/// <filterpriority>1</filterpriority>
	public static char Parse(string s)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		if (s.Length != 1)
		{
			throw new FormatException(Environment.GetResourceString("String must be exactly one character long."));
		}
		return s[0];
	}

	/// <summary>Converts the value of the specified string to its equivalent Unicode character. A return code indicates whether the conversion succeeded or failed.</summary>
	/// <returns>true if the <paramref name="s" /> parameter was converted successfully; otherwise, false.</returns>
	/// <param name="s">A string that contains a single character, or null. </param>
	/// <param name="result">When this method returns, contains a Unicode character equivalent to the sole character in <paramref name="s" />, if the conversion succeeded, or an undefined value if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is null or the length of <paramref name="s" /> is not 1. This parameter is passed uninitialized. </param>
	/// <filterpriority>1</filterpriority>
	public static bool TryParse(string s, out char result)
	{
		result = '\0';
		if (s == null)
		{
			return false;
		}
		if (s.Length != 1)
		{
			return false;
		}
		result = s[0];
		return true;
	}

	/// <summary>Indicates whether the specified Unicode character is categorized as a decimal digit.</summary>
	/// <returns>true if <paramref name="c" /> is a decimal digit; otherwise, false.</returns>
	/// <param name="c">The Unicode character to evaluate. </param>
	/// <filterpriority>1</filterpriority>
	public static bool IsDigit(char c)
	{
		if (IsLatin1(c))
		{
			if (c >= '0')
			{
				return c <= '9';
			}
			return false;
		}
		return CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.DecimalDigitNumber;
	}

	internal static bool CheckLetter(UnicodeCategory uc)
	{
		if ((uint)uc <= 4u)
		{
			return true;
		}
		return false;
	}

	/// <summary>Indicates whether the specified Unicode character is categorized as a Unicode letter. </summary>
	/// <returns>true if <paramref name="c" /> is a letter; otherwise, false.</returns>
	/// <param name="c">The Unicode character to evaluate. </param>
	/// <filterpriority>1</filterpriority>
	public static bool IsLetter(char c)
	{
		if (IsLatin1(c))
		{
			if (IsAscii(c))
			{
				c = (char)(c | 0x20);
				if (c >= 'a')
				{
					return c <= 'z';
				}
				return false;
			}
			return CheckLetter(GetLatin1UnicodeCategory(c));
		}
		return CheckLetter(CharUnicodeInfo.GetUnicodeCategory(c));
	}

	private static bool IsWhiteSpaceLatin1(char c)
	{
		switch (c)
		{
		default:
			if (c != '\u00a0' && c != '\u0085')
			{
				return false;
			}
			goto case '\t';
		case '\t':
		case '\n':
		case '\v':
		case '\f':
		case '\r':
		case ' ':
			return true;
		}
	}

	/// <summary>Indicates whether the specified Unicode character is categorized as white space.</summary>
	/// <returns>true if <paramref name="c" /> is white space; otherwise, false.</returns>
	/// <param name="c">The Unicode character to evaluate. </param>
	/// <filterpriority>1</filterpriority>
	public static bool IsWhiteSpace(char c)
	{
		if (IsLatin1(c))
		{
			return IsWhiteSpaceLatin1(c);
		}
		return CharUnicodeInfo.IsWhiteSpace(c);
	}

	/// <summary>Indicates whether the specified Unicode character is categorized as an uppercase letter.</summary>
	/// <returns>true if <paramref name="c" /> is an uppercase letter; otherwise, false.</returns>
	/// <param name="c">The Unicode character to evaluate. </param>
	/// <filterpriority>1</filterpriority>
	public static bool IsUpper(char c)
	{
		if (IsLatin1(c))
		{
			if (IsAscii(c))
			{
				if (c >= 'A')
				{
					return c <= 'Z';
				}
				return false;
			}
			return GetLatin1UnicodeCategory(c) == UnicodeCategory.UppercaseLetter;
		}
		return CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.UppercaseLetter;
	}

	/// <summary>Indicates whether the specified Unicode character is categorized as a lowercase letter.</summary>
	/// <returns>true if <paramref name="c" /> is a lowercase letter; otherwise, false.</returns>
	/// <param name="c">The Unicode character to evaluate. </param>
	/// <filterpriority>1</filterpriority>
	public static bool IsLower(char c)
	{
		if (IsLatin1(c))
		{
			if (IsAscii(c))
			{
				if (c >= 'a')
				{
					return c <= 'z';
				}
				return false;
			}
			return GetLatin1UnicodeCategory(c) == UnicodeCategory.LowercaseLetter;
		}
		return CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.LowercaseLetter;
	}

	internal static bool CheckPunctuation(UnicodeCategory uc)
	{
		if ((uint)(uc - 18) <= 6u)
		{
			return true;
		}
		return false;
	}

	/// <summary>Indicates whether the specified Unicode character is categorized as a punctuation mark.</summary>
	/// <returns>true if <paramref name="c" /> is a punctuation mark; otherwise, false.</returns>
	/// <param name="c">The Unicode character to evaluate. </param>
	/// <filterpriority>1</filterpriority>
	public static bool IsPunctuation(char c)
	{
		if (IsLatin1(c))
		{
			return CheckPunctuation(GetLatin1UnicodeCategory(c));
		}
		return CheckPunctuation(CharUnicodeInfo.GetUnicodeCategory(c));
	}

	internal static bool CheckLetterOrDigit(UnicodeCategory uc)
	{
		if ((uint)uc <= 4u || uc == UnicodeCategory.DecimalDigitNumber)
		{
			return true;
		}
		return false;
	}

	/// <summary>Indicates whether the specified Unicode character is categorized as a letter or a decimal digit.</summary>
	/// <returns>true if <paramref name="c" /> is a letter or a decimal digit; otherwise, false.</returns>
	/// <param name="c">The Unicode character to evaluate. </param>
	/// <filterpriority>1</filterpriority>
	public static bool IsLetterOrDigit(char c)
	{
		if (IsLatin1(c))
		{
			return CheckLetterOrDigit(GetLatin1UnicodeCategory(c));
		}
		return CheckLetterOrDigit(CharUnicodeInfo.GetUnicodeCategory(c));
	}

	/// <summary>Converts the value of a specified Unicode character to its uppercase equivalent using specified culture-specific formatting information.</summary>
	/// <returns>The uppercase equivalent of <paramref name="c" />, modified according to <paramref name="culture" />, or the unchanged value of <paramref name="c" /> if <paramref name="c" /> is already uppercase, has no uppercase equivalent, or is not alphabetic.</returns>
	/// <param name="c">The Unicode character to convert. </param>
	/// <param name="culture">An object that supplies culture-specific casing rules. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="culture" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public static char ToUpper(char c, CultureInfo culture)
	{
		if (culture == null)
		{
			throw new ArgumentNullException("culture");
		}
		return culture.TextInfo.ToUpper(c);
	}

	/// <summary>Converts the value of a Unicode character to its uppercase equivalent.</summary>
	/// <returns>The uppercase equivalent of <paramref name="c" />, or the unchanged value of <paramref name="c" /> if <paramref name="c" /> is already uppercase, has no uppercase equivalent, or is not alphabetic.</returns>
	/// <param name="c">The Unicode character to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static char ToUpper(char c)
	{
		return ToUpper(c, CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the value of a Unicode character to its uppercase equivalent using the casing rules of the invariant culture.</summary>
	/// <returns>The uppercase equivalent of the <paramref name="c" /> parameter, or the unchanged value of <paramref name="c" />, if <paramref name="c" /> is already uppercase or not alphabetic.</returns>
	/// <param name="c">The Unicode character to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static char ToUpperInvariant(char c)
	{
		return ToUpper(c, CultureInfo.InvariantCulture);
	}

	/// <summary>Converts the value of a specified Unicode character to its lowercase equivalent using specified culture-specific formatting information.</summary>
	/// <returns>The lowercase equivalent of <paramref name="c" />, modified according to <paramref name="culture" />, or the unchanged value of <paramref name="c" />, if <paramref name="c" /> is already lowercase or not alphabetic.</returns>
	/// <param name="c">The Unicode character to convert. </param>
	/// <param name="culture">An object that supplies culture-specific casing rules. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="culture" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public static char ToLower(char c, CultureInfo culture)
	{
		if (culture == null)
		{
			throw new ArgumentNullException("culture");
		}
		return culture.TextInfo.ToLower(c);
	}

	/// <summary>Converts the value of a Unicode character to its lowercase equivalent.</summary>
	/// <returns>The lowercase equivalent of <paramref name="c" />, or the unchanged value of <paramref name="c" />, if <paramref name="c" /> is already lowercase or not alphabetic.</returns>
	/// <param name="c">The Unicode character to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static char ToLower(char c)
	{
		return ToLower(c, CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the value of a Unicode character to its lowercase equivalent using the casing rules of the invariant culture.</summary>
	/// <returns>The lowercase equivalent of the <paramref name="c" /> parameter, or the unchanged value of <paramref name="c" />, if <paramref name="c" /> is already lowercase or not alphabetic.</returns>
	/// <param name="c">The Unicode character to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static char ToLowerInvariant(char c)
	{
		return ToLower(c, CultureInfo.InvariantCulture);
	}

	/// <summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.Char" />.</summary>
	/// <returns>The enumerated constant, <see cref="F:System.TypeCode.Char" />.</returns>
	/// <filterpriority>2</filterpriority>
	public TypeCode GetTypeCode()
	{
		return TypeCode.Char;
	}

	/// <summary>Note   This conversion is not supported. Attempting to do so throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported.</exception>
	bool IConvertible.ToBoolean(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Invalid cast from '{0}' to '{1}'.", "Char", "Boolean"));
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToChar(System.IFormatProvider)" />.</summary>
	/// <returns>The value of the current <see cref="T:System.Char" /> object unchanged.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	char IConvertible.ToChar(IFormatProvider provider)
	{
		return this;
	}

	/// <summary> For a description of this member, see <see cref="M:System.IConvertible.ToSByte(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.Char" /> object.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	sbyte IConvertible.ToSByte(IFormatProvider provider)
	{
		return Convert.ToSByte(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToByte(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.Char" /> object.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	byte IConvertible.ToByte(IFormatProvider provider)
	{
		return Convert.ToByte(this);
	}

	/// <summary> For a description of this member, see <see cref="M:System.IConvertible.ToInt16(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.Char" /> object.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	short IConvertible.ToInt16(IFormatProvider provider)
	{
		return Convert.ToInt16(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToUInt16(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.Char" /> object.</returns>
	/// <param name="provider">An <see cref="T:System.IFormatProvider" /> object. (Specify null because the <paramref name="provider" /> parameter is ignored.)</param>
	ushort IConvertible.ToUInt16(IFormatProvider provider)
	{
		return Convert.ToUInt16(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToInt32(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.Char" /> object.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	int IConvertible.ToInt32(IFormatProvider provider)
	{
		return Convert.ToInt32(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToUInt32(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.Char" /> object.</returns>
	/// <param name="provider">An <see cref="T:System.IFormatProvider" /> object. (Specify null because the <paramref name="provider" /> parameter is ignored.)</param>
	uint IConvertible.ToUInt32(IFormatProvider provider)
	{
		return Convert.ToUInt32(this);
	}

	/// <summary> For a description of this member, see <see cref="M:System.IConvertible.ToInt64(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.Char" /> object.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	long IConvertible.ToInt64(IFormatProvider provider)
	{
		return Convert.ToInt64(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToUInt64(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.Char" /> object.</returns>
	/// <param name="provider">An <see cref="T:System.IFormatProvider" /> object. (Specify null because the <paramref name="provider" /> parameter is ignored.)</param>
	ulong IConvertible.ToUInt64(IFormatProvider provider)
	{
		return Convert.ToUInt64(this);
	}

	/// <summary>Note   This conversion is not supported. Attempting to do so throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>No value is returned.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported.</exception>
	float IConvertible.ToSingle(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Invalid cast from '{0}' to '{1}'.", "Char", "Single"));
	}

	/// <summary>Note   This conversion is not supported. Attempting to do so throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>No value is returned.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported.</exception>
	double IConvertible.ToDouble(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Invalid cast from '{0}' to '{1}'.", "Char", "Double"));
	}

	/// <summary>Note   This conversion is not supported. Attempting to do so throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>No value is returned.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported.</exception>
	decimal IConvertible.ToDecimal(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Invalid cast from '{0}' to '{1}'.", "Char", "Decimal"));
	}

	/// <summary>Note   This conversion is not supported. Attempting to do so throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>No value is returned.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported.</exception>
	DateTime IConvertible.ToDateTime(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Invalid cast from '{0}' to '{1}'.", "Char", "DateTime"));
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToType(System.Type,System.IFormatProvider)" />.</summary>
	/// <returns>An object of the specified type.</returns>
	/// <param name="type">A <see cref="T:System.Type" /> object. </param>
	/// <param name="provider">An <see cref="T:System.IFormatProvider" /> object. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null.</exception>
	/// <exception cref="T:System.InvalidCastException">The value of the current <see cref="T:System.Char" /> object cannot be converted to the type specified by the <paramref name="type" /> parameter. </exception>
	object IConvertible.ToType(Type type, IFormatProvider provider)
	{
		return Convert.DefaultToType(this, type, provider);
	}

	/// <summary>Indicates whether the specified Unicode character is categorized as a control character.</summary>
	/// <returns>true if <paramref name="c" /> is a control character; otherwise, false.</returns>
	/// <param name="c">The Unicode character to evaluate. </param>
	/// <filterpriority>1</filterpriority>
	public static bool IsControl(char c)
	{
		if (IsLatin1(c))
		{
			return GetLatin1UnicodeCategory(c) == UnicodeCategory.Control;
		}
		return CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.Control;
	}

	/// <summary>Indicates whether the character at the specified position in a specified string is categorized as a control character.</summary>
	/// <returns>true if the character at position <paramref name="index" /> in <paramref name="s" /> is a control character; otherwise, false.</returns>
	/// <param name="s">A string. </param>
	/// <param name="index">The position of the character to evaluate in <paramref name="s" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool IsControl(string s, int index)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		if ((uint)index >= (uint)s.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		char ch = s[index];
		if (IsLatin1(ch))
		{
			return GetLatin1UnicodeCategory(ch) == UnicodeCategory.Control;
		}
		return CharUnicodeInfo.GetUnicodeCategory(s, index) == UnicodeCategory.Control;
	}

	/// <summary>Indicates whether the character at the specified position in a specified string is categorized as a decimal digit.</summary>
	/// <returns>true if the character at position <paramref name="index" /> in <paramref name="s" /> is a decimal digit; otherwise, false.</returns>
	/// <param name="s">A string. </param>
	/// <param name="index">The position of the character to evaluate in <paramref name="s" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool IsDigit(string s, int index)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		if ((uint)index >= (uint)s.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		char c = s[index];
		if (IsLatin1(c))
		{
			if (c >= '0')
			{
				return c <= '9';
			}
			return false;
		}
		return CharUnicodeInfo.GetUnicodeCategory(s, index) == UnicodeCategory.DecimalDigitNumber;
	}

	/// <summary>Indicates whether the character at the specified position in a specified string is categorized as a Unicode letter.</summary>
	/// <returns>true if the character at position <paramref name="index" /> in <paramref name="s" /> is a letter; otherwise, false.</returns>
	/// <param name="s">A string. </param>
	/// <param name="index">The position of the character to evaluate in <paramref name="s" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool IsLetter(string s, int index)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		if ((uint)index >= (uint)s.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		char c = s[index];
		if (IsLatin1(c))
		{
			if (IsAscii(c))
			{
				c = (char)(c | 0x20);
				if (c >= 'a')
				{
					return c <= 'z';
				}
				return false;
			}
			return CheckLetter(GetLatin1UnicodeCategory(c));
		}
		return CheckLetter(CharUnicodeInfo.GetUnicodeCategory(s, index));
	}

	/// <summary>Indicates whether the character at the specified position in a specified string is categorized as a letter or a decimal digit.</summary>
	/// <returns>true if the character at position <paramref name="index" /> in <paramref name="s" /> is a letter or a decimal digit; otherwise, false.</returns>
	/// <param name="s">A string. </param>
	/// <param name="index">The position of the character to evaluate in <paramref name="s" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool IsLetterOrDigit(string s, int index)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		if ((uint)index >= (uint)s.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		char ch = s[index];
		if (IsLatin1(ch))
		{
			return CheckLetterOrDigit(GetLatin1UnicodeCategory(ch));
		}
		return CheckLetterOrDigit(CharUnicodeInfo.GetUnicodeCategory(s, index));
	}

	/// <summary>Indicates whether the character at the specified position in a specified string is categorized as a lowercase letter.</summary>
	/// <returns>true if the character at position <paramref name="index" /> in <paramref name="s" /> is a lowercase letter; otherwise, false.</returns>
	/// <param name="s">A string. </param>
	/// <param name="index">The position of the character to evaluate in <paramref name="s" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool IsLower(string s, int index)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		if ((uint)index >= (uint)s.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		char c = s[index];
		if (IsLatin1(c))
		{
			if (IsAscii(c))
			{
				if (c >= 'a')
				{
					return c <= 'z';
				}
				return false;
			}
			return GetLatin1UnicodeCategory(c) == UnicodeCategory.LowercaseLetter;
		}
		return CharUnicodeInfo.GetUnicodeCategory(s, index) == UnicodeCategory.LowercaseLetter;
	}

	internal static bool CheckNumber(UnicodeCategory uc)
	{
		if ((uint)(uc - 8) <= 2u)
		{
			return true;
		}
		return false;
	}

	/// <summary>Indicates whether the specified Unicode character is categorized as a number.</summary>
	/// <returns>true if <paramref name="c" /> is a number; otherwise, false.</returns>
	/// <param name="c">The Unicode character to evaluate. </param>
	/// <filterpriority>1</filterpriority>
	public static bool IsNumber(char c)
	{
		if (IsLatin1(c))
		{
			if (IsAscii(c))
			{
				if (c >= '0')
				{
					return c <= '9';
				}
				return false;
			}
			return CheckNumber(GetLatin1UnicodeCategory(c));
		}
		return CheckNumber(CharUnicodeInfo.GetUnicodeCategory(c));
	}

	/// <summary>Indicates whether the character at the specified position in a specified string is categorized as a number.</summary>
	/// <returns>true if the character at position <paramref name="index" /> in <paramref name="s" /> is a number; otherwise, false.</returns>
	/// <param name="s">A string. </param>
	/// <param name="index">The position of the character to evaluate in <paramref name="s" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool IsNumber(string s, int index)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		if ((uint)index >= (uint)s.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		char c = s[index];
		if (IsLatin1(c))
		{
			if (IsAscii(c))
			{
				if (c >= '0')
				{
					return c <= '9';
				}
				return false;
			}
			return CheckNumber(GetLatin1UnicodeCategory(c));
		}
		return CheckNumber(CharUnicodeInfo.GetUnicodeCategory(s, index));
	}

	/// <summary>Indicates whether the character at the specified position in a specified string is categorized as a punctuation mark.</summary>
	/// <returns>true if the character at position <paramref name="index" /> in <paramref name="s" /> is a punctuation mark; otherwise, false.</returns>
	/// <param name="s">A string. </param>
	/// <param name="index">The position of the character to evaluate in <paramref name="s" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool IsPunctuation(string s, int index)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		if ((uint)index >= (uint)s.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		char ch = s[index];
		if (IsLatin1(ch))
		{
			return CheckPunctuation(GetLatin1UnicodeCategory(ch));
		}
		return CheckPunctuation(CharUnicodeInfo.GetUnicodeCategory(s, index));
	}

	internal static bool CheckSeparator(UnicodeCategory uc)
	{
		if ((uint)(uc - 11) <= 2u)
		{
			return true;
		}
		return false;
	}

	private static bool IsSeparatorLatin1(char c)
	{
		if (c != ' ')
		{
			return c == '\u00a0';
		}
		return true;
	}

	/// <summary>Indicates whether the specified Unicode character is categorized as a separator character.</summary>
	/// <returns>true if <paramref name="c" /> is a separator character; otherwise, false.</returns>
	/// <param name="c">The Unicode character to evaluate. </param>
	/// <filterpriority>1</filterpriority>
	public static bool IsSeparator(char c)
	{
		if (IsLatin1(c))
		{
			return IsSeparatorLatin1(c);
		}
		return CheckSeparator(CharUnicodeInfo.GetUnicodeCategory(c));
	}

	/// <summary>Indicates whether the character at the specified position in a specified string is categorized as a separator character.</summary>
	/// <returns>true if the character at position <paramref name="index" /> in <paramref name="s" /> is a separator character; otherwise, false.</returns>
	/// <param name="s">A string. </param>
	/// <param name="index">The position of the character to evaluate in <paramref name="s" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool IsSeparator(string s, int index)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		if ((uint)index >= (uint)s.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		char c = s[index];
		if (IsLatin1(c))
		{
			return IsSeparatorLatin1(c);
		}
		return CheckSeparator(CharUnicodeInfo.GetUnicodeCategory(s, index));
	}

	/// <summary>Indicates whether the specified character has a surrogate code unit.</summary>
	/// <returns>true if <paramref name="c" /> is either a high surrogate or a low surrogate; otherwise, false.</returns>
	/// <param name="c">The Unicode character to evaluate. </param>
	/// <filterpriority>1</filterpriority>
	public static bool IsSurrogate(char c)
	{
		if (c >= '\ud800')
		{
			return c <= '\udfff';
		}
		return false;
	}

	/// <summary>Indicates whether the character at the specified position in a specified string has a surrogate code unit.</summary>
	/// <returns>true if the character at position <paramref name="index" /> in <paramref name="s" /> is a either a high surrogate or a low surrogate; otherwise, false.</returns>
	/// <param name="s">A string. </param>
	/// <param name="index">The position of the character to evaluate in <paramref name="s" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool IsSurrogate(string s, int index)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		if ((uint)index >= (uint)s.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return IsSurrogate(s[index]);
	}

	internal static bool CheckSymbol(UnicodeCategory uc)
	{
		if ((uint)(uc - 25) <= 3u)
		{
			return true;
		}
		return false;
	}

	/// <summary>Indicates whether the specified Unicode character is categorized as a symbol character.</summary>
	/// <returns>true if <paramref name="c" /> is a symbol character; otherwise, false.</returns>
	/// <param name="c">The Unicode character to evaluate. </param>
	/// <filterpriority>1</filterpriority>
	public static bool IsSymbol(char c)
	{
		if (IsLatin1(c))
		{
			return CheckSymbol(GetLatin1UnicodeCategory(c));
		}
		return CheckSymbol(CharUnicodeInfo.GetUnicodeCategory(c));
	}

	/// <summary>Indicates whether the character at the specified position in a specified string is categorized as a symbol character.</summary>
	/// <returns>true if the character at position <paramref name="index" /> in <paramref name="s" /> is a symbol character; otherwise, false.</returns>
	/// <param name="s">A string. </param>
	/// <param name="index">The position of the character to evaluate in <paramref name="s" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool IsSymbol(string s, int index)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		if ((uint)index >= (uint)s.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (IsLatin1(s[index]))
		{
			return CheckSymbol(GetLatin1UnicodeCategory(s[index]));
		}
		return CheckSymbol(CharUnicodeInfo.GetUnicodeCategory(s, index));
	}

	/// <summary>Indicates whether the character at the specified position in a specified string is categorized as an uppercase letter.</summary>
	/// <returns>true if the character at position <paramref name="index" /> in <paramref name="s" /> is an uppercase letter; otherwise, false.</returns>
	/// <param name="s">A string. </param>
	/// <param name="index">The position of the character to evaluate in <paramref name="s" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool IsUpper(string s, int index)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		if ((uint)index >= (uint)s.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		char c = s[index];
		if (IsLatin1(c))
		{
			if (IsAscii(c))
			{
				if (c >= 'A')
				{
					return c <= 'Z';
				}
				return false;
			}
			return GetLatin1UnicodeCategory(c) == UnicodeCategory.UppercaseLetter;
		}
		return CharUnicodeInfo.GetUnicodeCategory(s, index) == UnicodeCategory.UppercaseLetter;
	}

	/// <summary>Indicates whether the character at the specified position in a specified string is categorized as white space.</summary>
	/// <returns>true if the character at position <paramref name="index" /> in <paramref name="s" /> is white space; otherwise, false.</returns>
	/// <param name="s">A string. </param>
	/// <param name="index">The position of the character to evaluate in <paramref name="s" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool IsWhiteSpace(string s, int index)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		if ((uint)index >= (uint)s.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (IsLatin1(s[index]))
		{
			return IsWhiteSpaceLatin1(s[index]);
		}
		return CharUnicodeInfo.IsWhiteSpace(s, index);
	}

	/// <summary>Categorizes a specified Unicode character into a group identified by one of the <see cref="T:System.Globalization.UnicodeCategory" /> values.</summary>
	/// <returns>A <see cref="T:System.Globalization.UnicodeCategory" /> value that identifies the group that contains <paramref name="c" />.</returns>
	/// <param name="c">The Unicode character to categorize. </param>
	/// <filterpriority>1</filterpriority>
	public static UnicodeCategory GetUnicodeCategory(char c)
	{
		if (IsLatin1(c))
		{
			return GetLatin1UnicodeCategory(c);
		}
		return CharUnicodeInfo.InternalGetUnicodeCategory(c);
	}

	/// <summary>Categorizes the character at the specified position in a specified string into a group identified by one of the <see cref="T:System.Globalization.UnicodeCategory" /> values.</summary>
	/// <returns>A <see cref="T:System.Globalization.UnicodeCategory" /> enumerated constant that identifies the group that contains the character at position <paramref name="index" /> in <paramref name="s" />.</returns>
	/// <param name="s">A <see cref="T:System.String" />. </param>
	/// <param name="index">The character position in <paramref name="s" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static UnicodeCategory GetUnicodeCategory(string s, int index)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		if ((uint)index >= (uint)s.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (IsLatin1(s[index]))
		{
			return GetLatin1UnicodeCategory(s[index]);
		}
		return CharUnicodeInfo.InternalGetUnicodeCategory(s, index);
	}

	/// <summary>Converts the specified numeric Unicode character to a double-precision floating point number.</summary>
	/// <returns>The numeric value of <paramref name="c" /> if that character represents a number; otherwise, -1.0.</returns>
	/// <param name="c">The Unicode character to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static double GetNumericValue(char c)
	{
		return CharUnicodeInfo.GetNumericValue(c);
	}

	/// <summary>Converts the numeric Unicode character at the specified position in a specified string to a double-precision floating point number.</summary>
	/// <returns>The numeric value of the character at position <paramref name="index" /> in <paramref name="s" /> if that character represents a number; otherwise, -1.</returns>
	/// <param name="s">A <see cref="T:System.String" />. </param>
	/// <param name="index">The character position in <paramref name="s" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static double GetNumericValue(string s, int index)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		if ((uint)index >= (uint)s.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return CharUnicodeInfo.GetNumericValue(s, index);
	}

	/// <summary>Indicates whether the specified <see cref="T:System.Char" /> object is a high surrogate.</summary>
	/// <returns>true if the numeric value of the <paramref name="c" /> parameter ranges from U+D800 through U+DBFF; otherwise, false.</returns>
	/// <param name="c">The Unicode character to evaluate. </param>
	/// <filterpriority>1</filterpriority>
	public static bool IsHighSurrogate(char c)
	{
		if (c >= '\ud800')
		{
			return c <= '\udbff';
		}
		return false;
	}

	/// <summary>Indicates whether the <see cref="T:System.Char" /> object at the specified position in a string is a high surrogate.</summary>
	/// <returns>true if the numeric value of the specified character in the <paramref name="s" /> parameter ranges from U+D800 through U+DBFF; otherwise, false.</returns>
	/// <param name="s">A string. </param>
	/// <param name="index">The position of the character to evaluate in <paramref name="s" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a position within <paramref name="s" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool IsHighSurrogate(string s, int index)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		if (index < 0 || index >= s.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return IsHighSurrogate(s[index]);
	}

	/// <summary>Indicates whether the specified <see cref="T:System.Char" /> object is a low surrogate.</summary>
	/// <returns>true if the numeric value of the <paramref name="c" /> parameter ranges from U+DC00 through U+DFFF; otherwise, false.</returns>
	/// <param name="c">The character to evaluate. </param>
	/// <filterpriority>1</filterpriority>
	public static bool IsLowSurrogate(char c)
	{
		if (c >= '\udc00')
		{
			return c <= '\udfff';
		}
		return false;
	}

	/// <summary>Indicates whether the <see cref="T:System.Char" /> object at the specified position in a string is a low surrogate.</summary>
	/// <returns>true if the numeric value of the specified character in the <paramref name="s" /> parameter ranges from U+DC00 through U+DFFF; otherwise, false.</returns>
	/// <param name="s">A string. </param>
	/// <param name="index">The position of the character to evaluate in <paramref name="s" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a position within <paramref name="s" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool IsLowSurrogate(string s, int index)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		if (index < 0 || index >= s.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return IsLowSurrogate(s[index]);
	}

	/// <summary>Indicates whether two adjacent <see cref="T:System.Char" /> objects at a specified position in a string form a surrogate pair.</summary>
	/// <returns>true if the <paramref name="s" /> parameter includes adjacent characters at positions <paramref name="index" /> and <paramref name="index" /> + 1, and the numeric value of the character at position <paramref name="index" /> ranges from U+D800 through U+DBFF, and the numeric value of the character at position <paramref name="index" />+1 ranges from U+DC00 through U+DFFF; otherwise, false.</returns>
	/// <param name="s">A string. </param>
	/// <param name="index">The starting position of the pair of characters to evaluate within <paramref name="s" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a position within <paramref name="s" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool IsSurrogatePair(string s, int index)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		if (index < 0 || index >= s.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (index + 1 < s.Length)
		{
			return IsSurrogatePair(s[index], s[index + 1]);
		}
		return false;
	}

	/// <summary>Indicates whether the two specified <see cref="T:System.Char" /> objects form a surrogate pair.</summary>
	/// <returns>true if the numeric value of the <paramref name="highSurrogate" /> parameter ranges from U+D800 through U+DBFF, and the numeric value of the <paramref name="lowSurrogate" /> parameter ranges from U+DC00 through U+DFFF; otherwise, false.</returns>
	/// <param name="highSurrogate">The character to evaluate as the high surrogate of a surrogate pair. </param>
	/// <param name="lowSurrogate">The character to evaluate as the low surrogate of a surrogate pair. </param>
	/// <filterpriority>1</filterpriority>
	public static bool IsSurrogatePair(char highSurrogate, char lowSurrogate)
	{
		if (highSurrogate >= '\ud800' && highSurrogate <= '\udbff')
		{
			if (lowSurrogate >= '\udc00')
			{
				return lowSurrogate <= '\udfff';
			}
			return false;
		}
		return false;
	}

	/// <summary>Converts the specified Unicode code point into a UTF-16 encoded string.</summary>
	/// <returns>A string consisting of one <see cref="T:System.Char" /> object or a surrogate pair of <see cref="T:System.Char" /> objects equivalent to the code point specified by the <paramref name="utf32" /> parameter.</returns>
	/// <param name="utf32">A 21-bit Unicode code point. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="utf32" /> is not a valid 21-bit Unicode code point ranging from U+0 through U+10FFFF, excluding the surrogate pair range from U+D800 through U+DFFF. </exception>
	/// <filterpriority>1</filterpriority>
	public static string ConvertFromUtf32(int utf32)
	{
		if (utf32 < 0 || utf32 > 1114111 || (utf32 >= 55296 && utf32 <= 57343))
		{
			throw new ArgumentOutOfRangeException("utf32", Environment.GetResourceString("A valid UTF32 value is between 0x000000 and 0x10ffff, inclusive, and should not include surrogate codepoint values (0x00d800 ~ 0x00dfff)."));
		}
		if (utf32 < 65536)
		{
			return ToString((char)utf32);
		}
		utf32 -= 65536;
		return new string(new char[2]
		{
			(char)(utf32 / 1024 + 55296),
			(char)(utf32 % 1024 + 56320)
		});
	}

	/// <summary>Converts the value of a UTF-16 encoded surrogate pair into a Unicode code point.</summary>
	/// <returns>The 21-bit Unicode code point represented by the <paramref name="highSurrogate" /> and <paramref name="lowSurrogate" /> parameters.</returns>
	/// <param name="highSurrogate">A high surrogate code unit (that is, a code unit ranging from U+D800 through U+DBFF). </param>
	/// <param name="lowSurrogate">A low surrogate code unit (that is, a code unit ranging from U+DC00 through U+DFFF). </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="highSurrogate" /> is not in the range U+D800 through U+DBFF, or <paramref name="lowSurrogate" /> is not in the range U+DC00 through U+DFFF. </exception>
	/// <filterpriority>1</filterpriority>
	public static int ConvertToUtf32(char highSurrogate, char lowSurrogate)
	{
		if (!IsHighSurrogate(highSurrogate))
		{
			throw new ArgumentOutOfRangeException("highSurrogate", Environment.GetResourceString("A valid high surrogate character is between 0xd800 and 0xdbff, inclusive."));
		}
		if (!IsLowSurrogate(lowSurrogate))
		{
			throw new ArgumentOutOfRangeException("lowSurrogate", Environment.GetResourceString("A valid low surrogate character is between 0xdc00 and 0xdfff, inclusive."));
		}
		return (highSurrogate - 55296) * 1024 + (lowSurrogate - 56320) + 65536;
	}

	/// <summary>Converts the value of a UTF-16 encoded character or surrogate pair at a specified position in a string into a Unicode code point.</summary>
	/// <returns>The 21-bit Unicode code point represented by the character or surrogate pair at the position in the <paramref name="s" /> parameter specified by the <paramref name="index" /> parameter.</returns>
	/// <param name="s">A string that contains a character or surrogate pair. </param>
	/// <param name="index">The index position of the character or surrogate pair in <paramref name="s" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a position within <paramref name="s" />. </exception>
	/// <exception cref="T:System.ArgumentException">The specified index position contains a surrogate pair, and either the first character in the pair is not a valid high surrogate or the second character in the pair is not a valid low surrogate. </exception>
	/// <filterpriority>1</filterpriority>
	public static int ConvertToUtf32(string s, int index)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		if (index < 0 || index >= s.Length)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		int num = s[index] - 55296;
		if (num >= 0 && num <= 2047)
		{
			if (num <= 1023)
			{
				if (index < s.Length - 1)
				{
					int num2 = s[index + 1] - 56320;
					if (num2 >= 0 && num2 <= 1023)
					{
						return num * 1024 + num2 + 65536;
					}
					throw new ArgumentException(Environment.GetResourceString("Found a high surrogate char without a following low surrogate at index: {0}. The input may not be in this encoding, or may not contain valid Unicode (UTF-16) characters.", index), "s");
				}
				throw new ArgumentException(Environment.GetResourceString("Found a high surrogate char without a following low surrogate at index: {0}. The input may not be in this encoding, or may not contain valid Unicode (UTF-16) characters.", index), "s");
			}
			throw new ArgumentException(Environment.GetResourceString("Found a low surrogate char without a preceding high surrogate at index: {0}. The input may not be in this encoding, or may not contain valid Unicode (UTF-16) characters.", index), "s");
		}
		return s[index];
	}
}
