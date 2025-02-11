namespace System.Windows.Controls;

internal class CalendarDateRangeChangingEventArgs : EventArgs
{
	private DateTime _start;

	private DateTime _end;

	public DateTime Start => _start;

	public DateTime End => _end;

	public CalendarDateRangeChangingEventArgs(DateTime start, DateTime end)
	{
		_start = start;
		_end = end;
	}
}
