using System.Runtime.InteropServices;

namespace System.Globalization;

/// <summary>Represents the Hebrew calendar.</summary>
[Serializable]
[ComVisible(true)]
public class HebrewCalendar : Calendar
{
	internal class __DateBuffer
	{
		internal int year;

		internal int month;

		internal int day;
	}

	/// <summary>Represents the current era. This field is constant.</summary>
	public static readonly int HebrewEra = 1;

	internal const int DatePartYear = 0;

	internal const int DatePartDayOfYear = 1;

	internal const int DatePartMonth = 2;

	internal const int DatePartDay = 3;

	internal const int DatePartDayOfWeek = 4;

	private const int HebrewYearOf1AD = 3760;

	private const int FirstGregorianTableYear = 1583;

	private const int LastGregorianTableYear = 2239;

	private const int TABLESIZE = 656;

	private const int MinHebrewYear = 5343;

	private const int MaxHebrewYear = 5999;

	private static readonly int[] HebrewTable = new int[1316]
	{
		7, 3, 17, 3, 0, 4, 11, 2, 21, 6,
		1, 3, 13, 2, 25, 4, 5, 3, 16, 2,
		27, 6, 9, 1, 20, 2, 0, 6, 11, 3,
		23, 4, 4, 2, 14, 3, 27, 4, 8, 2,
		18, 3, 28, 6, 11, 1, 22, 5, 2, 3,
		12, 3, 25, 4, 6, 2, 16, 3, 26, 6,
		8, 2, 20, 1, 0, 6, 11, 2, 24, 4,
		4, 3, 15, 2, 25, 6, 8, 1, 19, 2,
		29, 6, 9, 3, 22, 4, 3, 2, 13, 3,
		25, 4, 6, 3, 17, 2, 27, 6, 7, 3,
		19, 2, 31, 4, 11, 3, 23, 4, 5, 2,
		15, 3, 25, 6, 6, 2, 19, 1, 29, 6,
		10, 2, 22, 4, 3, 3, 14, 2, 24, 6,
		6, 1, 17, 3, 28, 5, 8, 3, 20, 1,
		32, 5, 12, 3, 22, 6, 4, 1, 16, 2,
		26, 6, 6, 3, 17, 2, 0, 4, 10, 3,
		22, 4, 3, 2, 14, 3, 24, 6, 5, 2,
		17, 1, 28, 6, 9, 2, 19, 3, 31, 4,
		13, 2, 23, 6, 3, 3, 15, 1, 27, 5,
		7, 3, 17, 3, 29, 4, 11, 2, 21, 6,
		3, 1, 14, 2, 25, 6, 5, 3, 16, 2,
		28, 4, 9, 3, 20, 2, 0, 6, 12, 1,
		23, 6, 4, 2, 14, 3, 26, 4, 8, 2,
		18, 3, 0, 4, 10, 3, 21, 5, 1, 3,
		13, 1, 24, 5, 5, 3, 15, 3, 27, 4,
		8, 2, 19, 3, 29, 6, 10, 2, 22, 4,
		3, 3, 14, 2, 26, 4, 6, 3, 18, 2,
		28, 6, 10, 1, 20, 6, 2, 2, 12, 3,
		24, 4, 5, 2, 16, 3, 28, 4, 8, 3,
		19, 2, 0, 6, 12, 1, 23, 5, 3, 3,
		14, 3, 26, 4, 7, 2, 17, 3, 28, 6,
		9, 2, 21, 4, 1, 3, 13, 2, 25, 4,
		5, 3, 16, 2, 27, 6, 9, 1, 19, 3,
		0, 5, 11, 3, 23, 4, 4, 2, 14, 3,
		25, 6, 7, 1, 18, 2, 28, 6, 9, 3,
		21, 4, 2, 2, 12, 3, 25, 4, 6, 2,
		16, 3, 26, 6, 8, 2, 20, 1, 0, 6,
		11, 2, 22, 6, 4, 1, 15, 2, 25, 6,
		6, 3, 18, 1, 29, 5, 9, 3, 22, 4,
		2, 3, 13, 2, 23, 6, 4, 3, 15, 2,
		27, 4, 7, 3, 19, 2, 31, 4, 11, 3,
		21, 6, 3, 2, 15, 1, 25, 6, 6, 2,
		17, 3, 29, 4, 10, 2, 20, 6, 3, 1,
		13, 3, 24, 5, 4, 3, 16, 1, 27, 5,
		7, 3, 17, 3, 0, 4, 11, 2, 21, 6,
		1, 3, 13, 2, 25, 4, 5, 3, 16, 2,
		29, 4, 9, 3, 19, 6, 30, 2, 13, 1,
		23, 6, 4, 2, 14, 3, 27, 4, 8, 2,
		18, 3, 0, 4, 11, 3, 22, 5, 2, 3,
		14, 1, 26, 5, 6, 3, 16, 3, 28, 4,
		10, 2, 20, 6, 30, 3, 11, 2, 24, 4,
		4, 3, 15, 2, 25, 6, 8, 1, 19, 2,
		29, 6, 9, 3, 22, 4, 3, 2, 13, 3,
		25, 4, 7, 2, 17, 3, 27, 6, 9, 1,
		21, 5, 1, 3, 11, 3, 23, 4, 5, 2,
		15, 3, 25, 6, 6, 2, 19, 1, 29, 6,
		10, 2, 22, 4, 3, 3, 14, 2, 24, 6,
		6, 1, 18, 2, 28, 6, 8, 3, 20, 4,
		2, 2, 12, 3, 24, 4, 4, 3, 16, 2,
		26, 6, 6, 3, 17, 2, 0, 4, 10, 3,
		22, 4, 3, 2, 14, 3, 24, 6, 5, 2,
		17, 1, 28, 6, 9, 2, 21, 4, 1, 3,
		13, 2, 23, 6, 5, 1, 15, 3, 27, 5,
		7, 3, 19, 1, 0, 5, 10, 3, 22, 4,
		2, 3, 13, 2, 24, 6, 4, 3, 15, 2,
		27, 4, 8, 3, 20, 4, 1, 2, 11, 3,
		22, 6, 3, 2, 15, 1, 25, 6, 7, 2,
		17, 3, 29, 4, 10, 2, 21, 6, 1, 3,
		13, 1, 24, 5, 5, 3, 15, 3, 27, 4,
		8, 2, 19, 6, 1, 1, 12, 2, 22, 6,
		3, 3, 14, 2, 26, 4, 6, 3, 18, 2,
		28, 6, 10, 1, 20, 6, 2, 2, 12, 3,
		24, 4, 5, 2, 16, 3, 28, 4, 9, 2,
		19, 6, 30, 3, 12, 1, 23, 5, 3, 3,
		14, 3, 26, 4, 7, 2, 17, 3, 28, 6,
		9, 2, 21, 4, 1, 3, 13, 2, 25, 4,
		5, 3, 16, 2, 27, 6, 9, 1, 19, 6,
		30, 2, 11, 3, 23, 4, 4, 2, 14, 3,
		27, 4, 7, 3, 18, 2, 28, 6, 11, 1,
		22, 5, 2, 3, 12, 3, 25, 4, 6, 2,
		16, 3, 26, 6, 8, 2, 20, 4, 30, 3,
		11, 2, 24, 4, 4, 3, 15, 2, 25, 6,
		8, 1, 18, 3, 29, 5, 9, 3, 22, 4,
		3, 2, 13, 3, 23, 6, 6, 1, 17, 2,
		27, 6, 7, 3, 20, 4, 1, 2, 11, 3,
		23, 4, 5, 2, 15, 3, 25, 6, 6, 2,
		19, 1, 29, 6, 10, 2, 20, 6, 3, 1,
		14, 2, 24, 6, 4, 3, 17, 1, 28, 5,
		8, 3, 20, 4, 1, 3, 12, 2, 22, 6,
		2, 3, 14, 2, 26, 4, 6, 3, 17, 2,
		0, 4, 10, 3, 20, 6, 1, 2, 14, 1,
		24, 6, 5, 2, 15, 3, 28, 4, 9, 2,
		19, 6, 1, 1, 12, 3, 23, 5, 3, 3,
		15, 1, 27, 5, 7, 3, 17, 3, 29, 4,
		11, 2, 21, 6, 1, 3, 12, 2, 25, 4,
		5, 3, 16, 2, 28, 4, 9, 3, 19, 6,
		30, 2, 12, 1, 23, 6, 4, 2, 14, 3,
		26, 4, 8, 2, 18, 3, 0, 4, 10, 3,
		22, 5, 2, 3, 14, 1, 25, 5, 6, 3,
		16, 3, 28, 4, 9, 2, 20, 6, 30, 3,
		11, 2, 23, 4, 4, 3, 15, 2, 27, 4,
		7, 3, 19, 2, 29, 6, 11, 1, 21, 6,
		3, 2, 13, 3, 25, 4, 6, 2, 17, 3,
		27, 6, 9, 1, 20, 5, 30, 3, 10, 3,
		22, 4, 3, 2, 14, 3, 24, 6, 5, 2,
		17, 1, 28, 6, 9, 2, 21, 4, 1, 3,
		13, 2, 23, 6, 5, 1, 16, 2, 27, 6,
		7, 3, 19, 4, 30, 2, 11, 3, 23, 4,
		3, 3, 14, 2, 25, 6, 5, 3, 16, 2,
		28, 4, 9, 3, 21, 4, 2, 2, 12, 3,
		23, 6, 4, 2, 16, 1, 26, 6, 8, 2,
		20, 4, 30, 3, 11, 2, 22, 6, 4, 1,
		14, 3, 25, 5, 6, 3, 18, 1, 29, 5,
		9, 3, 22, 4, 2, 3, 13, 2, 23, 6,
		4, 3, 15, 2, 27, 4, 7, 3, 20, 4,
		1, 2, 11, 3, 21, 6, 3, 2, 15, 1,
		25, 6, 6, 2, 17, 3, 29, 4, 10, 2,
		20, 6, 3, 1, 13, 3, 24, 5, 4, 3,
		17, 1, 28, 5, 8, 3, 18, 6, 1, 1,
		12, 2, 22, 6, 2, 3, 14, 2, 26, 4,
		6, 3, 17, 2, 28, 6, 10, 1, 20, 6,
		1, 2, 12, 3, 24, 4, 5, 2, 15, 3,
		28, 4, 9, 2, 19, 6, 33, 3, 12, 1,
		23, 5, 3, 3, 13, 3, 25, 4, 6, 2,
		16, 3, 26, 6, 8, 2, 20, 4, 30, 3,
		11, 2, 24, 4, 4, 3, 15, 2, 25, 6,
		8, 1, 18, 6, 33, 2, 9, 3, 22, 4,
		3, 2, 13, 3, 25, 4, 6, 3, 17, 2,
		27, 6, 9, 1, 21, 5, 1, 3, 11, 3,
		23, 4, 5, 2, 15, 3, 25, 6, 6, 2,
		19, 4, 33, 3, 10, 2, 22, 4, 3, 3,
		14, 2, 24, 6, 6, 1
	};

