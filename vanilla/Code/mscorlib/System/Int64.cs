using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;

namespace System;

/// <summary>Represents a 64-bit signed integer.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
public struct Int64 : IComparable, IFormattable, IConvertible, IComparable<long>, IEquatable<long>
{
	internal long m_value;

	/// <summary>Represents the largest possible value of an Int64. This field is constant.</summary>
	/// <filterpriority>1</filterpriority>
	public const long MaxValue = 9223372036854775807L;

	/// <summary>Represents the smallest possible value of an Int64. This field is constant.</summary>
	/// <filterpriority>1</filterpriority>
	public const long MinValue = -9223372036854775808L;

	/// <summary>Compares this instance to a specified object and returns an indication of their relative values.</summary>
	/// <returns>A signed number indicating the relative values of this instance and <paramref name="value" />.Return Value Description Less than zero This instance is less than <paramref name="value" />. Zero This instance is equal to <paramref name="value" />. Greater than zero This instance is greater than <paramref name="value" />.-or- <paramref name="value" /> is null. </returns>
	/// <param name="value">An object to compare, or null. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is not an <see cref="T:System.Int64" />. </exception>
	/// <filterpriority>2</filterpriority>
	public int CompareTo(object value)
	{
		if (value == null)
		{
			return 1;
		}
		if (value is long num)
		{
			if (this < num)
			{
				return -1;
			}
			if (this > num)
			{
				return 1;
			}
			return 0;
		}
		throw new ArgumentException(Environment.GetResourceString("Object must be of type Int64."));
	}

	/// <summary>Compares this instance to a specified 64-bit signed integer and returns an indication of their relative values.</summary>
	/// <returns>A signed number indicating the relative values of this instance and <paramref name="value" />.Return Value Description Less than zero This instance is less than <paramref name="value" />. Zero This instance is equal to <paramref name="value" />. Greater than zero This instance is greater than <paramref name="value" />. </returns>
	/// <param name="value">An integer to compare. </param>
	/// <filterpriority>2</filterpriority>
	public int CompareTo(long value)
	{
		if (this < value)
		{
			return -1;
		}
		if (this > value)
		{
			return 1;
		}
		return 0;
	}

	/// <summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	/// <returns>true if <paramref name="obj" /> is an instance of an <see cref="T:System.Int64" /> and equals the value of this instance; otherwise, false.</returns>
	/// <param name="obj">An object to compare with this instance. </param>
	/// <filterpriority>2</filterpriority>
	public override bool Equals(object obj)
	{
		if (!(obj is long))
		{
			return false;
		}
		return this == (long)obj;
	}

