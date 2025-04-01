using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Globalization;

/// <summary>Represents the Gregorian calendar.</summary>
[Serializable]
[ComVisible(true)]
public class GregorianCalendar : Calendar
{
	/// <summary>Represents the current era. This field is constant.</summary>
	public const int ADEra = 1;

	internal const int DatePartYear = 0;

	internal const int DatePartDayOfYear = 1;

	internal const int DatePartMonth = 2;

	internal const int DatePartDay = 3;

	internal const int MaxYear = 9999;

	internal GregorianCalendarTypes m_type;

	internal static readonly int[] DaysToMonth365 = new int[13]
	{
		0, 31, 59, 90, 120, 151, 181, 212, 243, 273,
		304, 334, 365
	};

	internal static readonly int[] DaysToMonth366 = new int[13]
	{
		0, 31, 60, 91, 121, 152, 182, 213, 244, 274,
		305, 335, 366
	};

	private static volatile Calendar s_defaultInstance;

	private const int DEFAULT_TWO_DIGIT_YEAR_MAX = 2029;

	/// <summary>Gets the earliest date and time supported by the <see cref="T:System.Globalization.GregorianCalendar" /> type.</summary>
	/// <returns>The earliest date and time supported by the <see cref="T:System.Globalization.GregorianCalendar" /> type, which is the first moment of January 1, 0001 C.E. and is equivalent to <see cref="F:System.DateTime.MinValue" />.</returns>
	[ComVisible(false)]
	public override DateTime MinSupportedDateTime => DateTime.MinValue;

	/// <summary>Gets the latest date and time supported by the <see cref="T:System.Globalization.GregorianCalendar" /> type.</summary>
	/// <returns>The latest date and time supported by the <see cref="T:System.Globalization.GregorianCalendar" /> type, which is the last moment of December 31, 9999 C.E. and is equivalent to <see cref="F:System.DateTime.MaxValue" />.</returns>
	[ComVisible(false)]
	public override DateTime MaxSupportedDateTime => DateTime.MaxValue;

	/// <summary>Gets a value that indicates whether the current calendar is solar-based, lunar-based, or a combination of both.</summary>
	/// <returns>Always returns <see cref="F:System.Globalization.CalendarAlgorithmType.SolarCalendar" />.</returns>
	[ComVisible(false)]
	public override CalendarAlgorithmType AlgorithmType => CalendarAlgorithmType.SolarCalendar;

	/// <summary>Gets or sets the <see cref="T:System.Globalization.GregorianCalendarTypes" /> value that denotes the language version of the current <see cref="T:System.Globalization.GregorianCalendar" />.</summary>
	/// <returns>A <see cref="T:System.Globalization.GregorianCalendarTypes" /> value that denotes the language version of the current <see cref="T:System.Globalization.GregorianCalendar" />.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified in a set operation is not a member of the <see cref="T:System.Globalization.GregorianCalendarTypes" /> enumeration.</exception>
	/// <exception cref="T:System.InvalidOperationException">In a set operation, the current instance is read-only.</exception>
	public virtual GregorianCalendarTypes CalendarType
	{
		get
		{
			return m_type;
		}
		set
		{
			VerifyWritable();
			if ((uint)(value - 1) <= 1u || (uint)(value - 9) <= 3u)
			{
				m_type = value;
				return;
			}
			throw new ArgumentOutOfRangeException("m_type", Environment.GetResourceString("Enum value was out of legal range."));
		}
	}

	internal override int ID => (int)m_type;

	/// <summary>Gets the list of eras in the <see cref="T:System.Globalization.GregorianCalendar" />.</summary>
	/// <returns>An array of integers that represents the eras in the <see cref="T:System.Globalization.GregorianCalendar" />.</returns>
	public override int[] Eras => new int[1] { 1 };

