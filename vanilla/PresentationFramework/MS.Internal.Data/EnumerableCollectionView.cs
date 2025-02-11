using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MS.Internal.Data;

internal class EnumerableCollectionView : CollectionView, IItemProperties
{
	private class IgnoreViewEventsHelper : IDisposable
	{
		private EnumerableCollectionView _parent;

		public IgnoreViewEventsHelper(EnumerableCollectionView parent)
		{
			_parent = parent;
			_parent.BeginIgnoreEvents();
		}

		public void Dispose()
		{
			if (_parent != null)
			{
				_parent.EndIgnoreEvents();
				_parent = null;
			}
			GC.SuppressFinalize(this);
		}
	}

	private ListCollectionView _view;

	private ObservableCollection<object> _snapshot;

	private IEnumerator _trackingEnumerator;

	private int _ignoreEventsLevel;

	private bool _pollForChanges;

	private bool _warningHasBeenRaised;

	public override CultureInfo Culture
	{
		get
		{
			return _view.Culture;
		}
		set
		{
			_view.Culture = value;
		}
	}

	public override Predicate<object> Filter
	{
		get
		{
			return _view.Filter;
		}
		set
		{
			_view.Filter = value;
		}
	}

	public override bool CanFilter => _view.CanFilter;

	public override SortDescriptionCollection SortDescriptions => _view.SortDescriptions;

	public override bool CanSort => _view.CanSort;

	public override bool CanGroup => _view.CanGroup;

	public override ObservableCollection<GroupDescription> GroupDescriptions => _view.GroupDescriptions;

	public override ReadOnlyObservableCollection<object> Groups => _view.Groups;

	public override object CurrentItem => _view.CurrentItem;

	public override int CurrentPosition => _view.CurrentPosition;

	public override bool IsCurrentAfterLast => _view.IsCurrentAfterLast;

	public override bool IsCurrentBeforeFirst => _view.IsCurrentBeforeFirst;

	public ReadOnlyCollection<ItemPropertyInfo> ItemProperties => ((IItemProperties)_view).ItemProperties;

	public override int Count
	{
		get
		{
			EnsureSnapshot();
			return _view.Count;
		}
	}

	public override bool IsEmpty
	{
		get
		{
			EnsureSnapshot();
			if (_view == null)
			{
				return true;
			}
			return _view.IsEmpty;
		}
	}

	public override bool NeedsRefresh => _view.NeedsRefresh;

	internal EnumerableCollectionView(IEnumerable source)
		: base(source, -1)
	{
		_snapshot = new ObservableCollection<object>();
		_pollForChanges = !(source is INotifyCollectionChanged);
		LoadSnapshotCore(source);
		if (_snapshot.Count > 0)
		{
			SetCurrent(_snapshot[0], 0, 1);
		}
		else
		{
			SetCurrent(null, -1, 0);
		}
		_view = new ListCollectionView(_snapshot);
		((INotifyCollectionChanged)_view).CollectionChanged += _OnViewChanged;
		((INotifyPropertyChanged)_view).PropertyChanged += _OnPropertyChanged;
		_view.CurrentChanging += _OnCurrentChanging;
		_view.CurrentChanged += _OnCurrentChanged;
	}

	public override bool Contains(object item)
	{
		EnsureSnapshot();
		return _view.Contains(item);
	}

	public override IDisposable DeferRefresh()
	{
		return _view.DeferRefresh();
	}

	public override bool MoveCurrentToFirst()
	{
		return _view.MoveCurrentToFirst();
	}

	public override bool MoveCurrentToPrevious()
	{
		return _view.MoveCurrentToPrevious();
	}

	public override bool MoveCurrentToNext()
	{
		return _view.MoveCurrentToNext();
	}

	public override bool MoveCurrentToLast()
	{
		return _view.MoveCurrentToLast();
	}

	public override bool MoveCurrentTo(object item)
	{
		return _view.MoveCurrentTo(item);
	}

	public override bool MoveCurrentToPosition(int position)
	{
		return _view.MoveCurrentToPosition(position);
	}

	public override int IndexOf(object item)
	{
		EnsureSnapshot();
		return _view.IndexOf(item);
	}

	public override bool PassesFilter(object item)
	{
		if (_view.CanFilter && _view.Filter != null)
		{
			return _view.Filter(item);
		}
		return true;
	}

	public override object GetItemAt(int index)
	{
		EnsureSnapshot();
		return _view.GetItemAt(index);
	}

	protected override IEnumerator GetEnumerator()
	{
		EnsureSnapshot();
		return ((IEnumerable)_view).GetEnumerator();
	}

	protected override void RefreshOverride()
	{
		LoadSnapshot(SourceCollection);
	}

