using System.Collections;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Represents an ordered collection of <see cref="T:System.Windows.Media.Visual" /> objects. </summary>
public sealed class VisualCollection : ICollection, IEnumerable
{
	/// <summary>Enumerates <see cref="T:System.Windows.Media.Visual" /> items in a <see cref="T:System.Windows.Media.VisualCollection" />.</summary>
	public struct Enumerator : IEnumerator
	{
		private VisualCollection _collection;

		private int _index;

		private uint _version;

		private Visual _currentElement;

		/// <summary>For a description of this members, see <see cref="P:System.Collections.IEnumerator.Current" />.</summary>
		/// <returns>The current element in the collection.</returns>
		object IEnumerator.Current => Current;

		/// <summary>Gets the current element in the collection.</summary>
		/// <returns>The current element in the collection.</returns>
		public Visual Current
		{
			get
			{
				if (_index < 0)
				{
					if (_index == -1)
					{
						throw new InvalidOperationException(SR.Enumerator_NotStarted);
					}
					throw new InvalidOperationException(SR.Enumerator_ReachedEnd);
				}
				return _currentElement;
			}
		}

		internal Enumerator(VisualCollection collection)
		{
			_collection = collection;
			_index = -1;
			_version = _collection.Version;
			_currentElement = null;
		}

		/// <summary>Advances the enumerator to the next element in the collection.</summary>
		/// <returns>true if the enumerator successfully advanced to the next element; otherwise, false.</returns>
		public bool MoveNext()
		{
			_collection.VerifyAPIReadOnly();
			if (_version == _collection.Version)
			{
				if (_index > -2 && _index < _collection.InternalCount - 1)
				{
					_index++;
					_currentElement = _collection[_index];
					return true;
				}
				_currentElement = null;
				_index = -2;
				return false;
			}
			throw new InvalidOperationException(SR.Enumerator_CollectionChanged);
		}

		/// <summary>Resets the enumerator to its initial position, which is before the first element in the collection.</summary>
		public void Reset()
		{
			_collection.VerifyAPIReadOnly();
			if (_version != _collection.Version)
			{
				throw new InvalidOperationException(SR.Enumerator_CollectionChanged);
			}
			_index = -1;
		}
	}

	private Visual[] _items;

	private int _size;

	private Visual _owner;

	private uint _data;

	private const int c_defaultCapacity = 4;

	private const float c_growFactor = 1.5f;

	internal int InternalCount => _size;

	internal Visual[] InternalArray => _items;