	private static readonly int[,] LunarMonthLen = new int[7, 14]
	{
		{
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0
		},
		{
			0, 30, 29, 29, 29, 30, 29, 30, 29, 30,
			29, 30, 29, 0
		},
		{
			0, 30, 29, 30, 29, 30, 29, 30, 29, 30,
			29, 30, 29, 0
		},
		{
			0, 30, 30, 30, 29, 30, 29, 30, 29, 30,
			29, 30, 29, 0
		},
		{
			0, 30, 29, 29, 29, 30, 30, 29, 30, 29,
			30, 29, 30, 29
		},
		{
			0, 30, 29, 30, 29, 30, 30, 29, 30, 29,
			30, 29, 30, 29
		},
		{
			0, 30, 30, 30, 29, 30, 30, 29, 30, 29,
			30, 29, 30, 29
		}
	};

	internal static readonly DateTime calendarMinValue = new DateTime(1583, 1, 1);

	internal static readonly DateTime calendarMaxValue = new DateTime(new DateTime(2239, 9, 29, 23, 59, 59, 999).Ticks + 9999);

	private const int DEFAULT_TWO_DIGIT_YEAR_MAX = 5790;

	/// <summary>Gets the earliest date and time supported by the <see cref="T:System.Globalization.HebrewCalendar" /> type.</summary>
	/// <returns>The earliest date and time supported by the <see cref="T:System.Globalization.HebrewCalendar" /> type, which is equivalent to the first moment of January, 1, 1583 C.E. in the Gregorian calendar.</returns>
	public override DateTime MinSupportedDateTime => calendarMinValue;

