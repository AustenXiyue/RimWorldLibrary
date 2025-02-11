using System.Collections;
using System.Collections.Generic;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Represents a collection of <see cref="T:System.Windows.Media.FamilyTypeface" /> instances.</summary>
public sealed class FamilyTypefaceCollection : IList<FamilyTypeface>, ICollection<FamilyTypeface>, IEnumerable<FamilyTypeface>, IEnumerable, IList, ICollection
{
	private class Enumerator : IEnumerator<FamilyTypeface>, IEnumerator, IDisposable
	{
		private FamilyTypefaceCollection _list;

		private int _index;

		private FamilyTypeface _current;

		public FamilyTypeface Current => _current;

		object IEnumerator.Current
		{
			get
			{
				if (_current == null)
				{
					throw new InvalidOperationException(SR.Enumerator_VerifyContext);
				}
				return _current;
			}
		}

		internal Enumerator(FamilyTypefaceCollection list)
		{
			_list = list;
			_index = -1;
			_current = null;
		}

		public bool MoveNext()
		{
			int count = _list.Count;
			if (_index < count)
			{
				_index++;
				if (_index < count)
				{
					_current = _list[_index];
					return true;
				}
			}
			_current = null;
			return false;
		}

		void IEnumerator.Reset()
		{
			_index = -1;
		}

		public void Dispose()
		{
		}
	}

	private const int InitialCapacity = 2;

	private ICollection<Typeface> _innerList;

	private FamilyTypeface[] _items;

	private int _count;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.IsSynchronized" />.</summary>
	/// <returns>true if access to the <see cref="T:System.Windows.Media.FamilyTypefaceCollection" /> is synchronized (thread safe); otherwise, false.</returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.SyncRoot" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Windows.Media.FamilyTypefaceCollection" />.</returns>
	object ICollection.SyncRoot => this;

