using System;
using System.Runtime.InteropServices;

namespace MS.Internal.Security.RightsManagement;

[StructLayout(LayoutKind.Sequential)]
internal class SystemTime
{
	private ushort Year;

	private ushort Month;

	private ushort DayOfWeek;

	private ushort Day;

	private ushort Hour;

	private ushort Minute;

	private ushort Second;

	private ushort Milliseconds;

	internal static uint Size => 16u;

	internal SystemTime(DateTime dateTime)
	{
		Year = (ushort)dateTime.Year;
		Month = (ushort)dateTime.Month;
		DayOfWeek = (ushort)dateTime.DayOfWeek;
		Day = (ushort)dateTime.Day;
		Hour = (ushort)dateTime.Hour;
		Minute = (ushort)dateTime.Minute;
		Second = (ushort)dateTime.Second;
		Milliseconds = (ushort)dateTime.Millisecond;
	}

	internal SystemTime(byte[] dataBuffer)
	{
		Year = BitConverter.ToUInt16(dataBuffer, 0);
		Month = BitConverter.ToUInt16(dataBuffer, 2);
		DayOfWeek = BitConverter.ToUInt16(dataBuffer, 4);
		Day = BitConverter.ToUInt16(dataBuffer, 6);
		Hour = BitConverter.ToUInt16(dataBuffer, 8);
		Minute = BitConverter.ToUInt16(dataBuffer, 10);
		Second = BitConverter.ToUInt16(dataBuffer, 12);
		Milliseconds = BitConverter.ToUInt16(dataBuffer, 14);
	}

	internal DateTime GetDateTime(DateTime defaultValue)
	{
		if (Year == 0 && Month == 0 && Day == 0 && Hour == 0 && Minute == 0 && Second == 0 && Milliseconds == 0)
		{
			return defaultValue;
		}
		return new DateTime(Year, Month, Day, Hour, Minute, Second, Milliseconds);
	}
}
