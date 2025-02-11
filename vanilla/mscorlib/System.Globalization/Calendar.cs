using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace System.Globalization;

/// <summary>Represents time in divisions, such as weeks, months, and years.</summary>
[Serializable]
[ComVisible(true)]
public abstract class Calendar : ICloneable
{
	internal const long TicksPerMillisecond = 10000L;

	internal const long TicksPerSecond = 10000000L;

	internal const long TicksPerMinute = 600000000L;

	internal const long TicksPerHour = 36000000000L;

	internal const long TicksPerDay = 864000000000L;

	internal const int MillisPerSecond = 1000;

	internal const int MillisPerMinute = 60000;

	internal const int MillisPerHour = 3600000;

	internal const int MillisPerDay = 86400000;

	internal const int DaysPerYear = 365;

	internal const int DaysPer4Years = 1461;

	internal const int DaysPer100Years = 36524;

	internal const int DaysPer400Years = 146097;

	internal const int DaysTo10000 = 3652059;

	internal const long MaxMillis = 315537897600000L;

	internal const int CAL_GREGORIAN = 1;

	internal const int CAL_GREGORIAN_US = 2;

	internal const int CAL_JAPAN = 3;

	internal const int CAL_TAIWAN = 4;

	internal const int CAL_KOREA = 5;

	internal const int CAL_HIJRI = 6;

	internal const int CAL_THAI = 7;

	internal const int CAL_HEBREW = 8;

	internal const int CAL_GREGORIAN_ME_FRENCH = 9;

	internal const int CAL_GREGORIAN_ARABIC = 10;

	internal const int CAL_GREGORIAN_XLIT_ENGLISH = 11;

	internal const int CAL_GREGORIAN_XLIT_FRENCH = 12;

	internal const int CAL_JULIAN = 13;

	internal const int CAL_JAPANESELUNISOLAR = 14;

	internal const int CAL_CHINESELUNISOLAR = 15;

	internal const int CAL_SAKA = 16;

	internal const int CAL_LUNAR_ETO_CHN = 17;

	internal const int CAL_LUNAR_ETO_KOR = 18;

	internal const int CAL_LUNAR_ETO_ROKUYOU = 19;

	internal const int CAL_KOREANLUNISOLAR = 20;

	internal const int CAL_TAIWANLUNISOLAR = 21;

	internal const int CAL_PERSIAN = 22;

	internal const int CAL_UMALQURA = 23;

	internal int m_currentEraValue = -1;

	[OptionalField(VersionAdded = 2)]
	private bool m_isReadOnly;

	/// <summary>Represents the current era of the current calendar. </summary>
	public const int CurrentEra = 0;

	internal int twoDigitYearMax = -1;

	/// <summary>Gets the earliest date and time supported by this <see cref="T:System.Globalization.Calendar" /> object.</summary>
	/// <returns>The earliest date and time supported by this calendar. The default is <see cref="F:System.DateTime.MinValue" />.</returns>
	[ComVisible(false)]
	public virtual DateTime MinSupportedDateTime => DateTime.MinValue;

	/// <summary>Gets the latest date and time supported by this <see cref="T:System.Globalization.Calendar" /> object.</summary>
	/// <returns>The latest date and time supported by this calendar. The default is <see cref="F:System.DateTime.MaxValue" />.</returns>
	[ComVisible(false)]
	public virtual DateTime MaxSupportedDateTime => DateTime.MaxValue;

	internal virtual int ID => -1;

	internal virtual int BaseCalendarID => ID;

	/// <summary>Gets a value indicating whether the current calendar is solar-based, lunar-based, or a combination of both.</summary>
	/// <returns>One of the <see cref="T:System.Globalization.CalendarAlgorithmType" /> values.</returns>
	[ComVisible(false)]
	public virtual CalendarAlgorithmType AlgorithmType => CalendarAlgorithmType.Unknown;

	/// <summary>Gets a value indicating whether this <see cref="T:System.Globalization.Calendar" /> object is read-only.</summary>
	/// <returns>true if this <see cref="T:System.Globalization.Calendar" /> object is read-only; otherwise, false.</returns>
	[ComVisible(false)]
	public bool IsReadOnly => m_isReadOnly;

	internal virtual int CurrentEraValue
	{
		get
		{
			if (m_currentEraValue == -1)
			{
				m_currentEraValue = CalendarData.GetCalendarData(BaseCalendarID).iCurrentEra;
			}
			return m_currentEraValue;
		}
	}