	/// <summary>Gets the number of <see cref="T:System.Windows.Media.FamilyTypeface" /> objects in the <see cref="T:System.Windows.Media.FamilyTypefaceCollection" />.</summary>
	/// <returns>The number of objects in the collection.</returns>
	public int Count => _count;

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Media.FamilyTypefaceCollection" /> is read-only.</summary>
	/// <returns>true if the collection is read-only; otherwise, false.</returns>
	public bool IsReadOnly => _innerList != null;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.FamilyTypeface" /> that is stored at the zero-based index of the <see cref="T:System.Windows.Media.FamilyTypefaceCollection" />.</summary>
	/// <returns>The element at the specified index. </returns>
	/// <param name="index">The zero-based index of the <see cref="T:System.Windows.Media.FamilyTypefaceCollection" /> from which to get or set the <see cref="T:System.Windows.Media.FamilyTypeface" />.</param>
	public FamilyTypeface this[int index]
	{
		get
		{
			return GetItem(index);
		}
		set
		{
			SetItem(index, value);
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsFixedSize" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.FamilyTypefaceCollection" /> has a fixed size; otherwise, false.</returns>
	bool IList.IsFixedSize => IsReadOnly;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.Item(System.Int32)" />.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the element to get or set. </param>
	object IList.this[int index]
	{
		get
		{
			return GetItem(index);
		}
		set
		{
			SetItem(index, ConvertValue(value));
		}
	}

	internal FamilyTypefaceCollection()
	{
		_innerList = null;
		_items = null;
		_count = 0;
	}

	internal FamilyTypefaceCollection(ICollection<Typeface> innerList)
	{
		_innerList = innerList;
		_items = null;
		_count = innerList.Count;
	}

	/// <summary>Returns an enumerator that can iterate through the collection.</summary>
	/// <returns>An enumerator that can iterate through the collection.</returns>
	public IEnumerator<FamilyTypeface> GetEnumerator()
	{
		return new Enumerator(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IEnumerable.GetEnumerator" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this);
	}

	/// <summary>Inserts the specified <see cref="T:System.Windows.Media.FamilyTypeface" /> object into the collection.</summary>
	/// <param name="item">The <see cref="T:System.Windows.Media.FamilyTypeface" /> object to insert.</param>
	public void Add(FamilyTypeface item)
	{
		InsertItem(_count, item);
	}

	/// <summary>Removes all <see cref="T:System.Windows.Media.FamilyTypeface" /> objects from the <see cref="T:System.Windows.Media.FamilyTypefaceCollection" />.</summary>
	public void Clear()
	{
		ClearItems();
	}

	/// <summary>Determines if the collection contains the specified <see cref="T:System.Windows.Media.FamilyTypeface" />.</summary>
	/// <returns>true if <paramref name="item" /> is in the collection; otherwise, false.</returns>
	/// <param name="item">The <see cref="T:System.Windows.Media.FamilyTypeface" /> object to locate.</param>
	public bool Contains(FamilyTypeface item)
	{
		return FindItem(item) >= 0;
	}

	/// <summary>Copies the <see cref="T:System.Windows.Media.FamilyTypeface" /> objects in the collection into an array of <see cref="T:System.Windows.Media.FamilyTypefaceCollection" />, starting at the specified index position.</summary>
	/// <param name="array">The destination array.</param>
	/// <param name="index">The zero-based index position where copying begins.</param>
	public void CopyTo(FamilyTypeface[] array, int index)
	{
		CopyItems(array, index);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items from the <see cref="T:System.Windows.Media.FamilyTypefaceCollection" />.</param>
	/// <param name="index">The first position in the specified <see cref="T:System.Array" /> to receive the copied contents.</param>
	void ICollection.CopyTo(Array array, int index)
	{
		CopyItems(array, index);
	}

	/// <summary>Removes the specified <see cref="T:System.Windows.Media.FamilyTypeface" /> object from the collection.</summary>
	/// <returns>true if <paramref name="item" /> was successfully deleted; otherwise false.</returns>
	/// <param name="item">The <see cref="T:System.Windows.Media.FamilyTypeface" /> object to remove.</param>
	public bool Remove(FamilyTypeface item)
	{
		VerifyChangeable();
		int num = FindItem(item);
		if (num >= 0)
		{
			RemoveAt(num);
			return true;
		}
		return false;
	}

	/// <summary>Returns the index of the specified <see cref="T:System.Windows.Media.FamilyTypeface" /> object within the collection.</summary>
	/// <returns>The zero-based index of <paramref name="item" />, if found; otherwise -1;</returns>
	/// <param name="item">The <see cref="T:System.Windows.Media.FamilyTypeface" /> object to locate.</param>
	public int IndexOf(FamilyTypeface item)
	{
		return FindItem(item);
	}

	/// <summary>Inserts the specified <see cref="T:System.Windows.Media.FamilyTypeface" /> object at the specified index position in the collection.</summary>
	/// <param name="index">The zero-based index position to insert the object.</param>
	/// <param name="item">The <see cref="T:System.Windows.Media.FamilyTypeface" /> object to insert.</param>
	public void Insert(int index, FamilyTypeface item)
	{
		InsertItem(index, item);
	}

	/// <summary>Removes the specified <see cref="T:System.Windows.Media.FamilyTypeface" /> object from the collection at the specified index.</summary>
	/// <param name="index">The zero-based index position from where to delete the object.</param>
	public void RemoveAt(int index)
	{
		RemoveItem(index);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Add(System.Object)" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.Media.FamilyTypefaceCollection" />.</param>
	int IList.Add(object value)
	{
		return InsertItem(_count, ConvertValue(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Contains(System.Object)" />.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.Media.FamilyTypefaceCollection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.TextEffectCollection" />.</param>
	bool IList.Contains(object value)
	{
		return FindItem(value as FamilyTypeface) >= 0;
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.IndexOf(System.Object)" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.FamilyTypefaceCollection" />.</param>
	int IList.IndexOf(object value)
	{
		return FindItem(value as FamilyTypeface);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Insert(System.Int32,System.Object)" />.</summary>
	/// <param name="index">The zero-based index at which to insert the <see cref="T:System.Object" />.</param>
	/// <param name="item">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Windows.Media.FamilyTypefaceCollection" />.</param>
	void IList.Insert(int index, object item)
	{
		InsertItem(index, ConvertValue(item));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Remove(System.Object)" />.</summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.Media.FamilyTypefaceCollection" />.</param>
	void IList.Remove(object value)
	{
		VerifyChangeable();
		int num = FindItem(value as FamilyTypeface);
		if (num >= 0)
		{
			RemoveItem(num);
		}
	}

	private int InsertItem(int index, FamilyTypeface item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		VerifyChangeable();
		if (index < 0 || index > Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (FindItem(item) >= 0)
		{
			throw new ArgumentException(SR.CompositeFont_DuplicateTypeface);
		}
		if (_items == null)
		{
			_items = new FamilyTypeface[2];
		}
		else if (_count == _items.Length)
		{
			FamilyTypeface[] array = new FamilyTypeface[_count * 2];
			for (int i = 0; i < index; i++)
			{
				array[i] = _items[i];
			}
			for (int j = index; j < _count; j++)
			{
				array[j + 1] = _items[j];
			}
			_items = array;
		}
		else if (index < _count)
		{
			for (int num = _count - 1; num >= index; num--)
			{
				_items[num + 1] = _items[num];
			}
		}
		_items[index] = item;
		_count++;
		return index;
	}

	private void InitializeItemsFromInnerList()
	{
		if (_innerList == null || _items != null)
		{
			return;
		}
		FamilyTypeface[] array = new FamilyTypeface[_count];
		int num = 0;
		foreach (Typeface inner in _innerList)
		{
			array[num++] = new FamilyTypeface(inner);
		}
		_items = array;
	}

	private FamilyTypeface GetItem(int index)
	{
		RangeCheck(index);
		InitializeItemsFromInnerList();
		return _items[index];
	}

	private void SetItem(int index, FamilyTypeface item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		VerifyChangeable();
		RangeCheck(index);
		_items[index] = item;
	}

	private void ClearItems()
	{
		VerifyChangeable();
		_count = 0;
		_items = null;
	}

	private void RemoveItem(int index)
	{
		VerifyChangeable();
		RangeCheck(index);
		_count--;
		for (int i = index; i < _count; i++)
		{
			_items[i] = _items[i + 1];
		}
		_items[_count] = null;
	}

	private int FindItem(FamilyTypeface item)
	{
		InitializeItemsFromInnerList();
		if (_count != 0 && item != null)
		{
			for (int i = 0; i < _count; i++)
			{
				if (GetItem(i).Equals(item))
				{
					return i;
				}
			}
		}
		return -1;
	}

	private void RangeCheck(int index)
	{
		if (index < 0 || index >= _count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
	}

	private void VerifyChangeable()
	{
		if (_innerList != null)
		{
			throw new NotSupportedException(SR.General_ObjectIsReadOnly);
		}
	}

	private FamilyTypeface ConvertValue(object obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		return (obj as FamilyTypeface) ?? throw new ArgumentException(SR.Format(SR.CannotConvertType, obj.GetType(), typeof(FamilyTypeface)));
	}

	private void CopyItems(Array array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (array.Rank != 1)
		{
			throw new ArgumentException(SR.Collection_CopyTo_ArrayCannotBeMultidimensional);
		}
		Type elementType = array.GetType().GetElementType();
		if (!elementType.IsAssignableFrom(typeof(FamilyTypeface)))
		{
			throw new ArgumentException(SR.Format(SR.CannotConvertType, typeof(FamilyTypeface[]), elementType));
		}
		if (index >= array.Length)
		{
			throw new ArgumentException(SR.Format(SR.Collection_CopyTo_IndexGreaterThanOrEqualToArrayLength, "index", "array"));
		}
		if (_count > array.Length - index)
		{
			throw new ArgumentException(SR.Format(SR.Collection_CopyTo_NumberOfElementsExceedsArrayLength, index, "array"));
		}
		if (_count != 0)
		{
			InitializeItemsFromInnerList();
			Array.Copy(_items, 0, array, index, _count);
		}
	}
}