	protected override void ProcessCollectionChanged(NotifyCollectionChangedEventArgs args)
	{
		if (_view == null)
		{
			return;
		}
		switch (args.Action)
		{
		case NotifyCollectionChangedAction.Add:
		{
			if (args.NewStartingIndex < 0 || _snapshot.Count <= args.NewStartingIndex)
			{
				for (int i = 0; i < args.NewItems.Count; i++)
				{
					_snapshot.Add(args.NewItems[i]);
				}
				break;
			}
			for (int num7 = args.NewItems.Count - 1; num7 >= 0; num7--)
			{
				_snapshot.Insert(args.NewStartingIndex, args.NewItems[num7]);
			}
			break;
		}
		case NotifyCollectionChangedAction.Remove:
		{
			if (args.OldStartingIndex < 0)
			{
				throw new InvalidOperationException(SR.RemovedItemNotFound);
			}
			int num10 = args.OldItems.Count - 1;
			int num11 = args.OldStartingIndex + num10;
			while (num10 >= 0)
			{
				if (!ItemsControl.EqualsEx(args.OldItems[num10], _snapshot[num11]))
				{
					throw new InvalidOperationException(SR.Format(SR.AddedItemNotAtIndex, num11));
				}
				_snapshot.RemoveAt(num11);
				num10--;
				num11--;
			}
			break;
		}
		case NotifyCollectionChangedAction.Replace:
		{
			int num8 = args.NewItems.Count - 1;
			int num9 = args.NewStartingIndex + num8;
			while (num8 >= 0)
			{
				if (!ItemsControl.EqualsEx(args.OldItems[num8], _snapshot[num9]))
				{
					throw new InvalidOperationException(SR.Format(SR.AddedItemNotAtIndex, num9));
				}
				_snapshot[num9] = args.NewItems[num8];
				num8--;
				num9--;
			}
			break;
		}
		case NotifyCollectionChangedAction.Move:
		{
			if (args.NewStartingIndex < 0)
			{
				throw new InvalidOperationException(SR.CannotMoveToUnknownPosition);
			}
			if (args.OldStartingIndex < args.NewStartingIndex)
			{
				int num = args.OldItems.Count - 1;
				int num2 = args.OldStartingIndex + num;
				int num3 = args.NewStartingIndex + num;
				while (num >= 0)
				{
					if (!ItemsControl.EqualsEx(args.OldItems[num], _snapshot[num2]))
					{
						throw new InvalidOperationException(SR.Format(SR.AddedItemNotAtIndex, num2));
					}
					_snapshot.Move(num2, num3);
					num--;
					num2--;
					num3--;
				}
				break;
			}
			int num4 = 0;
			int num5 = args.OldStartingIndex + num4;
			int num6 = args.NewStartingIndex + num4;
			while (num4 < args.OldItems.Count)
			{
				if (!ItemsControl.EqualsEx(args.OldItems[num4], _snapshot[num5]))
				{
					throw new InvalidOperationException(SR.Format(SR.AddedItemNotAtIndex, num5));
				}
				_snapshot.Move(num5, num6);
				num4++;
				num5++;
				num6++;
			}
			break;
		}
		case NotifyCollectionChangedAction.Reset:
			LoadSnapshot(SourceCollection);
			break;
		}
	}

	private void LoadSnapshot(IEnumerable source)
	{
		OnCurrentChanging();
		object currentItem = CurrentItem;
		int currentPosition = CurrentPosition;
		bool isCurrentBeforeFirst = IsCurrentBeforeFirst;
		bool isCurrentAfterLast = IsCurrentAfterLast;
		LoadSnapshotCore(source);
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		OnCurrentChanged();
		if (IsCurrentAfterLast != isCurrentAfterLast)
		{
			OnPropertyChanged(new PropertyChangedEventArgs("IsCurrentAfterLast"));
		}
		if (IsCurrentBeforeFirst != isCurrentBeforeFirst)
		{
			OnPropertyChanged(new PropertyChangedEventArgs("IsCurrentBeforeFirst"));
		}
		if (currentPosition != CurrentPosition)
		{
			OnPropertyChanged(new PropertyChangedEventArgs("CurrentPosition"));
		}
		if (currentItem != CurrentItem)
		{
			OnPropertyChanged(new PropertyChangedEventArgs("CurrentItem"));
		}
	}

	private void LoadSnapshotCore(IEnumerable source)
	{
		IEnumerator enumerator = source.GetEnumerator();
		using (IgnoreViewEvents())
		{
			_snapshot.Clear();
			while (enumerator.MoveNext())
			{
				_snapshot.Add(enumerator.Current);
			}
		}
		if (_pollForChanges)
		{
			IEnumerator trackingEnumerator = _trackingEnumerator;
			_trackingEnumerator = enumerator;
			enumerator = trackingEnumerator;
		}
		if (enumerator is IDisposable disposable2)
		{
			disposable2.Dispose();
		}
	}

	private void EnsureSnapshot()
	{
		if (!_pollForChanges)
		{
			return;
		}
		try
		{
			_trackingEnumerator.MoveNext();
		}
		catch (InvalidOperationException)
		{
			if (TraceData.IsEnabled && !_warningHasBeenRaised)
			{
				_warningHasBeenRaised = true;
				TraceData.TraceAndNotify(TraceEventType.Warning, TraceData.CollectionChangedWithoutNotification(SourceCollection.GetType().FullName));
			}
			LoadSnapshotCore(SourceCollection);
		}
	}

	private IDisposable IgnoreViewEvents()
	{
		return new IgnoreViewEventsHelper(this);
	}

	private void BeginIgnoreEvents()
	{
		_ignoreEventsLevel++;
	}

	private void EndIgnoreEvents()
	{
		_ignoreEventsLevel--;
	}

	private void _OnPropertyChanged(object sender, PropertyChangedEventArgs args)
	{
		if (_ignoreEventsLevel == 0)
		{
			OnPropertyChanged(args);
		}
	}

	private void _OnViewChanged(object sender, NotifyCollectionChangedEventArgs args)
	{
		if (_ignoreEventsLevel == 0)
		{
			OnCollectionChanged(args);
		}
	}

	private void _OnCurrentChanging(object sender, CurrentChangingEventArgs args)
	{
		if (_ignoreEventsLevel == 0)
		{
			OnCurrentChanging();
		}
	}

	private void _OnCurrentChanged(object sender, EventArgs args)
	{
		if (_ignoreEventsLevel == 0)
		{
			OnCurrentChanged();
		}
	}
}
