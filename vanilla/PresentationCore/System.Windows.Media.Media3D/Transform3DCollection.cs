using System.Collections;
using System.Collections.Generic;
using System.Windows.Media.Animation;
using MS.Internal.Collections;
using MS.Internal.PresentationCore;
using MS.Utility;

namespace System.Windows.Media.Media3D;

/// <summary>Represents an ordered collection of <see cref="T:System.Windows.Media.Media3D.Transform3D" /> objects. </summary>
public sealed class Transform3DCollection : Animatable, IList, ICollection, IEnumerable, IList<Transform3D>, ICollection<Transform3D>, IEnumerable<Transform3D>
{
	/// <summary>Enumerates <see cref="T:System.Windows.Media.Media3D.Transform3D" /> items in a <see cref="T:System.Windows.Media.Media3D.Transform3DCollection" />. </summary>
	public struct Enumerator : IEnumerator, IEnumerator<Transform3D>, IDisposable
	{
		private Transform3D _current;

		private Transform3DCollection _list;

		private uint _version;

		private int _index;

		/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
		object IEnumerator.Current => Current;

		/// <summary>Gets the current element in the collection.</summary>
		/// <returns>The current element in the collection.</returns>
		public Transform3D Current
		{
			get
			{
				if (_index > -1)
				{
					return _current;
				}
				if (_index == -1)
				{
					throw new InvalidOperationException(SR.Enumerator_NotStarted);
				}
				throw new InvalidOperationException(SR.Enumerator_ReachedEnd);
			}
		}

		internal Enumerator(Transform3DCollection list)
		{
			_list = list;
			_version = list._version;
			_index = -1;
			_current = null;
		}

		/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
		void IDisposable.Dispose()
		{
		}

		/// <summary>Advances the enumerator to the next element in the collection. </summary>
		/// <returns>true if the enumerator successfully advanced to the next element; otherwise, false.</returns>
		public bool MoveNext()
		{
			_list.ReadPreamble();
			if (_version == _list._version)
			{
				if (_index > -2 && _index < _list._collection.Count - 1)
				{
					_current = _list._collection[++_index];
					return true;
				}
				_index = -2;
				return false;
			}
			throw new InvalidOperationException(SR.Enumerator_CollectionChanged);
		}

		/// <summary>Resets the enumerator to its initial position, which is before the first element in the collection. </summary>
		public void Reset()
		{
			_list.ReadPreamble();
			if (_version == _list._version)
			{
				_index = -1;
				return;
			}
			throw new InvalidOperationException(SR.Enumerator_CollectionChanged);
		}
	}

	private static Transform3DCollection s_empty;

	internal FrugalStructList<Transform3D> _collection;