	/// <summary>Gets the latest date and time supported by the <see cref="T:System.Globalization.HebrewCalendar" /> type.</summary>
	/// <returns>The latest date and time supported by the <see cref="T:System.Globalization.HebrewCalendar" /> type, which is equivalent to the last moment of September, 29, 2239 C.E. in the Gregorian calendar.</returns>
	public override DateTime MaxSupportedDateTime => calendarMaxValue;

	/// <summary>Gets a value that indicates whether the current calendar is solar-based, lunar-based, or a combination of both.</summary>
	/// <returns>Always returns <see cref="F:System.Globalization.CalendarAlgorithmType.LunisolarCalendar" />.</returns>
	public override CalendarAlgorithmType AlgorithmType => CalendarAlgorithmType.LunisolarCalendar;

	internal override int ID => 8;

	/// <summary>Gets the list of eras in the <see cref="T:System.Globalization.HebrewCalendar" />.</summary>
	/// <returns>An array of integers that represents the eras in the <see cref="T:System.Globalization.HebrewCalendar" /> type. The return value is always an array containing one element equal to <see cref="F:System.Globalization.HebrewCalendar.HebrewEra" />. </returns>
	public override int[] Eras => new int[1] { HebrewEra };

	/// <summary>Gets or sets the last year of a 100-year range that can be represented by a 2-digit year.</summary>
	/// <returns>The last year of a 100-year range that can be represented by a 2-digit year.</returns>
	/// <exception cref="T:System.InvalidOperationException">The current <see cref="T:System.Globalization.HebrewCalendar" /> object is read-only.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">In a set operation, the Hebrew calendar year value is less than 5343 but is not 99, or the year value is greater than 5999. </exception>
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
				twoDigitYearMax = Calendar.GetSystemTwoDigitYearSetting(ID, 5790);
			}
			return twoDigitYearMax;
		}
		set
		{
			VerifyWritable();
			if (value != 99)
			{
				CheckHebrewYearValue(value, HebrewEra, "value");
			}
			twoDigitYearMax = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Globalization.HebrewCalendar" /> class.</summary>
	public HebrewCalendar()
	{
	}

	private static void CheckHebrewYearValue(int y, int era, string varName)
	{
		CheckEraRange(era);
		if (y > 5999 || y < 5343)
		{
			throw new ArgumentOutOfRangeException(varName, string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 5343, 5999));
		}
	}

	private void CheckHebrewMonthValue(int year, int month, int era)
	{
		int monthsInYear = GetMonthsInYear(year, era);
		if (month < 1 || month > monthsInYear)
		{
			throw new ArgumentOutOfRangeException("month", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 1, monthsInYear));
		}
	}

	private void CheckHebrewDayValue(int year, int month, int day, int era)
	{
		int daysInMonth = GetDaysInMonth(year, month, era);
		if (day < 1 || day > daysInMonth)
		{
			throw new ArgumentOutOfRangeException("day", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 1, daysInMonth));
		}
	}

	internal static void CheckEraRange(int era)
	{
		if (era != 0 && era != HebrewEra)
		{
			throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("Era value was not valid."));
		}
	}

	private static void CheckTicksRange(long ticks)
	{
		if (ticks < calendarMinValue.Ticks || ticks > calendarMaxValue.Ticks)
		{
			throw new ArgumentOutOfRangeException("time", string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("Specified time is not supported in this calendar. It should be between {0} (Gregorian date) and {1} (Gregorian date), inclusive."), calendarMinValue, calendarMaxValue));
		}
	}

	internal static int GetResult(__DateBuffer result, int part)
	{
		return part switch
		{
			0 => result.year, 
			2 => result.month, 
			3 => result.day, 
			_ => throw new InvalidOperationException(Environment.GetResourceString("Internal Error in DateTime and Calendar operations.")), 
		};
	}

	internal static int GetLunarMonthDay(int gregorianYear, __DateBuffer lunarDate)
	{
		int num = gregorianYear - 1583;
		if (num < 0 || num > 656)
		{
			throw new ArgumentOutOfRangeException("gregorianYear");
		}
		num *= 2;
		lunarDate.day = HebrewTable[num];
		int result = HebrewTable[num + 1];
		switch (lunarDate.day)
		{
		case 0:
			lunarDate.month = 5;
			lunarDate.day = 1;
			break;
		case 30:
			lunarDate.month = 3;
			break;
		case 31:
			lunarDate.month = 5;
			lunarDate.day = 2;
			break;
		case 32:
			lunarDate.month = 5;
			lunarDate.day = 3;
			break;
		case 33:
			lunarDate.month = 3;
			lunarDate.day = 29;
			break;
		default:
			lunarDate.month = 4;
			break;
		}
		return result;
	}

	internal virtual int GetDatePart(long ticks, int part)
	{
		CheckTicksRange(ticks);
		DateTime dateTime = new DateTime(ticks);
		int year = dateTime.Year;
		int month = dateTime.Month;
		int day = dateTime.Day;
		__DateBuffer _DateBuffer = new __DateBuffer();
		_DateBuffer.year = year + 3760;
		int num = GetLunarMonthDay(year, _DateBuffer);
		__DateBuffer _DateBuffer2 = new __DateBuffer();
		_DateBuffer2.year = _DateBuffer.year;
		_DateBuffer2.month = _DateBuffer.month;
		_DateBuffer2.day = _DateBuffer.day;
		long absoluteDate = GregorianCalendar.GetAbsoluteDate(year, month, day);
		if (month == 1 && day == 1)
		{
			return GetResult(_DateBuffer2, part);
		}
		long num2 = absoluteDate - GregorianCalendar.GetAbsoluteDate(year, 1, 1);
		if (num2 + _DateBuffer.day <= LunarMonthLen[num, _DateBuffer.month])
		{
			_DateBuffer2.day += (int)num2;
			return GetResult(_DateBuffer2, part);
		}
		_DateBuffer2.month++;
		_DateBuffer2.day = 1;
		num2 -= LunarMonthLen[num, _DateBuffer.month] - _DateBuffer.day;
		if (num2 > 1)
		{
			while (num2 > LunarMonthLen[num, _DateBuffer2.month])
			{
				num2 -= LunarMonthLen[num, _DateBuffer2.month++];
				if (_DateBuffer2.month > 13 || LunarMonthLen[num, _DateBuffer2.month] == 0)
				{
					_DateBuffer2.year++;
					num = HebrewTable[(year + 1 - 1583) * 2 + 1];
					_DateBuffer2.month = 1;
				}
			}
			_DateBuffer2.day += (int)(num2 - 1);
		}
		return GetResult(_DateBuffer2, part);
	}

	/// <summary>Returns a <see cref="T:System.DateTime" /> that is the specified number of months away from the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>The <see cref="T:System.DateTime" /> that results from adding the specified number of months to the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to which to add <paramref name="months" />. </param>
	/// <param name="months">The number of months to add. </param>
	/// <exception cref="T:System.ArgumentException">The resulting <see cref="T:System.DateTime" /> is outside the supported range. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="months" /> is less than -120,000 or greater than 120,000. </exception>
	public override DateTime AddMonths(DateTime time, int months)
	{
		try
		{
			int num = GetDatePart(time.Ticks, 0);
			int datePart = GetDatePart(time.Ticks, 2);
			int num2 = GetDatePart(time.Ticks, 3);
			int num3;
			if (months >= 0)
			{
				int monthsInYear;
				for (num3 = datePart + months; num3 > (monthsInYear = GetMonthsInYear(num, 0)); num3 -= monthsInYear)
				{
					num++;
				}
			}
			else if ((num3 = datePart + months) <= 0)
			{
				months = -months;
				months -= datePart;
				num--;
				int monthsInYear;
				while (months > (monthsInYear = GetMonthsInYear(num, 0)))
				{
					num--;
					months -= monthsInYear;
				}
				monthsInYear = GetMonthsInYear(num, 0);
				num3 = monthsInYear - months;
			}
			int daysInMonth = GetDaysInMonth(num, num3);
			if (num2 > daysInMonth)
			{
				num2 = daysInMonth;
			}
			return new DateTime(ToDateTime(num, num3, num2, 0, 0, 0, 0).Ticks + time.Ticks % 864000000000L);
		}
		catch (ArgumentException)
		{
			throw new ArgumentOutOfRangeException("months", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Value to add was out of range.")));
		}
	}

	/// <summary>Returns a <see cref="T:System.DateTime" /> that is the specified number of years away from the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>The <see cref="T:System.DateTime" /> that results from adding the specified number of years to the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to which to add <paramref name="years" />. </param>
	/// <param name="years">The number of years to add. </param>
	/// <exception cref="T:System.ArgumentException">The resulting <see cref="T:System.DateTime" /> is outside the supported range. </exception>
	public override DateTime AddYears(DateTime time, int years)
	{
		int datePart = GetDatePart(time.Ticks, 0);
		int num = GetDatePart(time.Ticks, 2);
		int num2 = GetDatePart(time.Ticks, 3);
		datePart += years;
		CheckHebrewYearValue(datePart, 0, "years");
		int monthsInYear = GetMonthsInYear(datePart, 0);
		if (num > monthsInYear)
		{
			num = monthsInYear;
		}
		int daysInMonth = GetDaysInMonth(datePart, num);
		if (num2 > daysInMonth)
		{
			num2 = daysInMonth;
		}
		long ticks = ToDateTime(datePart, num, num2, 0, 0, 0, 0).Ticks + time.Ticks % 864000000000L;
		Calendar.CheckAddResult(ticks, MinSupportedDateTime, MaxSupportedDateTime);
		return new DateTime(ticks);
	}

	/// <summary>Returns the day of the month in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>An integer from 1 to 30 that represents the day of the month in the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
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

	internal static int GetHebrewYearType(int year, int era)
	{
		CheckHebrewYearValue(year, era, "year");
		return HebrewTable[(year - 3760 - 1583) * 2 + 1];
	}

	/// <summary>Returns the day of the year in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>An integer from 1 to 385 that represents the day of the year in the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="time" /> is earlier than September 17, 1583 in the Gregorian calendar, or greater than <see cref="P:System.Globalization.HebrewCalendar.MaxSupportedDateTime" />. </exception>
	public override int GetDayOfYear(DateTime time)
	{
		int year = GetYear(time);
		DateTime dateTime = ((year != 5343) ? ToDateTime(year, 1, 1, 0, 0, 0, 0, 0) : new DateTime(1582, 9, 27));
		return (int)((time.Ticks - dateTime.Ticks) / 864000000000L) + 1;
	}

	/// <summary>Returns the number of days in the specified month in the specified year in the specified era.</summary>
	/// <returns>The number of days in the specified month in the specified year in the specified era.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="month">An integer from 1 to 13 that represents the month. </param>
	/// <param name="era">An integer that represents the era. Specify either <see cref="F:System.Globalization.HebrewCalendar.HebrewEra" /> or Calendar.Eras[Calendar.CurrentEra].</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" />, <paramref name="month" />, or <paramref name="era" /> is outside the range supported by the current <see cref="T:System.Globalization.HebrewCalendar" /> object. </exception>
	public override int GetDaysInMonth(int year, int month, int era)
	{
		CheckEraRange(era);
		int hebrewYearType = GetHebrewYearType(year, era);
		CheckHebrewMonthValue(year, month, era);
		int num = LunarMonthLen[hebrewYearType, month];
		if (num == 0)
		{
			throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("Month must be between one and twelve."));
		}
		return num;
	}

	/// <summary>Returns the number of days in the specified year in the specified era.</summary>
	/// <returns>The number of days in the specified year in the specified era.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="era">An integer that represents the era. Specify either <see cref="F:System.Globalization.HebrewCalendar.HebrewEra" /> or HebrewCalendar.Eras[Calendar.CurrentEra].</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> or <paramref name="era" /> is outside the range supported by the current <see cref="T:System.Globalization.HebrewCalendar" /> object. </exception>
	public override int GetDaysInYear(int year, int era)
	{
		CheckEraRange(era);
		int hebrewYearType = GetHebrewYearType(year, era);
		if (hebrewYearType < 4)
		{
			return 352 + hebrewYearType;
		}
		return 382 + (hebrewYearType - 3);
	}

	/// <summary>Returns the era in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>An integer that represents the era in the specified <see cref="T:System.DateTime" />. The return value is always <see cref="F:System.Globalization.HebrewCalendar.HebrewEra" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	public override int GetEra(DateTime time)
	{
		return HebrewEra;
	}

	/// <summary>Returns the month in the specified <see cref="T:System.DateTime" />.</summary>
	/// <returns>An integer from 1 to 13 that represents the month in the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="time" /> is less than <see cref="P:System.Globalization.HebrewCalendar.MinSupportedDateTime" /> or greater than <see cref="P:System.Globalization.HebrewCalendar.MaxSupportedDateTime" />.</exception>
	public override int GetMonth(DateTime time)
	{
		return GetDatePart(time.Ticks, 2);
	}

	/// <summary>Returns the number of months in the specified year in the specified era.</summary>
	/// <returns>The number of months in the specified year in the specified era. The return value is either 12 in a common year, or 13 in a leap year.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="era">An integer that represents the era. Specify either <see cref="F:System.Globalization.HebrewCalendar.HebrewEra" /> or HebrewCalendar.Eras[Calendar.CurrentEra].</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> or <paramref name="era" /> is outside the range supported by the current <see cref="T:System.Globalization.HebrewCalendar" /> object. </exception>
	public override int GetMonthsInYear(int year, int era)
	{
		if (!IsLeapYear(year, era))
		{
			return 12;
		}
		return 13;
	}

	/// <summary>Returns the year in the specified <see cref="T:System.DateTime" /> value.</summary>
	/// <returns>An integer that represents the year in the specified <see cref="T:System.DateTime" /> value.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> to read. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="time" /> is outside the range supported by the current <see cref="T:System.Globalization.HebrewCalendar" /> object. </exception>
	public override int GetYear(DateTime time)
	{
		return GetDatePart(time.Ticks, 0);
	}

	/// <summary>Determines whether the specified date in the specified era is a leap day.</summary>
	/// <returns>true if the specified day is a leap day; otherwise, false.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="month">An integer from 1 to 13 that represents the month. </param>
	/// <param name="day">An integer from 1 to 30 that represents the day. </param>
	/// <param name="era">An integer that represents the era. Specify either <see cref="F:System.Globalization.HebrewCalendar.HebrewEra" /> or HebrewCalendar.Eras[Calendar.CurrentEra]..</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" />, <paramref name="month" />, <paramref name="day" />, or <paramref name="era" /> is outside the range supported by this calendar. </exception>
	public override bool IsLeapDay(int year, int month, int day, int era)
	{
		if (IsLeapMonth(year, month, era))
		{
			CheckHebrewDayValue(year, month, day, era);
			return true;
		}
		if (IsLeapYear(year, 0) && month == 6 && day == 30)
		{
			return true;
		}
		CheckHebrewDayValue(year, month, day, era);
		return false;
	}

	/// <summary>Calculates the leap month for a specified year and era.</summary>
	/// <returns>A positive integer that indicates the leap month in the specified year and era. The return value is 7 if the <paramref name="year" /> and <paramref name="era" /> parameters specify a leap year, or 0 if the year is not a leap year.</returns>
	/// <param name="year">A year.</param>
	/// <param name="era">An era. Specify either <see cref="F:System.Globalization.HebrewCalendar.HebrewEra" /> or HebrewCalendar.Eras[Calendar.CurrentEra].</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="era" /> is not <see cref="F:System.Globalization.HebrewCalendar.HebrewEra" /> or HebrewCalendar.Eras[Calendar.CurrentEra].-or-<paramref name="year" /> is less than the Hebrew calendar year 5343 or greater than the Hebrew calendar year 5999.</exception>
	public override int GetLeapMonth(int year, int era)
	{
		if (IsLeapYear(year, era))
		{
			return 7;
		}
		return 0;
	}

	/// <summary>Determines whether the specified month in the specified year in the specified era is a leap month.</summary>
	/// <returns>true if the specified month is a leap month; otherwise, false.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="month">An integer from 1 to 13 that represents the month. </param>
	/// <param name="era">An integer that represents the era. Specify either <see cref="F:System.Globalization.HebrewCalendar.HebrewEra" /> or HebrewCalendar.Eras[Calendar.CurrentEra].</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" />, <paramref name="month" />, or <paramref name="era" /> is outside the range supported by this calendar. </exception>
	public override bool IsLeapMonth(int year, int month, int era)
	{
		bool num = IsLeapYear(year, era);
		CheckHebrewMonthValue(year, month, era);
		if (num && month == 7)
		{
			return true;
		}
		return false;
	}

	/// <summary>Determines whether the specified year in the specified era is a leap year.</summary>
	/// <returns>true if the specified year is a leap year; otherwise, false.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="era">An integer that represents the era. Specify either <see cref="F:System.Globalization.HebrewCalendar.HebrewEra" /> or HebrewCalendar.Eras[Calendar.CurrentEra].</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> or <paramref name="era" /> is outside the range supported by this calendar. </exception>
	public override bool IsLeapYear(int year, int era)
	{
		CheckHebrewYearValue(year, era, "year");
		return (7L * (long)year + 1) % 19 < 7;
	}

	private static int GetDayDifference(int lunarYearType, int month1, int day1, int month2, int day2)
	{
		if (month1 == month2)
		{
			return day1 - day2;
		}
		bool flag = month1 > month2;
		if (flag)
		{
			int num = month1;
			int num2 = day1;
			month1 = month2;
			day1 = day2;
			month2 = num;
			day2 = num2;
		}
		int num3 = LunarMonthLen[lunarYearType, month1] - day1;
		month1++;
		while (month1 < month2)
		{
			num3 += LunarMonthLen[lunarYearType, month1++];
		}
		num3 += day2;
		if (!flag)
		{
			return -num3;
		}
		return num3;
	}

	private static DateTime HebrewToGregorian(int hebrewYear, int hebrewMonth, int hebrewDay, int hour, int minute, int second, int millisecond)
	{
		int num = hebrewYear - 3760;
		__DateBuffer _DateBuffer = new __DateBuffer();
		int lunarMonthDay = GetLunarMonthDay(num, _DateBuffer);
		if (hebrewMonth == _DateBuffer.month && hebrewDay == _DateBuffer.day)
		{
			return new DateTime(num, 1, 1, hour, minute, second, millisecond);
		}
		int dayDifference = GetDayDifference(lunarMonthDay, hebrewMonth, hebrewDay, _DateBuffer.month, _DateBuffer.day);
		return new DateTime(new DateTime(num, 1, 1).Ticks + dayDifference * 864000000000L + Calendar.TimeToTicks(hour, minute, second, millisecond));
	}

	/// <summary>Returns a <see cref="T:System.DateTime" /> that is set to the specified date and time in the specified era.</summary>
	/// <returns>The <see cref="T:System.DateTime" /> that is set to the specified date and time in the current era.</returns>
	/// <param name="year">An integer that represents the year. </param>
	/// <param name="month">An integer from 1 to 13 that represents the month. </param>
	/// <param name="day">An integer from 1 to 30 that represents the day. </param>
	/// <param name="hour">An integer from 0 to 23 that represents the hour. </param>
	/// <param name="minute">An integer from 0 to 59 that represents the minute. </param>
	/// <param name="second">An integer from 0 to 59 that represents the second. </param>
	/// <param name="millisecond">An integer from 0 to 999 that represents the millisecond. </param>
	/// <param name="era">An integer that represents the era. Specify either <see cref="F:System.Globalization.HebrewCalendar.HebrewEra" /> or HebrewCalendar.Eras[Calendar.CurrentEra].</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" />, <paramref name="month" />, <paramref name="day" /> or <paramref name="era" /> is outside the range supported by the current <see cref="T:System.Globalization.HebrewCalendar" /> object.-or- <paramref name="hour" /> is less than 0 or greater than 23.-or- <paramref name="minute" /> is less than 0 or greater than 59.-or- <paramref name="second" /> is less than 0 or greater than 59.-or- <paramref name="millisecond" /> is less than 0 or greater than 999. </exception>
	public override DateTime ToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era)
	{
		CheckHebrewYearValue(year, era, "year");
		CheckHebrewMonthValue(year, month, era);
		CheckHebrewDayValue(year, month, day, era);
		DateTime result = HebrewToGregorian(year, month, day, hour, minute, second, millisecond);
		CheckTicksRange(result.Ticks);
		return result;
	}

	/// <summary>Converts the specified year to a 4-digit year by using the <see cref="P:System.Globalization.HebrewCalendar.TwoDigitYearMax" /> property to determine the appropriate century.</summary>
	/// <returns>If the <paramref name="year" /> parameter is a 2-digit year, the return value is the corresponding 4-digit year. If the <paramref name="year" /> parameter is a 4-digit year, the return value is the unchanged <paramref name="year" /> parameter.</returns>
	/// <param name="year">A 2-digit year from 0 through 99, or a 4-digit Hebrew calendar year from 5343 through 5999.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is less than 0.-or-<paramref name="year" /> is less than <see cref="P:System.Globalization.HebrewCalendar.MinSupportedDateTime" /> or greater than <see cref="P:System.Globalization.HebrewCalendar.MaxSupportedDateTime" />. </exception>
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
		if (year > 5999 || year < 5343)
		{
			throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 5343, 5999));
		}
		return year;
	}
}
