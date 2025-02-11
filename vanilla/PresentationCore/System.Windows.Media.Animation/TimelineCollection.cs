using System.Collections;
using System.Collections.Generic;
using MS.Internal.PresentationCore;
using MS.Utility;

namespace System.Windows.Media.Animation;

/// <summary>Represents a collection of <see cref="T:System.Windows.Media.Animation.Timeline" /> objects. </summary>
public sealed class TimelineCollection : Animatable, IList, ICollection, IEnumerable, IList<Timeline>, ICollection<Timeline>, IEnumerable<Timeline>
{
	/// <summary>Enumerates the members of a <see cref="T:System.Windows.Media.Animation.TimelineCollection" />.</summary>
	public struct Enumerator : IEnumerator, IEnumerator<Timeline>, IDisposable
	{
		private Timeline _current;

		private TimelineCollection _list;

		private uint _version;

		private int _index;

		/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
		object IEnumerator.Current => Current;

		/// <summary>Gets a value that represents the current element in the collection.</summary>
		/// <returns>The current element in the collection.</returns>
		public Timeline Current
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

		internal Enumerator(TimelineCollection list)
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

	private static TimelineCollection s_empty;

	internal FrugalStructList<Timeline> _collection;

	internal uint _version;

	/// <summary> Gets or sets an item at the specified index position within this <see cref="T:System.Windows.Media.Animation.TimelineCollection" />. </summary>
	/// <returns>The Timeline object at the <paramref name="index" /> position.</returns>
	/// <param name="index">The index position to access.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than or equal to the size of the <see cref="T:System.Windows.Media.Animation.TimelineCollection" />.</exception>
	public Timeline this[int index]
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
				Timeline oldValue = _collection[index];
				OnFreezablePropertyChanged(oldValue, value);
				_collection[index] = value;
			}
			_version++;
			WritePostscript();
		}
	}

	/// <summary> Gets the number of items contained in this <see cref="T:System.Windows.Media.Animation.TimelineCollection" />.  </summary>
	/// <returns>The number of items contained in this instance. </returns>
	public int Count
	{
		get
		{
			ReadPreamble();
			return _collection.Count;
		}
	}

	bool ICollection<Timeline>.IsReadOnly
	{
		get
		{
			ReadPreamble();
			return base.IsFrozen;
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsReadOnly" />.</summary>
	/// <returns>true if the the <see cref="T:System.Windows.Media.Animation.TimelineCollection" /> is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => ((ICollection<Timeline>)this).IsReadOnly;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsFixedSize" />.</summary>
	/// <returns>true if the the <see cref="T:System.Windows.Media.Animation.TimelineCollection" /> has a fixed size; otherwise, false.</returns>
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
	/// <returns>true if access to the <see cref="T:System.Windows.Media.Animation.TimelineCollection" /> is synchronized (thread safe); otherwise, false.</returns>
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
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Windows.Media.Animation.TimelineCollection" />.</returns>
	object ICollection.SyncRoot
	{
		get
		{
			ReadPreamble();
			return this;
		}
	}

	internal static TimelineCollection Empty
	{
		get
		{
			if (s_empty == null)
			{
				TimelineCollection timelineCollection = new TimelineCollection();
				timelineCollection.Freeze();
				s_empty = timelineCollection;
			}
			return s_empty;
		}
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Animation.TimelineCollection" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new TimelineCollection Clone()
	{
		return (TimelineCollection)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Animation.TimelineCollection" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new TimelineCollection CloneCurrentValue()
	{
		return (TimelineCollection)base.CloneCurrentValue();
	}

	/// <summary>Inserts a new <see cref="T:System.Windows.Media.Animation.Timeline" /> object into the <see cref="T:System.Windows.Media.Animation.TimelineCollection" />. </summary>
	/// <param name="value">The object to add.</param>
	public void Add(Timeline value)
	{
		AddHelper(value);
	}

	/// <summary>Removes all items from the <see cref="T:System.Windows.Media.Animation.TimelineCollection" />. </summary>
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

	/// <summary>Determines whether the <see cref="T:System.Windows.Media.Animation.TimelineCollection" /> contains the specified <see cref="T:System.Windows.Media.Animation.Timeline" /> object. </summary>
	/// <returns>true if <paramref name="value" /> is found; otherwise, false.</returns>
	/// <param name="value">The object to locate.</param>
	public bool Contains(Timeline value)
	{
		ReadPreamble();
		return _collection.Contains(value);
	}

	/// <summary>Gets the zero-based index position of a Timeline object in the <see cref="T:System.Windows.Media.Animation.TimelineCollection" />. </summary>
	/// <returns>The index position of <paramref name="value" /> within this list.  If not found, -1 is returned.</returns>
	/// <param name="value">The object to locate.</param>
	public int IndexOf(Timeline value)
	{
		ReadPreamble();
		return _collection.IndexOf(value);
	}

	/// <summary>Inserts the specified Timeline object into this <see cref="T:System.Windows.Media.Animation.TimelineCollection" /> at the specified index position. </summary>
	/// <param name="index">The zero-based index position into which <paramref name="value" /> is inserted.</param>
	/// <param name="value">The object to insert.</param>
	public void Insert(int index, Timeline value)
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

	/// <summary>Removes the first occurrence of a specified <see cref="T:System.Windows.Media.Animation.Timeline" /> from this <see cref="T:System.Windows.Media.Animation.TimelineCollection" />. </summary>
	/// <returns>true if the operation was successful; otherwise, false.</returns>
	/// <param name="value">The object to remove.</param>
	public bool Remove(Timeline value)
	{
		WritePreamble();
		int num = IndexOf(value);
		if (num >= 0)
		{
			Timeline oldValue = _collection[num];
			OnFreezablePropertyChanged(oldValue, null);
			_collection.RemoveAt(num);
			_version++;
			WritePostscript();
			return true;
		}
		return false;
	}

	/// <summary> Removes the <see cref="T:System.Windows.Media.Animation.Timeline" /> at the specified index position from this <see cref="T:System.Windows.Media.Animation.TimelineCollection" />. </summary>
	/// <param name="index">The zero-based index position of the item to remove.</param>
	public void RemoveAt(int index)
	{
		RemoveAtWithoutFiringPublicEvents(index);
		WritePostscript();
	}

	internal void RemoveAtWithoutFiringPublicEvents(int index)
	{
		WritePreamble();
		Timeline oldValue = _collection[index];
		OnFreezablePropertyChanged(oldValue, null);
		_collection.RemoveAt(index);
		_version++;
	}

	/// <summary>Copies the items of the <see cref="T:System.Windows.Media.Animation.TimelineCollection" /> to the passed Timeline array, starting at the specified index position. </summary>
	/// <param name="array">The destination array.</param>
	/// <param name="index">The zero-based index position where copying begins.</param>
	public void CopyTo(Timeline[] array, int index)
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

	/// <summary>Gets an enumerator that can iterate the members of the collection.</summary>
	/// <returns>An object that can iterate the members of the collection.</returns>
	public Enumerator GetEnumerator()
	{
		ReadPreamble();
		return new Enumerator(this);
	}

	IEnumerator<Timeline> IEnumerable<Timeline>.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Add(System.Object)" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.Media.Animation.TimelineCollection" />.</param>
	int IList.Add(object value)
	{
		return AddHelper(Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Contains(System.Object)" />.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.Media.Animation.TimelineCollection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.Animation.TimelineCollection" />.</param>
	bool IList.Contains(object value)
	{
		return Contains(value as Timeline);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.IndexOf(System.Object)" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.Animation.TimelineCollection" />.</param>
	int IList.IndexOf(object value)
	{
		return IndexOf(value as Timeline);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Insert(System.Int32,System.Object)" />.</summary>
	/// <param name="index">The zero-based index at which to insert the <see cref="T:System.Object" />.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Windows.Media.Animation.TimelineCollection" />.</param>
	void IList.Insert(int index, object value)
	{
		Insert(index, Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Remove(System.Object)" />.</summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.Media.Animation.TimelineCollection" />.</param>
	void IList.Remove(object value)
	{
		Remove(value as Timeline);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items from the <see cref="T:System.Windows.Media.Animation.TimelineCollection" />.</param>
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

	internal Timeline Internal_GetItem(int i)
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

	private Timeline Cast(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is Timeline))
		{
			throw new ArgumentException(SR.Format(SR.Collection_BadType, GetType().Name, value.GetType().Name, "Timeline"));
		}
		return (Timeline)value;
	}

	private int AddHelper(Timeline value)
	{
		int result = AddWithoutFiringPublicEvents(value);
		WritePostscript();
		return result;
	}

	internal int AddWithoutFiringPublicEvents(Timeline value)
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
		return new TimelineCollection();
	}

	protected override void CloneCore(Freezable source)
	{
		TimelineCollection timelineCollection = (TimelineCollection)source;
		base.CloneCore(source);
		int count = timelineCollection._collection.Count;
		_collection = new FrugalStructList<Timeline>(count);
		for (int i = 0; i < count; i++)
		{
			Timeline timeline = timelineCollection._collection[i].Clone();
			OnFreezablePropertyChanged(null, timeline);
			_collection.Add(timeline);
		}
	}

	protected override void CloneCurrentValueCore(Freezable source)
	{
		TimelineCollection timelineCollection = (TimelineCollection)source;
		base.CloneCurrentValueCore(source);
		int count = timelineCollection._collection.Count;
		_collection = new FrugalStructList<Timeline>(count);
		for (int i = 0; i < count; i++)
		{
			Timeline timeline = timelineCollection._collection[i].CloneCurrentValue();
			OnFreezablePropertyChanged(null, timeline);
			_collection.Add(timeline);
		}
	}

	protected override void GetAsFrozenCore(Freezable source)
	{
		TimelineCollection timelineCollection = (TimelineCollection)source;
		base.GetAsFrozenCore(source);
		int count = timelineCollection._collection.Count;
		_collection = new FrugalStructList<Timeline>(count);
		for (int i = 0; i < count; i++)
		{
			Timeline timeline = (Timeline)timelineCollection._collection[i].GetAsFrozen();
			OnFreezablePropertyChanged(null, timeline);
			_collection.Add(timeline);
		}
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable source)
	{
		TimelineCollection timelineCollection = (TimelineCollection)source;
		base.GetCurrentValueAsFrozenCore(source);
		int count = timelineCollection._collection.Count;
		_collection = new FrugalStructList<Timeline>(count);
		for (int i = 0; i < count; i++)
		{
			Timeline timeline = (Timeline)timelineCollection._collection[i].GetCurrentValueAsFrozen();
			OnFreezablePropertyChanged(null, timeline);
			_collection.Add(timeline);
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.TimelineCollection" /> class.</summary>
	public TimelineCollection()
	{
		_collection = default(FrugalStructList<Timeline>);
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Windows.Media.Animation.TimelineCollection" /> with the specified initial capacity.</summary>
	/// <param name="capacity">The initial capacity of the <see cref="T:System.Windows.Media.Animation.TimelineCollection" />.</param>
	public TimelineCollection(int capacity)
	{
		_collection = new FrugalStructList<Timeline>(capacity);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.TimelineCollection" /> class that includes all of the same elements as an existing collection.</summary>
	/// <param name="collection">Collection of elements this instance is based on.</param>
	/// <exception cref="T:System.ArgumentException">Occurs when <paramref name="collection" /> is empty.</exception>
	public TimelineCollection(IEnumerable<Timeline> collection)
	{
		WritePreamble();
		if (collection != null)
		{
			bool flag = true;
			if (collection is ICollection<Timeline> collection2)
			{
				_collection = new FrugalStructList<Timeline>(collection2);
			}
			else if (collection is ICollection collection3)
			{
				_collection = new FrugalStructList<Timeline>(collection3);
			}
			else
			{
				_collection = default(FrugalStructList<Timeline>);
				foreach (Timeline item in collection)
				{
					Timeline timeline = item ?? throw new ArgumentException(SR.Collection_NoNull);
					OnFreezablePropertyChanged(null, timeline);
					_collection.Add(timeline);
				}
				flag = false;
			}
			if (flag)
			{
				foreach (Timeline item2 in collection)
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
