using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Collections.Generic;

/// <summary>Represents a first-in, first-out collection of objects.</summary>
/// <typeparam name="T">Specifies the type of elements in the queue.</typeparam>
/// <filterpriority>1</filterpriority>
[Serializable]
[DebuggerTypeProxy(typeof(QueueDebugView<>))]
[DebuggerDisplay("Count = {Count}")]
public class Queue<T> : IEnumerable<T>, IEnumerable, ICollection, IReadOnlyCollection<T>
{
	/// <summary>Enumerates the elements of a <see cref="T:System.Collections.Generic.Queue`1" />.</summary>
	[Serializable]
	public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
	{
		private readonly Queue<T> _q;

		private readonly int _version;

		private int _index;

		private T _currentElement;

		/// <summary>Gets the element at the current position of the enumerator.</summary>
		/// <returns>The element in the <see cref="T:System.Collections.Generic.Queue`1" /> at the current position of the enumerator.</returns>
		/// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element. </exception>
		public T Current
		{
			get
			{
				if (_index < 0)
				{
					ThrowEnumerationNotStartedOrEnded();
				}
				return _currentElement;
			}
		}

		/// <summary>Gets the element at the current position of the enumerator.</summary>
		/// <returns>The element in the collection at the current position of the enumerator.</returns>
		/// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element. </exception>
		object IEnumerator.Current => Current;

		internal Enumerator(Queue<T> q)
		{
			_q = q;
			_version = q._version;
			_index = -1;
			_currentElement = default(T);
		}

		/// <summary>Releases all resources used by the <see cref="T:System.Collections.Generic.Queue`1.Enumerator" />.</summary>
		public void Dispose()
		{
			_index = -2;
			_currentElement = default(T);
		}

		/// <summary>Advances the enumerator to the next element of the <see cref="T:System.Collections.Generic.Queue`1" />.</summary>
		/// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
		/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
		public bool MoveNext()
		{
			if (_version != _q._version)
			{
				throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}
			if (_index == -2)
			{
				return false;
			}
			_index++;
			if (_index == _q._size)
			{
				_index = -2;
				_currentElement = default(T);
				return false;
			}
			T[] array = _q._array;
			int num = array.Length;
			int num2 = _q._head + _index;
			if (num2 >= num)
			{
				num2 -= num;
			}
			_currentElement = array[num2];
			return true;
		}

		private void ThrowEnumerationNotStartedOrEnded()
		{
			throw new InvalidOperationException((_index == -1) ? "Enumeration has not started. Call MoveNext." : "Enumeration already finished.");
		}

		/// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
		/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
		void IEnumerator.Reset()
		{
			if (_version != _q._version)
			{
				throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}
			_index = -1;
			_currentElement = default(T);
		}
	}

	private T[] _array;

	private int _head;

	private int _tail;

	private int _size;

	private int _version;

	[NonSerialized]
	private object _syncRoot;

	private const int MinimumGrow = 4;

	private const int GrowFactor = 200;

	/// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.Queue`1" />.</summary>
	/// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.Queue`1" />.</returns>
	public int Count => _size;

