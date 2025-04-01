using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace System;

/// <summary>Represents an instant in time, typically expressed as a date and time of day. </summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[StructLayout(LayoutKind.Auto)]
public struct DateTime : IComparable, IFormattable, IConvertible, ISerializable, IComparable<DateTime>, IEquatable<DateTime>
{
	private const long TicksPerMillisecond = 10000L;

	private const long TicksPerSecond = 10000000L;

	private const long TicksPerMinute = 600000000L;

	private const long TicksPerHour = 36000000000L;

	private const long TicksPerDay = 864000000000L;

	private const int MillisPerSecond = 1000;

	private const int MillisPerMinute = 60000;

	private const int MillisPerHour = 3600000;

	private const int MillisPerDay = 86400000;

	private const int DaysPerYear = 365;

	private const int DaysPer4Years = 1461;

	private const int DaysPer100Years = 36524;

	private const int DaysPer400Years = 146097;

	private const int DaysTo1601 = 584388;

	private const int DaysTo1899 = 693593;

	internal const int DaysTo1970 = 719162;

	private const int DaysTo10000 = 3652059;

	internal const long MinTicks = 0L;

	internal const long MaxTicks = 3155378975999999999L;

	private const long MaxMillis = 315537897600000L;

	private const long FileTimeOffset = 504911232000000000L;

	private const long DoubleDateOffset = 599264352000000000L;

	private const long OADateMinAsTicks = 31241376000000000L;

	private const double OADateMinAsDouble = -657435.0;

	private const double OADateMaxAsDouble = 2958466.0;

	private const int DatePartYear = 0;

	private const int DatePartDayOfYear = 1;

	private const int DatePartMonth = 2;

	private const int DatePartDay = 3;

	private static readonly int[] DaysToMonth365 = new int[13]
	{
		0, 31, 59, 90, 120, 151, 181, 212, 243, 273,
		304, 334, 365
	};

	private static readonly int[] DaysToMonth366 = new int[13]
	{
		0, 31, 60, 91, 121, 152, 182, 213, 244, 274,
		305, 335, 366
	};

	/// <summary>Represents the smallest possible value of <see cref="T:System.DateTime" />. This field is read-only.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly DateTime MinValue = new DateTime(0L, DateTimeKind.Unspecified);

	/// <summary>Represents the largest possible value of <see cref="T:System.DateTime" />. This field is read-only.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly DateTime MaxValue = new DateTime(3155378975999999999L, DateTimeKind.Unspecified);

	private const ulong TicksMask = 4611686018427387903uL;

	private const ulong FlagsMask = 13835058055282163712uL;

	private const ulong LocalMask = 9223372036854775808uL;

	private const long TicksCeiling = 4611686018427387904L;

	private const ulong KindUnspecified = 0uL;

	private const ulong KindUtc = 4611686018427387904uL;

	private const ulong KindLocal = 9223372036854775808uL;

	private const ulong KindLocalAmbiguousDst = 13835058055282163712uL;

	private const int KindShift = 62;

	private const string TicksField = "ticks";

	private const string DateDataField = "dateData";

	private ulong dateData;

	internal long InternalTicks => (long)(dateData & 0x3FFFFFFFFFFFFFFFL);

	private ulong InternalKind => dateData & 0xC000000000000000uL;

	/// <summary>Gets the date component of this instance.</summary>
	/// <returns>A new object with the same date as this instance, and the time value set to 12:00:00 midnight (00:00:00).</returns>
	/// <filterpriority>1</filterpriority>
	public DateTime Date
	{
		get
		{
			long internalTicks = InternalTicks;
			return new DateTime((ulong)(internalTicks - internalTicks % 864000000000L) | InternalKind);
		}
	}

	/// <summary>Gets the day of the month represented by this instance.</summary>
	/// <returns>The day component, expressed as a value between 1 and 31.</returns>
	/// <filterpriority>1</filterpriority>
	public int Day => GetDatePart(3);

	/// <summary>Gets the day of the week represented by this instance.</summary>
	/// <returns>An enumerated constant that indicates the day of the week of this <see cref="T:System.DateTime" /> value. </returns>
	/// <filterpriority>1</filterpriority>
	public DayOfWeek DayOfWeek => (DayOfWeek)((InternalTicks / 864000000000L + 1) % 7);

	/// <summary>Gets the day of the year represented by this instance.</summary>
	/// <returns>The day of the year, expressed as a value between 1 and 366.</returns>
	/// <filterpriority>1</filterpriority>
	public int DayOfYear => GetDatePart(1);

	/// <summary>Gets the hour component of the date represented by this instance.</summary>
	/// <returns>The hour component, expressed as a value between 0 and 23.</returns>
	/// <filterpriority>1</filterpriority>
	public int Hour => (int)(InternalTicks / 36000000000L % 24);

	/// <summary>Gets a value that indicates whether the time represented by this instance is based on local time, Coordinated Universal Time (UTC), or neither.</summary>
	/// <returns>One of the enumeration values that indicates what the current time represents. The default is <see cref="F:System.DateTimeKind.Unspecified" />.</returns>
	/// <filterpriority>1</filterpriority>
	public DateTimeKind Kind => InternalKind switch
	{
		0uL => DateTimeKind.Unspecified, 
		4611686018427387904uL => DateTimeKind.Utc, 
		_ => DateTimeKind.Local, 
	};

	/// <summary>Gets the milliseconds component of the date represented by this instance.</summary>
	/// <returns>The milliseconds component, expressed as a value between 0 and 999.</returns>
	/// <filterpriority>1</filterpriority>
	public int Millisecond => (int)(InternalTicks / 10000 % 1000);

	/// <summary>Gets the minute component of the date represented by this instance.</summary>
	/// <returns>The minute component, expressed as a value between 0 and 59.</returns>
	/// <filterpriority>1</filterpriority>
	public int Minute => (int)(InternalTicks / 600000000 % 60);

	/// <summary>Gets the month component of the date represented by this instance.</summary>
	/// <returns>The month component, expressed as a value between 1 and 12.</returns>
	/// <filterpriority>1</filterpriority>
	public int Month => GetDatePart(2);

	/// <summary>Gets a <see cref="T:System.DateTime" /> object that is set to the current date and time on this computer, expressed as the local time.</summary>
	/// <returns>An object whose value is the current local date and time.</returns>
	/// <filterpriority>1</filterpriority>
	public static DateTime Now
	{
		get
		{
			DateTime utcNow = UtcNow;
			bool isAmbiguousLocalDst = false;
			long ticks = TimeZoneInfo.GetDateTimeNowUtcOffsetFromUtc(utcNow, out isAmbiguousLocalDst).Ticks;
			long num = utcNow.Ticks + ticks;
			if (num > 3155378975999999999L)
			{
				return new DateTime(3155378975999999999L, DateTimeKind.Local);
			}
			if (num < 0)
			{
				return new DateTime(0L, DateTimeKind.Local);
			}
			return new DateTime(num, DateTimeKind.Local, isAmbiguousLocalDst);
		}
	}

	/// <summary>Gets a <see cref="T:System.DateTime" /> object that is set to the current date and time on this computer, expressed as the Coordinated Universal Time (UTC).</summary>
	/// <returns>An object whose value is the current UTC date and time.</returns>
	/// <filterpriority>1</filterpriority>
	public static DateTime UtcNow
	{
		[SecuritySafeCritical]
		get
		{
			return new DateTime((ulong)((GetSystemTimeAsFileTime() + 504911232000000000L) | 0x4000000000000000L));
		}
	}

	/// <summary>Gets the seconds component of the date represented by this instance.</summary>
	/// <returns>The seconds component, expressed as a value between 0 and 59.</returns>
	/// <filterpriority>1</filterpriority>
	public int Second => (int)(InternalTicks / 10000000 % 60);

	/// <summary>Gets the number of ticks that represent the date and time of this instance.</summary>
	/// <returns>The number of ticks that represent the date and time of this instance. The value is between DateTime.MinValue.Ticks and DateTime.MaxValue.Ticks.</returns>
	/// <filterpriority>1</filterpriority>
	public long Ticks => InternalTicks;

	/// <summary>Gets the time of day for this instance.</summary>
	/// <returns>A time interval that represents the fraction of the day that has elapsed since midnight.</returns>
	/// <filterpriority>1</filterpriority>
	public TimeSpan TimeOfDay => new TimeSpan(InternalTicks % 864000000000L);

	/// <summary>Gets the current date.</summary>
	/// <returns>An object that is set to today's date, with the time component set to 00:00:00.</returns>
	/// <filterpriority>1</filterpriority>
	public static DateTime Today => Now.Date;

	/// <summary>Gets the year component of the date represented by this instance.</summary>
	/// <returns>The year, between 1 and 9999.</returns>
	/// <filterpriority>1</filterpriority>
	public int Year => GetDatePart(0);

	/// <summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to a specified number of ticks.</summary>
	/// <param name="ticks">A date and time expressed in the number of 100-nanosecond intervals that have elapsed since January 1, 0001 at 00:00:00.000 in the Gregorian calendar. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="ticks" /> is less than <see cref="F:System.DateTime.MinValue" /> or greater than <see cref="F:System.DateTime.MaxValue" />. </exception>
	public DateTime(long ticks)
	{
		if (ticks < 0 || ticks > 3155378975999999999L)
		{
			throw new ArgumentOutOfRangeException("ticks", Environment.GetResourceString("Ticks must be between DateTime.MinValue.Ticks and DateTime.MaxValue.Ticks."));
		}
		dateData = (ulong)ticks;
	}