	/// <summary>Gets or sets the last year of a 100-year range that can be represented by a 2-digit year.</summary>
	/// <returns>The last year of a 100-year range that can be represented by a 2-digit year.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified in a set operation is less than 99. -or- The value specified in a set operation is greater than MaxSupportedDateTime.Year.</exception>
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
				twoDigitYearMax = Calendar.GetSystemTwoDigitYearSetting(ID, 2029);
			}
			return twoDigitYearMax;
		}
		set
		{
			VerifyWritable();
			if (value < 99 || value > 9999)
			{
				throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 99, 9999));
			}
			twoDigitYearMax = value;
		}
	}

	[OnDeserialized]
	private void OnDeserialized(StreamingContext ctx)
	{
		if (m_type == (GregorianCalendarTypes)0)
		{
			m_type = GregorianCalendarTypes.Localized;
		}
		if (m_type < GregorianCalendarTypes.Localized || m_type > GregorianCalendarTypes.TransliteratedFrench)
		{
			throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("The deserialized value of the member \"{0}\" in the class \"{1}\" is out of range."), "type", "GregorianCalendar"));
		}
	}

	internal static Calendar GetDefaultInstance()
	{
		if (s_defaultInstance == null)
		{
			s_defaultInstance = new GregorianCalendar();
		}
		return s_defaultInstance;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Globalization.GregorianCalendar" /> class using the default <see cref="T:System.Globalization.GregorianCalendarTypes" /> value.</summary>
	public GregorianCalendar()
		: this(GregorianCalendarTypes.Localized)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Globalization.GregorianCalendar" /> class using the specified <see cref="T:System.Globalization.GregorianCalendarTypes" /> value.</summary>
	/// <param name="type">The <see cref="T:System.Globalization.GregorianCalendarTypes" /> value that denotes which language version of the calendar to create. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="type" /> is not a member of the <see cref="T:System.Globalization.GregorianCalendarTypes" /> enumeration.</exception>
	public GregorianCalendar(GregorianCalendarTypes type)
	{
		if (type < GregorianCalendarTypes.Localized || type > GregorianCalendarTypes.TransliteratedFrench)
		{
			throw new ArgumentOutOfRangeException("type", Environment.GetResourceString("Valid values are between {0} and {1}, inclusive.", GregorianCalendarTypes.Localized, GregorianCalendarTypes.TransliteratedFrench));
		}
		m_type = type;
	}

	internal virtual int GetDatePart(long ticks, int part)
	{
		int num = (int)(ticks / 864000000000L);
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

	internal static long GetAbsoluteDate(int year, int month, int day)
	{
		if (year >= 1 && year <= 9999 && month >= 1 && month <= 12)
		{
			int[] array = ((year % 4 == 0 && (year % 100 != 0 || year % 400 == 0)) ? DaysToMonth366 : DaysToMonth365);
			if (day >= 1 && day <= array[month] - array[month - 1])
			{
				int num = year - 1;
				return num * 365 + num / 4 - num / 100 + num / 400 + array[month - 1] + day - 1;
			}
		}
		throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("Year, Month, and Day parameters describe an un-representable DateTime."));
	}

	internal virtual long DateToTicks(int year, int month, int day)
	{
		return GetAbsoluteDate(year, month, day) * 864000000000L;
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
	///   <paramref name="era" /> is outside the range supported by the calendar.-or- <paramref name="year" /> is outside the range supported by the calendar.-or- <paramref name="month" /> is outside the range supported by the calendar. </exception>
	public override int GetDaysInMonth(int year, int month, int era)
	{
		if (era == 0 || era == 1)
		{
			if (year < 1 || year > 9999)
			{
				throw new ArgumentOutOfRangeException("year", Environment.GetResourceString("Valid values are between {0} and {1}, inclusive.", 1, 9999));
			}
			if (month < 1 || month > 12)
			{
				throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("Month must be between one and twelve."));
			}
			int[] array = ((year % 4 == 0 && (year % 100 != 0 || year % 400 == 0)) ? DaysToMonth366 : DaysToMonth365);
			return array[month] - array[month - 1];
		}
		throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("Era value was not valid."));
	}

	/// <summary>Returns the number of days in the specified year in the specified era.</summary>
	/// <returns>The number of days in the specified year in the specified era.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="era" /> is outside the range supported by the calendar.-or- <paramref name="year" /> is outside the range supported by the calendar.</exception>
	public override int GetDaysInYear(int year, int era)
	{
		if (era == 0 || era == 1)
		{
			if (year >= 1 && year <= 9999)
			{
				if (year % 4 != 0 || (year % 100 == 0 && year % 400 != 0))
				{
					return 365;
				}
				return 366;
			}
			throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 1, 9999));
		}
		throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("Era value was not valid."));
	}

	/// <summary>Returns the era in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>An integer that represents the era in <paramref name="time" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	public override int GetEra(DateTime time)
	{
		return 1;
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
	///   <paramref name="era" /> is outside the range supported by the calendar.-or- <paramref name="year" /> is outside the range supported by the calendar. </exception>
	public override int GetMonthsInYear(int year, int era)
	{
		if (era == 0 || era == 1)
		{
			if (year >= 1 && year <= 9999)
			{
				return 12;
			}
			throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 1, 9999));
		}
		throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("Era value was not valid."));
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
	///   <paramref name="era" /> is outside the range supported by the calendar. -or- <paramref name="year" /> is outside the range supported by the calendar.-or- <paramref name="month" /> is outside the range supported by the calendar.-or- <paramref name="day" /> is outside the range supported by the calendar. </exception>
	public override bool IsLeapDay(int year, int month, int day, int era)
	{
		if (month < 1 || month > 12)
		{
			throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("Valid values are between {0} and {1}, inclusive.", 1, 12));
		}
		if (era != 0 && era != 1)
		{
			throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("Era value was not valid."));
		}
		if (year < 1 || year > 9999)
		{
			throw new ArgumentOutOfRangeException("year", Environment.GetResourceString("Valid values are between {0} and {1}, inclusive.", 1, 9999));
		}
		if (day < 1 || day > GetDaysInMonth(year, month))
		{
			throw new ArgumentOutOfRangeException("day", Environment.GetResourceString("Valid values are between {0} and {1}, inclusive.", 1, GetDaysInMonth(year, month)));
		}
		if (!IsLeapYear(year))
		{
			return false;
		}
		if (month == 2 && day == 29)
		{
			return true;
		}
		return false;
	}

	/// <summary>Calculates the leap month for a specified year and era.</summary>
	/// <returns>Always 0 because the Gregorian calendar does not recognize leap months.</returns>
	/// <param name="year">A year.</param>
	/// <param name="era">An era. Specify either <see cref="F:System.Globalization.GregorianCalendar.ADEra" /> or GregorianCalendar.Eras[Calendar.CurrentEra].</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is less than the Gregorian calendar year 1 or greater than the Gregorian calendar year 9999.-or-<paramref name="era" /> is not <see cref="F:System.Globalization.GregorianCalendar.ADEra" /> or GregorianCalendar.Eras[Calendar.CurrentEra].</exception>
	[ComVisible(false)]
	public override int GetLeapMonth(int year, int era)
	{
		if (era != 0 && era != 1)
		{
			throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("Era value was not valid."));
		}
		if (year < 1 || year > 9999)
		{
			throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 1, 9999));
		}
		return 0;
	}

	/// <summary>Determines whether the specified month in the specified year in the specified era is a leap month.</summary>
	/// <returns>This method always returns false, unless overridden by a derived class.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="month">An integer from 1 to 12 that represents the month. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="era" /> is outside the range supported by the calendar.-or- <paramref name="year" /> is outside the range supported by the calendar.-or- <paramref name="month" /> is outside the range supported by the calendar. </exception>
	public override bool IsLeapMonth(int year, int month, int era)
	{
		if (era != 0 && era != 1)
		{
			throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("Era value was not valid."));
		}
		if (year < 1 || year > 9999)
		{
			throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 1, 9999));
		}
		if (month < 1 || month > 12)
		{
			throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("Valid values are between {0} and {1}, inclusive.", 1, 12));
		}
		return false;
	}

	/// <summary>Determines whether the specified year in the specified era is a leap year.</summary>
	/// <returns>true if the specified year is a leap year; otherwise, false.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="era">An integer that represents the era. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="era" /> is outside the range supported by the calendar.-or- <paramref name="year" /> is outside the range supported by the calendar. </exception>
	public override bool IsLeapYear(int year, int era)
	{
		if (era == 0 || era == 1)
		{
			if (year >= 1 && year <= 9999)
			{
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
			throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 1, 9999));
		}
		throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("Era value was not valid."));
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
	///   <paramref name="era" /> is outside the range supported by the calendar.-or- <paramref name="year" /> is outside the range supported by the calendar.-or- <paramref name="month" /> is outside the range supported by the calendar.-or- <paramref name="day" /> is outside the range supported by the calendar.-or- <paramref name="hour" /> is less than zero or greater than 23.-or- <paramref name="minute" /> is less than zero or greater than 59.-or- <paramref name="second" /> is less than zero or greater than 59.-or- <paramref name="millisecond" /> is less than zero or greater than 999. </exception>
	public override DateTime ToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era)
	{
		if (era == 0 || era == 1)
		{
			return new DateTime(year, month, day, hour, minute, second, millisecond);
		}
		throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("Era value was not valid."));
	}

	internal override bool TryToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era, out DateTime result)
	{
		if (era == 0 || era == 1)
		{
			return DateTime.TryCreate(year, month, day, hour, minute, second, millisecond, out result);
		}
		result = DateTime.MinValue;
		return false;
	}

	/// <summary>Converts the specified year to a four-digit year by using the <see cref="P:System.Globalization.GregorianCalendar.TwoDigitYearMax" /> property to determine the appropriate century.</summary>
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
		if (year > 9999)
		{
			throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 1, 9999));
		}
		return base.ToFourDigitYear(year);
	}
}