	/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).</summary>
	/// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false.  In the default implementation of <see cref="T:System.Collections.Generic.Queue`1" />, this property always returns false.</returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.  In the default implementation of <see cref="T:System.Collections.Generic.Queue`1" />, this property always returns the current instance.</returns>
	object ICollection.SyncRoot
	{
		get
		{
			if (_syncRoot == null)
			{
				Interlocked.CompareExchange<object>(ref _syncRoot, new object(), (object)null);
			}
			return _syncRoot;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Queue`1" /> class that is empty and has the default initial capacity.</summary>
	public Queue()
	{
		_array = Array.Empty<T>();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Queue`1" /> class that is empty and has the specified initial capacity.</summary>
	/// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Generic.Queue`1" /> can contain.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than zero.</exception>
	public Queue(int capacity)
	{
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException("capacity", capacity, "Non-negative number required.");
		}
		_array = new T[capacity];
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Queue`1" /> class that contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.</summary>
	/// <param name="collection">The collection whose elements are copied to the new <see cref="T:System.Collections.Generic.Queue`1" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="collection" /> is null.</exception>
	public Queue(IEnumerable<T> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		_array = EnumerableHelpers.ToArray(collection, out _size);
		if (_size != _array.Length)
		{
			_tail = _size;
		}
	}

	/// <summary>Removes all objects from the <see cref="T:System.Collections.Generic.Queue`1" />.</summary>
	/// <filterpriority>1</filterpriority>
	public void Clear()
	{
		if (_size != 0)
		{
			if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
			{
				if (_head < _tail)
				{
					Array.Clear(_array, _head, _size);
				}
				else
				{
					Array.Clear(_array, _head, _array.Length - _head);
					Array.Clear(_array, 0, _tail);
				}
			}
			_size = 0;
		}
		_head = 0;
		_tail = 0;
		_version++;
	}

	/// <summary>Copies the <see cref="T:System.Collections.Generic.Queue`1" /> elements to an existing one-dimensional <see cref="T:System.Array" />, starting at the specified array index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.Queue`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
	/// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="arrayIndex" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.Queue`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.</exception>
	public void CopyTo(T[] array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (arrayIndex < 0 || arrayIndex > array.Length)
		{
			throw new ArgumentOutOfRangeException("arrayIndex", arrayIndex, "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if (array.Length - arrayIndex < _size)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		int size = _size;
		if (size != 0)
		{
			int num = Math.Min(_array.Length - _head, size);
			Array.Copy(_array, _head, array, arrayIndex, num);
			size -= num;
			if (size > 0)
			{
				Array.Copy(_array, 0, array, arrayIndex + _array.Length - _head, size);
			}
		}
	}

	/// <summary>Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional.-or-<paramref name="array" /> does not have zero-based indexing.-or-The number of elements in the source <see cref="T:System.Collections.ICollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.-or-The type of the source <see cref="T:System.Collections.ICollection" /> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>
	void ICollection.CopyTo(Array array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (array.Rank != 1)
		{
			throw new ArgumentException("Only single dimensional arrays are supported for the requested action.", "array");
		}
		if (array.GetLowerBound(0) != 0)
		{
			throw new ArgumentException("The lower bound of target array must be zero.", "array");
		}
		int length = array.Length;
		if (index < 0 || index > length)
		{
			throw new ArgumentOutOfRangeException("index", index, "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if (length - index < _size)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		int size = _size;
		if (size == 0)
		{
			return;
		}
		try
		{
			int num = ((_array.Length - _head < size) ? (_array.Length - _head) : size);
			Array.Copy(_array, _head, array, index, num);
			size -= num;
			if (size > 0)
			{
				Array.Copy(_array, 0, array, index + _array.Length - _head, size);
			}
		}
		catch (ArrayTypeMismatchException)
		{
			throw new ArgumentException("Target array type is not compatible with the type of items in the collection.", "array");
		}
	}

	/// <summary>Adds an object to the end of the <see cref="T:System.Collections.Generic.Queue`1" />.</summary>
	/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.Queue`1" />. The value can be null for reference types.</param>
	public void Enqueue(T item)
	{
		if (_size == _array.Length)
		{
			int num = (int)((long)_array.Length * 200L / 100);
			if (num < _array.Length + 4)
			{
				num = _array.Length + 4;
			}
			SetCapacity(num);
		}
		_array[_tail] = item;
		MoveNext(ref _tail);
		_size++;
		_version++;
	}

	/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Generic.Queue`1" />.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.Queue`1.Enumerator" /> for the <see cref="T:System.Collections.Generic.Queue`1" />.</returns>
	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	/// <summary>Returns an enumerator that iterates through a collection.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return new Enumerator(this);
	}

	/// <summary>Returns an enumerator that iterates through a collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this);
	}

	/// <summary>Removes and returns the object at the beginning of the <see cref="T:System.Collections.Generic.Queue`1" />.</summary>
	/// <returns>The object that is removed from the beginning of the <see cref="T:System.Collections.Generic.Queue`1" />.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Collections.Generic.Queue`1" /> is empty.</exception>
	public T Dequeue()
	{
		if (_size == 0)
		{
			ThrowForEmptyQueue();
		}
		T result = _array[_head];
		if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
		{
			_array[_head] = default(T);
		}
		MoveNext(ref _head);
		_size--;
		_version++;
		return result;
	}

	public bool TryDequeue(out T result)
	{
		if (_size == 0)
		{
			result = default(T);
			return false;
		}
		result = _array[_head];
		if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
		{
			_array[_head] = default(T);
		}
		MoveNext(ref _head);
		_size--;
		_version++;
		return true;
	}

	/// <summary>Returns the object at the beginning of the <see cref="T:System.Collections.Generic.Queue`1" /> without removing it.</summary>
	/// <returns>The object at the beginning of the <see cref="T:System.Collections.Generic.Queue`1" />.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Collections.Generic.Queue`1" /> is empty.</exception>
	public T Peek()
	{
		if (_size == 0)
		{
			ThrowForEmptyQueue();
		}
		return _array[_head];
	}

	public bool TryPeek(out T result)
	{
		if (_size == 0)
		{
			result = default(T);
			return false;
		}
		result = _array[_head];
		return true;
	}

	/// <summary>Determines whether an element is in the <see cref="T:System.Collections.Generic.Queue`1" />.</summary>
	/// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.Queue`1" />; otherwise, false.</returns>
	/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.Queue`1" />. The value can be null for reference types.</param>
	public bool Contains(T item)
	{
		if (_size == 0)
		{
			return false;
		}
		if (_head < _tail)
		{
			return Array.IndexOf(_array, item, _head, _size) >= 0;
		}
		if (Array.IndexOf(_array, item, _head, _array.Length - _head) < 0)
		{
			return Array.IndexOf(_array, item, 0, _tail) >= 0;
		}
		return true;
	}

	/// <summary>Copies the <see cref="T:System.Collections.Generic.Queue`1" /> elements to a new array.</summary>
	/// <returns>A new array containing elements copied from the <see cref="T:System.Collections.Generic.Queue`1" />.</returns>
	public T[] ToArray()
	{
		if (_size == 0)
		{
			return Array.Empty<T>();
		}
		T[] array = new T[_size];
		if (_head < _tail)
		{
			Array.Copy(_array, _head, array, 0, _size);
		}
		else
		{
			Array.Copy(_array, _head, array, 0, _array.Length - _head);
			Array.Copy(_array, 0, array, _array.Length - _head, _tail);
		}
		return array;
	}

	private void SetCapacity(int capacity)
	{
		T[] array = new T[capacity];
		if (_size > 0)
		{
			if (_head < _tail)
			{
				Array.Copy(_array, _head, array, 0, _size);
			}
			else
			{
				Array.Copy(_array, _head, array, 0, _array.Length - _head);
				Array.Copy(_array, 0, array, _array.Length - _head, _tail);
			}
		}
		_array = array;
		_head = 0;
		_tail = ((_size != capacity) ? _size : 0);
		_version++;
	}

	private void MoveNext(ref int index)
	{
		int num = index + 1;
		index = ((num != _array.Length) ? num : 0);
	}

	private void ThrowForEmptyQueue()
	{
		throw new InvalidOperationException("Queue empty.");
	}

	/// <summary>Sets the capacity to the actual number of elements in the <see cref="T:System.Collections.Generic.Queue`1" />, if that number is less than 90 percent of current capacity.</summary>
	public void TrimExcess()
	{
		int num = (int)((double)_array.Length * 0.9);
		if (_size < num)
		{
			SetCapacity(_size);
		}
	}
}
