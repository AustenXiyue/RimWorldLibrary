using System.Collections;
using System.Collections.Generic;
using System.Windows.Media.Animation;
using MS.Internal.PresentationCore;
using MS.Utility;

namespace System.Windows.Media;

/// <summary>Represents an ordered collection of <see cref="T:System.Windows.Media.GeneralTransform" /> objects. </summary>
public sealed class GeneralTransformCollection : Animatable, IList, ICollection, IEnumerable, IList<GeneralTransform>, ICollection<GeneralTransform>, IEnumerable<GeneralTransform>
{
	/// <summary>Enumerates <see cref="T:System.Windows.Media.GeneralTransform" /> items in a <see cref="T:System.Windows.Media.GeneralTransformCollection" />. </summary>
	public struct Enumerator : IEnumerator, IEnumerator<GeneralTransform>, IDisposable
	{
		private GeneralTransform _current;

		private GeneralTransformCollection _list;

		private uint _version;

		private int _index;

		/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
		object IEnumerator.Current => Current;

		/// <summary>Gets the current element in the collection.</summary>
		/// <returns>The current element in the collection.</returns>
		public GeneralTransform Current
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

		internal Enumerator(GeneralTransformCollection list)
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

	private static GeneralTransformCollection s_empty;

	internal FrugalStructList<GeneralTransform> _collection;