	/// <summary>Returns a value indicating whether this instance is equal to a specified <see cref="T:System.Int64" /> value.</summary>
	/// <returns>true if <paramref name="obj" /> has the same value as this instance; otherwise, false.</returns>
	/// <param name="obj">An <see cref="T:System.Int64" /> value to compare to this instance.</param>
	/// <filterpriority>2</filterpriority>
	public bool Equals(long obj)
	{
		return this == obj;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <filterpriority>2</filterpriority>
	public override int GetHashCode()
	{
		return (int)this ^ (int)(this >> 32);
	}

	/// <summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
	/// <returns>The string representation of the value of this instance, consisting of a minus sign if the value is negative, and a sequence of digits ranging from 0 to 9 with no leading zeroes.</returns>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public override string ToString()
	{
		return Number.FormatInt64(this, null, NumberFormatInfo.CurrentInfo);
	}

	/// <summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
	/// <returns>The string representation of the value of this instance as specified by <paramref name="provider" />.</returns>
	/// <param name="provider">An <see cref="T:System.IFormatProvider" /> that supplies culture-specific formatting information. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public string ToString(IFormatProvider provider)
	{
		return Number.FormatInt64(this, null, NumberFormatInfo.GetInstance(provider));
	}

	/// <summary>Converts the numeric value of this instance to its equivalent string representation, using the specified format.</summary>
	/// <returns>The string representation of the value of this instance as specified by <paramref name="format" />.</returns>
	/// <param name="format">A numeric format string.</param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is invalid or not supported.</exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public string ToString(string format)
	{
		return Number.FormatInt64(this, format, NumberFormatInfo.CurrentInfo);
	}

	/// <summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
	/// <returns>The string representation of the value of this instance as specified by <paramref name="format" /> and <paramref name="provider" />.</returns>
	/// <param name="format">A numeric format string.</param>
	/// <param name="provider">An object that supplies culture-specific formatting information about this instance. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is invalid or not supported.</exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public string ToString(string format, IFormatProvider provider)
	{
		return Number.FormatInt64(this, format, NumberFormatInfo.GetInstance(provider));
	}

	/// <summary>Converts the string representation of a number to its 64-bit signed integer equivalent.</summary>
	/// <returns>A 64-bit signed integer equivalent to the number contained in <paramref name="s" />.</returns>
	/// <param name="s">A string containing a number to convert. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="s" /> is not in the correct format. </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="s" /> represents a number less than <see cref="F:System.Int64.MinValue" /> or greater than <see cref="F:System.Int64.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static long Parse(string s)
	{
		return Number.ParseInt64(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo);
	}

	/// <summary>Converts the string representation of a number in a specified style to its 64-bit signed integer equivalent.</summary>
	/// <returns>A 64-bit signed integer equivalent to the number specified in <paramref name="s" />.</returns>
	/// <param name="s">A string containing a number to convert. </param>
	/// <param name="style">A bitwise combination of <see cref="T:System.Globalization.NumberStyles" /> values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or-<paramref name="style" /> is not a combination of <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> and <see cref="F:System.Globalization.NumberStyles.HexNumber" /> values.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="s" /> is not in a format compliant with <paramref name="style" />. </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="s" /> represents a number less than <see cref="F:System.Int64.MinValue" /> or greater than <see cref="F:System.Int64.MaxValue" />.-or-<paramref name="style" /> supports fractional digits but <paramref name="s" /> includes non-zero fractional digits. </exception>
	/// <filterpriority>1</filterpriority>
	public static long Parse(string s, NumberStyles style)
	{
		NumberFormatInfo.ValidateParseStyleInteger(style);
		return Number.ParseInt64(s, style, NumberFormatInfo.CurrentInfo);
	}

	/// <summary>Converts the string representation of a number in a specified culture-specific format to its 64-bit signed integer equivalent.</summary>
	/// <returns>A 64-bit signed integer equivalent to the number specified in <paramref name="s" />.</returns>
	/// <param name="s">A string containing a number to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="s" /> is not in the correct format. </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="s" /> represents a number less than <see cref="F:System.Int64.MinValue" /> or greater than <see cref="F:System.Int64.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static long Parse(string s, IFormatProvider provider)
	{
		return Number.ParseInt64(s, NumberStyles.Integer, NumberFormatInfo.GetInstance(provider));
	}

	/// <summary>Converts the string representation of a number in a specified style and culture-specific format to its 64-bit signed integer equivalent.</summary>
	/// <returns>A 64-bit signed integer equivalent to the number specified in <paramref name="s" />.</returns>
	/// <param name="s">A string containing a number to convert. </param>
	/// <param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
	/// <param name="provider">An <see cref="T:System.IFormatProvider" /> that supplies culture-specific formatting information about <paramref name="s" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or-<paramref name="style" /> is not a combination of <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> and <see cref="F:System.Globalization.NumberStyles.HexNumber" /> values.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="s" /> is not in a format compliant with <paramref name="style" />. </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="s" /> represents a number less than <see cref="F:System.Int64.MinValue" /> or greater than <see cref="F:System.Int64.MaxValue" />. -or-<paramref name="style" /> supports fractional digits, but <paramref name="s" /> includes non-zero fractional digits.</exception>
	/// <filterpriority>1</filterpriority>
	public static long Parse(string s, NumberStyles style, IFormatProvider provider)
	{
		NumberFormatInfo.ValidateParseStyleInteger(style);
		return Number.ParseInt64(s, style, NumberFormatInfo.GetInstance(provider));
	}

	/// <summary>Converts the string representation of a number to its 64-bit signed integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	/// <returns>true if <paramref name="s" /> was converted successfully; otherwise, false.</returns>
	/// <param name="s">A string containing a number to convert. </param>
	/// <param name="result">When this method returns, contains the 64-bit signed integer value equivalent to the number contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is null, is not of the correct format, or represents a number less than <see cref="F:System.Int64.MinValue" /> or greater than <see cref="F:System.Int64.MaxValue" />. This parameter is passed uninitialized. </param>
	/// <filterpriority>1</filterpriority>
	public static bool TryParse(string s, out long result)
	{
		return Number.TryParseInt64(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
	}

	/// <summary>Converts the string representation of a number in a specified style and culture-specific format to its 64-bit signed integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	/// <returns>true if <paramref name="s" /> was converted successfully; otherwise, false.</returns>
	/// <param name="s">A string containing a number to convert. The string is interpreted using the style specified by <paramref name="style" />. </param>
	/// <param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
	/// <param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />. </param>
	/// <param name="result">When this method returns, contains the 64-bit signed integer value equivalent to the number contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is null, is not in a format compliant with <paramref name="style" />, or represents a number less than <see cref="F:System.Int64.MinValue" /> or greater than <see cref="F:System.Int64.MaxValue" />. This parameter is passed uninitialized. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or-<paramref name="style" /> is not a combination of <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> and <see cref="F:System.Globalization.NumberStyles.HexNumber" /> values.</exception>
	/// <filterpriority>1</filterpriority>
	public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out long result)
	{
		NumberFormatInfo.ValidateParseStyleInteger(style);
		return Number.TryParseInt64(s, style, NumberFormatInfo.GetInstance(provider), out result);
	}

	/// <summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.Int64" />.</summary>
	/// <returns>The enumerated constant, <see cref="F:System.TypeCode.Int64" />.</returns>
	/// <filterpriority>2</filterpriority>
	public TypeCode GetTypeCode()
	{
		return TypeCode.Int64;
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToBoolean(System.IFormatProvider)" />. </summary>
	/// <returns>true if the value of the current instance is not zero; otherwise, false.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	bool IConvertible.ToBoolean(IFormatProvider provider)
	{
		return Convert.ToBoolean(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToChar(System.IFormatProvider)" />. </summary>
	/// <returns>The value of the current instance, converted to a <see cref="T:System.Char" />.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	char IConvertible.ToChar(IFormatProvider provider)
	{
		return Convert.ToChar(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToSByte(System.IFormatProvider)" />. </summary>
	/// <returns>The value of the current instance, converted to an <see cref="T:System.SByte" />.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	sbyte IConvertible.ToSByte(IFormatProvider provider)
	{
		return Convert.ToSByte(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToByte(System.IFormatProvider)" />. </summary>
	/// <returns>The value of the current instance, converted to a <see cref="T:System.Byte" />.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	byte IConvertible.ToByte(IFormatProvider provider)
	{
		return Convert.ToByte(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToInt16(System.IFormatProvider)" />. </summary>
	/// <returns>The value of the current instance, converted to an <see cref="T:System.Int16" />.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	short IConvertible.ToInt16(IFormatProvider provider)
	{
		return Convert.ToInt16(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToUInt16(System.IFormatProvider)" />. </summary>
	/// <returns>The value of the current instance, converted to a <see cref="T:System.UInt16" />.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	ushort IConvertible.ToUInt16(IFormatProvider provider)
	{
		return Convert.ToUInt16(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToInt32(System.IFormatProvider)" />. </summary>
	/// <returns>The value of the current instance, converted to an <see cref="T:System.Int32" />.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	int IConvertible.ToInt32(IFormatProvider provider)
	{
		return Convert.ToInt32(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToUInt32(System.IFormatProvider)" />. </summary>
	/// <returns>The value of the current instance, converted to a <see cref="T:System.UInt32" />.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	uint IConvertible.ToUInt32(IFormatProvider provider)
	{
		return Convert.ToUInt32(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToInt64(System.IFormatProvider)" />. </summary>
	/// <returns>The value of the current instance, unchanged.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	long IConvertible.ToInt64(IFormatProvider provider)
	{
		return this;
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToUInt64(System.IFormatProvider)" />. </summary>
	/// <returns>The value of the current instance, converted to a <see cref="T:System.UInt64" />.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	ulong IConvertible.ToUInt64(IFormatProvider provider)
	{
		return Convert.ToUInt64(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToSingle(System.IFormatProvider)" />. </summary>
	/// <returns>The value of the current instance, converted to a <see cref="T:System.Single" />.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	float IConvertible.ToSingle(IFormatProvider provider)
	{
		return Convert.ToSingle(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToDouble(System.IFormatProvider)" />. </summary>
	/// <returns>The value of the current instance, converted to a <see cref="T:System.Double" />.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	double IConvertible.ToDouble(IFormatProvider provider)
	{
		return Convert.ToDouble(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToDecimal(System.IFormatProvider)" />. </summary>
	/// <returns>The value of the current instance, converted to a <see cref="T:System.Decimal" />.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	decimal IConvertible.ToDecimal(IFormatProvider provider)
	{
		return Convert.ToDecimal(this);
	}

	/// <summary>This conversion is not supported. Attempting to use this method throws an <see cref="T:System.InvalidCastException" />. </summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	/// <exception cref="T:System.InvalidCastException">In all cases.</exception>
	DateTime IConvertible.ToDateTime(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Invalid cast from '{0}' to '{1}'.", "Int64", "DateTime"));
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToType(System.Type,System.IFormatProvider)" />. </summary>
	/// <returns>The value of the current instance, converted to <paramref name="type" />.</returns>
	/// <param name="type">The type to which to convert this <see cref="T:System.Int64" /> value.</param>
	/// <param name="provider">An <see cref="T:System.IFormatProvider" /> implementation that provides information about the format of the returned value.</param>
	object IConvertible.ToType(Type type, IFormatProvider provider)
	{
		return Convert.DefaultToType(this, type, provider);
	}
}
