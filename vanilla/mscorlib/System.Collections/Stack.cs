using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;

namespace System.Collections;

/// <summary>Represents a simple last-in-first-out (LIFO) non-generic collection of objects.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[DebuggerTypeProxy(typeof(StackDebugView))]
[ComVisible(true)]
[DebuggerDisplay("Count = {Count}")]
public class Stack : ICollection, IEnumerable, ICloneable
{
	[Serializable]
	private class SyncStack : Stack
	{
		private Stack _s;

		private object _root;

		public override bool IsSynchronized => true;

		public override object SyncRoot => _root;

		public override int Count
		{
			get
			{
				lock (_root)
				{
					return _s.Count;
				}
			}
		}

		internal SyncStack(Stack stack)
		{
			_s = stack;
			_root = stack.SyncRoot;
		}

		public override bool Contains(object obj)
		{
			lock (_root)
			{
				return _s.Contains(obj);
			}
		}

		public override object Clone()
		{
			lock (_root)
			{
				return new SyncStack((Stack)_s.Clone());
			}
		}

		public override void Clear()
		{
			lock (_root)
			{
				_s.Clear();
			}
		}

		public override void CopyTo(Array array, int arrayIndex)
		{
			lock (_root)
			{
				_s.CopyTo(array, arrayIndex);
			}
		}

		public override void Push(object value)
		{
			lock (_root)
			{
				_s.Push(value);
			}
		}

		public override object Pop()
		{
			lock (_root)
			{
				return _s.Pop();
			}
		}

		public override IEnumerator GetEnumerator()
		{
			lock (_root)
			{
				return _s.GetEnumerator();
			}
		}

		public override object Peek()
		{
			lock (_root)
			{
				return _s.Peek();
			}
		}

		public override object[] ToArray()
		{
			lock (_root)
			{
				return _s.ToArray();
			}
		}
	}

	[Serializable]
	private class StackEnumerator : IEnumerator, ICloneable
	{
		private Stack _stack;

		private int _index;

		private int _version;

		private object currentElement;

