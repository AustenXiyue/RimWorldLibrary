namespace System.Windows.Controls;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.Calendar.DisplayDateChanged" /> event.</summary>
public class CalendarDateChangedEventArgs : RoutedEventArgs
{
	/// <summary>Gets or sets the date to be newly displayed.</summary>
	/// <returns>The date to be newly displayed.</returns>
	public DateTime? AddedDate { get; private set; }

	/// <summary>Gets or sets the date that was previously displayed.</summary>
	/// <returns>The date that was previously displayed.</returns>
	public DateTime? RemovedDate { get; private set; }

	internal CalendarDateChangedEventArgs(DateTime? removedDate, DateTime? addedDate)
	{
		RemovedDate = removedDate;
		AddedDate = addedDate;
	}
}
