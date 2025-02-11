using System.ComponentModel;

namespace System.Windows.Controls;

/// <summary>Represents a range of dates in a <see cref="T:System.Windows.Controls.Calendar" />.</summary>
public sealed class CalendarDateRange : INotifyPropertyChanged
{
	private DateTime _end;

	private DateTime _start;

	/// <summary>Gets the last date in the represented range.</summary>
	/// <returns>The last date in the represented range.</returns>
	public DateTime End
	{
		get
		{
			return CoerceEnd(_start, _end);
		}
		set
		{
			DateTime dateTime = CoerceEnd(_start, value);
			if (dateTime != End)
			{
				OnChanging(new CalendarDateRangeChangingEventArgs(_start, dateTime));
				_end = value;
				OnPropertyChanged(new PropertyChangedEventArgs("End"));
			}
		}
	}

	/// <summary>Gets the first date in the represented range.</summary>
	/// <returns>The first date in the represented range.</returns>
	public DateTime Start
	{
		get
		{
			return _start;
		}
		set
		{
			if (_start != value)
			{
				DateTime end = End;
				DateTime dateTime = CoerceEnd(value, _end);
				OnChanging(new CalendarDateRangeChangingEventArgs(value, dateTime));
				_start = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Start"));
				if (dateTime != end)
				{
					OnPropertyChanged(new PropertyChangedEventArgs("End"));
				}
			}
		}
	}

	/// <summary>Occurs when a property value changes.</summary>
	public event PropertyChangedEventHandler PropertyChanged;

	internal event EventHandler<CalendarDateRangeChangingEventArgs> Changing;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.CalendarDateRange" /> class. </summary>
	public CalendarDateRange()
		: this(DateTime.MinValue, DateTime.MaxValue)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.CalendarDateRange" /> class with a single date.</summary>
	/// <param name="day">The date to add.</param>
	public CalendarDateRange(DateTime day)
		: this(day, day)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.CalendarDateRange" /> class with a range of dates.</summary>
	/// <param name="start">The start of the range to be represented.</param>
	/// <param name="end">The end of the range to be represented.</param>
	public CalendarDateRange(DateTime start, DateTime end)
	{
		_start = start;
		_end = end;
	}

	internal bool ContainsAny(CalendarDateRange range)
	{
		if (range.End >= Start)
		{
			return End >= range.Start;
		}
		return false;
	}

	private void OnChanging(CalendarDateRangeChangingEventArgs e)
	{
		this.Changing?.Invoke(this, e);
	}

	private void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		this.PropertyChanged?.Invoke(this, e);
	}

	private static DateTime CoerceEnd(DateTime start, DateTime end)
	{
		if (DateTime.Compare(start, end) > 0)
		{
			return start;
		}
		return end;
	}
}
