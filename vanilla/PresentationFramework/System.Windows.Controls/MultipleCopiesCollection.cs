using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;

namespace System.Windows.Controls;

internal class MultipleCopiesCollection : IList, ICollection, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
{
	private class MultipleCopiesCollectionEnumerator : IEnumerator
	{
		private object _item;

		private int _count;

		private int _current;

		private MultipleCopiesCollection _collection;

		object IEnumerator.Current
		{
			get
			{
				if (_current >= 0)
				{
					if (_current < _count)
					{
						return _item;
					}
					throw new InvalidOperationException();
				}
				return null;
			}
		}

		private bool IsCollectionUnchanged
		{
			get
			{
				if (_collection.RepeatCount == _count)
				{
					return _collection.CopiedItem == _item;
				}
				return false;
			}
		}

		public MultipleCopiesCollectionEnumerator(MultipleCopiesCollection collection)
		{
			_collection = collection;
			_item = _collection.CopiedItem;
			_count = _collection.RepeatCount;
			_current = -1;
		}

		bool IEnumerator.MoveNext()
		{
			if (IsCollectionUnchanged)
			{
				int num = _current + 1;
				if (num < _count)
				{
					_current = num;
					return true;
				}
				return false;
			}
			throw new InvalidOperationException();
		}

		void IEnumerator.Reset()
		{
			if (IsCollectionUnchanged)
			{
				_current = -1;
				return;
			}
			throw new InvalidOperationException();
		}
	}

	private object _item;

	private int _count;

	private const string CountName = "Count";

	private const string IndexerName = "Item[]";

	internal object CopiedItem
	{
		get
		{
			return _item;
		}
		set
		{
			if (value == CollectionView.NewItemPlaceholder)
			{
				value = DataGrid.NewItemPlaceholder;
			}
			if (_item != value)
			{
				object item = _item;
				_item = value;
				OnPropertyChanged("Item[]");
				for (int i = 0; i < _count; i++)
				{
					OnReplace(item, _item, i);
				}
			}
		}
	}

	private int RepeatCount
	{
		get
		{
			return _count;
		}
		set
		{
			if (_count != value)
			{
				_count = value;
				OnPropertyChanged("Count");
				OnPropertyChanged("Item[]");
			}
		}
	}

	public bool IsFixedSize => false;

	public bool IsReadOnly => true;

	public object this[int index]
	{
		get
		{
			if (index >= 0 && index < RepeatCount)
			{
				return _item;
			}
			throw new ArgumentOutOfRangeException("index");
		}
		set
		{
			throw new InvalidOperationException();
		}
	}

	public int Count => RepeatCount;

	public bool IsSynchronized => false;

	public object SyncRoot => this;

	public event NotifyCollectionChangedEventHandler CollectionChanged;

	public event PropertyChangedEventHandler PropertyChanged;

	internal MultipleCopiesCollection(object item, int count)
	{
		CopiedItem = item;
		_count = count;
	}

	internal void MirrorCollectionChange(NotifyCollectionChangedEventArgs e)
	{
		switch (e.Action)
		{
		case NotifyCollectionChangedAction.Add:
			Insert(e.NewStartingIndex);
			break;
		case NotifyCollectionChangedAction.Move:
			Move(e.OldStartingIndex, e.NewStartingIndex);
			break;
		case NotifyCollectionChangedAction.Remove:
			RemoveAt(e.OldStartingIndex);
			break;
		case NotifyCollectionChangedAction.Replace:
			OnReplace(CopiedItem, CopiedItem, e.NewStartingIndex);
			break;
		case NotifyCollectionChangedAction.Reset:
			Reset();
			break;
		}
	}

	internal void SyncToCount(int newCount)
	{
		int repeatCount = RepeatCount;
		if (newCount != repeatCount)
		{
			if (newCount > repeatCount)
			{
				InsertRange(repeatCount, newCount - repeatCount);
				return;
			}
			int num = repeatCount - newCount;
			RemoveRange(repeatCount - num, num);
		}
	}

	private void Insert(int index)
	{
		RepeatCount++;
		OnCollectionChanged(NotifyCollectionChangedAction.Add, CopiedItem, index);
	}

	private void InsertRange(int index, int count)
	{
		for (int i = 0; i < count; i++)
		{
			Insert(index);
			index++;
		}
	}

	private void Move(int oldIndex, int newIndex)
	{
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, CopiedItem, newIndex, oldIndex));
	}

	private void RemoveAt(int index)
	{
		RepeatCount--;
		OnCollectionChanged(NotifyCollectionChangedAction.Remove, CopiedItem, index);
	}

	private void RemoveRange(int index, int count)
	{
		for (int i = 0; i < count; i++)
		{
			RemoveAt(index);
		}
	}

	private void OnReplace(object oldItem, object newItem, int index)
	{
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
	}

	private void Reset()
	{
		RepeatCount = 0;
		OnCollectionReset();
	}

	public int Add(object value)
	{
		throw new NotSupportedException(SR.DataGrid_ReadonlyCellsItemsSource);
	}

	public void Clear()
	{
		throw new NotSupportedException(SR.DataGrid_ReadonlyCellsItemsSource);
	}

	public bool Contains(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return _item == value;
	}

	public int IndexOf(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (_item != value)
		{
			return -1;
		}
		return 0;
	}

	public void Insert(int index, object value)
	{
		throw new NotSupportedException(SR.DataGrid_ReadonlyCellsItemsSource);
	}

	public void Remove(object value)
	{
		throw new NotSupportedException(SR.DataGrid_ReadonlyCellsItemsSource);
	}

	void IList.RemoveAt(int index)
	{
		throw new NotSupportedException(SR.DataGrid_ReadonlyCellsItemsSource);
	}

	public void CopyTo(Array array, int index)
	{
		throw new NotSupportedException();
	}

	public IEnumerator GetEnumerator()
	{
		return new MultipleCopiesCollectionEnumerator(this);
	}

	private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
	{
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
	}

	private void OnCollectionReset()
	{
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	}

	private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		if (this.CollectionChanged != null)
		{
			this.CollectionChanged(this, e);
		}
	}

	private void OnPropertyChanged(string propertyName)
	{
		OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
	}

	private void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		if (this.PropertyChanged != null)
		{
			this.PropertyChanged(this, e);
		}
	}
}