	private DateTime(ulong dateData)
	{
		this.dateData = dateData;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to a specified number of ticks and to Coordinated Universal Time (UTC) or local time.</summary>
	/// <param name="ticks">A date and time expressed in the number of 100-nanosecond intervals that have elapsed since January 1, 0001 at 00:00:00.000 in the Gregorian calendar. </param>
	/// <param name="kind">One of the enumeration values that indicates whether <paramref name="ticks" /> specifies a local time, Coordinated Universal Time (UTC), or neither.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="ticks" /> is less than <see cref="F:System.DateTime.MinValue" /> or greater than <see cref="F:System.DateTime.MaxValue" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="kind" /> is not one of the <see cref="T:System.DateTimeKind" /> values.</exception>
	public DateTime(long ticks, DateTimeKind kind)
	{
		if (ticks < 0 || ticks > 3155378975999999999L)
		{
			throw new ArgumentOutOfRangeException("ticks", Environment.GetResourceString("Ticks must be between DateTime.MinValue.Ticks and DateTime.MaxValue.Ticks."));
		}
		if (kind < DateTimeKind.Unspecified || kind > DateTimeKind.Local)
		{
			throw new ArgumentException(Environment.GetResourceString("Invalid DateTimeKind value."), "kind");
		}
		dateData = (ulong)(ticks | ((long)kind << 62));
	}

	internal DateTime(long ticks, DateTimeKind kind, bool isAmbiguousDst)
	{
		if (ticks < 0 || ticks > 3155378975999999999L)
		{
			throw new ArgumentOutOfRangeException("ticks", Environment.GetResourceString("Ticks must be between DateTime.MinValue.Ticks and DateTime.MaxValue.Ticks."));
		}
		dateData = (ulong)(ticks | (isAmbiguousDst ? (-4611686018427387904L) : long.MinValue));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, and day.</summary>
	/// <param name="year">The year (1 through 9999). </param>
	/// <param name="month">The month (1 through 12). </param>
	/// <param name="day">The day (1 through the number of days in <paramref name="month" />). </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is less than 1 or greater than 9999.-or- <paramref name="month" /> is less than 1 or greater than 12.-or- <paramref name="day" /> is less than 1 or greater than the number of days in <paramref name="month" />. </exception>
	public DateTime(int year, int month, int day)
	{
		dateData = (ulong)DateToTicks(year, month, day);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, and day for the specified calendar.</summary>
	/// <param name="year">The year (1 through the number of years in <paramref name="calendar" />). </param>
	/// <param name="month">The month (1 through the number of months in <paramref name="calendar" />). </param>
	/// <param name="day">The day (1 through the number of days in <paramref name="month" />). </param>
	/// <param name="calendar">The calendar that is used to interpret <paramref name="year" />, <paramref name="month" />, and <paramref name="day" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="calendar" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is not in the range supported by <paramref name="calendar" />.-or- <paramref name="month" /> is less than 1 or greater than the number of months in <paramref name="calendar" />.-or- <paramref name="day" /> is less than 1 or greater than the number of days in <paramref name="month" />. </exception>
	public DateTime(int year, int month, int day, Calendar calendar)
		: this(year, month, day, 0, 0, 0, calendar)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, and second.</summary>
	/// <param name="year">The year (1 through 9999). </param>
	/// <param name="month">The month (1 through 12). </param>
	/// <param name="day">The day (1 through the number of days in <paramref name="month" />). </param>
	/// <param name="hour">The hours (0 through 23). </param>
	/// <param name="minute">The minutes (0 through 59). </param>
	/// <param name="second">The seconds (0 through 59). </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is less than 1 or greater than 9999. -or- <paramref name="month" /> is less than 1 or greater than 12. -or- <paramref name="day" /> is less than 1 or greater than the number of days in <paramref name="month" />.-or- <paramref name="hour" /> is less than 0 or greater than 23. -or- <paramref name="minute" /> is less than 0 or greater than 59. -or- <paramref name="second" /> is less than 0 or greater than 59. </exception>
	public DateTime(int year, int month, int day, int hour, int minute, int second)
	{
		dateData = (ulong)(DateToTicks(year, month, day) + TimeToTicks(hour, minute, second));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, and Coordinated Universal Time (UTC) or local time.</summary>
	/// <param name="year">The year (1 through 9999). </param>
	/// <param name="month">The month (1 through 12). </param>
	/// <param name="day">The day (1 through the number of days in <paramref name="month" />). </param>
	/// <param name="hour">The hours (0 through 23). </param>
	/// <param name="minute">The minutes (0 through 59). </param>
	/// <param name="second">The seconds (0 through 59). </param>
	/// <param name="kind">One of the enumeration values that indicates whether <paramref name="year" />, <paramref name="month" />, <paramref name="day" />, <paramref name="hour" />, <paramref name="minute" /> and <paramref name="second" /> specify a local time, Coordinated Universal Time (UTC), or neither.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is less than 1 or greater than 9999. -or- <paramref name="month" /> is less than 1 or greater than 12. -or- <paramref name="day" /> is less than 1 or greater than the number of days in <paramref name="month" />.-or- <paramref name="hour" /> is less than 0 or greater than 23. -or- <paramref name="minute" /> is less than 0 or greater than 59. -or- <paramref name="second" /> is less than 0 or greater than 59. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="kind" /> is not one of the <see cref="T:System.DateTimeKind" /> values.</exception>
	public DateTime(int year, int month, int day, int hour, int minute, int second, DateTimeKind kind)
	{
		if (kind < DateTimeKind.Unspecified || kind > DateTimeKind.Local)
		{
			throw new ArgumentException(Environment.GetResourceString("Invalid DateTimeKind value."), "kind");
		}
		long num = DateToTicks(year, month, day) + TimeToTicks(hour, minute, second);
		dateData = (ulong)(num | ((long)kind << 62));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, and second for the specified calendar.</summary>
	/// <param name="year">The year (1 through the number of years in <paramref name="calendar" />). </param>
	/// <param name="month">The month (1 through the number of months in <paramref name="calendar" />). </param>
	/// <param name="day">The day (1 through the number of days in <paramref name="month" />). </param>
	/// <param name="hour">The hours (0 through 23). </param>
	/// <param name="minute">The minutes (0 through 59). </param>
	/// <param name="second">The seconds (0 through 59). </param>
	/// <param name="calendar">The calendar that is used to interpret <paramref name="year" />, <paramref name="month" />, and <paramref name="day" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="calendar" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is not in the range supported by <paramref name="calendar" />.-or- <paramref name="month" /> is less than 1 or greater than the number of months in <paramref name="calendar" />.-or- <paramref name="day" /> is less than 1 or greater than the number of days in <paramref name="month" />.-or- <paramref name="hour" /> is less than 0 or greater than 23 -or- <paramref name="minute" /> is less than 0 or greater than 59. -or- <paramref name="second" /> is less than 0 or greater than 59. </exception>
	public DateTime(int year, int month, int day, int hour, int minute, int second, Calendar calendar)
	{
		if (calendar == null)
		{
			throw new ArgumentNullException("calendar");
		}
		dateData = (ulong)calendar.ToDateTime(year, month, day, hour, minute, second, 0).Ticks;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, and millisecond.</summary>
	/// <param name="year">The year (1 through 9999). </param>
	/// <param name="month">The month (1 through 12). </param>
	/// <param name="day">The day (1 through the number of days in <paramref name="month" />). </param>
	/// <param name="hour">The hours (0 through 23). </param>
	/// <param name="minute">The minutes (0 through 59). </param>
	/// <param name="second">The seconds (0 through 59). </param>
	/// <param name="millisecond">The milliseconds (0 through 999). </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is less than 1 or greater than 9999.-or- <paramref name="month" /> is less than 1 or greater than 12.-or- <paramref name="day" /> is less than 1 or greater than the number of days in <paramref name="month" />.-or- <paramref name="hour" /> is less than 0 or greater than 23.-or- <paramref name="minute" /> is less than 0 or greater than 59.-or- <paramref name="second" /> is less than 0 or greater than 59.-or- <paramref name="millisecond" /> is less than 0 or greater than 999. </exception>
	public DateTime(int year, int month, int day, int hour, int minute, int second, int millisecond)
	{
		if (millisecond < 0 || millisecond >= 1000)
		{
			throw new ArgumentOutOfRangeException("millisecond", Environment.GetResourceString("Valid values are between {0} and {1}, inclusive.", 0, 999));
		}
		long num = DateToTicks(year, month, day) + TimeToTicks(hour, minute, second);
		num += (long)millisecond * 10000L;
		if (num < 0 || num > 3155378975999999999L)
		{
			throw new ArgumentException(Environment.GetResourceString("Combination of arguments to the DateTime constructor is out of the legal range."));
		}
		dateData = (ulong)num;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time.</summary>
	/// <param name="year">The year (1 through 9999). </param>
	/// <param name="month">The month (1 through 12). </param>
	/// <param name="day">The day (1 through the number of days in <paramref name="month" />). </param>
	/// <param name="hour">The hours (0 through 23). </param>
	/// <param name="minute">The minutes (0 through 59). </param>
	/// <param name="second">The seconds (0 through 59). </param>
	/// <param name="millisecond">The milliseconds (0 through 999). </param>
	/// <param name="kind">One of the enumeration values that indicates whether <paramref name="year" />, <paramref name="month" />, <paramref name="day" />, <paramref name="hour" />, <paramref name="minute" />, <paramref name="second" />, and <paramref name="millisecond" /> specify a local time, Coordinated Universal Time (UTC), or neither.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is less than 1 or greater than 9999.-or- <paramref name="month" /> is less than 1 or greater than 12.-or- <paramref name="day" /> is less than 1 or greater than the number of days in <paramref name="month" />.-or- <paramref name="hour" /> is less than 0 or greater than 23.-or- <paramref name="minute" /> is less than 0 or greater than 59.-or- <paramref name="second" /> is less than 0 or greater than 59.-or- <paramref name="millisecond" /> is less than 0 or greater than 999. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="kind" /> is not one of the <see cref="T:System.DateTimeKind" /> values.</exception>
	public DateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, DateTimeKind kind)
	{
		if (millisecond < 0 || millisecond >= 1000)
		{
			throw new ArgumentOutOfRangeException("millisecond", Environment.GetResourceString("Valid values are between {0} and {1}, inclusive.", 0, 999));
		}
		if (kind < DateTimeKind.Unspecified || kind > DateTimeKind.Local)
		{
			throw new ArgumentException(Environment.GetResourceString("Invalid DateTimeKind value."), "kind");
		}
		long num = DateToTicks(year, month, day) + TimeToTicks(hour, minute, second);
		num += (long)millisecond * 10000L;
		if (num < 0 || num > 3155378975999999999L)
		{
			throw new ArgumentException(Environment.GetResourceString("Combination of arguments to the DateTime constructor is out of the legal range."));
		}
		dateData = (ulong)(num | ((long)kind << 62));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, and millisecond for the specified calendar.</summary>
	/// <param name="year">The year (1 through the number of years in <paramref name="calendar" />). </param>
	/// <param name="month">The month (1 through the number of months in <paramref name="calendar" />). </param>
	/// <param name="day">The day (1 through the number of days in <paramref name="month" />). </param>
	/// <param name="hour">The hours (0 through 23). </param>
	/// <param name="minute">The minutes (0 through 59). </param>
	/// <param name="second">The seconds (0 through 59). </param>
	/// <param name="millisecond">The milliseconds (0 through 999). </param>
	/// <param name="calendar">The calendar that is used to interpret <paramref name="year" />, <paramref name="month" />, and <paramref name="day" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="calendar" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is not in the range supported by <paramref name="calendar" />.-or- <paramref name="month" /> is less than 1 or greater than the number of months in <paramref name="calendar" />.-or- <paramref name="day" /> is less than 1 or greater than the number of days in <paramref name="month" />.-or- <paramref name="hour" /> is less than 0 or greater than 23.-or- <paramref name="minute" /> is less than 0 or greater than 59.-or- <paramref name="second" /> is less than 0 or greater than 59.-or- <paramref name="millisecond" /> is less than 0 or greater than 999. </exception>
	public DateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, Calendar calendar)
	{
		if (calendar == null)
		{
			throw new ArgumentNullException("calendar");
		}
		if (millisecond < 0 || millisecond >= 1000)
		{
			throw new ArgumentOutOfRangeException("millisecond", Environment.GetResourceString("Valid values are between {0} and {1}, inclusive.", 0, 999));
		}
		long ticks = calendar.ToDateTime(year, month, day, hour, minute, second, 0).Ticks;
		ticks += (long)millisecond * 10000L;
		if (ticks < 0 || ticks > 3155378975999999999L)
		{
			throw new ArgumentException(Environment.GetResourceString("Combination of arguments to the DateTime constructor is out of the legal range."));
		}
		dateData = (ulong)ticks;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time for the specified calendar.</summary>
	/// <param name="year">The year (1 through the number of years in <paramref name="calendar" />). </param>
	/// <param name="month">The month (1 through the number of months in <paramref name="calendar" />). </param>
	/// <param name="day">The day (1 through the number of days in <paramref name="month" />). </param>
	/// <param name="hour">The hours (0 through 23). </param>
	/// <param name="minute">The minutes (0 through 59). </param>
	/// <param name="second">The seconds (0 through 59). </param>
	/// <param name="millisecond">The milliseconds (0 through 999). </param>
	/// <param name="calendar">The calendar that is used to interpret <paramref name="year" />, <paramref name="month" />, and <paramref name="day" />.</param>
	/// <param name="kind">One of the enumeration values that indicates whether <paramref name="year" />, <paramref name="month" />, <paramref name="day" />, <paramref name="hour" />, <paramref name="minute" />, <paramref name="second" />, and <paramref name="millisecond" /> specify a local time, Coordinated Universal Time (UTC), or neither.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="calendar" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is not in the range supported by <paramref name="calendar" />.-or- <paramref name="month" /> is less than 1 or greater than the number of months in <paramref name="calendar" />.-or- <paramref name="day" /> is less than 1 or greater than the number of days in <paramref name="month" />.-or- <paramref name="hour" /> is less than 0 or greater than 23.-or- <paramref name="minute" /> is less than 0 or greater than 59.-or- <paramref name="second" /> is less than 0 or greater than 59.-or- <paramref name="millisecond" /> is less than 0 or greater than 999. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="kind" /> is not one of the <see cref="T:System.DateTimeKind" /> values.</exception>
	public DateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, Calendar calendar, DateTimeKind kind)
	{
		if (calendar == null)
		{
			throw new ArgumentNullException("calendar");
		}
		if (millisecond < 0 || millisecond >= 1000)
		{
			throw new ArgumentOutOfRangeException("millisecond", Environment.GetResourceString("Valid values are between {0} and {1}, inclusive.", 0, 999));
		}
		if (kind < DateTimeKind.Unspecified || kind > DateTimeKind.Local)
		{
			throw new ArgumentException(Environment.GetResourceString("Invalid DateTimeKind value."), "kind");
		}
		long ticks = calendar.ToDateTime(year, month, day, hour, minute, second, 0).Ticks;
		ticks += (long)millisecond * 10000L;
		if (ticks < 0 || ticks > 3155378975999999999L)
		{
			throw new ArgumentException(Environment.GetResourceString("Combination of arguments to the DateTime constructor is out of the legal range."));
		}
		dateData = (ulong)(ticks | ((long)kind << 62));
	}

	private DateTime(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		bool flag = false;
		bool flag2 = false;
		long num = 0L;
		ulong num2 = 0uL;
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			string name = enumerator.Name;
			if (!(name == "ticks"))
			{
				if (name == "dateData")
				{
					num2 = Convert.ToUInt64(enumerator.Value, CultureInfo.InvariantCulture);
					flag2 = true;
				}
			}
			else
			{
				num = Convert.ToInt64(enumerator.Value, CultureInfo.InvariantCulture);
				flag = true;
			}
		}
		if (flag2)
		{
			dateData = num2;
		}
		else
		{
			if (!flag)
			{
				throw new SerializationException(Environment.GetResourceString("Invalid serialized DateTime data. Unable to find 'ticks' or 'dateData'."));
			}
			dateData = (ulong)num;
		}
		long internalTicks = InternalTicks;
		if (internalTicks < 0 || internalTicks > 3155378975999999999L)
		{
			throw new SerializationException(Environment.GetResourceString("Invalid serialized DateTime data. Ticks must be between DateTime.MinValue.Ticks and DateTime.MaxValue.Ticks."));
		}
	}

	/// <summary>Returns a new <see cref="T:System.DateTime" /> that adds the value of the specified <see cref="T:System.TimeSpan" /> to the value of this instance.</summary>
	/// <returns>An object whose value is the sum of the date and time represented by this instance and the time interval represented by <paramref name="value" />.</returns>
	/// <param name="value">A positive or negative time interval. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTime" /> is less than <see cref="F:System.DateTime.MinValue" /> or greater than <see cref="F:System.DateTime.MaxValue" />. </exception>
	/// <filterpriority>2</filterpriority>
	public DateTime Add(TimeSpan value)
	{
		return AddTicks(value._ticks);
	}

	private DateTime Add(double value, int scale)
	{
		long num;
		try
		{
			num = checked((long)(value * (double)scale + ((value >= 0.0) ? 0.5 : (-0.5))));
		}
		catch (OverflowException)
		{
			throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("Value to add was out of range."));
		}
		if (num <= -315537897600000L || num >= 315537897600000L)
		{
			throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("Value to add was out of range."));
		}
		return AddTicks(num * 10000);
	}

	/// <summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of days to the value of this instance.</summary>
	/// <returns>An object whose value is the sum of the date and time represented by this instance and the number of days represented by <paramref name="value" />.</returns>
	/// <param name="value">A number of whole and fractional days. The <paramref name="value" /> parameter can be negative or positive. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTime" /> is less than <see cref="F:System.DateTime.MinValue" /> or greater than <see cref="F:System.DateTime.MaxValue" />. </exception>
	/// <filterpriority>2</filterpriority>
	public DateTime AddDays(double value)
	{
		return Add(value, 86400000);
	}

	/// <summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of hours to the value of this instance.</summary>
	/// <returns>An object whose value is the sum of the date and time represented by this instance and the number of hours represented by <paramref name="value" />.</returns>
	/// <param name="value">A number of whole and fractional hours. The <paramref name="value" /> parameter can be negative or positive. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTime" /> is less than <see cref="F:System.DateTime.MinValue" /> or greater than <see cref="F:System.DateTime.MaxValue" />. </exception>
	/// <filterpriority>2</filterpriority>
	public DateTime AddHours(double value)
	{
		return Add(value, 3600000);
	}

	/// <summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of milliseconds to the value of this instance.</summary>
	/// <returns>An object whose value is the sum of the date and time represented by this instance and the number of milliseconds represented by <paramref name="value" />.</returns>
	/// <param name="value">A number of whole and fractional milliseconds. The <paramref name="value" /> parameter can be negative or positive. Note that this value is rounded to the nearest integer.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTime" /> is less than <see cref="F:System.DateTime.MinValue" /> or greater than <see cref="F:System.DateTime.MaxValue" />. </exception>
	/// <filterpriority>2</filterpriority>
	public DateTime AddMilliseconds(double value)
	{
		return Add(value, 1);
	}

	/// <summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of minutes to the value of this instance.</summary>
	/// <returns>An object whose value is the sum of the date and time represented by this instance and the number of minutes represented by <paramref name="value" />.</returns>
	/// <param name="value">A number of whole and fractional minutes. The <paramref name="value" /> parameter can be negative or positive. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTime" /> is less than <see cref="F:System.DateTime.MinValue" /> or greater than <see cref="F:System.DateTime.MaxValue" />. </exception>
	/// <filterpriority>2</filterpriority>
	public DateTime AddMinutes(double value)
	{
		return Add(value, 60000);
	}

	/// <summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of months to the value of this instance.</summary>
	/// <returns>An object whose value is the sum of the date and time represented by this instance and <paramref name="months" />.</returns>
	/// <param name="months">A number of months. The <paramref name="months" /> parameter can be negative or positive. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTime" /> is less than <see cref="F:System.DateTime.MinValue" /> or greater than <see cref="F:System.DateTime.MaxValue" />.-or- <paramref name="months" /> is less than -120,000 or greater than 120,000. </exception>
	/// <filterpriority>2</filterpriority>
	public DateTime AddMonths(int months)
	{
		if (months < -120000 || months > 120000)
		{
			throw new ArgumentOutOfRangeException("months", Environment.GetResourceString("Months value must be between +/-120000."));
		}
		int datePart = GetDatePart(0);
		int datePart2 = GetDatePart(2);
		int num = GetDatePart(3);
		int num2 = datePart2 - 1 + months;
		if (num2 >= 0)
		{
			datePart2 = num2 % 12 + 1;
			datePart += num2 / 12;
		}
		else
		{
			datePart2 = 12 + (num2 + 1) % 12;
			datePart += (num2 - 11) / 12;
		}
		if (datePart < 1 || datePart > 9999)
		{
			throw new ArgumentOutOfRangeException("months", Environment.GetResourceString("The added or subtracted value results in an un-representable DateTime."));
		}
		int num3 = DaysInMonth(datePart, datePart2);
		if (num > num3)
		{
			num = num3;
		}
		return new DateTime((ulong)(DateToTicks(datePart, datePart2, num) + InternalTicks % 864000000000L) | InternalKind);
	}

	/// <summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of seconds to the value of this instance.</summary>
	/// <returns>An object whose value is the sum of the date and time represented by this instance and the number of seconds represented by <paramref name="value" />.</returns>
	/// <param name="value">A number of whole and fractional seconds. The <paramref name="value" /> parameter can be negative or positive. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTime" /> is less than <see cref="F:System.DateTime.MinValue" /> or greater than <see cref="F:System.DateTime.MaxValue" />. </exception>
	/// <filterpriority>2</filterpriority>
	public DateTime AddSeconds(double value)
	{
		return Add(value, 1000);
	}

	/// <summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of ticks to the value of this instance.</summary>
	/// <returns>An object whose value is the sum of the date and time represented by this instance and the time represented by <paramref name="value" />.</returns>
	/// <param name="value">A number of 100-nanosecond ticks. The <paramref name="value" /> parameter can be positive or negative. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTime" /> is less than <see cref="F:System.DateTime.MinValue" /> or greater than <see cref="F:System.DateTime.MaxValue" />. </exception>
	/// <filterpriority>2</filterpriority>
	public DateTime AddTicks(long value)
	{
		long internalTicks = InternalTicks;
		if (value > 3155378975999999999L - internalTicks || value < -internalTicks)
		{
			throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("The added or subtracted value results in an un-representable DateTime."));
		}
		return new DateTime((ulong)(internalTicks + value) | InternalKind);
	}

	/// <summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of years to the value of this instance.</summary>
	/// <returns>An object whose value is the sum of the date and time represented by this instance and the number of years represented by <paramref name="value" />.</returns>
	/// <param name="value">A number of years. The <paramref name="value" /> parameter can be negative or positive. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="value" /> or the resulting <see cref="T:System.DateTime" /> is less than <see cref="F:System.DateTime.MinValue" /> or greater than <see cref="F:System.DateTime.MaxValue" />. </exception>
	/// <filterpriority>2</filterpriority>
	public DateTime AddYears(int value)
	{
		if (value < -10000 || value > 10000)
		{
			throw new ArgumentOutOfRangeException("years", Environment.GetResourceString("Years value must be between +/-10000."));
		}
		return AddMonths(value * 12);
	}

	/// <summary>Compares two instances of <see cref="T:System.DateTime" /> and returns an integer that indicates whether the first instance is earlier than, the same as, or later than the second instance.</summary>
	/// <returns>A signed number indicating the relative values of <paramref name="t1" /> and <paramref name="t2" />.Value Type Condition Less than zero <paramref name="t1" /> is earlier than <paramref name="t2" />. Zero <paramref name="t1" /> is the same as <paramref name="t2" />. Greater than zero <paramref name="t1" /> is later than <paramref name="t2" />. </returns>
	/// <param name="t1">The first object to compare. </param>
	/// <param name="t2">The second object to compare. </param>
	/// <filterpriority>1</filterpriority>
	public static int Compare(DateTime t1, DateTime t2)
	{
		long internalTicks = t1.InternalTicks;
		long internalTicks2 = t2.InternalTicks;
		if (internalTicks > internalTicks2)
		{
			return 1;
		}
		if (internalTicks < internalTicks2)
		{
			return -1;
		}
		return 0;
	}

	/// <summary>Compares the value of this instance to a specified object that contains a specified <see cref="T:System.DateTime" /> value, and returns an integer that indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.DateTime" /> value.</summary>
	/// <returns>A signed number indicating the relative values of this instance and <paramref name="value" />.Value Description Less than zero This instance is earlier than <paramref name="value" />. Zero This instance is the same as <paramref name="value" />. Greater than zero This instance is later than <paramref name="value" />, or <paramref name="value" /> is null. </returns>
	/// <param name="value">A boxed object to compare, or null. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is not a <see cref="T:System.DateTime" />. </exception>
	/// <filterpriority>2</filterpriority>
	public int CompareTo(object value)
	{
		if (value == null)
		{
			return 1;
		}
		if (!(value is DateTime { InternalTicks: var internalTicks }))
		{
			throw new ArgumentException(Environment.GetResourceString("Object must be of type DateTime."));
		}
		long internalTicks2 = InternalTicks;
		if (internalTicks2 > internalTicks)
		{
			return 1;
		}
		if (internalTicks2 < internalTicks)
		{
			return -1;
		}
		return 0;
	}

	/// <summary>Compares the value of this instance to a specified <see cref="T:System.DateTime" /> value and returns an integer that indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.DateTime" /> value.</summary>
	/// <returns>A signed number indicating the relative values of this instance and the <paramref name="value" /> parameter.Value Description Less than zero This instance is earlier than <paramref name="value" />. Zero This instance is the same as <paramref name="value" />. Greater than zero This instance is later than <paramref name="value" />. </returns>
	/// <param name="value">The object to compare to the current instance. </param>
	/// <filterpriority>2</filterpriority>
	public int CompareTo(DateTime value)
	{
		long internalTicks = value.InternalTicks;
		long internalTicks2 = InternalTicks;
		if (internalTicks2 > internalTicks)
		{
			return 1;
		}
		if (internalTicks2 < internalTicks)
		{
			return -1;
		}
		return 0;
	}

	private static long DateToTicks(int year, int month, int day)
	{
		if (year >= 1 && year <= 9999 && month >= 1 && month <= 12)
		{
			int[] array = (IsLeapYear(year) ? DaysToMonth366 : DaysToMonth365);
			if (day >= 1 && day <= array[month] - array[month - 1])
			{
				int num = year - 1;
				return (num * 365 + num / 4 - num / 100 + num / 400 + array[month - 1] + day - 1) * 864000000000L;
			}
		}
		throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("Year, Month, and Day parameters describe an un-representable DateTime."));
	}

	private static long TimeToTicks(int hour, int minute, int second)
	{
		if (hour >= 0 && hour < 24 && minute >= 0 && minute < 60 && second >= 0 && second < 60)
		{
			return TimeSpan.TimeToTicks(hour, minute, second);
		}
		throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("Hour, Minute, and Second parameters describe an un-representable DateTime."));
	}

