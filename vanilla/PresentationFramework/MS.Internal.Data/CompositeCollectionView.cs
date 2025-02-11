using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MS.Internal.Hashing.PresentationFramework;
using MS.Internal.Utility;

namespace MS.Internal.Data;

internal sealed class CompositeCollectionView : CollectionView
{
	private class FlatteningEnumerator : IEnumerator, IDisposable
	{
		private CompositeCollection _collection;

		private CompositeCollectionView _view;

		private int _index;

		private object _current;

		private IEnumerator _containerEnumerator;

		private bool _done;

		private bool _isInvalidated;

		private int _version;

		public object Current
		{
			get
			{
				if (_index < 0)
				{
					throw new InvalidOperationException(SR.EnumeratorNotStarted);
				}
				if (_done)
				{
					throw new InvalidOperationException(SR.EnumeratorReachedEnd);
				}
				return _current;
			}
		}

		internal FlatteningEnumerator(CompositeCollection collection, CompositeCollectionView view)
		{
			Invariant.Assert(collection != null && view != null);
			_collection = collection;
			_view = view;
			_version = view._version;
			Reset();
		}

		public bool MoveNext()
		{
			CheckVersion();
			bool result = true;
			while (true)
			{
				if (_containerEnumerator != null)
				{
					if (_containerEnumerator.MoveNext())
					{
						_current = _containerEnumerator.Current;
						break;
					}
					DisposeContainerEnumerator();
				}
				if (++_index < _collection.Count)
				{
					object obj = _collection[_index];
					if (obj is CollectionContainer collectionContainer)
					{
						_containerEnumerator = collectionContainer.View?.GetEnumerator();
						continue;
					}
					_current = obj;
					break;
				}
				_current = null;
				_done = true;
				result = false;
				break;
			}
			return result;
		}

		public void Reset()
		{
			CheckVersion();
			_index = -1;
			_current = null;
			DisposeContainerEnumerator();
			_done = false;
		}

		public void Dispose()
		{
			DisposeContainerEnumerator();
		}

		private void DisposeContainerEnumerator()
		{
			if (_containerEnumerator is IDisposable disposable)
			{
				disposable.Dispose();
			}
			_containerEnumerator = null;
		}

		private void CheckVersion()
		{
			if (_isInvalidated || (_isInvalidated = _version != _view._version))
			{
				throw new InvalidOperationException(SR.EnumeratorVersionChanged);
			}
		}
	}

	private TraceLog _traceLog;

	private CompositeCollection _collection;

	private int _count = -1;

	private int _version;

	private int _currentPositionX = -1;

	private int _currentPositionY;

	private static readonly object s_afterLast = new object();

	public override int Count
	{
		get
		{
			if (_count == -1)
			{
				_count = CountDeep(_collection.Count);
			}
			return _count;
		}
	}

	public override bool IsEmpty => PrivateIsEmpty;

	private bool PrivateIsEmpty
	{
		get
		{
			if (_count < 0)
			{
				for (int i = 0; i < _collection.Count; i++)
				{
					if (!(_collection[i] is CollectionContainer { ViewCount: 0 }))
					{
						return false;
					}
				}
				CacheCount(0);
			}
			return _count == 0;
		}
	}

	public override bool IsCurrentAfterLast
	{
		get
		{
			if (!IsEmpty)
			{
				return _currentPositionX >= _collection.Count;
			}
			return true;
		}
	}

	public override bool IsCurrentBeforeFirst
	{
		get
		{
			if (!IsEmpty)
			{
				return _currentPositionX < 0;
			}
			return true;
		}
	}

	public override bool CanFilter => false;

	private bool IsCurrentInView
	{
		get
		{
			if (0 <= _currentPositionX)
			{
				return _currentPositionX < _collection.Count;
			}
			return false;
		}
	}

	internal CompositeCollectionView(CompositeCollection collection)
		: base(collection, -1)
	{
		_collection = collection;
		_collection.ContainedCollectionChanged += OnContainedCollectionChanged;
		int num = (PrivateIsEmpty ? (-1) : 0);
		int count = ((!PrivateIsEmpty) ? 1 : 0);
		SetCurrent(GetItem(num, out _currentPositionX, out _currentPositionY), num, count);
	}