	/// <summary>Gets the number of elements in the collection.</summary>
	/// <returns>The number of elements that the <see cref="T:System.Windows.Media.VisualCollection" /> contains.</returns>
	public int Count
	{
		get
		{
			VerifyAPIReadOnly();
			return InternalCount;
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Media.VisualCollection" /> is read-only.</summary>
	/// <returns>The value that indicates whether the <see cref="T:System.Windows.Media.VisualCollection" /> is read-only.</returns>
	public bool IsReadOnly
	{
		get
		{
			VerifyAPIReadOnly();
			return IsReadOnlyInternal;
		}
	}

	/// <summary>Gets a value that indicates whether access to the <see cref="T:System.Windows.Media.VisualCollection" /> is synchronized (thread-safe). </summary>
	/// <returns>The value that indicates whether the <see cref="T:System.Windows.Media.VisualCollection" /> is synchronized (thread-safe).</returns>
	public bool IsSynchronized
	{
		get
		{
			VerifyAPIReadOnly();
			return false;
		}
	}

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Windows.Media.VisualCollection" />.</summary>
	/// <returns>A value of type <see cref="T:System.Object" />.</returns>
	public object SyncRoot
	{
		get
		{
			VerifyAPIReadOnly();
			return this;
		}
	}

	internal int InternalCapacity
	{
		get
		{
			if (_items == null)
			{
				return 0;
			}
			return _items.Length;
		}
		set
		{
			int num = ((_items != null) ? _items.Length : 0);
			if (value == num)
			{
				return;
			}
			if (value < _size)
			{
				throw new ArgumentOutOfRangeException("value", SR.VisualCollection_NotEnoughCapacity);
			}
			if (value > 0)
			{
				Visual[] array = new Visual[value];
				if (_size > 0)
				{
					Array.Copy(_items, 0, array, 0, _size);
				}
				_items = array;
			}
			else
			{
				_items = null;
			}
		}
	}

	/// <summary>Gets or sets the number of elements that the <see cref="T:System.Windows.Media.VisualCollection" /> can contain.</summary>
	/// <returns>The number of elements that the <see cref="T:System.Windows.Media.VisualCollection" /> can contain.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <see cref="P:System.Windows.Media.VisualCollection.Capacity" /> is set to a value that is less than <see cref="P:System.Windows.Media.VisualCollection.Count" />.</exception>
	public int Capacity
	{
		get
		{
			VerifyAPIReadOnly();
			return InternalCapacity;
		}
		set
		{
			VerifyAPIReadWrite();
			InternalCapacity = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Visual" /> that is stored at the zero-based index of the <see cref="T:System.Windows.Media.VisualCollection" />. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Visual" /> that is stored at <paramref name="index" />.</returns>
	/// <param name="index">The zero-based index of the <see cref="T:System.Windows.Media.VisualCollection" /> from which to get or set the <see cref="T:System.Windows.Media.Visual" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero, or <paramref name="index" /> is equal to or greater than <see cref="P:System.Windows.Media.VisualCollection.Count" />.</exception>
	/// <exception cref="T:System.ArgumentException">The new child element already has a parent, or the value at the specified index is not null.</exception>
	public Visual this[int index]
	{
		get
		{
			if (index < 0 || index >= _size)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			return _items[index];
		}
		set
		{
			VerifyAPIReadWrite(value);
			if (index < 0 || index >= _size)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			Visual visual = _items[index];
			if (value == null && visual != null)
			{
				DisconnectChild(index);
			}
			else if (value != null)
			{
				if (visual != null)
				{
					throw new ArgumentException(SR.VisualCollection_EntryInUse);
				}
				if (value._parent != null || value.IsRootElement)
				{
					throw new ArgumentException(SR.VisualCollection_VisualHasParent);
				}
				ConnectChild(index, value);
			}
		}
	}

	private uint Version => _data >> 1;

	private bool IsReadOnlyInternal => (_data & 1) == 1;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.VisualCollection" /> class.</summary>
	/// <param name="parent">The parent visual object whose <see cref="T:System.Windows.Media.VisualCollection" /> is returned.</param>
	public VisualCollection(Visual parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		_owner = parent;
	}

	internal void VerifyAPIReadOnly()
	{
		_owner.VerifyAPIReadOnly();
	}

	internal void VerifyAPIReadOnly(Visual other)
	{
		_owner.VerifyAPIReadOnly(other);
	}

	internal void VerifyAPIReadWrite()
	{
		_owner.VerifyAPIReadWrite();
		VerifyNotReadOnly();
	}

	internal void VerifyAPIReadWrite(Visual other)
	{
		_owner.VerifyAPIReadWrite(other);
		VerifyNotReadOnly();
	}

	internal void VerifyNotReadOnly()
	{
		if (IsReadOnlyInternal)
		{
			throw new InvalidOperationException(SR.VisualCollection_ReadOnly);
		}
	}

	/// <summary>Copies the items in the collection to an array, starting at a specific array index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements that are copied from the <see cref="T:System.Windows.Media.VisualCollection" />.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	public void CopyTo(Array array, int index)
	{
		VerifyAPIReadOnly();
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (array.Rank != 1)
		{
			throw new ArgumentException(SR.Collection_BadRank);
		}
		if (index < 0 || array.Length - index < _size)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		for (int i = 0; i < _size; i++)
		{
			array.SetValue(_items[i], i + index);
		}
	}

	/// <summary>Copies the current collection into the passed <see cref="T:System.Windows.Media.Visual" /> array.</summary>
	/// <param name="array">An array of <see cref="T:System.Windows.Media.Visual" /> objects (which must have zero-based indexing).</param>
	/// <param name="index">The index to start copying from within <paramref name="array" />.</param>
	public void CopyTo(Visual[] array, int index)
	{
		VerifyAPIReadOnly();
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0 || array.Length - index < _size)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		for (int i = 0; i < _size; i++)
		{
			array[i + index] = _items[i];
		}
	}

	private void EnsureCapacity(int min)
	{
		if (InternalCapacity < min)
		{
			InternalCapacity = Math.Max(min, (int)((float)InternalCapacity * 1.5f));
		}
	}

	private void ConnectChild(int index, Visual value)
	{
		_owner.VerifyAccess();
		value.VerifyAccess();
		if (_owner.IsVisualChildrenIterationInProgress)
		{
			throw new InvalidOperationException(SR.CannotModifyVisualChildrenDuringTreeWalk);
		}
		value._parentIndex = index;
		_items[index] = value;
		IncrementVersion();
		_owner.InternalAddVisualChild(value);
	}

	private void DisconnectChild(int index)
	{
		Visual visual = _items[index];
		visual.VerifyAccess();
		Visual containingVisual2D = VisualTreeHelper.GetContainingVisual2D(visual._parent);
		_ = visual._parentIndex;
		if (containingVisual2D.IsVisualChildrenIterationInProgress)
		{
			throw new InvalidOperationException(SR.CannotModifyVisualChildrenDuringTreeWalk);
		}
		_items[index] = null;
		IncrementVersion();
		_owner.InternalRemoveVisualChild(visual);
	}

	/// <summary>Appends a <see cref="T:System.Windows.Media.Visual" /> to the end of the <see cref="T:System.Windows.Media.VisualCollection" />.</summary>
	/// <returns>The index in the collection at which <paramref name="visual" /> was added.</returns>
	/// <param name="visual">The <see cref="T:System.Windows.Media.Visual" /> to append to the <see cref="T:System.Windows.Media.VisualCollection" />.</param>
	/// <exception cref="T:System.ArgumentException">An <see cref="T:System.ArgumentException" /> is thrown if the <see cref="T:System.Windows.Media.Visual" /> is a root element.</exception>
	public int Add(Visual visual)
	{
		VerifyAPIReadWrite(visual);
		if (visual != null && (visual._parent != null || visual.IsRootElement))
		{
			throw new ArgumentException(SR.VisualCollection_VisualHasParent);
		}
		if (_items == null || _size == _items.Length)
		{
			EnsureCapacity(_size + 1);
		}
		int num = _size++;
		if (visual != null)
		{
			ConnectChild(num, visual);
		}
		IncrementVersion();
		return num;
	}

	/// <summary>Returns the zero-based index of the <see cref="T:System.Windows.Media.Visual" />.</summary>
	/// <returns>The index of the <see cref="T:System.Windows.Media.Visual" />.</returns>
	/// <param name="visual">The <see cref="T:System.Windows.Media.Visual" /> to locate in the <see cref="T:System.Windows.Media.VisualCollection" />.</param>
	public int IndexOf(Visual visual)
	{
		VerifyAPIReadOnly();
		if (visual == null)
		{
			for (int i = 0; i < _size; i++)
			{
				if (_items[i] == null)
				{
					return i;
				}
			}
			return -1;
		}
		if (visual._parent != _owner)
		{
			return -1;
		}
		return visual._parentIndex;
	}

	/// <summary>Removes the specified <see cref="T:System.Windows.Media.Visual" /> object from the <see cref="T:System.Windows.Media.VisualCollection" />.</summary>
	/// <param name="visual">The <see cref="T:System.Windows.Media.Visual" /> to remove from the <see cref="T:System.Windows.Media.VisualCollection" />.</param>
	public void Remove(Visual visual)
	{
		VerifyAPIReadWrite(visual);
		InternalRemove(visual);
	}

	private void InternalRemove(Visual visual)
	{
		int num = -1;
		if (visual != null)
		{
			if (visual._parent != _owner)
			{
				return;
			}
			num = visual._parentIndex;
			DisconnectChild(num);
		}
		else
		{
			for (int i = 0; i < _size; i++)
			{
				if (_items[i] == null)
				{
					num = i;
					break;
				}
			}
		}
		if (num == -1)
		{
			return;
		}
		_size--;
		for (int j = num; j < _size; j++)
		{
			Visual visual2 = _items[j + 1];
			if (visual2 != null)
			{
				visual2._parentIndex = j;
			}
			_items[j] = visual2;
		}
		_items[_size] = null;
	}

	private void IncrementVersion()
	{
		_data += 2u;
	}

	internal void SetReadOnly()
	{
		_data |= 1u;
	}

	/// <summary>Returns a <see cref="T:System.Boolean" /> value that indicates whether the specified <see cref="T:System.Windows.Media.Visual" /> is contained in the collection.</summary>
	/// <returns>true if <paramref name="visual" /> is contained in the collection; otherwise, false.</returns>
	/// <param name="visual">The <see cref="T:System.Windows.Media.Visual" /> to search for in the collection.</param>
	public bool Contains(Visual visual)
	{
		VerifyAPIReadOnly(visual);
		if (visual == null)
		{
			for (int i = 0; i < _size; i++)
			{
				if (_items[i] == null)
				{
					return true;
				}
			}
			return false;
		}
		return visual._parent == _owner;
	}

	/// <summary>Removes all elements from the <see cref="T:System.Windows.Media.VisualCollection" />.</summary>
	public void Clear()
	{
		VerifyAPIReadWrite();
		for (int i = 0; i < _size; i++)
		{
			if (_items[i] != null)
			{
				DisconnectChild(i);
			}
			_items[i] = null;
		}
		_size = 0;
		IncrementVersion();
	}

	/// <summary>Inserts an element into the <see cref="T:System.Windows.Media.VisualCollection" /> at the specified index.</summary>
	/// <param name="index">The zero-based index at which the value should be inserted.</param>
	/// <param name="visual">The <see cref="T:System.Windows.Media.Visual" /> to insert into the <see cref="T:System.Windows.Media.VisualCollection" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than <see cref="P:System.Windows.Media.VisualCollection.Count" />.</exception>
	public void Insert(int index, Visual visual)
	{
		VerifyAPIReadWrite(visual);
		if (index < 0 || index > _size)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (visual != null && (visual._parent != null || visual.IsRootElement))
		{
			throw new ArgumentException(SR.VisualCollection_VisualHasParent);
		}
		if (_items == null || _size == _items.Length)
		{
			EnsureCapacity(_size + 1);
		}
		for (int num = _size - 1; num >= index; num--)
		{
			Visual visual2 = _items[num];
			if (visual2 != null)
			{
				visual2._parentIndex = num + 1;
			}
			_items[num + 1] = visual2;
		}
		_items[index] = null;
		_size++;
		if (visual != null)
		{
			ConnectChild(index, visual);
		}
	}

	/// <summary>Removes the visual object at the specified index in the <see cref="T:System.Windows.Media.VisualCollection" />.</summary>
	/// <param name="index">The zero-based index of the visual to remove.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero, or <paramref name="index" /> is equal to or greater than <see cref="P:System.Windows.Media.VisualCollection.Count" />.</exception>
	public void RemoveAt(int index)
	{
		VerifyAPIReadWrite();
		if (index < 0 || index >= _size)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		InternalRemove(_items[index]);
	}

	/// <summary>Removes a range of visual objects from the <see cref="T:System.Windows.Media.VisualCollection" />.</summary>
	/// <param name="index">The zero-based index of the range of elements to remove.</param>
	/// <param name="count">The number of elements to remove.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or <paramref name="count" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> and <paramref name="count" /> do not refer to a valid range of elements in the <see cref="T:System.Windows.Media.VisualCollection" />.</exception>
	public void RemoveRange(int index, int count)
	{
		VerifyAPIReadWrite();
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (_size - index < count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (count <= 0)
		{
			return;
		}
		for (int i = index; i < index + count; i++)
		{
			if (_items[i] != null)
			{
				DisconnectChild(i);
				_items[i] = null;
			}
		}
		_size -= count;
		for (int j = index; j < _size; j++)
		{
			Visual visual = _items[j + count];
			if (visual != null)
			{
				visual._parentIndex = j;
			}
			_items[j] = visual;
			_items[j + count] = null;
		}
		IncrementVersion();
	}

	internal void Move(Visual visual, Visual destination)
	{
		Invariant.Assert(visual != null, "we don't support moving a null visual");
		if (visual._parent != _owner)
		{
			return;
		}
		int parentIndex = visual._parentIndex;
		int num = destination?._parentIndex ?? _size;
		if (parentIndex == num)
		{
			return;
		}
		if (parentIndex < num)
		{
			num--;
			for (int i = parentIndex; i < num; i++)
			{
				Visual visual2 = _items[i + 1];
				if (visual2 != null)
				{
					visual2._parentIndex = i;
				}
				_items[i] = visual2;
			}
		}
		else
		{
			for (int num2 = parentIndex; num2 > num; num2--)
			{
				Visual visual3 = _items[num2 - 1];
				if (visual3 != null)
				{
					visual3._parentIndex = num2;
				}
				_items[num2] = visual3;
			}
		}
		visual._parentIndex = num;
		_items[num] = visual;
	}

	/// <summary>This member supports the WPF infrastructure and is not intended to be used directly from your code. For a description of this member, see <see cref="M:System.Collections.IEnumerable.GetEnumerator" />.</summary>
	/// <returns>An object that can be used to iterate through the collection. </returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>Retrieves an enumerator that can iterate through the <see cref="T:System.Windows.Media.VisualCollection" />.</summary>
	/// <returns>An enumerator that can be used to iterate through the collection.</returns>
	public Enumerator GetEnumerator()
	{
		VerifyAPIReadOnly();
		return new Enumerator(this);
	}
}
