using System.Runtime.InteropServices;
using System.Security;

namespace System.Globalization;

/// <summary>Represents the Japanese calendar.</summary>
[Serializable]
[ComVisible(true)]
public class JapaneseCalendar : Calendar
{
	internal static readonly DateTime calendarMinValue = new DateTime(1868, 9, 8);

	internal static volatile EraInfo[] japaneseEraInfo;

	private const string c_japaneseErasHive = "System\\CurrentControlSet\\Control\\Nls\\Calendars\\Japanese\\Eras";

	private const string c_japaneseErasHivePermissionList = "HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Control\\Nls\\Calendars\\Japanese\\Eras";

	internal static volatile Calendar s_defaultInstance;

	internal GregorianCalendarHelper helper;

	private const int DEFAULT_TWO_DIGIT_YEAR_MAX = 99;

	/// <summary>Gets the earliest date and time supported by the current <see cref="T:System.Globalization.JapaneseCalendar" /> object.</summary>
	/// <returns>The earliest date and time supported by the <see cref="T:System.Globalization.JapaneseCalendar" /> type, which is equivalent to the first moment of September 8, 1868 C.E. in the Gregorian calendar. </returns>
	[ComVisible(false)]
	public override DateTime MinSupportedDateTime => calendarMinValue;

	/// <summary>Gets the latest date and time supported by the current <see cref="T:System.Globalization.JapaneseCalendar" /> object.</summary>
	/// <returns>The latest date and time supported by the <see cref="T:System.Globalization.JapaneseCalendar" /> type, which is equivalent to the last moment of December 31, 9999 C.E. in the Gregorian calendar.</returns>
	[ComVisible(false)]
	public override DateTime MaxSupportedDateTime => DateTime.MaxValue;

	/// <summary>Gets a value that indicates whether the current calendar is solar-based, lunar-based, or a combination of both.</summary>
	/// <returns>Always returns <see cref="F:System.Globalization.CalendarAlgorithmType.SolarCalendar" />.</returns>
	[ComVisible(false)]
	public override CalendarAlgorithmType AlgorithmType => CalendarAlgorithmType.SolarCalendar;

	internal override int ID => 3;

	/// <summary>Gets the list of eras in the <see cref="T:System.Globalization.JapaneseCalendar" />.</summary>
	/// <returns>An array of integers that represents the eras in the <see cref="T:System.Globalization.JapaneseCalendar" />.</returns>
	public override int[] Eras => helper.Eras;

