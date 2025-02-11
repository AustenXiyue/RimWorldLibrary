using System.Globalization;
using System.Runtime.InteropServices;

namespace System;

/// <summary>Represents a time zone.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
public abstract class TimeZone
{
	private static TimeZone currentTimeZone;

	[NonSerialized]
	private static object tz_lock = new object();

	[NonSerialized]
	private static long timezone_check;

	/// <summary>Gets the time zone of the current computer.</summary>
	/// <returns>A <see cref="T:System.TimeZone" /> object that represents the current local time zone.</returns>
	/// <filterpriority>1</filterpriority>
	public static TimeZone CurrentTimeZone
	{
		get
		{
			long ticks = DateTime.UtcNow.Ticks;
			TimeZone timeZone = currentTimeZone;
			lock (tz_lock)
			{
				if (timeZone == null || Math.Abs(ticks - timezone_check) > 600000000)
				{
					timeZone = new CurrentSystemTimeZone();
					timezone_check = ticks;
					currentTimeZone = timeZone;
				}
			}
			return timeZone;
		}
	}

	/// <summary>Gets the daylight saving time zone name.</summary>
	/// <returns>The daylight saving time zone name.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract string DaylightName { get; }

	/// <summary>Gets the standard time zone name.</summary>
	/// <returns>The standard time zone name.</returns>
	/// <exception cref="T:System.ArgumentNullException">An attempt was made to set this property to null. </exception>
	/// <filterpriority>2</filterpriority>
	public abstract string StandardName { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.TimeZone" /> class.</summary>
	protected TimeZone()
	{
	}

	/// <summary>Returns the daylight saving time period for a particular year.</summary>
	/// <returns>A <see cref="T:System.Globalization.DaylightTime" /> object that contains the start and end date for daylight saving time in <paramref name="year" />.</returns>
	/// <param name="year">The year that the daylight saving time period applies to. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="year" /> is less than 1 or greater than 9999. </exception>
	/// <filterpriority>2</filterpriority>
	public abstract DaylightTime GetDaylightChanges(int year);

	/// <summary>Returns the Coordinated Universal Time (UTC) offset for the specified local time.</summary>
	/// <returns>The Coordinated Universal Time (UTC) offset from <paramref name="time" />.</returns>
	/// <param name="time">A date and time value.</param>
	/// <filterpriority>2</filterpriority>
	public abstract TimeSpan GetUtcOffset(DateTime time);

	/// <summary>Returns a value indicating whether the specified date and time is within a daylight saving time period.</summary>
	/// <returns>true if <paramref name="time" /> is in a daylight saving time period; otherwise, false.</returns>
	/// <param name="time">A date and time. </param>
	/// <filterpriority>2</filterpriority>
	public virtual bool IsDaylightSavingTime(DateTime time)
	{
		return IsDaylightSavingTime(time, GetDaylightChanges(time.Year));
	}

	/// <summary>Returns a value indicating whether the specified date and time is within the specified daylight saving time period.</summary>
	/// <returns>true if <paramref name="time" /> is in <paramref name="daylightTimes" />; otherwise, false.</returns>
	/// <param name="time">A date and time. </param>
	/// <param name="daylightTimes">A daylight saving time period. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="daylightTimes" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool IsDaylightSavingTime(DateTime time, DaylightTime daylightTimes)
	{
		if (daylightTimes == null)
		{
			throw new ArgumentNullException("daylightTimes");
		}
		if (daylightTimes.Start.Ticks == daylightTimes.End.Ticks)
		{
			return false;
		}
		if (daylightTimes.Start.Ticks < daylightTimes.End.Ticks)
		{
			if (daylightTimes.Start.Ticks < time.Ticks && daylightTimes.End.Ticks > time.Ticks)
			{
				return true;
			}
		}
		else if (time.Year == daylightTimes.Start.Year && time.Year == daylightTimes.End.Year && (time.Ticks < daylightTimes.End.Ticks || time.Ticks > daylightTimes.Start.Ticks))
		{
			return true;
		}
		return false;
	}

	/// <summary>Returns the local time that corresponds to a specified date and time value.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> object whose value is the local time that corresponds to <paramref name="time" />.</returns>
	/// <param name="time">A Coordinated Universal Time (UTC) time. </param>
	/// <filterpriority>2</filterpriority>
	public virtual DateTime ToLocalTime(DateTime time)
	{
		if (time.Kind == DateTimeKind.Local)
		{
			return time;
		}
		TimeSpan utcOffset = GetUtcOffset(new DateTime(time.Ticks));
		if (utcOffset.Ticks > 0)
		{
			if (DateTime.MaxValue - utcOffset < time)
			{
				return DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Local);
			}
		}
		else if (utcOffset.Ticks < 0 && time.Ticks + utcOffset.Ticks < DateTime.MinValue.Ticks)
		{
			return DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Local);
		}
		return DateTime.SpecifyKind(time.Add(utcOffset), DateTimeKind.Local);
	}

	/// <summary>Returns the Coordinated Universal Time (UTC) that corresponds to a specified time.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> object whose value is the Coordinated Universal Time (UTC) that corresponds to <paramref name="time" />.</returns>
	/// <param name="time">A date and time. </param>
	/// <filterpriority>2</filterpriority>
	public virtual DateTime ToUniversalTime(DateTime time)
	{
		if (time.Kind == DateTimeKind.Utc)
		{
			return time;
		}
		TimeSpan utcOffset = GetUtcOffset(time);
		if (utcOffset.Ticks < 0)
		{
			if (DateTime.MaxValue + utcOffset < time)
			{
				return DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);
			}
		}
		else if (utcOffset.Ticks > 0 && DateTime.MinValue + utcOffset > time)
		{
			return DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
		}
		return DateTime.SpecifyKind(new DateTime(time.Ticks - utcOffset.Ticks), DateTimeKind.Utc);
	}

	internal static void ClearCachedData()
	{
		currentTimeZone = null;
	}
}
