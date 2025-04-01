using System.Runtime.InteropServices;
using System.Security;

namespace System.Globalization;

/// <summary>Represents the Hijri calendar.</summary>
[Serializable]
[ComVisible(true)]
public class HijriCalendar : Calendar
{
	/// <summary>Represents the current era. This field is constant.</summary>
	public static readonly int HijriEra = 1;

	internal const int DatePartYear = 0;

	internal const int DatePartDayOfYear = 1;

	internal const int DatePartMonth = 2;

	internal const int DatePartDay = 3;

	internal const int MinAdvancedHijri = -2;

	internal const int MaxAdvancedHijri = 2;

	internal static readonly int[] HijriMonthDays = new int[13]
	{
		0, 30, 59, 89, 118, 148, 177, 207, 236, 266,
		295, 325, 355
	};

	private const string HijriAdvanceRegKeyEntry = "AddHijriDate";

	private int m_HijriAdvance = int.MinValue;

	internal const int MaxCalendarYear = 9666;

	internal const int MaxCalendarMonth = 4;

	internal const int MaxCalendarDay = 3;

	internal static readonly DateTime calendarMinValue = new DateTime(622, 7, 18);

	internal static readonly DateTime calendarMaxValue = DateTime.MaxValue;

	private const int DEFAULT_TWO_DIGIT_YEAR_MAX = 1451;

	/// <summary>Gets the earliest date and time supported by this calendar.</summary>
	/// <returns>The earliest date and time supported by the <see cref="T:System.Globalization.HijriCalendar" /> type, which is equivalent to the first moment of July 18, 622 C.E. in the Gregorian calendar.</returns>
	[ComVisible(false)]
	public override DateTime MinSupportedDateTime => calendarMinValue;

	/// <summary>Gets the latest date and time supported by this calendar.</summary>
	/// <returns>The latest date and time supported by the <see cref="T:System.Globalization.HijriCalendar" /> type, which is equivalent to the last moment of December 31, 9999 C.E. in the Gregorian calendar.</returns>
	[ComVisible(false)]
	public override DateTime MaxSupportedDateTime => calendarMaxValue;

	/// <summary>Gets a value that indicates whether the current calendar is solar-based, lunar-based, or a combination of both.</summary>
	/// <returns>Always returns <see cref="F:System.Globalization.CalendarAlgorithmType.LunarCalendar" />.</returns>
	[ComVisible(false)]
	public override CalendarAlgorithmType AlgorithmType => CalendarAlgorithmType.LunarCalendar;

	internal override int ID => 6;

	/// <summary>Gets the number of days in the year that precedes the year that is specified by the <see cref="P:System.Globalization.HijriCalendar.MinSupportedDateTime" /> property.</summary>
	/// <returns>The number of days in the year that precedes the year specified by <see cref="P:System.Globalization.HijriCalendar.MinSupportedDateTime" />.</returns>
	protected override int DaysInYearBeforeMinSupportedYear => 354;