	/// <summary>Returns the number of days in the specified month and year.</summary>
	/// <returns>The number of days in <paramref name="month" /> for the specified <paramref name="year" />.For example, if <paramref name="month" /> equals 2 for February, the return value is 28 or 29 depending upon whether <paramref name="year" /> is a leap year.</returns>
	/// <param name="year">The year. </param>
	/// <param name="month">The month (a number ranging from 1 to 12). </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="month" /> is less than 1 or greater than 12.-or-<paramref name="year" /> is less than 1 or greater than 9999.</exception>
	/// <filterpriority>1</filterpriority>
	public static int DaysInMonth(int year, int month)
	{
		if (month < 1 || month > 12)
		{
			throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("Month must be between one and twelve."));
		}
		int[] array = (IsLeapYear(year) ? DaysToMonth366 : DaysToMonth365);
		return array[month] - array[month - 1];
	}

	internal static long DoubleDateToTicks(double value)
	{
		if (!(value < 2958466.0) || !(value > -657435.0))
		{
			throw new ArgumentException(Environment.GetResourceString("Not a legal OleAut date."));
		}
		long num = (long)(value * 86400000.0 + ((value >= 0.0) ? 0.5 : (-0.5)));
		if (num < 0)
		{
			num -= num % 86400000 * 2;
		}
		num += 59926435200000L;
		if (num < 0 || num >= 315537897600000L)
		{
			throw new ArgumentException(Environment.GetResourceString("OleAut date did not convert to a DateTime correctly."));
		}
		return num * 10000;
	}

	/// <summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	/// <returns>true if <paramref name="value" /> is an instance of <see cref="T:System.DateTime" /> and equals the value of this instance; otherwise, false.</returns>
	/// <param name="value">The object to compare to this instance. </param>
	/// <filterpriority>2</filterpriority>
	public override bool Equals(object value)
	{
		if (value is DateTime)
		{
			return InternalTicks == ((DateTime)value).InternalTicks;
		}
		return false;
	}

	/// <summary>Returns a value indicating whether the value of this instance is equal to the value of the specified <see cref="T:System.DateTime" /> instance.</summary>
	/// <returns>true if the <paramref name="value" /> parameter equals the value of this instance; otherwise, false.</returns>
	/// <param name="value">The object to compare to this instance. </param>
	/// <filterpriority>2</filterpriority>
	public bool Equals(DateTime value)
	{
		return InternalTicks == value.InternalTicks;
	}

	/// <summary>Returns a value indicating whether two <see cref="T:System.DateTime" /> instances  have the same date and time value.</summary>
	/// <returns>true if the two values are equal; otherwise, false.</returns>
	/// <param name="t1">The first object to compare. </param>
	/// <param name="t2">The second object to compare. </param>
	/// <filterpriority>1</filterpriority>
	public static bool Equals(DateTime t1, DateTime t2)
	{
		return t1.InternalTicks == t2.InternalTicks;
	}

	/// <summary>Deserializes a 64-bit binary value and recreates an original serialized <see cref="T:System.DateTime" /> object.</summary>
	/// <returns>An object that is equivalent to the <see cref="T:System.DateTime" /> object that was serialized by the <see cref="M:System.DateTime.ToBinary" /> method.</returns>
	/// <param name="dateData">A 64-bit signed integer that encodes the <see cref="P:System.DateTime.Kind" /> property in a 2-bit field and the <see cref="P:System.DateTime.Ticks" /> property in a 62-bit field. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="dateData" /> is less than <see cref="F:System.DateTime.MinValue" /> or greater than <see cref="F:System.DateTime.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static DateTime FromBinary(long dateData)
	{
		if ((dateData & long.MinValue) != 0L)
		{
			long num = dateData & 0x3FFFFFFFFFFFFFFFL;
			if (num > 4611685154427387904L)
			{
				num -= 4611686018427387904L;
			}
			bool isAmbiguousLocalDst = false;
			long ticks;
			if (num < 0)
			{
				ticks = TimeZoneInfo.GetLocalUtcOffset(MinValue, TimeZoneInfoOptions.NoThrowOnInvalidTime).Ticks;
			}
			else if (num > 3155378975999999999L)
			{
				ticks = TimeZoneInfo.GetLocalUtcOffset(MaxValue, TimeZoneInfoOptions.NoThrowOnInvalidTime).Ticks;
			}
			else
			{
				DateTime time = new DateTime(num, DateTimeKind.Utc);
				bool isDaylightSavings = false;
				ticks = TimeZoneInfo.GetUtcOffsetFromUtc(time, TimeZoneInfo.Local, out isDaylightSavings, out isAmbiguousLocalDst).Ticks;
			}
			num += ticks;
			if (num < 0)
			{
				num += 864000000000L;
			}
			if (num < 0 || num > 3155378975999999999L)
			{
				throw new ArgumentException(Environment.GetResourceString("The binary data must result in a DateTime with ticks between DateTime.MinValue.Ticks and DateTime.MaxValue.Ticks."), "dateData");
			}
			return new DateTime(num, DateTimeKind.Local, isAmbiguousLocalDst);
		}
		return FromBinaryRaw(dateData);
	}

	internal static DateTime FromBinaryRaw(long dateData)
	{
		long num = dateData & 0x3FFFFFFFFFFFFFFFL;
		if (num < 0 || num > 3155378975999999999L)
		{
			throw new ArgumentException(Environment.GetResourceString("The binary data must result in a DateTime with ticks between DateTime.MinValue.Ticks and DateTime.MaxValue.Ticks."), "dateData");
		}
		return new DateTime((ulong)dateData);
	}

	/// <summary>Converts the specified Windows file time to an equivalent local time.</summary>
	/// <returns>An object that represents the local time equivalent of the date and time represented by the <paramref name="fileTime" /> parameter.</returns>
	/// <param name="fileTime">A Windows file time expressed in ticks. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="fileTime" /> is less than 0 or represents a time greater than <see cref="F:System.DateTime.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static DateTime FromFileTime(long fileTime)
	{
		return FromFileTimeUtc(fileTime).ToLocalTime();
	}

	/// <summary>Converts the specified Windows file time to an equivalent UTC time.</summary>
	/// <returns>An object that represents the UTC time equivalent of the date and time represented by the <paramref name="fileTime" /> parameter.</returns>
	/// <param name="fileTime">A Windows file time expressed in ticks. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="fileTime" /> is less than 0 or represents a time greater than <see cref="F:System.DateTime.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static DateTime FromFileTimeUtc(long fileTime)
	{
		if (fileTime < 0 || fileTime > 2650467743999999999L)
		{
			throw new ArgumentOutOfRangeException("fileTime", Environment.GetResourceString("Not a valid Win32 FileTime."));
		}
		return new DateTime(fileTime + 504911232000000000L, DateTimeKind.Utc);
	}

	/// <summary>Returns a <see cref="T:System.DateTime" /> equivalent to the specified OLE Automation Date.</summary>
	/// <returns>An object that represents the same date and time as <paramref name="d" />.</returns>
	/// <param name="d">An OLE Automation Date value. </param>
	/// <exception cref="T:System.ArgumentException">The date is not a valid OLE Automation Date value. </exception>
	/// <filterpriority>1</filterpriority>
	public static DateTime FromOADate(double d)
	{
		return new DateTime(DoubleDateToTicks(d), DateTimeKind.Unspecified);
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with the data needed to serialize the current <see cref="T:System.DateTime" /> object.</summary>
	/// <param name="info">The object to populate with data. </param>
	/// <param name="context">The destination for this serialization. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null.</exception>
	[SecurityCritical]
	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("ticks", InternalTicks);
		info.AddValue("dateData", dateData);
	}

	/// <summary>Indicates whether this instance of <see cref="T:System.DateTime" /> is within the daylight saving time range for the current time zone.</summary>
	/// <returns>true if the value of the <see cref="P:System.DateTime.Kind" /> property is <see cref="F:System.DateTimeKind.Local" /> or <see cref="F:System.DateTimeKind.Unspecified" /> and the value of this instance of <see cref="T:System.DateTime" /> is within the daylight saving time range for the local time zone; false if <see cref="P:System.DateTime.Kind" /> is <see cref="F:System.DateTimeKind.Utc" />.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsDaylightSavingTime()
	{
		if (Kind == DateTimeKind.Utc)
		{
			return false;
		}
		return TimeZoneInfo.Local.IsDaylightSavingTime(this, TimeZoneInfoOptions.NoThrowOnInvalidTime);
	}

	/// <summary>Creates a new <see cref="T:System.DateTime" /> object that has the same number of ticks as the specified <see cref="T:System.DateTime" />, but is designated as either local time, Coordinated Universal Time (UTC), or neither, as indicated by the specified <see cref="T:System.DateTimeKind" /> value.</summary>
	/// <returns>A new object that has the same number of ticks as the object represented by the <paramref name="value" /> parameter and the <see cref="T:System.DateTimeKind" /> value specified by the <paramref name="kind" /> parameter. </returns>
	/// <param name="value">A date and time. </param>
	/// <param name="kind">One of the enumeration values that indicates whether the new object represents local time, UTC, or neither.</param>
	/// <filterpriority>2</filterpriority>
	public static DateTime SpecifyKind(DateTime value, DateTimeKind kind)
	{
		return new DateTime(value.InternalTicks, kind);
	}

	/// <summary>Serializes the current <see cref="T:System.DateTime" /> object to a 64-bit binary value that subsequently can be used to recreate the <see cref="T:System.DateTime" /> object.</summary>
	/// <returns>A 64-bit signed integer that encodes the <see cref="P:System.DateTime.Kind" /> and <see cref="P:System.DateTime.Ticks" /> properties. </returns>
	/// <filterpriority>2</filterpriority>
	public long ToBinary()
	{
		if (Kind == DateTimeKind.Local)
		{
			TimeSpan localUtcOffset = TimeZoneInfo.GetLocalUtcOffset(this, TimeZoneInfoOptions.NoThrowOnInvalidTime);
			long num = Ticks - localUtcOffset.Ticks;
			if (num < 0)
			{
				num = 4611686018427387904L + num;
			}
			return num | long.MinValue;
		}
		return (long)dateData;
	}

	internal long ToBinaryRaw()
	{
		return (long)dateData;
	}

	private int GetDatePart(int part)
	{
		int num = (int)(InternalTicks / 864000000000L);
		int num2 = num / 146097;
		num -= num2 * 146097;
		int num3 = num / 36524;
		if (num3 == 4)
		{
			num3 = 3;
		}
		num -= num3 * 36524;
		int num4 = num / 1461;
		num -= num4 * 1461;
		int num5 = num / 365;
		if (num5 == 4)
		{
			num5 = 3;
		}
		if (part == 0)
		{
			return num2 * 400 + num3 * 100 + num4 * 4 + num5 + 1;
		}
		num -= num5 * 365;
		if (part == 1)
		{
			return num + 1;
		}
		int[] array = ((num5 == 3 && (num4 != 24 || num3 == 3)) ? DaysToMonth366 : DaysToMonth365);
		int i;
		for (i = num >> 6; num >= array[i]; i++)
		{
		}
		if (part == 2)
		{
			return i;
		}
		return num - array[i - 1] + 1;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <filterpriority>2</filterpriority>
	public override int GetHashCode()
	{
		long internalTicks = InternalTicks;
		return (int)internalTicks ^ (int)(internalTicks >> 32);
	}

	internal bool IsAmbiguousDaylightSavingTime()
	{
		return InternalKind == 13835058055282163712uL;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	internal static extern long GetSystemTimeAsFileTime();

	/// <summary>Returns an indication whether the specified year is a leap year.</summary>
	/// <returns>true if <paramref name="year" /> is a leap year; otherwise, false.</returns>
	/// <param name="year">A 4-digit year. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is less than 1 or greater than 9999.</exception>
	/// <filterpriority>1</filterpriority>
	public static bool IsLeapYear(int year)
	{
		if (year < 1 || year > 9999)
		{
			throw new ArgumentOutOfRangeException("year", Environment.GetResourceString("Year must be between 1 and 9999."));
		}
		if (year % 4 == 0)
		{
			if (year % 100 == 0)
			{
				return year % 400 == 0;
			}
			return true;
		}
		return false;
	}

	/// <summary>Converts the string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent.</summary>
	/// <returns>An object that is equivalent to the date and time contained in <paramref name="s" />.</returns>
	/// <param name="s">A string that contains a date and time to convert. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="s" /> does not contain a valid string representation of a date and time. </exception>
	/// <filterpriority>1</filterpriority>
	public static DateTime Parse(string s)
	{
		return DateTimeParse.Parse(s, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None);
	}

	/// <summary>Converts the string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent by using culture-specific format information.</summary>
	/// <returns>An object that is equivalent to the date and time contained in <paramref name="s" /> as specified by <paramref name="provider" />.</returns>
	/// <param name="s">A string that contains a date and time to convert. </param>
	/// <param name="provider">An object that supplies culture-specific format information about <paramref name="s" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="s" /> does not contain a valid string representation of a date and time. </exception>
	/// <filterpriority>1</filterpriority>
	public static DateTime Parse(string s, IFormatProvider provider)
	{
		return DateTimeParse.Parse(s, DateTimeFormatInfo.GetInstance(provider), DateTimeStyles.None);
	}

	/// <summary>Converts the string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent by using culture-specific format information and formatting style.</summary>
	/// <returns>An object that is equivalent to the date and time contained in <paramref name="s" />, as specified by <paramref name="provider" /> and <paramref name="styles" />.</returns>
	/// <param name="s">A string that contains a date and time to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />. </param>
	/// <param name="styles">A bitwise combination of the enumeration values that indicates the style elements that can be present in <paramref name="s" /> for the parse operation to succeed, and that defines how to interpret the parsed date in relation to the current time zone or the current date. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="s" /> does not contain a valid string representation of a date and time. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="styles" /> contains an invalid combination of <see cref="T:System.Globalization.DateTimeStyles" /> values. For example, both <see cref="F:System.Globalization.DateTimeStyles.AssumeLocal" /> and <see cref="F:System.Globalization.DateTimeStyles.AssumeUniversal" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static DateTime Parse(string s, IFormatProvider provider, DateTimeStyles styles)
	{
		DateTimeFormatInfo.ValidateStyles(styles, "styles");
		return DateTimeParse.Parse(s, DateTimeFormatInfo.GetInstance(provider), styles);
	}

	/// <summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.</summary>
	/// <returns>An object that is equivalent to the date and time contained in <paramref name="s" />, as specified by <paramref name="format" /> and <paramref name="provider" />.</returns>
	/// <param name="s">A string that contains a date and time to convert. </param>
	/// <param name="format">A format specifier that defines the required format of <paramref name="s" />. For more information, see the Remarks section. </param>
	/// <param name="provider">An object that supplies culture-specific format information about <paramref name="s" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> or <paramref name="format" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="s" /> or <paramref name="format" /> is an empty string. -or- <paramref name="s" /> does not contain a date and time that corresponds to the pattern specified in <paramref name="format" />. -or-The hour component and the AM/PM designator in <paramref name="s" /> do not agree.</exception>
	/// <filterpriority>2</filterpriority>
	public static DateTime ParseExact(string s, string format, IFormatProvider provider)
	{
		return DateTimeParse.ParseExact(s, format, DateTimeFormatInfo.GetInstance(provider), DateTimeStyles.None);
	}

	/// <summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
	/// <returns>An object that is equivalent to the date and time contained in <paramref name="s" />, as specified by <paramref name="format" />, <paramref name="provider" />, and <paramref name="style" />.</returns>
	/// <param name="s">A string containing a date and time to convert. </param>
	/// <param name="format">A format specifier that defines the required format of <paramref name="s" />. For more information, see the Remarks section. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />. </param>
	/// <param name="style">A bitwise combination of the enumeration values that provides additional information about <paramref name="s" />, about style elements that may be present in <paramref name="s" />, or about the conversion from <paramref name="s" /> to a <see cref="T:System.DateTime" /> value. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> or <paramref name="format" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="s" /> or <paramref name="format" /> is an empty string. -or- <paramref name="s" /> does not contain a date and time that corresponds to the pattern specified in <paramref name="format" />. -or-The hour component and the AM/PM designator in <paramref name="s" /> do not agree.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="style" /> contains an invalid combination of <see cref="T:System.Globalization.DateTimeStyles" /> values. For example, both <see cref="F:System.Globalization.DateTimeStyles.AssumeLocal" /> and <see cref="F:System.Globalization.DateTimeStyles.AssumeUniversal" />.</exception>
	/// <filterpriority>2</filterpriority>
	public static DateTime ParseExact(string s, string format, IFormatProvider provider, DateTimeStyles style)
	{
		DateTimeFormatInfo.ValidateStyles(style, "style");
		return DateTimeParse.ParseExact(s, format, DateTimeFormatInfo.GetInstance(provider), style);
	}

	/// <summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified array of formats, culture-specific format information, and style. The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	/// <returns>An object that is equivalent to the date and time contained in <paramref name="s" />, as specified by <paramref name="formats" />, <paramref name="provider" />, and <paramref name="style" />.</returns>
	/// <param name="s">A string that contains a date and time to convert. </param>
	/// <param name="formats">An array of allowable formats of <paramref name="s" />. For more information, see the Remarks section. </param>
	/// <param name="provider">An object that supplies culture-specific format information about <paramref name="s" />. </param>
	/// <param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> or <paramref name="formats" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="s" /> is an empty string. -or- an element of <paramref name="formats" /> is an empty string. -or- <paramref name="s" /> does not contain a date and time that corresponds to any element of <paramref name="formats" />. -or-The hour component and the AM/PM designator in <paramref name="s" /> do not agree.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="style" /> contains an invalid combination of <see cref="T:System.Globalization.DateTimeStyles" /> values. For example, both <see cref="F:System.Globalization.DateTimeStyles.AssumeLocal" /> and <see cref="F:System.Globalization.DateTimeStyles.AssumeUniversal" />.</exception>
	/// <filterpriority>2</filterpriority>
	public static DateTime ParseExact(string s, string[] formats, IFormatProvider provider, DateTimeStyles style)
	{
		DateTimeFormatInfo.ValidateStyles(style, "style");
		return DateTimeParse.ParseExactMultiple(s, formats, DateTimeFormatInfo.GetInstance(provider), style);
	}

	/// <summary>Subtracts the specified date and time from this instance.</summary>
	/// <returns>A time interval that is equal to the date and time represented by this instance minus the date and time represented by <paramref name="value" />.</returns>
	/// <param name="value">The date and time value to subtract. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The result is less than <see cref="F:System.DateTime.MinValue" /> or greater than <see cref="F:System.DateTime.MaxValue" />. </exception>
	/// <filterpriority>2</filterpriority>
	public TimeSpan Subtract(DateTime value)
	{
		return new TimeSpan(InternalTicks - value.InternalTicks);
	}

	/// <summary>Subtracts the specified duration from this instance.</summary>
	/// <returns>An object that is equal to the date and time represented by this instance minus the time interval represented by <paramref name="value" />.</returns>
	/// <param name="value">The time interval to subtract. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The result is less than <see cref="F:System.DateTime.MinValue" /> or greater than <see cref="F:System.DateTime.MaxValue" />. </exception>
	/// <filterpriority>2</filterpriority>
	public DateTime Subtract(TimeSpan value)
	{
		long internalTicks = InternalTicks;
		long ticks = value._ticks;
		if (internalTicks < ticks || internalTicks - 3155378975999999999L > ticks)
		{
			throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("The added or subtracted value results in an un-representable DateTime."));
		}
		return new DateTime((ulong)(internalTicks - ticks) | InternalKind);
	}

	private static double TicksToOADate(long value)
	{
		if (value == 0L)
		{
			return 0.0;
		}
		if (value < 864000000000L)
		{
			value += 599264352000000000L;
		}
		if (value < 31241376000000000L)
		{
			throw new OverflowException(Environment.GetResourceString("Not a legal OleAut date."));
		}
		long num = (value - 599264352000000000L) / 10000;
		if (num < 0)
		{
			long num2 = num % 86400000;
			if (num2 != 0L)
			{
				num -= (86400000 + num2) * 2;
			}
		}
		return (double)num / 86400000.0;
	}

	/// <summary>Converts the value of this instance to the equivalent OLE Automation date.</summary>
	/// <returns>A double-precision floating-point number that contains an OLE Automation date equivalent to the value of this instance.</returns>
	/// <exception cref="T:System.OverflowException">The value of this instance cannot be represented as an OLE Automation Date. </exception>
	/// <filterpriority>2</filterpriority>
	public double ToOADate()
	{
		return TicksToOADate(InternalTicks);
	}

	/// <summary>Converts the value of the current <see cref="T:System.DateTime" /> object to a Windows file time.</summary>
	/// <returns>The value of the current <see cref="T:System.DateTime" /> object expressed as a Windows file time.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The resulting file time would represent a date and time before 12:00 midnight January 1, 1601 C.E. UTC. </exception>
	/// <filterpriority>2</filterpriority>
	public long ToFileTime()
	{
		return ToUniversalTime().ToFileTimeUtc();
	}

	/// <summary>Converts the value of the current <see cref="T:System.DateTime" /> object to a Windows file time.</summary>
	/// <returns>The value of the current <see cref="T:System.DateTime" /> object expressed as a Windows file time.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The resulting file time would represent a date and time before 12:00 midnight January 1, 1601 C.E. UTC. </exception>
	/// <filterpriority>2</filterpriority>
	public long ToFileTimeUtc()
	{
		long num = (((InternalKind & 0x8000000000000000uL) != 0L) ? ToUniversalTime().InternalTicks : InternalTicks) - 504911232000000000L;
		if (num < 0)
		{
			throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("Not a valid Win32 FileTime."));
		}
		return num;
	}

	/// <summary>Converts the value of the current <see cref="T:System.DateTime" /> object to local time.</summary>
	/// <returns>An object whose <see cref="P:System.DateTime.Kind" /> property is <see cref="F:System.DateTimeKind.Local" />, and whose value is the local time equivalent to the value of the current <see cref="T:System.DateTime" /> object, or <see cref="F:System.DateTime.MaxValue" /> if the converted value is too large to be represented by a <see cref="T:System.DateTime" /> object, or <see cref="F:System.DateTime.MinValue" /> if the converted value is too small to be represented as a <see cref="T:System.DateTime" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public DateTime ToLocalTime()
	{
		return ToLocalTime(throwOnOverflow: false);
	}

	internal DateTime ToLocalTime(bool throwOnOverflow)
	{
		if (Kind == DateTimeKind.Local)
		{
			return this;
		}
		bool isDaylightSavings = false;
		bool isAmbiguousLocalDst = false;
		long ticks = TimeZoneInfo.GetUtcOffsetFromUtc(this, TimeZoneInfo.Local, out isDaylightSavings, out isAmbiguousLocalDst).Ticks;
		long num = Ticks + ticks;
		if (num > 3155378975999999999L)
		{
			if (throwOnOverflow)
			{
				throw new ArgumentException(Environment.GetResourceString("Specified argument was out of the range of valid values."));
			}
			return new DateTime(3155378975999999999L, DateTimeKind.Local);
		}
		if (num < 0)
		{
			if (throwOnOverflow)
			{
				throw new ArgumentException(Environment.GetResourceString("Specified argument was out of the range of valid values."));
			}
			return new DateTime(0L, DateTimeKind.Local);
		}
		return new DateTime(num, DateTimeKind.Local, isAmbiguousLocalDst);
	}

	/// <summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent long date string representation.</summary>
	/// <returns>A string that contains the long date string representation of the current <see cref="T:System.DateTime" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public string ToLongDateString()
	{
		return DateTimeFormat.Format(this, "D", DateTimeFormatInfo.CurrentInfo);
	}

	/// <summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent long time string representation.</summary>
	/// <returns>A string that contains the long time string representation of the current <see cref="T:System.DateTime" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public string ToLongTimeString()
	{
		return DateTimeFormat.Format(this, "T", DateTimeFormatInfo.CurrentInfo);
	}

	/// <summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent short date string representation.</summary>
	/// <returns>A string that contains the short date string representation of the current <see cref="T:System.DateTime" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public string ToShortDateString()
	{
		return DateTimeFormat.Format(this, "d", DateTimeFormatInfo.CurrentInfo);
	}

	/// <summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent short time string representation.</summary>
	/// <returns>A string that contains the short time string representation of the current <see cref="T:System.DateTime" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public string ToShortTimeString()
	{
		return DateTimeFormat.Format(this, "t", DateTimeFormatInfo.CurrentInfo);
	}

	/// <summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent string representation.</summary>
	/// <returns>A string representation of the value of the current <see cref="T:System.DateTime" /> object.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The date and time is outside the range of dates supported by the calendar used by the current culture. </exception>
	/// <filterpriority>1</filterpriority>
	public override string ToString()
	{
		return DateTimeFormat.Format(this, null, DateTimeFormatInfo.CurrentInfo);
	}

	/// <summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent string representation using the specified format.</summary>
	/// <returns>A string representation of value of the current <see cref="T:System.DateTime" /> object as specified by <paramref name="format" />.</returns>
	/// <param name="format">A standard or custom date and time format string (see Remarks). </param>
	/// <exception cref="T:System.FormatException">The length of <paramref name="format" /> is 1, and it is not one of the format specifier characters defined for <see cref="T:System.Globalization.DateTimeFormatInfo" />.-or- <paramref name="format" /> does not contain a valid custom format pattern. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The date and time is outside the range of dates supported by the calendar used by the current culture.</exception>
	/// <filterpriority>1</filterpriority>
	public string ToString(string format)
	{
		return DateTimeFormat.Format(this, format, DateTimeFormatInfo.CurrentInfo);
	}

	/// <summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent string representation using the specified culture-specific format information.</summary>
	/// <returns>A string representation of value of the current <see cref="T:System.DateTime" /> object as specified by <paramref name="provider" />.</returns>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The date and time is outside the range of dates supported by the calendar used by <paramref name="provider" />. </exception>
	/// <filterpriority>1</filterpriority>
	public string ToString(IFormatProvider provider)
	{
		return DateTimeFormat.Format(this, null, DateTimeFormatInfo.GetInstance(provider));
	}

	/// <summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent string representation using the specified format and culture-specific format information.</summary>
	/// <returns>A string representation of value of the current <see cref="T:System.DateTime" /> object as specified by <paramref name="format" /> and <paramref name="provider" />.</returns>
	/// <param name="format">A standard or custom date and time format string. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">The length of <paramref name="format" /> is 1, and it is not one of the format specifier characters defined for <see cref="T:System.Globalization.DateTimeFormatInfo" />.-or- <paramref name="format" /> does not contain a valid custom format pattern. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The date and time is outside the range of dates supported by the calendar used by <paramref name="provider" />. </exception>
	/// <filterpriority>1</filterpriority>
	public string ToString(string format, IFormatProvider provider)
	{
		return DateTimeFormat.Format(this, format, DateTimeFormatInfo.GetInstance(provider));
	}

	/// <summary>Converts the value of the current <see cref="T:System.DateTime" /> object to Coordinated Universal Time (UTC).</summary>
	/// <returns>An object whose <see cref="P:System.DateTime.Kind" /> property is <see cref="F:System.DateTimeKind.Utc" />, and whose value is the UTC equivalent to the value of the current <see cref="T:System.DateTime" /> object, or <see cref="F:System.DateTime.MaxValue" /> if the converted value is too large to be represented by a <see cref="T:System.DateTime" /> object, or <see cref="F:System.DateTime.MinValue" /> if the converted value is too small to be represented by a <see cref="T:System.DateTime" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public DateTime ToUniversalTime()
	{
		return TimeZoneInfo.ConvertTimeToUtc(this, TimeZoneInfoOptions.NoThrowOnInvalidTime);
	}

	/// <summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	/// <returns>true if the <paramref name="s" /> parameter was converted successfully; otherwise, false.</returns>
	/// <param name="s">A string containing a date and time to convert. </param>
	/// <param name="result">When this method returns, contains the <see cref="T:System.DateTime" /> value equivalent to the date and time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateTime.MinValue" /> if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is null, is an empty string (""), or does not contain a valid string representation of a date and time. This parameter is passed uninitialized. </param>
	/// <filterpriority>1</filterpriority>
	public static bool TryParse(string s, out DateTime result)
	{
		return DateTimeParse.TryParse(s, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None, out result);
	}

	/// <summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified culture-specific format information and formatting style, and returns a value that indicates whether the conversion succeeded.</summary>
	/// <returns>true if the <paramref name="s" /> parameter was converted successfully; otherwise, false.</returns>
	/// <param name="s">A string containing a date and time to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />. </param>
	/// <param name="styles">A bitwise combination of enumeration values that defines how to interpret the parsed date in relation to the current time zone or the current date. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
	/// <param name="result">When this method returns, contains the <see cref="T:System.DateTime" /> value equivalent to the date and time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateTime.MinValue" /> if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is null, is an empty string (""), or does not contain a valid string representation of a date and time. This parameter is passed uninitialized. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="styles" /> is not a valid <see cref="T:System.Globalization.DateTimeStyles" /> value.-or-<paramref name="styles" /> contains an invalid combination of <see cref="T:System.Globalization.DateTimeStyles" /> values (for example, both <see cref="F:System.Globalization.DateTimeStyles.AssumeLocal" /> and <see cref="F:System.Globalization.DateTimeStyles.AssumeUniversal" />).</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="provider" /> is a neutral culture and cannot be used in a parsing operation.</exception>
	/// <filterpriority>1</filterpriority>
	public static bool TryParse(string s, IFormatProvider provider, DateTimeStyles styles, out DateTime result)
	{
		DateTimeFormatInfo.ValidateStyles(styles, "styles");
		return DateTimeParse.TryParse(s, DateTimeFormatInfo.GetInstance(provider), styles, out result);
	}

	/// <summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	/// <returns>true if <paramref name="s" /> was converted successfully; otherwise, false.</returns>
	/// <param name="s">A string containing a date and time to convert. </param>
	/// <param name="format">The required format of <paramref name="s" />. See the Remarks section for more information. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />. </param>
	/// <param name="style">A bitwise combination of one or more enumeration values that indicate the permitted format of <paramref name="s" />. </param>
	/// <param name="result">When this method returns, contains the <see cref="T:System.DateTime" /> value equivalent to the date and time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateTime.MinValue" /> if the conversion failed. The conversion fails if either the <paramref name="s" /> or <paramref name="format" /> parameter is null, is an empty string, or does not contain a date and time that correspond to the pattern specified in <paramref name="format" />. This parameter is passed uninitialized. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="styles" /> is not a valid <see cref="T:System.Globalization.DateTimeStyles" /> value.-or-<paramref name="styles" /> contains an invalid combination of <see cref="T:System.Globalization.DateTimeStyles" /> values (for example, both <see cref="F:System.Globalization.DateTimeStyles.AssumeLocal" /> and <see cref="F:System.Globalization.DateTimeStyles.AssumeUniversal" />).</exception>
	/// <filterpriority>1</filterpriority>
	public static bool TryParseExact(string s, string format, IFormatProvider provider, DateTimeStyles style, out DateTime result)
	{
		DateTimeFormatInfo.ValidateStyles(style, "style");
		return DateTimeParse.TryParseExact(s, format, DateTimeFormatInfo.GetInstance(provider), style, out result);
	}

	/// <summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified array of formats, culture-specific format information, and style. The format of the string representation must match at least one of the specified formats exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	/// <returns>true if the <paramref name="s" /> parameter was converted successfully; otherwise, false.</returns>
	/// <param name="s">A string that contains a date and time to convert. </param>
	/// <param name="formats">An array of allowable formats of <paramref name="s" />. See the Remarks section for more information. </param>
	/// <param name="provider">An object that supplies culture-specific format information about <paramref name="s" />. </param>
	/// <param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
	/// <param name="result">When this method returns, contains the <see cref="T:System.DateTime" /> value equivalent to the date and time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateTime.MinValue" /> if the conversion failed. The conversion fails if <paramref name="s" /> or <paramref name="formats" /> is null, <paramref name="s" /> or an element of <paramref name="formats" /> is an empty string, or the format of <paramref name="s" /> is not exactly as specified by at least one of the format patterns in <paramref name="formats" />. This parameter is passed uninitialized. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="styles" /> is not a valid <see cref="T:System.Globalization.DateTimeStyles" /> value.-or-<paramref name="styles" /> contains an invalid combination of <see cref="T:System.Globalization.DateTimeStyles" /> values (for example, both <see cref="F:System.Globalization.DateTimeStyles.AssumeLocal" /> and <see cref="F:System.Globalization.DateTimeStyles.AssumeUniversal" />).</exception>
	/// <filterpriority>1</filterpriority>
	public static bool TryParseExact(string s, string[] formats, IFormatProvider provider, DateTimeStyles style, out DateTime result)
	{
		DateTimeFormatInfo.ValidateStyles(style, "style");
		return DateTimeParse.TryParseExactMultiple(s, formats, DateTimeFormatInfo.GetInstance(provider), style, out result);
	}

	/// <summary>Adds a specified time interval to a specified date and time, yielding a new date and time.</summary>
	/// <returns>An object that is the sum of the values of <paramref name="d" /> and <paramref name="t" />.</returns>
	/// <param name="d">The date and time value to add. </param>
	/// <param name="t">The time interval to add. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTime" /> is less than <see cref="F:System.DateTime.MinValue" /> or greater than <see cref="F:System.DateTime.MaxValue" />. </exception>
	/// <filterpriority>3</filterpriority>
	public static DateTime operator +(DateTime d, TimeSpan t)
	{
		long internalTicks = d.InternalTicks;
		long ticks = t._ticks;
		if (ticks > 3155378975999999999L - internalTicks || ticks < -internalTicks)
		{
			throw new ArgumentOutOfRangeException("t", Environment.GetResourceString("The added or subtracted value results in an un-representable DateTime."));
		}
		return new DateTime((ulong)(internalTicks + ticks) | d.InternalKind);
	}

	/// <summary>Subtracts a specified time interval from a specified date and time and returns a new date and time.</summary>
	/// <returns>An object whose value is the value of <paramref name="d" /> minus the value of <paramref name="t" />.</returns>
	/// <param name="d">The date and time value to subtract from. </param>
	/// <param name="t">The time interval to subtract. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTime" /> is less than <see cref="F:System.DateTime.MinValue" /> or greater than <see cref="F:System.DateTime.MaxValue" />. </exception>
	/// <filterpriority>3</filterpriority>
	public static DateTime operator -(DateTime d, TimeSpan t)
	{
		long internalTicks = d.InternalTicks;
		long ticks = t._ticks;
		if (internalTicks < ticks || internalTicks - 3155378975999999999L > ticks)
		{
			throw new ArgumentOutOfRangeException("t", Environment.GetResourceString("The added or subtracted value results in an un-representable DateTime."));
		}
		return new DateTime((ulong)(internalTicks - ticks) | d.InternalKind);
	}

	/// <summary>Subtracts a specified date and time from another specified date and time and returns a time interval.</summary>
	/// <returns>The time interval between <paramref name="d1" /> and <paramref name="d2" />; that is, <paramref name="d1" /> minus <paramref name="d2" />.</returns>
	/// <param name="d1">The date and time value to subtract from (the minuend). </param>
	/// <param name="d2">The date and time value to subtract (the subtrahend). </param>
	/// <filterpriority>3</filterpriority>
	public static TimeSpan operator -(DateTime d1, DateTime d2)
	{
		return new TimeSpan(d1.InternalTicks - d2.InternalTicks);
	}

	/// <summary>Determines whether two specified instances of <see cref="T:System.DateTime" /> are equal.</summary>
	/// <returns>true if <paramref name="d1" /> and <paramref name="d2" /> represent the same date and time; otherwise, false.</returns>
	/// <param name="d1">The first object to compare. </param>
	/// <param name="d2">The second object to compare. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator ==(DateTime d1, DateTime d2)
	{
		return d1.InternalTicks == d2.InternalTicks;
	}

	/// <summary>Determines whether two specified instances of <see cref="T:System.DateTime" /> are not equal.</summary>
	/// <returns>true if <paramref name="d1" /> and <paramref name="d2" /> do not represent the same date and time; otherwise, false.</returns>
	/// <param name="d1">The first object to compare. </param>
	/// <param name="d2">The second object to compare. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator !=(DateTime d1, DateTime d2)
	{
		return d1.InternalTicks != d2.InternalTicks;
	}

	/// <summary>Determines whether one specified <see cref="T:System.DateTime" /> is earlier than another specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>true if <paramref name="t1" /> is less than <paramref name="t2" />; otherwise, false.</returns>
	/// <param name="t1">The first object to compare. </param>
	/// <param name="t2">The second object to compare. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator <(DateTime t1, DateTime t2)
	{
		return t1.InternalTicks < t2.InternalTicks;
	}

	/// <summary>Determines whether one specified <see cref="T:System.DateTime" /> represents a date and time that is the same as or earlier than another specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>true if <paramref name="t1" /> is less than or equal to <paramref name="t2" />; otherwise, false.</returns>
	/// <param name="t1">The first object to compare. </param>
	/// <param name="t2">The second object to compare. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator <=(DateTime t1, DateTime t2)
	{
		return t1.InternalTicks <= t2.InternalTicks;
	}

	/// <summary>Determines whether one specified <see cref="T:System.DateTime" /> is later than another specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>true if <paramref name="t1" /> is greater than <paramref name="t2" />; otherwise, false.</returns>
	/// <param name="t1">The first object to compare. </param>
	/// <param name="t2">The second object to compare. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator >(DateTime t1, DateTime t2)
	{
		return t1.InternalTicks > t2.InternalTicks;
	}

	/// <summary>Determines whether one specified <see cref="T:System.DateTime" /> represents a date and time that is the same as or later than another specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>true if <paramref name="t1" /> is greater than or equal to <paramref name="t2" />; otherwise, false.</returns>
	/// <param name="t1">The first object to compare. </param>
	/// <param name="t2">The second object to compare. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator >=(DateTime t1, DateTime t2)
	{
		return t1.InternalTicks >= t2.InternalTicks;
	}

	/// <summary>Converts the value of this instance to all the string representations supported by the standard date and time format specifiers.</summary>
	/// <returns>A string array where each element is the representation of the value of this instance formatted with one of the standard date and time format specifiers.</returns>
	/// <filterpriority>2</filterpriority>
	public string[] GetDateTimeFormats()
	{
		return GetDateTimeFormats(CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the value of this instance to all the string representations supported by the standard date and time format specifiers and the specified culture-specific formatting information.</summary>
	/// <returns>A string array where each element is the representation of the value of this instance formatted with one of the standard date and time format specifiers.</returns>
	/// <param name="provider">An object that supplies culture-specific formatting information about this instance. </param>
	/// <filterpriority>2</filterpriority>
	public string[] GetDateTimeFormats(IFormatProvider provider)
	{
		return DateTimeFormat.GetAllDateTimes(this, DateTimeFormatInfo.GetInstance(provider));
	}

	/// <summary>Converts the value of this instance to all the string representations supported by the specified standard date and time format specifier.</summary>
	/// <returns>A string array where each element is the representation of the value of this instance formatted with the <paramref name="format" /> standard date and time format specifier.</returns>
	/// <param name="format">A standard date and time format string (see Remarks). </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is not a valid standard date and time format specifier character.</exception>
	/// <filterpriority>2</filterpriority>
	public string[] GetDateTimeFormats(char format)
	{
		return GetDateTimeFormats(format, CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the value of this instance to all the string representations supported by the specified standard date and time format specifier and culture-specific formatting information.</summary>
	/// <returns>A string array where each element is the representation of the value of this instance formatted with one of the standard date and time format specifiers.</returns>
	/// <param name="format">A date and time format string (see Remarks). </param>
	/// <param name="provider">An object that supplies culture-specific formatting information about this instance. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is not a valid standard date and time format specifier character.</exception>
	/// <filterpriority>2</filterpriority>
	public string[] GetDateTimeFormats(char format, IFormatProvider provider)
	{
		return DateTimeFormat.GetAllDateTimes(this, format, DateTimeFormatInfo.GetInstance(provider));
	}

	/// <summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.DateTime" />.</summary>
	/// <returns>The enumerated constant, <see cref="F:System.TypeCode.DateTime" />.</returns>
	/// <filterpriority>2</filterpriority>
	public TypeCode GetTypeCode()
	{
		return TypeCode.DateTime;
	}

	/// <summary>This conversion is not supported. Attempting to use this method throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>The return value for this member is not used.</returns>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.InvalidCastException">In all cases.</exception>
	bool IConvertible.ToBoolean(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Invalid cast from '{0}' to '{1}'.", "DateTime", "Boolean"));
	}

	/// <summary>This conversion is not supported. Attempting to use this method throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>The return value for this member is not used.</returns>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.InvalidCastException">In all cases.</exception>
	char IConvertible.ToChar(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Invalid cast from '{0}' to '{1}'.", "DateTime", "Char"));
	}

	/// <summary>This conversion is not supported. Attempting to use this method throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>The return value for this member is not used.</returns>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.InvalidCastException">In all cases.</exception>
	sbyte IConvertible.ToSByte(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Invalid cast from '{0}' to '{1}'.", "DateTime", "SByte"));
	}

	/// <summary>This conversion is not supported. Attempting to use this method throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>The return value for this member is not used.</returns>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.InvalidCastException">In all cases.</exception>
	byte IConvertible.ToByte(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Invalid cast from '{0}' to '{1}'.", "DateTime", "Byte"));
	}

	/// <summary>This conversion is not supported. Attempting to use this method throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>The return value for this member is not used.</returns>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.InvalidCastException">In all cases.</exception>
	short IConvertible.ToInt16(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Invalid cast from '{0}' to '{1}'.", "DateTime", "Int16"));
	}

	/// <summary>This conversion is not supported. Attempting to use this method throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>The return value for this member is not used.</returns>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.InvalidCastException">In all cases.</exception>
	ushort IConvertible.ToUInt16(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Invalid cast from '{0}' to '{1}'.", "DateTime", "UInt16"));
	}

	/// <summary>This conversion is not supported. Attempting to use this method throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>The return value for this member is not used.</returns>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.InvalidCastException">In all cases.</exception>
	int IConvertible.ToInt32(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Invalid cast from '{0}' to '{1}'.", "DateTime", "Int32"));
	}

	/// <summary>This conversion is not supported. Attempting to use this method throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>The return value for this member is not used.</returns>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.InvalidCastException">In all cases.</exception>
	uint IConvertible.ToUInt32(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Invalid cast from '{0}' to '{1}'.", "DateTime", "UInt32"));
	}

	/// <summary>This conversion is not supported. Attempting to use this method throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>The return value for this member is not used.</returns>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.InvalidCastException">In all cases.</exception>
	long IConvertible.ToInt64(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Invalid cast from '{0}' to '{1}'.", "DateTime", "Int64"));
	}

	/// <summary>This conversion is not supported. Attempting to use this method throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>The return value for this member is not used.</returns>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.InvalidCastException">In all cases.</exception>
	ulong IConvertible.ToUInt64(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Invalid cast from '{0}' to '{1}'.", "DateTime", "UInt64"));
	}

	/// <summary>This conversion is not supported. Attempting to use this method throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>The return value for this member is not used.</returns>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.InvalidCastException">In all cases. </exception>
	float IConvertible.ToSingle(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Invalid cast from '{0}' to '{1}'.", "DateTime", "Single"));
	}

	/// <summary>This conversion is not supported. Attempting to use this method throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>The return value for this member is not used.</returns>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.InvalidCastException">In all cases.</exception>
	double IConvertible.ToDouble(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Invalid cast from '{0}' to '{1}'.", "DateTime", "Double"));
	}

	/// <summary>This conversion is not supported. Attempting to use this method throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>The return value for this member is not used.</returns>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.InvalidCastException">In all cases.</exception>
	decimal IConvertible.ToDecimal(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Invalid cast from '{0}' to '{1}'.", "DateTime", "Decimal"));
	}

	/// <summary>Returns the current <see cref="T:System.DateTime" /> object.</summary>
	/// <returns>The current object.</returns>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	DateTime IConvertible.ToDateTime(IFormatProvider provider)
	{
		return this;
	}

	/// <summary>Converts the current <see cref="T:System.DateTime" /> object to an object of a specified type.</summary>
	/// <returns>An object of the type specified by the <paramref name="type" /> parameter, with a value equivalent to the current <see cref="T:System.DateTime" /> object.</returns>
	/// <param name="type">The desired type. </param>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null. </exception>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported for the <see cref="T:System.DateTime" /> type.</exception>
	object IConvertible.ToType(Type type, IFormatProvider provider)
	{
		return Convert.DefaultToType(this, type, provider);
	}

	internal static bool TryCreate(int year, int month, int day, int hour, int minute, int second, int millisecond, out DateTime result)
	{
		result = MinValue;
		if (year < 1 || year > 9999 || month < 1 || month > 12)
		{
			return false;
		}
		int[] array = (IsLeapYear(year) ? DaysToMonth366 : DaysToMonth365);
		if (day < 1 || day > array[month] - array[month - 1])
		{
			return false;
		}
		if (hour < 0 || hour >= 24 || minute < 0 || minute >= 60 || second < 0 || second >= 60)
		{
			return false;
		}
		if (millisecond < 0 || millisecond >= 1000)
		{
			return false;
		}
		long num = DateToTicks(year, month, day) + TimeToTicks(hour, minute, second);
		num += (long)millisecond * 10000L;
		if (num < 0 || num > 3155378975999999999L)
		{
			return false;
		}
		result = new DateTime(num, DateTimeKind.Unspecified);
		return true;
	}
}
