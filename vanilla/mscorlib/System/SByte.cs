using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;

namespace System;

/// <summary>Represents an 8-bit signed integer.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[CLSCompliant(false)]
[ComVisible(true)]
public struct SByte : IComparable, IFormattable, IConvertible, IComparable<sbyte>, IEquatable<sbyte>
{
	private sbyte m_value;

	/// <summary>Represents the largest possible value of <see cref="T:System.SByte" />. This field is constant.</summary>
	/// <filterpriority>1</filterpriority>
	public const sbyte MaxValue = 127;

	/// <summary>Represents the smallest possible value of <see cref="T:System.SByte" />. This field is constant.</summary>
	/// <filterpriority>1</filterpriority>
	public const sbyte MinValue = -128;

	/// <summary>Compares this instance to a specified object and returns an indication of their relative values.</summary>
	/// <returns>A signed number indicating the relative values of this instance and <paramref name="obj" />.Return Value Description Less than zero This instance is less than <paramref name="obj" />. Zero This instance is equal to <paramref name="obj" />. Greater than zero This instance is greater than <paramref name="obj" />.-or- <paramref name="obj" /> is null. </returns>
	/// <param name="obj">An object to compare, or null. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="obj" /> is not an <see cref="T:System.SByte" />. </exception>
	/// <filterpriority>2</filterpriority>
	public int CompareTo(object obj)
	{
		if (obj == null)
		{
			return 1;
		}
		if (!(obj is sbyte))
		{
			throw new ArgumentException(Environment.GetResourceString("Object must be of type SByte."));
		}
		return this - (sbyte)obj;
	}

	/// <summary>Compares this instance to a specified 8-bit signed integer and returns an indication of their relative values.</summary>
	/// <returns>A signed integer that indicates the relative order of this instance and <paramref name="value" />.Return Value Description Less than zero This instance is less than <paramref name="value" />. Zero This instance is equal to <paramref name="value" />. Greater than zero This instance is greater than <paramref name="value" />. </returns>
	/// <param name="value">An 8-bit signed integer to compare. </param>
	/// <filterpriority>2</filterpriority>
	public int CompareTo(sbyte value)
	{
		return this - value;
	}

	/// <summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	/// <returns>true if <paramref name="obj" /> is an instance of <see cref="T:System.SByte" /> and equals the value of this instance; otherwise, false.</returns>
	/// <param name="obj">An object to compare with this instance. </param>
	/// <filterpriority>2</filterpriority>
	public override bool Equals(object obj)
	{
		if (!(obj is sbyte))
		{
			return false;
		}
		return this == (sbyte)obj;
	}

