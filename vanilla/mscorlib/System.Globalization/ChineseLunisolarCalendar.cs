using System.Runtime.InteropServices;

namespace System.Globalization;

/// <summary>Represents time in divisions, such as months, days, and years. Years are calculated using the Chinese calendar, while days and months are calculated using the lunisolar calendar.</summary>
[Serializable]
public class ChineseLunisolarCalendar : EastAsianLunisolarCalendar
{
	/// <summary>Specifies the era that corresponds to the current <see cref="T:System.Globalization.ChineseLunisolarCalendar" /> object.</summary>
	public const int ChineseEra = 1;

	internal const int MIN_LUNISOLAR_YEAR = 1901;

	internal const int MAX_LUNISOLAR_YEAR = 2100;

	internal const int MIN_GREGORIAN_YEAR = 1901;

	internal const int MIN_GREGORIAN_MONTH = 2;

	internal const int MIN_GREGORIAN_DAY = 19;

	internal const int MAX_GREGORIAN_YEAR = 2101;

	internal const int MAX_GREGORIAN_MONTH = 1;

	internal const int MAX_GREGORIAN_DAY = 28;

	internal static DateTime minDate = new DateTime(1901, 2, 19);

	internal static DateTime maxDate = new DateTime(new DateTime(2101, 1, 28, 23, 59, 59, 999).Ticks + 9999);

