namespace System.Globalization;

/// <summary>Represents the Persian calendar.</summary>
[Serializable]
public class PersianCalendar : Calendar
{
	/// <summary>Represents the current era. This field is constant.</summary>
	public static readonly int PersianEra = 1;

	internal static long PersianEpoch = new DateTime(622, 3, 22).Ticks / 864000000000L;

	private const int ApproximateHalfYear = 180;

	internal const int DatePartYear = 0;

	internal const int DatePartDayOfYear = 1;

	internal const int DatePartMonth = 2;

	internal const int DatePartDay = 3;

	internal const int MonthsPerYear = 12;

	internal static int[] DaysToMonth = new int[13]
	{
		0, 31, 62, 93, 124, 155, 186, 216, 246, 276,
		306, 336, 366
	};

	internal const int MaxCalendarYear = 9378;

	internal const int MaxCalendarMonth = 10;

	internal const int MaxCalendarDay = 13;

	internal static DateTime minDate = new DateTime(622, 3, 22);

	internal static DateTime maxDate = DateTime.MaxValue;

	private const int DEFAULT_TWO_DIGIT_YEAR_MAX = 1410;

	/// <summary>Gets the earliest date and time supported by the <see cref="T:System.Globalization.PersianCalendar" /> class.</summary>
	/// <returns>The earliest date and time supported by the <see cref="T:System.Globalization.PersianCalendar" /> class, which is equivalent to the first moment of March 21, 622 C.E. in the Gregorian calendar.</returns>
	public override DateTime MinSupportedDateTime => minDate;

	/// <summary>Gets the latest date and time supported by the <see cref="T:System.Globalization.PersianCalendar" /> class.</summary>
	/// <returns>The latest date and time supported by the <see cref="T:System.Globalization.PersianCalendar" /> class, which is equivalent to the last moment of December 31, 9999 C.E. in the Gregorian calendar.</returns>
	public override DateTime MaxSupportedDateTime => maxDate;

	/// <summary>Gets a value indicating whether the current calendar is solar-based, lunar-based, or lunisolar-based.</summary>
	/// <returns>Always returns <see cref="F:System.Globalization.CalendarAlgorithmType.SolarCalendar" />.</returns>
	public override CalendarAlgorithmType AlgorithmType => CalendarAlgorithmType.SolarCalendar;

	internal override int BaseCalendarID => 1;

	internal override int ID => 22;

	/// <summary>Gets the list of eras in a <see cref="T:System.Globalization.PersianCalendar" /> object.</summary>
	/// <returns>An array of integers that represents the eras in a <see cref="T:System.Globalization.PersianCalendar" /> object. The array consists of a single element having a value of <see cref="F:System.Globalization.PersianCalendar.PersianEra" />.</returns>
	public override int[] Eras => new int[1] { PersianEra };

	/// <summary>Gets or sets the last year of a 100-year range that can be represented by a 2-digit year.</summary>
	/// <returns>The last year of a 100-year range that can be represented by a 2-digit year.</returns>
	/// <exception cref="T:System.InvalidOperationException">This calendar is read-only.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value in a set operation is less than 100 or greater than 9378.</exception>
	public override int TwoDigitYearMax
	{
		get
		{
			if (twoDigitYearMax == -1)
			{
				twoDigitYearMax = Calendar.GetSystemTwoDigitYearSetting(ID, 1410);
			}
			return twoDigitYearMax;
		}
		set
		{
			VerifyWritable();
			if (value < 99 || value > 9378)
			{
				throw new ArgumentOutOfRangeException("value", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 99, 9378));
			}
			twoDigitYearMax = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Globalization.PersianCalendar" /> class. </summary>
	public PersianCalendar()
	{
	}

	private long GetAbsoluteDatePersian(int year, int month, int day)
	{
		if (year >= 1 && year <= 9378 && month >= 1 && month <= 12)
		{
			int num = DaysInPreviousMonths(month) + day - 1;
			int num2 = (int)(365.242189 * (double)(year - 1));
			return CalendricalCalculationsHelper.PersianNewYearOnOrBefore(PersianEpoch + num2 + 180) + num;
		}
		throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("Year, Month, and Day parameters describe an un-representable DateTime."));
	}

