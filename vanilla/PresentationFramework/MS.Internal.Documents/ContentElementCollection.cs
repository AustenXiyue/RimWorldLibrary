using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;

namespace MS.Internal.Documents;

internal abstract class ContentElementCollection<TParent, TItem> : IList<TItem>, ICollection<TItem>, IEnumerable<TItem>, IEnumerable, IList, ICollection where TParent : TextElement, IAcceptInsertion where TItem : FrameworkContentElement, IIndexedChild<TParent>
{
	protected class ContentElementCollectionEnumeratorSimple : IEnumerator<TItem>, IEnumerator, IDisposable
	{
		private ContentElementCollection<TParent, TItem> _collection;

		private int _index;

		protected int Version;

		private object _currentElement;

		public TItem Current
		{
			get
			{
				if (_currentElement == _collection)
				{
					if (_index == -1)
					{
						throw new InvalidOperationException(SR.EnumeratorNotStarted);
					}
					throw new InvalidOperationException(SR.EnumeratorReachedEnd);
				}
				return (TItem)_currentElement;
			}
		}

		object IEnumerator.Current => Current;

		internal ContentElementCollectionEnumeratorSimple(ContentElementCollection<TParent, TItem> collection)
		{
			_collection = collection;
			_index = -1;
			Version = _collection.Version;
			_currentElement = collection;
		}

		public bool MoveNext()
		{
			if (Version != _collection.Version)
			{
				throw new InvalidOperationException(SR.EnumeratorVersionChanged);
			}
			if (_index < _collection.Size - 1)
			{
				_index++;
				_currentElement = _collection[_index];
				return true;
			}
			_currentElement = _collection;
			_index = _collection.Size;
			return false;
		}

		public void Reset()
		{
			if (Version != _collection.Version)
			{
				throw new InvalidOperationException(SR.EnumeratorVersionChanged);
			}
			_currentElement = _collection;
			_index = -1;
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}
	}

	protected class DummyProxy : DependencyObject
	{
	}

	private readonly TParent _owner;

	private TItem[] _items;

	private int _size;

	private int _version;

	protected const int c_defaultCapacity = 8;

	bool IList.IsFixedSize => false;

	bool IList.IsReadOnly => IsReadOnly;

	object IList.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (!(value is TItem value2))
			{
				throw new ArgumentException(SR.Format(SR.TableCollectionElementTypeExpected, typeof(TItem).Name), "value");
			}
			this[index] = value2;
		}
	}

	public abstract TItem this[int index] { get; set; }

	public int Count => Size;

	public bool IsReadOnly => false;

	public bool IsSynchronized => false;

	public object SyncRoot => this;

	public int Capacity
	{
		get
		{
			return PrivateCapacity;
		}
		set
		{
			PrivateCapacity = value;
		}
	}

	public TParent Owner => _owner;

	protected TItem[] Items
	{
		get
		{
			return _items;
		}
		private set
		{
			_items = value;
		}
	}

	protected int Size
	{
		get
		{
			return _size;
		}
		set
		{
			_size = value;
		}
	}

	protected int Version
	{
		get
		{
			return _version;
		}
		set
		{
			_version = value;
		}
	}

	protected int DefaultCapacity => 8;

	internal int PrivateCapacity
	{
		get
		{
			return Items.Length;
		}
		set
		{
			if (value == Items.Length)
			{
				return;
			}
			if (value < Size)
			{
				throw new ArgumentOutOfRangeException(SR.TableCollectionNotEnoughCapacity);
			}
			if (value > 0)
			{
				TItem[] array = new TItem[value];
				if (Size > 0)
				{
					Array.Copy(Items, 0, array, 0, Size);
				}
				Items = array;
			}
			else
			{
				Items = new TItem[DefaultCapacity];
			}
		}
	}

	internal ContentElementCollection(TParent owner)
	{
		_owner = owner;
		Items = new TItem[DefaultCapacity];
	}

	public void CopyTo(Array array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (array.Rank != 1)
		{
			throw new ArgumentException(SR.TableCollectionRankMultiDimNotSupported);
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", SR.TableCollectionOutOfRangeNeedNonNegNum);
		}
		if (array.Length - index < Size)
		{
			throw new ArgumentException(SR.TableCollectionInvalidOffLen);
		}
		Array.Copy(Items, 0, array, index, Size);
	}

	public void CopyTo(TItem[] array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", SR.TableCollectionOutOfRangeNeedNonNegNum);
		}
		if (array.Length - index < Size)
		{
			throw new ArgumentException(SR.TableCollectionInvalidOffLen);
		}
		Array.Copy(Items, 0, array, index, Size);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	internal IEnumerator GetEnumerator()
	{
		return new ContentElementCollectionEnumeratorSimple(this);
	}

	IEnumerator<TItem> IEnumerable<TItem>.GetEnumerator()
	{
		return new ContentElementCollectionEnumeratorSimple(this);
	}

	public abstract void Add(TItem item);

	public abstract void Clear();

	public bool Contains(TItem item)
	{
		if (BelongsToOwner(item))
		{
			return true;
		}
		return false;
	}

	public int IndexOf(TItem item)
	{
		if (BelongsToOwner(item))
		{
			return item.Index;
		}
		return -1;
	}

	public abstract void Insert(int index, TItem item);

	public abstract bool Remove(TItem item);

	public abstract void RemoveAt(int index);

	public abstract void RemoveRange(int index, int count);

	public void TrimToSize()
	{
		PrivateCapacity = Size;
	}

	int IList.Add(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		TItem val = value as TItem;
		Add(val);
		return ((IList)this).IndexOf((object?)val);
	}

	void IList.Clear()
	{
		Clear();
	}

	bool IList.Contains(object value)
	{
		if (!(value is TItem item))
		{
			return false;
		}
		return Contains(item);
	}

	int IList.IndexOf(object value)
	{
		if (!(value is TItem item))
		{
			return -1;
		}
		return IndexOf(item);
	}

	void IList.Insert(int index, object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is TItem item))
		{
			throw new ArgumentException(SR.Format(SR.TableCollectionElementTypeExpected, typeof(TItem).Name), "value");
		}
		Insert(index, item);
	}

	void IList.Remove(object value)
	{
		if (value is TItem item)
		{
			Remove(item);
		}
	}

	void IList.RemoveAt(int index)
	{
		RemoveAt(index);
	}

	internal void EnsureCapacity(int min)
	{
		if (PrivateCapacity < min)
		{
			PrivateCapacity = Math.Max(min, PrivateCapacity * 2);
		}
	}

	internal abstract void PrivateConnectChild(int index, TItem item);

	internal abstract void PrivateDisconnectChild(TItem item);

	internal void PrivateRemove(TItem item)
	{
		int index = item.Index;
		PrivateDisconnectChild(item);
		int size = Size - 1;
		Size = size;
		for (int i = index; i < Size; i++)
		{
			Items[i] = Items[i + 1];
			Items[i].Index = i;
		}
		Items[Size] = null;
	}

	internal bool BelongsToOwner(TItem item)
	{
		if (item == null)
		{
			return false;
		}
		DependencyObject parent = item.Parent;
		if (parent is DummyProxy)
		{
			parent = LogicalTreeHelper.GetParent(parent);
		}
		return parent == Owner;
	}
}
