using System.Runtime.InteropServices;

namespace System.Globalization;

/// <summary>Represents the Julian calendar.</summary>
[Serializable]
[ComVisible(true)]
public class JulianCalendar : Calendar
{
	/// <summary>Represents the current era. This field is constant.</summary>
	public static readonly int JulianEra = 1;

	private const int DatePartYear = 0;

	private const int DatePartDayOfYear = 1;

	private const int DatePartMonth = 2;

	private const int DatePartDay = 3;

	private const int JulianDaysPerYear = 365;

	private const int JulianDaysPer4Years = 1461;

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

	internal int MaxYear = 9999;

	/// <summary>Gets the earliest date and time supported by the <see cref="T:System.Globalization.JulianCalendar" /> class.</summary>
	/// <returns>The earliest date and time supported by the <see cref="T:System.Globalization.JulianCalendar" /> class, which is equivalent to the first moment of January 1, 0001 C.E. in the Gregorian calendar.</returns>
	[ComVisible(false)]
	public override DateTime MinSupportedDateTime => DateTime.MinValue;

	/// <summary>Gets the latest date and time supported by the <see cref="T:System.Globalization.JulianCalendar" /> class.</summary>
	/// <returns>The latest date and time supported by the <see cref="T:System.Globalization.JulianCalendar" /> class, which is equivalent to the last moment of December 31, 9999 C.E. in the Gregorian calendar.</returns>
	[ComVisible(false)]
	public override DateTime MaxSupportedDateTime => DateTime.MaxValue;

	/// <summary>Gets a value that indicates whether the current calendar is solar-based, lunar-based, or a combination of both.</summary>
	/// <returns>Always returns <see cref="F:System.Globalization.CalendarAlgorithmType.SolarCalendar" />.</returns>
	[ComVisible(false)]
	public override CalendarAlgorithmType AlgorithmType => CalendarAlgorithmType.SolarCalendar;

	internal override int ID => 13;

	/// <summary>Gets the list of eras in the <see cref="T:System.Globalization.JulianCalendar" />.</summary>
	/// <returns>An array of integers that represents the eras in the <see cref="T:System.Globalization.JulianCalendar" />.</returns>
	public override int[] Eras => new int[1] { JulianEra };

	/// <summary>Gets or sets the last year of a 100-year range that can be represented by a 2-digit year.</summary>
	/// <returns>The last year of a 100-year range that can be represented by a 2-digit year.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified in a set operation is less than 99. -or- The value specified in a set operation is greater than MaxSupportedDateTime.Year.</exception>
	/// <exception cref="T:System.InvalidOperationException">In a set operation, the current instance is read-only.</exception>
	public override int TwoDigitYearMax
	{
		get
		{
			return twoDigitYearMax;
		}
		set
		{
			VerifyWritable();
			if (value < 99 || value > MaxYear)
			{
				throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 99, MaxYear));
			}
			twoDigitYearMax = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Globalization.JulianCalendar" /> class.</summary>
	public JulianCalendar()
	{
		twoDigitYearMax = 2029;
	}

