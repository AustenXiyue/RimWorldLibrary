namespace System.Windows.Controls;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.Calendar.DisplayModeChanged" /> event.</summary>
public class CalendarModeChangedEventArgs : RoutedEventArgs
{
	/// <summary>Gets the new mode of the <see cref="T:System.Windows.Controls.Calendar" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Controls.CalendarMode" /> that represents the new mode.</returns>
	public CalendarMode NewMode { get; private set; }

	/// <summary>Gets the previous mode of the <see cref="T:System.Windows.Controls.Calendar" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Controls.CalendarMode" /> that represents the old mode.</returns>
	public CalendarMode OldMode { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.CalendarModeChangedEventArgs" /> class. </summary>
	/// <param name="oldMode">The previous mode.</param>
	/// <param name="newMode">The new mode.</param>
	public CalendarModeChangedEventArgs(CalendarMode oldMode, CalendarMode newMode)
	{
		OldMode = oldMode;
		NewMode = newMode;
	}
}
