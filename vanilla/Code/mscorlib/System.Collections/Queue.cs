using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;

namespace System.Collections;

/// <summary>Represents a first-in, first-out collection of objects.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(QueueDebugView))]
public class Queue : ICollection, IEnumerable, ICloneable
{
	[Serializable]
	private class SynchronizedQueue : Queue
	{
		private Queue _q;

		private object root;

		public override bool IsSynchronized => true;

		public override object SyncRoot => root;

		public override int Count
		{
			get
			{
				lock (root)
				{
					return _q.Count;
				}
			}
		}

		internal SynchronizedQueue(Queue q)
		{
			_q = q;
			root = _q.SyncRoot;
		}

		public override void Clear()
		{
			lock (root)
			{
				_q.Clear();
			}
		}

		public override object Clone()
		{
			lock (root)
			{
				return new SynchronizedQueue((Queue)_q.Clone());
			}
		}

		public override bool Contains(object obj)
		{
			lock (root)
			{
				return _q.Contains(obj);
			}
		}

		public override void CopyTo(Array array, int arrayIndex)
		{
			lock (root)
			{
				_q.CopyTo(array, arrayIndex);
			}
		}

		public override void Enqueue(object value)
		{
			lock (root)
			{
				_q.Enqueue(value);
			}
		}

		public override object Dequeue()
		{
			lock (root)
			{
				return _q.Dequeue();
			}
		}

		public override IEnumerator GetEnumerator()
		{
			lock (root)
			{
				return _q.GetEnumerator();
			}
		}

		public override object Peek()
		{
			lock (root)
			{
				return _q.Peek();
			}
		}

		public override object[] ToArray()
		{
			lock (root)
			{
				return _q.ToArray();
			}
		}

		public override void TrimToSize()
		{
			lock (root)
			{
				_q.TrimToSize();
			}
		}
	}

	[Serializable]
	private class QueueEnumerator : IEnumerator, ICloneable
	{
		private Queue _q;

		private int _index;

		private int _version;

		private object currentElement;

		public virtual object Current
		{
			get
			{
				if (currentElement == _q._array)
				{
					if (_index == 0)
					{
						throw new InvalidOperationException(Environment.GetResourceString("Enumeration has not started. Call MoveNext."));
					}
					throw new InvalidOperationException(Environment.GetResourceString("Enumeration already finished."));
				}
				return currentElement;
			}
		}

		internal QueueEnumerator(Queue q)
		{
			_q = q;
			_version = _q._version;
			_index = 0;
			currentElement = _q._array;
			if (_q._size == 0)
			{
				_index = -1;
			}
		}

		public object Clone()
		{
			return MemberwiseClone();
		}

		public virtual bool MoveNext()
		{
			if (_version != _q._version)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Collection was modified; enumeration operation may not execute."));
			}
			if (_index < 0)
			{
				currentElement = _q._array;
				return false;
			}
			currentElement = _q.GetElement(_index);
			_index++;
			if (_index == _q._size)
			{
				_index = -1;
			}
			return true;
		}