	public override bool Contains(object item)
	{
		return FindItem(item, changeCurrent: false) >= 0;
	}

	public override int IndexOf(object item)
	{
		return FindItem(item, changeCurrent: false);
	}

	public override object GetItemAt(int index)
	{
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		object item = GetItem(index, out var _, out var _);
		if (item == s_afterLast)
		{
			item = null;
			throw new ArgumentOutOfRangeException("index");
		}
		return item;
	}

	public override bool MoveCurrentTo(object item)
	{
		if (ItemsControl.EqualsEx(CurrentItem, item) && (item != null || IsCurrentInView))
		{
			return IsCurrentInView;
		}
		if (!IsEmpty)
		{
			FindItem(item, changeCurrent: true);
		}
		return IsCurrentInView;
	}

	public override bool MoveCurrentToFirst()
	{
		if (IsEmpty)
		{
			return false;
		}
		return _MoveTo(0);
	}

	public override bool MoveCurrentToLast()
	{
		bool isCurrentAfterLast = IsCurrentAfterLast;
		bool isCurrentBeforeFirst = IsCurrentBeforeFirst;
		int num = Count - 1;
		int positionX;
		int positionY;
		object lastItem = GetLastItem(out positionX, out positionY);
		if ((CurrentPosition != num || CurrentItem != lastItem) && OKToChangeCurrent())
		{
			_currentPositionX = positionX;
			_currentPositionY = positionY;
			SetCurrent(lastItem, num);
			OnCurrentChanged();
			if (IsCurrentAfterLast != isCurrentAfterLast)
			{
				OnPropertyChanged("IsCurrentAfterLast");
			}
			if (IsCurrentBeforeFirst != isCurrentBeforeFirst)
			{
				OnPropertyChanged("IsCurrentBeforeFirst");
			}
			OnPropertyChanged("CurrentPosition");
			OnPropertyChanged("CurrentItem");
		}
		return IsCurrentInView;
	}

	public override bool MoveCurrentToNext()
	{
		if (IsCurrentAfterLast)
		{
			return false;
		}
		return _MoveTo(CurrentPosition + 1);
	}

	public override bool MoveCurrentToPrevious()
	{
		if (IsCurrentBeforeFirst)
		{
			return false;
		}
		return _MoveTo(CurrentPosition - 1);
	}

	public override bool MoveCurrentToPosition(int position)
	{
		if (position < -1)
		{
			throw new ArgumentOutOfRangeException("position");
		}
		int positionX;
		int positionY;
		object obj = GetItem(position, out positionX, out positionY);
		if (position != CurrentPosition || obj != CurrentItem)
		{
			if (obj == s_afterLast)
			{
				obj = null;
				if (position > Count)
				{
					throw new ArgumentOutOfRangeException("position");
				}
			}
			if (OKToChangeCurrent())
			{
				bool isCurrentAfterLast = IsCurrentAfterLast;
				bool isCurrentBeforeFirst = IsCurrentBeforeFirst;
				_currentPositionX = positionX;
				_currentPositionY = positionY;
				SetCurrent(obj, position);
				OnCurrentChanged();
				if (IsCurrentAfterLast != isCurrentAfterLast)
				{
					OnPropertyChanged("IsCurrentAfterLast");
				}
				if (IsCurrentBeforeFirst != isCurrentBeforeFirst)
				{
					OnPropertyChanged("IsCurrentBeforeFirst");
				}
				OnPropertyChanged("CurrentPosition");
				OnPropertyChanged("CurrentItem");
			}
		}
		return IsCurrentInView;
	}

	protected override void RefreshOverride()
	{
		_version++;
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	}

	protected override IEnumerator GetEnumerator()
	{
		return new FlatteningEnumerator(_collection, this);
	}