	private static readonly int[,] yinfo = new int[200, 4]
	{
		{ 0, 2, 19, 19168 },
		{ 0, 2, 8, 42352 },
		{ 5, 1, 29, 21096 },
		{ 0, 2, 16, 53856 },
		{ 0, 2, 4, 55632 },
		{ 4, 1, 25, 27304 },
		{ 0, 2, 13, 22176 },
		{ 0, 2, 2, 39632 },
		{ 2, 1, 22, 19176 },
		{ 0, 2, 10, 19168 },
		{ 6, 1, 30, 42200 },
		{ 0, 2, 18, 42192 },
		{ 0, 2, 6, 53840 },
		{ 5, 1, 26, 54568 },
		{ 0, 2, 14, 46400 },
		{ 0, 2, 3, 54944 },
		{ 2, 1, 23, 38608 },
		{ 0, 2, 11, 38320 },
		{ 7, 2, 1, 18872 },
		{ 0, 2, 20, 18800 },
		{ 0, 2, 8, 42160 },
		{ 5, 1, 28, 45656 },
		{ 0, 2, 16, 27216 },
		{ 0, 2, 5, 27968 },
		{ 4, 1, 24, 44456 },
		{ 0, 2, 13, 11104 },
		{ 0, 2, 2, 38256 },
		{ 2, 1, 23, 18808 },
		{ 0, 2, 10, 18800 },
		{ 6, 1, 30, 25776 },
		{ 0, 2, 17, 54432 },
		{ 0, 2, 6, 59984 },
		{ 5, 1, 26, 27976 },
		{ 0, 2, 14, 23248 },
		{ 0, 2, 4, 11104 },
		{ 3, 1, 24, 37744 },
		{ 0, 2, 11, 37600 },
		{ 7, 1, 31, 51560 },
		{ 0, 2, 19, 51536 },
		{ 0, 2, 8, 54432 },
		{ 6, 1, 27, 55888 },
		{ 0, 2, 15, 46416 },
		{ 0, 2, 5, 22176 },
		{ 4, 1, 25, 43736 },
		{ 0, 2, 13, 9680 },
		{ 0, 2, 2, 37584 },
		{ 2, 1, 22, 51544 },
		{ 0, 2, 10, 43344 },
		{ 7, 1, 29, 46248 },
		{ 0, 2, 17, 27808 },
		{ 0, 2, 6, 46416 },
		{ 5, 1, 27, 21928 },
		{ 0, 2, 14, 19872 },
		{ 0, 2, 3, 42416 },
		{ 3, 1, 24, 21176 },
		{ 0, 2, 12, 21168 },
		{ 8, 1, 31, 43344 },
		{ 0, 2, 18, 59728 },
		{ 0, 2, 8, 27296 },
		{ 6, 1, 28, 44368 },
		{ 0, 2, 15, 43856 },
		{ 0, 2, 5, 19296 },
		{ 4, 1, 25, 42352 },
		{ 0, 2, 13, 42352 },
		{ 0, 2, 2, 21088 },
		{ 3, 1, 21, 59696 },
		{ 0, 2, 9, 55632 },
		{ 7, 1, 30, 23208 },
		{ 0, 2, 17, 22176 },
		{ 0, 2, 6, 38608 },
		{ 5, 1, 27, 19176 },
		{ 0, 2, 15, 19152 },
		{ 0, 2, 3, 42192 },
		{ 4, 1, 23, 53864 },
		{ 0, 2, 11, 53840 },
		{ 8, 1, 31, 54568 },
		{ 0, 2, 18, 46400 },
		{ 0, 2, 7, 46752 },
		{ 6, 1, 28, 38608 },
		{ 0, 2, 16, 38320 },
		{ 0, 2, 5, 18864 },
		{ 4, 1, 25, 42168 },
		{ 0, 2, 13, 42160 },
		{ 10, 2, 2, 45656 },
		{ 0, 2, 20, 27216 },
		{ 0, 2, 9, 27968 },
		{ 6, 1, 29, 44448 },
		{ 0, 2, 17, 43872 },
		{ 0, 2, 6, 38256 },
		{ 5, 1, 27, 18808 },
		{ 0, 2, 15, 18800 },
		{ 0, 2, 4, 25776 },
		{ 3, 1, 23, 27216 },
		{ 0, 2, 10, 59984 },
		{ 8, 1, 31, 27432 },
		{ 0, 2, 19, 23232 },
		{ 0, 2, 7, 43872 },
		{ 5, 1, 28, 37736 },
		{ 0, 2, 16, 37600 },
		{ 0, 2, 5, 51552 },
		{ 4, 1, 24, 54440 },
		{ 0, 2, 12, 54432 },
		{ 0, 2, 1, 55888 },
		{ 2, 1, 22, 23208 },
		{ 0, 2, 9, 22176 },
		{ 7, 1, 29, 43736 },
		{ 0, 2, 18, 9680 },
		{ 0, 2, 7, 37584 },
		{ 5, 1, 26, 51544 },
		{ 0, 2, 14, 43344 },
		{ 0, 2, 3, 46240 },
		{ 4, 1, 23, 46416 },
		{ 0, 2, 10, 44368 },
		{ 9, 1, 31, 21928 },
		{ 0, 2, 19, 19360 },
		{ 0, 2, 8, 42416 },
		{ 6, 1, 28, 21176 },
		{ 0, 2, 16, 21168 },
		{ 0, 2, 5, 43312 },
		{ 4, 1, 25, 29864 },
		{ 0, 2, 12, 27296 },
		{ 0, 2, 1, 44368 },
		{ 2, 1, 22, 19880 },
		{ 0, 2, 10, 19296 },
		{ 6, 1, 29, 42352 },
		{ 0, 2, 17, 42208 },
		{ 0, 2, 6, 53856 },
		{ 5, 1, 26, 59696 },
		{ 0, 2, 13, 54576 },
		{ 0, 2, 3, 23200 },
		{ 3, 1, 23, 27472 },
		{ 0, 2, 11, 38608 },
		{ 11, 1, 31, 19176 },
		{ 0, 2, 19, 19152 },
		{ 0, 2, 8, 42192 },
		{ 6, 1, 28, 53848 },
		{ 0, 2, 15, 53840 },
		{ 0, 2, 4, 54560 },
		{ 5, 1, 24, 55968 },
		{ 0, 2, 12, 46496 },
		{ 0, 2, 1, 22224 },
		{ 2, 1, 22, 19160 },
		{ 0, 2, 10, 18864 },
		{ 7, 1, 30, 42168 },
		{ 0, 2, 17, 42160 },
		{ 0, 2, 6, 43600 },
		{ 5, 1, 26, 46376 },
		{ 0, 2, 14, 27936 },
		{ 0, 2, 2, 44448 },
		{ 3, 1, 23, 21936 },
		{ 0, 2, 11, 37744 },
		{ 8, 2, 1, 18808 },
		{ 0, 2, 19, 18800 },
		{ 0, 2, 8, 25776 },
		{ 6, 1, 28, 27216 },
		{ 0, 2, 15, 59984 },
		{ 0, 2, 4, 27424 },
		{ 4, 1, 24, 43872 },
		{ 0, 2, 12, 43744 },
		{ 0, 2, 2, 37600 },
		{ 3, 1, 21, 51568 },
		{ 0, 2, 9, 51552 },
		{ 7, 1, 29, 54440 },
		{ 0, 2, 17, 54432 },
		{ 0, 2, 5, 55888 },
		{ 5, 1, 26, 23208 },
		{ 0, 2, 14, 22176 },
		{ 0, 2, 3, 42704 },
		{ 4, 1, 23, 21224 },
		{ 0, 2, 11, 21200 },
		{ 8, 1, 31, 43352 },
		{ 0, 2, 19, 43344 },
		{ 0, 2, 7, 46240 },
		{ 6, 1, 27, 46416 },
		{ 0, 2, 15, 44368 },
		{ 0, 2, 5, 21920 },
		{ 4, 1, 24, 42448 },
		{ 0, 2, 12, 42416 },
		{ 0, 2, 2, 21168 },
		{ 3, 1, 22, 43320 },
		{ 0, 2, 9, 26928 },
		{ 7, 1, 29, 29336 },
		{ 0, 2, 17, 27296 },
		{ 0, 2, 6, 44368 },
		{ 5, 1, 26, 19880 },
		{ 0, 2, 14, 19296 },
		{ 0, 2, 3, 42352 },
		{ 4, 1, 24, 21104 },
		{ 0, 2, 10, 53856 },
		{ 8, 1, 30, 59696 },
		{ 0, 2, 18, 54560 },
		{ 0, 2, 7, 55968 },
		{ 6, 1, 27, 27472 },
		{ 0, 2, 15, 22224 },
		{ 0, 2, 5, 19168 },
		{ 4, 1, 25, 42216 },
		{ 0, 2, 12, 42192 },
		{ 0, 2, 1, 53584 },
		{ 2, 1, 21, 55592 },
		{ 0, 2, 9, 54560 }
	};

