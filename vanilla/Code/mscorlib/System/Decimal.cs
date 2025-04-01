using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace System;

/// <summary>Represents a decimal number.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
public struct Decimal : IFormattable, IComparable, IConvertible, IDeserializationCallback, IComparable<decimal>, IEquatable<decimal>
{
	private const int SignMask = int.MinValue;

	private const byte DECIMAL_NEG = 128;

	private const byte DECIMAL_ADD = 0;

	private const int ScaleMask = 16711680;

	private const int ScaleShift = 16;

	private const int MaxInt32Scale = 9;

	private static uint[] Powers10 = new uint[10] { 1u, 10u, 100u, 1000u, 10000u, 100000u, 1000000u, 10000000u, 100000000u, 1000000000u };

	/// <summary>Represents the number zero (0).</summary>
	/// <filterpriority>1</filterpriority>
	public const decimal Zero = 0m;

	/// <summary>Represents the number one (1).</summary>
	/// <filterpriority>1</filterpriority>
	public const decimal One = 1m;

	/// <summary>Represents the number negative one (-1).</summary>
	/// <filterpriority>1</filterpriority>
	public const decimal MinusOne = -1m;

	/// <summary>Represents the largest possible value of <see cref="T:System.Decimal" />. This field is constant and read-only.</summary>
	/// <filterpriority>1</filterpriority>
	public const decimal MaxValue = 79228162514264337593543950335m;

	/// <summary>Represents the smallest possible value of <see cref="T:System.Decimal" />. This field is constant and read-only.</summary>
	/// <filterpriority>1</filterpriority>
	public const decimal MinValue = -79228162514264337593543950335m;

	private const decimal NearNegativeZero = -0.000000000000000000000000001m;

	private const decimal NearPositiveZero = 0.000000000000000000000000001m;

	private int flags;

	private int hi;

	private int lo;

	private int mid;