	protected override void ProcessCollectionChanged(NotifyCollectionChangedEventArgs args)
	{
		ValidateCollectionChangedEventArgs(args);
		bool flag = false;
		switch (args.Action)
		{
		case NotifyCollectionChangedAction.Add:
		case NotifyCollectionChangedAction.Remove:
		{
			object obj = null;
			int num = -1;
			if (args.Action == NotifyCollectionChangedAction.Add)
			{
				obj = args.NewItems[0];
				num = args.NewStartingIndex;
			}
			else
			{
				obj = args.OldItems[0];
				num = args.OldStartingIndex;
			}
			int num2 = num;
			if (_traceLog != null)
			{
				_traceLog.Add("ProcessCollectionChanged  action = {0}  item = {1}", args.Action, TraceLog.IdFor(obj));
			}
			if (!(obj is CollectionContainer collectionContainer))
			{
				for (int num3 = num2 - 1; num3 >= 0; num3--)
				{
					if (_collection[num3] is CollectionContainer collectionContainer2)
					{
						num2 += collectionContainer2.ViewCount - 1;
					}
				}
				if (args.Action == NotifyCollectionChangedAction.Add)
				{
					if (_count >= 0)
					{
						_count++;
					}
					UpdateCurrencyAfterAdd(num2, args.NewStartingIndex, isCompositeItem: true);
				}
				else if (args.Action == NotifyCollectionChangedAction.Remove)
				{
					if (_count >= 0)
					{
						_count--;
					}
					UpdateCurrencyAfterRemove(num2, args.OldStartingIndex, isCompositeItem: true);
				}
				args = new NotifyCollectionChangedEventArgs(args.Action, obj, num2);
				break;
			}
			if (args.Action == NotifyCollectionChangedAction.Add)
			{
				if (_count >= 0)
				{
					_count += collectionContainer.ViewCount;
				}
			}
			else if (_count >= 0)
			{
				_count -= collectionContainer.ViewCount;
			}
			if (num <= _currentPositionX)
			{
				if (args.Action == NotifyCollectionChangedAction.Add)
				{
					_currentPositionX++;
					SetCurrentPositionFromXY(_currentPositionX, _currentPositionY);
				}
				else
				{
					Invariant.Assert(args.Action == NotifyCollectionChangedAction.Remove);
					if (num == _currentPositionX)
					{
						flag = true;
					}
					else
					{
						_currentPositionX--;
						SetCurrentPositionFromXY(_currentPositionX, _currentPositionY);
					}
				}
			}
			args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
			break;
		}
		case NotifyCollectionChangedAction.Replace:
		{
			CollectionContainer collectionContainer3 = args.NewItems[0] as CollectionContainer;
			CollectionContainer collectionContainer4 = args.OldItems[0] as CollectionContainer;
			int num4 = args.OldStartingIndex;
			if (collectionContainer3 == null && collectionContainer4 == null)
			{
				for (int num5 = num4 - 1; num5 >= 0; num5--)
				{
					if (_collection[num5] is CollectionContainer collectionContainer5)
					{
						num4 += collectionContainer5.ViewCount - 1;
					}
				}
				if (num4 == CurrentPosition)
				{
					flag = true;
				}
				args = new NotifyCollectionChangedEventArgs(args.Action, args.NewItems, args.OldItems, num4);
			}
			else
			{
				if (_count >= 0)
				{
					_count -= collectionContainer4?.ViewCount ?? 1;
					_count += collectionContainer3?.ViewCount ?? 1;
				}
				if (num4 < _currentPositionX)
				{
					SetCurrentPositionFromXY(_currentPositionX, _currentPositionY);
				}
				else if (num4 == _currentPositionX)
				{
					flag = true;
				}
				args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
			}
			break;
		}
		case NotifyCollectionChangedAction.Move:
		{
			CollectionContainer obj2 = args.OldItems[0] as CollectionContainer;
			int num6 = args.OldStartingIndex;
			int num7 = args.NewStartingIndex;
			if (obj2 == null)
			{
				for (int num8 = num6 - 1; num8 >= 0; num8--)
				{
					if (_collection[num8] is CollectionContainer collectionContainer6)
					{
						num6 += collectionContainer6.ViewCount - 1;
					}
				}
				for (int num9 = num7 - 1; num9 >= 0; num9--)
				{
					if (_collection[num9] is CollectionContainer collectionContainer7)
					{
						num7 += collectionContainer7.ViewCount - 1;
					}
				}
				if (num6 == CurrentPosition)
				{
					flag = true;
				}
				else if (num7 <= CurrentPosition && num6 > CurrentPosition)
				{
					UpdateCurrencyAfterAdd(num7, args.NewStartingIndex, isCompositeItem: true);
				}
				else if (num6 < CurrentPosition && num7 >= CurrentPosition)
				{
					UpdateCurrencyAfterRemove(num6, args.OldStartingIndex, isCompositeItem: true);
				}
				args = new NotifyCollectionChangedEventArgs(args.Action, args.OldItems, num7, num6);
			}
			else
			{
				if (num6 == _currentPositionX)
				{
					flag = true;
				}
				else if (num7 <= _currentPositionX && num6 > _currentPositionX)
				{
					_currentPositionX++;
					SetCurrentPositionFromXY(_currentPositionX, _currentPositionY);
				}
				else if (num6 < _currentPositionX && num7 >= _currentPositionX)
				{
					_currentPositionX--;
					SetCurrentPositionFromXY(_currentPositionX, _currentPositionY);
				}
				args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
			}
			break;
		}
		case NotifyCollectionChangedAction.Reset:
			if (_traceLog != null)
			{
				_traceLog.Add("ProcessCollectionChanged  action = {0}", args.Action);
			}
			if (_collection.Count != 0)
			{
				throw new InvalidOperationException(SR.CompositeCollectionResetOnlyOnClear);
			}
			_count = 0;
			if (_currentPositionX >= 0)
			{
				OnCurrentChanging();
				SetCurrentBeforeFirst();
				OnCurrentChanged();
				OnPropertyChanged("IsCurrentBeforeFirst");
				OnPropertyChanged("CurrentPosition");
				OnPropertyChanged("CurrentItem");
			}
			break;
		default:
			throw new NotSupportedException(SR.Format(SR.UnexpectedCollectionChangeAction, args.Action));
		}
		_version++;
		OnCollectionChanged(args);
		if (flag)
		{
			_currentPositionY = 0;
			MoveCurrencyOffDeletedElement();
		}
	}

