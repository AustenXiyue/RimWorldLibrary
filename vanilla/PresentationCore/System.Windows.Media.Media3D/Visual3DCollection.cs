using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Utility;

namespace System.Windows.Media.Media3D;

/// <summary>Represents an ordered collection of <see cref="T:System.Windows.Media.Media3D.Visual3D" /> objects.</summary>
public sealed class Visual3DCollection : IList, ICollection, IEnumerable, IList<Visual3D>, ICollection<Visual3D>, IEnumerable<Visual3D>
{
	/// <summary>Enumerates <see cref="T:System.Windows.Media.Media3D.Visual3D" /> items in a <see cref="T:System.Windows.Media.Media3D.Visual3DCollection" />.</summary>
	public struct Enumerator : IEnumerator<Visual3D>, IEnumerator, IDisposable
	{
		private Visual3DCollection _list;

		private int _index;

		private int _version;

		/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
		object IEnumerator.Current => Current;

		/// <summary>Gets the current element in the collection.</summary>
		/// <returns>The current element in the collection.</returns>
		public Visual3D Current
		{
			get
			{
				if (_index < 0 || _index >= _list.Count)
				{
					throw new InvalidOperationException(SR.Enumerator_VerifyContext);
				}
				return _list[_index];
			}
		}

		internal Enumerator(Visual3DCollection list)
		{
			_list = list;
			_index = -1;
			_version = _list._version;
		}

		/// <summary>Advances the enumerator to the next element in the collection.</summary>
		/// <returns>true if the enumerator successfully advanced to the next element; otherwise, false.</returns>
		public bool MoveNext()
		{
			if (_list._version != _version)
			{
				throw new InvalidOperationException(SR.Enumerator_CollectionChanged);
			}
			int count = _list.Count;
			if (_index < count)
			{
				_index++;
			}
			return _index < count;
		}

		/// <summary>Resets the enumerator to its initial position, which is before the first element in the collection.</summary>
		public void Reset()
		{
			if (_list._version != _version)
			{
				throw new InvalidOperationException(SR.Enumerator_CollectionChanged);
			}
			_index = -1;
		}

		/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
		void IDisposable.Dispose()
		{
		}
	}

	private IVisual3DContainer _owner;

	private FrugalStructList<Visual3D> _collection;

	private int _version;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Media3D.Visual3D" /> at the specified zero-based index.</summary>
	/// <returns>The Visual3D at the specified index.</returns>
	/// <param name="index">The zero-based index of the Visual3D to get or set.</param>
	public Visual3D this[int index]
	{
		get
		{
			VerifyAPIReadOnly();
			return InternalGetItem(index);
		}
		set
		{
			if (index < 0 || index >= InternalCount)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			VerifyAPIForAdd(value);
			InternalRemoveAt(index);
			InternalInsert(index, value);
		}
	}