	/// <summary>When overridden in a derived class, gets the list of eras in the current calendar.</summary>
	/// <returns>An array of integers that represents the eras in the current calendar.</returns>
	public abstract int[] Eras { get; }

	/// <summary>Gets the number of days in the year that precedes the year that is specified by the <see cref="P:System.Globalization.Calendar.MinSupportedDateTime" /> property.</summary>
	/// <returns>The number of days in the year that precedes the year specified by <see cref="P:System.Globalization.Calendar.MinSupportedDateTime" />.</returns>
	protected virtual int DaysInYearBeforeMinSupportedYear => 365;

	/// <summary>Gets or sets the last year of a 100-year range that can be represented by a 2-digit year.</summary>
	/// <returns>The last year of a 100-year range that can be represented by a 2-digit year.</returns>
	/// <exception cref="T:System.InvalidOperationException">The current <see cref="T:System.Globalization.Calendar" /> object is read-only.</exception>
	public virtual int TwoDigitYearMax
	{
		get
		{
			return twoDigitYearMax;
		}
		set
		{
			VerifyWritable();
			twoDigitYearMax = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Globalization.Calendar" /> class.</summary>
	protected Calendar()
	{
	}

	/// <summary>Creates a new object that is a copy of the current <see cref="T:System.Globalization.Calendar" /> object.</summary>
	/// <returns>A new instance of <see cref="T:System.Object" /> that is the memberwise clone of the current <see cref="T:System.Globalization.Calendar" /> object.</returns>
	[ComVisible(false)]
	public virtual object Clone()
	{
		object obj = MemberwiseClone();
		((Calendar)obj).SetReadOnlyState(readOnly: false);
		return obj;
	}

	/// <summary>Returns a read-only version of the specified <see cref="T:System.Globalization.Calendar" /> object.</summary>
	/// <returns>The <see cref="T:System.Globalization.Calendar" /> object specified by the <paramref name="calendar" /> parameter, if <paramref name="calendar" /> is read-only.-or-A read-only memberwise clone of the <see cref="T:System.Globalization.Calendar" /> object specified by <paramref name="calendar" />, if <paramref name="calendar" /> is not read-only.</returns>
	/// <param name="calendar">A <see cref="T:System.Globalization.Calendar" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="calendar" /> is null.</exception>
	[ComVisible(false)]
	public static Calendar ReadOnly(Calendar calendar)
	{
		if (calendar == null)
		{
			throw new ArgumentNullException("calendar");
		}
		if (calendar.IsReadOnly)
		{
			return calendar;
		}
		Calendar obj = (Calendar)calendar.MemberwiseClone();
		obj.SetReadOnlyState(readOnly: true);
		return obj;
	}

	internal void VerifyWritable()
	{
		if (m_isReadOnly)
		{
			throw new InvalidOperationException(Environment.GetResourceString("Instance is read-only."));
		}
	}

	internal void SetReadOnlyState(bool readOnly)
	{
		m_isReadOnly = readOnly;
	}

	internal static void CheckAddResult(long ticks, DateTime minValue, DateTime maxValue)
	{
		if (ticks < minValue.Ticks || ticks > maxValue.Ticks)
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("The result is out of the supported range for this calendar. The result should be between {0} (Gregorian date) and {1} (Gregorian date), inclusive."), minValue, maxValue));
		}
	}

	internal DateTime Add(DateTime time, double value, int scale)
	{
		double num = value * (double)scale + ((value >= 0.0) ? 0.5 : (-0.5));
		if (!(num > -315537897600000.0) || !(num < 315537897600000.0))
		{
			throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("Value to add was out of range."));
		}
		long num2 = (long)num;
		long ticks = time.Ticks + num2 * 10000;
		CheckAddResult(ticks, MinSupportedDateTime, MaxSupportedDateTime);
		return new DateTime(ticks);
	}

	/// <summary>Returns a <see cref="T:System.DateTime" /> that is the specified number of milliseconds away from the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>The <see cref="T:System.DateTime" /> that results from adding the specified number of milliseconds to the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to add milliseconds to. </param>
	/// <param name="milliseconds">The number of milliseconds to add.</param>
	/// <exception cref="T:System.ArgumentException">The resulting <see cref="T:System.DateTime" /> is outside the supported range of this calendar. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="milliseconds" /> is outside the supported range of the <see cref="T:System.DateTime" /> return value. </exception>
	public virtual DateTime AddMilliseconds(DateTime time, double milliseconds)
	{
		return Add(time, milliseconds, 1);
	}

	/// <summary>Returns a <see cref="T:System.DateTime" /> that is the specified number of days away from the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>The <see cref="T:System.DateTime" /> that results from adding the specified number of days to the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to which to add days. </param>
	/// <param name="days">The number of days to add. </param>
	/// <exception cref="T:System.ArgumentException">The resulting <see cref="T:System.DateTime" /> is outside the supported range of this calendar. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="days" /> is outside the supported range of the <see cref="T:System.DateTime" /> return value. </exception>
	public virtual DateTime AddDays(DateTime time, int days)
	{
		return Add(time, days, 86400000);
	}

	/// <summary>Returns a <see cref="T:System.DateTime" /> that is the specified number of hours away from the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>The <see cref="T:System.DateTime" /> that results from adding the specified number of hours to the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to which to add hours. </param>
	/// <param name="hours">The number of hours to add. </param>
	/// <exception cref="T:System.ArgumentException">The resulting <see cref="T:System.DateTime" /> is outside the supported range of this calendar. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="hours" /> is outside the supported range of the <see cref="T:System.DateTime" /> return value. </exception>
	public virtual DateTime AddHours(DateTime time, int hours)
	{
		return Add(time, hours, 3600000);
	}

	/// <summary>Returns a <see cref="T:System.DateTime" /> that is the specified number of minutes away from the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>The <see cref="T:System.DateTime" /> that results from adding the specified number of minutes to the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to which to add minutes. </param>
	/// <param name="minutes">The number of minutes to add. </param>
	/// <exception cref="T:System.ArgumentException">The resulting <see cref="T:System.DateTime" /> is outside the supported range of this calendar. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="minutes" /> is outside the supported range of the <see cref="T:System.DateTime" /> return value. </exception>
	public virtual DateTime AddMinutes(DateTime time, int minutes)
	{
		return Add(time, minutes, 60000);
	}

	/// <summary>When overridden in a derived class, returns a <see cref="T:System.DateTime" /> that is the specified number of months away from the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>The <see cref="T:System.DateTime" /> that results from adding the specified number of months to the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to which to add months. </param>
	/// <param name="months">The number of months to add. </param>
	/// <exception cref="T:System.ArgumentException">The resulting <see cref="T:System.DateTime" /> is outside the supported range of this calendar. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="months" /> is outside the supported range of the <see cref="T:System.DateTime" /> return value. </exception>
	public abstract DateTime AddMonths(DateTime time, int months);

	/// <summary>Returns a <see cref="T:System.DateTime" /> that is the specified number of seconds away from the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>The <see cref="T:System.DateTime" /> that results from adding the specified number of seconds to the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to which to add seconds. </param>
	/// <param name="seconds">The number of seconds to add. </param>
	/// <exception cref="T:System.ArgumentException">The resulting <see cref="T:System.DateTime" /> is outside the supported range of this calendar. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="seconds" /> is outside the supported range of the <see cref="T:System.DateTime" /> return value. </exception>
	public virtual DateTime AddSeconds(DateTime time, int seconds)
	{
		return Add(time, seconds, 1000);
	}

	/// <summary>Returns a <see cref="T:System.DateTime" /> that is the specified number of weeks away from the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>The <see cref="T:System.DateTime" /> that results from adding the specified number of weeks to the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to which to add weeks. </param>
	/// <param name="weeks">The number of weeks to add. </param>
	/// <exception cref="T:System.ArgumentException">The resulting <see cref="T:System.DateTime" /> is outside the supported range of this calendar. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="weeks" /> is outside the supported range of the <see cref="T:System.DateTime" /> return value. </exception>
	public virtual DateTime AddWeeks(DateTime time, int weeks)
	{
		return AddDays(time, weeks * 7);
	}

	/// <summary>When overridden in a derived class, returns a <see cref="T:System.DateTime" /> that is the specified number of years away from the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>The <see cref="T:System.DateTime" /> that results from adding the specified number of years to the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to which to add years. </param>
	/// <param name="years">The number of years to add. </param>
	/// <exception cref="T:System.ArgumentException">The resulting <see cref="T:System.DateTime" /> is outside the supported range of this calendar. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="years" /> is outside the supported range of the <see cref="T:System.DateTime" /> return value. </exception>
	public abstract DateTime AddYears(DateTime time, int years);

	/// <summary>When overridden in a derived class, returns the day of the month in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>A positive integer that represents the day of the month in the <paramref name="time" /> parameter.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	public abstract int GetDayOfMonth(DateTime time);

	/// <summary>When overridden in a derived class, returns the day of the week in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>A <see cref="T:System.DayOfWeek" /> value that represents the day of the week in the <paramref name="time" /> parameter.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	public abstract DayOfWeek GetDayOfWeek(DateTime time);

	/// <summary>When overridden in a derived class, returns the day of the year in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>A positive integer that represents the day of the year in the <paramref name="time" /> parameter.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	public abstract int GetDayOfYear(DateTime time);

	/// <summary>Returns the number of days in the specified month and year of the current era.</summary>
	/// <returns>The number of days in the specified month in the specified year in the current era.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="month">A positive integer that represents the month. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is outside the range supported by the calendar.-or- <paramref name="month" /> is outside the range supported by the calendar. </exception>
	public virtual int GetDaysInMonth(int year, int month)
	{
		return GetDaysInMonth(year, month, 0);
	}

	/// <summary>When overridden in a derived class, returns the number of days in the specified month, year, and era.</summary>
	/// <returns>The number of days in the specified month in the specified year in the specified era.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="month">A positive integer that represents the month. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is outside the range supported by the calendar.-or- <paramref name="month" /> is outside the range supported by the calendar.-or- <paramref name="era" /> is outside the range supported by the calendar. </exception>
	public abstract int GetDaysInMonth(int year, int month, int era);

	/// <summary>Returns the number of days in the specified year of the current era.</summary>
	/// <returns>The number of days in the specified year in the current era.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is outside the range supported by the calendar. </exception>
	public virtual int GetDaysInYear(int year)
	{
		return GetDaysInYear(year, 0);
	}

	/// <summary>When overridden in a derived class, returns the number of days in the specified year and era.</summary>
	/// <returns>The number of days in the specified year in the specified era.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is outside the range supported by the calendar.-or- <paramref name="era" /> is outside the range supported by the calendar. </exception>
	public abstract int GetDaysInYear(int year, int era);

	/// <summary>When overridden in a derived class, returns the era in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>An integer that represents the era in <paramref name="time" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	public abstract int GetEra(DateTime time);

	/// <summary>Returns the hours value in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>An integer from 0 to 23 that represents the hour in <paramref name="time" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	public virtual int GetHour(DateTime time)
	{
		return (int)(time.Ticks / 36000000000L % 24);
	}

	/// <summary>Returns the milliseconds value in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>A double-precision floating-point number from 0 to 999 that represents the milliseconds in the <paramref name="time" /> parameter.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	public virtual double GetMilliseconds(DateTime time)
	{
		return time.Ticks / 10000 % 1000;
	}

	/// <summary>Returns the minutes value in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>An integer from 0 to 59 that represents the minutes in <paramref name="time" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	public virtual int GetMinute(DateTime time)
	{
		return (int)(time.Ticks / 600000000 % 60);
	}

	/// <summary>When overridden in a derived class, returns the month in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>A positive integer that represents the month in <paramref name="time" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	public abstract int GetMonth(DateTime time);

	/// <summary>Returns the number of months in the specified year in the current era.</summary>
	/// <returns>The number of months in the specified year in the current era.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is outside the range supported by the calendar. </exception>
	public virtual int GetMonthsInYear(int year)
	{
		return GetMonthsInYear(year, 0);
	}

	/// <summary>When overridden in a derived class, returns the number of months in the specified year in the specified era.</summary>
	/// <returns>The number of months in the specified year in the specified era.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is outside the range supported by the calendar.-or- <paramref name="era" /> is outside the range supported by the calendar. </exception>
	public abstract int GetMonthsInYear(int year, int era);

	/// <summary>Returns the seconds value in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>An integer from 0 to 59 that represents the seconds in <paramref name="time" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	public virtual int GetSecond(DateTime time)
	{
		return (int)(time.Ticks / 10000000 % 60);
	}

	internal int GetFirstDayWeekOfYear(DateTime time, int firstDayOfWeek)
	{
		int num = GetDayOfYear(time) - 1;
		int num2 = (int)(GetDayOfWeek(time) - num % 7 - firstDayOfWeek + 14) % 7;
		return (num + num2) / 7 + 1;
	}

	private int GetWeekOfYearFullDays(DateTime time, int firstDayOfWeek, int fullDays)
	{
		int num = GetDayOfYear(time) - 1;
		int num2 = (int)(GetDayOfWeek(time) - num % 7);
		int num3 = (firstDayOfWeek - num2 + 14) % 7;
		if (num3 != 0 && num3 >= fullDays)
		{
			num3 -= 7;
		}
		int num4 = num - num3;
		if (num4 >= 0)
		{
			return num4 / 7 + 1;
		}
		if (time <= MinSupportedDateTime.AddDays(num))
		{
			return GetWeekOfYearOfMinSupportedDateTime(firstDayOfWeek, fullDays);
		}
		return GetWeekOfYearFullDays(time.AddDays(-(num + 1)), firstDayOfWeek, fullDays);
	}

	private int GetWeekOfYearOfMinSupportedDateTime(int firstDayOfWeek, int minimumDaysInFirstWeek)
	{
		int num = GetDayOfYear(MinSupportedDateTime) - 1;
		int num2 = (int)(GetDayOfWeek(MinSupportedDateTime) - num % 7);
		int num3 = (firstDayOfWeek + 7 - num2) % 7;
		if (num3 == 0 || num3 >= minimumDaysInFirstWeek)
		{
			return 1;
		}
		int num4 = DaysInYearBeforeMinSupportedYear - 1;
		int num5 = num2 - 1 - num4 % 7;
		int num6 = (firstDayOfWeek - num5 + 14) % 7;
		int num7 = num4 - num6;
		if (num6 >= minimumDaysInFirstWeek)
		{
			num7 += 7;
		}
		return num7 / 7 + 1;
	}

	/// <summary>Returns the week of the year that includes the date in the specified <see cref="T:System.DateTime" /> value.</summary>
	/// <returns>A positive integer that represents the week of the year that includes the date in the <paramref name="time" /> parameter.</returns>
	/// <param name="time">A date and time value. </param>
	/// <param name="rule">An enumeration value that defines a calendar week. </param>
	/// <param name="firstDayOfWeek">An enumeration value that represents the first day of the week. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="time" /> is earlier than <see cref="P:System.Globalization.Calendar.MinSupportedDateTime" /> or later than <see cref="P:System.Globalization.Calendar.MaxSupportedDateTime" />.-or-<paramref name="firstDayOfWeek" /> is not a valid <see cref="T:System.DayOfWeek" /> value.-or- <paramref name="rule" /> is not a valid <see cref="T:System.Globalization.CalendarWeekRule" /> value. </exception>
	public virtual int GetWeekOfYear(DateTime time, CalendarWeekRule rule, DayOfWeek firstDayOfWeek)
	{
		if (firstDayOfWeek < DayOfWeek.Sunday || firstDayOfWeek > DayOfWeek.Saturday)
		{
			throw new ArgumentOutOfRangeException("firstDayOfWeek", Environment.GetResourceString("Valid values are between {0} and {1}, inclusive.", DayOfWeek.Sunday, DayOfWeek.Saturday));
		}
		return rule switch
		{
			CalendarWeekRule.FirstDay => GetFirstDayWeekOfYear(time, (int)firstDayOfWeek), 
			CalendarWeekRule.FirstFullWeek => GetWeekOfYearFullDays(time, (int)firstDayOfWeek, 7), 
			CalendarWeekRule.FirstFourDayWeek => GetWeekOfYearFullDays(time, (int)firstDayOfWeek, 4), 
			_ => throw new ArgumentOutOfRangeException("rule", Environment.GetResourceString("Valid values are between {0} and {1}, inclusive.", CalendarWeekRule.FirstDay, CalendarWeekRule.FirstFourDayWeek)), 
		};
	}

	/// <summary>When overridden in a derived class, returns the year in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>An integer that represents the year in <paramref name="time" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	public abstract int GetYear(DateTime time);

	/// <summary>Determines whether the specified date in the current era is a leap day.</summary>
	/// <returns>true if the specified day is a leap day; otherwise, false.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="month">A positive integer that represents the month. </param>
	/// <param name="day">A positive integer that represents the day. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is outside the range supported by the calendar.-or- <paramref name="month" /> is outside the range supported by the calendar.-or- <paramref name="day" /> is outside the range supported by the calendar. </exception>
	public virtual bool IsLeapDay(int year, int month, int day)
	{
		return IsLeapDay(year, month, day, 0);
	}

	/// <summary>When overridden in a derived class, determines whether the specified date in the specified era is a leap day.</summary>
	/// <returns>true if the specified day is a leap day; otherwise, false.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="month">A positive integer that represents the month. </param>
	/// <param name="day">A positive integer that represents the day. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is outside the range supported by the calendar.-or- <paramref name="month" /> is outside the range supported by the calendar.-or- <paramref name="day" /> is outside the range supported by the calendar.-or- <paramref name="era" /> is outside the range supported by the calendar. </exception>
	public abstract bool IsLeapDay(int year, int month, int day, int era);

	/// <summary>Determines whether the specified month in the specified year in the current era is a leap month.</summary>
	/// <returns>true if the specified month is a leap month; otherwise, false.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="month">A positive integer that represents the month. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is outside the range supported by the calendar.-or- <paramref name="month" /> is outside the range supported by the calendar. </exception>
	public virtual bool IsLeapMonth(int year, int month)
	{
		return IsLeapMonth(year, month, 0);
	}

	/// <summary>When overridden in a derived class, determines whether the specified month in the specified year in the specified era is a leap month.</summary>
	/// <returns>true if the specified month is a leap month; otherwise, false.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="month">A positive integer that represents the month. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is outside the range supported by the calendar.-or- <paramref name="month" /> is outside the range supported by the calendar.-or- <paramref name="era" /> is outside the range supported by the calendar. </exception>
	public abstract bool IsLeapMonth(int year, int month, int era);

	/// <summary>Calculates the leap month for a specified year.</summary>
	/// <returns>A positive integer that indicates the leap month in the specified year.-or-Zero if this calendar does not support a leap month or if the <paramref name="year" /> parameter does not represent a leap year.</returns>
	/// <param name="year">A year.</param>
	[ComVisible(false)]
	public virtual int GetLeapMonth(int year)
	{
		return GetLeapMonth(year, 0);
	}

	/// <summary>Calculates the leap month for a specified year and era.</summary>
	/// <returns>A positive integer that indicates the leap month in the specified year and era.-or-Zero if this calendar does not support a leap month or if the <paramref name="year" /> and <paramref name="era" /> parameters do not specify a leap year.</returns>
	/// <param name="year">A year.</param>
	/// <param name="era">An era.</param>
	[ComVisible(false)]
	public virtual int GetLeapMonth(int year, int era)
	{
		if (!IsLeapYear(year, era))
		{
			return 0;
		}
		int monthsInYear = GetMonthsInYear(year, era);
		for (int i = 1; i <= monthsInYear; i++)
		{
			if (IsLeapMonth(year, i, era))
			{
				return i;
			}
		}
		return 0;
	}

	/// <summary>Determines whether the specified year in the current era is a leap year.</summary>
	/// <returns>true if the specified year is a leap year; otherwise, false.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is outside the range supported by the calendar. </exception>
	public virtual bool IsLeapYear(int year)
	{
		return IsLeapYear(year, 0);
	}

	/// <summary>When overridden in a derived class, determines whether the specified year in the specified era is a leap year.</summary>
	/// <returns>true if the specified year is a leap year; otherwise, false.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is outside the range supported by the calendar.-or- <paramref name="era" /> is outside the range supported by the calendar. </exception>
	public abstract bool IsLeapYear(int year, int era);

	/// <summary>Returns a <see cref="T:System.DateTime" /> that is set to the specified date and time in the current era.</summary>
	/// <returns>The <see cref="T:System.DateTime" /> that is set to the specified date and time in the current era.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="month">A positive integer that represents the month. </param>
	/// <param name="day">A positive integer that represents the day. </param>
	/// <param name="hour">An integer from 0 to 23 that represents the hour. </param>
	/// <param name="minute">An integer from 0 to 59 that represents the minute. </param>
	/// <param name="second">An integer from 0 to 59 that represents the second. </param>
	/// <param name="millisecond">An integer from 0 to 999 that represents the millisecond. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is outside the range supported by the calendar.-or- <paramref name="month" /> is outside the range supported by the calendar.-or- <paramref name="day" /> is outside the range supported by the calendar.-or- <paramref name="hour" /> is less than zero or greater than 23.-or- <paramref name="minute" /> is less than zero or greater than 59.-or- <paramref name="second" /> is less than zero or greater than 59.-or- <paramref name="millisecond" /> is less than zero or greater than 999. </exception>
	public virtual DateTime ToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond)
	{
		return ToDateTime(year, month, day, hour, minute, second, millisecond, 0);
	}

	/// <summary>When overridden in a derived class, returns a <see cref="T:System.DateTime" /> that is set to the specified date and time in the specified era.</summary>
	/// <returns>The <see cref="T:System.DateTime" /> that is set to the specified date and time in the current era.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="month">A positive integer that represents the month. </param>
	/// <param name="day">A positive integer that represents the day. </param>
	/// <param name="hour">An integer from 0 to 23 that represents the hour. </param>
	/// <param name="minute">An integer from 0 to 59 that represents the minute. </param>
	/// <param name="second">An integer from 0 to 59 that represents the second. </param>
	/// <param name="millisecond">An integer from 0 to 999 that represents the millisecond. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is outside the range supported by the calendar.-or- <paramref name="month" /> is outside the range supported by the calendar.-or- <paramref name="day" /> is outside the range supported by the calendar.-or- <paramref name="hour" /> is less than zero or greater than 23.-or- <paramref name="minute" /> is less than zero or greater than 59.-or- <paramref name="second" /> is less than zero or greater than 59.-or- <paramref name="millisecond" /> is less than zero or greater than 999.-or- <paramref name="era" /> is outside the range supported by the calendar. </exception>
	public abstract DateTime ToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era);

	internal virtual bool TryToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era, out DateTime result)
	{
		result = DateTime.MinValue;
		try
		{
			result = ToDateTime(year, month, day, hour, minute, second, millisecond, era);
			return true;
		}
		catch (ArgumentException)
		{
			return false;
		}
	}

	internal virtual bool IsValidYear(int year, int era)
	{
		if (year >= GetYear(MinSupportedDateTime))
		{
			return year <= GetYear(MaxSupportedDateTime);
		}
		return false;
	}

	internal virtual bool IsValidMonth(int year, int month, int era)
	{
		if (IsValidYear(year, era) && month >= 1)
		{
			return month <= GetMonthsInYear(year, era);
		}
		return false;
	}

	internal virtual bool IsValidDay(int year, int month, int day, int era)
	{
		if (IsValidMonth(year, month, era) && day >= 1)
		{
			return day <= GetDaysInMonth(year, month, era);
		}
		return false;
	}

	/// <summary>Converts the specified year to a four-digit year by using the <see cref="P:System.Globalization.Calendar.TwoDigitYearMax" /> property to determine the appropriate century.</summary>
	/// <returns>An integer that contains the four-digit representation of <paramref name="year" />.</returns>
	/// <param name="year">A two-digit or four-digit integer that represents the year to convert. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is outside the range supported by the calendar. </exception>
	public virtual int ToFourDigitYear(int year)
	{
		if (year < 0)
		{
			throw new ArgumentOutOfRangeException("year", Environment.GetResourceString("Non-negative number required."));
		}
		if (year < 100)
		{
			return (TwoDigitYearMax / 100 - ((year > TwoDigitYearMax % 100) ? 1 : 0)) * 100 + year;
		}
		return year;
	}

	internal static long TimeToTicks(int hour, int minute, int second, int millisecond)
	{
		if (hour >= 0 && hour < 24 && minute >= 0 && minute < 60 && second >= 0 && second < 60)
		{
			if (millisecond < 0 || millisecond >= 1000)
			{
				throw new ArgumentOutOfRangeException("millisecond", string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 0, 999));
			}
			return TimeSpan.TimeToTicks(hour, minute, second) + (long)millisecond * 10000L;
		}
		throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("Hour, Minute, and Second parameters describe an un-representable DateTime."));
	}

	[SecuritySafeCritical]
	internal static int GetSystemTwoDigitYearSetting(int CalID, int defaultYearValue)
	{
		int num = CalendarData.nativeGetTwoDigitYearMax(CalID);
		if (num < 0)
		{
			num = defaultYearValue;
		}
		return num;
	}
}