	/// <summary>Gets the minimum date and time supported by the <see cref="T:System.Globalization.ChineseLunisolarCalendar" /> class.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> type that represents February 19, 1901 in the Gregorian calendar, which is equivalent to the constructor, DateTime(1901, 2, 19).</returns>
	[ComVisible(false)]
	public override DateTime MinSupportedDateTime => minDate;

	/// <summary>Gets the maximum date and time supported by the <see cref="T:System.Globalization.ChineseLunisolarCalendar" /> class.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> type that represents the last moment on January 28, 2101 in the Gregorian calendar, which is approximately equal to the constructor DateTime(2101, 1, 28, 23, 59, 59, 999).</returns>
	[ComVisible(false)]
	public override DateTime MaxSupportedDateTime => maxDate;

	/// <summary>Gets the number of days in the year that precedes the year that is specified by the <see cref="P:System.Globalization.ChineseLunisolarCalendar.MinSupportedDateTime" />property.</summary>
	/// <returns>The number of days in the year that precedes the year specified by <see cref="P:System.Globalization.ChineseLunisolarCalendar.MinSupportedDateTime" />.</returns>
	protected override int DaysInYearBeforeMinSupportedYear => 384;

	internal override int MinCalendarYear => 1901;

	internal override int MaxCalendarYear => 2100;

	internal override DateTime MinDate => minDate;

	internal override DateTime MaxDate => maxDate;

	internal override EraInfo[] CalEraInfo => null;

	internal override int ID => 15;

	internal override int BaseCalendarID => 1;

	/// <summary>Gets the eras that correspond to the range of dates and times supported by the current <see cref="T:System.Globalization.ChineseLunisolarCalendar" /> object.</summary>
	/// <returns>An array of 32-bit signed integers that specify the relevant eras. The return value for a <see cref="T:System.Globalization.ChineseLunisolarCalendar" /> object is always an array containing one element equal to the <see cref="F:System.Globalization.ChineseLunisolarCalendar.ChineseEra" /> value.</returns>
	[ComVisible(false)]
	public override int[] Eras => new int[1] { 1 };

	internal override int GetYearInfo(int LunarYear, int Index)
	{
		if (LunarYear < 1901 || LunarYear > 2100)
		{
			throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 1901, 2100));
		}
		return yinfo[LunarYear - 1901, Index];
	}

	internal override int GetYear(int year, DateTime time)
	{
		return year;
	}

	internal override int GetGregorianYear(int year, int era)
	{
		if (era != 0 && era != 1)
		{
			throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("Era value was not valid."));
		}
		if (year < 1901 || year > 2100)
		{
			throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 1901, 2100));
		}
		return year;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Globalization.ChineseLunisolarCalendar" /> class. </summary>
	public ChineseLunisolarCalendar()
	{
	}

	/// <summary>Retrieves the era that corresponds to the specified <see cref="T:System.DateTime" /> type.</summary>
	/// <returns>An integer that represents the era in the <paramref name="time" /> parameter.</returns>
	/// <param name="time">The <see cref="T:System.DateTime" /> type to read. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="time" /> is less than <see cref="P:System.Globalization.ChineseLunisolarCalendar.MinSupportedDateTime" /> or greater than <see cref="P:System.Globalization.ChineseLunisolarCalendar.MaxSupportedDateTime" />.</exception>
	[ComVisible(false)]
	public override int GetEra(DateTime time)
	{
		CheckTicksRange(time.Ticks);
		return 1;
	}
}
