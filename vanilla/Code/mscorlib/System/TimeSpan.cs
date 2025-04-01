using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace System;

/// <summary>Represents a time interval.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
public struct TimeSpan : IComparable, IComparable<TimeSpan>, IEquatable<TimeSpan>, IFormattable
{
	/// <summary>Represents the number of ticks in 1 millisecond. This field is constant.</summary>
	/// <filterpriority>1</filterpriority>
	public const long TicksPerMillisecond = 10000L;

	private const double MillisecondsPerTick = 0.0001;

	/// <summary>Represents the number of ticks in 1 second.</summary>
	/// <filterpriority>1</filterpriority>
	public const long TicksPerSecond = 10000000L;

	private const double SecondsPerTick = 1E-07;

	/// <summary>Represents the number of ticks in 1 minute. This field is constant.</summary>
	/// <filterpriority>1</filterpriority>
	public const long TicksPerMinute = 600000000L;

	private const double MinutesPerTick = 1.6666666666666667E-09;

	/// <summary>Represents the number of ticks in 1 hour. This field is constant.</summary>
	/// <filterpriority>1</filterpriority>
	public const long TicksPerHour = 36000000000L;

	private const double HoursPerTick = 2.7777777777777777E-11;

	/// <summary>Represents the number of ticks in 1 day. This field is constant.</summary>
	/// <filterpriority>1</filterpriority>
	public const long TicksPerDay = 864000000000L;

	private const double DaysPerTick = 1.1574074074074074E-12;

	private const int MillisPerSecond = 1000;

	private const int MillisPerMinute = 60000;

	private const int MillisPerHour = 3600000;

	private const int MillisPerDay = 86400000;

	internal const long MaxSeconds = 922337203685L;

	internal const long MinSeconds = -922337203685L;

	internal const long MaxMilliSeconds = 922337203685477L;

	internal const long MinMilliSeconds = -922337203685477L;

	internal const long TicksPerTenthSecond = 1000000L;

	/// <summary>Represents the zero <see cref="T:System.TimeSpan" /> value. This field is read-only.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly TimeSpan Zero = new TimeSpan(0L);

	/// <summary>Represents the maximum <see cref="T:System.TimeSpan" /> value. This field is read-only.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly TimeSpan MaxValue = new TimeSpan(long.MaxValue);

	/// <summary>Represents the minimum <see cref="T:System.TimeSpan" /> value. This field is read-only.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly TimeSpan MinValue = new TimeSpan(long.MinValue);

	internal long _ticks;

	private static volatile bool _legacyConfigChecked;

	private static volatile bool _legacyMode;

	/// <summary>Gets the number of ticks that represent the value of the current <see cref="T:System.TimeSpan" /> structure.</summary>
	/// <returns>The number of ticks contained in this instance.</returns>
	/// <filterpriority>1</filterpriority>
	public long Ticks => _ticks;

	/// <summary>Gets the days component of the time interval represented by the current <see cref="T:System.TimeSpan" /> structure.</summary>
	/// <returns>The day component of this instance. The return value can be positive or negative.</returns>
	/// <filterpriority>1</filterpriority>
	public int Days => (int)(_ticks / 864000000000L);

	/// <summary>Gets the hours component of the time interval represented by the current <see cref="T:System.TimeSpan" /> structure.</summary>
	/// <returns>The hour component of the current <see cref="T:System.TimeSpan" /> structure. The return value ranges from -23 through 23.</returns>
	/// <filterpriority>1</filterpriority>
	public int Hours => (int)(_ticks / 36000000000L % 24);

	/// <summary>Gets the milliseconds component of the time interval represented by the current <see cref="T:System.TimeSpan" /> structure.</summary>
	/// <returns>The millisecond component of the current <see cref="T:System.TimeSpan" /> structure. The return value ranges from -999 through 999.</returns>
	/// <filterpriority>1</filterpriority>
	public int Milliseconds => (int)(_ticks / 10000 % 1000);

	/// <summary>Gets the minutes component of the time interval represented by the current <see cref="T:System.TimeSpan" /> structure.</summary>
	/// <returns>The minute component of the current <see cref="T:System.TimeSpan" /> structure. The return value ranges from -59 through 59.</returns>
	/// <filterpriority>1</filterpriority>
	public int Minutes => (int)(_ticks / 600000000 % 60);

	/// <summary>Gets the seconds component of the time interval represented by the current <see cref="T:System.TimeSpan" /> structure.</summary>
	/// <returns>The second component of the current <see cref="T:System.TimeSpan" /> structure. The return value ranges from -59 through 59.</returns>
	/// <filterpriority>1</filterpriority>
	public int Seconds => (int)(_ticks / 10000000 % 60);

	/// <summary>Gets the value of the current <see cref="T:System.TimeSpan" /> structure expressed in whole and fractional days.</summary>
	/// <returns>The total number of days represented by this instance.</returns>
	/// <filterpriority>1</filterpriority>
	public double TotalDays => (double)_ticks * 1.1574074074074074E-12;

