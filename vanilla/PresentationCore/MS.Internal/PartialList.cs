using System;
using System.Collections;
using System.Collections.Generic;

namespace MS.Internal;

internal class PartialList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
{
	private IList<T> _list;

	private int _initialIndex;

	private int _count;

	public T this[int index]
	{
		get
		{
			return _list[index + _initialIndex];
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public bool IsReadOnly => true;

	public int Count => _count;

	public PartialList(IList<T> list)
	{
		_list = list;
		_initialIndex = 0;
		_count = list.Count;
	}

	public PartialList(IList<T> list, int initialIndex, int count)
	{
		_list = list;
		_initialIndex = initialIndex;
		_count = count;
	}

	public void RemoveAt(int index)
	{
		throw new NotSupportedException();
	}

	public void Insert(int index, T item)
	{
		throw new NotSupportedException();
	}

	public int IndexOf(T item)
	{
		int num = _list.IndexOf(item);
		if (num == -1 || num < _initialIndex || num - _initialIndex >= _count)
		{
			return -1;
		}
		return num - _initialIndex;
	}

	public void Clear()
	{
		throw new NotSupportedException();
	}

	public void Add(T item)
	{
		throw new NotSupportedException();
	}

	public bool Contains(T item)
	{
		return IndexOf(item) != -1;
	}

	public bool Remove(T item)
	{
		throw new NotSupportedException();
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		if (arrayIndex < 0)
		{
			throw new ArgumentOutOfRangeException("arrayIndex");
		}
		for (int i = 0; i < _count; i++)
		{
			array[arrayIndex + i] = this[i];
		}
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		int i = _initialIndex;
		while (i < _initialIndex + _count)
		{
			yield return _list[i];
			int num = i + 1;
			i = num;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<T>)this).GetEnumerator();
	}
}