	internal static void CheckEraRange(int era)
	{
		if (era != 0 && era != JulianEra)
		{
			throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("Era value was not valid."));
		}
	}

	internal void CheckYearEraRange(int year, int era)
	{
		CheckEraRange(era);
		if (year <= 0 || year > MaxYear)
		{
			throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 1, MaxYear));
		}
	}

	internal static void CheckMonthRange(int month)
	{
		if (month < 1 || month > 12)
		{
			throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("Month must be between one and twelve."));
		}
	}

	internal static void CheckDayRange(int year, int month, int day)
	{
		if (year == 1 && month == 1 && day < 3)
		{
			throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("Year, Month, and Day parameters describe an un-representable DateTime."));
		}
		int[] array = ((year % 4 == 0) ? DaysToMonth366 : DaysToMonth365);
		int num = array[month] - array[month - 1];
		if (day < 1 || day > num)
		{
			throw new ArgumentOutOfRangeException("day", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 1, num));
		}
	}

	internal static int GetDatePart(long ticks, int part)
	{
		int num = (int)((ticks + 1728000000000L) / 864000000000L);
		int num2 = num / 1461;
		num -= num2 * 1461;
		int num3 = num / 365;
		if (num3 == 4)
		{
			num3 = 3;
		}
		if (part == 0)
		{
			return num2 * 4 + num3 + 1;
		}
		num -= num3 * 365;
		if (part == 1)
		{
			return num + 1;
		}
		int[] array = ((num3 == 3) ? DaysToMonth366 : DaysToMonth365);
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

	internal static long DateToTicks(int year, int month, int day)
	{
		int[] array = ((year % 4 == 0) ? DaysToMonth366 : DaysToMonth365);
		int num = year - 1;
		return (num * 365 + num / 4 + array[month - 1] + day - 1 - 2) * 864000000000L;
	}

	/// <summary>Returns a <see cref="T:System.DateTime" /> that is the specified number of months away from the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>The <see cref="T:System.DateTime" /> that results from adding the specified number of months to the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to which to add months. </param>
	/// <param name="months">The number of months to add. </param>
	/// <exception cref="T:System.ArgumentException">The resulting <see cref="T:System.DateTime" /> is outside the supported range. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="months" /> is less than -120000.-or- <paramref name="months" /> is greater than 120000. </exception>
	public override DateTime AddMonths(DateTime time, int months)
	{
		if (months < -120000 || months > 120000)
		{
			throw new ArgumentOutOfRangeException("months", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), -120000, 120000));
		}
		int datePart = GetDatePart(time.Ticks, 0);
		int datePart2 = GetDatePart(time.Ticks, 2);
		int num = GetDatePart(time.Ticks, 3);
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
		int[] array = ((datePart % 4 == 0 && (datePart % 100 != 0 || datePart % 400 == 0)) ? DaysToMonth366 : DaysToMonth365);
		int num3 = array[datePart2] - array[datePart2 - 1];
		if (num > num3)
		{
			num = num3;
		}
		long ticks = DateToTicks(datePart, datePart2, num) + time.Ticks % 864000000000L;
		Calendar.CheckAddResult(ticks, MinSupportedDateTime, MaxSupportedDateTime);
		return new DateTime(ticks);
	}

	/// <summary>Returns a <see cref="T:System.DateTime" /> that is the specified number of years away from the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>The <see cref="T:System.DateTime" /> that results from adding the specified number of years to the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to which to add years. </param>
	/// <param name="years">The number of years to add. </param>
	/// <exception cref="T:System.ArgumentException">The resulting <see cref="T:System.DateTime" /> is outside the supported range. </exception>
	public override DateTime AddYears(DateTime time, int years)
	{
		return AddMonths(time, years * 12);
	}

	/// <summary>Returns the day of the month in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>An integer from 1 to 31 that represents the day of the month in <paramref name="time" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	public override int GetDayOfMonth(DateTime time)
	{
		return GetDatePart(time.Ticks, 3);
	}

	/// <summary>Returns the day of the week in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>A <see cref="T:System.DayOfWeek" /> value that represents the day of the week in <paramref name="time" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	public override DayOfWeek GetDayOfWeek(DateTime time)
	{
		return (DayOfWeek)((int)(time.Ticks / 864000000000L + 1) % 7);
	}

	/// <summary>Returns the day of the year in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>An integer from 1 to 366 that represents the day of the year in <paramref name="time" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	public override int GetDayOfYear(DateTime time)
	{
		return GetDatePart(time.Ticks, 1);
	}

	/// <summary>Returns the number of days in the specified month in the specified year in the specified era.</summary>
	/// <returns>The number of days in the specified month in the specified year in the specified era.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="month">An integer from 1 to 12 that represents the month. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="era" /> is outside the range supported by the calendar. -or- <paramref name="year" /> is outside the range supported by the calendar.-or- <paramref name="month" /> is outside the range supported by the calendar. </exception>
	public override int GetDaysInMonth(int year, int month, int era)
	{
		CheckYearEraRange(year, era);
		CheckMonthRange(month);
		int[] array = ((year % 4 == 0) ? DaysToMonth366 : DaysToMonth365);
		return array[month] - array[month - 1];
	}

	/// <summary>Returns the number of days in the specified year in the specified era.</summary>
	/// <returns>The number of days in the specified year in the specified era.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="era" /> is outside the range supported by the calendar. -or- <paramref name="year" /> is outside the range supported by the calendar. </exception>
	public override int GetDaysInYear(int year, int era)
	{
		if (!IsLeapYear(year, era))
		{
			return 365;
		}
		return 366;
	}

	/// <summary>Returns the era in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>An integer that represents the era in <paramref name="time" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	public override int GetEra(DateTime time)
	{
		return JulianEra;
	}

	/// <summary>Returns the month in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>An integer from 1 to 12 that represents the month in <paramref name="time" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	public override int GetMonth(DateTime time)
	{
		return GetDatePart(time.Ticks, 2);
	}

	/// <summary>Returns the number of months in the specified year in the specified era.</summary>
	/// <returns>The number of months in the specified year in the specified era.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="era" /> is outside the range supported by the calendar. -or- <paramref name="year" /> is outside the range supported by the calendar. </exception>
	public override int GetMonthsInYear(int year, int era)
	{
		CheckYearEraRange(year, era);
		return 12;
	}

	/// <summary>Returns the year in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>An integer that represents the year in <paramref name="time" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	public override int GetYear(DateTime time)
	{
		return GetDatePart(time.Ticks, 0);
	}

	/// <summary>Determines whether the specified date in the specified era is a leap day.</summary>
	/// <returns>true if the specified day is a leap day; otherwise, false.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="month">An integer from 1 to 12 that represents the month. </param>
	/// <param name="day">An integer from 1 to 31 that represents the day. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is outside the range supported by the calendar.-or- <paramref name="month" /> is outside the range supported by the calendar.-or- <paramref name="day" /> is outside the range supported by the calendar. -or- <paramref name="era" /> is outside the range supported by the calendar. </exception>
	public override bool IsLeapDay(int year, int month, int day, int era)
	{
		CheckMonthRange(month);
		if (IsLeapYear(year, era))
		{
			CheckDayRange(year, month, day);
			if (month == 2)
			{
				return day == 29;
			}
			return false;
		}
		CheckDayRange(year, month, day);
		return false;
	}

	/// <summary>Calculates the leap month for a specified year and era.</summary>
	/// <returns>A positive integer that indicates the leap month in the specified year and era. Alternatively, this method returns zero if the calendar does not support a leap month, or if <paramref name="year" /> and <paramref name="era" /> do not specify a leap year.</returns>
	/// <param name="year">An integer that represents the year.</param>
	/// <param name="era">An integer that represents the era.</param>
	[ComVisible(false)]
	public override int GetLeapMonth(int year, int era)
	{
		CheckYearEraRange(year, era);
		return 0;
	}

	/// <summary>Determines whether the specified month in the specified year in the specified era is a leap month.</summary>
	/// <returns>This method always returns false, unless overridden by a derived class.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="month">An integer from 1 to 12 that represents the month. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is outside the range supported by the calendar.-or- <paramref name="month" /> is outside the range supported by the calendar. -or- <paramref name="era" /> is outside the range supported by the calendar. </exception>
	public override bool IsLeapMonth(int year, int month, int era)
	{
		CheckYearEraRange(year, era);
		CheckMonthRange(month);
		return false;
	}

	/// <summary>Determines whether the specified year in the specified era is a leap year.</summary>
	/// <returns>true if the specified year is a leap year; otherwise, false.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is outside the range supported by the calendar. -or- <paramref name="era" /> is outside the range supported by the calendar. </exception>
	public override bool IsLeapYear(int year, int era)
	{
		CheckYearEraRange(year, era);
		return year % 4 == 0;
	}

	/// <summary>Returns a <see cref="T:System.DateTime" /> that is set to the specified date and time in the specified era.</summary>
	/// <returns>The <see cref="T:System.DateTime" /> that is set to the specified date and time in the current era.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="month">An integer from 1 to 12 that represents the month. </param>
	/// <param name="day">An integer from 1 to 31 that represents the day. </param>
	/// <param name="hour">An integer from 0 to 23 that represents the hour. </param>
	/// <param name="minute">An integer from 0 to 59 that represents the minute. </param>
	/// <param name="second">An integer from 0 to 59 that represents the second. </param>
	/// <param name="millisecond">An integer from 0 to 999 that represents the millisecond. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is outside the range supported by the calendar.-or- <paramref name="month" /> is outside the range supported by the calendar.-or- <paramref name="day" /> is outside the range supported by the calendar.-or- <paramref name="hour" /> is less than zero or greater than 23.-or- <paramref name="minute" /> is less than zero or greater than 59.-or- <paramref name="second" /> is less than zero or greater than 59.-or- <paramref name="millisecond" /> is less than zero or greater than 999. -or- <paramref name="era" /> is outside the range supported by the calendar. </exception>
	public override DateTime ToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era)
	{
		CheckYearEraRange(year, era);
		CheckMonthRange(month);
		CheckDayRange(year, month, day);
		if (millisecond < 0 || millisecond >= 1000)
		{
			throw new ArgumentOutOfRangeException("millisecond", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 0, 999));
		}
		if (hour >= 0 && hour < 24 && minute >= 0 && minute < 60 && second >= 0 && second < 60)
		{
			return new DateTime(DateToTicks(year, month, day) + new TimeSpan(0, hour, minute, second, millisecond).Ticks);
		}
		throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("Hour, Minute, and Second parameters describe an un-representable DateTime."));
	}

	/// <summary>Converts the specified year to a four-digit year by using the <see cref="P:System.Globalization.JulianCalendar.TwoDigitYearMax" /> property to determine the appropriate century.</summary>
	/// <returns>An integer that contains the four-digit representation of <paramref name="year" />.</returns>
	/// <param name="year">A two-digit or four-digit integer that represents the year to convert. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is outside the range supported by the calendar. </exception>
	public override int ToFourDigitYear(int year)
	{
		if (year < 0)
		{
			throw new ArgumentOutOfRangeException("year", Environment.GetResourceString("Non-negative number required."));
		}
		if (year > MaxYear)
		{
			throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument must be between {0} and {1}."), 1, MaxYear));
		}
		return base.ToFourDigitYear(year);
	}
}
