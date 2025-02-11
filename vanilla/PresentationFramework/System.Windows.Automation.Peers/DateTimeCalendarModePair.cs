using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

internal struct DateTimeCalendarModePair
{
	private CalendarMode ButtonMode;

	private DateTime Date;

	internal DateTimeCalendarModePair(DateTime date, CalendarMode mode)
	{
		ButtonMode = mode;
		Date = date;
	}
}
