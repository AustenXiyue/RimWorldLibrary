using System;
using System.Collections;
using System.Collections.Generic;
using MS.Internal.PresentationCore;

namespace MS.Internal;

internal class SequentialUshortCollection : ICollection<ushort>, IEnumerable<ushort>, IEnumerable
{
	private ushort _count;

	public int Count => _count;

	public bool IsReadOnly => true;

	public SequentialUshortCollection(ushort count)
	{
		_count = count;
	}

	public void Add(ushort item)
	{
		throw new NotSupportedException();
	}

	public void Clear()
	{
		throw new NotSupportedException();
	}

	public bool Contains(ushort item)
	{
		return item < _count;
	}

	public void CopyTo(ushort[] array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (array.Rank != 1)
		{
			throw new ArgumentException(SR.Collection_BadRank);
		}
		if (arrayIndex < 0 || arrayIndex >= array.Length || arrayIndex + Count > array.Length)
		{
			throw new ArgumentOutOfRangeException("arrayIndex");
		}
		for (ushort num = 0; num < _count; num++)
		{
			array[arrayIndex + num] = num;
		}
	}

	public bool Remove(ushort item)
	{
		throw new NotSupportedException();
	}

	public IEnumerator<ushort> GetEnumerator()
	{
		ushort i = 0;
		while (i < _count)
		{
			yield return i;
			ushort num = (ushort)(i + 1);
			i = num;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<ushort>)this).GetEnumerator();
	}
}