	/// <summary>Gets the value of the current <see cref="T:System.TimeSpan" /> structure expressed in whole and fractional hours.</summary>
	/// <returns>The total number of hours represented by this instance.</returns>
	/// <filterpriority>1</filterpriority>
	public double TotalHours => (double)_ticks * 2.7777777777777777E-11;

	/// <summary>Gets the value of the current <see cref="T:System.TimeSpan" /> structure expressed in whole and fractional milliseconds.</summary>
	/// <returns>The total number of milliseconds represented by this instance.</returns>
	/// <filterpriority>1</filterpriority>
	public double TotalMilliseconds
	{
		get
		{
			double num = (double)_ticks * 0.0001;
			if (num > 922337203685477.0)
			{
				return 922337203685477.0;
			}
			if (num < -922337203685477.0)
			{
				return -922337203685477.0;
			}
			return num;
		}
	}

	/// <summary>Gets the value of the current <see cref="T:System.TimeSpan" /> structure expressed in whole and fractional minutes.</summary>
	/// <returns>The total number of minutes represented by this instance.</returns>
	/// <filterpriority>1</filterpriority>
	public double TotalMinutes => (double)_ticks * 1.6666666666666667E-09;

	/// <summary>Gets the value of the current <see cref="T:System.TimeSpan" /> structure expressed in whole and fractional seconds.</summary>
	/// <returns>The total number of seconds represented by this instance.</returns>
	/// <filterpriority>1</filterpriority>
	public double TotalSeconds => (double)_ticks * 1E-07;