	internal void OnContainedCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
	{
		ValidateCollectionChangedEventArgs(args);
		_count = -1;
		int num = args.OldStartingIndex;
		int num2 = args.NewStartingIndex;
		int num3 = 0;
		int i;
		for (i = 0; i < _collection.Count; i++)
		{
			if (_collection[i] is CollectionContainer collectionContainer)
			{
				if (sender == collectionContainer)
				{
					break;
				}
				num3 += collectionContainer.ViewCount;
			}
			else
			{
				num3++;
			}
		}
		if (args.OldStartingIndex >= 0)
		{
			num += num3;
		}
		if (args.NewStartingIndex >= 0)
		{
			num2 += num3;
		}
		if (i >= _collection.Count)
		{
			if (_traceLog != null)
			{
				_traceLog.Add("Received ContainerCollectionChange from unknown sender {0}  action = {1} old item = {2}, new item = {3}", TraceLog.IdFor(sender), args.Action, TraceLog.IdFor(args.OldItems[0]), TraceLog.IdFor(args.NewItems[0]));
				_traceLog.Add("Unhook CollectionChanged event handler from unknown sender.");
			}
			CacheCount(num3);
			return;
		}
		switch (args.Action)
		{
		case NotifyCollectionChangedAction.Add:
			TraceContainerCollectionChange(sender, args.Action, null, args.NewItems[0]);
			if (num2 < 0)
			{
				num2 = DeduceFlatIndexForAdd((CollectionContainer)sender, i);
			}
			UpdateCurrencyAfterAdd(num2, i, isCompositeItem: false);
			args = new NotifyCollectionChangedEventArgs(args.Action, args.NewItems[0], num2);
			break;
		case NotifyCollectionChangedAction.Remove:
			TraceContainerCollectionChange(sender, args.Action, args.OldItems[0], null);
			if (num < 0)
			{
				num = DeduceFlatIndexForRemove((CollectionContainer)sender, i, args.OldItems[0]);
			}
			UpdateCurrencyAfterRemove(num, i, isCompositeItem: false);
			args = new NotifyCollectionChangedEventArgs(args.Action, args.OldItems[0], num);
			break;
		case NotifyCollectionChangedAction.Replace:
			TraceContainerCollectionChange(sender, args.Action, args.OldItems[0], args.NewItems[0]);
			if (num == CurrentPosition)
			{
				MoveCurrencyOffDeletedElement();
			}
			args = new NotifyCollectionChangedEventArgs(args.Action, args.NewItems[0], args.OldItems[0], num);
			break;
		case NotifyCollectionChangedAction.Move:
			TraceContainerCollectionChange(sender, args.Action, args.OldItems[0], args.NewItems[0]);
			if (num < 0)
			{
				num = DeduceFlatIndexForRemove((CollectionContainer)sender, i, args.NewItems[0]);
			}
			if (num2 < 0)
			{
				num2 = DeduceFlatIndexForAdd((CollectionContainer)sender, i);
			}
			UpdateCurrencyAfterMove(num, num2, i, isCompositeItem: false);
			args = new NotifyCollectionChangedEventArgs(args.Action, args.OldItems[0], num2, num);
			break;
		case NotifyCollectionChangedAction.Reset:
			if (_traceLog != null)
			{
				_traceLog.Add("ContainerCollectionChange from {0}  action = {1}", TraceLog.IdFor(sender), args.Action);
			}
			UpdateCurrencyAfterRefresh(sender);
			break;
		default:
			throw new NotSupportedException(SR.Format(SR.UnexpectedCollectionChangeAction, args.Action));
		}
		_version++;
		OnCollectionChanged(args);
	}