	internal static void CheckTicksRange(long ticks)
	{
		if (ticks < minDate.Ticks || ticks > maxDate.Ticks)
		{
			throw new ArgumentOutOfRangeException("time", string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("Specified time is not supported in this calendar. It should be between {0} (Gregorian date) and {1} (Gregorian date), inclusive."), minDate, maxDate));
		}
	}

	internal static void CheckEraRange(int era)
	{
		if (era != 0 && era != PersianEra)
		{
			throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("Era value was not valid."));
		}
	}

	internal static void CheckYearRange(int year, int era)
	{
		CheckEraRange(era);
		if (year < 1 || year > 9378)
		{
			throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 1, 9378));
		}
	}

	internal static void CheckYearMonthRange(int year, int month, int era)
	{
		CheckYearRange(year, era);
		if (year == 9378 && month > 10)
		{
			throw new ArgumentOutOfRangeException("month", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 1, 10));
		}
		if (month < 1 || month > 12)
		{
			throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("Month must be between one and twelve."));
		}
	}

	private static int MonthFromOrdinalDay(int ordinalDay)
	{
		int i;
		for (i = 0; ordinalDay > DaysToMonth[i]; i++)
		{
		}
		return i;
	}

	private static int DaysInPreviousMonths(int month)
	{
		month--;
		return DaysToMonth[month];
	}

	internal int GetDatePart(long ticks, int part)
	{
		CheckTicksRange(ticks);
		long num = ticks / 864000000000L + 1;
		int num2 = (int)Math.Floor((double)(CalendricalCalculationsHelper.PersianNewYearOnOrBefore(num) - PersianEpoch) / 365.242189 + 0.5) + 1;
		if (part == 0)
		{
			return num2;
		}
		int num3 = (int)(num - CalendricalCalculationsHelper.GetNumberOfDays(ToDateTime(num2, 1, 1, 0, 0, 0, 0, 1)));
		if (part == 1)
		{
			return num3;
		}
		int num4 = MonthFromOrdinalDay(num3);
		if (part == 2)
		{
			return num4;
		}
		int result = num3 - DaysInPreviousMonths(num4);
		if (part == 3)
		{
			return result;
		}
		throw new InvalidOperationException(Environment.GetResourceString("Internal Error in DateTime and Calendar operations."));
	}

	/// <summary>Returns a <see cref="T:System.DateTime" /> object that is offset the specified number of months from the specified <see cref="T:System.DateTime" /> object.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> object that represents the date yielded by adding the number of months specified by the <paramref name="months" /> parameter to the date specified by the <paramref name="time" /> parameter.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to which to add months. </param>
	/// <param name="months">The positive or negative number of months to add. </param>
	/// <exception cref="T:System.ArgumentException">The resulting <see cref="T:System.DateTime" /> is outside the supported range. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="months" /> is less than -120,000 or greater than 120,000. </exception>
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
		int daysInMonth = GetDaysInMonth(datePart, datePart2);
		if (num > daysInMonth)
		{
			num = daysInMonth;
		}
		long ticks = GetAbsoluteDatePersian(datePart, datePart2, num) * 864000000000L + time.Ticks % 864000000000L;
		Calendar.CheckAddResult(ticks, MinSupportedDateTime, MaxSupportedDateTime);
		return new DateTime(ticks);
	}

	/// <summary>Returns a <see cref="T:System.DateTime" /> object that is offset the specified number of years from the specified <see cref="T:System.DateTime" /> object.</summary>
	/// <returns>The <see cref="T:System.DateTime" /> object that results from adding the specified number of years to the specified <see cref="T:System.DateTime" /> object.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to which to add years. </param>
	/// <param name="years">The positive or negative number of years to add. </param>
	/// <exception cref="T:System.ArgumentException">The resulting <see cref="T:System.DateTime" /> is outside the supported range. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="years" /> is less than -10,000 or greater than 10,000. </exception>
	public override DateTime AddYears(DateTime time, int years)
	{
		return AddMonths(time, years * 12);
	}

	/// <summary>Returns the day of the month in the specified <see cref="T:System.DateTime" /> object.</summary>
	/// <returns>An integer from 1 through 31 that represents the day of the month in the specified <see cref="T:System.DateTime" /> object.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="time" /> parameter represents a date less than <see cref="P:System.Globalization.PersianCalendar.MinSupportedDateTime" /> or greater than <see cref="P:System.Globalization.PersianCalendar.MaxSupportedDateTime" />.</exception>
	public override int GetDayOfMonth(DateTime time)
	{
		return GetDatePart(time.Ticks, 3);
	}

	/// <summary>Returns the day of the week in the specified <see cref="T:System.DateTime" /> object.</summary>
	/// <returns>A <see cref="T:System.DayOfWeek" /> value that represents the day of the week in the specified <see cref="T:System.DateTime" /> object.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	public override DayOfWeek GetDayOfWeek(DateTime time)
	{
		return (DayOfWeek)((int)(time.Ticks / 864000000000L + 1) % 7);
	}

	/// <summary>Returns the day of the year in the specified <see cref="T:System.DateTime" /> object.</summary>
	/// <returns>An integer from 1 through 366 that represents the day of the year in the specified <see cref="T:System.DateTime" /> object.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="time" /> parameter represents a date less than <see cref="P:System.Globalization.PersianCalendar.MinSupportedDateTime" /> or greater than <see cref="P:System.Globalization.PersianCalendar.MaxSupportedDateTime" />.</exception>
	public override int GetDayOfYear(DateTime time)
	{
		return GetDatePart(time.Ticks, 1);
	}

	/// <summary>Returns the number of days in the specified month of the specified year and era.</summary>
	/// <returns>The number of days in the specified month of the specified year and era.</returns>
	/// <param name="year">An integer from 1 through 9378 that represents the year. </param>
	/// <param name="month">An integer that represents the month, and ranges from 1 through 12 if <paramref name="year" /> is not 9378, or 1 through 10 if <paramref name="year" /> is 9378.</param>
	/// <param name="era">An integer from 0 through 1 that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" />, <paramref name="month" />, or <paramref name="era" /> is outside the range supported by this calendar. </exception>
	public override int GetDaysInMonth(int year, int month, int era)
	{
		CheckYearMonthRange(year, month, era);
		if (month == 10 && year == 9378)
		{
			return 13;
		}
		int num = DaysToMonth[month] - DaysToMonth[month - 1];
		if (month == 12 && !IsLeapYear(year))
		{
			num--;
		}
		return num;
	}

	/// <summary>Returns the number of days in the specified year of the specified era.</summary>
	/// <returns>The number of days in the specified year and era. The number of days is 365 in a common year or 366 in a leap year.</returns>
	/// <param name="year">An integer from 1 through 9378 that represents the year. </param>
	/// <param name="era">An integer from 0 through 1 that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> or <paramref name="era" /> is outside the range supported by this calendar. </exception>
	public override int GetDaysInYear(int year, int era)
	{
		CheckYearRange(year, era);
		if (year == 9378)
		{
			return DaysToMonth[9] + 13;
		}
		if (!IsLeapYear(year, 0))
		{
			return 365;
		}
		return 366;
	}

	/// <summary>Returns the era in the specified <see cref="T:System.DateTime" /> object.</summary>
	/// <returns>Always returns <see cref="F:System.Globalization.PersianCalendar.PersianEra" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="time" /> parameter represents a date less than <see cref="P:System.Globalization.PersianCalendar.MinSupportedDateTime" /> or greater than <see cref="P:System.Globalization.PersianCalendar.MaxSupportedDateTime" />.</exception>
	public override int GetEra(DateTime time)
	{
		CheckTicksRange(time.Ticks);
		return PersianEra;
	}

	/// <summary>Returns the month in the specified <see cref="T:System.DateTime" /> object.</summary>
	/// <returns>An integer from 1 through 12 that represents the month in the specified <see cref="T:System.DateTime" /> object.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="time" /> parameter represents a date less than <see cref="P:System.Globalization.PersianCalendar.MinSupportedDateTime" /> or greater than <see cref="P:System.Globalization.PersianCalendar.MaxSupportedDateTime" />.</exception>
	public override int GetMonth(DateTime time)
	{
		return GetDatePart(time.Ticks, 2);
	}

	/// <summary>Returns the number of months in the specified year of the specified era.</summary>
	/// <returns>Returns 10 if the <paramref name="year" /> parameter is 9378; otherwise, always returns 12.</returns>
	/// <param name="year">An integer from 1 through 9378 that represents the year. </param>
	/// <param name="era">An integer from 0 through 1 that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> or <paramref name="era" /> is outside the range supported by this calendar. </exception>
	public override int GetMonthsInYear(int year, int era)
	{
		CheckYearRange(year, era);
		if (year == 9378)
		{
			return 10;
		}
		return 12;
	}

	/// <summary>Returns the year in the specified <see cref="T:System.DateTime" /> object.</summary>
	/// <returns>An integer from 1 through 9378 that represents the year in the specified <see cref="T:System.DateTime" />. </returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="time" /> parameter represents a date less than <see cref="P:System.Globalization.PersianCalendar.MinSupportedDateTime" /> or greater than <see cref="P:System.Globalization.PersianCalendar.MaxSupportedDateTime" />.</exception>
	public override int GetYear(DateTime time)
	{
		return GetDatePart(time.Ticks, 0);
	}

	/// <summary>Determines whether the specified date is a leap day.</summary>
	/// <returns>true if the specified day is a leap day; otherwise, false.</returns>
	/// <param name="year">An integer from 1 through 9378 that represents the year. </param>
	/// <param name="month">An integer that represents the month and ranges from 1 through 12 if <paramref name="year" /> is not 9378, or 1 through 10 if <paramref name="year" /> is 9378.</param>
	/// <param name="day">An integer from 1 through 31 that represents the day. </param>
	/// <param name="era">An integer from 0 through 1 that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" />, <paramref name="month" />, <paramref name="day" />, or <paramref name="era" /> is outside the range supported by this calendar. </exception>
	public override bool IsLeapDay(int year, int month, int day, int era)
	{
		int daysInMonth = GetDaysInMonth(year, month, era);
		if (day < 1 || day > daysInMonth)
		{
			throw new ArgumentOutOfRangeException("day", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Day must be between 1 and {0} for month {1}."), daysInMonth, month));
		}
		if (IsLeapYear(year, era) && month == 12)
		{
			return day == 30;
		}
		return false;
	}

	/// <summary>Returns the leap month for a specified year and era.</summary>
	/// <returns>The return value is always 0.</returns>
	/// <param name="year">An integer from 1 through 9378 that represents the year to convert. </param>
	/// <param name="era">An integer from 0 through 1 that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> or <paramref name="era" /> is outside the range supported by this calendar. </exception>
	public override int GetLeapMonth(int year, int era)
	{
		CheckYearRange(year, era);
		return 0;
	}

	/// <summary>Determines whether the specified month in the specified year and era is a leap month.</summary>
	/// <returns>Always returns false because the <see cref="T:System.Globalization.PersianCalendar" /> class does not support the notion of a leap month.</returns>
	/// <param name="year">An integer from 1 through 9378 that represents the year. </param>
	/// <param name="month">An integer that represents the month and ranges from 1 through 12 if <paramref name="year" /> is not 9378, or 1 through 10 if <paramref name="year" /> is 9378.</param>
	/// <param name="era">An integer from 0 through 1 that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" />, <paramref name="month" />, or <paramref name="era" /> is outside the range supported by this calendar. </exception>
	public override bool IsLeapMonth(int year, int month, int era)
	{
		CheckYearMonthRange(year, month, era);
		return false;
	}

	/// <summary>Determines whether the specified year in the specified era is a leap year.</summary>
	/// <returns>true if the specified year is a leap year; otherwise, false.</returns>
	/// <param name="year">An integer from 1 through 9378 that represents the year. </param>
	/// <param name="era">An integer from 0 through 1 that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> or <paramref name="era" /> is outside the range supported by this calendar. </exception>
	public override bool IsLeapYear(int year, int era)
	{
		CheckYearRange(year, era);
		if (year == 9378)
		{
			return false;
		}
		return GetAbsoluteDatePersian(year + 1, 1, 1) - GetAbsoluteDatePersian(year, 1, 1) == 366;
	}

	/// <summary>Returns a <see cref="T:System.DateTime" /> object that is set to the specified date, time, and era.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> object that is set to the specified date and time in the current era.</returns>
	/// <param name="year">An integer from 1 through 9378 that represents the year. </param>
	/// <param name="month">An integer from 1 through 12 that represents the month. </param>
	/// <param name="day">An integer from 1 through 31 that represents the day. </param>
	/// <param name="hour">An integer from 0 through 23 that represents the hour. </param>
	/// <param name="minute">An integer from 0 through 59 that represents the minute. </param>
	/// <param name="second">An integer from 0 through 59 that represents the second. </param>
	/// <param name="millisecond">An integer from 0 through 999 that represents the millisecond. </param>
	/// <param name="era">An integer from 0 through 1 that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" />, <paramref name="month" />, <paramref name="day" />, <paramref name="hour" />, <paramref name="minute" />, <paramref name="second" />, <paramref name="millisecond" />, or <paramref name="era" /> is outside the range supported by this calendar.</exception>
	public override DateTime ToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era)
	{
		int daysInMonth = GetDaysInMonth(year, month, era);
		if (day < 1 || day > daysInMonth)
		{
			throw new ArgumentOutOfRangeException("day", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Day must be between 1 and {0} for month {1}."), daysInMonth, month));
		}
		long absoluteDatePersian = GetAbsoluteDatePersian(year, month, day);
		if (absoluteDatePersian >= 0)
		{
			return new DateTime(absoluteDatePersian * 864000000000L + Calendar.TimeToTicks(hour, minute, second, millisecond));
		}
		throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("Year, Month, and Day parameters describe an un-representable DateTime."));
	}

	/// <summary>Converts the specified year to a four-digit year representation.</summary>
	/// <returns>An integer that contains the four-digit representation of <paramref name="year" />.</returns>
	/// <param name="year">An integer from 1 through 9378 that represents the year to convert. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is less than 0 or greater than 9378. </exception>
	public override int ToFourDigitYear(int year)
	{
		if (year < 0)
		{
			throw new ArgumentOutOfRangeException("year", Environment.GetResourceString("Non-negative number required."));
		}
		if (year < 100)
		{
			return base.ToFourDigitYear(year);
		}
		if (year > 9378)
		{
			throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 1, 9378));
		}
		return year;
	}
}
