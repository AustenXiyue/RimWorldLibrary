using System.Collections;
using System.Collections.Generic;
using MS.Internal.FontFace;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Represents an ordered collection of <see cref="T:System.Windows.Media.FontFamilyMap" /> objects.</summary>
public sealed class FontFamilyMapCollection : IList<FontFamilyMap>, ICollection<FontFamilyMap>, IEnumerable<FontFamilyMap>, IEnumerable, IList, ICollection
{
	private class Enumerator : IEnumerator<FontFamilyMap>, IEnumerator, IDisposable
	{
		private FontFamilyMap[] _items;

		private int _count;

		private int _index;

		private FontFamilyMap _current;

		public FontFamilyMap Current => _current;

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

		internal Enumerator(FontFamilyMap[] items, int count)
		{
			_items = items;
			_count = count;
			_index = -1;
			_current = null;
		}

		public bool MoveNext()
		{
			if (_index < _count)
			{
				_index++;
				if (_index < _count)
				{
					_current = _items[_index];
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

	private const int InitialCapacity = 8;

	private CompositeFontInfo _fontInfo;

	private FontFamilyMap[] _items;

	private int _count;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.IsSynchronized" />.</summary>
	/// <returns>true if access to the <see cref="T:System.Windows.Media.FontFamilyMapCollection" /> is synchronized (thread safe); otherwise, false.</returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.SyncRoot" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Windows.Media.FontFamilyMapCollection" />.</returns>
	object ICollection.SyncRoot
	{
		get
		{
			if (_fontInfo == null)
			{
				return this;
			}
			return _fontInfo;
		}
	}

	/// <summary>Gets the number of <see cref="T:System.Windows.Media.FontFamilyMap" /> objects in the <see cref="T:System.Windows.Media.FontFamilyMapCollection" />.</summary>
	/// <returns>The number of objects in the collection.</returns>
	public int Count => _count;

	/// <summary>Gets a value that indicates if a <see cref="T:System.Windows.Media.FontFamilyMapCollection" /> is read only.</summary>
	/// <returns>true if the collection is read only; otherwise false.</returns>
	public bool IsReadOnly => _fontInfo == null;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.FontFamilyMap" /> object at the specified index position.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.FontFamilyMap" /> object at the <paramref name="index" /> position.</returns>
	/// <param name="index">The zero-based index position of the object to get or set.</param>
	public FontFamilyMap this[int index]
	{
		get
		{
			RangeCheck(index);
			return _items[index];
		}
		set
		{
			SetItem(index, value);
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsFixedSize" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.FontFamilyMapCollection" /> has a fixed size; otherwise, false.</returns>
	bool IList.IsFixedSize => IsReadOnly;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.Item(System.Int32)" />.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the element to get or set. </param>
	object IList.this[int index]
	{
		get
		{
			RangeCheck(index);
			return _items[index];
		}
		set
		{
			SetItem(index, ConvertValue(value));
		}
	}

	internal FontFamilyMapCollection(CompositeFontInfo fontInfo)
	{
		_fontInfo = fontInfo;
		_items = null;
		_count = 0;
	}

	/// <summary>Returns an enumerator that can iterate through the collection.</summary>
	/// <returns>An enumerator that can iterate through the collection.</returns>
	public IEnumerator<FontFamilyMap> GetEnumerator()
	{
		return new Enumerator(_items, _count);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IEnumerable.GetEnumerator" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(_items, _count);
	}

	/// <summary>Inserts the specified <see cref="T:System.Windows.Media.FontFamilyMap" /> object into the collection.</summary>
	/// <param name="item">The object to insert.</param>
	public void Add(FontFamilyMap item)
	{
		InsertItem(_count, item);
	}

	/// <summary>Removes all <see cref="T:System.Windows.Media.FontFamilyMap" /> objects from the <see cref="T:System.Windows.Media.FontFamilyMapCollection" />.</summary>
	public void Clear()
	{
		ClearItems();
	}

	/// <summary>Indicates whether the <see cref="T:System.Windows.Media.FontFamilyMapCollection" /> contains the specified <see cref="T:System.Windows.Media.FontFamilyMap" /> object.</summary>
	/// <returns>true if the collection contains <paramref name="item" />; otherwise false.</returns>
	/// <param name="item">The object to locate.</param>
	public bool Contains(FontFamilyMap item)
	{
		return FindItem(item) >= 0;
	}

	/// <summary>Copies the <see cref="T:System.Windows.Media.FontFamilyMap" /> objects in the collection into an array of FontFamilyMaps, starting at the specified index position.</summary>
	/// <param name="array">The destination array.</param>
	/// <param name="index">The zero-based index position where copying begins.</param>
	public void CopyTo(FontFamilyMap[] array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
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
			Array.Copy(_items, 0, array, index, _count);
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items from the <see cref="T:System.Windows.Media.FontFamilyMapCollection" />.</param>
	/// <param name="index">The first position in the specified <see cref="T:System.Array" /> to receive the copied contents.</param>
	void ICollection.CopyTo(Array array, int index)
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
			throw new ArgumentException(SR.Format(SR.CannotConvertType, typeof(FamilyTypeface), elementType));
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
			Array.Copy(_items, 0, array, index, _count);
		}
	}

	/// <summary>Removes the specified <see cref="T:System.Windows.Media.FontFamilyMap" /> object from the collection.</summary>
	/// <returns>true if <paramref name="item" /> was successfully deleted; otherwise false.</returns>
	/// <param name="item">The object to remove.</param>
	public bool Remove(FontFamilyMap item)
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

	/// <summary>Returns the index of the specified <see cref="T:System.Windows.Media.FontFamilyMap" /> object within the collection.</summary>
	/// <returns>The zero-based index of <paramref name="item" />, if found; otherwise -1;</returns>
	/// <param name="item">The object to locate.</param>
	public int IndexOf(FontFamilyMap item)
	{
		return FindItem(item);
	}

	/// <summary>Inserts the specified <see cref="T:System.Windows.Media.FontFamilyMap" /> object at the specified index position in the collection.</summary>
	/// <param name="index">The zero-based index position to insert the object.</param>
	/// <param name="item">The object to insert.</param>
	public void Insert(int index, FontFamilyMap item)
	{
		InsertItem(index, item);
	}

	/// <summary>Deletes a <see cref="T:System.Windows.Media.FontFamilyMap" /> object from the <see cref="T:System.Windows.Media.FontFamilyMapCollection" />.</summary>
	/// <param name="index">The zero-based index position to remove the object.</param>
	public void RemoveAt(int index)
	{
		RemoveItem(index);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Add(System.Object)" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.Media.FontFamilyMapCollection" />.</param>
	int IList.Add(object value)
	{
		return InsertItem(_count, ConvertValue(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Contains(System.Object)" />.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.Media.FontFamilyMapCollection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.TextEffectCollection" />.</param>
	bool IList.Contains(object value)
	{
		return FindItem(value as FontFamilyMap) >= 0;
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.IndexOf(System.Object)" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.FontFamilyMapCollection" />.</param>
	int IList.IndexOf(object value)
	{
		return FindItem(value as FontFamilyMap);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Insert(System.Int32,System.Object)" />.</summary>
	/// <param name="index">The zero-based index at which to insert the <see cref="T:System.Object" />.</param>
	/// <param name="item">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Windows.Media.FontFamilyMapCollection" />.</param>
	void IList.Insert(int index, object item)
	{
		InsertItem(index, ConvertValue(item));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Remove(System.Object)" />.</summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.Media.FontFamilyMapCollection" />.</param>
	void IList.Remove(object value)
	{
		VerifyChangeable();
		int num = FindItem(value as FontFamilyMap);
		if (num >= 0)
		{
			RemoveItem(num);
		}
	}

	private int InsertItem(int index, FontFamilyMap item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		VerifyChangeable();
		if (_count + 1 >= 65535)
		{
			throw new InvalidOperationException(SR.CompositeFont_TooManyFamilyMaps);
		}
		if (index < 0 || index > Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		_fontInfo.PrepareToAddFamilyMap(item);
		if (_items == null)
		{
			_items = new FontFamilyMap[8];
		}
		else if (_count == _items.Length)
		{
			FontFamilyMap[] array = new FontFamilyMap[_count * 2];
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

	private void SetItem(int index, FontFamilyMap item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		VerifyChangeable();
		RangeCheck(index);
		_fontInfo.PrepareToAddFamilyMap(item);
		if (item.Language != _items[index].Language)
		{
			_fontInfo.InvalidateFamilyMapRanges();
		}
		_items[index] = item;
	}

	private void ClearItems()
	{
		VerifyChangeable();
		_fontInfo.InvalidateFamilyMapRanges();
		_count = 0;
		_items = null;
	}

	private void RemoveItem(int index)
	{
		VerifyChangeable();
		RangeCheck(index);
		_fontInfo.InvalidateFamilyMapRanges();
		_count--;
		for (int i = index; i < _count; i++)
		{
			_items[i] = _items[i + 1];
		}
		_items[_count] = null;
	}

	private int FindItem(FontFamilyMap item)
	{
		if (_count != 0 && item != null)
		{
			for (int i = 0; i < _count; i++)
			{
				if (_items[i].Equals(item))
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
		if (_fontInfo == null)
		{
			throw new NotSupportedException(SR.General_ObjectIsReadOnly);
		}
	}

	private FontFamilyMap ConvertValue(object obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		return (obj as FontFamilyMap) ?? throw new ArgumentException(SR.Format(SR.CannotConvertType, obj.GetType(), typeof(FontFamilyMap)));
	}
}