	/// <summary>Gets or sets the number of days to add or subtract from the calendar to accommodate the variances in the start and the end of Ramadan and to accommodate the date difference between countries/regions.</summary>
	/// <returns>An integer from -2 to 2 that represents the number of days to add or subtract from the calendar.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The property is being set to an invalid value. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int HijriAdjustment
	{
		[SecuritySafeCritical]
		get
		{
			if (m_HijriAdvance == int.MinValue)
			{
				m_HijriAdvance = GetAdvanceHijriDate();
			}
			return m_HijriAdvance;
		}
		set
		{
			if (value < -2 || value > 2)
			{
				throw new ArgumentOutOfRangeException("HijriAdjustment", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument must be between {0} and {1}."), -2, 2));
			}
			VerifyWritable();
			m_HijriAdvance = value;
		}
	}

	/// <summary>Gets the list of eras in the <see cref="T:System.Globalization.HijriCalendar" />.</summary>
	/// <returns>An array of integers that represents the eras in the <see cref="T:System.Globalization.HijriCalendar" />.</returns>
	public override int[] Eras => new int[1] { HijriEra };

	/// <summary>Gets or sets the last year of a 100-year range that can be represented by a 2-digit year.</summary>
	/// <returns>The last year of a 100-year range that can be represented by a 2-digit year.</returns>
	/// <exception cref="T:System.InvalidOperationException">This calendar is read-only.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value in a set operation is less than 100 or greater than 9666.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override int TwoDigitYearMax
	{
		get
		{
			if (twoDigitYearMax == -1)
			{
				twoDigitYearMax = Calendar.GetSystemTwoDigitYearSetting(ID, 1451);
			}
			return twoDigitYearMax;
		}
		set
		{
			VerifyWritable();
			if (value < 99 || value > 9666)
			{
				throw new ArgumentOutOfRangeException("value", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 99, 9666));
			}
			twoDigitYearMax = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Globalization.HijriCalendar" /> class.</summary>
	public HijriCalendar()
	{
	}

	private long GetAbsoluteDateHijri(int y, int m, int d)
	{
		return DaysUpToHijriYear(y) + HijriMonthDays[m - 1] + d - 1 - HijriAdjustment;
	}

	private long DaysUpToHijriYear(int HijriYear)
	{
		int num = (HijriYear - 1) / 30 * 30;
		int num2 = HijriYear - num - 1;
		long num3 = (long)num * 10631L / 30 + 227013;
		while (num2 > 0)
		{
			num3 += 354 + (IsLeapYear(num2, 0) ? 1 : 0);
			num2--;
		}
		return num3;
	}

	[SecurityCritical]
	private static int GetAdvanceHijriDate()
	{
		return 0;
	}

	internal static void CheckTicksRange(long ticks)
	{
		if (ticks < calendarMinValue.Ticks || ticks > calendarMaxValue.Ticks)
		{
			throw new ArgumentOutOfRangeException("time", string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("Specified time is not supported in this calendar. It should be between {0} (Gregorian date) and {1} (Gregorian date), inclusive."), calendarMinValue, calendarMaxValue));
		}
	}

	internal static void CheckEraRange(int era)
	{
		if (era != 0 && era != HijriEra)
		{
			throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("Era value was not valid."));
		}
	}