	internal override bool HasReliableHashCodes()
	{
		int i = 0;
		for (int count = _collection.Count; i < count; i++)
		{
			if (_collection[i] is CollectionContainer collectionContainer)
			{
				if (collectionContainer.View is CollectionView collectionView && !collectionView.HasReliableHashCodes())
				{
					return false;
				}
			}
			else if (!HashHelper.HasReliableHashCode(_collection[i]))
			{
				return false;
			}
		}
		return true;
	}

	internal override void GetCollectionChangedSources(int level, Action<int, object, bool?, List<string>> format, List<string> sources)
	{
		format(level, this, false, sources);
		if (_collection != null)
		{
			_collection.GetCollectionChangedSources(level + 1, format, sources);
		}
	}

	private int FindItem(object item, bool changeCurrent)
	{
		int i = 0;
		int num = 0;
		int num2 = 0;
		for (; i < _collection.Count; i++)
		{
			if (!(_collection[i] is CollectionContainer collectionContainer))
			{
				if (ItemsControl.EqualsEx(_collection[i], item))
				{
					break;
				}
				num2++;
				continue;
			}
			num = collectionContainer.ViewIndexOf(item);
			if (num >= 0)
			{
				num2 += num;
				break;
			}
			num = 0;
			num2 += collectionContainer.ViewCount;
		}
		if (i >= _collection.Count)
		{
			CacheCount(num2);
			num2 = -1;
			item = null;
			i = -1;
			num = 0;
		}
		if (changeCurrent && CurrentPosition != num2 && OKToChangeCurrent())
		{
			object currentItem = CurrentItem;
			int currentPosition = CurrentPosition;
			bool isCurrentAfterLast = IsCurrentAfterLast;
			bool isCurrentBeforeFirst = IsCurrentBeforeFirst;
			SetCurrent(item, num2);
			_currentPositionX = i;
			_currentPositionY = num;
			OnCurrentChanged();
			if (IsCurrentAfterLast != isCurrentAfterLast)
			{
				OnPropertyChanged("IsCurrentAfterLast");
			}
			if (IsCurrentBeforeFirst != isCurrentBeforeFirst)
			{
				OnPropertyChanged("IsCurrentBeforeFirst");
			}
			if (currentPosition != CurrentPosition)
			{
				OnPropertyChanged("CurrentPosition");
			}
			if (currentItem != CurrentItem)
			{
				OnPropertyChanged("CurrentItem");
			}
		}
		return num2;
	}

