using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;

namespace System;

/// <summary>Represents a single-precision floating-point number.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
public struct Single : IComparable, IFormattable, IConvertible, IComparable<float>, IEquatable<float>
{
	internal float m_value;

	/// <summary>Represents the smallest possible value of <see cref="T:System.Single" />. This field is constant.</summary>
	/// <filterpriority>1</filterpriority>
	public const float MinValue = -3.4028235E+38f;

	/// <summary>Represents the smallest positive <see cref="T:System.Single" /> value that is greater than zero. This field is constant.</summary>
	/// <filterpriority>1</filterpriority>
	public const float Epsilon = 1E-45f;

	/// <summary>Represents the largest possible value of <see cref="T:System.Single" />. This field is constant.</summary>
	/// <filterpriority>1</filterpriority>
	public const float MaxValue = 3.4028235E+38f;

	/// <summary>Represents positive infinity. This field is constant.</summary>
	/// <filterpriority>1</filterpriority>
	public const float PositiveInfinity = 1f / 0f;

	/// <summary>Represents negative infinity. This field is constant.</summary>
	/// <filterpriority>1</filterpriority>
	public const float NegativeInfinity = -1f / 0f;

	/// <summary>Represents not a number (NaN). This field is constant.</summary>
	/// <filterpriority>1</filterpriority>
	public const float NaN = 0f / 0f;

	/// <summary>Returns a value indicating whether the specified number evaluates to negative or positive infinity.</summary>
	/// <returns>true if <paramref name="f" /> evaluates to <see cref="F:System.Single.PositiveInfinity" /> or <see cref="F:System.Single.NegativeInfinity" />; otherwise, false.</returns>
	/// <param name="f">A single-precision floating-point number. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe static bool IsInfinity(float f)
	{
		return (*(int*)(&f) & 0x7FFFFFFF) == 2139095040;
	}

	/// <summary>Returns a value indicating whether the specified number evaluates to positive infinity.</summary>
	/// <returns>true if <paramref name="f" /> evaluates to <see cref="F:System.Single.PositiveInfinity" />; otherwise, false.</returns>
	/// <param name="f">A single-precision floating-point number. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe static bool IsPositiveInfinity(float f)
	{
		return *(int*)(&f) == 2139095040;
	}

	/// <summary>Returns a value indicating whether the specified number evaluates to negative infinity.</summary>
	/// <returns>true if <paramref name="f" /> evaluates to <see cref="F:System.Single.NegativeInfinity" />; otherwise, false.</returns>
	/// <param name="f">A single-precision floating-point number. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe static bool IsNegativeInfinity(float f)
	{
		return *(int*)(&f) == -8388608;
	}