		public virtual object Current
		{
			get
			{
				if (_index == -2)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Enumeration has not started. Call MoveNext."));
				}
				if (_index == -1)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Enumeration already finished."));
				}
				return currentElement;
			}
		}

		internal StackEnumerator(Stack stack)
		{
			_stack = stack;
			_version = _stack._version;
			_index = -2;
			currentElement = null;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}

		public virtual bool MoveNext()
		{
			if (_version != _stack._version)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Collection was modified; enumeration operation may not execute."));
			}
			if (_index == -2)
			{
				_index = _stack._size - 1;
				bool num = _index >= 0;
				if (num)
				{
					currentElement = _stack._array[_index];
				}
				return num;
			}
			if (_index == -1)
			{
				return false;
			}
			bool num2 = --_index >= 0;
			if (num2)
			{
				currentElement = _stack._array[_index];
				return num2;
			}
			currentElement = null;
			return num2;
		}

		public virtual void Reset()
		{
			if (_version != _stack._version)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Collection was modified; enumeration operation may not execute."));
			}
			_index = -2;
			currentElement = null;
		}
	}

	internal class StackDebugView
	{
		private Stack stack;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public object[] Items => stack.ToArray();

		public StackDebugView(Stack stack)
		{
			if (stack == null)
			{
				throw new ArgumentNullException("stack");
			}
			this.stack = stack;
		}
	}

	private object[] _array;

	private int _size;

	private int _version;

	[NonSerialized]
	private object _syncRoot;

	private const int _defaultCapacity = 10;

	/// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Stack" />.</summary>
	/// <returns>The number of elements contained in the <see cref="T:System.Collections.Stack" />.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual int Count => _size;

	/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.Stack" /> is synchronized (thread safe).</summary>
	/// <returns>true, if access to the <see cref="T:System.Collections.Stack" /> is synchronized (thread safe); otherwise, false. The default is false.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual bool IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.Stack" />.</summary>
	/// <returns>An <see cref="T:System.Object" /> that can be used to synchronize access to the <see cref="T:System.Collections.Stack" />.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual object SyncRoot
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Stack" /> class that is empty and has the default initial capacity.</summary>
	public Stack()
	{
		_array = new object[10];
		_size = 0;
		_version = 0;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Stack" /> class that is empty and has the specified initial capacity or the default initial capacity, whichever is greater.</summary>
	/// <param name="initialCapacity">The initial number of elements that the <see cref="T:System.Collections.Stack" /> can contain. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="initialCapacity" /> is less than zero. </exception>
	public Stack(int initialCapacity)
	{
		if (initialCapacity < 0)
		{
			throw new ArgumentOutOfRangeException("initialCapacity", Environment.GetResourceString("Non-negative number required."));
		}
		if (initialCapacity < 10)
		{
			initialCapacity = 10;
		}
		_array = new object[initialCapacity];
		_size = 0;
		_version = 0;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Stack" /> class that contains elements copied from the specified collection and has the same initial capacity as the number of elements copied.</summary>
	/// <param name="col">The <see cref="T:System.Collections.ICollection" /> to copy elements from. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="col" /> is null. </exception>
	public Stack(ICollection col)
		: this(col?.Count ?? 32)
	{
		if (col == null)
		{
			throw new ArgumentNullException("col");
		}
		IEnumerator enumerator = col.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Push(enumerator.Current);
		}
	}

	/// <summary>Removes all objects from the <see cref="T:System.Collections.Stack" />.</summary>
	/// <filterpriority>2</filterpriority>
	public virtual void Clear()
	{
		Array.Clear(_array, 0, _size);
		_size = 0;
		_version++;
	}

	/// <summary>Creates a shallow copy of the <see cref="T:System.Collections.Stack" />.</summary>
	/// <returns>A shallow copy of the <see cref="T:System.Collections.Stack" />.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual object Clone()
	{
		Stack stack = new Stack(_size);
		stack._size = _size;
		Array.Copy(_array, 0, stack._array, 0, _size);
		stack._version = _version;
		return stack;
	}

	/// <summary>Determines whether an element is in the <see cref="T:System.Collections.Stack" />.</summary>
	/// <returns>true, if <paramref name="obj" /> is found in the <see cref="T:System.Collections.Stack" />; otherwise, false.</returns>
	/// <param name="obj">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Collections.Stack" />. The value can be null. </param>
	/// <filterpriority>2</filterpriority>
	public virtual bool Contains(object obj)
	{
		int size = _size;
		while (size-- > 0)
		{
			if (obj == null)
			{
				if (_array[size] == null)
				{
					return true;
				}
			}
			else if (_array[size] != null && _array[size].Equals(obj))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Copies the <see cref="T:System.Collections.Stack" /> to an existing one-dimensional <see cref="T:System.Array" />, starting at the specified array index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Stack" />. The <see cref="T:System.Array" /> must have zero-based indexing. </param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional.-or- The number of elements in the source <see cref="T:System.Collections.Stack" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />. </exception>
	/// <exception cref="T:System.InvalidCastException">The type of the source <see cref="T:System.Collections.Stack" /> cannot be cast automatically to the type of the destination <paramref name="array" />. </exception>
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
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Non-negative number required."));
		}
		if (array.Length - index < _size)
		{
			throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
		}
		int i = 0;
		if (array is object[])
		{
			object[] array2 = (object[])array;
			for (; i < _size; i++)
			{
				array2[i + index] = _array[_size - i - 1];
			}
		}
		else
		{
			for (; i < _size; i++)
			{
				array.SetValue(_array[_size - i - 1], i + index);
			}
		}
	}

	/// <summary>Returns an <see cref="T:System.Collections.IEnumerator" /> for the <see cref="T:System.Collections.Stack" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> for the <see cref="T:System.Collections.Stack" />.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual IEnumerator GetEnumerator()
	{
		return new StackEnumerator(this);
	}

	/// <summary>Returns the object at the top of the <see cref="T:System.Collections.Stack" /> without removing it.</summary>
	/// <returns>The <see cref="T:System.Object" /> at the top of the <see cref="T:System.Collections.Stack" />.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Collections.Stack" /> is empty. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual object Peek()
	{
		if (_size == 0)
		{
			throw new InvalidOperationException(Environment.GetResourceString("Stack empty."));
		}
		return _array[_size - 1];
	}

	/// <summary>Removes and returns the object at the top of the <see cref="T:System.Collections.Stack" />.</summary>
	/// <returns>The <see cref="T:System.Object" /> removed from the top of the <see cref="T:System.Collections.Stack" />.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Collections.Stack" /> is empty. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual object Pop()
	{
		if (_size == 0)
		{
			throw new InvalidOperationException(Environment.GetResourceString("Stack empty."));
		}
		_version++;
		object result = _array[--_size];
		_array[_size] = null;
		return result;
	}

	/// <summary>Inserts an object at the top of the <see cref="T:System.Collections.Stack" />.</summary>
	/// <param name="obj">The <see cref="T:System.Object" /> to push onto the <see cref="T:System.Collections.Stack" />. The value can be null. </param>
	/// <filterpriority>2</filterpriority>
	public virtual void Push(object obj)
	{
		if (_size == _array.Length)
		{
			object[] array = new object[2 * _array.Length];
			Array.Copy(_array, 0, array, 0, _size);
			_array = array;
		}
		_array[_size++] = obj;
		_version++;
	}

	/// <summary>Returns a synchronized (thread safe) wrapper for the <see cref="T:System.Collections.Stack" />.</summary>
	/// <returns>A synchronized wrapper around the <see cref="T:System.Collections.Stack" />.</returns>
	/// <param name="stack">The <see cref="T:System.Collections.Stack" /> to synchronize. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stack" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
	public static Stack Synchronized(Stack stack)
	{
		if (stack == null)
		{
			throw new ArgumentNullException("stack");
		}
		return new SyncStack(stack);
	}

	/// <summary>Copies the <see cref="T:System.Collections.Stack" /> to a new array.</summary>
	/// <returns>A new array containing copies of the elements of the <see cref="T:System.Collections.Stack" />.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual object[] ToArray()
	{
		object[] array = new object[_size];
		for (int i = 0; i < _size; i++)
		{
			array[i] = _array[_size - i - 1];
		}
		return array;
	}
}
