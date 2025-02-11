using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Media.Animation;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows;

/// <summary>Represents a collection of <see cref="T:System.Windows.DependencyObject" />, <see cref="T:System.Windows.Freezable" />, or <see cref="T:System.Windows.Media.Animation.Animatable" /> objects. <see cref="T:System.Windows.FreezableCollection`1" /> is itself an <see cref="T:System.Windows.Media.Animation.Animatable" /> type. </summary>
/// <typeparam name="T">The type of collection. This type must be a <see cref="T:System.Windows.DependencyObject" /> or a derived class.  </typeparam>
public class FreezableCollection<T> : Animatable, IList, ICollection, IEnumerable, IList<T>, ICollection<T>, IEnumerable<T>, INotifyCollectionChanged, INotifyPropertyChanged where T : DependencyObject
{
	private enum CloneCommonType
	{
		Clone,
		CloneCurrentValue,
		GetAsFrozen,
		GetCurrentValueAsFrozen
	}

	/// <summary>Enumerates the members of a <see cref="T:System.Windows.FreezableCollection`1" />.</summary>
	public struct Enumerator : IEnumerator, IEnumerator<T>, IDisposable
	{
		private T _current;

		private FreezableCollection<T> _list;

		private uint _version;

		private int _index;

		/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
		object IEnumerator.Current => Current;

		/// <summary>Gets a value that represents the current element in the collection.</summary>
		/// <returns>The current element in the collection.</returns>
		public T Current
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

		internal Enumerator(FreezableCollection<T> list)
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

		/// <summary>Advances the enumerator to the next element in the collection.</summary>
		/// <returns>true if the enumerator successfully advanced to the next element in the collection; otherwise, false.</returns>
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

		/// <summary>Resets the enumerator to its initial position.</summary>
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

	private class SimpleMonitor : IDisposable
	{
		private int _busyCount;

		public bool Busy => _busyCount > 0;

		public void Enter()
		{
			_busyCount++;
		}

		public void Dispose()
		{
			_busyCount--;
			GC.SuppressFinalize(this);
		}
	}

	internal List<T> _collection;

	internal uint _version;

	private const string CountPropertyName = "Count";

	private const string IndexerPropertyName = "Item[]";

	private SimpleMonitor _monitor = new SimpleMonitor();

