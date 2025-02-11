using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Animation;
using MS.Internal.PresentationCore;
using MS.Utility;

namespace System.Windows.Media;

/// <summary>Represents a collection of <see cref="T:System.Windows.Media.PathSegment" /> objects that can be individually accessed by index.</summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public sealed class PathSegmentCollection : Animatable, IList, ICollection, IEnumerable, IList<PathSegment>, ICollection<PathSegment>, IEnumerable<PathSegment>
{
	/// <summary>Supports a simple iteration over a <see cref="T:System.Windows.Media.PathSegmentCollection" />. </summary>
	public struct Enumerator : IEnumerator, IEnumerator<PathSegment>, IDisposable
	{
		private PathSegment _current;

		private PathSegmentCollection _list;

		private uint _version;

		private int _index;

		/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
		object IEnumerator.Current => Current;

		/// <summary>Gets the current item in the <see cref="T:System.Windows.Media.PathSegmentCollection" />.</summary>
		/// <returns>The current item in the <see cref="T:System.Windows.Media.PathSegmentCollection" />.</returns>
		/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Media.PathSegmentCollection" /> was modified after the enumerator was created.</exception>
		public PathSegment Current
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

		internal Enumerator(PathSegmentCollection list)
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
		/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Media.PathSegmentCollection" /> was modified after the enumerator was created.</exception>
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

		/// <summary>Resets the enumerator to its initial position, which is before the first item in the <see cref="T:System.Windows.Media.PathSegmentCollection" />. </summary>
		/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Media.PathSegmentCollection" /> was modified after the enumerator was created.</exception>
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

	private static PathSegmentCollection s_empty;

	internal FrugalStructList<PathSegment> _collection;

	internal uint _version;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.PathSegment" /> at the specified zero-based index. </summary>
	/// <returns>The item at the specified index.</returns>
	/// <param name="index">The zero-based index of the <see cref="T:System.Windows.Media.PathSegment" /> object to get or set.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the <see cref="T:System.Windows.Media.PathSegmentCollection" />.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.PathSegmentCollection" /> is read-only.- or -The <see cref="T:System.Windows.Media.PathSegmentCollection" /> has a fixed size.</exception>
	public PathSegment this[int index]
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
				PathSegment oldValue = _collection[index];
				OnFreezablePropertyChanged(oldValue, value);
				_collection[index] = value;
			}
			_version++;
			WritePostscript();
		}
	}

	/// <summary> Gets the number of path segments contained in the <see cref="T:System.Windows.Media.PathSegmentCollection" />.  </summary>
	/// <returns>The number of path segments contained in the <see cref="T:System.Windows.Media.PathSegmentCollection" />. </returns>
	public int Count
	{
		get
		{
			ReadPreamble();
			return _collection.Count;
		}
	}

	bool ICollection<PathSegment>.IsReadOnly
	{
		get
		{
			ReadPreamble();
			return base.IsFrozen;
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsReadOnly" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.PathSegmentCollection" /> is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => ((ICollection<PathSegment>)this).IsReadOnly;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsFixedSize" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.PathSegmentCollection" /> has a fixed size; otherwise, false.</returns>
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
	/// <returns>true if access to the <see cref="T:System.Windows.Media.PathSegmentCollection" /> is synchronized (thread safe); otherwise, false.</returns>
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
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Windows.Media.PathSegmentCollection" />.</returns>
	object ICollection.SyncRoot
	{
		get
		{
			ReadPreamble();
			return this;
		}
	}

	internal static PathSegmentCollection Empty
	{
		get
		{
			if (s_empty == null)
			{
				PathSegmentCollection pathSegmentCollection = new PathSegmentCollection();
				pathSegmentCollection.Freeze();
				s_empty = pathSegmentCollection;
			}
			return s_empty;
		}
	}

	internal bool CanSerializeToString()
	{
		bool result = true;
		for (int i = 0; i < _collection.Count; i++)
		{
			if (!_collection[i].IsStroked)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	internal string ConvertToString(string format, IFormatProvider provider)
	{
		if (_collection.Count == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < _collection.Count; i++)
		{
			stringBuilder.Append(_collection[i].ConvertToString(format, provider));
		}
		return stringBuilder.ToString();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.PathSegmentCollection" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new PathSegmentCollection Clone()
	{
		return (PathSegmentCollection)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.PathSegmentCollection" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new PathSegmentCollection CloneCurrentValue()
	{
		return (PathSegmentCollection)base.CloneCurrentValue();
	}

	/// <summary>Adds a <see cref="T:System.Windows.Media.PathSegment" /> to the end of the collection. </summary>
	/// <param name="value">The segment to add to the collection.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.PathSegmentCollection" /> is read-only.- or -The <see cref="T:System.Windows.Media.PathSegmentCollection" /> has a fixed size.</exception>
	public void Add(PathSegment value)
	{
		AddHelper(value);
	}

	/// <summary> Clears the collection of all segments and resets <see cref="P:System.Windows.Media.PathSegmentCollection.Count" /> to zero. </summary>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.PathSegmentCollection" /> is read-only.</exception>
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

	/// <summary> Returns a <see cref="T:System.Boolean" /> that indicates whether the specified <see cref="T:System.Windows.Media.PathSegment" /> is contained within the collection. </summary>
	/// <returns>true if the specified <see cref="T:System.Windows.Media.PathSegment" /> is contained within the collection; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Windows.Media.PathSegment" /> to search for.</param>
	public bool Contains(PathSegment value)
	{
		ReadPreamble();
		return _collection.Contains(value);
	}

	/// <summary>Returns the index of the first occurrence of the specified <see cref="T:System.Windows.Media.PathSegment" />. </summary>
	/// <returns>The index of the specified <see cref="T:System.Windows.Media.PathSegment" />.</returns>
	/// <param name="value">The item to search for.</param>
	public int IndexOf(PathSegment value)
	{
		ReadPreamble();
		return _collection.IndexOf(value);
	}

	/// <summary>Inserts a <see cref="T:System.Windows.Media.PathSegment" /> into this <see cref="T:System.Windows.Media.PathSegmentCollection" /> at the specified index.</summary>
	/// <param name="index">The index at which to insert <paramref name="value" />, the specified <see cref="T:System.Windows.Media.PathSegment" />.</param>
	/// <param name="value">The item to insert.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the <see cref="T:System.Windows.Media.PathSegmentCollection" />.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.PathSegmentCollection" /> is read-only.- or -The <see cref="T:System.Windows.Media.PathSegmentCollection" /> has a fixed size.</exception>
	public void Insert(int index, PathSegment value)
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

	/// <summary>Removes the first occurrence of the specified <see cref="T:System.Windows.Media.PathSegment" /> from this <see cref="T:System.Windows.Media.PathSegmentCollection" />.</summary>
	/// <returns>true if the specified <see cref="T:System.Windows.Media.PathSegment" /> is removed from the collection; otherwise, false.</returns>
	/// <param name="value">The item to remove from this collection.</param>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.PathSegmentCollection" /> is read-only.- or -The <see cref="T:System.Windows.Media.PathSegmentCollection" /> has a fixed size.</exception>
	public bool Remove(PathSegment value)
	{
		WritePreamble();
		int num = IndexOf(value);
		if (num >= 0)
		{
			PathSegment oldValue = _collection[num];
			OnFreezablePropertyChanged(oldValue, null);
			_collection.RemoveAt(num);
			_version++;
			WritePostscript();
			return true;
		}
		return false;
	}

	/// <summary>Removes the <see cref="T:System.Windows.Media.PathSegment" /> at the specified index from this <see cref="T:System.Windows.Media.PathSegmentCollection" />.</summary>
	/// <param name="index">The index of the item to remove.</param>
	public void RemoveAt(int index)
	{
		RemoveAtWithoutFiringPublicEvents(index);
		WritePostscript();
	}

	internal void RemoveAtWithoutFiringPublicEvents(int index)
	{
		WritePreamble();
		PathSegment oldValue = _collection[index];
		OnFreezablePropertyChanged(oldValue, null);
		_collection.RemoveAt(index);
		_version++;
	}

	/// <summary> Copies the entire <see cref="T:System.Windows.Media.PathSegmentCollection" /> to a one-dimensional <see cref="T:System.Windows.Media.PathSegment" /> array, starting at the specified index of the target array. </summary>
	/// <param name="array">The array into which the collection's items are to be copied.</param>
	/// <param name="index">The index of <paramref name="array" /> at which to start copying the contents of the <see cref="T:System.Windows.Media.PathSegmentCollection" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional. -or-The number of items in the source <see cref="T:System.Windows.Media.PathSegmentCollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
	public void CopyTo(PathSegment[] array, int index)
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
	/// <returns>An <see cref="T:System.Windows.Media.PathSegmentCollection.Enumerator" /> that can iterate through the collection.</returns>
	public Enumerator GetEnumerator()
	{
		ReadPreamble();
		return new Enumerator(this);
	}

	IEnumerator<PathSegment> IEnumerable<PathSegment>.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Add(System.Object)" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.Media.PathSegmentCollection" />.</param>
	int IList.Add(object value)
	{
		return AddHelper(Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Contains(System.Object)" />.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.Media.PathSegmentCollection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.PathSegmentCollection" />.</param>
	bool IList.Contains(object value)
	{
		return Contains(value as PathSegment);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.IndexOf(System.Object)" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.PathSegmentCollection" />.</param>
	int IList.IndexOf(object value)
	{
		return IndexOf(value as PathSegment);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Insert(System.Int32,System.Object)" />.</summary>
	/// <param name="index">The zero-based index at which to insert the <see cref="T:System.Object" />.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Windows.Media.PathSegmentCollection" />.</param>
	void IList.Insert(int index, object value)
	{
		Insert(index, Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Remove(System.Object)" />.</summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.Media.PathSegmentCollection" />.</param>
	void IList.Remove(object value)
	{
		Remove(value as PathSegment);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items from the <see cref="T:System.Windows.Media.PathSegmentCollection" />.</param>
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

	internal PathSegment Internal_GetItem(int i)
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

	private PathSegment Cast(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is PathSegment))
		{
			throw new ArgumentException(SR.Format(SR.Collection_BadType, GetType().Name, value.GetType().Name, "PathSegment"));
		}
		return (PathSegment)value;
	}

	private int AddHelper(PathSegment value)
	{
		int result = AddWithoutFiringPublicEvents(value);
		WritePostscript();
		return result;
	}

	internal int AddWithoutFiringPublicEvents(PathSegment value)
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
		return new PathSegmentCollection();
	}

	protected override void CloneCore(Freezable source)
	{
		PathSegmentCollection pathSegmentCollection = (PathSegmentCollection)source;
		base.CloneCore(source);
		int count = pathSegmentCollection._collection.Count;
		_collection = new FrugalStructList<PathSegment>(count);
		for (int i = 0; i < count; i++)
		{
			PathSegment pathSegment = pathSegmentCollection._collection[i].Clone();
			OnFreezablePropertyChanged(null, pathSegment);
			_collection.Add(pathSegment);
		}
	}

	protected override void CloneCurrentValueCore(Freezable source)
	{
		PathSegmentCollection pathSegmentCollection = (PathSegmentCollection)source;
		base.CloneCurrentValueCore(source);
		int count = pathSegmentCollection._collection.Count;
		_collection = new FrugalStructList<PathSegment>(count);
		for (int i = 0; i < count; i++)
		{
			PathSegment pathSegment = pathSegmentCollection._collection[i].CloneCurrentValue();
			OnFreezablePropertyChanged(null, pathSegment);
			_collection.Add(pathSegment);
		}
	}

	protected override void GetAsFrozenCore(Freezable source)
	{
		PathSegmentCollection pathSegmentCollection = (PathSegmentCollection)source;
		base.GetAsFrozenCore(source);
		int count = pathSegmentCollection._collection.Count;
		_collection = new FrugalStructList<PathSegment>(count);
		for (int i = 0; i < count; i++)
		{
			PathSegment pathSegment = (PathSegment)pathSegmentCollection._collection[i].GetAsFrozen();
			OnFreezablePropertyChanged(null, pathSegment);
			_collection.Add(pathSegment);
		}
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable source)
	{
		PathSegmentCollection pathSegmentCollection = (PathSegmentCollection)source;
		base.GetCurrentValueAsFrozenCore(source);
		int count = pathSegmentCollection._collection.Count;
		_collection = new FrugalStructList<PathSegment>(count);
		for (int i = 0; i < count; i++)
		{
			PathSegment pathSegment = (PathSegment)pathSegmentCollection._collection[i].GetCurrentValueAsFrozen();
			OnFreezablePropertyChanged(null, pathSegment);
			_collection.Add(pathSegment);
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

	/// <summary>Initializes a new instance of a <see cref="T:System.Windows.Media.PathSegmentCollection" />.</summary>
	public PathSegmentCollection()
	{
		_collection = default(FrugalStructList<PathSegment>);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.PathSegmentCollection" /> class with the specified capacity, or the number of <see cref="T:System.Windows.Media.PathSegment" /> objects the collection is initially capable of storing.</summary>
	/// <param name="capacity">The number of <see cref="T:System.Windows.Media.PathSegment" /> objects that the collection is initially capable of storing.</param>
	public PathSegmentCollection(int capacity)
	{
		_collection = new FrugalStructList<PathSegment>(capacity);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.PathSegmentCollection" /> class with the specified collection of <see cref="T:System.Windows.Media.PathSegment" /> objects.</summary>
	/// <param name="collection">The collection of <see cref="T:System.Windows.Media.PathSegment" /> objects that make up the <see cref="T:System.Windows.Media.PathSegmentCollection" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="collection" /> is null.</exception>
	public PathSegmentCollection(IEnumerable<PathSegment> collection)
	{
		WritePreamble();
		if (collection != null)
		{
			bool flag = true;
			if (collection is ICollection<PathSegment> collection2)
			{
				_collection = new FrugalStructList<PathSegment>(collection2);
			}
			else if (collection is ICollection collection3)
			{
				_collection = new FrugalStructList<PathSegment>(collection3);
			}
			else
			{
				_collection = default(FrugalStructList<PathSegment>);
				foreach (PathSegment item in collection)
				{
					PathSegment pathSegment = item ?? throw new ArgumentException(SR.Collection_NoNull);
					OnFreezablePropertyChanged(null, pathSegment);
					_collection.Add(pathSegment);
				}
				flag = false;
			}
			if (flag)
			{
				foreach (PathSegment item2 in collection)
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
