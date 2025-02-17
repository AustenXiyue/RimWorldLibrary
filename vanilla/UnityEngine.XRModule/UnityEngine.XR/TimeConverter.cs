using System;

namespace UnityEngine.XR;

internal static class TimeConverter
{
	private static readonly DateTime s_Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public static DateTime now => DateTime.Now;

	public static long LocalDateTimeToUnixTimeMilliseconds(DateTime date)
	{
		return Convert.ToInt64((date.ToUniversalTime() - s_Epoch).TotalMilliseconds);
	}

	public static DateTime UnixTimeMillisecondsToLocalDateTime(long unixTimeInMilliseconds)
	{
		return s_Epoch.AddMilliseconds(unixTimeInMilliseconds).ToLocalTime();
	}
}