	private static bool LegacyMode
	{
		get
		{
			if (!_legacyConfigChecked)
			{
				_legacyMode = GetLegacyFormatMode();
				_legacyConfigChecked = true;
			}
			return _legacyMode;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to the specified number of ticks.</summary>
	/// <param name="ticks">A time period expressed in 100-nanosecond units. </param>
	public TimeSpan(long ticks)
	{
		_ticks = ticks;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of hours, minutes, and seconds.</summary>
	/// <param name="hours">Number of hours. </param>
	/// <param name="minutes">Number of minutes. </param>
	/// <param name="seconds">Number of seconds. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The parameters specify a <see cref="T:System.TimeSpan" /> value less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" />. </exception>
	public TimeSpan(int hours, int minutes, int seconds)
	{
		_ticks = TimeToTicks(hours, minutes, seconds);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of days, hours, minutes, and seconds.</summary>
	/// <param name="days">Number of days. </param>
	/// <param name="hours">Number of hours. </param>
	/// <param name="minutes">Number of minutes. </param>
	/// <param name="seconds">Number of seconds. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The parameters specify a <see cref="T:System.TimeSpan" /> value less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" />. </exception>
	public TimeSpan(int days, int hours, int minutes, int seconds)
		: this(days, hours, minutes, seconds, 0)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> strrructure to a specified number of days, hours, minutes, seconds, and milliseconds.</summary>
	/// <param name="days">Number of days. </param>
	/// <param name="hours">Number of hours. </param>
	/// <param name="minutes">Number of minutes. </param>
	/// <param name="seconds">Number of seconds. </param>
	/// <param name="milliseconds">Number of milliseconds. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The parameters specify a <see cref="T:System.TimeSpan" /> value less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" />. </exception>
	public TimeSpan(int days, int hours, int minutes, int seconds, int milliseconds)
	{
		long num = ((long)days * 3600L * 24 + (long)hours * 3600L + (long)minutes * 60L + seconds) * 1000 + milliseconds;
		if (num > 922337203685477L || num < -922337203685477L)
		{
			throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("TimeSpan overflowed because the duration is too long."));
		}
		_ticks = num * 10000;
	}

	/// <summary>Returns a new <see cref="T:System.TimeSpan" /> object whose value is the sum of the specified <see cref="T:System.TimeSpan" /> object and this instance.</summary>
	/// <returns>A new object that represents the value of this instance plus the value of <paramref name="ts" />.</returns>
	/// <param name="ts">The time interval to add.</param>
	/// <exception cref="T:System.OverflowException">The resulting <see cref="T:System.TimeSpan" /> is less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public TimeSpan Add(TimeSpan ts)
	{
		long num = _ticks + ts._ticks;
		if (_ticks >> 63 == ts._ticks >> 63 && _ticks >> 63 != num >> 63)
		{
			throw new OverflowException(Environment.GetResourceString("TimeSpan overflowed because the duration is too long."));
		}
		return new TimeSpan(num);
	}

	/// <summary>Compares two <see cref="T:System.TimeSpan" /> values and returns an integer that indicates whether the first value is shorter than, equal to, or longer than the second value.</summary>
	/// <returns>One of the following values.Value Description -1 <paramref name="t1" /> is shorter than <paramref name="t2" />. 0 <paramref name="t1" /> is equal to <paramref name="t2" />. 1 <paramref name="t1" /> is longer than <paramref name="t2" />. </returns>
	/// <param name="t1">The first time interval to compare. </param>
	/// <param name="t2">The second time interval to compare. </param>
	/// <filterpriority>1</filterpriority>
	public static int Compare(TimeSpan t1, TimeSpan t2)
	{
		if (t1._ticks > t2._ticks)
		{
			return 1;
		}
		if (t1._ticks < t2._ticks)
		{
			return -1;
		}
		return 0;
	}

	/// <summary>Compares this instance to a specified object and returns an integer that indicates whether this instance is shorter than, equal to, or longer than the specified object.</summary>
	/// <returns>One of the following values.Value Description -1 This instance is shorter than <paramref name="value" />. 0 This instance is equal to <paramref name="value" />. 1 This instance is longer than <paramref name="value" />.-or- <paramref name="value" /> is null. </returns>
	/// <param name="value">An object to compare, or null. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is not a <see cref="T:System.TimeSpan" />. </exception>
	/// <filterpriority>1</filterpriority>
	public int CompareTo(object value)
	{
		if (value == null)
		{
			return 1;
		}
		if (!(value is TimeSpan))
		{
			throw new ArgumentException(Environment.GetResourceString("Object must be of type TimeSpan."));
		}
		long ticks = ((TimeSpan)value)._ticks;
		if (_ticks > ticks)
		{
			return 1;
		}
		if (_ticks < ticks)
		{
			return -1;
		}
		return 0;
	}

	/// <summary>Compares this instance to a specified <see cref="T:System.TimeSpan" /> object and returns an integer that indicates whether this instance is shorter than, equal to, or longer than the <see cref="T:System.TimeSpan" /> object.</summary>
	/// <returns>A signed number indicating the relative values of this instance and <paramref name="value" />.Value Description A negative integer This instance is shorter than <paramref name="value" />. Zero This instance is equal to <paramref name="value" />. A positive integer This instance is longer than <paramref name="value" />. </returns>
	/// <param name="value">An object to compare to this instance.</param>
	/// <filterpriority>1</filterpriority>
	public int CompareTo(TimeSpan value)
	{
		long ticks = value._ticks;
		if (_ticks > ticks)
		{
			return 1;
		}
		if (_ticks < ticks)
		{
			return -1;
		}
		return 0;
	}

	/// <summary>Returns a <see cref="T:System.TimeSpan" /> that represents a specified number of days, where the specification is accurate to the nearest millisecond.</summary>
	/// <returns>An object that represents <paramref name="value" />.</returns>
	/// <param name="value">A number of days, accurate to the nearest millisecond. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" />. -or-<paramref name="value" /> is <see cref="F:System.Double.PositiveInfinity" />.-or-<paramref name="value" /> is <see cref="F:System.Double.NegativeInfinity" />.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is equal to <see cref="F:System.Double.NaN" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static TimeSpan FromDays(double value)
	{
		return Interval(value, 86400000);
	}

	/// <summary>Returns a new <see cref="T:System.TimeSpan" /> object whose value is the absolute value of the current <see cref="T:System.TimeSpan" /> object.</summary>
	/// <returns>A new object whose value is the absolute value of the current <see cref="T:System.TimeSpan" /> object.</returns>
	/// <exception cref="T:System.OverflowException">The value of this instance is <see cref="F:System.TimeSpan.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public TimeSpan Duration()
	{
		if (Ticks == MinValue.Ticks)
		{
			throw new OverflowException(Environment.GetResourceString("The duration cannot be returned for TimeSpan.MinValue because the absolute value of TimeSpan.MinValue exceeds the value of TimeSpan.MaxValue."));
		}
		return new TimeSpan((_ticks >= 0) ? _ticks : (-_ticks));
	}

	/// <summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	/// <returns>true if <paramref name="value" /> is a <see cref="T:System.TimeSpan" /> object that represents the same time interval as the current <see cref="T:System.TimeSpan" /> structure; otherwise, false.</returns>
	/// <param name="value">An object to compare with this instance. </param>
	/// <filterpriority>1</filterpriority>
	public override bool Equals(object value)
	{
		if (value is TimeSpan)
		{
			return _ticks == ((TimeSpan)value)._ticks;
		}
		return false;
	}

	/// <summary>Returns a value indicating whether this instance is equal to a specified <see cref="T:System.TimeSpan" /> object.</summary>
	/// <returns>true if <paramref name="obj" /> represents the same time interval as this instance; otherwise, false.</returns>
	/// <param name="obj">An object to compare with this instance. </param>
	/// <filterpriority>1</filterpriority>
	public bool Equals(TimeSpan obj)
	{
		return _ticks == obj._ticks;
	}

	/// <summary>Returns a value that indicates whether two specified instances of <see cref="T:System.TimeSpan" /> are equal.</summary>
	/// <returns>true if the values of <paramref name="t1" /> and <paramref name="t2" /> are equal; otherwise, false.</returns>
	/// <param name="t1">The first time interval to compare. </param>
	/// <param name="t2">The second time interval to compare. </param>
	/// <filterpriority>1</filterpriority>
	public static bool Equals(TimeSpan t1, TimeSpan t2)
	{
		return t1._ticks == t2._ticks;
	}

	/// <summary>Returns a hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <filterpriority>2</filterpriority>
	public override int GetHashCode()
	{
		return (int)_ticks ^ (int)(_ticks >> 32);
	}

	/// <summary>Returns a <see cref="T:System.TimeSpan" /> that represents a specified number of hours, where the specification is accurate to the nearest millisecond.</summary>
	/// <returns>An object that represents <paramref name="value" />.</returns>
	/// <param name="value">A number of hours accurate to the nearest millisecond. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" />. -or-<paramref name="value" /> is <see cref="F:System.Double.PositiveInfinity" />.-or-<paramref name="value" /> is <see cref="F:System.Double.NegativeInfinity" />.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is equal to <see cref="F:System.Double.NaN" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static TimeSpan FromHours(double value)
	{
		return Interval(value, 3600000);
	}

	private static TimeSpan Interval(double value, int scale)
	{
		if (double.IsNaN(value))
		{
			throw new ArgumentException(Environment.GetResourceString("TimeSpan does not accept floating point Not-a-Number values."));
		}
		double num = value * (double)scale + ((value >= 0.0) ? 0.5 : (-0.5));
		if (num > 922337203685477.0 || num < -922337203685477.0)
		{
			throw new OverflowException(Environment.GetResourceString("TimeSpan overflowed because the duration is too long."));
		}
		return new TimeSpan((long)num * 10000);
	}

	/// <summary>Returns a <see cref="T:System.TimeSpan" /> that represents a specified number of milliseconds.</summary>
	/// <returns>An object that represents <paramref name="value" />.</returns>
	/// <param name="value">A number of milliseconds. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" />.-or-<paramref name="value" /> is <see cref="F:System.Double.PositiveInfinity" />.-or-<paramref name="value" /> is <see cref="F:System.Double.NegativeInfinity" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is equal to <see cref="F:System.Double.NaN" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static TimeSpan FromMilliseconds(double value)
	{
		return Interval(value, 1);
	}

	/// <summary>Returns a <see cref="T:System.TimeSpan" /> that represents a specified number of minutes, where the specification is accurate to the nearest millisecond.</summary>
	/// <returns>An object that represents <paramref name="value" />.</returns>
	/// <param name="value">A number of minutes, accurate to the nearest millisecond. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" />.-or-<paramref name="value" /> is <see cref="F:System.Double.PositiveInfinity" />.-or-<paramref name="value" /> is <see cref="F:System.Double.NegativeInfinity" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is equal to <see cref="F:System.Double.NaN" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static TimeSpan FromMinutes(double value)
	{
		return Interval(value, 60000);
	}

	/// <summary>Returns a new <see cref="T:System.TimeSpan" /> object whose value is the negated value of this instance.</summary>
	/// <returns>A new object with the same numeric value as this instance, but with the opposite sign.</returns>
	/// <exception cref="T:System.OverflowException">The negated value of this instance cannot be represented by a <see cref="T:System.TimeSpan" />; that is, the value of this instance is <see cref="F:System.TimeSpan.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public TimeSpan Negate()
	{
		if (Ticks == MinValue.Ticks)
		{
			throw new OverflowException(Environment.GetResourceString("Negating the minimum value of a twos complement number is invalid."));
		}
		return new TimeSpan(-_ticks);
	}

	/// <summary>Returns a <see cref="T:System.TimeSpan" /> that represents a specified number of seconds, where the specification is accurate to the nearest millisecond.</summary>
	/// <returns>An object that represents <paramref name="value" />.</returns>
	/// <param name="value">A number of seconds, accurate to the nearest millisecond. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" />.-or-<paramref name="value" /> is <see cref="F:System.Double.PositiveInfinity" />.-or-<paramref name="value" /> is <see cref="F:System.Double.NegativeInfinity" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is equal to <see cref="F:System.Double.NaN" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static TimeSpan FromSeconds(double value)
	{
		return Interval(value, 1000);
	}

	/// <summary>Returns a new <see cref="T:System.TimeSpan" /> object whose value is the difference between the specified <see cref="T:System.TimeSpan" /> object and this instance.</summary>
	/// <returns>A new time interval whose value is the result of the value of this instance minus the value of <paramref name="ts" />.</returns>
	/// <param name="ts">The time interval to be subtracted. </param>
	/// <exception cref="T:System.OverflowException">The return value is less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public TimeSpan Subtract(TimeSpan ts)
	{
		long num = _ticks - ts._ticks;
		if (_ticks >> 63 != ts._ticks >> 63 && _ticks >> 63 != num >> 63)
		{
			throw new OverflowException(Environment.GetResourceString("TimeSpan overflowed because the duration is too long."));
		}
		return new TimeSpan(num);
	}

	/// <summary>Returns a <see cref="T:System.TimeSpan" /> that represents a specified time, where the specification is in units of ticks.</summary>
	/// <returns>An object that represents <paramref name="value" />.</returns>
	/// <param name="value">A number of ticks that represent a time. </param>
	/// <filterpriority>1</filterpriority>
	public static TimeSpan FromTicks(long value)
	{
		return new TimeSpan(value);
	}

	internal static long TimeToTicks(int hour, int minute, int second)
	{
		long num = (long)hour * 3600L + (long)minute * 60L + second;
		if (num > 922337203685L || num < -922337203685L)
		{
			throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("TimeSpan overflowed because the duration is too long."));
		}
		return num * 10000000;
	}

	/// <summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent. </summary>
	/// <returns>A time interval that corresponds to <paramref name="s" />.</returns>
	/// <param name="s">A string that specifies the time interval to convert. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="s" /> has an invalid format. </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="s" /> represents a number that is less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" />.-or- At least one of the days, hours, minutes, or seconds components is outside its valid range. </exception>
	/// <filterpriority>1</filterpriority>
	public static TimeSpan Parse(string s)
	{
		return TimeSpanParse.Parse(s, null);
	}

	/// <summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified culture-specific format information.</summary>
	/// <returns>A time interval that corresponds to <paramref name="input" />, as specified by <paramref name="formatProvider" />.</returns>
	/// <param name="input">A string that specifies the time interval to convert.</param>
	/// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="input" /> has an invalid format. </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="input" /> represents a number that is less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" />.-or- At least one of the days, hours, minutes, or seconds components in <paramref name="input" /> is outside its valid range. </exception>
	public static TimeSpan Parse(string input, IFormatProvider formatProvider)
	{
		return TimeSpanParse.Parse(input, formatProvider);
	}

	/// <summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.</summary>
	/// <returns>A time interval that corresponds to <paramref name="input" />, as specified by <paramref name="format" /> and <paramref name="formatProvider" />.</returns>
	/// <param name="input">A string that specifies the time interval to convert.</param>
	/// <param name="format">A standard or custom format string that defines the required format of <paramref name="input" />.</param>
	/// <param name="formatProvider">An object that provides culture-specific formatting information.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="input" /> has an invalid format. </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="input" /> represents a number that is less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" />.-or- At least one of the days, hours, minutes, or seconds components in <paramref name="input" /> is outside its valid range. </exception>
	public static TimeSpan ParseExact(string input, string format, IFormatProvider formatProvider)
	{
		return TimeSpanParse.ParseExact(input, format, formatProvider, TimeSpanStyles.None);
	}

	/// <summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified array of format strings and culture-specific format information. The format of the string representation must match one of the specified formats exactly.</summary>
	/// <returns>A time interval that corresponds to <paramref name="input" />, as specified by <paramref name="formats" /> and <paramref name="formatProvider" />.</returns>
	/// <param name="input">A string that specifies the time interval to convert.</param>
	/// <param name="formats">A array of standard or custom format strings that defines the required format of <paramref name="input" />.</param>
	/// <param name="formatProvider">An object that provides culture-specific formatting information.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="input" /> has an invalid format. </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="input" /> represents a number that is less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" />.-or- At least one of the days, hours, minutes, or seconds components in <paramref name="input" /> is outside its valid range. </exception>
	public static TimeSpan ParseExact(string input, string[] formats, IFormatProvider formatProvider)
	{
		return TimeSpanParse.ParseExactMultiple(input, formats, formatProvider, TimeSpanStyles.None);
	}

	/// <summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified format, culture-specific format information, and styles. The format of the string representation must match the specified format exactly.</summary>
	/// <returns>A time interval that corresponds to <paramref name="input" />, as specified by <paramref name="format" />, <paramref name="formatProvider" />, and <paramref name="styles" />.</returns>
	/// <param name="input">A string that specifies the time interval to convert.</param>
	/// <param name="format">A standard or custom format string that defines the required format of <paramref name="input" />.</param>
	/// <param name="formatProvider">An object that provides culture-specific formatting information.</param>
	/// <param name="styles">A bitwise combination of enumeration values that defines the style elements that may be present in <paramref name="input" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="styles" /> is an invalid <see cref="T:System.Globalization.TimeSpanStyles" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="input" /> has an invalid format. </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="input" /> represents a number that is less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" />.-or- At least one of the days, hours, minutes, or seconds components in <paramref name="input" /> is outside its valid range. </exception>
	public static TimeSpan ParseExact(string input, string format, IFormatProvider formatProvider, TimeSpanStyles styles)
	{
		TimeSpanParse.ValidateStyles(styles, "styles");
		return TimeSpanParse.ParseExact(input, format, formatProvider, styles);
	}

	/// <summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified formats, culture-specific format information, and styles. The format of the string representation must match one of the specified formats exactly. </summary>
	/// <returns>A time interval that corresponds to <paramref name="input" />, as specified by <paramref name="formats" />, <paramref name="formatProvider" />, and <paramref name="styles" />.</returns>
	/// <param name="input">A string that specifies the time interval to convert.</param>
	/// <param name="formats">A array of standard or custom format strings that define the required format of <paramref name="input" />.</param>
	/// <param name="formatProvider">An object that provides culture-specific formatting information.</param>
	/// <param name="styles">A bitwise combination of enumeration values that defines the style elements that may be present in input.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="styles" /> is an invalid <see cref="T:System.Globalization.TimeSpanStyles" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="input" /> has an invalid format. </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="input" /> represents a number that is less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" />.-or- At least one of the days, hours, minutes, or seconds components in <paramref name="input" /> is outside its valid range. </exception>
	public static TimeSpan ParseExact(string input, string[] formats, IFormatProvider formatProvider, TimeSpanStyles styles)
	{
		TimeSpanParse.ValidateStyles(styles, "styles");
		return TimeSpanParse.ParseExactMultiple(input, formats, formatProvider, styles);
	}

	/// <summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	/// <returns>true if <paramref name="s" /> was converted successfully; otherwise, false. This operation returns false if the <paramref name="s" /> parameter is null or <see cref="F:System.String.Empty" />, has an invalid format, represents a time interval that is less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" />, or has at least one days, hours, minutes, or seconds component outside its valid range.</returns>
	/// <param name="s">A string that specifies the time interval to convert.</param>
	/// <param name="result">When this method returns, contains an object that represents the time interval specified by <paramref name="s" />, or <see cref="F:System.TimeSpan.Zero" /> if the conversion failed. This parameter is passed uninitialized.</param>
	/// <filterpriority>1</filterpriority>
	public static bool TryParse(string s, out TimeSpan result)
	{
		return TimeSpanParse.TryParse(s, null, out result);
	}

	/// <summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified culture-specific formatting information, and returns a value that indicates whether the conversion succeeded.</summary>
	/// <returns>true if <paramref name="input" /> was converted successfully; otherwise, false. This operation returns false if the <paramref name="input" /> parameter is null or <see cref="F:System.String.Empty" />, has an invalid format, represents a time interval that is less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" />, or has at least one days, hours, minutes, or seconds component outside its valid range.</returns>
	/// <param name="input">A string that specifies the time interval to convert.</param>
	/// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
	/// <param name="result">When this method returns, contains an object that represents the time interval specified by <paramref name="input" />, or <see cref="F:System.TimeSpan.Zero" /> if the conversion failed. This parameter is passed uninitialized.</param>
	public static bool TryParse(string input, IFormatProvider formatProvider, out TimeSpan result)
	{
		return TimeSpanParse.TryParse(input, formatProvider, out result);
	}

	/// <summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified format and culture-specific format information, and returns a value that indicates whether the conversion succeeded. The format of the string representation must match the specified format exactly. </summary>
	/// <returns>true if <paramref name="input" /> was converted successfully; otherwise, false.</returns>
	/// <param name="input">A string that specifies the time interval to convert.</param>
	/// <param name="format">A standard or custom format string that defines the required format of <paramref name="input" />.</param>
	/// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
	/// <param name="result">When this method returns, contains an object that represents the time interval specified by <paramref name="input" />, or <see cref="F:System.TimeSpan.Zero" /> if the conversion failed. This parameter is passed uninitialized.</param>
	public static bool TryParseExact(string input, string format, IFormatProvider formatProvider, out TimeSpan result)
	{
		return TimeSpanParse.TryParseExact(input, format, formatProvider, TimeSpanStyles.None, out result);
	}

	/// <summary>Converts the specified string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified formats and culture-specific format information, and returns a value that indicates whether the conversion succeeded. The format of the string representation must match one of the specified formats exactly. </summary>
	/// <returns>true if <paramref name="input" /> was converted successfully; otherwise, false.</returns>
	/// <param name="input">A string that specifies the time interval to convert.</param>
	/// <param name="formats">A array of standard or custom format strings that define the acceptable formats of <paramref name="input" />.</param>
	/// <param name="formatProvider">An object that provides culture-specific formatting information.</param>
	/// <param name="result">When this method returns, contains an object that represents the time interval specified by <paramref name="input" />, or <see cref="F:System.TimeSpan.Zero" /> if the conversion failed. This parameter is passed uninitialized.</param>
	public static bool TryParseExact(string input, string[] formats, IFormatProvider formatProvider, out TimeSpan result)
	{
		return TimeSpanParse.TryParseExactMultiple(input, formats, formatProvider, TimeSpanStyles.None, out result);
	}

	/// <summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified format, culture-specific format information, and styles, and returns a value that indicates whether the conversion succeeded. The format of the string representation must match the specified format exactly. </summary>
	/// <returns>true if <paramref name="input" /> was converted successfully; otherwise, false.</returns>
	/// <param name="input">A string that specifies the time interval to convert.</param>
	/// <param name="format">A standard or custom format string that defines the required format of <paramref name="input" />.</param>
	/// <param name="formatProvider">An object that provides culture-specific formatting information.</param>
	/// <param name="styles">One or more enumeration values that indicate the style of <paramref name="input" />.</param>
	/// <param name="result">When this method returns, contains an object that represents the time interval specified by <paramref name="input" />, or <see cref="F:System.TimeSpan.Zero" /> if the conversion failed. This parameter is passed uninitialized.</param>
	public static bool TryParseExact(string input, string format, IFormatProvider formatProvider, TimeSpanStyles styles, out TimeSpan result)
	{
		TimeSpanParse.ValidateStyles(styles, "styles");
		return TimeSpanParse.TryParseExact(input, format, formatProvider, styles, out result);
	}

	/// <summary>Converts the specified string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified formats, culture-specific format information, and styles, and returns a value that indicates whether the conversion succeeded. The format of the string representation must match one of the specified formats exactly. </summary>
	/// <returns>true if <paramref name="input" /> was converted successfully; otherwise, false.</returns>
	/// <param name="input">A string that specifies the time interval to convert.</param>
	/// <param name="formats">A array of standard or custom format strings that define the acceptable formats of <paramref name="input" />.</param>
	/// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
	/// <param name="styles">One or more enumeration values that indicate the style of <paramref name="input" />.</param>
	/// <param name="result">When this method returns, contains an object that represents the time interval specified by <paramref name="input" />, or <see cref="F:System.TimeSpan.Zero" /> if the conversion failed. This parameter is passed uninitialized.</param>
	public static bool TryParseExact(string input, string[] formats, IFormatProvider formatProvider, TimeSpanStyles styles, out TimeSpan result)
	{
		TimeSpanParse.ValidateStyles(styles, "styles");
		return TimeSpanParse.TryParseExactMultiple(input, formats, formatProvider, styles, out result);
	}

	/// <summary>Converts the value of the current <see cref="T:System.TimeSpan" /> object to its equivalent string representation.</summary>
	/// <returns>The string representation of the current <see cref="T:System.TimeSpan" /> value. </returns>
	/// <filterpriority>1</filterpriority>
	public override string ToString()
	{
		return TimeSpanFormat.Format(this, null, null);
	}

	/// <summary>Converts the value of the current <see cref="T:System.TimeSpan" /> object to its equivalent string representation by using the specified format.</summary>
	/// <returns>The string representation of the current <see cref="T:System.TimeSpan" /> value in the format specified by the <paramref name="format" /> parameter.</returns>
	/// <param name="format">A standard or custom <see cref="T:System.TimeSpan" /> format string.</param>
	/// <exception cref="T:System.FormatException">The <paramref name="format" /> parameter is not recognized or is not supported.</exception>
	public string ToString(string format)
	{
		return TimeSpanFormat.Format(this, format, null);
	}

	/// <summary>Converts the value of the current <see cref="T:System.TimeSpan" /> object to its equivalent string representation by using the specified format and culture-specific formatting information.</summary>
	/// <returns>The string representation of the current <see cref="T:System.TimeSpan" /> value, as specified by <paramref name="format" /> and <paramref name="formatProvider" />.</returns>
	/// <param name="format">A standard or custom <see cref="T:System.TimeSpan" /> format string.</param>
	/// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
	/// <exception cref="T:System.FormatException">The <paramref name="format" /> parameter is not recognized or is not supported.</exception>
	public string ToString(string format, IFormatProvider formatProvider)
	{
		if (LegacyMode)
		{
			return TimeSpanFormat.Format(this, null, null);
		}
		return TimeSpanFormat.Format(this, format, formatProvider);
	}

	/// <summary>Returns a <see cref="T:System.TimeSpan" /> whose value is the negated value of the specified instance.</summary>
	/// <returns>An object that has the same numeric value as this instance, but the opposite sign.</returns>
	/// <param name="t">The time interval to be negated. </param>
	/// <exception cref="T:System.OverflowException">The negated value of this instance cannot be represented by a <see cref="T:System.TimeSpan" />; that is, the value of this instance is <see cref="F:System.TimeSpan.MinValue" />. </exception>
	/// <filterpriority>3</filterpriority>
	public static TimeSpan operator -(TimeSpan t)
	{
		if (t._ticks == MinValue._ticks)
		{
			throw new OverflowException(Environment.GetResourceString("Negating the minimum value of a twos complement number is invalid."));
		}
		return new TimeSpan(-t._ticks);
	}

	/// <summary>Subtracts a specified <see cref="T:System.TimeSpan" /> from another specified <see cref="T:System.TimeSpan" />.</summary>
	/// <returns>An object whose value is the result of the value of <paramref name="t1" /> minus the value of <paramref name="t2" />.</returns>
	/// <param name="t1">The minuend. </param>
	/// <param name="t2">The subtrahend. </param>
	/// <exception cref="T:System.OverflowException">The return value is less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" />. </exception>
	/// <filterpriority>3</filterpriority>
	public static TimeSpan operator -(TimeSpan t1, TimeSpan t2)
	{
		return t1.Subtract(t2);
	}

	/// <summary>Returns the specified instance of <see cref="T:System.TimeSpan" />.</summary>
	/// <returns>The time interval specified by <paramref name="t" />.</returns>
	/// <param name="t">The time interval to return. </param>
	/// <filterpriority>3</filterpriority>
	public static TimeSpan operator +(TimeSpan t)
	{
		return t;
	}

	/// <summary>Adds two specified <see cref="T:System.TimeSpan" /> instances.</summary>
	/// <returns>An object whose value is the sum of the values of <paramref name="t1" /> and <paramref name="t2" />.</returns>
	/// <param name="t1">The first time interval to add. </param>
	/// <param name="t2">The second time interval to add.</param>
	/// <exception cref="T:System.OverflowException">The resulting <see cref="T:System.TimeSpan" /> is less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" />. </exception>
	/// <filterpriority>3</filterpriority>
	public static TimeSpan operator +(TimeSpan t1, TimeSpan t2)
	{
		return t1.Add(t2);
	}

	/// <summary>Indicates whether two <see cref="T:System.TimeSpan" /> instances are equal.</summary>
	/// <returns>true if the values of <paramref name="t1" /> and <paramref name="t2" /> are equal; otherwise, false.</returns>
	/// <param name="t1">The first time interval to compare. </param>
	/// <param name="t2">The second time interval to compare. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator ==(TimeSpan t1, TimeSpan t2)
	{
		return t1._ticks == t2._ticks;
	}

	/// <summary>Indicates whether two <see cref="T:System.TimeSpan" /> instances are not equal.</summary>
	/// <returns>true if the values of <paramref name="t1" /> and <paramref name="t2" /> are not equal; otherwise, false.</returns>
	/// <param name="t1">The first time interval to compare.</param>
	/// <param name="t2">The second time interval to compare.</param>
	/// <filterpriority>3</filterpriority>
	public static bool operator !=(TimeSpan t1, TimeSpan t2)
	{
		return t1._ticks != t2._ticks;
	}

	/// <summary>Indicates whether a specified <see cref="T:System.TimeSpan" /> is less than another specified <see cref="T:System.TimeSpan" />.</summary>
	/// <returns>true if the value of <paramref name="t1" /> is less than the value of <paramref name="t2" />; otherwise, false.</returns>
	/// <param name="t1">The first time interval to compare.</param>
	/// <param name="t2">The second time interval to compare. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator <(TimeSpan t1, TimeSpan t2)
	{
		return t1._ticks < t2._ticks;
	}

	/// <summary>Indicates whether a specified <see cref="T:System.TimeSpan" /> is less than or equal to another specified <see cref="T:System.TimeSpan" />.</summary>
	/// <returns>true if the value of <paramref name="t1" /> is less than or equal to the value of <paramref name="t2" />; otherwise, false.</returns>
	/// <param name="t1">The first time interval to compare. </param>
	/// <param name="t2">The second time interval to compare. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator <=(TimeSpan t1, TimeSpan t2)
	{
		return t1._ticks <= t2._ticks;
	}

	/// <summary>Indicates whether a specified <see cref="T:System.TimeSpan" /> is greater than another specified <see cref="T:System.TimeSpan" />.</summary>
	/// <returns>true if the value of <paramref name="t1" /> is greater than the value of <paramref name="t2" />; otherwise, false.</returns>
	/// <param name="t1">The first time interval to compare. </param>
	/// <param name="t2">The second time interval to compare. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator >(TimeSpan t1, TimeSpan t2)
	{
		return t1._ticks > t2._ticks;
	}

	/// <summary>Indicates whether a specified <see cref="T:System.TimeSpan" /> is greater than or equal to another specified <see cref="T:System.TimeSpan" />.</summary>
	/// <returns>true if the value of <paramref name="t1" /> is greater than or equal to the value of <paramref name="t2" />; otherwise, false.</returns>
	/// <param name="t1">The first time interval to compare.</param>
	/// <param name="t2">The second time interval to compare.</param>
	/// <filterpriority>3</filterpriority>
	public static bool operator >=(TimeSpan t1, TimeSpan t2)
	{
		return t1._ticks >= t2._ticks;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private static extern bool LegacyFormatMode();

	[SecuritySafeCritical]
	private static bool GetLegacyFormatMode()
	{
		return CompatibilitySwitches.IsAppEarlierThanSilverlight4;
	}
}
