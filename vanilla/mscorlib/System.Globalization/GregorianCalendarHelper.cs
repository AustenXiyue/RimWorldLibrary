using System.Runtime.Serialization;

namespace System.Globalization;

[Serializable]
internal class GregorianCalendarHelper
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

	internal const int DatePartYear = 0;

	internal const int DatePartDayOfYear = 1;

	internal const int DatePartMonth = 2;

	internal const int DatePartDay = 3;

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

	[OptionalField(VersionAdded = 1)]
	internal int m_maxYear = 9999;

	[OptionalField(VersionAdded = 1)]
	internal int m_minYear;

	internal Calendar m_Cal;

	[OptionalField(VersionAdded = 1)]
	internal EraInfo[] m_EraInfo;

	[OptionalField(VersionAdded = 1)]
	internal int[] m_eras;

	[OptionalField(VersionAdded = 1)]
	internal DateTime m_minDate;

	internal int MaxYear => m_maxYear;

	public int[] Eras
	{
		get
		{
			if (m_eras == null)
			{
				m_eras = new int[m_EraInfo.Length];
				for (int i = 0; i < m_EraInfo.Length; i++)
				{
					m_eras[i] = m_EraInfo[i].era;
				}
			}
			return (int[])m_eras.Clone();
		}
	}

	internal GregorianCalendarHelper(Calendar cal, EraInfo[] eraInfo)
	{
		m_Cal = cal;
		m_EraInfo = eraInfo;
		m_minDate = m_Cal.MinSupportedDateTime;
		m_maxYear = m_EraInfo[0].maxEraYear;
		m_minYear = m_EraInfo[0].minEraYear;
	}

	internal int GetGregorianYear(int year, int era)
	{
		if (year < 0)
		{
			throw new ArgumentOutOfRangeException("year", Environment.GetResourceString("Non-negative number required."));
		}
		if (era == 0)
		{
			era = m_Cal.CurrentEraValue;
		}
		for (int i = 0; i < m_EraInfo.Length; i++)
		{
			if (era == m_EraInfo[i].era)
			{
				if (year < m_EraInfo[i].minEraYear || year > m_EraInfo[i].maxEraYear)
				{
					throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), m_EraInfo[i].minEraYear, m_EraInfo[i].maxEraYear));
				}
				return m_EraInfo[i].yearOffset + year;
			}
		}
		throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("Era value was not valid."));
	}

	internal bool IsValidYear(int year, int era)
	{
		if (year < 0)
		{
			return false;
		}
		if (era == 0)
		{
			era = m_Cal.CurrentEraValue;
		}
		for (int i = 0; i < m_EraInfo.Length; i++)
		{
			if (era == m_EraInfo[i].era)
			{
				if (year < m_EraInfo[i].minEraYear || year > m_EraInfo[i].maxEraYear)
				{
					return false;
				}
				return true;
			}
		}
		return false;
	}

	internal virtual int GetDatePart(long ticks, int part)
	{
		CheckTicksRange(ticks);
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

	internal static long DateToTicks(int year, int month, int day)
	{
		return GetAbsoluteDate(year, month, day) * 864000000000L;
	}

	internal static long TimeToTicks(int hour, int minute, int second, int millisecond)
	{
		if (hour >= 0 && hour < 24 && minute >= 0 && minute < 60 && second >= 0 && second < 60)
		{
			if (millisecond < 0 || millisecond >= 1000)
			{
				throw new ArgumentOutOfRangeException("millisecond", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 0, 999));
			}
			return TimeSpan.TimeToTicks(hour, minute, second) + (long)millisecond * 10000L;
		}
		throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("Hour, Minute, and Second parameters describe an un-representable DateTime."));
	}

	internal void CheckTicksRange(long ticks)
	{
		if (ticks < m_Cal.MinSupportedDateTime.Ticks || ticks > m_Cal.MaxSupportedDateTime.Ticks)
		{
			throw new ArgumentOutOfRangeException("time", string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("Specified time is not supported in this calendar. It should be between {0} (Gregorian date) and {1} (Gregorian date), inclusive."), m_Cal.MinSupportedDateTime, m_Cal.MaxSupportedDateTime));
		}
	}

	public DateTime AddMonths(DateTime time, int months)
	{
		if (months < -120000 || months > 120000)
		{
			throw new ArgumentOutOfRangeException("months", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), -120000, 120000));
		}
		CheckTicksRange(time.Ticks);
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
		Calendar.CheckAddResult(ticks, m_Cal.MinSupportedDateTime, m_Cal.MaxSupportedDateTime);
		return new DateTime(ticks);
	}

	public DateTime AddYears(DateTime time, int years)
	{
		return AddMonths(time, years * 12);
	}

	public int GetDayOfMonth(DateTime time)
	{
		return GetDatePart(time.Ticks, 3);
	}

	public DayOfWeek GetDayOfWeek(DateTime time)
	{
		CheckTicksRange(time.Ticks);
		return (DayOfWeek)((time.Ticks / 864000000000L + 1) % 7);
	}

	public int GetDayOfYear(DateTime time)
	{
		return GetDatePart(time.Ticks, 1);
	}

	public int GetDaysInMonth(int year, int month, int era)
	{
		year = GetGregorianYear(year, era);
		if (month < 1 || month > 12)
		{
			throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("Month must be between one and twelve."));
		}
		int[] array = ((year % 4 == 0 && (year % 100 != 0 || year % 400 == 0)) ? DaysToMonth366 : DaysToMonth365);
		return array[month] - array[month - 1];
	}

	public int GetDaysInYear(int year, int era)
	{
		year = GetGregorianYear(year, era);
		if (year % 4 != 0 || (year % 100 == 0 && year % 400 != 0))
		{
			return 365;
		}
		return 366;
	}

	public int GetEra(DateTime time)
	{
		long ticks = time.Ticks;
		for (int i = 0; i < m_EraInfo.Length; i++)
		{
			if (ticks >= m_EraInfo[i].ticks)
			{
				return m_EraInfo[i].era;
			}
		}
		throw new ArgumentOutOfRangeException(Environment.GetResourceString("Time value was out of era range."));
	}

	public int GetMonth(DateTime time)
	{
		return GetDatePart(time.Ticks, 2);
	}

	public int GetMonthsInYear(int year, int era)
	{
		year = GetGregorianYear(year, era);
		return 12;
	}

	public int GetYear(DateTime time)
	{
		long ticks = time.Ticks;
		int datePart = GetDatePart(ticks, 0);
		for (int i = 0; i < m_EraInfo.Length; i++)
		{
			if (ticks >= m_EraInfo[i].ticks)
			{
				return datePart - m_EraInfo[i].yearOffset;
			}
		}
		throw new ArgumentException(Environment.GetResourceString("No Era was supplied."));
	}

	public int GetYear(int year, DateTime time)
	{
		long ticks = time.Ticks;
		for (int i = 0; i < m_EraInfo.Length; i++)
		{
			if (ticks >= m_EraInfo[i].ticks)
			{
				return year - m_EraInfo[i].yearOffset;
			}
		}
		throw new ArgumentException(Environment.GetResourceString("No Era was supplied."));
	}

	public bool IsLeapDay(int year, int month, int day, int era)
	{
		if (day < 1 || day > GetDaysInMonth(year, month, era))
		{
			throw new ArgumentOutOfRangeException("day", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 1, GetDaysInMonth(year, month, era)));
		}
		if (!IsLeapYear(year, era))
		{
			return false;
		}
		if (month == 2 && day == 29)
		{
			return true;
		}
		return false;
	}

	public int GetLeapMonth(int year, int era)
	{
		year = GetGregorianYear(year, era);
		return 0;
	}

	public bool IsLeapMonth(int year, int month, int era)
	{
		year = GetGregorianYear(year, era);
		if (month < 1 || month > 12)
		{
			throw new ArgumentOutOfRangeException("month", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 1, 12));
		}
		return false;
	}

	public bool IsLeapYear(int year, int era)
	{
		year = GetGregorianYear(year, era);
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

	public DateTime ToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era)
	{
		year = GetGregorianYear(year, era);
		long ticks = DateToTicks(year, month, day) + TimeToTicks(hour, minute, second, millisecond);
		CheckTicksRange(ticks);
		return new DateTime(ticks);
	}

	public virtual int GetWeekOfYear(DateTime time, CalendarWeekRule rule, DayOfWeek firstDayOfWeek)
	{
		CheckTicksRange(time.Ticks);
		return GregorianCalendar.GetDefaultInstance().GetWeekOfYear(time, rule, firstDayOfWeek);
	}

	public int ToFourDigitYear(int year, int twoDigitYearMax)
	{
		if (year < 0)
		{
			throw new ArgumentOutOfRangeException("year", Environment.GetResourceString("Positive number required."));
		}
		if (year < 100)
		{
			int num = year % 100;
			return (twoDigitYearMax / 100 - ((num > twoDigitYearMax % 100) ? 1 : 0)) * 100 + num;
		}
		if (year < m_minYear || year > m_maxYear)
		{
			throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), m_minYear, m_maxYear));
		}
		return year;
	}
}