	/// <summary>Initializes a new instance of <see cref="T:System.Decimal" /> to the value of the specified 32-bit signed integer.</summary>
	/// <param name="value">The value to represent as a <see cref="T:System.Decimal" />. </param>
	public Decimal(int value)
	{
		int num = value;
		if (num >= 0)
		{
			flags = 0;
		}
		else
		{
			flags = int.MinValue;
			num = -num;
		}
		lo = num;
		mid = 0;
		hi = 0;
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Decimal" /> to the value of the specified 32-bit unsigned integer.</summary>
	/// <param name="value">The value to represent as a <see cref="T:System.Decimal" />. </param>
	[CLSCompliant(false)]
	public Decimal(uint value)
	{
		flags = 0;
		lo = (int)value;
		mid = 0;
		hi = 0;
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Decimal" /> to the value of the specified 64-bit signed integer.</summary>
	/// <param name="value">The value to represent as a <see cref="T:System.Decimal" />. </param>
	public Decimal(long value)
	{
		long num = value;
		if (num >= 0)
		{
			flags = 0;
		}
		else
		{
			flags = int.MinValue;
			num = -num;
		}
		lo = (int)num;
		mid = (int)(num >> 32);
		hi = 0;
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Decimal" /> to the value of the specified 64-bit unsigned integer.</summary>
	/// <param name="value">The value to represent as a <see cref="T:System.Decimal" />. </param>
	[CLSCompliant(false)]
	public Decimal(ulong value)
	{
		flags = 0;
		lo = (int)value;
		mid = (int)(value >> 32);
		hi = 0;
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Decimal" /> to the value of the specified single-precision floating-point number.</summary>
	/// <param name="value">The value to represent as a <see cref="T:System.Decimal" />. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Decimal.MaxValue" /> or less than <see cref="F:System.Decimal.MinValue" />.-or- <paramref name="value" /> is <see cref="F:System.Single.NaN" />, <see cref="F:System.Single.PositiveInfinity" />, or <see cref="F:System.Single.NegativeInfinity" />. </exception>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecuritySafeCritical]
	public extern Decimal(float value);

	/// <summary>Initializes a new instance of <see cref="T:System.Decimal" /> to the value of the specified double-precision floating-point number.</summary>
	/// <param name="value">The value to represent as a <see cref="T:System.Decimal" />. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Decimal.MaxValue" /> or less than <see cref="F:System.Decimal.MinValue" />.-or- <paramref name="value" /> is <see cref="F:System.Double.NaN" />, <see cref="F:System.Double.PositiveInfinity" />, or <see cref="F:System.Double.NegativeInfinity" />. </exception>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecuritySafeCritical]
	public extern Decimal(double value);

	/// <summary>Converts the specified <see cref="T:System.Decimal" /> value to the equivalent OLE Automation Currency value, which is contained in a 64-bit signed integer.</summary>
	/// <returns>A 64-bit signed integer that contains the OLE Automation equivalent of <paramref name="value" />.</returns>
	/// <param name="value">The decimal number to convert. </param>
	/// <filterpriority>2</filterpriority>
	public static long ToOACurrency(decimal value)
	{
		return (long)(value * 10000m);
	}

	/// <summary>Converts the specified 64-bit signed integer, which contains an OLE Automation Currency value, to the equivalent <see cref="T:System.Decimal" /> value.</summary>
	/// <returns>A <see cref="T:System.Decimal" /> that contains the equivalent of <paramref name="cy" />.</returns>
	/// <param name="cy">An OLE Automation Currency value. </param>
	/// <filterpriority>1</filterpriority>
	public static decimal FromOACurrency(long cy)
	{
		return (decimal)cy / 10000m;
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Decimal" /> to a decimal value represented in binary and contained in a specified array.</summary>
	/// <param name="bits">An array of 32-bit signed integers containing a representation of a decimal value. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="bits" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">The length of the <paramref name="bits" /> is not 4.-or- The representation of the decimal value in <paramref name="bits" /> is not valid. </exception>
	public Decimal(int[] bits)
	{
		if (bits == null)
		{
			throw new ArgumentNullException("bits");
		}
		if (bits.Length == 4)
		{
			int num = bits[3];
			if ((num & 0x7F00FFFF) == 0 && (num & 0xFF0000) <= 1835008)
			{
				lo = bits[0];
				mid = bits[1];
				hi = bits[2];
				flags = num;
				return;
			}
		}
		throw new ArgumentException(Environment.GetResourceString("Decimal byte array constructor requires an array of length four containing valid decimal bytes."));
	}

	private void SetBits(int[] bits)
	{
		if (bits == null)
		{
			throw new ArgumentNullException("bits");
		}
		if (bits.Length == 4)
		{
			int num = bits[3];
			if ((num & 0x7F00FFFF) == 0 && (num & 0xFF0000) <= 1835008)
			{
				lo = bits[0];
				mid = bits[1];
				hi = bits[2];
				flags = num;
				return;
			}
		}
		throw new ArgumentException(Environment.GetResourceString("Decimal byte array constructor requires an array of length four containing valid decimal bytes."));
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Decimal" /> from parameters specifying the instance's constituent parts.</summary>
	/// <param name="lo">The low 32 bits of a 96-bit integer. </param>
	/// <param name="mid">The middle 32 bits of a 96-bit integer. </param>
	/// <param name="hi">The high 32 bits of a 96-bit integer. </param>
	/// <param name="isNegative">true to indicate a negative number; false to indicate a positive number. </param>
	/// <param name="scale">A power of 10 ranging from 0 to 28. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="scale" /> is greater than 28. </exception>
	public Decimal(int lo, int mid, int hi, bool isNegative, byte scale)
	{
		if (scale > 28)
		{
			throw new ArgumentOutOfRangeException("scale", Environment.GetResourceString("Decimal's scale value must be between 0 and 28, inclusive."));
		}
		this.lo = lo;
		this.mid = mid;
		this.hi = hi;
		flags = scale << 16;
		if (isNegative)
		{
			flags |= int.MinValue;
		}
	}

	[OnSerializing]
	private void OnSerializing(StreamingContext ctx)
	{
		try
		{
			SetBits(GetBits(this));
		}
		catch (ArgumentException innerException)
		{
			throw new SerializationException(Environment.GetResourceString("Value was either too large or too small for a Decimal."), innerException);
		}
	}

	/// <summary>Runs when the deserialization of an object has been completed.</summary>
	/// <param name="sender">The object that initiated the callback. The functionality for this parameter is not currently implemented.</param>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">The <see cref="T:System.Decimal" /> object contains invalid or corrupted data.</exception>
	void IDeserializationCallback.OnDeserialization(object sender)
	{
		try
		{
			SetBits(GetBits(this));
		}
		catch (ArgumentException innerException)
		{
			throw new SerializationException(Environment.GetResourceString("Value was either too large or too small for a Decimal."), innerException);
		}
	}

	private Decimal(int lo, int mid, int hi, int flags)
	{
		if ((flags & 0x7F00FFFF) == 0 && (flags & 0xFF0000) <= 1835008)
		{
			this.lo = lo;
			this.mid = mid;
			this.hi = hi;
			this.flags = flags;
			return;
		}
		throw new ArgumentException(Environment.GetResourceString("Decimal byte array constructor requires an array of length four containing valid decimal bytes."));
	}

	internal static decimal Abs(decimal d)
	{
		return new decimal(d.lo, d.mid, d.hi, d.flags & 0x7FFFFFFF);
	}

	/// <summary>Adds two specified <see cref="T:System.Decimal" /> values.</summary>
	/// <returns>The sum of <paramref name="d1" /> and <paramref name="d2" />.</returns>
	/// <param name="d1">The first value to add. </param>
	/// <param name="d2">The second value to add. </param>
	/// <exception cref="T:System.OverflowException">The sum of <paramref name="d1" /> and <paramref name="d2" /> is less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static decimal Add(decimal d1, decimal d2)
	{
		FCallAddSub(ref d1, ref d2, 0);
		return d1;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private static extern void FCallAddSub(ref decimal d1, ref decimal d2, byte bSign);

	/// <summary>Returns the smallest integral value that is greater than or equal to the specified decimal number.</summary>
	/// <returns>The smallest integral value that is greater than or equal to the <paramref name="d" /> parameter. Note that this method returns a <see cref="T:System.Decimal" /> instead of an integral type.</returns>
	/// <param name="d">A decimal number. </param>
	/// <filterpriority>1</filterpriority>
	public static decimal Ceiling(decimal d)
	{
		return -Floor(-d);
	}

	/// <summary>Compares two specified <see cref="T:System.Decimal" /> values.</summary>
	/// <returns>A signed number indicating the relative values of <paramref name="d1" /> and <paramref name="d2" />.Return value Meaning Less than zero <paramref name="d1" /> is less than <paramref name="d2" />. Zero <paramref name="d1" /> and <paramref name="d2" /> are equal. Greater than zero <paramref name="d1" /> is greater than <paramref name="d2" />. </returns>
	/// <param name="d1">The first value to compare. </param>
	/// <param name="d2">The second value to compare. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static int Compare(decimal d1, decimal d2)
	{
		return FCallCompare(ref d1, ref d2);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	private static extern int FCallCompare(ref decimal d1, ref decimal d2);

	/// <summary>Compares this instance to a specified object and returns a comparison of their relative values.</summary>
	/// <returns>A signed number indicating the relative values of this instance and <paramref name="value" />.Return value Meaning Less than zero This instance is less than <paramref name="value" />. Zero This instance is equal to <paramref name="value" />. Greater than zero This instance is greater than <paramref name="value" />.-or- <paramref name="value" /> is null. </returns>
	/// <param name="value">The object to compare with this instance, or null. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is not a <see cref="T:System.Decimal" />. </exception>
	/// <filterpriority>2</filterpriority>
	[SecuritySafeCritical]
	public int CompareTo(object value)
	{
		if (value == null)
		{
			return 1;
		}
		if (!(value is decimal d))
		{
			throw new ArgumentException(Environment.GetResourceString("Object must be of type Decimal."));
		}
		return FCallCompare(ref this, ref d);
	}

	/// <summary>Compares this instance to a specified <see cref="T:System.Decimal" /> object and returns a comparison of their relative values.</summary>
	/// <returns>A signed number indicating the relative values of this instance and <paramref name="value" />.Return value Meaning Less than zero This instance is less than <paramref name="value" />. Zero This instance is equal to <paramref name="value" />. Greater than zero This instance is greater than <paramref name="value" />. </returns>
	/// <param name="value">The object to compare with this instance.</param>
	/// <filterpriority>2</filterpriority>
	[SecuritySafeCritical]
	public int CompareTo(decimal value)
	{
		return FCallCompare(ref this, ref value);
	}

	/// <summary>Divides two specified <see cref="T:System.Decimal" /> values.</summary>
	/// <returns>The result of dividing <paramref name="d1" /> by <paramref name="d2" />.</returns>
	/// <param name="d1">The dividend. </param>
	/// <param name="d2">The divisor. </param>
	/// <exception cref="T:System.DivideByZeroException">
	///   <paramref name="d2" /> is zero. </exception>
	/// <exception cref="T:System.OverflowException">The return value (that is, the quotient) is less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static decimal Divide(decimal d1, decimal d2)
	{
		FCallDivide(ref d1, ref d2);
		return d1;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private static extern void FCallDivide(ref decimal d1, ref decimal d2);

	/// <summary>Returns a value indicating whether this instance and a specified <see cref="T:System.Object" /> represent the same type and value.</summary>
	/// <returns>true if <paramref name="value" /> is a <see cref="T:System.Decimal" /> and equal to this instance; otherwise, false.</returns>
	/// <param name="value">The object to compare with this instance. </param>
	/// <filterpriority>2</filterpriority>
	[SecuritySafeCritical]
	public override bool Equals(object value)
	{
		if (value is decimal d)
		{
			return FCallCompare(ref this, ref d) == 0;
		}
		return false;
	}

	/// <summary>Returns a value indicating whether this instance and a specified <see cref="T:System.Decimal" /> object represent the same value.</summary>
	/// <returns>true if <paramref name="value" /> is equal to this instance; otherwise, false.</returns>
	/// <param name="value">An object to compare to this instance.</param>
	/// <filterpriority>2</filterpriority>
	[SecuritySafeCritical]
	public bool Equals(decimal value)
	{
		return FCallCompare(ref this, ref value) == 0;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <filterpriority>2</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecuritySafeCritical]
	public override extern int GetHashCode();

	/// <summary>Returns a value indicating whether two specified instances of <see cref="T:System.Decimal" /> represent the same value.</summary>
	/// <returns>true if <paramref name="d1" /> and <paramref name="d2" /> are equal; otherwise, false.</returns>
	/// <param name="d1">The first value to compare. </param>
	/// <param name="d2">The second value to compare. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static bool Equals(decimal d1, decimal d2)
	{
		return FCallCompare(ref d1, ref d2) == 0;
	}

	/// <summary>Rounds a specified <see cref="T:System.Decimal" /> number to the closest integer toward negative infinity.</summary>
	/// <returns>If <paramref name="d" /> has a fractional part, the next whole <see cref="T:System.Decimal" /> number toward negative infinity that is less than <paramref name="d" />.-or- If <paramref name="d" /> doesn't have a fractional part, <paramref name="d" /> is returned unchanged. Note that the method returns an integral value of type <see cref="T:System.Decimal" />. </returns>
	/// <param name="d">The value to round. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static decimal Floor(decimal d)
	{
		FCallFloor(ref d);
		return d;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private static extern void FCallFloor(ref decimal d);

	/// <summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
	/// <returns>A string that represents the value of this instance.</returns>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public override string ToString()
	{
		return Number.FormatDecimal(this, null, NumberFormatInfo.CurrentInfo);
	}

	/// <summary>Converts the numeric value of this instance to its equivalent string representation, using the specified format.</summary>
	/// <returns>The string representation of the value of this instance as specified by <paramref name="format" />.</returns>
	/// <param name="format">A standard or custom numeric format string (see Remarks).</param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is invalid. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public string ToString(string format)
	{
		return Number.FormatDecimal(this, format, NumberFormatInfo.CurrentInfo);
	}

	/// <summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
	/// <returns>The string representation of the value of this instance as specified by <paramref name="provider" />.</returns>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public string ToString(IFormatProvider provider)
	{
		return Number.FormatDecimal(this, null, NumberFormatInfo.GetInstance(provider));
	}

	/// <summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
	/// <returns>The string representation of the value of this instance as specified by <paramref name="format" /> and <paramref name="provider" />.</returns>
	/// <param name="format">A numeric format string (see Remarks).</param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is invalid. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public string ToString(string format, IFormatProvider provider)
	{
		return Number.FormatDecimal(this, format, NumberFormatInfo.GetInstance(provider));
	}

	/// <summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent.</summary>
	/// <returns>The equivalent to the number contained in <paramref name="s" />.</returns>
	/// <param name="s">The string representation of the number to convert.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="s" /> is not in the correct format. </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="s" /> represents a number less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static decimal Parse(string s)
	{
		return Number.ParseDecimal(s, NumberStyles.Number, NumberFormatInfo.CurrentInfo);
	}

	/// <summary>Converts the string representation of a number in a specified style to its <see cref="T:System.Decimal" /> equivalent.</summary>
	/// <returns>The <see cref="T:System.Decimal" /> number equivalent to the number contained in <paramref name="s" /> as specified by <paramref name="style" />.</returns>
	/// <param name="s">The string representation of the number to convert. </param>
	/// <param name="style">A bitwise combination of <see cref="T:System.Globalization.NumberStyles" /> values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Number" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or-<paramref name="style" /> is the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> value.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="s" /> is not in the correct format. </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="s" /> represents a number less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" /></exception>
	/// <filterpriority>1</filterpriority>
	public static decimal Parse(string s, NumberStyles style)
	{
		NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
		return Number.ParseDecimal(s, style, NumberFormatInfo.CurrentInfo);
	}

	/// <summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified culture-specific format information.</summary>
	/// <returns>The <see cref="T:System.Decimal" /> number equivalent to the number contained in <paramref name="s" /> as specified by <paramref name="provider" />.</returns>
	/// <param name="s">The string representation of the number to convert. </param>
	/// <param name="provider">An <see cref="T:System.IFormatProvider" /> that supplies culture-specific parsing information about <paramref name="s" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="s" /> is not of the correct format </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="s" /> represents a number less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" /></exception>
	/// <filterpriority>1</filterpriority>
	public static decimal Parse(string s, IFormatProvider provider)
	{
		return Number.ParseDecimal(s, NumberStyles.Number, NumberFormatInfo.GetInstance(provider));
	}

	/// <summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified style and culture-specific format.</summary>
	/// <returns>The <see cref="T:System.Decimal" /> number equivalent to the number contained in <paramref name="s" /> as specified by <paramref name="style" /> and <paramref name="provider" />.</returns>
	/// <param name="s">The string representation of the number to convert. </param>
	/// <param name="style">A bitwise combination of <see cref="T:System.Globalization.NumberStyles" /> values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Number" />.</param>
	/// <param name="provider">An <see cref="T:System.IFormatProvider" /> object that supplies culture-specific information about the format of <paramref name="s" />. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="s" /> is not in the correct format. </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="s" /> represents a number less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or-<paramref name="style" /> is the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> value.</exception>
	/// <filterpriority>1</filterpriority>
	public static decimal Parse(string s, NumberStyles style, IFormatProvider provider)
	{
		NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
		return Number.ParseDecimal(s, style, NumberFormatInfo.GetInstance(provider));
	}

	/// <summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	/// <returns>true if <paramref name="s" /> was converted successfully; otherwise, false.</returns>
	/// <param name="s">The string representation of the number to convert.</param>
	/// <param name="result">When this method returns, contains the <see cref="T:System.Decimal" /> number that is equivalent to the numeric value contained in <paramref name="s" />, if the conversion succeeded, or is zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is null, is not a number in a valid format, or represents a number less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" />. This parameter is passed uninitialized. </param>
	/// <filterpriority>1</filterpriority>
	public static bool TryParse(string s, out decimal result)
	{
		return Number.TryParseDecimal(s, NumberStyles.Number, NumberFormatInfo.CurrentInfo, out result);
	}

	/// <summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified style and culture-specific format. A return value indicates whether the conversion succeeded or failed.</summary>
	/// <returns>true if <paramref name="s" /> was converted successfully; otherwise, false.</returns>
	/// <param name="s">The string representation of the number to convert.</param>
	/// <param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Number" />.</param>
	/// <param name="provider">An object that supplies culture-specific parsing information about <paramref name="s" />. </param>
	/// <param name="result">When this method returns, contains the <see cref="T:System.Decimal" /> number that is equivalent to the numeric value contained in <paramref name="s" />, if the conversion succeeded, or is zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is null, is not in a format compliant with <paramref name="style" />, or represents a number less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" />. This parameter is passed uninitialized. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or-<paramref name="style" /> is the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> value.</exception>
	/// <filterpriority>1</filterpriority>
	public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out decimal result)
	{
		NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
		return Number.TryParseDecimal(s, style, NumberFormatInfo.GetInstance(provider), out result);
	}

	/// <summary>Converts the value of a specified instance of <see cref="T:System.Decimal" /> to its equivalent binary representation.</summary>
	/// <returns>A 32-bit signed integer array with four elements that contain the binary representation of <paramref name="d" />.</returns>
	/// <param name="d">The value to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static int[] GetBits(decimal d)
	{
		return new int[4] { d.lo, d.mid, d.hi, d.flags };
	}

	internal static void GetBytes(decimal d, byte[] buffer)
	{
		buffer[0] = (byte)d.lo;
		buffer[1] = (byte)(d.lo >> 8);
		buffer[2] = (byte)(d.lo >> 16);
		buffer[3] = (byte)(d.lo >> 24);
		buffer[4] = (byte)d.mid;
		buffer[5] = (byte)(d.mid >> 8);
		buffer[6] = (byte)(d.mid >> 16);
		buffer[7] = (byte)(d.mid >> 24);
		buffer[8] = (byte)d.hi;
		buffer[9] = (byte)(d.hi >> 8);
		buffer[10] = (byte)(d.hi >> 16);
		buffer[11] = (byte)(d.hi >> 24);
		buffer[12] = (byte)d.flags;
		buffer[13] = (byte)(d.flags >> 8);
		buffer[14] = (byte)(d.flags >> 16);
		buffer[15] = (byte)(d.flags >> 24);
	}

	internal static decimal ToDecimal(byte[] buffer)
	{
		int num = buffer[0] | (buffer[1] << 8) | (buffer[2] << 16) | (buffer[3] << 24);
		int num2 = buffer[4] | (buffer[5] << 8) | (buffer[6] << 16) | (buffer[7] << 24);
		int num3 = buffer[8] | (buffer[9] << 8) | (buffer[10] << 16) | (buffer[11] << 24);
		int num4 = buffer[12] | (buffer[13] << 8) | (buffer[14] << 16) | (buffer[15] << 24);
		return new decimal(num, num2, num3, num4);
	}

	private static void InternalAddUInt32RawUnchecked(ref decimal value, uint i)
	{
		uint num = (uint)value.lo;
		uint num2 = (uint)(value.lo = (int)(num + i));
		if (num2 < num || num2 < i)
		{
			num = (uint)value.mid;
			num2 = (uint)(value.mid = (int)(num + 1));
			if (num2 < num || num2 < 1)
			{
				value.hi++;
			}
		}
	}

	private static uint InternalDivRemUInt32(ref decimal value, uint divisor)
	{
		uint num = 0u;
		if (value.hi != 0)
		{
			ulong num2 = (uint)value.hi;
			value.hi = (int)(num2 / divisor);
			num = (uint)(num2 % divisor);
		}
		if (value.mid != 0 || num != 0)
		{
			ulong num2 = ((ulong)num << 32) | (uint)value.mid;
			value.mid = (int)(num2 / divisor);
			num = (uint)(num2 % divisor);
		}
		if (value.lo != 0 || num != 0)
		{
			ulong num2 = ((ulong)num << 32) | (uint)value.lo;
			value.lo = (int)(num2 / divisor);
			num = (uint)(num2 % divisor);
		}
		return num;
	}

	private static void InternalRoundFromZero(ref decimal d, int decimalCount)
	{
		int num = ((d.flags & 0xFF0000) >> 16) - decimalCount;
		if (num > 0)
		{
			uint num3;
			uint num4;
			do
			{
				int num2 = ((num > 9) ? 9 : num);
				num3 = Powers10[num2];
				num4 = InternalDivRemUInt32(ref d, num3);
				num -= num2;
			}
			while (num > 0);
			if (num4 >= num3 >> 1)
			{
				InternalAddUInt32RawUnchecked(ref d, 1u);
			}
			d.flags = ((decimalCount << 16) & 0xFF0000) | (d.flags & int.MinValue);
		}
	}

	[SecuritySafeCritical]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	internal static decimal Max(decimal d1, decimal d2)
	{
		if (FCallCompare(ref d1, ref d2) < 0)
		{
			return d2;
		}
		return d1;
	}

	[SecuritySafeCritical]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	internal static decimal Min(decimal d1, decimal d2)
	{
		if (FCallCompare(ref d1, ref d2) >= 0)
		{
			return d2;
		}
		return d1;
	}

	/// <summary>Computes the remainder after dividing two <see cref="T:System.Decimal" /> values.</summary>
	/// <returns>The remainder after dividing <paramref name="d1" /> by <paramref name="d2" />.</returns>
	/// <param name="d1">The dividend. </param>
	/// <param name="d2">The divisor. </param>
	/// <exception cref="T:System.DivideByZeroException">
	///   <paramref name="d2" /> is zero. </exception>
	/// <exception cref="T:System.OverflowException">The return value is less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static decimal Remainder(decimal d1, decimal d2)
	{
		d2.flags = (d2.flags & 0x7FFFFFFF) | (d1.flags & int.MinValue);
		if (Abs(d1) < Abs(d2))
		{
			return d1;
		}
		d1 -= d2;
		if (d1 == 0m)
		{
			d1.flags = (d1.flags & 0x7FFFFFFF) | (d2.flags & int.MinValue);
		}
		decimal num = Truncate(d1 / d2) * d2;
		decimal num2 = d1 - num;
		if ((d1.flags & int.MinValue) != (num2.flags & int.MinValue))
		{
			if (-0.000000000000000000000000001m <= num2 && num2 <= 0.000000000000000000000000001m)
			{
				num2.flags = (num2.flags & 0x7FFFFFFF) | (d1.flags & int.MinValue);
			}
			else
			{
				num2 += d2;
			}
		}
		return num2;
	}

	/// <summary>Multiplies two specified <see cref="T:System.Decimal" /> values.</summary>
	/// <returns>The result of multiplying <paramref name="d1" /> and <paramref name="d2" />.</returns>
	/// <param name="d1">The multiplicand. </param>
	/// <param name="d2">The multiplier. </param>
	/// <exception cref="T:System.OverflowException">The return value is less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static decimal Multiply(decimal d1, decimal d2)
	{
		FCallMultiply(ref d1, ref d2);
		return d1;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private static extern void FCallMultiply(ref decimal d1, ref decimal d2);

	/// <summary>Returns the result of multiplying the specified <see cref="T:System.Decimal" /> value by negative one.</summary>
	/// <returns>A decimal number with the value of <paramref name="d" />, but the opposite sign.-or- Zero, if <paramref name="d" /> is zero.</returns>
	/// <param name="d">The value to negate. </param>
	/// <filterpriority>1</filterpriority>
	public static decimal Negate(decimal d)
	{
		return new decimal(d.lo, d.mid, d.hi, d.flags ^ int.MinValue);
	}

	/// <summary>Rounds a decimal value to the nearest integer.</summary>
	/// <returns>The integer that is nearest to the <paramref name="d" /> parameter. If <paramref name="d" /> is halfway between two integers, one of which is even and the other odd, the even number is returned.</returns>
	/// <param name="d">A decimal number to round. </param>
	/// <exception cref="T:System.OverflowException">The result is outside the range of a <see cref="T:System.Decimal" /> object.</exception>
	/// <filterpriority>1</filterpriority>
	public static decimal Round(decimal d)
	{
		return Round(d, 0);
	}

	/// <summary>Rounds a <see cref="T:System.Decimal" /> value to a specified number of decimal places.</summary>
	/// <returns>The decimal number equivalent to <paramref name="d" /> rounded to <paramref name="decimals" /> number of decimal places.</returns>
	/// <param name="d">A decimal number to round. </param>
	/// <param name="decimals">A value from 0 to 28 that specifies the number of decimal places to round to. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="decimals" /> is not a value from 0 to 28. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static decimal Round(decimal d, int decimals)
	{
		FCallRound(ref d, decimals);
		return d;
	}

	/// <summary>Rounds a decimal value to the nearest integer. A parameter specifies how to round the value if it is midway between two other numbers.</summary>
	/// <returns>The integer that is nearest to the <paramref name="d" /> parameter. If <paramref name="d" /> is halfway between two numbers, one of which is even and the other odd, the <paramref name="mode" /> parameter determines which of the two numbers is returned. </returns>
	/// <param name="d">A decimal number to round. </param>
	/// <param name="mode">A value that specifies how to round <paramref name="d" /> if it is midway between two other numbers.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="mode" /> is not a <see cref="T:System.MidpointRounding" /> value.</exception>
	/// <exception cref="T:System.OverflowException">The result is outside the range of a <see cref="T:System.Decimal" /> object.</exception>
	/// <filterpriority>1</filterpriority>
	public static decimal Round(decimal d, MidpointRounding mode)
	{
		return Round(d, 0, mode);
	}

	/// <summary>Rounds a decimal value to a specified precision. A parameter specifies how to round the value if it is midway between two other numbers.</summary>
	/// <returns>The number that is nearest to the <paramref name="d" /> parameter with a precision equal to the <paramref name="decimals" /> parameter. If <paramref name="d" /> is halfway between two numbers, one of which is even and the other odd, the <paramref name="mode" /> parameter determines which of the two numbers is returned. If the precision of <paramref name="d" /> is less than <paramref name="decimals" />, <paramref name="d" /> is returned unchanged.</returns>
	/// <param name="d">A decimal number to round. </param>
	/// <param name="decimals">The number of significant decimal places (precision) in the return value. </param>
	/// <param name="mode">A value that specifies how to round <paramref name="d" /> if it is midway between two other numbers.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="decimals" /> is less than 0 or greater than 28. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="mode" /> is not a <see cref="T:System.MidpointRounding" /> value.</exception>
	/// <exception cref="T:System.OverflowException">The result is outside the range of a <see cref="T:System.Decimal" /> object.</exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static decimal Round(decimal d, int decimals, MidpointRounding mode)
	{
		if (decimals < 0 || decimals > 28)
		{
			throw new ArgumentOutOfRangeException("decimals", Environment.GetResourceString("Decimal can only round to between 0 and 28 digits of precision."));
		}
		switch (mode)
		{
		default:
			throw new ArgumentException(Environment.GetResourceString("The value '{0}' is not valid for this usage of the type {1}.", mode, "MidpointRounding"), "mode");
		case MidpointRounding.ToEven:
			FCallRound(ref d, decimals);
			break;
		case MidpointRounding.AwayFromZero:
			InternalRoundFromZero(ref d, decimals);
			break;
		}
		return d;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private static extern void FCallRound(ref decimal d, int decimals);

	/// <summary>Subtracts one specified <see cref="T:System.Decimal" /> value from another.</summary>
	/// <returns>The result of subtracting <paramref name="d2" /> from <paramref name="d1" />.</returns>
	/// <param name="d1">The minuend. </param>
	/// <param name="d2">The subtrahend. </param>
	/// <exception cref="T:System.OverflowException">The return value is less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static decimal Subtract(decimal d1, decimal d2)
	{
		FCallAddSub(ref d1, ref d2, 128);
		return d1;
	}

	/// <summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 8-bit unsigned integer.</summary>
	/// <returns>An 8-bit unsigned integer equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The decimal number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.Byte.MinValue" /> or greater than <see cref="F:System.Byte.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static byte ToByte(decimal value)
	{
		uint num;
		try
		{
			num = ToUInt32(value);
		}
		catch (OverflowException innerException)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for an unsigned byte."), innerException);
		}
		if (num < 0 || num > 255)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for an unsigned byte."));
		}
		return (byte)num;
	}

	/// <summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 8-bit signed integer.</summary>
	/// <returns>An 8-bit signed integer equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The decimal number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.SByte.MinValue" /> or greater than <see cref="F:System.SByte.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static sbyte ToSByte(decimal value)
	{
		int num;
		try
		{
			num = ToInt32(value);
		}
		catch (OverflowException innerException)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a signed byte."), innerException);
		}
		if (num < -128 || num > 127)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a signed byte."));
		}
		return (sbyte)num;
	}

	/// <summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 16-bit signed integer.</summary>
	/// <returns>A 16-bit signed integer equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The decimal number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.Int16.MinValue" /> or greater than <see cref="F:System.Int16.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static short ToInt16(decimal value)
	{
		int num;
		try
		{
			num = ToInt32(value);
		}
		catch (OverflowException innerException)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for an Int16."), innerException);
		}
		if (num < -32768 || num > 32767)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for an Int16."));
		}
		return (short)num;
	}

	/// <summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent double-precision floating-point number.</summary>
	/// <returns>A double-precision floating-point number equivalent to <paramref name="d" />.</returns>
	/// <param name="d">The decimal number to convert. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecuritySafeCritical]
	public static extern double ToDouble(decimal d);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	internal static extern int FCallToInt32(decimal d);

	/// <summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 32-bit signed integer.</summary>
	/// <returns>A 32-bit signed integer equivalent to the value of <paramref name="d" />.</returns>
	/// <param name="d">The decimal number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="d" /> is less than <see cref="F:System.Int32.MinValue" /> or greater than <see cref="F:System.Int32.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static int ToInt32(decimal d)
	{
		if ((d.flags & 0xFF0000) != 0)
		{
			FCallTruncate(ref d);
		}
		if (d.hi == 0 && d.mid == 0)
		{
			int num = d.lo;
			if (d.flags >= 0)
			{
				if (num >= 0)
				{
					return num;
				}
			}
			else
			{
				num = -num;
				if (num <= 0)
				{
					return num;
				}
			}
		}
		throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for an Int32."));
	}

	/// <summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 64-bit signed integer.</summary>
	/// <returns>A 64-bit signed integer equivalent to the value of <paramref name="d" />.</returns>
	/// <param name="d">The decimal number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="d" /> is less than <see cref="F:System.Int64.MinValue" /> or greater than <see cref="F:System.Int64.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static long ToInt64(decimal d)
	{
		if ((d.flags & 0xFF0000) != 0)
		{
			FCallTruncate(ref d);
		}
		if (d.hi == 0)
		{
			long num = (d.lo & 0xFFFFFFFFu) | ((long)d.mid << 32);
			if (d.flags >= 0)
			{
				if (num >= 0)
				{
					return num;
				}
			}
			else
			{
				num = -num;
				if (num <= 0)
				{
					return num;
				}
			}
		}
		throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for an Int64."));
	}

	/// <summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 16-bit unsigned integer.</summary>
	/// <returns>A 16-bit unsigned integer equivalent to the value of <paramref name="value" />.</returns>
	/// <param name="value">The decimal number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.UInt16.MaxValue" /> or less than <see cref="F:System.UInt16.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ushort ToUInt16(decimal value)
	{
		uint num;
		try
		{
			num = ToUInt32(value);
		}
		catch (OverflowException innerException)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a UInt16."), innerException);
		}
		if (num < 0 || num > 65535)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a UInt16."));
		}
		return (ushort)num;
	}

	/// <summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 32-bit unsigned integer.</summary>
	/// <returns>A 32-bit unsigned integer equivalent to the value of <paramref name="d" />.</returns>
	/// <param name="d">The decimal number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="d" /> is negative or greater than <see cref="F:System.UInt32.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	[SecuritySafeCritical]
	public static uint ToUInt32(decimal d)
	{
		if ((d.flags & 0xFF0000) != 0)
		{
			FCallTruncate(ref d);
		}
		if (d.hi == 0 && d.mid == 0)
		{
			uint num = (uint)d.lo;
			if (d.flags >= 0 || num == 0)
			{
				return num;
			}
		}
		throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a UInt32."));
	}

	/// <summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 64-bit unsigned integer.</summary>
	/// <returns>A 64-bit unsigned integer equivalent to the value of <paramref name="d" />.</returns>
	/// <param name="d">The decimal number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="d" /> is negative or greater than <see cref="F:System.UInt64.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	[SecuritySafeCritical]
	public static ulong ToUInt64(decimal d)
	{
		if ((d.flags & 0xFF0000) != 0)
		{
			FCallTruncate(ref d);
		}
		if (d.hi == 0)
		{
			ulong num = (uint)d.lo | ((ulong)(uint)d.mid << 32);
			if (d.flags >= 0 || num == 0L)
			{
				return num;
			}
		}
		throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a UInt64."));
	}

	/// <summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent single-precision floating-point number.</summary>
	/// <returns>A single-precision floating-point number equivalent to the value of <paramref name="d" />.</returns>
	/// <param name="d">The decimal number to convert. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecuritySafeCritical]
	public static extern float ToSingle(decimal d);

	/// <summary>Returns the integral digits of the specified <see cref="T:System.Decimal" />; any fractional digits are discarded.</summary>
	/// <returns>The result of <paramref name="d" /> rounded toward zero, to the nearest whole number.</returns>
	/// <param name="d">The decimal number to truncate. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static decimal Truncate(decimal d)
	{
		FCallTruncate(ref d);
		return d;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private static extern void FCallTruncate(ref decimal d);

	/// <summary>Defines an explicit conversion of an 8-bit unsigned integer to a <see cref="T:System.Decimal" />.</summary>
	/// <returns>The converted 8-bit unsigned integer.</returns>
	/// <param name="value">The 8-bit unsigned integer to convert. </param>
	/// <filterpriority>3</filterpriority>
	public static implicit operator decimal(byte value)
	{
		return new decimal(value);
	}

	/// <summary>Defines an explicit conversion of an 8-bit signed integer to a <see cref="T:System.Decimal" />.</summary>
	/// <returns>The converted 8-bit signed integer.</returns>
	/// <param name="value">The 8-bit signed integer to convert. </param>
	/// <filterpriority>3</filterpriority>
	[CLSCompliant(false)]
	public static implicit operator decimal(sbyte value)
	{
		return new decimal(value);
	}

	/// <summary>Defines an explicit conversion of a 16-bit signed integer to a <see cref="T:System.Decimal" />.</summary>
	/// <returns>The converted 16-bit signed integer.</returns>
	/// <param name="value">The16-bit signed integer to convert. </param>
	/// <filterpriority>3</filterpriority>
	public static implicit operator decimal(short value)
	{
		return new decimal(value);
	}

	/// <summary>Defines an explicit conversion of a 16-bit unsigned integer to a <see cref="T:System.Decimal" />.</summary>
	/// <returns>The converted 16-bit unsigned integer.</returns>
	/// <param name="value">The 16-bit unsigned integer to convert. </param>
	/// <filterpriority>3</filterpriority>
	[CLSCompliant(false)]
	public static implicit operator decimal(ushort value)
	{
		return new decimal(value);
	}

	/// <summary>Defines an explicit conversion of a Unicode character to a <see cref="T:System.Decimal" />.</summary>
	/// <returns>The converted Unicode character.</returns>
	/// <param name="value">The Unicode character to convert. </param>
	/// <filterpriority>3</filterpriority>
	public static implicit operator decimal(char value)
	{
		return new decimal(value);
	}

	/// <summary>Defines an explicit conversion of a 32-bit signed integer to a <see cref="T:System.Decimal" />.</summary>
	/// <returns>The converted 32-bit signed integer.</returns>
	/// <param name="value">The 32-bit signed integer to convert. </param>
	/// <filterpriority>3</filterpriority>
	public static implicit operator decimal(int value)
	{
		return new decimal(value);
	}

	/// <summary>Defines an explicit conversion of a 32-bit unsigned integer to a <see cref="T:System.Decimal" />.</summary>
	/// <returns>The converted 32-bit unsigned integer.</returns>
	/// <param name="value">The 32-bit unsigned integer to convert. </param>
	/// <filterpriority>3</filterpriority>
	[CLSCompliant(false)]
	public static implicit operator decimal(uint value)
	{
		return new decimal(value);
	}

	/// <summary>Defines an explicit conversion of a 64-bit signed integer to a <see cref="T:System.Decimal" />.</summary>
	/// <returns>The converted 64-bit signed integer.</returns>
	/// <param name="value">The 64-bit signed integer to convert. </param>
	/// <filterpriority>3</filterpriority>
	public static implicit operator decimal(long value)
	{
		return new decimal(value);
	}

	/// <summary>Defines an explicit conversion of a 64-bit unsigned integer to a <see cref="T:System.Decimal" />.</summary>
	/// <returns>The converted 64-bit unsigned integer.</returns>
	/// <param name="value">The 64-bit unsigned integer to convert. </param>
	/// <filterpriority>3</filterpriority>
	[CLSCompliant(false)]
	public static implicit operator decimal(ulong value)
	{
		return new decimal(value);
	}

	/// <summary>Defines an explicit conversion of a single-precision floating-point number to a <see cref="T:System.Decimal" />.</summary>
	/// <returns>The converted single-precision floating point number.</returns>
	/// <param name="value">The single-precision floating-point number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" />.-or- <paramref name="value" /> is <see cref="F:System.Single.NaN" />, <see cref="F:System.Single.PositiveInfinity" />, or <see cref="F:System.Single.NegativeInfinity" />. </exception>
	/// <filterpriority>3</filterpriority>
	public static explicit operator decimal(float value)
	{
		return new decimal(value);
	}

	/// <summary>Defines an explicit conversion of a double-precision floating-point number to a <see cref="T:System.Decimal" />.</summary>
	/// <returns>The converted double-precision floating point number.</returns>
	/// <param name="value">The double-precision floating-point number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" />.-or- <paramref name="value" /> is <see cref="F:System.Double.NaN" />, <see cref="F:System.Double.PositiveInfinity" />, or <see cref="F:System.Double.NegativeInfinity" />. </exception>
	/// <filterpriority>3</filterpriority>
	public static explicit operator decimal(double value)
	{
		return new decimal(value);
	}

	/// <summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to an 8-bit unsigned integer.</summary>
	/// <returns>An 8-bit unsigned integer that represents the converted <see cref="T:System.Decimal" />.</returns>
	/// <param name="value">The value to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.Byte.MinValue" /> or greater than <see cref="F:System.Byte.MaxValue" />. </exception>
	/// <filterpriority>3</filterpriority>
	public static explicit operator byte(decimal value)
	{
		return ToByte(value);
	}

	/// <summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to an 8-bit signed integer.</summary>
	/// <returns>An 8-bit signed integer that represents the converted <see cref="T:System.Decimal" />.</returns>
	/// <param name="value">The value to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.SByte.MinValue" /> or greater than <see cref="F:System.SByte.MaxValue" />. </exception>
	/// <filterpriority>3</filterpriority>
	[CLSCompliant(false)]
	public static explicit operator sbyte(decimal value)
	{
		return ToSByte(value);
	}

	/// <summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a Unicode character.</summary>
	/// <returns>A Unicode character that represents the converted <see cref="T:System.Decimal" />.</returns>
	/// <param name="value">The value to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.Char.MinValue" /> or greater than <see cref="F:System.Char.MaxValue" />. </exception>
	/// <filterpriority>3</filterpriority>
	public static explicit operator char(decimal value)
	{
		try
		{
			return (char)ToUInt16(value);
		}
		catch (OverflowException innerException)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a character."), innerException);
		}
	}

	/// <summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 16-bit signed integer.</summary>
	/// <returns>A 16-bit signed integer that represents the converted <see cref="T:System.Decimal" />.</returns>
	/// <param name="value">The value to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.Int16.MinValue" /> or greater than <see cref="F:System.Int16.MaxValue" />. </exception>
	/// <filterpriority>3</filterpriority>
	public static explicit operator short(decimal value)
	{
		return ToInt16(value);
	}

	/// <summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 16-bit unsigned integer.</summary>
	/// <returns>A 16-bit unsigned integer that represents the converted <see cref="T:System.Decimal" />.</returns>
	/// <param name="value">The value to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.UInt16.MaxValue" /> or less than <see cref="F:System.UInt16.MinValue" />. </exception>
	/// <filterpriority>3</filterpriority>
	[CLSCompliant(false)]
	public static explicit operator ushort(decimal value)
	{
		return ToUInt16(value);
	}

	/// <summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 32-bit signed integer.</summary>
	/// <returns>A 32-bit signed integer that represents the converted <see cref="T:System.Decimal" />.</returns>
	/// <param name="value">The value to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.Int32.MinValue" /> or greater than <see cref="F:System.Int32.MaxValue" />. </exception>
	/// <filterpriority>3</filterpriority>
	public static explicit operator int(decimal value)
	{
		return ToInt32(value);
	}

	/// <summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 32-bit unsigned integer.</summary>
	/// <returns>A 32-bit unsigned integer that represents the converted <see cref="T:System.Decimal" />.</returns>
	/// <param name="value">The value to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is negative or greater than <see cref="F:System.UInt32.MaxValue" />. </exception>
	/// <filterpriority>3</filterpriority>
	[CLSCompliant(false)]
	public static explicit operator uint(decimal value)
	{
		return ToUInt32(value);
	}

	/// <summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 64-bit signed integer.</summary>
	/// <returns>A 64-bit signed integer that represents the converted <see cref="T:System.Decimal" />.</returns>
	/// <param name="value">The value to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.Int64.MinValue" /> or greater than <see cref="F:System.Int64.MaxValue" />. </exception>
	/// <filterpriority>3</filterpriority>
	public static explicit operator long(decimal value)
	{
		return ToInt64(value);
	}

	/// <summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 64-bit unsigned integer.</summary>
	/// <returns>A 64-bit unsigned integer that represents the converted <see cref="T:System.Decimal" />.</returns>
	/// <param name="value">The value to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is negative or greater than <see cref="F:System.UInt64.MaxValue" />. </exception>
	/// <filterpriority>3</filterpriority>
	[CLSCompliant(false)]
	public static explicit operator ulong(decimal value)
	{
		return ToUInt64(value);
	}

	/// <summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a single-precision floating-point number.</summary>
	/// <returns>A single-precision floating-point number that represents the converted <see cref="T:System.Decimal" />.</returns>
	/// <param name="value">The value to convert. </param>
	/// <filterpriority>3</filterpriority>
	public static explicit operator float(decimal value)
	{
		return ToSingle(value);
	}

	/// <summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a double-precision floating-point number.</summary>
	/// <returns>A double-precision floating-point number that represents the converted <see cref="T:System.Decimal" />.</returns>
	/// <param name="value">The value to convert. </param>
	/// <filterpriority>3</filterpriority>
	public static explicit operator double(decimal value)
	{
		return ToDouble(value);
	}

	/// <summary>Returns the value of the <see cref="T:System.Decimal" /> operand (the sign of the operand is unchanged).</summary>
	/// <returns>The value of the operand, <paramref name="d" />.</returns>
	/// <param name="d">The operand to return.</param>
	/// <filterpriority>3</filterpriority>
	public static decimal operator +(decimal d)
	{
		return d;
	}

	/// <summary>Negates the value of the specified <see cref="T:System.Decimal" /> operand.</summary>
	/// <returns>The result of <paramref name="d" /> multiplied by negative one (-1).</returns>
	/// <param name="d">The value to negate. </param>
	/// <filterpriority>3</filterpriority>
	public static decimal operator -(decimal d)
	{
		return Negate(d);
	}

	/// <summary>Increments the <see cref="T:System.Decimal" /> operand by 1.</summary>
	/// <returns>The value of <paramref name="d" /> incremented by 1.</returns>
	/// <param name="d">The value to increment. </param>
	/// <exception cref="T:System.OverflowException">The return value is less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" />. </exception>
	/// <filterpriority>3</filterpriority>
	public static decimal operator ++(decimal d)
	{
		return Add(d, 1m);
	}

	/// <summary>Decrements the <see cref="T:System.Decimal" /> operand by one.</summary>
	/// <returns>The value of <paramref name="d" /> decremented by 1.</returns>
	/// <param name="d">The value to decrement. </param>
	/// <exception cref="T:System.OverflowException">The return value is less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" />. </exception>
	/// <filterpriority>3</filterpriority>
	public static decimal operator --(decimal d)
	{
		return Subtract(d, 1m);
	}

	/// <summary>Adds two specified <see cref="T:System.Decimal" /> values.</summary>
	/// <returns>The result of adding <paramref name="d1" /> and <paramref name="d2" />.</returns>
	/// <param name="d1">The first value to add. </param>
	/// <param name="d2">The second value to add. </param>
	/// <exception cref="T:System.OverflowException">The return value is less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" />. </exception>
	/// <filterpriority>3</filterpriority>
	[SecuritySafeCritical]
	public static decimal operator +(decimal d1, decimal d2)
	{
		FCallAddSub(ref d1, ref d2, 0);
		return d1;
	}

	/// <summary>Subtracts two specified <see cref="T:System.Decimal" /> values.</summary>
	/// <returns>The result of subtracting <paramref name="d2" /> from <paramref name="d1" />.</returns>
	/// <param name="d1">The minuend. </param>
	/// <param name="d2">The subtrahend. </param>
	/// <exception cref="T:System.OverflowException">The return value is less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" />. </exception>
	/// <filterpriority>3</filterpriority>
	[SecuritySafeCritical]
	public static decimal operator -(decimal d1, decimal d2)
	{
		FCallAddSub(ref d1, ref d2, 128);
		return d1;
	}

	/// <summary>Multiplies two specified <see cref="T:System.Decimal" /> values.</summary>
	/// <returns>The result of multiplying <paramref name="d1" /> by <paramref name="d2" />.</returns>
	/// <param name="d1">The first value to multiply. </param>
	/// <param name="d2">The second value to multiply. </param>
	/// <exception cref="T:System.OverflowException">The return value is less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" />. </exception>
	/// <filterpriority>3</filterpriority>
	[SecuritySafeCritical]
	public static decimal operator *(decimal d1, decimal d2)
	{
		FCallMultiply(ref d1, ref d2);
		return d1;
	}

	/// <summary>Divides two specified <see cref="T:System.Decimal" /> values.</summary>
	/// <returns>The result of dividing <paramref name="d1" /> by <paramref name="d2" />.</returns>
	/// <param name="d1">The dividend. </param>
	/// <param name="d2">The divisor. </param>
	/// <exception cref="T:System.DivideByZeroException">
	///   <paramref name="d2" /> is zero. </exception>
	/// <exception cref="T:System.OverflowException">The return value is less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" />. </exception>
	/// <filterpriority>3</filterpriority>
	[SecuritySafeCritical]
	public static decimal operator /(decimal d1, decimal d2)
	{
		FCallDivide(ref d1, ref d2);
		return d1;
	}

	/// <summary>Returns the remainder resulting from dividing two specified <see cref="T:System.Decimal" /> values.</summary>
	/// <returns>The remainder resulting from dividing <paramref name="d1" /> by <paramref name="d2" />.</returns>
	/// <param name="d1">The dividend. </param>
	/// <param name="d2">The divisor. </param>
	/// <exception cref="T:System.DivideByZeroException">
	///   <paramref name="d2" /> is zero. </exception>
	/// <exception cref="T:System.OverflowException">The return value is less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" />. </exception>
	/// <filterpriority>3</filterpriority>
	public static decimal operator %(decimal d1, decimal d2)
	{
		return Remainder(d1, d2);
	}

	/// <summary>Returns a value that indicates whether two <see cref="T:System.Decimal" /> values are equal.</summary>
	/// <returns>true if <paramref name="d1" /> and <paramref name="d2" /> are equal; otherwise, false.</returns>
	/// <param name="d1">The first value to compare. </param>
	/// <param name="d2">The second value to compare. </param>
	/// <filterpriority>3</filterpriority>
	[SecuritySafeCritical]
	public static bool operator ==(decimal d1, decimal d2)
	{
		return FCallCompare(ref d1, ref d2) == 0;
	}

	/// <summary>Returns a value that indicates whether two <see cref="T:System.Decimal" /> objects have different values.</summary>
	/// <returns>true if <paramref name="d1" /> and <paramref name="d2" /> are not equal; otherwise, false.</returns>
	/// <param name="d1">The first value to compare. </param>
	/// <param name="d2">The second value to compare. </param>
	/// <filterpriority>3</filterpriority>
	[SecuritySafeCritical]
	public static bool operator !=(decimal d1, decimal d2)
	{
		return FCallCompare(ref d1, ref d2) != 0;
	}

	/// <summary>Returns a value indicating whether a specified <see cref="T:System.Decimal" /> is less than another specified <see cref="T:System.Decimal" />.</summary>
	/// <returns>true if <paramref name="d1" /> is less than <paramref name="d2" />; otherwise, false.</returns>
	/// <param name="d1">The first value to compare. </param>
	/// <param name="d2">The second value to compare. </param>
	/// <filterpriority>3</filterpriority>
	[SecuritySafeCritical]
	public static bool operator <(decimal d1, decimal d2)
	{
		return FCallCompare(ref d1, ref d2) < 0;
	}

	/// <summary>Returns a value indicating whether a specified <see cref="T:System.Decimal" /> is less than or equal to another specified <see cref="T:System.Decimal" />.</summary>
	/// <returns>true if <paramref name="d1" /> is less than or equal to <paramref name="d2" />; otherwise, false.</returns>
	/// <param name="d1">The first value to compare. </param>
	/// <param name="d2">The second value to compare. </param>
	/// <filterpriority>3</filterpriority>
	[SecuritySafeCritical]
	public static bool operator <=(decimal d1, decimal d2)
	{
		return FCallCompare(ref d1, ref d2) <= 0;
	}

	/// <summary>Returns a value indicating whether a specified <see cref="T:System.Decimal" /> is greater than another specified <see cref="T:System.Decimal" />.</summary>
	/// <returns>true if <paramref name="d1" /> is greater than <paramref name="d2" />; otherwise, false.</returns>
	/// <param name="d1">The first value to compare. </param>
	/// <param name="d2">The second value to compare. </param>
	/// <filterpriority>3</filterpriority>
	[SecuritySafeCritical]
	public static bool operator >(decimal d1, decimal d2)
	{
		return FCallCompare(ref d1, ref d2) > 0;
	}

	/// <summary>Returns a value indicating whether a specified <see cref="T:System.Decimal" /> is greater than or equal to another specified <see cref="T:System.Decimal" />.</summary>
	/// <returns>true if <paramref name="d1" /> is greater than or equal to <paramref name="d2" />; otherwise, false.</returns>
	/// <param name="d1">The first value to compare. </param>
	/// <param name="d2">The second value to compare. </param>
	/// <filterpriority>3</filterpriority>
	[SecuritySafeCritical]
	public static bool operator >=(decimal d1, decimal d2)
	{
		return FCallCompare(ref d1, ref d2) >= 0;
	}

	/// <summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.Decimal" />.</summary>
	/// <returns>The enumerated constant <see cref="F:System.TypeCode.Decimal" />.</returns>
	/// <filterpriority>2</filterpriority>
	public TypeCode GetTypeCode()
	{
		return TypeCode.Decimal;
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToBoolean(System.IFormatProvider)" />.</summary>
	/// <returns>true if the value of the current instance is not zero; otherwise, false.</returns>
	/// <param name="provider">This parameter is ignored. </param>
	bool IConvertible.ToBoolean(IFormatProvider provider)
	{
		return Convert.ToBoolean(this);
	}

	/// <summary>This conversion is not supported. Attempting to use this method throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>None. This conversion is not supported. </returns>
	/// <param name="provider">This parameter is ignored.</param>
	/// <exception cref="T:System.InvalidCastException">In all cases. </exception>
	char IConvertible.ToChar(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Invalid cast from '{0}' to '{1}'.", "Decimal", "Char"));
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToSByte(System.IFormatProvider)" />.</summary>
	/// <returns>The value of the current instance, converted to a <see cref="T:System.SByte" />.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	/// <exception cref="T:System.OverflowException">The resulting integer value is less than <see cref="F:System.SByte.MinValue" /> or greater than <see cref="F:System.SByte.MaxValue" />. </exception>
	sbyte IConvertible.ToSByte(IFormatProvider provider)
	{
		return Convert.ToSByte(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToByte(System.IFormatProvider)" />.</summary>
	/// <returns>The value of the current instance, converted to a <see cref="T:System.Byte" />.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	/// <exception cref="T:System.OverflowException">The resulting integer value is less than <see cref="F:System.Byte.MinValue" /> or greater than <see cref="F:System.Byte.MaxValue" />. </exception>
	byte IConvertible.ToByte(IFormatProvider provider)
	{
		return Convert.ToByte(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToInt16(System.IFormatProvider)" />.</summary>
	/// <returns>The value of the current instance, converted to a <see cref="T:System.Int16" />.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	/// <exception cref="T:System.OverflowException">The resulting integer value is less than <see cref="F:System.Int16.MinValue" /> or greater than <see cref="F:System.Int16.MaxValue" />.</exception>
	short IConvertible.ToInt16(IFormatProvider provider)
	{
		return Convert.ToInt16(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToUInt16(System.IFormatProvider)" />.</summary>
	/// <returns>The value of the current instance, converted to a <see cref="T:System.UInt16" />.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	/// <exception cref="T:System.OverflowException">The resulting integer value is less than <see cref="F:System.UInt16.MinValue" /> or greater than <see cref="F:System.UInt16.MaxValue" />.</exception>
	ushort IConvertible.ToUInt16(IFormatProvider provider)
	{
		return Convert.ToUInt16(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToInt32(System.IFormatProvider)" />.</summary>
	/// <returns>The value of the current instance, converted to a <see cref="T:System.Int32" />.</returns>
	/// <param name="provider">The parameter is ignored.</param>
	/// <exception cref="T:System.OverflowException">The resulting integer value is less than <see cref="F:System.Int32.MinValue" /> or greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	int IConvertible.ToInt32(IFormatProvider provider)
	{
		return Convert.ToInt32(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToInt32(System.IFormatProvider)" />.</summary>
	/// <returns>The value of the current instance, converted to a <see cref="T:System.UInt32" />.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	/// <exception cref="T:System.OverflowException">The resulting integer value is less than <see cref="F:System.UInt32.MinValue" /> or greater than <see cref="F:System.UInt32.MaxValue" />.</exception>
	uint IConvertible.ToUInt32(IFormatProvider provider)
	{
		return Convert.ToUInt32(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToInt64(System.IFormatProvider)" />.</summary>
	/// <returns>The value of the current instance, converted to a <see cref="T:System.Int64" />.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	/// <exception cref="T:System.OverflowException">The resulting integer value is less than <see cref="F:System.Int64.MinValue" /> or greater than <see cref="F:System.Int64.MaxValue" />. </exception>
	long IConvertible.ToInt64(IFormatProvider provider)
	{
		return Convert.ToInt64(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToInt64(System.IFormatProvider)" />.</summary>
	/// <returns>The value of the current instance, converted to a <see cref="T:System.UInt64" />.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	/// <exception cref="T:System.OverflowException">The resulting integer value is less than <see cref="F:System.UInt64.MinValue" /> or greater than <see cref="F:System.UInt64.MaxValue" />.</exception>
	ulong IConvertible.ToUInt64(IFormatProvider provider)
	{
		return Convert.ToUInt64(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToSingle(System.IFormatProvider)" />.</summary>
	/// <returns>The value of the current instance, converted to a <see cref="T:System.Single" />.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	float IConvertible.ToSingle(IFormatProvider provider)
	{
		return Convert.ToSingle(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToDouble(System.IFormatProvider)" />.</summary>
	/// <returns>The value of the current instance, converted to a <see cref="T:System.Double" />.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	double IConvertible.ToDouble(IFormatProvider provider)
	{
		return Convert.ToDouble(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToDecimal(System.IFormatProvider)" />.</summary>
	/// <returns>The value of the current instance, unchanged.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	decimal IConvertible.ToDecimal(IFormatProvider provider)
	{
		return this;
	}

	/// <summary>This conversion is not supported. Attempting to use this method throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>None. This conversion is not supported. </returns>
	/// <param name="provider">This parameter is ignored.</param>
	/// <exception cref="T:System.InvalidCastException">In all cases.</exception>
	DateTime IConvertible.ToDateTime(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Invalid cast from '{0}' to '{1}'.", "Decimal", "DateTime"));
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToType(System.Type,System.IFormatProvider)" />.</summary>
	/// <returns>The value of the current instance, converted to a <paramref name="type" />.</returns>
	/// <param name="type">The type to which to convert the value of this <see cref="T:System.Decimal" /> instance. </param>
	/// <param name="provider">An <see cref="T:System.IFormatProvider" /> implementation that supplies culture-specific information about the format of the returned value.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null. </exception>
	/// <exception cref="T:System.InvalidCastException">The requested type conversion is not supported. </exception>
	object IConvertible.ToType(Type type, IFormatProvider provider)
	{
		return Convert.DefaultToType(this, type, provider);
	}
}
