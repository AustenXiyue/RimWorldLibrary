using System.Collections;
using System.Collections.Generic;
using System.Windows.Media.Animation;
using MS.Internal.Collections;
using MS.Internal.PresentationCore;
using MS.Utility;

namespace System.Windows.Media;

/// <summary> Represents a collection of <see cref="T:System.Windows.Media.Geometry" /> objects.</summary>
public sealed class GeometryCollection : Animatable, IList, ICollection, IEnumerable, IList<Geometry>, ICollection<Geometry>, IEnumerable<Geometry>
{
	/// <summary>Enumerates <see cref="T:System.Windows.Media.Geometry" /> items in a <see cref="T:System.Windows.Media.GeometryCollection" />. </summary>
	public struct Enumerator : IEnumerator, IEnumerator<Geometry>, IDisposable
	{
		private Geometry _current;

		private GeometryCollection _list;

		private uint _version;

		private int _index;

		/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
		object IEnumerator.Current => Current;

		/// <summary>Gets the current element in the collection.</summary>
		/// <returns>The current element in the collection.</returns>
		public Geometry Current
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

		internal Enumerator(GeometryCollection list)
		{
			_list = list;
			_version = list._version;
			_index = -1;
			_current = null;
		}

		/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
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

	private static GeometryCollection s_empty;

	internal FrugalStructList<Geometry> _collection;

