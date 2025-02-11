using System.Collections;
using System.Collections.Generic;
using System.Windows.Media.Animation;
using MS.Internal.PresentationCore;
using MS.Utility;

namespace System.Windows.Media.Media3D;

/// <summary>Represents an ordered collection of <see cref="T:System.Windows.Media.Media3D.GeneralTransform3D" /> objects. </summary>
public sealed class GeneralTransform3DCollection : Animatable, IList, ICollection, IEnumerable, IList<GeneralTransform3D>, ICollection<GeneralTransform3D>, IEnumerable<GeneralTransform3D>
{
	/// <summary>Enumerates <see cref="T:System.Windows.Media.Media3D.GeneralTransform3D" /> items in a <see cref="T:System.Windows.Media.Media3D.GeneralTransform3DCollection" />.</summary>
	public struct Enumerator : IEnumerator, IEnumerator<GeneralTransform3D>, IDisposable
	{
		private GeneralTransform3D _current;

		private GeneralTransform3DCollection _list;

		private uint _version;

		private int _index;

		/// <summary>For a description of this member, see <see cref="P:System.Collections.IEnumerator.Current" />.</summary>
		object IEnumerator.Current => Current;

		/// <summary>Gets the current element in the collection.</summary>
		/// <returns>The current element in the collection.</returns>
		public GeneralTransform3D Current
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

		internal Enumerator(GeneralTransform3DCollection list)
		{
			_list = list;
			_version = list._version;
			_index = -1;
			_current = null;
		}

		/// <summary>For a description of this member, see <see cref="M:System.IDisposable.Dispose" />.</summary>
		void IDisposable.Dispose()
		{
		}

		/// <summary>Advances the enumerator to the next element in the collection.</summary>
		/// <returns>true if the enumerator successfully advances to the next element; otherwise, false.</returns>
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

	private static GeneralTransform3DCollection s_empty;

	internal FrugalStructList<GeneralTransform3D> _collection;