	private object GetItem(int flatIndex, out int positionX, out int positionY)
	{
		positionY = 0;
		if (flatIndex == -1)
		{
			positionX = -1;
			return null;
		}
		if (_count >= 0 && flatIndex >= _count)
		{
			positionX = _collection.Count;
			return s_afterLast;
		}
		int num = 0;
		for (int i = 0; i < _collection.Count; i++)
		{
			if (!(_collection[i] is CollectionContainer collectionContainer))
			{
				if (num == flatIndex)
				{
					positionX = i;
					return _collection[i];
				}
				num++;
			}
			else if (collectionContainer.Collection != null)
			{
				int num2 = flatIndex - num;
				int viewCount = collectionContainer.ViewCount;
				if (num2 < viewCount)
				{
					positionX = i;
					positionY = num2;
					return collectionContainer.ViewItem(num2);
				}
				num += viewCount;
			}
		}
		CacheCount(num);
		positionX = _collection.Count;
		return s_afterLast;
	}

	private object GetNextItemFromXY(int positionX, int positionY)
	{
		Invariant.Assert(positionY >= 0);
		object result = null;
		while (positionX < _collection.Count)
		{
			if (!(_collection[positionX] is CollectionContainer collectionContainer))
			{
				result = _collection[positionX];
				positionY = 0;
				break;
			}
			if (positionY < collectionContainer.ViewCount)
			{
				result = collectionContainer.ViewItem(positionY);
				break;
			}
			positionY = 0;
			positionX++;
		}
		if (positionX < _collection.Count)
		{
			_currentPositionX = positionX;
			_currentPositionY = positionY;
		}
		else
		{
			_currentPositionX = _collection.Count;
			_currentPositionY = 0;
		}
		return result;
	}

	private int CountDeep(int end)
	{
		if (Invariant.Strict)
		{
			Invariant.Assert(end <= _collection.Count);
		}
		int num = 0;
		for (int i = 0; i < end; i++)
		{
			num = ((_collection[i] is CollectionContainer collectionContainer) ? (num + collectionContainer.ViewCount) : (num + 1));
		}
		return num;
	}

	private void CacheCount(int count)
	{
		bool num = _count != count && _count >= 0;
		_count = count;
		if (num)
		{
			OnPropertyChanged("Count");
		}
	}

	private bool _MoveTo(int proposed)
	{
		int positionX;
		int positionY;
		object item = GetItem(proposed, out positionX, out positionY);
		if (proposed != CurrentPosition || item != CurrentItem)
		{
			Invariant.Assert(_count < 0 || proposed <= _count);
			if (OKToChangeCurrent())
			{
				object currentItem = CurrentItem;
				int currentPosition = CurrentPosition;
				bool isCurrentAfterLast = IsCurrentAfterLast;
				bool isCurrentBeforeFirst = IsCurrentBeforeFirst;
				_currentPositionX = positionX;
				_currentPositionY = positionY;
				if (item == s_afterLast)
				{
					SetCurrent(null, Count);
				}
				else
				{
					SetCurrent(item, proposed);
				}
				OnCurrentChanged();
				if (IsCurrentAfterLast != isCurrentAfterLast)
				{
					OnPropertyChanged("IsCurrentAfterLast");
				}
				if (IsCurrentBeforeFirst != isCurrentBeforeFirst)
				{
					OnPropertyChanged("IsCurrentBeforeFirst");
				}
				if (currentPosition != CurrentPosition)
				{
					OnPropertyChanged("CurrentPosition");
				}
				if (currentItem != CurrentItem)
				{
					OnPropertyChanged("CurrentItem");
				}
			}
		}
		return IsCurrentInView;
	}

	private int DeduceFlatIndexForAdd(CollectionContainer sender, int x)
	{
		if (_currentPositionX > x)
		{
			return 0;
		}
		if (_currentPositionX < x)
		{
			return CurrentPosition + 1;
		}
		object o = sender.ViewItem(_currentPositionY);
		if (ItemsControl.EqualsEx(CurrentItem, o))
		{
			return CurrentPosition + 1;
		}
		return 0;
	}