	internal uint _version;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Geometry" /> at the specified index position. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Geometry" /> at the specified index.</returns>
	/// <param name="index">    The zero-based index of the <see cref="T:System.Windows.Media.Geometry" /> to get or set.</param>
	/// <exception cref="T:System.InvalidOperationException">The attempt to modify the collection is invalid because the collection is frozen (its <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true).</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is equal to or greater than <see cref="P:System.Windows.Media.Animation.DoubleKeyFrameCollection.Count" />.</exception>
	public Geometry this[int index]
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
				Geometry oldValue = _collection[index];
				OnFreezablePropertyChanged(oldValue, value);
				_collection[index] = value;
				OnSet(oldValue, value);
			}
			_version++;
			WritePostscript();
		}
	}

	/// <summary> Gets the number of geometries contained in the <see cref="T:System.Windows.Media.GeometryCollection" />.  </summary>
	/// <returns>The number of geometries contained in the <see cref="T:System.Windows.Media.GeometryCollection" />. </returns>
	public int Count
	{
		get
		{
			ReadPreamble();
			return _collection.Count;
		}
	}

	bool ICollection<Geometry>.IsReadOnly
	{
		get
		{
			ReadPreamble();
			return base.IsFrozen;
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsReadOnly" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.GeometryCollection" /> is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => ((ICollection<Geometry>)this).IsReadOnly;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsFixedSize" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.GeometryCollection" /> has a fixed size; otherwise, false.</returns>
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
	/// <returns>true if access to the <see cref="T:System.Windows.Media.GeometryCollection" /> is synchronized (thread safe); otherwise, false.</returns>
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
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Windows.Media.GeometryCollection" />.</returns>
	object ICollection.SyncRoot
	{
		get
		{
			ReadPreamble();
			return this;
		}
	}

	internal static GeometryCollection Empty
	{
		get
		{
			if (s_empty == null)
			{
				GeometryCollection geometryCollection = new GeometryCollection();
				geometryCollection.Freeze();
				s_empty = geometryCollection;
			}
			return s_empty;
		}
	}

	internal event ItemInsertedHandler ItemInserted;

	internal event ItemRemovedHandler ItemRemoved;

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.GeometryCollection" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new GeometryCollection Clone()
	{
		return (GeometryCollection)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.GeometryCollection" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new GeometryCollection CloneCurrentValue()
	{
		return (GeometryCollection)base.CloneCurrentValue();
	}

	/// <summary>Adds a <see cref="T:System.Windows.Media.Geometry" /> to the end of the collection. </summary>
	/// <param name="value">The <see cref="T:System.Windows.Media.Geometry" /> to add to the end of the collection.</param>
	public void Add(Geometry value)
	{
		AddHelper(value);
	}

	/// <summary>Removes all <see cref="T:System.Windows.Media.Geometry" /> objects from the collection. </summary>
	public void Clear()
	{
		WritePreamble();
		FrugalStructList<Geometry> collection = _collection;
		_collection = new FrugalStructList<Geometry>(_collection.Capacity);
		for (int num = collection.Count - 1; num >= 0; num--)
		{
			OnFreezablePropertyChanged(collection[num], null);
			OnRemove(collection[num]);
		}
		_version++;
		WritePostscript();
	}

	/// <summary>Returns a value that indicates whether the collection contains the specified <see cref="T:System.Windows.Media.Geometry" />.</summary>
	/// <returns>true if <paramref name="value" /> is found in the <see cref="T:System.Collections.IList" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Windows.Media.Geometry" /> to locate in the collection.</param>
	public bool Contains(Geometry value)
	{
		ReadPreamble();
		return _collection.Contains(value);
	}

	/// <summary>Searches for the specified <see cref="T:System.Windows.Media.Geometry" /> and returns the zero-based index of the first occurrence within the entire collection.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="value" /> within the entire collection, if found; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Windows.Media.Geometry" /> to locate in the collection.</param>
	public int IndexOf(Geometry value)
	{
		ReadPreamble();
		return _collection.IndexOf(value);
	}

	/// <summary>Inserts a <see cref="T:System.Windows.Media.Geometry" /> into a specific location within the collection. </summary>
	/// <param name="index">The index position at which the <see cref="T:System.Windows.Media.Geometry" /> is inserted.</param>
	/// <param name="value">The <see cref="T:System.Windows.Media.Geometry" /> object to insert in the collection.</param>
	public void Insert(int index, Geometry value)
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

	/// <summary> Removes the first occurrence of the specified <see cref="T:System.Windows.Media.Geometry" />  from this <see cref="T:System.Windows.Media.GeometryCollection" />. </summary>
	/// <returns>true if <paramref name="value" /> was removed from the collection; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Windows.Media.Geometry" />  to remove from this <see cref="T:System.Windows.Media.GeometryCollection" />.</param>
	public bool Remove(Geometry value)
	{
		WritePreamble();
		int num = IndexOf(value);
		if (num >= 0)
		{
			Geometry oldValue = _collection[num];
			OnFreezablePropertyChanged(oldValue, null);
			_collection.RemoveAt(num);
			OnRemove(oldValue);
			_version++;
			WritePostscript();
			return true;
		}
		return false;
	}

	/// <summary> Removes the <see cref="T:System.Windows.Media.Geometry" />  at the specified index from this <see cref="T:System.Windows.Media.GeometryCollection" />. </summary>
	/// <param name="index">The index of the <see cref="T:System.Windows.Media.Geometry" />  to remove.</param>
	public void RemoveAt(int index)
	{
		RemoveAtWithoutFiringPublicEvents(index);
		WritePostscript();
	}

	internal void RemoveAtWithoutFiringPublicEvents(int index)
	{
		WritePreamble();
		Geometry oldValue = _collection[index];
		OnFreezablePropertyChanged(oldValue, null);
		_collection.RemoveAt(index);
		OnRemove(oldValue);
		_version++;
	}

	/// <summary>Copies all of the <see cref="T:System.Windows.Media.Geometry" /> objects in a collection to a specified array.</summary>
	/// <param name="array">Identifies the array to which content is copied.</param>
	/// <param name="index">Index position in the array to which the contents of the collection are copied.</param>
	public void CopyTo(Geometry[] array, int index)
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
	/// <returns>An <see cref="T:System.Windows.Media.GeometryCollection.Enumerator" /> that can iterate through the collection.</returns>
	public Enumerator GetEnumerator()
	{
		ReadPreamble();
		return new Enumerator(this);
	}

	IEnumerator<Geometry> IEnumerable<Geometry>.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Add(System.Object)" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.Media.GeometryCollection" />.</param>
	int IList.Add(object value)
	{
		return AddHelper(Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Contains(System.Object)" />.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.Media.GeometryCollection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.GeometryCollection" />.</param>
	bool IList.Contains(object value)
	{
		return Contains(value as Geometry);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.IndexOf(System.Object)" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.GeometryCollection" />.</param>
	int IList.IndexOf(object value)
	{
		return IndexOf(value as Geometry);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Insert(System.Int32,System.Object)" />.</summary>
	/// <param name="index">The zero-based index at which to insert the <see cref="T:System.Object" />.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Windows.Media.GeometryCollection" />.</param>
	void IList.Insert(int index, object value)
	{
		Insert(index, Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Remove(System.Object)" />.</summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.Media.GeometryCollection" />.</param>
	void IList.Remove(object value)
	{
		Remove(value as Geometry);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items from the <see cref="T:System.Windows.Media.GeometryCollection" />.</param>
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

	internal Geometry Internal_GetItem(int i)
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

	private Geometry Cast(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is Geometry))
		{
			throw new ArgumentException(SR.Format(SR.Collection_BadType, GetType().Name, value.GetType().Name, "Geometry"));
		}
		return (Geometry)value;
	}

	private int AddHelper(Geometry value)
	{
		int result = AddWithoutFiringPublicEvents(value);
		WritePostscript();
		return result;
	}

	internal int AddWithoutFiringPublicEvents(Geometry value)
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
		return new GeometryCollection();
	}

	protected override void CloneCore(Freezable source)
	{
		GeometryCollection geometryCollection = (GeometryCollection)source;
		base.CloneCore(source);
		int count = geometryCollection._collection.Count;
		_collection = new FrugalStructList<Geometry>(count);
		for (int i = 0; i < count; i++)
		{
			Geometry geometry = geometryCollection._collection[i].Clone();
			OnFreezablePropertyChanged(null, geometry);
			_collection.Add(geometry);
			OnInsert(geometry);
		}
	}

	protected override void CloneCurrentValueCore(Freezable source)
	{
		GeometryCollection geometryCollection = (GeometryCollection)source;
		base.CloneCurrentValueCore(source);
		int count = geometryCollection._collection.Count;
		_collection = new FrugalStructList<Geometry>(count);
		for (int i = 0; i < count; i++)
		{
			Geometry geometry = geometryCollection._collection[i].CloneCurrentValue();
			OnFreezablePropertyChanged(null, geometry);
			_collection.Add(geometry);
			OnInsert(geometry);
		}
	}

	protected override void GetAsFrozenCore(Freezable source)
	{
		GeometryCollection geometryCollection = (GeometryCollection)source;
		base.GetAsFrozenCore(source);
		int count = geometryCollection._collection.Count;
		_collection = new FrugalStructList<Geometry>(count);
		for (int i = 0; i < count; i++)
		{
			Geometry geometry = (Geometry)geometryCollection._collection[i].GetAsFrozen();
			OnFreezablePropertyChanged(null, geometry);
			_collection.Add(geometry);
			OnInsert(geometry);
		}
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable source)
	{
		GeometryCollection geometryCollection = (GeometryCollection)source;
		base.GetCurrentValueAsFrozenCore(source);
		int count = geometryCollection._collection.Count;
		_collection = new FrugalStructList<Geometry>(count);
		for (int i = 0; i < count; i++)
		{
			Geometry geometry = (Geometry)geometryCollection._collection[i].GetCurrentValueAsFrozen();
			OnFreezablePropertyChanged(null, geometry);
			_collection.Add(geometry);
			OnInsert(geometry);
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.GeometryCollection" /> class.</summary>
	public GeometryCollection()
	{
		_collection = default(FrugalStructList<Geometry>);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.GeometryCollection" /> class with the specified capacity, or the number of <see cref="T:System.Windows.Media.Geometry" /> objects the collection is initially capable of storing.</summary>
	/// <param name="capacity">The number of <see cref="T:System.Windows.Media.Geometry" /> objects that the collection is initially capable of storing.</param>
	public GeometryCollection(int capacity)
	{
		_collection = new FrugalStructList<Geometry>(capacity);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.GeometryCollection" /> class with the specified collection of <see cref="T:System.Windows.Media.Geometry" /> objects.</summary>
	/// <param name="collection">The collection of <see cref="T:System.Windows.Media.Geometry" /> objects that make up the <see cref="T:System.Windows.Media.GeometryCollection" />.</param>
	public GeometryCollection(IEnumerable<Geometry> collection)
	{
		WritePreamble();
		if (collection != null)
		{
			bool flag = true;
			if (collection is ICollection<Geometry> collection2)
			{
				_collection = new FrugalStructList<Geometry>(collection2);
			}
			else if (collection is ICollection collection3)
			{
				_collection = new FrugalStructList<Geometry>(collection3);
			}
			else
			{
				_collection = default(FrugalStructList<Geometry>);
				foreach (Geometry item in collection)
				{
					Geometry geometry = item ?? throw new ArgumentException(SR.Collection_NoNull);
					OnFreezablePropertyChanged(null, geometry);
					_collection.Add(geometry);
					OnInsert(geometry);
				}
				flag = false;
			}
			if (flag)
			{
				foreach (Geometry item2 in collection)
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