	/// <summary>Gets the number of items contained in a <see cref="T:System.Windows.Media.Media3D.Visual3DCollection" />.</summary>
	/// <returns>The number of items contained in the Visual3Dcollection.</returns>
	public int Count
	{
		get
		{
			VerifyAPIReadOnly();
			return InternalCount;
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.IsSynchronized" />.</summary>
	/// <returns>true if access to the <see cref="T:System.Windows.Media.Media3D.Visual3DCollection" /> is synchronized (thread safe); otherwise, false.</returns>
	bool ICollection.IsSynchronized
	{
		get
		{
			VerifyAPIReadOnly();
			return true;
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.SyncRoot" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Windows.Media.Media3D.Visual3DCollection" />.</returns>
	object ICollection.SyncRoot
	{
		get
		{
			VerifyAPIReadOnly();
			return _owner;
		}
	}

	bool ICollection<Visual3D>.IsReadOnly
	{
		get
		{
			VerifyAPIReadOnly();
			return false;
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsFixedSize" />.</summary>
	/// <returns>true if the the <see cref="T:System.Windows.Media.Media3D.Visual3DCollection" /> has a fixed size; otherwise, false.</returns>
	bool IList.IsFixedSize => false;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsReadOnly" />.</summary>
	/// <returns>true if the the <see cref="T:System.Windows.Media.Media3D.Visual3DCollection" /> is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => false;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.Item(System.Int32)" />.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the element to get or set. </param>
	object IList.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			this[index] = Cast(value);
		}
	}

	internal int InternalCount => _collection.Count;

	internal Visual3DCollection(IVisual3DContainer owner)
	{
		_owner = owner;
	}

	/// <summary>Adds a <see cref="T:System.Windows.Media.Media3D.Visual3D" /> object to the end of this <see cref="T:System.Windows.Media.Media3D.Visual3DCollection" />.</summary>
	/// <param name="value">The Visual3D to add to this Visual3DCollection.</param>
	public void Add(Visual3D value)
	{
		VerifyAPIForAdd(value);
		int internalCount = InternalCount;
		_collection.Add(value);
		InvalidateEnumerators();
		ConnectChild(internalCount, value);
	}

	private void ConnectChild(int index, Visual3D value)
	{
		value.ParentIndex = index;
		_owner.AddChild(value);
	}

	/// <summary>Inserts a <see cref="T:System.Windows.Media.Media3D.Visual3D" /> object into this <see cref="T:System.Windows.Media.Media3D.Visual3DCollection" /> at the specified index.</summary>
	/// <param name="index">The index at which to insert the Visual3D.</param>
	/// <param name="value">Visual3D to insert.</param>
	public void Insert(int index, Visual3D value)
	{
		VerifyAPIForAdd(value);
		InternalInsert(index, value);
	}

	/// <summary>Removes the first occurrence of the specified <see cref="T:System.Windows.Media.Media3D.Visual3D" /> object from this <see cref="T:System.Windows.Media.Media3D.Visual3DCollection" />.</summary>
	/// <returns>true if <paramref name="value" /> was removed from the collection; otherwise, false. </returns>
	/// <param name="value">Visual3D to remove.</param>
	public bool Remove(Visual3D value)
	{
		VerifyAPIReadWrite(value);
		if (!_collection.Contains(value))
		{
			return false;
		}
		InternalRemoveAt(value.ParentIndex);
		return true;
	}

	/// <summary>Removes the <see cref="T:System.Windows.Media.Media3D.Visual3D" /> object at the specified index from this <see cref="T:System.Windows.Media.Media3D.Visual3DCollection" />.</summary>
	/// <param name="index">Index of the Visual3D to remove.</param>
	public void RemoveAt(int index)
	{
		if (index < 0 || index >= InternalCount)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		VerifyAPIReadWrite(_collection[index]);
		InternalRemoveAt(index);
	}

	/// <summary>Removes all the items from this <see cref="T:System.Windows.Media.Media3D.Visual3DCollection" />.</summary>
	public void Clear()
	{
		VerifyAPIReadWrite();
		FrugalStructList<Visual3D> collection = _collection;
		_collection = default(FrugalStructList<Visual3D>);
		InvalidateEnumerators();
		for (int num = collection.Count - 1; num >= 0; num--)
		{
			_owner.RemoveChild(collection[num]);
		}
	}

	/// <summary>Copies the items of this Visual3DCollection, starting with the specified index, into an array of <see cref="T:System.Windows.Media.Media3D.Visual3D" /> objects.</summary>
	/// <param name="array">Array that is the destination of the items copied from this Visual3DCollection.</param>
	/// <param name="index">The index at which to begin copying.</param>
	public void CopyTo(Visual3D[] array, int index)
	{
		VerifyAPIReadOnly();
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0 || index >= array.Length || index + _collection.Count > array.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		_collection.CopyTo(array, index);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items from the <see cref="T:System.Windows.Media.Media3D.Visual3DCollection" />.</param>
	/// <param name="index">The first position in the specified <see cref="T:System.Array" /> to receive the copied contents.</param>
	void ICollection.CopyTo(Array array, int index)
	{
		VerifyAPIReadOnly();
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0 || index >= array.Length || index + _collection.Count > array.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (array.Rank != 1)
		{
			throw new ArgumentException(SR.Collection_BadRank);
		}
		try
		{
			int count = _collection.Count;
			for (int i = 0; i < count; i++)
			{
				array.SetValue(_collection[i], index + i);
			}
		}
		catch (InvalidCastException innerException)
		{
			throw new ArgumentException(SR.Format(SR.Collection_BadDestArray, "Visual3DCollection"), innerException);
		}
	}

	/// <summary>Determines whether a specified <see cref="T:System.Windows.Media.Media3D.Visual3D" /> is in this <see cref="T:System.Windows.Media.Media3D.Visual3DCollection" />.</summary>
	/// <returns>True if <paramref name="value" />, the specified Visual3D, is in this Visual3DCollection; otherwise, false.</returns>
	/// <param name="value">Visual3D to locate in this Visual3Dcollection.</param>
	public bool Contains(Visual3D value)
	{
		VerifyAPIReadOnly(value);
		if (value != null)
		{
			return value.InternalVisualParent == _owner;
		}
		return false;
	}

	/// <summary>Gets the index of the first occurrence of the specified <see cref="T:System.Windows.Media.Media3D.Visual3D" /> object. </summary>
	/// <returns>The index of the specified <see cref="T:System.Windows.Media.Media3D.Visual3D" />, or -1 if <paramref name="value" /> is null or has a different visual parent.</returns>
	/// <param name="value">The <see cref="T:System.Windows.Media.Media3D.Visual3D" /> for which to search.</param>
	public int IndexOf(Visual3D value)
	{
		VerifyAPIReadOnly(value);
		if (value == null || value.InternalVisualParent != _owner)
		{
			return -1;
		}
		return value.ParentIndex;
	}

	/// <summary>Gets an enumerator for the <see cref="T:System.Windows.Media.Media3D.Visual3DCollection" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
	public Enumerator GetEnumerator()
	{
		VerifyAPIReadOnly();
		return new Enumerator(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IEnumerable.GetEnumerator" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator<Visual3D> IEnumerable<Visual3D>.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Add(System.Object)" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.Media.Media3D.Visual3DCollection" />.</param>
	int IList.Add(object value)
	{
		Add(Cast(value));
		return InternalCount - 1;
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Contains(System.Object)" />.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.Media.Media3D.Visual3DCollection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.Media3D.Visual3DCollection" />.</param>
	bool IList.Contains(object value)
	{
		return Contains(value as Visual3D);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.IndexOf(System.Object)" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.Media3D.Visual3DCollection" />.</param>
	int IList.IndexOf(object value)
	{
		return IndexOf(value as Visual3D);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Insert(System.Int32,System.Object)" />.</summary>
	/// <param name="index">The zero-based index at which to insert the <see cref="T:System.Object" />.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Windows.Media.Media3D.Visual3DCollection" />.</param>
	void IList.Insert(int index, object value)
	{
		Insert(index, Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Remove(System.Object)" />.</summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.Media.Media3D.Visual3DCollection" />.</param>
	void IList.Remove(object value)
	{
		Remove(value as Visual3D);
	}

	internal Visual3D InternalGetItem(int index)
	{
		return _collection[index];
	}

	private void VerifyAPIReadOnly()
	{
		_owner.VerifyAPIReadOnly();
	}

	private void VerifyAPIReadOnly(Visual3D other)
	{
		_owner.VerifyAPIReadOnly(other);
	}

	private void VerifyAPIReadWrite()
	{
		_owner.VerifyAPIReadWrite();
	}

	private void VerifyAPIReadWrite(Visual3D other)
	{
		_owner.VerifyAPIReadWrite(other);
	}

	private Visual3D Cast(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is Visual3D))
		{
			throw new ArgumentException(SR.Format(SR.Collection_BadType, GetType().Name, value.GetType().Name, "Visual3D"));
		}
		return (Visual3D)value;
	}

	private void VerifyAPIForAdd(Visual3D value)
	{
		if (value == null)
		{
			throw new ArgumentException(SR.Collection_NoNull);
		}
		VerifyAPIReadWrite(value);
		if (value.InternalVisualParent != null)
		{
			throw new ArgumentException(SR.VisualCollection_VisualHasParent);
		}
	}

	private void InternalInsert(int index, Visual3D value)
	{
		_collection.Insert(index, value);
		int i = index + 1;
		for (int internalCount = InternalCount; i < internalCount; i++)
		{
			InternalGetItem(i).ParentIndex = i;
		}
		InvalidateEnumerators();
		ConnectChild(index, value);
	}

	private void InternalRemoveAt(int index)
	{
		Visual3D child = _collection[index];
		_collection.RemoveAt(index);
		for (int i = index; i < InternalCount; i++)
		{
			InternalGetItem(i).ParentIndex = i;
		}
		InvalidateEnumerators();
		_owner.RemoveChild(child);
	}

	private void InvalidateEnumerators()
	{
		_version++;
	}

	[Conditional("DEBUG")]
	private void Debug_ICC()
	{
		Dictionary<Visual3D, string> dictionary = new Dictionary<Visual3D, string>();
		for (int i = 0; i < _collection.Count; i++)
		{
			Visual3D key = _collection[i];
			dictionary.Add(key, string.Empty);
		}
	}
}