	/// <summary>Returns a value that indicates whether the specified value is not a number (<see cref="F:System.Single.NaN" />).</summary>
	/// <returns>true if <paramref name="f" /> evaluates to not a number (<see cref="F:System.Single.NaN" />); otherwise, false.</returns>
	/// <param name="f">A single-precision floating-point number. </param>
	/// <filterpriority>1</filterpriority>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	[SecuritySafeCritical]
	public unsafe static bool IsNaN(float f)
	{
		return (*(int*)(&f) & 0x7FFFFFFF) > 2139095040;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static bool IsFinite(float f)
	{
		return (*(int*)(&f) & 0x7FFFFFFF) < 2139095040;
	}

	/// <summary>Compares this instance to a specified object and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the specified object.</summary>
	/// <returns>A signed number indicating the relative values of this instance and <paramref name="value" />.Return Value Description Less than zero This instance is less than <paramref name="value" />.-or- This instance is not a number (<see cref="F:System.Single.NaN" />) and <paramref name="value" /> is a number. Zero This instance is equal to <paramref name="value" />.-or- This instance and value are both not a number (<see cref="F:System.Single.NaN" />), <see cref="F:System.Single.PositiveInfinity" />, or <see cref="F:System.Single.NegativeInfinity" />. Greater than zero This instance is greater than <paramref name="value" />.-or- This instance is a number and <paramref name="value" /> is not a number (<see cref="F:System.Single.NaN" />).-or- <paramref name="value" /> is null. </returns>
	/// <param name="value">An object to compare, or null. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is not a <see cref="T:System.Single" />. </exception>
	/// <filterpriority>2</filterpriority>
	public int CompareTo(object value)
	{
		if (value == null)
		{
			return 1;
		}
		if (value is float num)
		{
			if (this < num)
			{
				return -1;
			}
			if (this > num)
			{
				return 1;
			}
			if (this == num)
			{
				return 0;
			}
			if (IsNaN(this))
			{
				if (!IsNaN(num))
				{
					return -1;
				}
				return 0;
			}
			return 1;
		}
		throw new ArgumentException(Environment.GetResourceString("Object must be of type Single."));
	}

	/// <summary>Compares this instance to a specified single-precision floating-point number and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the specified single-precision floating-point number.</summary>
	/// <returns>A signed number indicating the relative values of this instance and <paramref name="value" />.Return Value Description Less than zero This instance is less than <paramref name="value" />.-or- This instance is not a number (<see cref="F:System.Single.NaN" />) and <paramref name="value" /> is a number. Zero This instance is equal to <paramref name="value" />.-or- Both this instance and <paramref name="value" /> are not a number (<see cref="F:System.Single.NaN" />), <see cref="F:System.Single.PositiveInfinity" />, or <see cref="F:System.Single.NegativeInfinity" />. Greater than zero This instance is greater than <paramref name="value" />.-or- This instance is a number and <paramref name="value" /> is not a number (<see cref="F:System.Single.NaN" />). </returns>
	/// <param name="value">A single-precision floating-point number to compare. </param>
	/// <filterpriority>2</filterpriority>
	public int CompareTo(float value)
	{
		if (this < value)
		{
			return -1;
		}
		if (this > value)
		{
			return 1;
		}
		if (this == value)
		{
			return 0;
		}
		if (IsNaN(this))
		{
			if (!IsNaN(value))
			{
				return -1;
			}
			return 0;
		}
		return 1;
	}

	/// <summary>Returns a value that indicates whether two specified <see cref="T:System.Single" /> values are equal.</summary>
	/// <returns>true if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	public static bool operator ==(float left, float right)
	{
		return left == right;
	}

	/// <summary>Returns a value that indicates whether two specified <see cref="T:System.Single" /> values are not equal.</summary>
	/// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	public static bool operator !=(float left, float right)
	{
		return left != right;
	}

	/// <summary>Returns a value that indicates whether a specified <see cref="T:System.Single" /> value is less than another specified <see cref="T:System.Single" /> value.</summary>
	/// <returns>true if <paramref name="left" /> is less than <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	public static bool operator <(float left, float right)
	{
		return left < right;
	}

	/// <summary>Returns a value that indicates whether a specified <see cref="T:System.Single" /> value is greater than another specified <see cref="T:System.Single" /> value.</summary>
	/// <returns>true if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	public static bool operator >(float left, float right)
	{
		return left > right;
	}

	/// <summary>Returns a value that indicates whether a specified <see cref="T:System.Single" /> value is less than or equal to another specified <see cref="T:System.Single" /> value.</summary>
	/// <returns>true if <paramref name="left" /> is less than or equal to <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	public static bool operator <=(float left, float right)
	{
		return left <= right;
	}

	/// <summary>Returns a value that indicates whether a specified <see cref="T:System.Single" /> value is greater than or equal to another specified <see cref="T:System.Single" /> value.</summary>
	/// <returns>true if <paramref name="left" /> is greater than or equal to <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	public static bool operator >=(float left, float right)
	{
		return left >= right;
	}

	/// <summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	/// <returns>true if <paramref name="obj" /> is an instance of <see cref="T:System.Single" /> and equals the value of this instance; otherwise, false.</returns>
	/// <param name="obj">An object to compare with this instance. </param>
	/// <filterpriority>2</filterpriority>
	public override bool Equals(object obj)
	{
		if (!(obj is float num))
		{
			return false;
		}
		if (num == this)
		{
			return true;
		}
		if (IsNaN(num))
		{
			return IsNaN(this);
		}
		return false;
	}

	/// <summary>Returns a value indicating whether this instance and a specified <see cref="T:System.Single" /> object represent the same value.</summary>
	/// <returns>true if <paramref name="obj" /> is equal to this instance; otherwise, false.</returns>
	/// <param name="obj">An object to compare with this instance.</param>
	/// <filterpriority>2</filterpriority>
	public bool Equals(float obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (IsNaN(obj))
		{
			return IsNaN(this);
		}
		return false;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <filterpriority>2</filterpriority>
	[SecuritySafeCritical]
	public unsafe override int GetHashCode()
	{
		float num = this;
		if (num == 0f)
		{
			return 0;
		}
		return *(int*)(&num);
	}

	/// <summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
	/// <returns>The string representation of the value of this instance.</returns>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public override string ToString()
	{
		return Number.FormatSingle(this, null, NumberFormatInfo.CurrentInfo);
	}

	/// <summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
	/// <returns>The string representation of the value of this instance as specified by <paramref name="provider" />.</returns>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public string ToString(IFormatProvider provider)
	{
		return Number.FormatSingle(this, null, NumberFormatInfo.GetInstance(provider));
	}

	/// <summary>Converts the numeric value of this instance to its equivalent string representation, using the specified format.</summary>
	/// <returns>The string representation of the value of this instance as specified by <paramref name="format" />.</returns>
	/// <param name="format">A numeric format string.</param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is invalid. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public string ToString(string format)
	{
		return Number.FormatSingle(this, format, NumberFormatInfo.CurrentInfo);
	}

	/// <summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
	/// <returns>The string representation of the value of this instance as specified by <paramref name="format" /> and <paramref name="provider" />.</returns>
	/// <param name="format">A numeric format string.</param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public string ToString(string format, IFormatProvider provider)
	{
		return Number.FormatSingle(this, format, NumberFormatInfo.GetInstance(provider));
	}

	/// <summary>Converts the string representation of a number to its single-precision floating-point number equivalent.</summary>
	/// <returns>A single-precision floating-point number equivalent to the numeric value or symbol specified in <paramref name="s" />.</returns>
	/// <param name="s">A string that contains a number to convert. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="s" /> does not represent a number in a valid format. </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="s" /> represents a number less than <see cref="F:System.Single.MinValue" /> or greater than <see cref="F:System.Single.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static float Parse(string s)
	{
		return Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.CurrentInfo);
	}

	/// <summary>Converts the string representation of a number in a specified style to its single-precision floating-point number equivalent.</summary>
	/// <returns>A single-precision floating-point number that is equivalent to the numeric value or symbol specified in <paramref name="s" />.</returns>
	/// <param name="s">A string that contains a number to convert. </param>
	/// <param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Float" /> combined with <see cref="F:System.Globalization.NumberStyles.AllowThousands" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="s" /> is not a number in a valid format. </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="s" /> represents a number that is less than <see cref="F:System.Single.MinValue" /> or greater than <see cref="F:System.Single.MaxValue" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or-<paramref name="style" /> includes the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> value.</exception>
	/// <filterpriority>1</filterpriority>
	public static float Parse(string s, NumberStyles style)
	{
		NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
		return Parse(s, style, NumberFormatInfo.CurrentInfo);
	}

	/// <summary>Converts the string representation of a number in a specified culture-specific format to its single-precision floating-point number equivalent.</summary>
	/// <returns>A single-precision floating-point number equivalent to the numeric value or symbol specified in <paramref name="s" />.</returns>
	/// <param name="s">A string that contains a number to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="s" /> does not represent a number in a valid format. </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="s" /> represents a number less than <see cref="F:System.Single.MinValue" /> or greater than <see cref="F:System.Single.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static float Parse(string s, IFormatProvider provider)
	{
		return Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.GetInstance(provider));
	}

