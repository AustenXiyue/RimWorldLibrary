using System;
using System.Collections;
using System.Collections.Generic;
using MS.Internal.PresentationCore;

namespace MS.Internal;

internal struct PartialArray<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
{
	private T[] _array;

	private int _initialIndex;

	private int _count;

	public bool IsReadOnly => false;

	public bool IsFixedSize => true;

	public T this[int index]
	{
		get
		{
			return _array[index + _initialIndex];
		}
		set
		{
			_array[index + _initialIndex] = value;
		}
	}

	public int Count => _count;

	public PartialArray(T[] array, int initialIndex, int count)
	{
		_array = array;
		_initialIndex = initialIndex;
		_count = count;
	}

	public PartialArray(T[] array)
		: this(array, 0, array.Length)
	{
	}

	public bool Contains(T item)
	{
		return IndexOf(item) >= 0;
	}

	public bool Remove(T item)
	{
		throw new NotSupportedException(SR.CollectionIsFixedSize);
	}

	public void RemoveAt(int index)
	{
		throw new NotSupportedException(SR.CollectionIsFixedSize);
	}

	public void Clear()
	{
		throw new NotSupportedException();
	}

	public void Add(T item)
	{
		throw new NotSupportedException(SR.CollectionIsFixedSize);
	}

	public void Insert(int index, T item)
	{
		throw new NotSupportedException(SR.CollectionIsFixedSize);
	}

	public int IndexOf(T item)
	{
		int num = Array.IndexOf(_array, item, _initialIndex, _count);
		if (num >= 0)
		{
			return num - _initialIndex;
		}
		return -1;
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (array.Rank != 1)
		{
			throw new ArgumentException(SR.Collection_CopyTo_ArrayCannotBeMultidimensional, "array");
		}
		if (arrayIndex < 0)
		{
			throw new ArgumentOutOfRangeException("arrayIndex");
		}
		if (arrayIndex >= array.Length)
		{
			throw new ArgumentException(SR.Format(SR.Collection_CopyTo_IndexGreaterThanOrEqualToArrayLength, "arrayIndex", "array"), "arrayIndex");
		}
		if (array.Length - Count - arrayIndex < 0)
		{
			throw new ArgumentException(SR.Format(SR.Collection_CopyTo_NumberOfElementsExceedsArrayLength, "arrayIndex", "array"));
		}
		for (int i = 0; i < Count; i++)
		{
			array[arrayIndex + i] = this[i];
		}
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		for (int i = 0; i < Count; i++)
		{
			yield return this[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<T>)this).GetEnumerator();
	}
}
