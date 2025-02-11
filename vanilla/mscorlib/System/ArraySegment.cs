using System.Collections;
using System.Collections.Generic;

namespace System;

/// <summary>Delimits a section of a one-dimensional array.</summary>
/// <typeparam name="T">The type of the elements in the array segment.</typeparam>
/// <filterpriority>2</filterpriority>
[Serializable]
public struct ArraySegment<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>, IReadOnlyCollection<T>
{
	[Serializable]
	private sealed class ArraySegmentEnumerator : IEnumerator<T>, IDisposable, IEnumerator
	{
		private T[] _array;

		private int _start;

		private int _end;

		private int _current;

		public T Current
		{
			get
			{
				if (_current < _start)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Enumeration has not started. Call MoveNext."));
				}
				if (_current >= _end)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Enumeration already finished."));
				}
				return _array[_current];
			}
		}

		object IEnumerator.Current => Current;

		internal ArraySegmentEnumerator(ArraySegment<T> arraySegment)
		{
			_array = arraySegment._array;
			_start = arraySegment._offset;
			_end = _start + arraySegment._count;
			_current = _start - 1;
		}

		public bool MoveNext()
		{
			if (_current < _end)
			{
				_current++;
				return _current < _end;
			}
			return false;
		}

		void IEnumerator.Reset()
		{
			_current = _start - 1;
		}

		public void Dispose()
		{
		}
	}

	private T[] _array;

	private int _offset;

	private int _count;

	/// <summary>Gets the original array containing the range of elements that the array segment delimits.</summary>
	/// <returns>The original array that was passed to the constructor, and that contains the range delimited by the <see cref="T:System.ArraySegment`1" />.</returns>
	/// <filterpriority>1</filterpriority>
	public T[] Array => _array;

	/// <summary>Gets the position of the first element in the range delimited by the array segment, relative to the start of the original array.</summary>
	/// <returns>The position of the first element in the range delimited by the <see cref="T:System.ArraySegment`1" />, relative to the start of the original array.</returns>
	/// <filterpriority>1</filterpriority>
	public int Offset => _offset;

	/// <summary>Gets the number of elements in the range delimited by the array segment.</summary>
	/// <returns>The number of elements in the range delimited by the <see cref="T:System.ArraySegment`1" />.</returns>
	/// <filterpriority>1</filterpriority>
	public int Count => _count;

	/// <summary>Gets or sets the element at the specified index.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the element to get or set.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the <see cref="T:System.ArraySegment`1" />.</exception>
	/// <exception cref="T:System.NotSupportedException">The property is set and the array segment is read-only.</exception>
	T IList<T>.this[int index]
	{
		get
		{
			if (_array == null)
			{
				throw new InvalidOperationException(Environment.GetResourceString("The underlying array is null."));
			}
			if (index < 0 || index >= _count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			return _array[_offset + index];
		}
		set
		{
			if (_array == null)
			{
				throw new InvalidOperationException(Environment.GetResourceString("The underlying array is null."));
			}
			if (index < 0 || index >= _count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			_array[_offset + index] = value;
		}
	}

	/// <summary>Gets the element at the specified index of the array segment.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the element to get.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the <see cref="T:System.ArraySegment`1" />.</exception>
	/// <exception cref="T:System.NotSupportedException">The property is set.</exception>
	T IReadOnlyList<T>.this[int index]
	{
		get
		{
			if (_array == null)
			{
				throw new InvalidOperationException(Environment.GetResourceString("The underlying array is null."));
			}
			if (index < 0 || index >= _count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			return _array[_offset + index];
		}
	}

	/// <summary>Gets a value that indicates whether the array segment  is read-only.</summary>
	/// <returns>true if the array segment is read-only; otherwise, false.</returns>
	bool ICollection<T>.IsReadOnly => true;

	/// <summary>Initializes a new instance of the <see cref="T:System.ArraySegment`1" /> structure that delimits all the elements in the specified array.</summary>
	/// <param name="array">The array to wrap.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	public ArraySegment(T[] array)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		_array = array;
		_offset = 0;
		_count = array.Length;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ArraySegment`1" /> structure that delimits the specified range of the elements in the specified array.</summary>
	/// <param name="array">The array containing the range of elements to delimit.</param>
	/// <param name="offset">The zero-based index of the first element in the range.</param>
	/// <param name="count">The number of elements in the range.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="count" /> is less than 0.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="offset" /> and <paramref name="count" /> do not specify a valid range in <paramref name="array" />.</exception>
	public ArraySegment(T[] array, int offset, int count)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("Non-negative number required."));
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Non-negative number required."));
		}
		if (array.Length - offset < count)
		{
			throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
		}
		_array = array;
		_offset = offset;
		_count = count;
	}

	/// <summary>Returns the hash code for the current instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		if (_array != null)
		{
			return _array.GetHashCode() ^ _offset ^ _count;
		}
		return 0;
	}

	/// <summary>Determines whether the specified object is equal to the current instance.</summary>
	/// <returns>true if the specified object is a <see cref="T:System.ArraySegment`1" /> structure and is equal to the current instance; otherwise, false.</returns>
	/// <param name="obj">The object to be compared with the current instance.</param>
	public override bool Equals(object obj)
	{
		if (obj is ArraySegment<T>)
		{
			return Equals((ArraySegment<T>)obj);
		}
		return false;
	}

	/// <summary>Determines whether the specified <see cref="T:System.ArraySegment`1" /> structure is equal to the current instance.</summary>
	/// <returns>true if the specified <see cref="T:System.ArraySegment`1" /> structure is equal to the current instance; otherwise, false.</returns>
	/// <param name="obj">The <see cref="T:System.ArraySegment`1" /> structure to be compared with the current instance.</param>
	public bool Equals(ArraySegment<T> obj)
	{
		if (obj._array == _array && obj._offset == _offset)
		{
			return obj._count == _count;
		}
		return false;
	}

	/// <summary>Indicates whether two <see cref="T:System.ArraySegment`1" /> structures are equal.</summary>
	/// <returns>true if <paramref name="a" /> is equal to <paramref name="b" />; otherwise, false.</returns>
	/// <param name="a">The <see cref="T:System.ArraySegment`1" /> structure on the left side of the equality operator.</param>
	/// <param name="b">The <see cref="T:System.ArraySegment`1" /> structure on the right side of the equality operator.</param>
	public static bool operator ==(ArraySegment<T> a, ArraySegment<T> b)
	{
		return a.Equals(b);
	}

	/// <summary>Indicates whether two <see cref="T:System.ArraySegment`1" /> structures are unequal.</summary>
	/// <returns>true if <paramref name="a" /> is not equal to <paramref name="b" />; otherwise, false.</returns>
	/// <param name="a">The <see cref="T:System.ArraySegment`1" /> structure on the left side of the inequality operator.</param>
	/// <param name="b">The <see cref="T:System.ArraySegment`1" /> structure on the right side of the inequality operator.</param>
	public static bool operator !=(ArraySegment<T> a, ArraySegment<T> b)
	{
		return !(a == b);
	}

	/// <summary>Determines the index of a specific item in the array segment.</summary>
	/// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
	/// <param name="item">The object to locate in the array segment.</param>
	int IList<T>.IndexOf(T item)
	{
		if (_array == null)
		{
			throw new InvalidOperationException(Environment.GetResourceString("The underlying array is null."));
		}
		int num = System.Array.IndexOf(_array, item, _offset, _count);
		if (num < 0)
		{
			return -1;
		}
		return num - _offset;
	}

	/// <summary>Inserts an item into the array segment at the specified index.</summary>
	/// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
	/// <param name="item">The object to insert into the array segment.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the array segment.</exception>
	/// <exception cref="T:System.NotSupportedException">The array segment is read-only.</exception>
	void IList<T>.Insert(int index, T item)
	{
		throw new NotSupportedException();
	}

	/// <summary>Removes the array segment item at the specified index.</summary>
	/// <param name="index">The zero-based index of the item to remove.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the array segment.</exception>
	/// <exception cref="T:System.NotSupportedException">The array segment is read-only.</exception>
	void IList<T>.RemoveAt(int index)
	{
		throw new NotSupportedException();
	}

	/// <summary>Adds an item to the array segment.</summary>
	/// <param name="item">The object to add to the array segment.</param>
	/// <exception cref="T:System.NotSupportedException">The array segment is read-only.</exception>
	void ICollection<T>.Add(T item)
	{
		throw new NotSupportedException();
	}

	/// <summary>Removes all items from the array segment.</summary>
	/// <exception cref="T:System.NotSupportedException">The array segment is read-only. </exception>
	void ICollection<T>.Clear()
	{
		throw new NotSupportedException();
	}

	/// <summary>Determines whether the array segment contains a specific value.</summary>
	/// <returns>true if <paramref name="item" /> is found in the array segment; otherwise, false.</returns>
	/// <param name="item">The object to locate in the array segment.</param>
	bool ICollection<T>.Contains(T item)
	{
		if (_array == null)
		{
			throw new InvalidOperationException(Environment.GetResourceString("The underlying array is null."));
		}
		return System.Array.IndexOf(_array, item, _offset, _count) >= 0;
	}

	/// <summary>Copies the elements of the array segment to an array, starting at the specified array index.</summary>
	/// <param name="array">The one-dimensional array that is the destination of the elements copied from the array segment. The array must have zero-based indexing.</param>
	/// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="arrayIndex" /> is less than 0.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional.-or-The number of elements in the source array segment is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.-or-Type <paramref name="T" /> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>
	void ICollection<T>.CopyTo(T[] array, int arrayIndex)
	{
		if (_array == null)
		{
			throw new InvalidOperationException(Environment.GetResourceString("The underlying array is null."));
		}
		System.Array.Copy(_array, _offset, array, arrayIndex, _count);
	}

	/// <summary>Removes the first occurrence of a specific object from the array segment.</summary>
	/// <returns>true if <paramref name="item" /> was successfully removed from the array segment; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the array segment.</returns>
	/// <param name="item">The object to remove from the array segment.</param>
	/// <exception cref="T:System.NotSupportedException">The array segment is read-only.</exception>
	bool ICollection<T>.Remove(T item)
	{
		throw new NotSupportedException();
	}

	/// <summary>Returns an enumerator that iterates through the array segment.</summary>
	/// <returns>An enumerator that can be used to iterate through the array segment.</returns>
	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		if (_array == null)
		{
			throw new InvalidOperationException(Environment.GetResourceString("The underlying array is null."));
		}
		return new ArraySegmentEnumerator(this);
	}

	/// <summary>Returns an enumerator that iterates through an array segment.</summary>
	/// <returns>An enumerator that can be used to iterate through the array segment.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		if (_array == null)
		{
			throw new InvalidOperationException(Environment.GetResourceString("The underlying array is null."));
		}
		return new ArraySegmentEnumerator(this);
	}
}