	/// <summary>Converts the string representation of a number in a specified style and culture-specific format to its single-precision floating-point number equivalent.</summary>
	/// <returns>A single-precision floating-point number equivalent to the numeric value or symbol specified in <paramref name="s" />.</returns>
	/// <param name="s">A string that contains a number to convert. </param>
	/// <param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Float" /> combined with <see cref="F:System.Globalization.NumberStyles.AllowThousands" />.</param>
	/// <param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="s" /> does not represent a numeric value. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or-<paramref name="style" /> is the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> value.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="s" /> represents a number that is less than <see cref="F:System.Single.MinValue" /> or greater than <see cref="F:System.Single.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static float Parse(string s, NumberStyles style, IFormatProvider provider)
	{
		NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
		return Parse(s, style, NumberFormatInfo.GetInstance(provider));
	}

	private static float Parse(string s, NumberStyles style, NumberFormatInfo info)
	{
		return Number.ParseSingle(s, style, info);
	}

	/// <summary>Converts the string representation of a number to its single-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	/// <returns>true if <paramref name="s" /> was converted successfully; otherwise, false.</returns>
	/// <param name="s">A string representing a number to convert. </param>
	/// <param name="result">When this method returns, contains single-precision floating-point number equivalent to the numeric value or symbol contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is null, is not a number in a valid format, or represents a number less than <see cref="F:System.Single.MinValue" /> or greater than <see cref="F:System.Single.MaxValue" />. This parameter is passed uninitialized. </param>
	/// <filterpriority>1</filterpriority>
	public static bool TryParse(string s, out float result)
	{
		return TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.CurrentInfo, out result);
	}

	/// <summary>Converts the string representation of a number in a specified style and culture-specific format to its single-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	/// <returns>true if <paramref name="s" /> was converted successfully; otherwise, false.</returns>
	/// <param name="s">A string representing a number to convert. </param>
	/// <param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Float" /> combined with <see cref="F:System.Globalization.NumberStyles.AllowThousands" />.</param>
	/// <param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />. </param>
	/// <param name="result">When this method returns, contains the single-precision floating-point number equivalent to the numeric value or symbol contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is null, is not in a format compliant with <paramref name="style" />, represents a number less than <see cref="F:System.Single.MinValue" /> or greater than <see cref="F:System.Single.MaxValue" />, or if <paramref name="style" /> is not a valid combination of <see cref="T:System.Globalization.NumberStyles" /> enumerated constants. This parameter is passed uninitialized. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or-<paramref name="style" /> is the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> value.</exception>
	/// <filterpriority>1</filterpriority>
	public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out float result)
	{
		NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
		return TryParse(s, style, NumberFormatInfo.GetInstance(provider), out result);
	}

	private static bool TryParse(string s, NumberStyles style, NumberFormatInfo info, out float result)
	{
		if (s == null)
		{
			result = 0f;
			return false;
		}
		if (!Number.TryParseSingle(s, style, info, out result))
		{
			string text = s.Trim();
			if (text.Equals(info.PositiveInfinitySymbol))
			{
				result = float.PositiveInfinity;
			}
			else if (text.Equals(info.NegativeInfinitySymbol))
			{
				result = float.NegativeInfinity;
			}
			else
			{
				if (!text.Equals(info.NaNSymbol))
				{
					return false;
				}
				result = float.NaN;
			}
		}
		return true;
	}

	/// <summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.Single" />.</summary>
	/// <returns>The enumerated constant, <see cref="F:System.TypeCode.Single" />.</returns>
	/// <filterpriority>2</filterpriority>
	public TypeCode GetTypeCode()
	{
		return TypeCode.Single;
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToBoolean(System.IFormatProvider)" />. </summary>
	/// <returns>true if the value of the current instance is not zero; otherwise, false.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	bool IConvertible.ToBoolean(IFormatProvider provider)
	{
		return Convert.ToBoolean(this);
	}

	/// <summary>This conversion is not supported. Attempting to use this method throws an <see cref="T:System.InvalidCastException" />. </summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	/// <exception cref="T:System.InvalidCastException">In all cases.</exception>
	char IConvertible.ToChar(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Invalid cast from '{0}' to '{1}'.", "Single", "Char"));
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

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToUInt16(System.IFormatProvider)" />.</summary>
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
	/// <returns>The value of the current instance, unchanged.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	float IConvertible.ToSingle(IFormatProvider provider)
	{
		return this;
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

	/// <summary>This conversion is not supported. Attempting to use this method throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	/// <exception cref="T:System.InvalidCastException">In all cases.</exception>
	DateTime IConvertible.ToDateTime(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Invalid cast from '{0}' to '{1}'.", "Single", "DateTime"));
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToType(System.Type,System.IFormatProvider)" />. </summary>
	/// <returns>The value of the current instance, converted to <paramref name="type" />.</returns>
	/// <param name="type">The type to which to convert this <see cref="T:System.Single" /> value.</param>
	/// <param name="provider">An object that supplies information about the format of the returned value.</param>
	object IConvertible.ToType(Type type, IFormatProvider provider)
	{
		return Convert.DefaultToType(this, type, provider);
	}
}