	internal uint _version;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Media3D.Transform3D" /> at the specified zero-based index.</summary>
	/// <returns>The item at the specified index.</returns>
	/// <param name="index">The zero-based index of the Transform3D object to get or set.</param>
	public Transform3D this[int index]
	{
		get
		{
			ReadPreamble();
			return _collection[index];
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentException(SR.Collection_NoNull);
			}
			WritePreamble();
			if (_collection[index] != value)
			{
				Transform3D oldValue = _collection[index];
				OnFreezablePropertyChanged(oldValue, value);
				_collection[index] = value;
				OnSet(oldValue, value);
			}
			_version++;
			WritePostscript();
		}
	}

	/// <summary>Gets the number of <see cref="T:System.Windows.Media.Media3D.Transform3D" /> objects contained in the <see cref="T:System.Windows.Media.Media3D.Transform3DCollection" />. </summary>
	/// <returns>The number of Transform3D objects contained in the Transform3DCollection.</returns>
	public int Count
	{
		get
		{
			ReadPreamble();
			return _collection.Count;
		}
	}

	bool ICollection<Transform3D>.IsReadOnly
	{
		get
		{
			ReadPreamble();
			return base.IsFrozen;
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsReadOnly" />.</summary>
	/// <returns>true if the the <see cref="T:System.Windows.Media.Media3D.Transform3DCollection" /> is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => ((ICollection<Transform3D>)this).IsReadOnly;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsFixedSize" />.</summary>
	/// <returns>For a description of this member, see <see cref="P:System.Collections.IList.IsFixedSize" />.</returns>
	bool IList.IsFixedSize
	{
		get
		{
			ReadPreamble();
			return base.IsFrozen;
		}
	}

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

	/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.IsSynchronized" />.</summary>
	/// <returns>true if access to the <see cref="T:System.Windows.Media.Media3D.Transform3DCollection" /> is synchronized (thread safe); otherwise, false.</returns>
	bool ICollection.IsSynchronized
	{
		get
		{
			ReadPreamble();
			if (!base.IsFrozen)
			{
				return base.Dispatcher != null;
			}
			return true;
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.SyncRoot" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Windows.Media.Media3D.Transform3DCollection" />.</returns>
	object ICollection.SyncRoot
	{
		get
		{
			ReadPreamble();
			return this;
		}
	}

	internal static Transform3DCollection Empty
	{
		get
		{
			if (s_empty == null)
			{
				Transform3DCollection transform3DCollection = new Transform3DCollection();
				transform3DCollection.Freeze();
				s_empty = transform3DCollection;
			}
			return s_empty;
		}
	}

	internal event ItemInsertedHandler ItemInserted;

	internal event ItemRemovedHandler ItemRemoved;

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.Transform3DCollection" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Transform3DCollection Clone()
	{
		return (Transform3DCollection)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.Transform3DCollection" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Transform3DCollection CloneCurrentValue()
	{
		return (Transform3DCollection)base.CloneCurrentValue();
	}

	/// <summary>Adds a <see cref="T:System.Windows.Media.Media3D.Transform3D" /> to the end of the <see cref="T:System.Windows.Media.Media3D.Transform3DCollection" />.</summary>
	/// <param name="value">Item to add to the end of this collection.</param>
	public void Add(Transform3D value)
	{
		AddHelper(value);
	}

	/// <summary>Removes all the items from this <see cref="T:System.Windows.Media.Media3D.Transform3DCollection" />.</summary>
	public void Clear()
	{
		WritePreamble();
		FrugalStructList<Transform3D> collection = _collection;
		_collection = new FrugalStructList<Transform3D>(_collection.Capacity);
		for (int num = collection.Count - 1; num >= 0; num--)
		{
			OnFreezablePropertyChanged(collection[num], null);
			OnRemove(collection[num]);
		}
		_version++;
		WritePostscript();
	}

	/// <summary>Determines whether the specified <see cref="T:System.Windows.Media.Media3D.Transform3D" /> is in this <see cref="T:System.Windows.Media.Media3D.Transform3DCollection" />.</summary>
	/// <returns>true if <paramref name="value" />, the specified Transform3D, is in this Transform3DCollection; otherwise, false.</returns>
	/// <param name="value">The item to locate in this collection.</param>
	public bool Contains(Transform3D value)
	{
		ReadPreamble();
		return _collection.Contains(value);
	}

	/// <summary>Gets the index position of the first occurrence of the specified <see cref="T:System.Windows.Media.Media3D.Transform3D" />.</summary>
	/// <returns>The index position of the specified Transform3D.</returns>
	/// <param name="value">The Transform3D to search for.</param>
	public int IndexOf(Transform3D value)
	{
		ReadPreamble();
		return _collection.IndexOf(value);
	}

	/// <summary>Inserts a <see cref="T:System.Windows.Media.Media3D.Transform3D" /> into this <see cref="T:System.Windows.Media.Media3D.Transform3DCollection" /> at the specified index position.</summary>
	/// <param name="index">The index position at which to insert the specified Transform3D.</param>
	/// <param name="value">The Transform3D to insert.</param>
	public void Insert(int index, Transform3D value)
	{
		if (value == null)
		{
			throw new ArgumentException(SR.Collection_NoNull);
		}
		WritePreamble();
		OnFreezablePropertyChanged(null, value);
		_collection.Insert(index, value);
		OnInsert(value);
		_version++;
		WritePostscript();
	}

	/// <summary>Removes the first occurrence of the specified <see cref="T:System.Windows.Media.Media3D.Transform3D" /> from the <see cref="T:System.Windows.Media.Media3D.Transform3DCollection" />.</summary>
	/// <returns>true if <paramref name="value" /> was removed fro the collection; otherwise, false. </returns>
	/// <param name="value">The Transform3D to remove from this collection.</param>
	public bool Remove(Transform3D value)
	{
		WritePreamble();
		int num = IndexOf(value);
		if (num >= 0)
		{
			Transform3D oldValue = _collection[num];
			OnFreezablePropertyChanged(oldValue, null);
			_collection.RemoveAt(num);
			OnRemove(oldValue);
			_version++;
			WritePostscript();
			return true;
		}
		return false;
	}

	/// <summary>Removes the <see cref="T:System.Windows.Media.Media3D.Transform3D" /> at the specified index position from the <see cref="T:System.Windows.Media.Media3D.Transform3DCollection" />.</summary>
	/// <param name="index">The index position of the Transform3D to remove.</param>
	public void RemoveAt(int index)
	{
		RemoveAtWithoutFiringPublicEvents(index);
		WritePostscript();
	}

	internal void RemoveAtWithoutFiringPublicEvents(int index)
	{
		WritePreamble();
		Transform3D oldValue = _collection[index];
		OnFreezablePropertyChanged(oldValue, null);
		_collection.RemoveAt(index);
		OnRemove(oldValue);
		_version++;
	}

	/// <summary>Copies the items of this <see cref="T:System.Windows.Media.Media3D.Transform3DCollection" />, starting with the specified index value, into an array of <see cref="T:System.Windows.Media.Media3D.Transform3D" /> objects.</summary>
	/// <param name="array">The array that is the destination of the items copied from this Transform3DCollection.</param>
	/// <param name="index">The index at which copying begins.</param>
	public void CopyTo(Transform3D[] array, int index)
	{
		ReadPreamble();
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0 || index + _collection.Count > array.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		_collection.CopyTo(array, index);
	}

	/// <summary>Returns an enumerator that can iterate through the collection.</summary>
	/// <returns>An enumerator that can iterate the collection.</returns>
	public Enumerator GetEnumerator()
	{
		ReadPreamble();
		return new Enumerator(this);
	}

	IEnumerator<Transform3D> IEnumerable<Transform3D>.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Add(System.Object)" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.Media.Media3D.Transform3DCollection" />.</param>
	int IList.Add(object value)
	{
		return AddHelper(Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Contains(System.Object)" />.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.Media.Media3D.Transform3DCollection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.Media3D.Transform3DCollection" />.</param>
	bool IList.Contains(object value)
	{
		return Contains(value as Transform3D);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.IndexOf(System.Object)" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.Media3D.Transform3DCollection" />.</param>
	int IList.IndexOf(object value)
	{
		return IndexOf(value as Transform3D);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Insert(System.Int32,System.Object)" />.</summary>
	/// <param name="index">The zero-based index at which to insert the <see cref="T:System.Object" />.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Windows.Media.Media3D.Transform3DCollection" />.</param>
	void IList.Insert(int index, object value)
	{
		Insert(index, Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Remove(System.Object)" />.</summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.Media.Media3D.Transform3DCollection" />.</param>
	void IList.Remove(object value)
	{
		Remove(value as Transform3D);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items from the <see cref="T:System.Windows.Media.Media3D.Transform3DCollection" />.</param>
	/// <param name="index">The first position in the specified <see cref="T:System.Array" /> to receive the copied contents.</param>
	void ICollection.CopyTo(Array array, int index)
	{
		ReadPreamble();
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0 || index + _collection.Count > array.Length)
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
			throw new ArgumentException(SR.Format(SR.Collection_BadDestArray, GetType().Name), innerException);
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IEnumerable.GetEnumerator" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	internal Transform3D Internal_GetItem(int i)
	{
		return _collection[i];
	}

	internal override void OnInheritanceContextChangedCore(EventArgs args)
	{
		base.OnInheritanceContextChangedCore(args);
		for (int i = 0; i < Count; i++)
		{
			DependencyObject dependencyObject = _collection[i];
			if (dependencyObject != null && dependencyObject.InheritanceContext == this)
			{
				dependencyObject.OnInheritanceContextChanged(args);
			}
		}
	}

	private Transform3D Cast(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is Transform3D))
		{
			throw new ArgumentException(SR.Format(SR.Collection_BadType, GetType().Name, value.GetType().Name, "Transform3D"));
		}
		return (Transform3D)value;
	}

	private int AddHelper(Transform3D value)
	{
		int result = AddWithoutFiringPublicEvents(value);
		WritePostscript();
		return result;
	}

	internal int AddWithoutFiringPublicEvents(Transform3D value)
	{
		if (value == null)
		{
			throw new ArgumentException(SR.Collection_NoNull);
		}
		WritePreamble();
		OnFreezablePropertyChanged(null, value);
		int result = _collection.Add(value);
		OnInsert(value);
		_version++;
		return result;
	}

	private void OnInsert(object item)
	{
		if (this.ItemInserted != null)
		{
			this.ItemInserted(this, item);
		}
	}

	private void OnRemove(object oldValue)
	{
		if (this.ItemRemoved != null)
		{
			this.ItemRemoved(this, oldValue);
		}
	}

	private void OnSet(object oldValue, object newValue)
	{
		OnInsert(newValue);
		OnRemove(oldValue);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new Transform3DCollection();
	}

	protected override void CloneCore(Freezable source)
	{
		Transform3DCollection transform3DCollection = (Transform3DCollection)source;
		base.CloneCore(source);
		int count = transform3DCollection._collection.Count;
		_collection = new FrugalStructList<Transform3D>(count);
		for (int i = 0; i < count; i++)
		{
			Transform3D transform3D = transform3DCollection._collection[i].Clone();
			OnFreezablePropertyChanged(null, transform3D);
			_collection.Add(transform3D);
			OnInsert(transform3D);
		}
	}

	protected override void CloneCurrentValueCore(Freezable source)
	{
		Transform3DCollection transform3DCollection = (Transform3DCollection)source;
		base.CloneCurrentValueCore(source);
		int count = transform3DCollection._collection.Count;
		_collection = new FrugalStructList<Transform3D>(count);
		for (int i = 0; i < count; i++)
		{
			Transform3D transform3D = transform3DCollection._collection[i].CloneCurrentValue();
			OnFreezablePropertyChanged(null, transform3D);
			_collection.Add(transform3D);
			OnInsert(transform3D);
		}
	}

	protected override void GetAsFrozenCore(Freezable source)
	{
		Transform3DCollection transform3DCollection = (Transform3DCollection)source;
		base.GetAsFrozenCore(source);
		int count = transform3DCollection._collection.Count;
		_collection = new FrugalStructList<Transform3D>(count);
		for (int i = 0; i < count; i++)
		{
			Transform3D transform3D = (Transform3D)transform3DCollection._collection[i].GetAsFrozen();
			OnFreezablePropertyChanged(null, transform3D);
			_collection.Add(transform3D);
			OnInsert(transform3D);
		}
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable source)
	{
		Transform3DCollection transform3DCollection = (Transform3DCollection)source;
		base.GetCurrentValueAsFrozenCore(source);
		int count = transform3DCollection._collection.Count;
		_collection = new FrugalStructList<Transform3D>(count);
		for (int i = 0; i < count; i++)
		{
			Transform3D transform3D = (Transform3D)transform3DCollection._collection[i].GetCurrentValueAsFrozen();
			OnFreezablePropertyChanged(null, transform3D);
			_collection.Add(transform3D);
			OnInsert(transform3D);
		}
	}

	protected override bool FreezeCore(bool isChecking)
	{
		bool flag = base.FreezeCore(isChecking);
		int count = _collection.Count;
		for (int i = 0; i < count && flag; i++)
		{
			flag &= Freezable.Freeze(_collection[i], isChecking);
		}
		return flag;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.Transform3DCollection" /> class.</summary>
	public Transform3DCollection()
	{
		_collection = default(FrugalStructList<Transform3D>);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.Transform3DCollection" /> class with the specified capacity.</summary>
	/// <param name="capacity">Integer that specifies the capacity of the Transform3DCollection.</param>
	public Transform3DCollection(int capacity)
	{
		_collection = new FrugalStructList<Transform3D>(capacity);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.Transform3DCollection" /> class using the specified collection.</summary>
	/// <param name="collection">Collection with which to instantiate the Transform3DCollection.</param>
	public Transform3DCollection(IEnumerable<Transform3D> collection)
	{
		WritePreamble();
		if (collection != null)
		{
			bool flag = true;
			if (collection is ICollection<Transform3D> collection2)
			{
				_collection = new FrugalStructList<Transform3D>(collection2);
			}
			else if (collection is ICollection collection3)
			{
				_collection = new FrugalStructList<Transform3D>(collection3);
			}
			else
			{
				_collection = default(FrugalStructList<Transform3D>);
				foreach (Transform3D item in collection)
				{
					Transform3D transform3D = item ?? throw new ArgumentException(SR.Collection_NoNull);
					OnFreezablePropertyChanged(null, transform3D);
					_collection.Add(transform3D);
					OnInsert(transform3D);
				}
				flag = false;
			}
			if (flag)
			{
				foreach (Transform3D item2 in collection)
				{
					if (item2 == null)
					{
						throw new ArgumentException(SR.Collection_NoNull);
					}
					OnFreezablePropertyChanged(null, item2);
					OnInsert(item2);
				}
			}
			WritePostscript();
			return;
		}
		throw new ArgumentNullException("collection");
	}
}
