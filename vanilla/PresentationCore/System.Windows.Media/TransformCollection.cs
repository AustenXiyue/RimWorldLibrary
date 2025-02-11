using System.Collections;
using System.Collections.Generic;
using System.Windows.Media.Animation;
using MS.Internal.Collections;
using MS.Internal.PresentationCore;
using MS.Utility;

namespace System.Windows.Media;

/// <summary> Represents a collection of <see cref="T:System.Windows.Media.Transform" /> objects that can be individually accessed by index. </summary>
public sealed class TransformCollection : Animatable, IList, ICollection, IEnumerable, IList<Transform>, ICollection<Transform>, IEnumerable<Transform>
{
	/// <summary>Supports a simple iteration over a <see cref="T:System.Windows.Media.TransformCollection" />. </summary>
	public struct Enumerator : IEnumerator, IEnumerator<Transform>, IDisposable
	{
		private Transform _current;

		private TransformCollection _list;

		private uint _version;

		private int _index;

		/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
		object IEnumerator.Current => Current;

		/// <summary>Gets the current item in the <see cref="T:System.Windows.Media.TransformCollection" />.</summary>
		/// <returns>The current item in the <see cref="T:System.Windows.Media.TransformCollection" />.</returns>
		/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Media.TransformCollection" /> was modified after the enumerator was created.</exception>
		public Transform Current
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

		internal Enumerator(TransformCollection list)
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
		/// <returns>true if the enumerator successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
		/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Media.TransformCollection" /> was modified after the enumerator was created.</exception>
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

		/// <summary>Resets the enumerator to its initial position, which is before the first item in the <see cref="T:System.Windows.Media.TransformCollection" />. </summary>
		/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Media.TransformCollection" /> was modified after the enumerator was created.</exception>
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

	private static TransformCollection s_empty;

	internal FrugalStructList<Transform> _collection;

	internal uint _version;

