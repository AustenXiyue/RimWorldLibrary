using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using MS.Internal.PresentationCore;

namespace MS.Internal.TextFormatting;

internal sealed class ThousandthOfEmRealPoints : IList<Point>, ICollection<Point>, IEnumerable<Point>, IEnumerable
{
	private ThousandthOfEmRealDoubles _xArray;

	private ThousandthOfEmRealDoubles _yArray;

	public int Count => _xArray.Count;

	public bool IsReadOnly => false;

	public Point this[int index]
	{
		get
		{
			return new Point(_xArray[index], _yArray[index]);
		}
		set
		{
			_xArray[index] = value.X;
			_yArray[index] = value.Y;
		}
	}

	internal ThousandthOfEmRealPoints(double emSize, int capacity)
	{
		InitArrays(emSize, capacity);
	}

	internal ThousandthOfEmRealPoints(double emSize, IList<Point> pointValues)
	{
		InitArrays(emSize, pointValues.Count);
		for (int i = 0; i < Count; i++)
		{
			_xArray[i] = pointValues[i].X;
			_yArray[i] = pointValues[i].Y;
		}
	}

	public int IndexOf(Point item)
	{
		for (int i = 0; i < Count; i++)
		{
			if (_xArray[i] == item.X && _yArray[i] == item.Y)
			{
				return i;
			}
		}
		return -1;
	}

	public void Clear()
	{
		_xArray.Clear();
		_yArray.Clear();
	}

	public bool Contains(Point item)
	{
		return IndexOf(item) >= 0;
	}

	public void CopyTo(Point[] array, int arrayIndex)
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

	public IEnumerator<Point> GetEnumerator()
	{
		for (int i = 0; i < Count; i++)
		{
			yield return this[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<Point>)this).GetEnumerator();
	}

	public void Add(Point value)
	{
		throw new NotSupportedException(SR.CollectionIsFixedSize);
	}

	public void Insert(int index, Point item)
	{
		throw new NotSupportedException(SR.CollectionIsFixedSize);
	}

	public bool Remove(Point item)
	{
		throw new NotSupportedException(SR.CollectionIsFixedSize);
	}

	public void RemoveAt(int index)
	{
		throw new NotSupportedException(SR.CollectionIsFixedSize);
	}

	private void InitArrays(double emSize, int capacity)
	{
		_xArray = new ThousandthOfEmRealDoubles(emSize, capacity);
		_yArray = new ThousandthOfEmRealDoubles(emSize, capacity);
	}
}