	private int DeduceFlatIndexForRemove(CollectionContainer sender, int x, object item)
	{
		if (_currentPositionX > x)
		{
			return 0;
		}
		if (_currentPositionX < x)
		{
			return CurrentPosition + 1;
		}
		if (ItemsControl.EqualsEx(item, CurrentItem))
		{
			return CurrentPosition;
		}
		object o = sender.ViewItem(_currentPositionY);
		if (ItemsControl.EqualsEx(item, o))
		{
			return CurrentPosition + 1;
		}
		return 0;
	}

	private void UpdateCurrencyAfterAdd(int flatIndex, int positionX, bool isCompositeItem)
	{
		if (flatIndex >= 0 && flatIndex <= CurrentPosition)
		{
			int newPosition = CurrentPosition + 1;
			if (isCompositeItem)
			{
				_currentPositionX++;
			}
			else if (positionX == _currentPositionX)
			{
				_currentPositionY++;
			}
			SetCurrent(GetNextItemFromXY(_currentPositionX, _currentPositionY), newPosition);
		}
	}

	private void UpdateCurrencyAfterRemove(int flatIndex, int positionX, bool isCompositeItem)
	{
		if (flatIndex < 0)
		{
			return;
		}
		if (flatIndex < CurrentPosition)
		{
			SetCurrent(CurrentItem, CurrentPosition - 1);
			if (isCompositeItem)
			{
				_currentPositionX--;
			}
			else if (positionX == _currentPositionX)
			{
				_currentPositionY--;
			}
		}
		else if (flatIndex == CurrentPosition)
		{
			MoveCurrencyOffDeletedElement();
		}
	}

	private void UpdateCurrencyAfterMove(int oldIndex, int newIndex, int positionX, bool isCompositeItem)
	{
		if ((oldIndex >= CurrentPosition || newIndex >= CurrentPosition) && (oldIndex <= CurrentPosition || newIndex <= CurrentPosition))
		{
			if (newIndex <= CurrentPosition)
			{
				UpdateCurrencyAfterAdd(newIndex, positionX, isCompositeItem);
			}
			if (oldIndex <= CurrentPosition)
			{
				UpdateCurrencyAfterRemove(oldIndex, positionX, isCompositeItem);
			}
		}
	}

	private void UpdateCurrencyAfterRefresh(object refreshedObject)
	{
		Invariant.Assert(refreshedObject is CollectionContainer);
		object currentItem = CurrentItem;
		int currentPosition = CurrentPosition;
		bool isCurrentAfterLast = IsCurrentAfterLast;
		bool isCurrentBeforeFirst = IsCurrentBeforeFirst;
		if (IsCurrentInView && refreshedObject == _collection[_currentPositionX])
		{
			CollectionContainer collectionContainer = refreshedObject as CollectionContainer;
			if (collectionContainer.ViewCount == 0)
			{
				_currentPositionY = 0;
				MoveCurrencyOffDeletedElement();
			}
			else
			{
				int num = collectionContainer.ViewIndexOf(CurrentItem);
				if (num >= 0)
				{
					_currentPositionY = num;
					SetCurrentPositionFromXY(_currentPositionX, _currentPositionY);
				}
				else
				{
					OnCurrentChanging();
					SetCurrentBeforeFirst();
					OnCurrentChanged();
				}
			}
		}
		else
		{
			for (int i = 0; i < _currentPositionX; i++)
			{
				if (_collection[i] == refreshedObject)
				{
					SetCurrentPositionFromXY(_currentPositionX, _currentPositionY);
					break;
				}
			}
		}
		if (IsCurrentAfterLast != isCurrentAfterLast)
		{
			OnPropertyChanged("IsCurrentAfterLast");
		}
		if (IsCurrentBeforeFirst != isCurrentBeforeFirst)
		{
			OnPropertyChanged("IsCurrentBeforeFirst");
		}
		if (currentPosition != CurrentPosition)
		{
			OnPropertyChanged("CurrentPosition");
		}
		if (currentItem != CurrentItem)
		{
			OnPropertyChanged("CurrentItem");
		}
	}