	internal uint _version;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.GeneralTransform" /> object at the specified index position. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.GeneralTransform" /> object at the <paramref name="index" /> position.</returns>
	/// <param name="index">The zero-based index position of the object to get or set.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the <see cref="T:System.Windows.Media.GeneralTransformCollection" />.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.GeneralTransformCollection" /> is read-only.- or -The <see cref="T:System.Windows.Media.GeneralTransformCollection" /> has a fixed size.</exception>
	public GeneralTransform this[int index]
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
				GeneralTransform oldValue = _collection[index];
				OnFreezablePropertyChanged(oldValue, value);
				_collection[index] = value;
			}
			_version++;
			WritePostscript();
		}
	}

	/// <summary>Gets the number of <see cref="T:System.Windows.Media.GeneralTransform" /> objects in the <see cref="T:System.Windows.Media.GeneralTransformCollection" />.</summary>
	/// <returns>The number of items in the collection.</returns>
	public int Count
	{
		get
		{
			ReadPreamble();
			return _collection.Count;
		}
	}

	bool ICollection<GeneralTransform>.IsReadOnly
	{
		get
		{
			ReadPreamble();
			return base.IsFrozen;
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsReadOnly" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.GeneralTransformCollection" /> is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => ((ICollection<GeneralTransform>)this).IsReadOnly;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsFixedSize" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.GeneralTransformCollection" /> has a fixed size; otherwise, false.</returns>
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
	/// <returns>true if access to the <see cref="T:System.Windows.Media.GeneralTransformCollection" /> is synchronized (thread safe); otherwise, false.</returns>
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
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Windows.Media.GeneralTransformCollection" />.</returns>
	object ICollection.SyncRoot
	{
		get
		{
			ReadPreamble();
			return this;
		}
	}

	internal static GeneralTransformCollection Empty
	{
		get
		{
			if (s_empty == null)
			{
				GeneralTransformCollection generalTransformCollection = new GeneralTransformCollection();
				generalTransformCollection.Freeze();
				s_empty = generalTransformCollection;
			}
			return s_empty;
		}
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.GeneralTransformCollection" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new GeneralTransformCollection Clone()
	{
		return (GeneralTransformCollection)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.GeneralTransformCollection" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new GeneralTransformCollection CloneCurrentValue()
	{
		return (GeneralTransformCollection)base.CloneCurrentValue();
	}

	/// <summary>Adds a <see cref="T:System.Windows.Media.GeneralTransform" /> object to the end of the <see cref="T:System.Windows.Media.GeneralTransformCollection" />.</summary>
	/// <param name="value">The <see cref="T:System.Windows.Media.GeneralTransform" /> to add to the end of the <see cref="T:System.Windows.Media.GeneralTransformCollection" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.GeneralTransformCollection" /> is read-only.- or -The <see cref="T:System.Windows.Media.GeneralTransformCollection" /> has a fixed size.</exception>
	public void Add(GeneralTransform value)
	{
		AddHelper(value);
	}

	/// <summary>Removes all <see cref="T:System.Windows.Media.GeneralTransform" /> objects from the <see cref="T:System.Windows.Media.GeneralTransformCollection" />.</summary>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.GeneralTransformCollection" /> is read-only.</exception>
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

	/// <summary>Indicates whether the <see cref="T:System.Windows.Media.GeneralTransformCollection" /> contains the specified <see cref="T:System.Windows.Media.GeneralTransform" /> object.</summary>
	/// <returns>true if the collection contains <paramref name="value" />; otherwise false.</returns>
	/// <param name="value">The <see cref="T:System.Windows.Media.GeneralTransform" /> to locate in the <see cref="T:System.Windows.Media.GeneralTransformCollection" />.</param>
	public bool Contains(GeneralTransform value)
	{
		ReadPreamble();
		return _collection.Contains(value);
	}

	/// <summary>Searches for the specified <see cref="T:System.Windows.Media.GeneralTransform" /> object within a <see cref="T:System.Windows.Media.GeneralTransformCollection" />. </summary>
	/// <returns>The zero-based index position of <paramref name="value" />, if found; otherwise -1;</returns>
	/// <param name="value">The object to locate.</param>
	public int IndexOf(GeneralTransform value)
	{
		ReadPreamble();
		return _collection.IndexOf(value);
	}

	/// <summary>Inserts a <see cref="T:System.Windows.Media.GeneralTransform" /> object into the specified index position within the <see cref="T:System.Windows.Media.GeneralTransformCollection" />.</summary>
	/// <param name="index">The zero-based index position to insert the object.</param>
	/// <param name="value">The object to insert.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the <see cref="T:System.Windows.Media.GeneralTransformCollection" />.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.GeneralTransformCollection" /> is read-only.- or -The <see cref="T:System.Windows.Media.GeneralTransformCollection" /> has a fixed size.</exception>
	public void Insert(int index, GeneralTransform value)
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

	/// <summary>Deletes a <see cref="T:System.Windows.Media.GeneralTransform" /> object from the <see cref="T:System.Windows.Media.GeneralTransformCollection" />.</summary>
	/// <returns>true if <paramref name="value" /> was successfully deleted; otherwise false.</returns>
	/// <param name="value">The object to remove.</param>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.GeneralTransformCollection" /> is read-only.- or -The <see cref="T:System.Windows.Media.GeneralTransformCollection" /> has a fixed size.</exception>
	public bool Remove(GeneralTransform value)
	{
		WritePreamble();
		int num = IndexOf(value);
		if (num >= 0)
		{
			GeneralTransform oldValue = _collection[num];
			OnFreezablePropertyChanged(oldValue, null);
			_collection.RemoveAt(num);
			_version++;
			WritePostscript();
			return true;
		}
		return false;
	}

	/// <summary>Deletes a <see cref="T:System.Windows.Media.GeneralTransform" /> object from the <see cref="T:System.Windows.Media.GeneralTransformCollection" />.</summary>
	/// <param name="index">The zero-based index position to remove the object.</param>
	public void RemoveAt(int index)
	{
		RemoveAtWithoutFiringPublicEvents(index);
		WritePostscript();
	}

	internal void RemoveAtWithoutFiringPublicEvents(int index)
	{
		WritePreamble();
		GeneralTransform oldValue = _collection[index];
		OnFreezablePropertyChanged(oldValue, null);
		_collection.RemoveAt(index);
		_version++;
	}

	/// <summary>Copies the <see cref="T:System.Windows.Media.GeneralTransform" /> objects in the collection into an array of GeneralTransforms, starting at the specified index position. </summary>
	/// <param name="array">The destination array.</param>
	/// <param name="index">The zero-based index position where copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional. -or-The number of items in the source <see cref="T:System.Windows.Media.GeneralTransformCollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
	public void CopyTo(GeneralTransform[] array, int index)
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

	IEnumerator<GeneralTransform> IEnumerable<GeneralTransform>.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Add(System.Object)" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.Media.GeneralTransformCollection" />.</param>
	int IList.Add(object value)
	{
		return AddHelper(Cast(value));
	}

	/// <summary>Determines whether the <see cref="T:System.Collections.IList" /> contains a specific value.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Collections.IList" />; otherwise, false.</returns>
	/// <param name="value">The object to locate in the <see cref="T:System.Collections.IList" />. </param>
	bool IList.Contains(object value)
	{
		return Contains(value as GeneralTransform);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.IndexOf(System.Object)" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.GeneralTransformCollection" />.</param>
	int IList.IndexOf(object value)
	{
		return IndexOf(value as GeneralTransform);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Insert(System.Int32,System.Object)" />.</summary>
	/// <param name="index">The zero-based index at which to insert the <see cref="T:System.Object" />.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Windows.Media.GeneralTransformCollection" />.</param>
	void IList.Insert(int index, object value)
	{
		Insert(index, Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Remove(System.Object)" />.</summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.Media.GeneralTransformCollection" />.</param>
	void IList.Remove(object value)
	{
		Remove(value as GeneralTransform);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items from the <see cref="T:System.Windows.Media.GeneralTransformCollection" />.</param>
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

	internal GeneralTransform Internal_GetItem(int i)
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

	private GeneralTransform Cast(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is GeneralTransform))
		{
			throw new ArgumentException(SR.Format(SR.Collection_BadType, GetType().Name, value.GetType().Name, "GeneralTransform"));
		}
		return (GeneralTransform)value;
	}

	private int AddHelper(GeneralTransform value)
	{
		int result = AddWithoutFiringPublicEvents(value);
		WritePostscript();
		return result;
	}

	internal int AddWithoutFiringPublicEvents(GeneralTransform value)
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
		return new GeneralTransformCollection();
	}

	protected override void CloneCore(Freezable source)
	{
		GeneralTransformCollection generalTransformCollection = (GeneralTransformCollection)source;
		base.CloneCore(source);
		int count = generalTransformCollection._collection.Count;
		_collection = new FrugalStructList<GeneralTransform>(count);
		for (int i = 0; i < count; i++)
		{
			GeneralTransform generalTransform = generalTransformCollection._collection[i].Clone();
			OnFreezablePropertyChanged(null, generalTransform);
			_collection.Add(generalTransform);
		}
	}

	protected override void CloneCurrentValueCore(Freezable source)
	{
		GeneralTransformCollection generalTransformCollection = (GeneralTransformCollection)source;
		base.CloneCurrentValueCore(source);
		int count = generalTransformCollection._collection.Count;
		_collection = new FrugalStructList<GeneralTransform>(count);
		for (int i = 0; i < count; i++)
		{
			GeneralTransform generalTransform = generalTransformCollection._collection[i].CloneCurrentValue();
			OnFreezablePropertyChanged(null, generalTransform);
			_collection.Add(generalTransform);
		}
	}

	protected override void GetAsFrozenCore(Freezable source)
	{
		GeneralTransformCollection generalTransformCollection = (GeneralTransformCollection)source;
		base.GetAsFrozenCore(source);
		int count = generalTransformCollection._collection.Count;
		_collection = new FrugalStructList<GeneralTransform>(count);
		for (int i = 0; i < count; i++)
		{
			GeneralTransform generalTransform = (GeneralTransform)generalTransformCollection._collection[i].GetAsFrozen();
			OnFreezablePropertyChanged(null, generalTransform);
			_collection.Add(generalTransform);
		}
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable source)
	{
		GeneralTransformCollection generalTransformCollection = (GeneralTransformCollection)source;
		base.GetCurrentValueAsFrozenCore(source);
		int count = generalTransformCollection._collection.Count;
		_collection = new FrugalStructList<GeneralTransform>(count);
		for (int i = 0; i < count; i++)
		{
			GeneralTransform generalTransform = (GeneralTransform)generalTransformCollection._collection[i].GetCurrentValueAsFrozen();
			OnFreezablePropertyChanged(null, generalTransform);
			_collection.Add(generalTransform);
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.GeneralTransformCollection" /> class.</summary>
	public GeneralTransformCollection()
	{
		_collection = default(FrugalStructList<GeneralTransform>);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.GeneralTransformCollection" /> class with the specified capacity, or the number of <see cref="T:System.Windows.Media.GeneralTransform" /> objects the collection is initially capable of storing.</summary>
	/// <param name="capacity">   The number of <see cref="T:System.Windows.Media.GeneralTransform" /> objects that the collection is initially capable of storing.</param>
	public GeneralTransformCollection(int capacity)
	{
		_collection = new FrugalStructList<GeneralTransform>(capacity);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.GeneralTransformCollection" /> class.</summary>
	/// <param name="collection">Initial object in the new collection class.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="collection" /> is null.</exception>
	public GeneralTransformCollection(IEnumerable<GeneralTransform> collection)
	{
		WritePreamble();
		if (collection != null)
		{
			bool flag = true;
			if (collection is ICollection<GeneralTransform> collection2)
			{
				_collection = new FrugalStructList<GeneralTransform>(collection2);
			}
			else if (collection is ICollection collection3)
			{
				_collection = new FrugalStructList<GeneralTransform>(collection3);
			}
			else
			{
				_collection = default(FrugalStructList<GeneralTransform>);
				foreach (GeneralTransform item in collection)
				{
					GeneralTransform generalTransform = item ?? throw new ArgumentException(SR.Collection_NoNull);
					OnFreezablePropertyChanged(null, generalTransform);
					_collection.Add(generalTransform);
				}
				flag = false;
			}
			if (flag)
			{
				foreach (GeneralTransform item2 in collection)
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
