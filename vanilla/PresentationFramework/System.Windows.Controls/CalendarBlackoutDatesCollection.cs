using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;

namespace System.Windows.Controls;

/// <summary>Represents a collection of non-selectable dates in a <see cref="T:System.Windows.Controls.Calendar" />.</summary>
public sealed class CalendarBlackoutDatesCollection : ObservableCollection<CalendarDateRange>
{
	private Thread _dispatcherThread;

	private Calendar _owner;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.CalendarBlackoutDatesCollection" /> class. </summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.Calendar" /> whose dates this object represents.</param>
	public CalendarBlackoutDatesCollection(Calendar owner)
	{
		_owner = owner;
		_dispatcherThread = Thread.CurrentThread;
	}

	/// <summary>Adds all dates before <see cref="P:System.DateTime.Today" /> to the collection.</summary>
	public void AddDatesInPast()
	{
		Add(new CalendarDateRange(DateTime.MinValue, DateTime.Today.AddDays(-1.0)));
	}

	/// <summary>Returns a value that represents whether this collection contains the specified date.</summary>
	/// <returns>true if the collection contains the specified date; otherwise, false.</returns>
	/// <param name="date">The date to search for.</param>
	public bool Contains(DateTime date)
	{
		return GetContainingDateRange(date) != null;
	}

	/// <summary>Returns a value that represents whether this collection contains the specified range of dates.</summary>
	/// <returns>true if all dates in the range are contained in the collection; otherwise, false.</returns>
	/// <param name="start">The start of the date range.</param>
	/// <param name="end">The end of the date range.</param>
	public bool Contains(DateTime start, DateTime end)
	{
		int count = base.Count;
		DateTime value;
		DateTime value2;
		if (DateTime.Compare(end, start) > -1)
		{
			value = DateTimeHelper.DiscardTime(start).Value;
			value2 = DateTimeHelper.DiscardTime(end).Value;
		}
		else
		{
			value = DateTimeHelper.DiscardTime(end).Value;
			value2 = DateTimeHelper.DiscardTime(start).Value;
		}
		for (int i = 0; i < count; i++)
		{
			if (DateTime.Compare(base[i].Start, value) == 0 && DateTime.Compare(base[i].End, value2) == 0)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Returns a value that represents whether this collection contains any dates in the specified range of dates.</summary>
	/// <returns>true if any dates in the range are contained in the collection; otherwise, false.</returns>
	/// <param name="range">The range of dates to search for.</param>
	public bool ContainsAny(CalendarDateRange range)
	{
		using (IEnumerator<CalendarDateRange> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.ContainsAny(range))
				{
					return true;
				}
			}
		}
		return false;
	}

	internal DateTime? GetNonBlackoutDate(DateTime? requestedDate, int dayInterval)
	{
		DateTime? dateTime = requestedDate;
		CalendarDateRange calendarDateRange = null;
		if (!requestedDate.HasValue)
		{
			return null;
		}
		if ((calendarDateRange = GetContainingDateRange(dateTime.Value)) == null)
		{
			return requestedDate;
		}
		do
		{
			dateTime = ((dayInterval <= 0) ? DateTimeHelper.AddDays(calendarDateRange.Start, dayInterval) : DateTimeHelper.AddDays(calendarDateRange.End, dayInterval));
		}
		while (dateTime.HasValue && (calendarDateRange = GetContainingDateRange(dateTime.Value)) != null);
		return dateTime;
	}

	protected override void ClearItems()
	{
		if (!IsValidThread())
		{
			throw new NotSupportedException(SR.CalendarCollection_MultiThreadedCollectionChangeNotSupported);
		}
		foreach (CalendarDateRange item in base.Items)
		{
			UnRegisterItem(item);
		}
		base.ClearItems();
		_owner.UpdateCellItems();
	}

	protected override void InsertItem(int index, CalendarDateRange item)
	{
		if (!IsValidThread())
		{
			throw new NotSupportedException(SR.CalendarCollection_MultiThreadedCollectionChangeNotSupported);
		}
		if (IsValid(item))
		{
			RegisterItem(item);
			base.InsertItem(index, item);
			_owner.UpdateCellItems();
			return;
		}
		throw new ArgumentOutOfRangeException(SR.Calendar_UnSelectableDates);
	}

	protected override void RemoveItem(int index)
	{
		if (!IsValidThread())
		{
			throw new NotSupportedException(SR.CalendarCollection_MultiThreadedCollectionChangeNotSupported);
		}
		if (index >= 0 && index < base.Count)
		{
			UnRegisterItem(base.Items[index]);
		}
		base.RemoveItem(index);
		_owner.UpdateCellItems();
	}

	protected override void SetItem(int index, CalendarDateRange item)
	{
		if (!IsValidThread())
		{
			throw new NotSupportedException(SR.CalendarCollection_MultiThreadedCollectionChangeNotSupported);
		}
		if (IsValid(item))
		{
			CalendarDateRange item2 = null;
			if (index >= 0 && index < base.Count)
			{
				item2 = base.Items[index];
			}
			base.SetItem(index, item);
			UnRegisterItem(item2);
			RegisterItem(base.Items[index]);
			_owner.UpdateCellItems();
			return;
		}
		throw new ArgumentOutOfRangeException(SR.Calendar_UnSelectableDates);
	}

	private void RegisterItem(CalendarDateRange item)
	{
		if (item != null)
		{
			item.Changing += Item_Changing;
			item.PropertyChanged += Item_PropertyChanged;
		}
	}

	private void UnRegisterItem(CalendarDateRange item)
	{
		if (item != null)
		{
			item.Changing -= Item_Changing;
			item.PropertyChanged -= Item_PropertyChanged;
		}
	}

	private void Item_Changing(object sender, CalendarDateRangeChangingEventArgs e)
	{
		if (sender is CalendarDateRange && !IsValid(e.Start, e.End))
		{
			throw new ArgumentOutOfRangeException(SR.Calendar_UnSelectableDates);
		}
	}

	private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (sender is CalendarDateRange)
		{
			_owner.UpdateCellItems();
		}
	}

	private bool IsValid(CalendarDateRange item)
	{
		return IsValid(item.Start, item.End);
	}

	private bool IsValid(DateTime start, DateTime end)
	{
		foreach (DateTime selectedDate in _owner.SelectedDates)
		{
			if (DateTimeHelper.InRange((selectedDate as DateTime?).Value, start, end))
			{
				return false;
			}
		}
		return true;
	}

	private bool IsValidThread()
	{
		return Thread.CurrentThread == _dispatcherThread;
	}

	private CalendarDateRange GetContainingDateRange(DateTime date)
	{
		for (int i = 0; i < base.Count; i++)
		{
			if (DateTimeHelper.InRange(date, base[i]))
			{
				return base[i];
			}
		}
		return null;
	}
}
