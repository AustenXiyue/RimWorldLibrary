using System.Collections;
using System.Collections.Generic;
using System.Windows.Media.Animation;
using MS.Internal.Collections;
using MS.Internal.PresentationCore;
using MS.Utility;

namespace System.Windows.Media;

/// <summary>Represents an ordered collection of <see cref="T:System.Windows.Media.Drawing" /> objects.</summary>
public sealed class DrawingCollection : Animatable, IList, ICollection, IEnumerable, IList<Drawing>, ICollection<Drawing>, IEnumerable<Drawing>
{
	/// <summary>Enumerates <see cref="T:System.Windows.Media.Drawing" /> items in a <see cref="T:System.Windows.Media.DrawingCollection" />. </summary>
	public struct Enumerator : IEnumerator, IEnumerator<Drawing>, IDisposable
	{
		private Drawing _current;

		private DrawingCollection _list;

		private uint _version;

		private int _index;

		/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
		object IEnumerator.Current => Current;

		/// <summary>Gets the current element in the collection.</summary>
		/// <returns>The current element in the collection.</returns>
		public Drawing Current
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

		internal Enumerator(DrawingCollection list)
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

	private static DrawingCollection s_empty;

	internal FrugalStructList<Drawing> _collection;

	internal uint _version;

	/// <summary> Gets or sets the <see cref="T:System.Windows.Media.Drawing" /> at the specified zero-based index.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.Drawing" /> at the specified index.</returns>
	/// <param name="index">The zero-based index of the <see cref="T:System.Windows.Media.Drawing" /> object to get or set</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the <see cref="T:System.Windows.Media.DrawingCollection" />.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.DrawingCollection" /> is read-only.- or -The <see cref="T:System.Windows.Media.DrawingCollection" /> has a fixed size.</exception>
	public Drawing this[int index]
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
				Drawing oldValue = _collection[index];
				OnFreezablePropertyChanged(oldValue, value);
				_collection[index] = value;
				OnSet(oldValue, value);
			}
			_version++;
			WritePostscript();
		}
	}

	/// <summary>Gets the number of <see cref="T:System.Windows.Media.Drawing" /> objects contained in the <see cref="T:System.Windows.Media.DrawingCollection" />.</summary>
	/// <returns>The number of <see cref="T:System.Windows.Media.Drawing" /> objects contained in the <see cref="T:System.Windows.Media.DrawingCollection" />.</returns>
	public int Count
	{
		get
		{
			ReadPreamble();
			return _collection.Count;
		}
	}

	bool ICollection<Drawing>.IsReadOnly
	{
		get
		{
			ReadPreamble();
			return base.IsFrozen;
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsReadOnly" />.</summary>
	/// <returns>true if the<see cref="T:System.Windows.Media.DrawingCollection" /> is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => ((ICollection<Drawing>)this).IsReadOnly;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsFixedSize" />.</summary>
	/// <returns>true if the<see cref="T:System.Windows.Media.DrawingCollection" /> has a fixed size; otherwise, false.</returns>
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
	/// <returns>true if access to the <see cref="T:System.Windows.Media.DrawingCollection" /> is synchronized (thread safe); otherwise, false.</returns>
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
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Windows.Media.DrawingCollection" />.</returns>
	object ICollection.SyncRoot
	{
		get
		{
			ReadPreamble();
			return this;
		}
	}

	internal static DrawingCollection Empty
	{
		get
		{
			if (s_empty == null)
			{
				DrawingCollection drawingCollection = new DrawingCollection();
				drawingCollection.Freeze();
				s_empty = drawingCollection;
			}
			return s_empty;
		}
	}

	internal event ItemInsertedHandler ItemInserted;

	internal event ItemRemovedHandler ItemRemoved;

	internal void TransactionalAppend(DrawingCollection collectionToAppend)
	{
		int count = collectionToAppend.Count;
		for (int i = 0; i < count; i++)
		{
			AddWithoutFiringPublicEvents(collectionToAppend.Internal_GetItem(i));
		}
		try
		{
			FireChanged();
		}
		catch (Exception)
		{
			int num = Count - count;
			for (int num2 = Count - 1; num2 >= num; num2--)
			{
				RemoveAtWithoutFiringPublicEvents(num2);
			}
			throw;
		}
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.DrawingCollection" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new DrawingCollection Clone()
	{
		return (DrawingCollection)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.DrawingCollection" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new DrawingCollection CloneCurrentValue()
	{
		return (DrawingCollection)base.CloneCurrentValue();
	}

	/// <summary>Adds a <see cref="T:System.Windows.Media.Drawing" /> to the end of the <see cref="T:System.Windows.Media.DrawingCollection" />. </summary>
	/// <param name="value">The item to add to the end of this collection.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.DrawingCollection" /> is read-only.- or -The <see cref="T:System.Windows.Media.DrawingCollection" /> has a fixed size.</exception>
	public void Add(Drawing value)
	{
		AddHelper(value);
	}

	/// <summary>Removes all the items from this <see cref="T:System.Windows.Media.DrawingCollection" />. </summary>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.DrawingCollection" /> is read-only.</exception>
	public void Clear()
	{
		WritePreamble();
		FrugalStructList<Drawing> collection = _collection;
		_collection = new FrugalStructList<Drawing>(_collection.Capacity);
		for (int num = collection.Count - 1; num >= 0; num--)
		{
			OnFreezablePropertyChanged(collection[num], null);
			OnRemove(collection[num]);
		}
		_version++;
		WritePostscript();
	}

	/// <summary> Determines whether a given <see cref="T:System.Windows.Media.Drawing" /> is in this <see cref="T:System.Windows.Media.DrawingCollection" />. </summary>
	/// <returns>true if <paramref name="value" />, the specified <see cref="T:System.Windows.Media.Drawing" />, is in this <see cref="T:System.Windows.Media.DrawingCollection" />; otherwise, false.</returns>
	/// <param name="value">The item to locate in this collection.</param>
	public bool Contains(Drawing value)
	{
		ReadPreamble();
		return _collection.Contains(value);
	}

	/// <summary>Gets the index position of the first occurrence of the specified <see cref="T:System.Windows.Media.Drawing" />. </summary>
	/// <returns>The index position of the specified <see cref="T:System.Windows.Media.Drawing" />.</returns>
	/// <param name="value">The item to search for.</param>
	public int IndexOf(Drawing value)
	{
		ReadPreamble();
		return _collection.IndexOf(value);
	}

	/// <summary>Inserts a <see cref="T:System.Windows.Media.Drawing" /> into this <see cref="T:System.Windows.Media.DrawingCollection" /> at the specified index position. </summary>
	/// <param name="index">The index position at which to insert <paramref name="value" />, the specified <see cref="T:System.Windows.Media.Drawing" />.</param>
	/// <param name="value">The item to insert.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the <see cref="T:System.Windows.Media.DrawingCollection" />.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.DrawingCollection" /> is read-only.- or -The <see cref="T:System.Windows.Media.DrawingCollection" /> has a fixed size.</exception>
	public void Insert(int index, Drawing value)
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

	/// <summary>Removes the first occurrence of the specified <see cref="T:System.Windows.Media.Drawing" /> from the <see cref="T:System.Windows.Media.DrawingCollection" />. </summary>
	/// <returns>true if the operation was successful; otherwise, false.</returns>
	/// <param name="value">The item to remove from this collection.</param>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.DrawingCollection" /> is read-only.- or -The <see cref="T:System.Windows.Media.DrawingCollection" /> has a fixed size.</exception>
	public bool Remove(Drawing value)
	{
		WritePreamble();
		int num = IndexOf(value);
		if (num >= 0)
		{
			Drawing oldValue = _collection[num];
			OnFreezablePropertyChanged(oldValue, null);
			_collection.RemoveAt(num);
			OnRemove(oldValue);
			_version++;
			WritePostscript();
			return true;
		}
		return false;
	}

	/// <summary>Removes the <see cref="T:System.Windows.Media.Drawing" /> at the specified index position from the <see cref="T:System.Windows.Media.DrawingCollection" />. </summary>
	/// <param name="index">The index position of the item to remove.</param>
	public void RemoveAt(int index)
	{
		RemoveAtWithoutFiringPublicEvents(index);
		WritePostscript();
	}

	internal void RemoveAtWithoutFiringPublicEvents(int index)
	{
		WritePreamble();
		Drawing oldValue = _collection[index];
		OnFreezablePropertyChanged(oldValue, null);
		_collection.RemoveAt(index);
		OnRemove(oldValue);
		_version++;
	}

	/// <summary>Copies the items of this <see cref="T:System.Windows.Media.DrawingCollection" />, starting with the specified index value, into an array of <see cref="T:System.Windows.Media.Drawing" /> objects. </summary>
	/// <param name="array">The array that is the destination of the items copied from this <see cref="T:System.Windows.Media.DrawingCollection" />.</param>
	/// <param name="index">The index at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional. -or-The number of items in the source <see cref="T:System.Windows.Media.DrawingCollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
	public void CopyTo(Drawing[] array, int index)
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

	/// <summary>Returns an enumerator that can iterate through the collection. </summary>
	/// <returns>An enumerator that can iterate the collection.</returns>
	public Enumerator GetEnumerator()
	{
		ReadPreamble();
		return new Enumerator(this);
	}

	IEnumerator<Drawing> IEnumerable<Drawing>.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Add(System.Object)" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.Media.DrawingCollection" />.</param>
	int IList.Add(object value)
	{
		return AddHelper(Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Contains(System.Object)" />.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.Media.DrawingCollection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.DrawingCollection" />.</param>
	bool IList.Contains(object value)
	{
		return Contains(value as Drawing);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.IndexOf(System.Object)" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.DrawingCollection" />.</param>
	int IList.IndexOf(object value)
	{
		return IndexOf(value as Drawing);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Insert(System.Int32,System.Object)" />.</summary>
	/// <param name="index">The zero-based index at which to insert the <see cref="T:System.Object" />.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Windows.Media.DrawingCollection" />.</param>
	void IList.Insert(int index, object value)
	{
		Insert(index, Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Remove(System.Object)" />.</summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.Media.DrawingCollection" />.</param>
	void IList.Remove(object value)
	{
		Remove(value as Drawing);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items from the <see cref="T:System.Windows.Media.DrawingCollection" />.</param>
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

	internal Drawing Internal_GetItem(int i)
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

	private Drawing Cast(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is Drawing))
		{
			throw new ArgumentException(SR.Format(SR.Collection_BadType, GetType().Name, value.GetType().Name, "Drawing"));
		}
		return (Drawing)value;
	}

	private int AddHelper(Drawing value)
	{
		int result = AddWithoutFiringPublicEvents(value);
		WritePostscript();
		return result;
	}

	internal int AddWithoutFiringPublicEvents(Drawing value)
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
		return new DrawingCollection();
	}

	protected override void CloneCore(Freezable source)
	{
		DrawingCollection drawingCollection = (DrawingCollection)source;
		base.CloneCore(source);
		int count = drawingCollection._collection.Count;
		_collection = new FrugalStructList<Drawing>(count);
		for (int i = 0; i < count; i++)
		{
			Drawing drawing = drawingCollection._collection[i].Clone();
			OnFreezablePropertyChanged(null, drawing);
			_collection.Add(drawing);
			OnInsert(drawing);
		}
	}

	protected override void CloneCurrentValueCore(Freezable source)
	{
		DrawingCollection drawingCollection = (DrawingCollection)source;
		base.CloneCurrentValueCore(source);
		int count = drawingCollection._collection.Count;
		_collection = new FrugalStructList<Drawing>(count);
		for (int i = 0; i < count; i++)
		{
			Drawing drawing = drawingCollection._collection[i].CloneCurrentValue();
			OnFreezablePropertyChanged(null, drawing);
			_collection.Add(drawing);
			OnInsert(drawing);
		}
	}

	protected override void GetAsFrozenCore(Freezable source)
	{
		DrawingCollection drawingCollection = (DrawingCollection)source;
		base.GetAsFrozenCore(source);
		int count = drawingCollection._collection.Count;
		_collection = new FrugalStructList<Drawing>(count);
		for (int i = 0; i < count; i++)
		{
			Drawing drawing = (Drawing)drawingCollection._collection[i].GetAsFrozen();
			OnFreezablePropertyChanged(null, drawing);
			_collection.Add(drawing);
			OnInsert(drawing);
		}
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable source)
	{
		DrawingCollection drawingCollection = (DrawingCollection)source;
		base.GetCurrentValueAsFrozenCore(source);
		int count = drawingCollection._collection.Count;
		_collection = new FrugalStructList<Drawing>(count);
		for (int i = 0; i < count; i++)
		{
			Drawing drawing = (Drawing)drawingCollection._collection[i].GetCurrentValueAsFrozen();
			OnFreezablePropertyChanged(null, drawing);
			_collection.Add(drawing);
			OnInsert(drawing);
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.DrawingCollection" /> class.</summary>
	public DrawingCollection()
	{
		_collection = default(FrugalStructList<Drawing>);
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Windows.Media.DrawingCollection" /> with the specified capacity.</summary>
	/// <param name="capacity">The total capacity of the collection.</param>
	public DrawingCollection(int capacity)
	{
		_collection = new FrugalStructList<Drawing>(capacity);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.DrawingCollection" /> class with the specified collection of <see cref="T:System.Windows.Media.Drawing" /> objects.</summary>
	/// <param name="collection">The collection of <see cref="T:System.Windows.Media.Drawing" /> objects that make up the <see cref="T:System.Windows.Media.DrawingCollection" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="collection" /> is null.</exception>
	public DrawingCollection(IEnumerable<Drawing> collection)
	{
		WritePreamble();
		if (collection != null)
		{
			bool flag = true;
			if (collection is ICollection<Drawing> collection2)
			{
				_collection = new FrugalStructList<Drawing>(collection2);
			}
			else if (collection is ICollection collection3)
			{
				_collection = new FrugalStructList<Drawing>(collection3);
			}
			else
			{
				_collection = default(FrugalStructList<Drawing>);
				foreach (Drawing item in collection)
				{
					Drawing drawing = item ?? throw new ArgumentException(SR.Collection_NoNull);
					OnFreezablePropertyChanged(null, drawing);
					_collection.Add(drawing);
					OnInsert(drawing);
				}
				flag = false;
			}
			if (flag)
			{
				foreach (Drawing item2 in collection)
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