	private void MoveCurrencyOffDeletedElement()
	{
		int currentPosition = CurrentPosition;
		OnCurrentChanging();
		object nextItemFromXY = GetNextItemFromXY(_currentPositionX, _currentPositionY);
		if (_currentPositionX >= _collection.Count)
		{
			nextItemFromXY = GetLastItem(out _currentPositionX, out _currentPositionY);
			SetCurrent(nextItemFromXY, Count - 1);
		}
		else
		{
			SetCurrentPositionFromXY(_currentPositionX, _currentPositionY);
			SetCurrent(nextItemFromXY, CurrentPosition);
		}
		OnCurrentChanged();
		OnPropertyChanged("Count");
		OnPropertyChanged("CurrentItem");
		if (IsCurrentAfterLast)
		{
			OnPropertyChanged("IsCurrentAfterLast");
		}
		if (IsCurrentBeforeFirst)
		{
			OnPropertyChanged("IsCurrentBeforeFirst");
		}
		if (CurrentPosition != currentPosition)
		{
			OnPropertyChanged("CurrentPosition");
		}
	}

	private object GetLastItem(out int positionX, out int positionY)
	{
		object result = null;
		positionX = -1;
		positionY = 0;
		if (_count != 0)
		{
			for (positionX = _collection.Count - 1; positionX >= 0; positionX--)
			{
				if (!(_collection[positionX] is CollectionContainer collectionContainer))
				{
					result = _collection[positionX];
					break;
				}
				if (collectionContainer.ViewCount > 0)
				{
					positionY = collectionContainer.ViewCount - 1;
					result = collectionContainer.ViewItem(positionY);
					break;
				}
			}
			if (positionX < 0)
			{
				CacheCount(0);
			}
		}
		return result;
	}

	private void SetCurrentBeforeFirst()
	{
		_currentPositionX = -1;
		_currentPositionY = 0;
		SetCurrent(null, -1);
	}

	private void SetCurrentPositionFromXY(int x, int y)
	{
		if (IsCurrentBeforeFirst)
		{
			SetCurrent(null, -1);
		}
		else if (IsCurrentAfterLast)
		{
			SetCurrent(null, Count);
		}
		else
		{
			SetCurrent(CurrentItem, CountDeep(x) + y);
		}
	}

	private void InitializeTraceLog()
	{
		_traceLog = new TraceLog(20);
	}

	private void TraceContainerCollectionChange(object sender, NotifyCollectionChangedAction action, object oldItem, object newItem)
	{
		if (_traceLog != null)
		{
			_traceLog.Add("ContainerCollectionChange from {0}  action = {1} oldItem = {2} newItem = {3}", TraceLog.IdFor(sender), action, TraceLog.IdFor(oldItem), TraceLog.IdFor(newItem));
		}
	}

	private void ValidateCollectionChangedEventArgs(NotifyCollectionChangedEventArgs e)
	{
		switch (e.Action)
		{
		case NotifyCollectionChangedAction.Add:
			if (e.NewItems.Count != 1)
			{
				throw new NotSupportedException(SR.RangeActionsNotSupported);
			}
			break;
		case NotifyCollectionChangedAction.Remove:
			if (e.OldItems.Count != 1)
			{
				throw new NotSupportedException(SR.RangeActionsNotSupported);
			}
			break;
		case NotifyCollectionChangedAction.Replace:
			if (e.NewItems.Count != 1 || e.OldItems.Count != 1)
			{
				throw new NotSupportedException(SR.RangeActionsNotSupported);
			}
			break;
		case NotifyCollectionChangedAction.Move:
			if (e.NewItems.Count != 1)
			{
				throw new NotSupportedException(SR.RangeActionsNotSupported);
			}
			if (e.NewStartingIndex < 0)
			{
				throw new InvalidOperationException(SR.CannotMoveToUnknownPosition);
			}
			break;
		default:
			throw new NotSupportedException(SR.Format(SR.UnexpectedCollectionChangeAction, e.Action));
		case NotifyCollectionChangedAction.Reset:
			break;
		}
	}

	private void OnPropertyChanged(string propertyName)
	{
		OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
	}
}