	/// <summary>Gets or sets the last year of a 100-year range that can be represented by a 2-digit year.</summary>
	/// <returns>The last year of a 100-year range that can be represented by a 2-digit year.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified in a set operation is less than 99. -or- The value specified in a set operation is greater than 8011 (or MaxSupportedDateTime.Year).</exception>
	/// <exception cref="T:System.InvalidOperationException">In a set operation, the current instance is read-only.</exception>
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
				twoDigitYearMax = Calendar.GetSystemTwoDigitYearSetting(ID, 99);
			}
			return twoDigitYearMax;
		}
		set
		{
			VerifyWritable();
			if (value < 99 || value > helper.MaxYear)
			{
				throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 99, helper.MaxYear));
			}
			twoDigitYearMax = value;
		}
	}

	internal static EraInfo[] GetEraInfo()
	{
		if (japaneseEraInfo == null)
		{
			japaneseEraInfo = GetErasFromRegistry();
			if (japaneseEraInfo == null)
			{
				japaneseEraInfo = new EraInfo[4]
				{
					new EraInfo(4, 1989, 1, 8, 1988, 1, 8011, "平成", "平", "H"),
					new EraInfo(3, 1926, 12, 25, 1925, 1, 64, "昭和", "昭", "S"),
					new EraInfo(2, 1912, 7, 30, 1911, 1, 15, "大正", "大", "T"),
					new EraInfo(1, 1868, 1, 1, 1867, 1, 45, "明治", "明", "M")
				};
			}
		}
		return japaneseEraInfo;
	}

	[SecuritySafeCritical]
	private static EraInfo[] GetErasFromRegistry()
	{
		return null;
	}

	private static int CompareEraRanges(EraInfo a, EraInfo b)
	{
		return b.ticks.CompareTo(a.ticks);
	}

	private static EraInfo GetEraFromValue(string value, string data)
	{
		if (value == null || data == null)
		{
			return null;
		}
		if (value.Length != 10)
		{
			return null;
		}
		if (!Number.TryParseInt32(value.Substring(0, 4), NumberStyles.None, NumberFormatInfo.InvariantInfo, out var result) || !Number.TryParseInt32(value.Substring(5, 2), NumberStyles.None, NumberFormatInfo.InvariantInfo, out var result2) || !Number.TryParseInt32(value.Substring(8, 2), NumberStyles.None, NumberFormatInfo.InvariantInfo, out var result3))
		{
			return null;
		}
		string[] array = data.Split('_');
		if (array.Length != 4)
		{
			return null;
		}
		if (array[0].Length == 0 || array[1].Length == 0 || array[2].Length == 0 || array[3].Length == 0)
		{
			return null;
		}
		return new EraInfo(0, result, result2, result3, result - 1, 1, 0, array[0], array[1], array[3]);
	}

	internal static Calendar GetDefaultInstance()
	{
		if (s_defaultInstance == null)
		{
			s_defaultInstance = new JapaneseCalendar();
		}
		return s_defaultInstance;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Globalization.JapaneseCalendar" /> class.</summary>
	/// <exception cref="T:System.TypeInitializationException">Unable to initialize a <see cref="T:System.Globalization.JapaneseCalendar" /> object because of missing culture information.</exception>
	public JapaneseCalendar()
	{
		try
		{
			new CultureInfo("ja-JP");
		}
		catch (ArgumentException innerException)
		{
			throw new TypeInitializationException(GetType().FullName, innerException);
		}
		helper = new GregorianCalendarHelper(this, GetEraInfo());
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
		return helper.AddMonths(time, months);
	}

	/// <summary>Returns a <see cref="T:System.DateTime" /> that is the specified number of years away from the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>The <see cref="T:System.DateTime" /> that results from adding the specified number of years to the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to which to add years. </param>
	/// <param name="years">The number of years to add. </param>
	/// <exception cref="T:System.ArgumentException">The resulting <see cref="T:System.DateTime" /> is outside the supported range. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="time" /> is outside the supported range of the <see cref="T:System.Globalization.JapaneseCalendar" /> type.-or-<paramref name="years" /> is less than -10,000 or greater than 10,000. </exception>
	public override DateTime AddYears(DateTime time, int years)
	{
		return helper.AddYears(time, years);
	}

	/// <summary>Returns the number of days in the specified month in the specified year in the specified era.</summary>
	/// <returns>The number of days in the specified month in the specified year in the specified era.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="month">An integer from 1 to 12 that represents the month. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is outside the range supported by the calendar.-or- <paramref name="month" /> is outside the range supported by the calendar.-or- <paramref name="era" /> is outside the range supported by the calendar. </exception>
	public override int GetDaysInMonth(int year, int month, int era)
	{
		return helper.GetDaysInMonth(year, month, era);
	}

	/// <summary>Returns the number of days in the specified year in the specified era.</summary>
	/// <returns>The number of days in the specified year in the specified era.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is outside the range supported by the calendar.-or- <paramref name="era" /> is outside the range supported by the calendar. </exception>
	public override int GetDaysInYear(int year, int era)
	{
		return helper.GetDaysInYear(year, era);
	}

	/// <summary>Returns the day of the month in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>An integer from 1 to 31 that represents the day of the month in the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	public override int GetDayOfMonth(DateTime time)
	{
		return helper.GetDayOfMonth(time);
	}

	/// <summary>Returns the day of the week in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>A <see cref="T:System.DayOfWeek" /> value that represents the day of the week in the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	public override DayOfWeek GetDayOfWeek(DateTime time)
	{
		return helper.GetDayOfWeek(time);
	}

	/// <summary>Returns the day of the year in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>An integer from 1 to 366 that represents the day of the year in the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	public override int GetDayOfYear(DateTime time)
	{
		return helper.GetDayOfYear(time);
	}

	/// <summary>Returns the number of months in the specified year in the specified era.</summary>
	/// <returns>The return value is always 12.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is outside the range supported by the calendar.-or- <paramref name="era" /> is outside the range supported by the calendar. </exception>
	public override int GetMonthsInYear(int year, int era)
	{
		return helper.GetMonthsInYear(year, era);
	}

	/// <summary>Returns the week of the year that includes the date in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>A 1-based integer that represents the week of the year that includes the date in the <paramref name="time" /> parameter.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	/// <param name="rule">One of the <see cref="T:System.Globalization.CalendarWeekRule" /> values that defines a calendar week. </param>
	/// <param name="firstDayOfWeek">One of the <see cref="T:System.DayOfWeek" /> values that represents the first day of the week. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="time" /> or <paramref name="firstDayOfWeek" /> is outside the range supported by the calendar.-or- <paramref name="rule" /> is not a valid <see cref="T:System.Globalization.CalendarWeekRule" /> value. </exception>
	[ComVisible(false)]
	public override int GetWeekOfYear(DateTime time, CalendarWeekRule rule, DayOfWeek firstDayOfWeek)
	{
		return helper.GetWeekOfYear(time, rule, firstDayOfWeek);
	}

	/// <summary>Returns the era in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>An integer that represents the era in the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTime" /> is outside the supported range. </exception>
	public override int GetEra(DateTime time)
	{
		return helper.GetEra(time);
	}

	/// <summary>Returns the month in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>An integer from 1 to 12 that represents the month in the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	public override int GetMonth(DateTime time)
	{
		return helper.GetMonth(time);
	}

	/// <summary>Returns the year in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>An integer that represents the year in the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	public override int GetYear(DateTime time)
	{
		return helper.GetYear(time);
	}

	/// <summary>Determines whether the specified date in the specified era is a leap day.</summary>
	/// <returns>true, if the specified day is a leap day; otherwise, false.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="month">An integer from 1 to 12 that represents the month. </param>
	/// <param name="day">An integer from 1 to 31 that represents the day. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is outside the range supported by the calendar.-or- <paramref name="month" /> is outside the range supported by the calendar.-or- <paramref name="day" /> is outside the range supported by the calendar.-or- <paramref name="era" /> is outside the range supported by the calendar. </exception>
	public override bool IsLeapDay(int year, int month, int day, int era)
	{
		return helper.IsLeapDay(year, month, day, era);
	}

	/// <summary>Determines whether the specified year in the specified era is a leap year.</summary>
	/// <returns>true, if the specified year is a leap year; otherwise, false.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is outside the range supported by the calendar.-or- <paramref name="era" /> is outside the range supported by the calendar. </exception>
	public override bool IsLeapYear(int year, int era)
	{
		return helper.IsLeapYear(year, era);
	}

	/// <summary>Calculates the leap month for a specified year and era.</summary>
	/// <returns>The return value is always 0 because the <see cref="T:System.Globalization.JapaneseCalendar" /> type does not support the notion of a leap month.</returns>
	/// <param name="year">A year.</param>
	/// <param name="era">An era.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> or <paramref name="era" /> is outside the range supported by the <see cref="T:System.Globalization.JapaneseCalendar" /> type.</exception>
	[ComVisible(false)]
	public override int GetLeapMonth(int year, int era)
	{
		return helper.GetLeapMonth(year, era);
	}

	/// <summary>Determines whether the specified month in the specified year in the specified era is a leap month.</summary>
	/// <returns>This method always returns false, unless overridden by a derived class.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="month">An integer from 1 to 12 that represents the month. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is outside the range supported by the calendar.-or- <paramref name="month" /> is outside the range supported by the calendar.-or- <paramref name="era" /> is outside the range supported by the calendar. </exception>
	public override bool IsLeapMonth(int year, int month, int era)
	{
		return helper.IsLeapMonth(year, month, era);
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
	///   <paramref name="year" /> is outside the range supported by the calendar.-or- <paramref name="month" /> is outside the range supported by the calendar.-or- <paramref name="day" /> is outside the range supported by the calendar.-or- <paramref name="hour" /> is less than zero or greater than 23.-or- <paramref name="minute" /> is less than zero or greater than 59.-or- <paramref name="second" /> is less than zero or greater than 59.-or- <paramref name="millisecond" /> is less than zero or greater than 999.-or- <paramref name="era" /> is outside the range supported by the calendar. </exception>
	public override DateTime ToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era)
	{
		return helper.ToDateTime(year, month, day, hour, minute, second, millisecond, era);
	}

	/// <summary>Converts the specified year to a four-digit year by using the <see cref="P:System.Globalization.JapaneseCalendar.TwoDigitYearMax" /> property to determine the appropriate century.</summary>
	/// <returns>An integer that contains the four-digit representation of <paramref name="year" />.</returns>
	/// <param name="year">An integer (usually two digits) that represents the year to convert. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is outside the range supported by the calendar. </exception>
	public override int ToFourDigitYear(int year)
	{
		if (year <= 0)
		{
			throw new ArgumentOutOfRangeException("year", Environment.GetResourceString("Positive number required."));
		}
		if (year > helper.MaxYear)
		{
			throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 1, helper.MaxYear));
		}
		return year;
	}

	internal static string[] EraNames()
	{
		EraInfo[] eraInfo = GetEraInfo();
		string[] array = new string[eraInfo.Length];
		for (int i = 0; i < eraInfo.Length; i++)
		{
			array[i] = eraInfo[eraInfo.Length - i - 1].eraName;
		}
		return array;
	}

	internal static string[] AbbrevEraNames()
	{
		EraInfo[] eraInfo = GetEraInfo();
		string[] array = new string[eraInfo.Length];
		for (int i = 0; i < eraInfo.Length; i++)
		{
			array[i] = eraInfo[eraInfo.Length - i - 1].abbrevEraName;
		}
		return array;
	}

	internal static string[] EnglishEraNames()
	{
		EraInfo[] eraInfo = GetEraInfo();
		string[] array = new string[eraInfo.Length];
		for (int i = 0; i < eraInfo.Length; i++)
		{
			array[i] = eraInfo[eraInfo.Length - i - 1].englishEraName;
		}
		return array;
	}

	internal override bool IsValidYear(int year, int era)
	{
		return helper.IsValidYear(year, era);
	}
}