	/// <summary>Returns a value indicating whether this instance is equal to a specified <see cref="T:System.SByte" /> value.</summary>
	/// <returns>true if <paramref name="obj" /> has the same value as this instance; otherwise, false.</returns>
	/// <param name="obj">An <see cref="T:System.SByte" /> value to compare to this instance.</param>
	/// <filterpriority>2</filterpriority>
	public bool Equals(sbyte obj)
	{
		return this == obj;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <filterpriority>2</filterpriority>
	public override int GetHashCode()
	{
		return this ^ (this << 8);
	}

	/// <summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
	/// <returns>The string representation of the value of this instance, consisting of a negative sign if the value is negative, and a sequence of digits ranging from 0 to 9 with no leading zeroes.</returns>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public override string ToString()
	{
		return Number.FormatInt32(this, null, NumberFormatInfo.CurrentInfo);
	}

	/// <summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
	/// <returns>The string representation of the value of this instance, as specified by <paramref name="provider" />.</returns>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public string ToString(IFormatProvider provider)
	{
		return Number.FormatInt32(this, null, NumberFormatInfo.GetInstance(provider));
	}

	/// <summary>Converts the numeric value of this instance to its equivalent string representation, using the specified format.</summary>
	/// <returns>The string representation of the value of this instance as specified by <paramref name="format" />.</returns>
	/// <param name="format">A standard or custom numeric format string.</param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is invalid. </exception>
	/// <filterpriority>1</filterpriority>
	public string ToString(string format)
	{
		return ToString(format, NumberFormatInfo.CurrentInfo);
	}

	/// <summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
	/// <returns>The string representation of the value of this instance as specified by <paramref name="format" /> and <paramref name="provider" />.</returns>
	/// <param name="format">A standard or custom numeric format string.</param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is invalid. </exception>
	/// <filterpriority>1</filterpriority>
	public string ToString(string format, IFormatProvider provider)
	{
		return ToString(format, NumberFormatInfo.GetInstance(provider));
	}

	[SecuritySafeCritical]
	private string ToString(string format, NumberFormatInfo info)
	{
		if (this < 0 && format != null && format.Length > 0 && (format[0] == 'X' || format[0] == 'x'))
		{
			return Number.FormatUInt32((uint)(this & 0xFF), format, info);
		}
		return Number.FormatInt32(this, format, info);
	}

	/// <summary>Converts the string representation of a number to its 8-bit signed integer equivalent.</summary>
	/// <returns>An 8-bit signed integer that is equivalent to the number contained in the <paramref name="s" /> parameter.</returns>
	/// <param name="s">A string that represents a number to convert. The string is interpreted using the <see cref="F:System.Globalization.NumberStyles.Integer" /> style.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="s" /> does not consist of an optional sign followed by a sequence of digits (zero through nine). </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="s" /> represents a number less than <see cref="F:System.SByte.MinValue" /> or greater than <see cref="F:System.SByte.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static sbyte Parse(string s)
	{
		return Parse(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo);
	}

	/// <summary>Converts the string representation of a number in a specified style to its 8-bit signed integer equivalent.</summary>
	/// <returns>An 8-bit signed integer that is equivalent to the number specified in <paramref name="s" />.</returns>
	/// <param name="s">A string that contains a number to convert. The string is interpreted using the style specified by <paramref name="style" />.</param>
	/// <param name="style">A bitwise combination of the enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="s" /> is not in a format that is compliant with <paramref name="style" />. </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="s" /> represents a number less than <see cref="F:System.SByte.MinValue" /> or greater than <see cref="F:System.SByte.MaxValue" />. -or-<paramref name="s" /> includes non-zero, fractional digits.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or-<paramref name="style" /> is not a combination of <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> and <see cref="F:System.Globalization.NumberStyles.HexNumber" /> values.</exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static sbyte Parse(string s, NumberStyles style)
	{
		NumberFormatInfo.ValidateParseStyleInteger(style);
		return Parse(s, style, NumberFormatInfo.CurrentInfo);
	}

	/// <summary>Converts the string representation of a number in a specified culture-specific format to its 8-bit signed integer equivalent.</summary>
	/// <returns>An 8-bit signed integer that is equivalent to the number specified in <paramref name="s" />.</returns>
	/// <param name="s">A string that represents a number to convert. The string is interpreted using the <see cref="F:System.Globalization.NumberStyles.Integer" /> style.</param>
	/// <param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />. If <paramref name="provider" /> is null, the thread current culture is used.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="s" /> is not in the correct format.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="s" /> represents a number less than <see cref="F:System.SByte.MinValue" /> or greater than <see cref="F:System.SByte.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static sbyte Parse(string s, IFormatProvider provider)
	{
		return Parse(s, NumberStyles.Integer, NumberFormatInfo.GetInstance(provider));
	}

	/// <summary>Converts the string representation of a number that is in a specified style and culture-specific format to its 8-bit signed equivalent.</summary>
	/// <returns>An 8-bit signed byte value that is equivalent to the number specified in the <paramref name="s" /> parameter.</returns>
	/// <param name="s">A string that contains the number to convert. The string is interpreted by using the style specified by <paramref name="style" />.</param>
	/// <param name="style">A bitwise combination of the enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
	/// <param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />. If <paramref name="provider" /> is null, the thread current culture is used.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value.-or-<paramref name="style" /> is not a combination of <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> and <see cref="F:System.Globalization.NumberStyles.HexNumber" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="s" /> is not in a format that is compliant with <paramref name="style" />.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="s" /> represents a number that is less than <see cref="F:System.SByte.MinValue" /> or greater than <see cref="F:System.SByte.MaxValue" />.-or-<paramref name="s" /> includes non-zero, fractional digits.</exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static sbyte Parse(string s, NumberStyles style, IFormatProvider provider)
	{
		NumberFormatInfo.ValidateParseStyleInteger(style);
		return Parse(s, style, NumberFormatInfo.GetInstance(provider));
	}

	private static sbyte Parse(string s, NumberStyles style, NumberFormatInfo info)
	{
		int num = 0;
		try
		{
			num = Number.ParseInt32(s, style, info);
		}
		catch (OverflowException innerException)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a signed byte."), innerException);
		}
		if ((style & NumberStyles.AllowHexSpecifier) != 0)
		{
			if (num < 0 || num > 255)
			{
				throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a signed byte."));
			}
			return (sbyte)num;
		}
		if (num < -128 || num > 127)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a signed byte."));
		}
		return (sbyte)num;
	}

	/// <summary>Tries to convert the string representation of a number to its <see cref="T:System.SByte" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	/// <returns>true if <paramref name="s" /> was converted successfully; otherwise, false.</returns>
	/// <param name="s">A string that contains a number to convert.</param>
	/// <param name="result">When this method returns, contains the 8-bit signed integer value that is equivalent to the number contained in <paramref name="s" /> if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is null, is not in the correct format, or represents a number that is less than <see cref="F:System.SByte.MinValue" /> or greater than <see cref="F:System.SByte.MaxValue" />. This parameter is passed uninitialized.</param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static bool TryParse(string s, out sbyte result)
	{
		return TryParse(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
	}

	/// <summary>Tries to convert the string representation of a number in a specified style and culture-specific format to its <see cref="T:System.SByte" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	/// <returns>true if <paramref name="s" /> was converted successfully; otherwise, false.</returns>
	/// <param name="s">A string representing a number to convert. </param>
	/// <param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
	/// <param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />. </param>
	/// <param name="result">When this method returns, contains the 8-bit signed integer value equivalent to the number contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is null, is not in a format compliant with <paramref name="style" />, or represents a number less than <see cref="F:System.SByte.MinValue" /> or greater than <see cref="F:System.SByte.MaxValue" />. This parameter is passed uninitialized. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or-<paramref name="style" /> is not a combination of <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> and <see cref="F:System.Globalization.NumberStyles.HexNumber" /> values.</exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out sbyte result)
	{
		NumberFormatInfo.ValidateParseStyleInteger(style);
		return TryParse(s, style, NumberFormatInfo.GetInstance(provider), out result);
	}

	private static bool TryParse(string s, NumberStyles style, NumberFormatInfo info, out sbyte result)
	{
		result = 0;
		if (!Number.TryParseInt32(s, style, info, out var result2))
		{
			return false;
		}
		if ((style & NumberStyles.AllowHexSpecifier) != 0)
		{
			if (result2 < 0 || result2 > 255)
			{
				return false;
			}
			result = (sbyte)result2;
			return true;
		}
		if (result2 < -128 || result2 > 127)
		{
			return false;
		}
		result = (sbyte)result2;
		return true;
	}

	/// <summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.SByte" />.</summary>
	/// <returns>The enumerated constant, <see cref="F:System.TypeCode.SByte" />.</returns>
	/// <filterpriority>2</filterpriority>
	public TypeCode GetTypeCode()
	{
		return TypeCode.SByte;
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToBoolean(System.IFormatProvider)" />. </summary>
	/// <returns>true if the value of the current instance is not zero; otherwise, false.</returns>
	/// <param name="provider">This parameter is unused.</param>
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
	/// <returns>The value of the current instance, unchanged.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	sbyte IConvertible.ToSByte(IFormatProvider provider)
	{
		return this;
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToByte(System.IFormatProvider)" />. </summary>
	/// <returns>The value of the current instance, converted to a <see cref="T:System.Byte" />.</returns>
	/// <param name="provider">This parameter is unused.</param>
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
		return this;
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToUInt32(System.IFormatProvider)" />. </summary>
	/// <returns>The value of the current instance, converted to a <see cref="T:System.UInt32" />.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	uint IConvertible.ToUInt32(IFormatProvider provider)
	{
		return Convert.ToUInt32(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToInt64(System.IFormatProvider)" />. </summary>
	/// <returns>The value of the current instance, converted to an <see cref="T:System.Int64" />.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	long IConvertible.ToInt64(IFormatProvider provider)
	{
		return Convert.ToInt64(this);
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
	/// <param name="provider">This parameter is unused.</param>
	decimal IConvertible.ToDecimal(IFormatProvider provider)
	{
		return Convert.ToDecimal(this);
	}

	/// <summary>This conversion is not supported. Attempting to do so throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>None. This conversion is not supported. </returns>
	/// <param name="provider">This parameter is ignored.</param>
	/// <exception cref="T:System.InvalidCastException">In all cases. </exception>
	DateTime IConvertible.ToDateTime(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Invalid cast from '{0}' to '{1}'.", "SByte", "DateTime"));
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToType(System.Type,System.IFormatProvider)" />. </summary>
	/// <returns>The value of the current instance, converted to an object of type <paramref name="type" />.</returns>
	/// <param name="type">The <see cref="T:System.Type" /> to which to convert this <see cref="T:System.SByte" /> value.</param>
	/// <param name="provider">A <see cref="T:System.IFormatProvider" /> implementation that provides information about the format of the returned value.</param>
	object IConvertible.ToType(Type type, IFormatProvider provider)
	{
		return Convert.DefaultToType(this, type, provider);
	}
}