		public virtual void Reset()
		{
			if (_version != _q._version)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Collection was modified; enumeration operation may not execute."));
			}
			if (_q._size == 0)
			{
				_index = -1;
			}
			else
			{
				_index = 0;
			}
			currentElement = _q._array;
		}
	}

	internal class QueueDebugView
	{
		private Queue queue;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public object[] Items => queue.ToArray();

		public QueueDebugView(Queue queue)
		{
			if (queue == null)
			{
				throw new ArgumentNullException("queue");
			}
			this.queue = queue;
		}
	}

	private object[] _array;

	private int _head;

	private int _tail;

	private int _size;

	private int _growFactor;

	private int _version;

	[NonSerialized]
	private object _syncRoot;

	private const int _MinimumGrow = 4;

	private const int _ShrinkThreshold = 32;

	/// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Queue" />.</summary>
	/// <returns>The number of elements contained in the <see cref="T:System.Collections.Queue" />.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual int Count => _size;

	/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.Queue" /> is synchronized (thread safe).</summary>
	/// <returns>true if access to the <see cref="T:System.Collections.Queue" /> is synchronized (thread safe); otherwise, false. The default is false.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual bool IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.Queue" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.Queue" />.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual object SyncRoot
	{
		get
		{
			if (_syncRoot == null)
			{
				Interlocked.CompareExchange(ref _syncRoot, new object(), null);
			}
			return _syncRoot;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Queue" /> class that is empty, has the default initial capacity, and uses the default growth factor.</summary>
	public Queue()
		: this(32, 2f)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Queue" /> class that is empty, has the specified initial capacity, and uses the default growth factor.</summary>
	/// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Queue" /> can contain. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than zero. </exception>
	public Queue(int capacity)
		: this(capacity, 2f)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Queue" /> class that is empty, has the specified initial capacity, and uses the specified growth factor.</summary>
	/// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Queue" /> can contain. </param>
	/// <param name="growFactor">The factor by which the capacity of the <see cref="T:System.Collections.Queue" /> is expanded. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than zero.-or- <paramref name="growFactor" /> is less than 1.0 or greater than 10.0. </exception>
	public Queue(int capacity, float growFactor)
	{
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException("capacity", Environment.GetResourceString("Non-negative number required."));
		}
		if (!((double)growFactor >= 1.0) || !((double)growFactor <= 10.0))
		{
			throw new ArgumentOutOfRangeException("growFactor", Environment.GetResourceString("Queue grow factor must be between {0} and {1}.", 1, 10));
		}
		_array = new object[capacity];
		_head = 0;
		_tail = 0;
		_size = 0;
		_growFactor = (int)(growFactor * 100f);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Queue" /> class that contains elements copied from the specified collection, has the same initial capacity as the number of elements copied, and uses the default growth factor.</summary>
	/// <param name="col">The <see cref="T:System.Collections.ICollection" /> to copy elements from. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="col" /> is null. </exception>
	public Queue(ICollection col)
		: this(col?.Count ?? 32)
	{
		if (col == null)
		{
			throw new ArgumentNullException("col");
		}
		IEnumerator enumerator = col.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Enqueue(enumerator.Current);
		}
	}

	/// <summary>Creates a shallow copy of the <see cref="T:System.Collections.Queue" />.</summary>
	/// <returns>A shallow copy of the <see cref="T:System.Collections.Queue" />.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual object Clone()
	{
		Queue queue = new Queue(_size);
		queue._size = _size;
		int size = _size;
		int num = ((_array.Length - _head < size) ? (_array.Length - _head) : size);
		Array.Copy(_array, _head, queue._array, 0, num);
		size -= num;
		if (size > 0)
		{
			Array.Copy(_array, 0, queue._array, _array.Length - _head, size);
		}
		queue._version = _version;
		return queue;
	}

	/// <summary>Removes all objects from the <see cref="T:System.Collections.Queue" />.</summary>
	/// <filterpriority>2</filterpriority>
	public virtual void Clear()
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
		_head = 0;
		_tail = 0;
		_size = 0;
		_version++;
	}

	/// <summary>Copies the <see cref="T:System.Collections.Queue" /> elements to an existing one-dimensional <see cref="T:System.Array" />, starting at the specified array index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Queue" />. The <see cref="T:System.Array" /> must have zero-based indexing. </param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional.-or- The number of elements in the source <see cref="T:System.Collections.Queue" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />. </exception>
	/// <exception cref="T:System.ArrayTypeMismatchException">The type of the source <see cref="T:System.Collections.Queue" /> cannot be cast automatically to the type of the destination <paramref name="array" />. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual void CopyTo(Array array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (array.Rank != 1)
		{
			throw new ArgumentException(Environment.GetResourceString("Only single dimensional arrays are supported for the requested action."));
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (array.Length - index < _size)
		{
			throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
		}
		int size = _size;
		if (size != 0)
		{
			int num = ((_array.Length - _head < size) ? (_array.Length - _head) : size);
			Array.Copy(_array, _head, array, index, num);
			size -= num;
			if (size > 0)
			{
				Array.Copy(_array, 0, array, index + _array.Length - _head, size);
			}
		}
	}

	/// <summary>Adds an object to the end of the <see cref="T:System.Collections.Queue" />.</summary>
	/// <param name="obj">The object to add to the <see cref="T:System.Collections.Queue" />. The value can be null. </param>
	/// <filterpriority>2</filterpriority>
	public virtual void Enqueue(object obj)
	{
		if (_size == _array.Length)
		{
			int num = (int)((long)_array.Length * (long)_growFactor / 100);
			if (num < _array.Length + 4)
			{
				num = _array.Length + 4;
			}
			SetCapacity(num);
		}
		_array[_tail] = obj;
		_tail = (_tail + 1) % _array.Length;
		_size++;
		_version++;
	}

	/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Queue" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> for the <see cref="T:System.Collections.Queue" />.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual IEnumerator GetEnumerator()
	{
		return new QueueEnumerator(this);
	}

	/// <summary>Removes and returns the object at the beginning of the <see cref="T:System.Collections.Queue" />.</summary>
	/// <returns>The object that is removed from the beginning of the <see cref="T:System.Collections.Queue" />.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Collections.Queue" /> is empty. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual object Dequeue()
	{
		if (Count == 0)
		{
			throw new InvalidOperationException(Environment.GetResourceString("Queue empty."));
		}
		object result = _array[_head];
		_array[_head] = null;
		_head = (_head + 1) % _array.Length;
		_size--;
		_version++;
		return result;
	}

	/// <summary>Returns the object at the beginning of the <see cref="T:System.Collections.Queue" /> without removing it.</summary>
	/// <returns>The object at the beginning of the <see cref="T:System.Collections.Queue" />.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Collections.Queue" /> is empty. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual object Peek()
	{
		if (Count == 0)
		{
			throw new InvalidOperationException(Environment.GetResourceString("Queue empty."));
		}
		return _array[_head];
	}

	/// <summary>Returns a <see cref="T:System.Collections.Queue" /> wrapper that is synchronized (thread safe).</summary>
	/// <returns>A <see cref="T:System.Collections.Queue" /> wrapper that is synchronized (thread safe).</returns>
	/// <param name="queue">The <see cref="T:System.Collections.Queue" /> to synchronize. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="queue" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
	public static Queue Synchronized(Queue queue)
	{
		if (queue == null)
		{
			throw new ArgumentNullException("queue");
		}
		return new SynchronizedQueue(queue);
	}

	/// <summary>Determines whether an element is in the <see cref="T:System.Collections.Queue" />.</summary>
	/// <returns>true if <paramref name="obj" /> is found in the <see cref="T:System.Collections.Queue" />; otherwise, false.</returns>
	/// <param name="obj">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Collections.Queue" />. The value can be null. </param>
	/// <filterpriority>2</filterpriority>
	public virtual bool Contains(object obj)
	{
		int num = _head;
		int size = _size;
		while (size-- > 0)
		{
			if (obj == null)
			{
				if (_array[num] == null)
				{
					return true;
				}
			}
			else if (_array[num] != null && _array[num].Equals(obj))
			{
				return true;
			}
			num = (num + 1) % _array.Length;
		}
		return false;
	}

	internal object GetElement(int i)
	{
		return _array[(_head + i) % _array.Length];
	}

	/// <summary>Copies the <see cref="T:System.Collections.Queue" /> elements to a new array.</summary>
	/// <returns>A new array containing elements copied from the <see cref="T:System.Collections.Queue" />.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual object[] ToArray()
	{
		object[] array = new object[_size];
		if (_size == 0)
		{
			return array;
		}
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
		object[] array = new object[capacity];
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

	/// <summary>Sets the capacity to the actual number of elements in the <see cref="T:System.Collections.Queue" />.</summary>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Queue" /> is read-only.</exception>
	/// <filterpriority>2</filterpriority>
	public virtual void TrimToSize()
	{
		SetCapacity(_size);
	}
}