	/// <summary> Gets or sets the <see cref="T:System.Windows.Media.Transform" /> at the specified index. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Transform" /> at the specified index.</returns>
	/// <param name="index">The zero-based index of the <see cref="T:System.Windows.Media.Transform" /> to get or set.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the <see cref="T:System.Windows.Media.TransformCollection" />.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.TransformCollection" /> is read-only.- or -The <see cref="T:System.Windows.Media.TransformCollection" /> has a fixed size.</exception>
	public Transform this[int index]
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
				Transform oldValue = _collection[index];
				OnFreezablePropertyChanged(oldValue, value);
				_collection[index] = value;
				OnSet(oldValue, value);
			}
			_version++;
			WritePostscript();
		}
	}

	/// <summary> Gets the number of items contained in the <see cref="T:System.Windows.Media.TransformCollection" />. </summary>
	/// <returns>The number of items in the <see cref="T:System.Windows.Media.TransformCollection" />.</returns>
	public int Count
	{
		get
		{
			ReadPreamble();
			return _collection.Count;
		}
	}

	bool ICollection<Transform>.IsReadOnly
	{
		get
		{
			ReadPreamble();
			return base.IsFrozen;
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsReadOnly" />.</summary>
	/// <returns>true if the the <see cref="T:System.Windows.Media.TransformCollection" /> is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => ((ICollection<Transform>)this).IsReadOnly;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsFixedSize" />.</summary>
	/// <returns>true if the the <see cref="T:System.Windows.Media.TransformCollection" /> has a fixed size; otherwise, false.</returns>
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
	/// <returns>true if access to the <see cref="T:System.Windows.Media.TransformCollection" /> is synchronized (thread safe); otherwise, false.</returns>
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
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Windows.Media.TransformCollection" />.</returns>
	object ICollection.SyncRoot
	{
		get
		{
			ReadPreamble();
			return this;
		}
	}

	internal static TransformCollection Empty
	{
		get
		{
			if (s_empty == null)
			{
				TransformCollection transformCollection = new TransformCollection();
				transformCollection.Freeze();
				s_empty = transformCollection;
			}
			return s_empty;
		}
	}

	internal event ItemInsertedHandler ItemInserted;

	internal event ItemRemovedHandler ItemRemoved;

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.TransformCollection" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new TransformCollection Clone()
	{
		return (TransformCollection)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.TransformCollection" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new TransformCollection CloneCurrentValue()
	{
		return (TransformCollection)base.CloneCurrentValue();
	}

	/// <summary>Adds a <see cref="T:System.Windows.Media.Transform" /> to the end of the <see cref="T:System.Windows.Media.TransformCollection" />. </summary>
	/// <param name="value">The <see cref="T:System.Windows.Media.Transform" /> to add to the end of the <see cref="T:System.Windows.Media.TransformCollection" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.TransformCollection" /> is read-only.- or -The <see cref="T:System.Windows.Media.TransformCollection" /> has a fixed size.</exception>
	public void Add(Transform value)
	{
		AddHelper(value);
	}

	/// <summary>Removes all items from the <see cref="T:System.Windows.Media.TransformCollection" />. </summary>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.TransformCollection" /> is read-only.</exception>
	public void Clear()
	{
		WritePreamble();
		FrugalStructList<Transform> collection = _collection;
		_collection = new FrugalStructList<Transform>(_collection.Capacity);
		for (int num = collection.Count - 1; num >= 0; num--)
		{
			OnFreezablePropertyChanged(collection[num], null);
			OnRemove(collection[num]);
		}
		_version++;
		WritePostscript();
	}

	/// <summary>Determines whether the <see cref="T:System.Windows.Media.TransformCollection" /> contains the specified <see cref="T:System.Windows.Media.Transform" />. </summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.Transform" /> is found in the <see cref="T:System.Windows.Media.TransformCollection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Windows.Media.Transform" /> to locate in the <see cref="T:System.Windows.Media.TransformCollection" />. </param>
	public bool Contains(Transform value)
	{
		ReadPreamble();
		return _collection.Contains(value);
	}

	/// <summary>Determines the index of the specified item in the <see cref="T:System.Windows.Media.TransformCollection" />. </summary>
	/// <returns>The index of <paramref name="value" /> if found in the <see cref="T:System.Windows.Media.TransformCollection" />; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Windows.Media.Transform" /> to locate in the <see cref="T:System.Windows.Media.TransformCollection" />.</param>
	public int IndexOf(Transform value)
	{
		ReadPreamble();
		return _collection.IndexOf(value);
	}

	/// <summary> Inserts a <see cref="T:System.Windows.Media.Transform" /> into the <see cref="T:System.Windows.Media.TransformCollection" /> at the specified index. </summary>
	/// <param name="index">The zero-based index at which <paramref name="value" /> should be inserted.</param>
	/// <param name="value">The <see cref="T:System.Windows.Media.Transform" /> to insert into the <see cref="T:System.Windows.Media.TransformCollection" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the <see cref="T:System.Windows.Media.TransformCollection" />.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.TransformCollection" /> is read-only.- or -The <see cref="T:System.Windows.Media.TransformCollection" /> has a fixed size.</exception>
	public void Insert(int index, Transform value)
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

	/// <summary>Removes the first occurrence of the specified <see cref="T:System.Windows.Media.Transform" /> from the <see cref="T:System.Windows.Media.TransformCollection" />. </summary>
	/// <returns>true if <paramref name="value" /> was removed from the <see cref="T:System.Windows.Media.TransformCollection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Windows.Media.Transform" /> to remove from the <see cref="T:System.Windows.Media.TransformCollection" />.</param>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.TransformCollection" /> is read-only.- or -The <see cref="T:System.Windows.Media.TransformCollection" /> has a fixed size.</exception>
	public bool Remove(Transform value)
	{
		WritePreamble();
		int num = IndexOf(value);
		if (num >= 0)
		{
			Transform oldValue = _collection[num];
			OnFreezablePropertyChanged(oldValue, null);
			_collection.RemoveAt(num);
			OnRemove(oldValue);
			_version++;
			WritePostscript();
			return true;
		}
		return false;
	}

	/// <summary> Removes the <see cref="T:System.Windows.Media.Transform" /> at the specified index. </summary>
	/// <param name="index">The zero-based index of the <see cref="T:System.Windows.Media.Transform" /> to remove.</param>
	public void RemoveAt(int index)
	{
		RemoveAtWithoutFiringPublicEvents(index);
		WritePostscript();
	}

	internal void RemoveAtWithoutFiringPublicEvents(int index)
	{
		WritePreamble();
		Transform oldValue = _collection[index];
		OnFreezablePropertyChanged(oldValue, null);
		_collection.RemoveAt(index);
		OnRemove(oldValue);
		_version++;
	}

	/// <summary>Copies the items of the <see cref="T:System.Windows.Media.TransformCollection" /> to an array, starting at the specified array index. </summary>
	/// <param name="array">The one-dimensional array that is the destination of the items copied from the <see cref="T:System.Windows.Media.TransformCollection" />. The array must have zero-based indexing.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional. -or-The number of items in the source <see cref="T:System.Windows.Media.TransformCollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
	public void CopyTo(Transform[] array, int index)
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

	/// <summary>Returns an enumerator that can iterate through the <see cref="T:System.Windows.Media.TransformCollection" />. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.TransformCollection.Enumerator" /> that can be used to iterate through the <see cref="T:System.Windows.Media.TransformCollection" />.</returns>
	public Enumerator GetEnumerator()
	{
		ReadPreamble();
		return new Enumerator(this);
	}

	IEnumerator<Transform> IEnumerable<Transform>.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Add(System.Object)" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.Media.TransformCollection" />.</param>
	int IList.Add(object value)
	{
		return AddHelper(Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Contains(System.Object)" />.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.Media.TransformCollection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.TransformCollection" />.</param>
	bool IList.Contains(object value)
	{
		return Contains(value as Transform);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.IndexOf(System.Object)" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.TransformCollection" />.</param>
	int IList.IndexOf(object value)
	{
		return IndexOf(value as Transform);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Insert(System.Int32,System.Object)" />.</summary>
	/// <param name="index">The zero-based index at which to insert the <see cref="T:System.Object" />.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Windows.Media.TransformCollection" />.</param>
	void IList.Insert(int index, object value)
	{
		Insert(index, Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Remove(System.Object)" />.</summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.Media.TransformCollection" />.</param>
	void IList.Remove(object value)
	{
		Remove(value as Transform);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items from the <see cref="T:System.Windows.Media.TransformCollection" />.</param>
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

	internal Transform Internal_GetItem(int i)
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

	private Transform Cast(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is Transform))
		{
			throw new ArgumentException(SR.Format(SR.Collection_BadType, GetType().Name, value.GetType().Name, "Transform"));
		}
		return (Transform)value;
	}

	private int AddHelper(Transform value)
	{
		int result = AddWithoutFiringPublicEvents(value);
		WritePostscript();
		return result;
	}

	internal int AddWithoutFiringPublicEvents(Transform value)
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
		return new TransformCollection();
	}

	protected override void CloneCore(Freezable source)
	{
		TransformCollection transformCollection = (TransformCollection)source;
		base.CloneCore(source);
		int count = transformCollection._collection.Count;
		_collection = new FrugalStructList<Transform>(count);
		for (int i = 0; i < count; i++)
		{
			Transform transform = transformCollection._collection[i].Clone();
			OnFreezablePropertyChanged(null, transform);
			_collection.Add(transform);
			OnInsert(transform);
		}
	}

	protected override void CloneCurrentValueCore(Freezable source)
	{
		TransformCollection transformCollection = (TransformCollection)source;
		base.CloneCurrentValueCore(source);
		int count = transformCollection._collection.Count;
		_collection = new FrugalStructList<Transform>(count);
		for (int i = 0; i < count; i++)
		{
			Transform transform = transformCollection._collection[i].CloneCurrentValue();
			OnFreezablePropertyChanged(null, transform);
			_collection.Add(transform);
			OnInsert(transform);
		}
	}

	protected override void GetAsFrozenCore(Freezable source)
	{
		TransformCollection transformCollection = (TransformCollection)source;
		base.GetAsFrozenCore(source);
		int count = transformCollection._collection.Count;
		_collection = new FrugalStructList<Transform>(count);
		for (int i = 0; i < count; i++)
		{
			Transform transform = (Transform)transformCollection._collection[i].GetAsFrozen();
			OnFreezablePropertyChanged(null, transform);
			_collection.Add(transform);
			OnInsert(transform);
		}
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable source)
	{
		TransformCollection transformCollection = (TransformCollection)source;
		base.GetCurrentValueAsFrozenCore(source);
		int count = transformCollection._collection.Count;
		_collection = new FrugalStructList<Transform>(count);
		for (int i = 0; i < count; i++)
		{
			Transform transform = (Transform)transformCollection._collection[i].GetCurrentValueAsFrozen();
			OnFreezablePropertyChanged(null, transform);
			_collection.Add(transform);
			OnInsert(transform);
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

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.TransformCollection" /> class. </summary>
	public TransformCollection()
	{
		_collection = default(FrugalStructList<Transform>);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TransformCollection" /> class with the specified capacity.</summary>
	/// <param name="capacity">The number of <see cref="T:System.Windows.Media.Transform" /> objects that the collection is initially capable of storing.</param>
	public TransformCollection(int capacity)
	{
		_collection = new FrugalStructList<Transform>(capacity);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TransformCollection" /> class that contains items copied from the specified collection of <see cref="T:System.Windows.Media.Transform" /> objects and has the same initial capacity as the number of items copied.</summary>
	/// <param name="collection">The collection whose items are copied to the new <see cref="T:System.Windows.Media.TransformCollection" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="collection" /> is null.</exception>
	public TransformCollection(IEnumerable<Transform> collection)
	{
		WritePreamble();
		if (collection != null)
		{
			bool flag = true;
			if (collection is ICollection<Transform> collection2)
			{
				_collection = new FrugalStructList<Transform>(collection2);
			}
			else if (collection is ICollection collection3)
			{
				_collection = new FrugalStructList<Transform>(collection3);
			}
			else
			{
				_collection = default(FrugalStructList<Transform>);
				foreach (Transform item in collection)
				{
					Transform transform = item ?? throw new ArgumentException(SR.Collection_NoNull);
					OnFreezablePropertyChanged(null, transform);
					_collection.Add(transform);
					OnInsert(transform);
				}
				flag = false;
			}
			if (flag)
			{
				foreach (Transform item2 in collection)
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
