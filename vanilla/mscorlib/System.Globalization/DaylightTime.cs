using System.Runtime.InteropServices;

namespace System.Globalization;

/// <summary>Defines the period of daylight saving time.</summary>
[Serializable]
[ComVisible(true)]
public class DaylightTime
{
	internal DateTime m_start;

	internal DateTime m_end;

	internal TimeSpan m_delta;

	/// <summary>Gets the object that represents the date and time when the daylight saving period begins.</summary>
	/// <returns>The object that represents the date and time when the daylight saving period begins. The value is in local time.</returns>
	public DateTime Start => m_start;

	/// <summary>Gets the object that represents the date and time when the daylight saving period ends.</summary>
	/// <returns>The object that represents the date and time when the daylight saving period ends. The value is in local time.</returns>
	public DateTime End => m_end;

	/// <summary>Gets the time interval that represents the difference between standard time and daylight saving time.</summary>
	/// <returns>The time interval that represents the difference between standard time and daylight saving time.</returns>
	public TimeSpan Delta => m_delta;

	private DaylightTime()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Globalization.DaylightTime" /> class with the specified start, end, and time difference information.</summary>
	/// <param name="start">The object that represents the date and time when daylight saving time begins. The value must be in local time. </param>
	/// <param name="end">The object that represents the date and time when daylight saving time ends. The value must be in local time. </param>
	/// <param name="delta">The object that represents the difference between standard time and daylight saving time, in ticks. </param>
	public DaylightTime(DateTime start, DateTime end, TimeSpan delta)
	{
		m_start = start;
		m_end = end;
		m_delta = delta;
	}
}