	internal uint _version;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Media3D.GeneralTransform3D" /> object at the specified index position. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Media3D.GeneralTransform3D" /> object at the <paramref name="index" /> position.</returns>
	/// <param name="index">The zero-based index position of the object to get or set.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the collection.</exception>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.- or -The collection has a fixed size.</exception>
	public GeneralTransform3D this[int index]
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
				GeneralTransform3D oldValue = _collection[index];
				OnFreezablePropertyChanged(oldValue, value);
				_collection[index] = value;
			}
			_version++;
			WritePostscript();
		}
	}

	/// <summary>Gets the number of objects in this collection.</summary>
	/// <returns>The number of items in the collection.</returns>
	public int Count
	{
		get
		{
			ReadPreamble();
			return _collection.Count;
		}
	}

	bool ICollection<GeneralTransform3D>.IsReadOnly
	{
		get
		{
			ReadPreamble();
			return base.IsFrozen;
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsReadOnly" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.Media3D.GeneralTransform3DCollection" /> is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => ((ICollection<GeneralTransform3D>)this).IsReadOnly;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsFixedSize" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.Media3D.GeneralTransform3DCollection" /> has a fixed size; otherwise, false.</returns>
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
	/// <returns>true if access to the <see cref="T:System.Windows.Media.Media3D.GeneralTransform3DCollection" /> is synchronized (thread safe); otherwise, false.</returns>
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
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Windows.Media.Media3D.GeneralTransform3DCollection" />.</returns>
	object ICollection.SyncRoot
	{
		get
		{
			ReadPreamble();
			return this;
		}
	}

	internal static GeneralTransform3DCollection Empty
	{
		get
		{
			if (s_empty == null)
			{
				GeneralTransform3DCollection generalTransform3DCollection = new GeneralTransform3DCollection();
				generalTransform3DCollection.Freeze();
				s_empty = generalTransform3DCollection;
			}
			return s_empty;
		}
	}

	/// <summary>Creates a modifiable clone of this collection, making deep copies of this object's values. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new GeneralTransform3DCollection Clone()
	{
		return (GeneralTransform3DCollection)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this collection object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new GeneralTransform3DCollection CloneCurrentValue()
	{
		return (GeneralTransform3DCollection)base.CloneCurrentValue();
	}

	/// <summary>Adds a <see cref="T:System.Windows.Media.Media3D.GeneralTransform3D" /> object to the end of the collection.</summary>
	/// <param name="value">The <see cref="T:System.Windows.Media.Media3D.GeneralTransform3D" /> to add to the end of the collection.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.- or -The collection has a fixed size.</exception>
	public void Add(GeneralTransform3D value)
	{
		AddHelper(value);
	}

	/// <summary>Removes all <see cref="T:System.Windows.Media.Media3D.GeneralTransform3D" /> objects from the collection.</summary>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	public void Clear()
	{
		WritePreamble();
		for (int num = _collection.Count - 1; num >= 0; num--)
		{
			OnFreezablePropertyChanged(_collection[num], null);
		}
		_collection.Clear();
		_version++;
		WritePostscript();
	}

	/// <summary>Indicates whether the collection contains the specified <see cref="T:System.Windows.Media.Media3D.GeneralTransform3D" /> object.</summary>
	/// <returns>true if the collection contains <paramref name="value" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Windows.Media.Media3D.GeneralTransform3D" /> object to locate in the collection.</param>
	public bool Contains(GeneralTransform3D value)
	{
		ReadPreamble();
		return _collection.Contains(value);
	}

	/// <summary>Searches for the specified <see cref="T:System.Windows.Media.Media3D.GeneralTransform3D" /> object within the collection. </summary>
	/// <returns>The zero-based index position of <paramref name="value" />, or –1 if the object is not found in the collection.</returns>
	/// <param name="value">The object to locate.</param>
	public int IndexOf(GeneralTransform3D value)
	{
		ReadPreamble();
		return _collection.IndexOf(value);
	}

	/// <summary>Inserts a <see cref="T:System.Windows.Media.Media3D.GeneralTransform3D" /> object into the specified index position within the collection.</summary>
	/// <param name="index">The zero-based index position to insert the object.</param>
	/// <param name="value">The object to insert.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the collection.</exception>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.- or -The collection has a fixed size.</exception>
	public void Insert(int index, GeneralTransform3D value)
	{
		if (value == null)
		{
			throw new ArgumentException(SR.Collection_NoNull);
		}
		WritePreamble();
		OnFreezablePropertyChanged(null, value);
		_collection.Insert(index, value);
		_version++;
		WritePostscript();
	}

	/// <summary>Deletes a <see cref="T:System.Windows.Media.Media3D.GeneralTransform3D" /> object from the collection.</summary>
	/// <returns>true if <paramref name="value" /> was successfully deleted; otherwise, false.</returns>
	/// <param name="value">The object to remove.</param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.- or -The collection has a fixed size.</exception>
	public bool Remove(GeneralTransform3D value)
	{
		WritePreamble();
		int num = IndexOf(value);
		if (num >= 0)
		{
			GeneralTransform3D oldValue = _collection[num];
			OnFreezablePropertyChanged(oldValue, null);
			_collection.RemoveAt(num);
			_version++;
			WritePostscript();
			return true;
		}
		return false;
	}

	/// <summary>Deletes a <see cref="T:System.Windows.Media.Media3D.GeneralTransform3D" /> object from the <see cref="T:System.Windows.Media.Media3D.GeneralTransform3DCollection" />.</summary>
	/// <param name="index">The zero-based index position to remove the object.</param>
	public void RemoveAt(int index)
	{
		RemoveAtWithoutFiringPublicEvents(index);
		WritePostscript();
	}

	internal void RemoveAtWithoutFiringPublicEvents(int index)
	{
		WritePreamble();
		GeneralTransform3D oldValue = _collection[index];
		OnFreezablePropertyChanged(oldValue, null);
		_collection.RemoveAt(index);
		_version++;
	}

	/// <summary>Copies the <see cref="T:System.Windows.Media.Media3D.GeneralTransform3D" /> objects in the collection into an array of <see cref="T:System.Windows.Media.Media3D.GeneralTransform3D" /> objects, starting at the specified index position. </summary>
	/// <param name="array">The destination array.</param>
	/// <param name="index">The zero-based index position where copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional. -or-The number of items in the source collection is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
	public void CopyTo(GeneralTransform3D[] array, int index)
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

	IEnumerator<GeneralTransform3D> IEnumerable<GeneralTransform3D>.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Add(System.Object)" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.Media.Media3D.GeneralTransform3DCollection" />.</param>
	int IList.Add(object value)
	{
		return AddHelper(Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Contains(System.Object)" />.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.Media.Media3D.GeneralTransform3DCollection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.Media3D.GeneralTransform3DCollection" />.</param>
	bool IList.Contains(object value)
	{
		return Contains(value as GeneralTransform3D);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.IndexOf(System.Object)" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.Media3D.GeneralTransform3DCollection" />.</param>
	int IList.IndexOf(object value)
	{
		return IndexOf(value as GeneralTransform3D);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Insert(System.Int32,System.Object)" />.</summary>
	/// <param name="index">The zero-based index at which to insert the <see cref="T:System.Object" />.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Windows.Media.Media3D.GeneralTransform3DCollection" />.</param>
	void IList.Insert(int index, object value)
	{
		Insert(index, Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Remove(System.Object)" />.</summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.Media.Media3D.GeneralTransform3DCollection" />.</param>
	void IList.Remove(object value)
	{
		Remove(value as GeneralTransform3D);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items from the <see cref="T:System.Windows.Media.Media3D.GeneralTransform3DCollection" />.</param>
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

	internal GeneralTransform3D Internal_GetItem(int i)
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

	private GeneralTransform3D Cast(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is GeneralTransform3D))
		{
			throw new ArgumentException(SR.Format(SR.Collection_BadType, GetType().Name, value.GetType().Name, "GeneralTransform3D"));
		}
		return (GeneralTransform3D)value;
	}

	private int AddHelper(GeneralTransform3D value)
	{
		int result = AddWithoutFiringPublicEvents(value);
		WritePostscript();
		return result;
	}

	internal int AddWithoutFiringPublicEvents(GeneralTransform3D value)
	{
		if (value == null)
		{
			throw new ArgumentException(SR.Collection_NoNull);
		}
		WritePreamble();
		OnFreezablePropertyChanged(null, value);
		int result = _collection.Add(value);
		_version++;
		return result;
	}

	protected override Freezable CreateInstanceCore()
	{
		return new GeneralTransform3DCollection();
	}

	protected override void CloneCore(Freezable source)
	{
		GeneralTransform3DCollection generalTransform3DCollection = (GeneralTransform3DCollection)source;
		base.CloneCore(source);
		int count = generalTransform3DCollection._collection.Count;
		_collection = new FrugalStructList<GeneralTransform3D>(count);
		for (int i = 0; i < count; i++)
		{
			GeneralTransform3D generalTransform3D = generalTransform3DCollection._collection[i].Clone();
			OnFreezablePropertyChanged(null, generalTransform3D);
			_collection.Add(generalTransform3D);
		}
	}

	protected override void CloneCurrentValueCore(Freezable source)
	{
		GeneralTransform3DCollection generalTransform3DCollection = (GeneralTransform3DCollection)source;
		base.CloneCurrentValueCore(source);
		int count = generalTransform3DCollection._collection.Count;
		_collection = new FrugalStructList<GeneralTransform3D>(count);
		for (int i = 0; i < count; i++)
		{
			GeneralTransform3D generalTransform3D = generalTransform3DCollection._collection[i].CloneCurrentValue();
			OnFreezablePropertyChanged(null, generalTransform3D);
			_collection.Add(generalTransform3D);
		}
	}

	protected override void GetAsFrozenCore(Freezable source)
	{
		GeneralTransform3DCollection generalTransform3DCollection = (GeneralTransform3DCollection)source;
		base.GetAsFrozenCore(source);
		int count = generalTransform3DCollection._collection.Count;
		_collection = new FrugalStructList<GeneralTransform3D>(count);
		for (int i = 0; i < count; i++)
		{
			GeneralTransform3D generalTransform3D = (GeneralTransform3D)generalTransform3DCollection._collection[i].GetAsFrozen();
			OnFreezablePropertyChanged(null, generalTransform3D);
			_collection.Add(generalTransform3D);
		}
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable source)
	{
		GeneralTransform3DCollection generalTransform3DCollection = (GeneralTransform3DCollection)source;
		base.GetCurrentValueAsFrozenCore(source);
		int count = generalTransform3DCollection._collection.Count;
		_collection = new FrugalStructList<GeneralTransform3D>(count);
		for (int i = 0; i < count; i++)
		{
			GeneralTransform3D generalTransform3D = (GeneralTransform3D)generalTransform3DCollection._collection[i].GetCurrentValueAsFrozen();
			OnFreezablePropertyChanged(null, generalTransform3D);
			_collection.Add(generalTransform3D);
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.GeneralTransform3DCollection" /> class.</summary>
	public GeneralTransform3DCollection()
	{
		_collection = default(FrugalStructList<GeneralTransform3D>);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.GeneralTransform3DCollection" /> class with the specified capacity, or the number of <see cref="T:System.Windows.Media.Media3D.GeneralTransform3D" /> objects the collection is initially capable of storing.</summary>
	/// <param name="capacity">The number of <see cref="T:System.Windows.Media.Media3D.GeneralTransform3D" /> objects that the collection is initially capable of storing.</param>
	public GeneralTransform3DCollection(int capacity)
	{
		_collection = new FrugalStructList<GeneralTransform3D>(capacity);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.GeneralTransform3DCollection" /> class, using the specified initial object.</summary>
	/// <param name="collection">Initial object in the new collection class.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="collection" /> is null.</exception>
	public GeneralTransform3DCollection(IEnumerable<GeneralTransform3D> collection)
	{
		WritePreamble();
		if (collection != null)
		{
			bool flag = true;
			if (collection is ICollection<GeneralTransform3D> collection2)
			{
				_collection = new FrugalStructList<GeneralTransform3D>(collection2);
			}
			else if (collection is ICollection collection3)
			{
				_collection = new FrugalStructList<GeneralTransform3D>(collection3);
			}
			else
			{
				_collection = default(FrugalStructList<GeneralTransform3D>);
				foreach (GeneralTransform3D item in collection)
				{
					GeneralTransform3D generalTransform3D = item ?? throw new ArgumentException(SR.Collection_NoNull);
					OnFreezablePropertyChanged(null, generalTransform3D);
					_collection.Add(generalTransform3D);
				}
				flag = false;
			}
			if (flag)
			{
				foreach (GeneralTransform3D item2 in collection)
				{
					if (item2 == null)
					{
						throw new ArgumentException(SR.Collection_NoNull);
					}
					OnFreezablePropertyChanged(null, item2);
				}
			}
			WritePostscript();
			return;
		}
		throw new ArgumentNullException("collection");
	}
}
