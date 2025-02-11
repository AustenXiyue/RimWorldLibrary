using System;
using System.Collections;
using System.Collections.Generic;
using MS.Internal.PresentationCore;

namespace MS.Internal.TextFormatting;

internal sealed class ThousandthOfEmRealDoubles : IList<double>, ICollection<double>, IEnumerable<double>, IEnumerable
{
	private short[] _shortList;

	private double[] _doubleList;

	private double _emSize;

	private const double ToThousandthOfEm = 1000.0;

	private const double ToReal = 0.001;

	private const double CutOffEmSize = 48.0;

	public int Count
	{
		get
		{
			if (_shortList != null)
			{
				return _shortList.Length;
			}
			return _doubleList.Length;
		}
	}

	public bool IsReadOnly => false;

	public double this[int index]
	{
		get
		{
			if (_shortList != null)
			{
				return ThousandthOfEmToReal(_shortList[index]);
			}
			return _doubleList[index];
		}
		set
		{
			if (_shortList != null)
			{
				if (RealToThousandthOfEm(value, out var thousandthOfEm))
				{
					_shortList[index] = thousandthOfEm;
					return;
				}
				_doubleList = new double[_shortList.Length];
				for (int i = 0; i < _shortList.Length; i++)
				{
					_doubleList[i] = ThousandthOfEmToReal(_shortList[i]);
				}
				_doubleList[index] = value;
				_shortList = null;
			}
			else
			{
				_doubleList[index] = value;
			}
		}
	}

	internal ThousandthOfEmRealDoubles(double emSize, int capacity)
	{
		_emSize = emSize;
		InitArrays(capacity);
	}

	internal ThousandthOfEmRealDoubles(double emSize, IList<double> realValues)
	{
		_emSize = emSize;
		InitArrays(realValues.Count);
		for (int i = 0; i < Count; i++)
		{
			this[i] = realValues[i];
		}
	}

	public int IndexOf(double item)
	{
		for (int i = 0; i < Count; i++)
		{
			if (this[i] == item)
			{
				return i;
			}
		}
		return -1;
	}

	public void Clear()
	{
		if (_shortList != null)
		{
			for (int i = 0; i < _shortList.Length; i++)
			{
				_shortList[i] = 0;
			}
		}
		else
		{
			for (int j = 0; j < _doubleList.Length; j++)
			{
				_doubleList[j] = 0.0;
			}
		}
	}

	public bool Contains(double item)
	{
		return IndexOf(item) >= 0;
	}

	public void CopyTo(double[] array, int arrayIndex)
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

	public IEnumerator<double> GetEnumerator()
	{
		for (int i = 0; i < Count; i++)
		{
			yield return this[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<double>)this).GetEnumerator();
	}

	public void Add(double value)
	{
		throw new NotSupportedException(SR.CollectionIsFixedSize);
	}

	public void Insert(int index, double item)
	{
		throw new NotSupportedException(SR.CollectionIsFixedSize);
	}

	public bool Remove(double item)
	{
		throw new NotSupportedException(SR.CollectionIsFixedSize);
	}

	public void RemoveAt(int index)
	{
		throw new NotSupportedException(SR.CollectionIsFixedSize);
	}

	private void InitArrays(int capacity)
	{
		if (_emSize > 48.0)
		{
			_doubleList = new double[capacity];
		}
		else
		{
			_shortList = new short[capacity];
		}
	}

	private bool RealToThousandthOfEm(double value, out short thousandthOfEm)
	{
		double num = value / _emSize * 1000.0;
		if (num > 32767.0 || num < -32768.0)
		{
			thousandthOfEm = 0;
			return false;
		}
		thousandthOfEm = (short)Math.Round(num);
		return true;
	}

	private double ThousandthOfEmToReal(short thousandthOfEm)
	{
		return (double)thousandthOfEm * 0.001 * _emSize;
	}
}