	internal static void CheckYearRange(int year, int era)
	{
		CheckEraRange(era);
		if (year < 1 || year > 9666)
		{
			throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 1, 9666));
		}
	}

	internal static void CheckYearMonthRange(int year, int month, int era)
	{
		CheckYearRange(year, era);
		if (year == 9666 && month > 4)
		{
			throw new ArgumentOutOfRangeException("month", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 1, 4));
		}
		if (month < 1 || month > 12)
		{
			throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("Month must be between one and twelve."));
		}
	}

	internal virtual int GetDatePart(long ticks, int part)
	{
		CheckTicksRange(ticks);
		long num = ticks / 864000000000L + 1;
		num += HijriAdjustment;
		int num2 = (int)((num - 227013) * 30 / 10631) + 1;
		long num3 = DaysUpToHijriYear(num2);
		long num4 = GetDaysInYear(num2, 0);
		if (num < num3)
		{
			num3 -= num4;
			num2--;
		}
		else if (num == num3)
		{
			num2--;
			num3 -= GetDaysInYear(num2, 0);
		}
		else if (num > num3 + num4)
		{
			num3 += num4;
			num2++;
		}
		if (part == 0)
		{
			return num2;
		}
		int i = 1;
		num -= num3;
		if (part == 1)
		{
			return (int)num;
		}
		for (; i <= 12 && num > HijriMonthDays[i - 1]; i++)
		{
		}
		i--;
		if (part == 2)
		{
			return i;
		}
		int result = (int)(num - HijriMonthDays[i - 1]);
		if (part == 3)
		{
			return result;
		}
		throw new InvalidOperationException(Environment.GetResourceString("Internal Error in DateTime and Calendar operations."));
	}

	/// <summary>Returns a <see cref="T:System.DateTime" /> that is the specified number of months away from the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>The <see cref="T:System.DateTime" /> that results from adding the specified number of months to the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to add months to. </param>
	/// <param name="months">The number of months to add. </param>
	/// <exception cref="T:System.ArgumentException">The resulting <see cref="T:System.DateTime" />.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="months" /> is less than -120000.-or- <paramref name="months" /> is greater than 120000. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="\" />
	/// </PermissionSet>
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
		long ticks = GetAbsoluteDateHijri(datePart, datePart2, num) * 864000000000L + time.Ticks % 864000000000L;
		Calendar.CheckAddResult(ticks, MinSupportedDateTime, MaxSupportedDateTime);
		return new DateTime(ticks);
	}

	/// <summary>Returns a <see cref="T:System.DateTime" /> that is the specified number of years away from the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>The <see cref="T:System.DateTime" /> that results from adding the specified number of years to the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to add years to. </param>
	/// <param name="years">The number of years to add. </param>
	/// <exception cref="T:System.ArgumentException">The resulting <see cref="T:System.DateTime" /> is outside the supported range. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="\" />
	/// </PermissionSet>
	public override DateTime AddYears(DateTime time, int years)
	{
		return AddMonths(time, years * 12);
	}

	/// <summary>Returns the day of the month in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>An integer from 1 to 30 that represents the day of the month in the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="\" />
	/// </PermissionSet>
	public override int GetDayOfMonth(DateTime time)
	{
		return GetDatePart(time.Ticks, 3);
	}

	/// <summary>Returns the day of the week in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>A <see cref="T:System.DayOfWeek" /> value that represents the day of the week in the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	public override DayOfWeek GetDayOfWeek(DateTime time)
	{
		return (DayOfWeek)((int)(time.Ticks / 864000000000L + 1) % 7);
	}

	/// <summary>Returns the day of the year in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>An integer from 1 to 355 that represents the day of the year in the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="\" />
	/// </PermissionSet>
	public override int GetDayOfYear(DateTime time)
	{
		return GetDatePart(time.Ticks, 1);
	}

	/// <summary>Returns the number of days in the specified month of the specified year and era.</summary>
	/// <returns>The number of days in the specified month in the specified year in the specified era.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="month">An integer from 1 to 12 that represents the month. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="era" /> is outside the range supported by this calendar. -or- <paramref name="year" /> is outside the range supported by this calendar.-or- <paramref name="month" /> is outside the range supported by this calendar. </exception>
	public override int GetDaysInMonth(int year, int month, int era)
	{
		CheckYearMonthRange(year, month, era);
		if (month == 12)
		{
			if (!IsLeapYear(year, 0))
			{
				return 29;
			}
			return 30;
		}
		if (month % 2 != 1)
		{
			return 29;
		}
		return 30;
	}

	/// <summary>Returns the number of days in the specified year and era.</summary>
	/// <returns>The number of days in the specified year and era. The number of days is 354 in a common year or 355 in a leap year.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> or <paramref name="era" /> is outside the range supported by this calendar. </exception>
	public override int GetDaysInYear(int year, int era)
	{
		CheckYearRange(year, era);
		if (!IsLeapYear(year, 0))
		{
			return 354;
		}
		return 355;
	}

	/// <summary>Returns the era in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>An integer that represents the era in the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	public override int GetEra(DateTime time)
	{
		CheckTicksRange(time.Ticks);
		return HijriEra;
	}

	/// <summary>Returns the month in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>An integer from 1 to 12 that represents the month in the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="\" />
	/// </PermissionSet>
	public override int GetMonth(DateTime time)
	{
		return GetDatePart(time.Ticks, 2);
	}

	/// <summary>Returns the number of months in the specified year and era.</summary>
	/// <returns>The number of months in the specified year and era.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="era" /> is outside the range supported by this calendar. -or- <paramref name="year" /> is outside the range supported by this calendar. </exception>
	public override int GetMonthsInYear(int year, int era)
	{
		CheckYearRange(year, era);
		return 12;
	}

	/// <summary>Returns the year in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>An integer that represents the year in the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="\" />
	/// </PermissionSet>
	public override int GetYear(DateTime time)
	{
		return GetDatePart(time.Ticks, 0);
	}

	/// <summary>Determines whether the specified date is a leap day.</summary>
	/// <returns>true if the specified day is a leap day; otherwise, false.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="month">An integer from 1 to 12 that represents the month. </param>
	/// <param name="day">An integer from 1 to 30 that represents the day. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="era" /> is outside the range supported by this calendar. -or- <paramref name="year" /> is outside the range supported by this calendar.-or- <paramref name="month" /> is outside the range supported by this calendar.-or- <paramref name="day" /> is outside the range supported by this calendar. </exception>
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

	/// <summary>Calculates the leap month for a specified year and era.</summary>
	/// <returns>Always 0 because the <see cref="T:System.Globalization.HijriCalendar" /> type does not support the notion of a leap month.</returns>
	/// <param name="year">A year.</param>
	/// <param name="era">An era. Specify <see cref="F:System.Globalization.Calendar.CurrentEra" /> or <see cref="F:System.Globalization.HijriCalendar.HijriEra" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is less than the Hijri calendar year 1 or greater than the year 9666.-or-<paramref name="era" /> is not <see cref="F:System.Globalization.Calendar.CurrentEra" /> or <see cref="F:System.Globalization.HijriCalendar.HijriEra" />.</exception>
	[ComVisible(false)]
	public override int GetLeapMonth(int year, int era)
	{
		CheckYearRange(year, era);
		return 0;
	}

	/// <summary>Determines whether the specified month in the specified year and era is a leap month.</summary>
	/// <returns>This method always returns false.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="month">An integer from 1 to 12 that represents the month. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="era" /> is outside the range supported by this calendar. -or- <paramref name="year" /> is outside the range supported by this calendar.-or- <paramref name="month" /> is outside the range supported by this calendar. </exception>
	public override bool IsLeapMonth(int year, int month, int era)
	{
		CheckYearMonthRange(year, month, era);
		return false;
	}

	/// <summary>Determines whether the specified year in the specified era is a leap year.</summary>
	/// <returns>true if the specified year is a leap year; otherwise, false.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="era" /> is outside the range supported by this calendar. -or- <paramref name="year" /> is outside the range supported by this calendar. </exception>
	public override bool IsLeapYear(int year, int era)
	{
		CheckYearRange(year, era);
		return (year * 11 + 14) % 30 < 11;
	}

	/// <summary>Returns a <see cref="T:System.DateTime" /> that is set to the specified date, time, and era.</summary>
	/// <returns>The <see cref="T:System.DateTime" /> that is set to the specified date and time in the current era.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="month">An integer from 1 to 12 that represents the month. </param>
	/// <param name="day">An integer from 1 to 30 that represents the day. </param>
	/// <param name="hour">An integer from 0 to 23 that represents the hour. </param>
	/// <param name="minute">An integer from 0 to 59 that represents the minute. </param>
	/// <param name="second">An integer from 0 to 59 that represents the second. </param>
	/// <param name="millisecond">An integer from 0 to 999 that represents the millisecond. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="era" /> is outside the range supported by this calendar. -or- <paramref name="year" /> is outside the range supported by this calendar.-or- <paramref name="month" /> is outside the range supported by this calendar.-or- <paramref name="day" /> is outside the range supported by this calendar.-or- <paramref name="hour" /> is less than zero or greater than 23.-or- <paramref name="minute" /> is less than zero or greater than 59.-or- <paramref name="second" /> is less than zero or greater than 59.-or- <paramref name="millisecond" /> is less than zero or greater than 999. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="\" />
	/// </PermissionSet>
	public override DateTime ToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era)
	{
		int daysInMonth = GetDaysInMonth(year, month, era);
		if (day < 1 || day > daysInMonth)
		{
			throw new ArgumentOutOfRangeException("day", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Day must be between 1 and {0} for month {1}."), daysInMonth, month));
		}
		long absoluteDateHijri = GetAbsoluteDateHijri(year, month, day);
		if (absoluteDateHijri >= 0)
		{
			return new DateTime(absoluteDateHijri * 864000000000L + Calendar.TimeToTicks(hour, minute, second, millisecond));
		}
		throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("Year, Month, and Day parameters describe an un-representable DateTime."));
	}

	/// <summary>Converts the specified year to a four-digit year by using the <see cref="P:System.Globalization.HijriCalendar.TwoDigitYearMax" /> property to determine the appropriate century.</summary>
	/// <returns>An integer that contains the four-digit representation of <paramref name="year" />.</returns>
	/// <param name="year">A two-digit or four-digit integer that represents the year to convert. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is outside the range supported by this calendar. </exception>
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
		if (year > 9666)
		{
			throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 1, 9666));
		}
		return year;
	}
}