	/// <summary>Gets or sets the element at the specified index.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the element to get or set.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0.- or -<paramref name="index" /> is equal to or greater than <see cref="P:System.Windows.FreezableCollection`1.Count" />.</exception>
	/// <exception cref="T:System.ArgumentException">The specified element is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">Attempting to set an item in the collection when the collection is frozen (its <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true).</exception>
	public T this[int index]
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
			CheckReentrancy();
			WritePreamble();
			T val = _collection[index];
			bool num = val != value;
			if (num)
			{
				OnFreezablePropertyChanged(val, value);
				_collection[index] = value;
			}
			_version++;
			WritePostscript();
			if (num)
			{
				OnCollectionChanged(NotifyCollectionChangedAction.Replace, index, val, index, value);
			}
		}
	}

	/// <summary>Gets the number of elements contained by this <see cref="T:System.Windows.FreezableCollection`1" />.</summary>
	/// <returns>The number of elements contained by this <see cref="T:System.Windows.FreezableCollection`1" />.</returns>
	public int Count
	{
		get
		{
			ReadPreamble();
			return _collection.Count;
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.Generic.ICollection`1.IsReadOnly" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.FreezableCollection`1" /> is read-only; otherwise, false.</returns>
	bool ICollection<T>.IsReadOnly
	{
		get
		{
			ReadPreamble();
			return base.IsFrozen;
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsReadOnly" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.FreezableCollection`1" /> is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => ((ICollection<T>)this).IsReadOnly;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsFixedSize" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.FreezableCollection`1" /> has a fixed size; otherwise, false.</returns>
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
	/// <returns>true if access to the <see cref="T:System.Windows.FreezableCollection`1" /> is synchronized (thread safe); otherwise, false.</returns>
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
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Windows.FreezableCollection`1" />.</returns>
	object ICollection.SyncRoot
	{
		get
		{
			ReadPreamble();
			return this;
		}
	}

	/// <summary>For a description of this member, see <see cref="E:System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged" />.</summary>
	event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
	{
		add
		{
			CollectionChanged += value;
		}
		remove
		{
			CollectionChanged -= value;
		}
	}

	private event NotifyCollectionChangedEventHandler CollectionChanged;

	/// <summary>For a description of this member, see <see cref="E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged" />.</summary>
	event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
	{
		add
		{
			PrivatePropertyChanged += value;
		}
		remove
		{
			PrivatePropertyChanged -= value;
		}
	}

	private event PropertyChangedEventHandler PrivatePropertyChanged;

	/// <summary>Initializes a new instance of <see cref="T:System.Windows.FreezableCollection`1" /> that is empty and has the default initial capacity.</summary>
	public FreezableCollection()
	{
		_collection = new List<T>();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.FreezableCollection`1" /> that is empty and has the specified initial capacity.</summary>
	/// <param name="capacity">A value that is greater than or equal to 0 that specifies the number of elements the new collection can initially store.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than 0.</exception>
	public FreezableCollection(int capacity)
	{
		_collection = new List<T>(capacity);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.FreezableCollection`1" /> class that contains the same elements as the specified collection.</summary>
	/// <param name="collection">The collection whose items should be added to the new <see cref="T:System.Windows.FreezableCollection`1" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="collection" /> is null.</exception>
	public FreezableCollection(IEnumerable<T> collection)
	{
		WritePreamble();
		if (collection != null)
		{
			int count = GetCount(collection);
			if (count > 0)
			{
				_collection = new List<T>(count);
			}
			else
			{
				_collection = new List<T>();
			}
			foreach (T item in collection)
			{
				if (item == null)
				{
					throw new ArgumentException(SR.Collection_NoNull);
				}
				OnFreezablePropertyChanged(null, item);
				_collection.Add(item);
			}
			WritePostscript();
			return;
		}
		throw new ArgumentNullException("collection");
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.FreezableCollection`1" /> and its contents, making deep copies. If this collection (or its contents) has animated dependency properties, the property’s base value is copied, not its current animated value.</summary>
	/// <returns>A modifiable copy of this collection and its contents. The copy's <see cref="P:System.Windows.Freezable.IsFrozen" /> value is false.</returns>
	public new FreezableCollection<T> Clone()
	{
		return (FreezableCollection<T>)base.Clone();
	}

	/// <summary>Creates a modifiable copy of this <see cref="T:System.Windows.FreezableCollection`1" /> and its contents, making deep copies of this object's current values. If this object (or the objects it contains) contains animated dependency properties, their current animated values are copied.</summary>
	/// <returns>A modifiable clone of the collection and its contents. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new FreezableCollection<T> CloneCurrentValue()
	{
		return (FreezableCollection<T>)base.CloneCurrentValue();
	}

	/// <summary>Adds the specified object to the end of the <see cref="T:System.Windows.FreezableCollection`1" />.</summary>
	/// <param name="value">The object to be added to the end of the <see cref="T:System.Windows.FreezableCollection`1" />. This value cannot be null.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.FreezableCollection`1" /> is frozen (its <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true).</exception>
	public void Add(T value)
	{
		AddHelper(value);
	}

	/// <summary>Removes all elements from the collection.</summary>
	public void Clear()
	{
		CheckReentrancy();
		WritePreamble();
		for (int num = _collection.Count - 1; num >= 0; num--)
		{
			OnFreezablePropertyChanged(_collection[num], null);
		}
		_collection.Clear();
		_version++;
		WritePostscript();
		OnCollectionChanged(NotifyCollectionChangedAction.Reset, 0, null, 0, null);
	}

	/// <summary>Determines whether this <see cref="T:System.Windows.FreezableCollection`1" /> contains the specified value.</summary>
	/// <returns>true if value is found in the <see cref="T:System.Windows.FreezableCollection`1" />; otherwise, false.</returns>
	/// <param name="value">The object to locate in this collection. This object may be null.</param>
	public bool Contains(T value)
	{
		ReadPreamble();
		return _collection.Contains(value);
	}

	/// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the entire <see cref="T:System.Windows.FreezableCollection`1" />.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="value" /> within the entire <see cref="T:System.Windows.FreezableCollection`1" />, if found; otherwise, -1.</returns>
	/// <param name="value">The object to locate in the <see cref="T:System.Windows.FreezableCollection`1" />.</param>
	public int IndexOf(T value)
	{
		ReadPreamble();
		return _collection.IndexOf(value);
	}

	/// <summary>Inserts the specified object into the <see cref="T:System.Windows.FreezableCollection`1" /> at the specified index.</summary>
	/// <param name="index">The zero-based index at which <paramref name="value" /> should be inserted.</param>
	/// <param name="value">The object to insert.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0.- or -<paramref name="index" /> is greater than <see cref="P:System.Windows.FreezableCollection`1.Count" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.FreezableCollection`1" /> is frozen (its <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true). </exception>
	public void Insert(int index, T value)
	{
		if (value == null)
		{
			throw new ArgumentException(SR.Collection_NoNull);
		}
		CheckReentrancy();
		WritePreamble();
		OnFreezablePropertyChanged(null, value);
		_collection.Insert(index, value);
		_version++;
		WritePostscript();
		OnCollectionChanged(NotifyCollectionChangedAction.Add, 0, null, index, value);
	}

	/// <summary>Removes the first occurrence of the specified object from the <see cref="T:System.Windows.FreezableCollection`1" />.</summary>
	/// <returns>true if an occurrence of <paramref name="value" /> was found in the collection and removed; false if <paramref name="value" /> could not be found in the collection.</returns>
	/// <param name="value">The object to remove.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.FreezableCollection`1" /> is frozen (its <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true).</exception>
	public bool Remove(T value)
	{
		WritePreamble();
		int num = IndexOf(value);
		if (num >= 0)
		{
			CheckReentrancy();
			T oldValue = _collection[num];
			OnFreezablePropertyChanged(oldValue, null);
			_collection.RemoveAt(num);
			_version++;
			WritePostscript();
			OnCollectionChanged(NotifyCollectionChangedAction.Remove, num, oldValue, 0, null);
			return true;
		}
		return false;
	}

	/// <summary>Removes the object at the specified zero-based index of the <see cref="T:System.Windows.FreezableCollection`1" />. </summary>
	/// <param name="index">The zero-based index of the object to remove.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0.- or -<paramref name="index" /> is greater than <see cref="P:System.Windows.FreezableCollection`1.Count" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.FreezableCollection`1" /> is frozen (its <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true).</exception>
	public void RemoveAt(int index)
	{
		T oldValue = _collection[index];
		RemoveAtWithoutFiringPublicEvents(index);
		WritePostscript();
		OnCollectionChanged(NotifyCollectionChangedAction.Remove, index, oldValue, 0, null);
	}

	internal void RemoveAtWithoutFiringPublicEvents(int index)
	{
		CheckReentrancy();
		WritePreamble();
		T oldValue = _collection[index];
		OnFreezablePropertyChanged(oldValue, null);
		_collection.RemoveAt(index);
		_version++;
	}

	/// <summary>Copies the entire <see cref="T:System.Windows.FreezableCollection`1" /> to a compatible one-dimensional array, starting at the specified index of the target array.</summary>
	/// <param name="array">The one-dimensional array that is the destination of the elements copied from <see cref="T:System.Windows.FreezableCollection`1" />.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0.</exception>
	/// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Windows.FreezableCollection`1" /> is greater than the available space from index to the end of the destination <paramref name="array" />.</exception>
	public void CopyTo(T[] array, int index)
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

	/// <summary>Returns an enumerator for the entire <see cref="T:System.Windows.FreezableCollection`1" />.</summary>
	/// <returns>An enumerator for the entire <see cref="T:System.Windows.FreezableCollection`1" />.</returns>
	public Enumerator GetEnumerator()
	{
		ReadPreamble();
		return new Enumerator(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.Generic.IEnumerable`1.GetEnumerator" />.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerator`1" /> object that can be used to iterate through the collection.</returns>
	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Add(System.Object)" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.FreezableCollection`1" />.</param>
	int IList.Add(object value)
	{
		return AddHelper(Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Contains(System.Object)" />.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.FreezableCollection`1" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.FreezableCollection`1" />.</param>
	bool IList.Contains(object value)
	{
		return Contains(value as T);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.IndexOf(System.Object)" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.FreezableCollection`1" />.</param>
	int IList.IndexOf(object value)
	{
		return IndexOf(value as T);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Insert(System.Int32,System.Object)" />.</summary>
	/// <param name="index">The zero-based index at which to insert the <see cref="T:System.Object" />.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Windows.FreezableCollection`1" />.</param>
	void IList.Insert(int index, object value)
	{
		Insert(index, Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Remove(System.Object)" />.</summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.FreezableCollection`1" />.</param>
	void IList.Remove(object value)
	{
		Remove(value as T);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items from the <see cref="T:System.Windows.FreezableCollection`1" />.</param>
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

	private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		if (this.CollectionChanged != null)
		{
			using (BlockReentrancy())
			{
				this.CollectionChanged(this, e);
			}
		}
	}

	private void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		if (this.PrivatePropertyChanged != null)
		{
			this.PrivatePropertyChanged(this, e);
		}
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

	private T Cast(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is T))
		{
			throw new ArgumentException(SR.Format(SR.Collection_BadType, GetType().Name, value.GetType().Name, "T"));
		}
		return (T)value;
	}

	private int GetCount(IEnumerable<T> enumerable)
	{
		if (enumerable is ICollection collection)
		{
			return collection.Count;
		}
		if (enumerable is ICollection<T> collection2)
		{
			return collection2.Count;
		}
		return -1;
	}

	private int AddHelper(T value)
	{
		CheckReentrancy();
		int num = AddWithoutFiringPublicEvents(value);
		WritePostscript();
		OnCollectionChanged(NotifyCollectionChangedAction.Add, 0, null, num - 1, value);
		return num;
	}

	internal int AddWithoutFiringPublicEvents(T value)
	{
		if (value == null)
		{
			throw new ArgumentException(SR.Collection_NoNull);
		}
		WritePreamble();
		OnFreezablePropertyChanged(null, value);
		_collection.Add(value);
		_version++;
		return _collection.Count;
	}

	private void OnCollectionChanged(NotifyCollectionChangedAction action, int oldIndex, T oldValue, int newIndex, T newValue)
	{
		if (this.PrivatePropertyChanged == null && this.CollectionChanged == null)
		{
			return;
		}
		using (BlockReentrancy())
		{
			if (this.PrivatePropertyChanged != null)
			{
				if (action != NotifyCollectionChangedAction.Replace && action != NotifyCollectionChangedAction.Move)
				{
					OnPropertyChanged(new PropertyChangedEventArgs("Count"));
				}
				OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
			}
			if (this.CollectionChanged != null)
			{
				OnCollectionChanged(action switch
				{
					NotifyCollectionChangedAction.Reset => new NotifyCollectionChangedEventArgs(action), 
					NotifyCollectionChangedAction.Add => new NotifyCollectionChangedEventArgs(action, newValue, newIndex), 
					NotifyCollectionChangedAction.Remove => new NotifyCollectionChangedEventArgs(action, oldValue, oldIndex), 
					NotifyCollectionChangedAction.Replace => new NotifyCollectionChangedEventArgs(action, newValue, oldValue, newIndex), 
					_ => throw new InvalidOperationException(SR.Freezable_UnexpectedChange), 
				});
			}
		}
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.FreezableCollection`1" />.</summary>
	/// <returns>The new instance.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new FreezableCollection<T>();
	}

	private void CloneCommon(FreezableCollection<T> source, CloneCommonType cloneType)
	{
		int count = source._collection.Count;
		_collection = new List<T>(count);
		for (int i = 0; i < count; i++)
		{
			T val = source._collection[i];
			if (val is Freezable freezable)
			{
				switch (cloneType)
				{
				case CloneCommonType.Clone:
					val = freezable.Clone() as T;
					break;
				case CloneCommonType.CloneCurrentValue:
					val = freezable.CloneCurrentValue() as T;
					break;
				case CloneCommonType.GetAsFrozen:
					val = freezable.GetAsFrozen() as T;
					break;
				case CloneCommonType.GetCurrentValueAsFrozen:
					val = freezable.GetCurrentValueAsFrozen() as T;
					break;
				default:
					Invariant.Assert(condition: false, "Invalid CloneCommonType encountered.");
					break;
				}
				if (val == null)
				{
					throw new InvalidOperationException(SR.Format(SR.Freezable_CloneInvalidType, typeof(T).Name));
				}
			}
			OnFreezablePropertyChanged(null, val);
			_collection.Add(val);
		}
	}

	/// <summary>Makes this instance a clone (deep copy) of the specified <see cref="T:System.Windows.FreezableCollection`1" /> using base (non-animated) property values.</summary>
	/// <param name="source">The <see cref="T:System.Windows.FreezableCollection`1" /> to copy.</param>
	protected override void CloneCore(Freezable source)
	{
		base.CloneCore(source);
		FreezableCollection<T> source2 = (FreezableCollection<T>)source;
		CloneCommon(source2, CloneCommonType.Clone);
	}

	/// <summary>Makes this instance a modifiable clone (deep copy) of the specified <see cref="T:System.Windows.FreezableCollection`1" /> using current property values.</summary>
	/// <param name="source">The <see cref="T:System.Windows.FreezableCollection`1" /> to clone.</param>
	protected override void CloneCurrentValueCore(Freezable source)
	{
		base.CloneCurrentValueCore(source);
		FreezableCollection<T> source2 = (FreezableCollection<T>)source;
		CloneCommon(source2, CloneCommonType.CloneCurrentValue);
	}

	/// <summary>Makes this instance a frozen clone of the specified <see cref="T:System.Windows.FreezableCollection`1" /> using base (non-animated) property values.</summary>
	/// <param name="source">The <see cref="T:System.Windows.FreezableCollection`1" /> to copy.</param>
	protected override void GetAsFrozenCore(Freezable source)
	{
		base.GetAsFrozenCore(source);
		FreezableCollection<T> source2 = (FreezableCollection<T>)source;
		CloneCommon(source2, CloneCommonType.GetAsFrozen);
	}

	/// <summary>Makes this instance a frozen clone of the specified <see cref="T:System.Windows.Freezable" />. If this object has animated dependency properties, their current animated values are copied.</summary>
	/// <param name="source">The <see cref="T:System.Windows.FreezableCollection`1" /> to copy.</param>
	protected override void GetCurrentValueAsFrozenCore(Freezable source)
	{
		base.GetCurrentValueAsFrozenCore(source);
		FreezableCollection<T> source2 = (FreezableCollection<T>)source;
		CloneCommon(source2, CloneCommonType.GetCurrentValueAsFrozen);
	}

	/// <summary>Makes this <see cref="T:System.Windows.FreezableCollection`1" /> object unmodifiable or determines whether it can be made unmodifiable.</summary>
	/// <returns>If <paramref name="isChecking" /> is true, this method returns true if this <see cref="T:System.Windows.FreezableCollection`1" /> can be made unmodifiable, or false if it cannot be made unmodifiable. If <paramref name="isChecking" /> is false, this method returns true if the if the specified <see cref="T:System.Windows.FreezableCollection`1" /> is now unmodifiable, or false if it cannot be made unmodifiable, with the side effect of having begun to change the frozen status of this object.</returns>
	/// <param name="isChecking">true if the <see cref="T:System.Windows.FreezableCollection`1" /> should simply return whether it can be frozen. false if the <see cref="T:System.Windows.FreezableCollection`1" /> instance should actually freeze itself when this method is called.</param>
	protected override bool FreezeCore(bool isChecking)
	{
		bool flag = base.FreezeCore(isChecking);
		int count = _collection.Count;
		for (int i = 0; i < count && flag; i++)
		{
			T val = _collection[i];
			flag = ((!(val is Freezable freezable)) ? (flag & (val.Dispatcher == null)) : (flag & Freezable.Freeze(freezable, isChecking)));
		}
		return flag;
	}

	private IDisposable BlockReentrancy()
	{
		_monitor.Enter();
		return _monitor;
	}

	private void CheckReentrancy()
	{
		if (_monitor.Busy)
		{
			throw new InvalidOperationException(SR.Freezable_Reentrant);
		}
	}
}
