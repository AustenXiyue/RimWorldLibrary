using System;
using System.Collections;
using System.Collections.Generic;
using MS.Utility;

namespace MS.Internal.Generic;

internal struct SpanVector<T> : IEnumerable<Span<T>>, IEnumerable
{
	private struct SpanEnumerator<U> : IEnumerator<Span<U>>, IEnumerator, IDisposable
	{
		private SpanVector<U> _vector;

		private int _current;

		public Span<U> Current => _vector[_current];

		object IEnumerator.Current => Current;

		internal SpanEnumerator(SpanVector<U> vector)
		{
			_vector = vector;
			_current = -1;
		}

		void IDisposable.Dispose()
		{
		}

		public bool MoveNext()
		{
			_current++;
			return _current < _vector.Count;
		}

		public void Reset()
		{
			_current = -1;
		}
	}

	private FrugalStructList<Span<T>> _spanList;

	private T _defaultValue;

	internal int Count => _spanList.Count;

	internal T DefaultValue => _defaultValue;

	internal Span<T> this[int index] => _spanList[index];

	internal SpanVector(T defaultValue)
		: this(defaultValue, default(FrugalStructList<Span<T>>))
	{
	}

	private SpanVector(T defaultValue, FrugalStructList<Span<T>> spanList)
	{
		_defaultValue = defaultValue;
		_spanList = spanList;
	}

	public IEnumerator<Span<T>> GetEnumerator()
	{
		return new SpanEnumerator<T>(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private void Add(Span<T> span)
	{
		_spanList.Add(span);
	}

	internal void Delete(int index, int count)
	{
		for (int num = index + count - 1; num >= index; num--)
		{
			_spanList.RemoveAt(num);
		}
	}

	private void Insert(int index, int count)
	{
		for (int i = 0; i < count; i++)
		{
			_spanList.Insert(index, default(Span<T>));
		}
	}

	internal void Set(int first, int length, T value)
	{
		int i = 0;
		int num;
		for (num = 0; i < Count && num + _spanList[i].Length <= first; i++)
		{
			num += _spanList[i].Length;
		}
		if (i >= Count)
		{
			if (num < first)
			{
				Add(new Span<T>(_defaultValue, first - num));
			}
			if (Count > 0 && _spanList[Count - 1].Value.Equals(value))
			{
				Span<T> span = _spanList[Count - 1];
				_spanList[Count - 1] = new Span<T>(span.Value, span.Length + length);
			}
			else
			{
				Add(new Span<T>(value, length));
			}
			return;
		}
		int j = i;
		int num2;
		for (num2 = num; j < Count && num2 + _spanList[j].Length <= first + length; j++)
		{
			num2 += _spanList[j].Length;
		}
		if (first == num)
		{
			if (i > 0 && _spanList[i - 1].Value.Equals(value))
			{
				i--;
				num -= _spanList[i].Length;
				first = num;
				length += _spanList[i].Length;
			}
		}
		else if (_spanList[i].Value.Equals(value))
		{
			length = first + length - num;
			first = num;
		}
		if (j < Count && _spanList[j].Value.Equals(value))
		{
			length = num2 + _spanList[j].Length - first;
			num2 += _spanList[j].Length;
			j++;
		}
		if (j >= Count)
		{
			if (num < first)
			{
				if (Count != i + 2 && !Resize(i + 2))
				{
					throw new OutOfMemoryException();
				}
				Span<T> span2 = _spanList[i];
				_spanList[i] = new Span<T>(span2.Value, first - num);
				_spanList[i + 1] = new Span<T>(value, length);
			}
			else
			{
				if (Count != i + 1 && !Resize(i + 1))
				{
					throw new OutOfMemoryException();
				}
				_spanList[i] = new Span<T>(value, length);
			}
			return;
		}
		T value2 = default(Span<T>).Value;
		int length2 = 0;
		if (first + length > num2)
		{
			value2 = _spanList[j].Value;
			length2 = num2 + _spanList[j].Length - (first + length);
		}
		int num3 = 1 + ((first > num) ? 1 : 0) - (j - i);
		if (num3 < 0)
		{
			Delete(i + 1, -num3);
		}
		else if (num3 > 0)
		{
			Insert(i + 1, num3);
			for (int k = 0; k < num3; k++)
			{
				_spanList[i + 1 + k] = default(Span<T>);
			}
		}
		if (num < first)
		{
			Span<T> span3 = _spanList[i];
			_spanList[i] = new Span<T>(span3.Value, first - num);
			i++;
		}
		_spanList[i] = new Span<T>(value, length);
		i++;
		if (num2 < first + length)
		{
			_spanList[i] = new Span<T>(value2, length2);
		}
	}

	private bool Resize(int targetCount)
	{
		if (targetCount > Count)
		{
			for (int i = 0; i < targetCount - Count; i++)
			{
				_spanList.Add(default(Span<T>));
			}
		}
		else if (targetCount < Count)
		{
			Delete(targetCount, Count - targetCount);
		}
		return true;
	}
}
