namespace System.Windows.Controls;

/// <summary>Specifies whether a single or multiple dates can be selected in a <see cref="T:System.Windows.Controls.Calendar" />.</summary>
public enum CalendarSelectionMode
{
	/// <summary>A single date can be selected. Use the <see cref="P:System.Windows.Controls.Calendar.SelectedDate" /> property to retrieve the selected date.</summary>
	SingleDate,
	/// <summary>A single range of dates can be selected. Use the <see cref="P:System.Windows.Controls.Calendar.SelectedDates" /> property to retrieve the selected dates.</summary>
	SingleRange,
	/// <summary>Multiple non-contiguous ranges of dates can be selected. Use the <see cref="P:System.Windows.Controls.Calendar.SelectedDates" /> property to retrieve the selected dates.</summary>
	MultipleRange,
	/// <summary>No selections are allowed.</summary>
	None
}
