using System;
using System.Collections;
using System.Collections.Generic;

namespace MS.Internal;

internal class WeakHashSet<T> : ICollection<T>, IEnumerable<T>, IEnumerable where T : class
{
	private WeakHashtable _hashTable = new WeakHashtable();

	public int Count => _hashTable.Count;

	public bool IsReadOnly => false;

	public void Add(T item)
	{
		if (!_hashTable.ContainsKey(item))
		{
			_hashTable.SetWeak(item, null);
		}
	}

	public void Clear()
	{
		_hashTable.Clear();
	}

	public bool Contains(T item)
	{
		return _hashTable.ContainsKey(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		if (arrayIndex < 0)
		{
			throw new ArgumentOutOfRangeException("arrayIndex");
		}
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		int num = 0;
		using (IEnumerator<T> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				_ = enumerator.Current;
				num++;
			}
		}
		if (num + arrayIndex > array.Length)
		{
			throw new ArgumentOutOfRangeException("arrayIndex");
		}
		using IEnumerator<T> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			array[arrayIndex++] = current;
		}
	}

	public bool Remove(T item)
	{
		if (_hashTable.ContainsKey(item))
		{
			_hashTable.Remove(item);
			return true;
		}
		return false;
	}

	public IEnumerator<T> GetEnumerator()
	{
		foreach (object key in _hashTable.Keys)
		{
			if (key is WeakHashtable.EqualityWeakReference { Target: T target })
			{
				yield return target;
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
