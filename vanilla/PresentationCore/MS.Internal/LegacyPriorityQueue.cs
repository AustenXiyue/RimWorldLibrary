using System.Collections.Generic;

namespace MS.Internal;

internal class LegacyPriorityQueue<T>
{
	private T[] _heap;

	private int _count;

	private IComparer<T> _comparer;

	private const int DefaultCapacity = 6;

	internal int Count => _count;

	internal T Top => _heap[0];

	internal LegacyPriorityQueue(int capacity, IComparer<T> comparer)
	{
		_heap = new T[(capacity > 0) ? capacity : 6];
		_count = 0;
		_comparer = comparer;
	}

	internal void Push(T value)
	{
		if (_count == _heap.Length)
		{
			T[] array = new T[_count * 2];
			for (int i = 0; i < _count; i++)
			{
				array[i] = _heap[i];
			}
			_heap = array;
		}
		int num = _count;
		while (num > 0)
		{
			int num2 = HeapParent(num);
			if (_comparer.Compare(value, _heap[num2]) >= 0)
			{
				break;
			}
			_heap[num] = _heap[num2];
			num = num2;
		}
		_heap[num] = value;
		_count++;
	}

	internal void Pop()
	{
		if (_count > 1)
		{
			int num = 0;
			for (int num2 = HeapLeftChild(num); num2 < _count; num2 = HeapLeftChild(num))
			{
				int num3 = HeapRightFromLeft(num2);
				int num4 = ((num3 < _count && _comparer.Compare(_heap[num3], _heap[num2]) < 0) ? num3 : num2);
				_heap[num] = _heap[num4];
				num = num4;
			}
			_heap[num] = _heap[_count - 1];
		}
		_count--;
	}

	private static int HeapParent(int i)
	{
		return (i - 1) / 2;
	}

	private static int HeapLeftChild(int i)
	{
		return i * 2 + 1;
	}

	private static int HeapRightFromLeft(int i)
	{
		return i + 1;
	}
}
