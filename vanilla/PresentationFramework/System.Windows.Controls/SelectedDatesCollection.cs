using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace System.Windows.Controls;

/// <summary>Represents a set of selected dates in a <see cref="T:System.Windows.Controls.Calendar" />.</summary>
public sealed class SelectedDatesCollection : ObservableCollection<DateTime>
{
	private Collection<DateTime> _addedItems;

	private Collection<DateTime> _removedItems;

	private Thread _dispatcherThread;

	private bool _isAddingRange;

	private Calendar _owner;

	private DateTime? _maximumDate;

	private DateTime? _minimumDate;

	internal DateTime? MinimumDate
	{
		get
		{
			if (base.Count < 1)
			{
				return null;
			}
			if (!_minimumDate.HasValue)
			{
				DateTime dateTime = base[0];
				using (IEnumerator<DateTime> enumerator = GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						DateTime current = enumerator.Current;
						if (DateTime.Compare(current, dateTime) < 0)
						{
							dateTime = current;
						}
					}
				}
				_maximumDate = dateTime;
			}
			return _minimumDate;
		}
	}

	internal DateTime? MaximumDate
	{
		get
		{
			if (base.Count < 1)
			{
				return null;
			}
			if (!_maximumDate.HasValue)
			{
				DateTime dateTime = base[0];
				using (IEnumerator<DateTime> enumerator = GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						DateTime current = enumerator.Current;
						if (DateTime.Compare(current, dateTime) > 0)
						{
							dateTime = current;
						}
					}
				}
				_maximumDate = dateTime;
			}
			return _maximumDate;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.SelectedDatesCollection" /> class. </summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.Calendar" /> associated with this collection.</param>
	public SelectedDatesCollection(Calendar owner)
	{
		_dispatcherThread = Thread.CurrentThread;
		_owner = owner;
		_addedItems = new Collection<DateTime>();
		_removedItems = new Collection<DateTime>();
	}

	/// <summary>Adds all the dates in the specified range, which includes the first and last dates, to the collection.</summary>
	/// <param name="start">The first date to add to the collection.</param>
	/// <param name="end">The last date to add to the collection.</param>
	public void AddRange(DateTime start, DateTime end)
	{
		BeginAddRange();
		if (_owner.SelectionMode == CalendarSelectionMode.SingleRange && base.Count > 0)
		{
			ClearInternal();
		}
		foreach (DateTime item in GetDaysInRange(start, end))
		{
			Add(item);
		}
		EndAddRange();
	}

	protected override void ClearItems()
	{
		if (!IsValidThread())
		{
			throw new NotSupportedException(SR.CalendarCollection_MultiThreadedCollectionChangeNotSupported);
		}
		_owner.HoverStart = null;
		ClearInternal(fireChangeNotification: true);
	}

	protected override void InsertItem(int index, DateTime item)
	{
		if (!IsValidThread())
		{
			throw new NotSupportedException(SR.CalendarCollection_MultiThreadedCollectionChangeNotSupported);
		}
		if (Contains(item))
		{
			return;
		}
		Collection<DateTime> collection = new Collection<DateTime>();
		bool flag = CheckSelectionMode();
		if (Calendar.IsValidDateSelection(_owner, item))
		{
			if (flag)
			{
				index = 0;
				flag = false;
			}
			base.InsertItem(index, item);
			UpdateMinMax(item);
			if (index == 0 && (!_owner.SelectedDate.HasValue || DateTime.Compare(_owner.SelectedDate.Value, item) != 0))
			{
				_owner.SelectedDate = item;
			}
			if (!_isAddingRange)
			{
				collection.Add(item);
				RaiseSelectionChanged(_removedItems, collection);
				_removedItems.Clear();
				int num = DateTimeHelper.CompareYearMonth(item, _owner.DisplayDateInternal);
				if (num < 2 && num > -2)
				{
					_owner.UpdateCellItems();
				}
			}
			else
			{
				_addedItems.Add(item);
			}
			return;
		}
		throw new ArgumentOutOfRangeException(SR.Calendar_OnSelectedDateChanged_InvalidValue);
	}

	protected override void RemoveItem(int index)
	{
		if (!IsValidThread())
		{
			throw new NotSupportedException(SR.CalendarCollection_MultiThreadedCollectionChangeNotSupported);
		}
		if (index >= base.Count)
		{
			base.RemoveItem(index);
			ClearMinMax();
			return;
		}
		Collection<DateTime> addedItems = new Collection<DateTime>();
		Collection<DateTime> collection = new Collection<DateTime>();
		int num = DateTimeHelper.CompareYearMonth(base[index], _owner.DisplayDateInternal);
		collection.Add(base[index]);
		base.RemoveItem(index);
		ClearMinMax();
		if (index == 0)
		{
			if (base.Count > 0)
			{
				_owner.SelectedDate = base[0];
			}
			else
			{
				_owner.SelectedDate = null;
			}
		}
		RaiseSelectionChanged(collection, addedItems);
		if (num < 2 && num > -2)
		{
			_owner.UpdateCellItems();
		}
	}

	protected override void SetItem(int index, DateTime item)
	{
		if (!IsValidThread())
		{
			throw new NotSupportedException(SR.CalendarCollection_MultiThreadedCollectionChangeNotSupported);
		}
		if (Contains(item))
		{
			return;
		}
		Collection<DateTime> collection = new Collection<DateTime>();
		Collection<DateTime> collection2 = new Collection<DateTime>();
		if (index >= base.Count)
		{
			base.SetItem(index, item);
			UpdateMinMax(item);
		}
		else if (DateTime.Compare(base[index], item) != 0 && Calendar.IsValidDateSelection(_owner, item))
		{
			collection2.Add(base[index]);
			base.SetItem(index, item);
			UpdateMinMax(item);
			collection.Add(item);
			if (index == 0 && (!_owner.SelectedDate.HasValue || DateTime.Compare(_owner.SelectedDate.Value, item) != 0))
			{
				_owner.SelectedDate = item;
			}
			RaiseSelectionChanged(collection2, collection);
			int num = DateTimeHelper.CompareYearMonth(item, _owner.DisplayDateInternal);
			if (num < 2 && num > -2)
			{
				_owner.UpdateCellItems();
			}
		}
	}

	internal void AddRangeInternal(DateTime start, DateTime end)
	{
		BeginAddRange();
		DateTime currentDate = start;
		foreach (DateTime item in GetDaysInRange(start, end))
		{
			if (Calendar.IsValidDateSelection(_owner, item))
			{
				Add(item);
				currentDate = item;
			}
			else if (_owner.SelectionMode == CalendarSelectionMode.SingleRange)
			{
				_owner.CurrentDate = currentDate;
				break;
			}
		}
		EndAddRange();
	}

	internal void ClearInternal()
	{
		ClearInternal(fireChangeNotification: false);
	}

	internal void ClearInternal(bool fireChangeNotification)
	{
		if (base.Count <= 0)
		{
			return;
		}
		using (IEnumerator<DateTime> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				DateTime current = enumerator.Current;
				_removedItems.Add(current);
			}
		}
		base.ClearItems();
		ClearMinMax();
		if (fireChangeNotification)
		{
			if (_owner.SelectedDate.HasValue)
			{
				_owner.SelectedDate = null;
			}
			if (_removedItems.Count > 0)
			{
				Collection<DateTime> addedItems = new Collection<DateTime>();
				RaiseSelectionChanged(_removedItems, addedItems);
				_removedItems.Clear();
			}
			_owner.UpdateCellItems();
		}
	}

	internal void Toggle(DateTime date)
	{
		if (!Calendar.IsValidDateSelection(_owner, date))
		{
			return;
		}
		switch (_owner.SelectionMode)
		{
		case CalendarSelectionMode.SingleDate:
			if (!_owner.SelectedDate.HasValue || DateTimeHelper.CompareDays(_owner.SelectedDate.Value, date) != 0)
			{
				_owner.SelectedDate = date;
			}
			else
			{
				_owner.SelectedDate = null;
			}
			break;
		case CalendarSelectionMode.MultipleRange:
			if (!Remove(date))
			{
				Add(date);
			}
			break;
		}
	}

	private void RaiseSelectionChanged(IList removedItems, IList addedItems)
	{
		_owner.OnSelectedDatesCollectionChanged(new CalendarSelectionChangedEventArgs(Calendar.SelectedDatesChangedEvent, removedItems, addedItems));
	}

	private void BeginAddRange()
	{
		_isAddingRange = true;
	}

	private void EndAddRange()
	{
		_isAddingRange = false;
		RaiseSelectionChanged(_removedItems, _addedItems);
		_removedItems.Clear();
		_addedItems.Clear();
		_owner.UpdateCellItems();
	}

	private bool CheckSelectionMode()
	{
		if (_owner.SelectionMode == CalendarSelectionMode.None)
		{
			throw new InvalidOperationException(SR.Calendar_OnSelectedDateChanged_InvalidOperation);
		}
		if (_owner.SelectionMode == CalendarSelectionMode.SingleDate && base.Count > 0)
		{
			throw new InvalidOperationException(SR.Calendar_CheckSelectionMode_InvalidOperation);
		}
		if (_owner.SelectionMode == CalendarSelectionMode.SingleRange && !_isAddingRange && base.Count > 0)
		{
			ClearInternal();
			return true;
		}
		return false;
	}

	private bool IsValidThread()
	{
		return Thread.CurrentThread == _dispatcherThread;
	}

	private void UpdateMinMax(DateTime date)
	{
		if (!_maximumDate.HasValue || date > _maximumDate.Value)
		{
			_maximumDate = date;
		}
		if (!_minimumDate.HasValue || date < _minimumDate.Value)
		{
			_minimumDate = date;
		}
	}

	private void ClearMinMax()
	{
		_maximumDate = null;
		_minimumDate = null;
	}

	private static IEnumerable<DateTime> GetDaysInRange(DateTime start, DateTime end)
	{
		int increment = GetDirection(start, end);
		DateTime? rangeStart = start;
		do
		{
			yield return rangeStart.Value;
			rangeStart = DateTimeHelper.AddDays(rangeStart.Value, increment);
		}
		while (rangeStart.HasValue && DateTime.Compare(end, rangeStart.Value) != -increment);
	}

	private static int GetDirection(DateTime start, DateTime end)
	{
		if (DateTime.Compare(end, start) < 0)
		{
			return -1;
		}
		return 1;
	}
}
