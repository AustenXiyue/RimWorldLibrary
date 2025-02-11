using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media.Animation;
using MS.Internal.PresentationCore;
using MS.Utility;

namespace System.Windows;

/// <summary>Represents a collection of <see cref="T:System.Windows.TextDecoration" /> instances.</summary>
[TypeConverter(typeof(TextDecorationCollectionConverter))]
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public sealed class TextDecorationCollection : Animatable, IList, ICollection, IEnumerable, IList<TextDecoration>, ICollection<TextDecoration>, IEnumerable<TextDecoration>
{
	/// <summary>Enumerates <see cref="T:System.Windows.TextDecoration" /> items in a <see cref="T:System.Windows.TextDecoration" />.</summary>
	public struct Enumerator : IEnumerator, IEnumerator<TextDecoration>, IDisposable
	{
		private TextDecoration _current;

		private TextDecorationCollection _list;

		private uint _version;

		private int _index;

		/// <summary>For a description of this members, see <see cref="P:System.Collections.IEnumerator.Current" />.</summary>
		/// <returns>The current element in the collection.</returns>
		object IEnumerator.Current => Current;

		/// <summary>Gets the current element in the collection.</summary>
		/// <returns>The current <see cref="T:System.Windows.TextDecoration" /> in the collection.</returns>
		public TextDecoration Current
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

		internal Enumerator(TextDecorationCollection list)
		{
			_list = list;
			_version = list._version;
			_index = -1;
			_current = null;
		}

		/// <summary>For a description of this members, see <see cref="M:System.IDisposable.Dispose" />.</summary>
		void IDisposable.Dispose()
		{
		}

		/// <summary>Advances the enumerator to the next element in the collection.</summary>
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

		/// <summary>Resets the enumerator to its initial position, which is before the first element in the collection.</summary>
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

	private static TextDecorationCollection s_empty;

	internal FrugalStructList<TextDecoration> _collection;

	internal uint _version;

	/// <summary>Gets or sets the <see cref="T:System.Windows.TextDecoration" /> object at the specified index position.</summary>
	/// <returns>The <see cref="T:System.Windows.TextDecoration" /> object at the <paramref name="index" /> position.</returns>
	/// <param name="index">The zero-based index position of the object to get or set.</param>
	public TextDecoration this[int index]
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
				TextDecoration oldValue = _collection[index];
				OnFreezablePropertyChanged(oldValue, value);
				_collection[index] = value;
			}
			_version++;
			WritePostscript();
		}
	}

	/// <summary>Gets the number of <see cref="T:System.Windows.TextDecoration" /> objects in the <see cref="T:System.Windows.TextDecorationCollection" />.</summary>
	/// <returns>The number of objects in the collection.</returns>
	public int Count
	{
		get
		{
			ReadPreamble();
			return _collection.Count;
		}
	}

	bool ICollection<TextDecoration>.IsReadOnly
	{
		get
		{
			ReadPreamble();
			return base.IsFrozen;
		}
	}

	/// <summary>Gets a value that indicates whether the collection is read-only.</summary>
	/// <returns>true if the <see cref="T:System.Windows.TextDecorationCollection" /> is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => ((ICollection<TextDecoration>)this).IsReadOnly;

	/// <summary>Gets a value that indicates whether the collection has a fixed size.</summary>
	/// <returns>true if the <see cref="T:System.Windows.TextDecorationCollection" /> has a fixed size; otherwise, false.</returns>
	bool IList.IsFixedSize
	{
		get
		{
			ReadPreamble();
			return base.IsFrozen;
		}
	}

	/// <summary>Gets or sets the element at the specified index.</summary>
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

	/// <summary>Gets a value that indicates whether access to the collection is synchronized (thread safe).</summary>
	/// <returns>true if access to the <see cref="T:System.Windows.TextDecorationCollection" /> is synchronized (thread safe); otherwise, false.</returns>
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

	/// <summary>Gets an object that can be used to synchronize access to the collection.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Windows.TextDecorationCollection" />.</returns>
	object ICollection.SyncRoot
	{
		get
		{
			ReadPreamble();
			return this;
		}
	}

	internal static TextDecorationCollection Empty
	{
		get
		{
			if (s_empty == null)
			{
				TextDecorationCollection textDecorationCollection = new TextDecorationCollection();
				textDecorationCollection.Freeze();
				s_empty = textDecorationCollection;
			}
			return s_empty;
		}
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.TextDecorationCollection" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new TextDecorationCollection Clone()
	{
		return (TextDecorationCollection)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.TextDecorationCollection" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new TextDecorationCollection CloneCurrentValue()
	{
		return (TextDecorationCollection)base.CloneCurrentValue();
	}

	/// <summary>Inserts the specified <see cref="T:System.Windows.TextDecoration" /> object into the collection.</summary>
	/// <param name="value">The <see cref="T:System.Windows.TextDecoration" /> object to insert.</param>
	public void Add(TextDecoration value)
	{
		AddHelper(value);
	}

	/// <summary>Removes all <see cref="T:System.Windows.TextDecoration" /> objects from the <see cref="T:System.Windows.TextDecorationCollection" />.</summary>
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

	/// <summary>Determines if the <see cref="T:System.Windows.TextDecorationCollection" /> contains the specified <see cref="T:System.Windows.TextDecoration" />.</summary>
	/// <returns>true if <paramref name="value" /> is in the collection; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Windows.TextDecoration" /> object to locate.</param>
	public bool Contains(TextDecoration value)
	{
		ReadPreamble();
		return _collection.Contains(value);
	}

	/// <summary>Returns the index of the specified <see cref="T:System.Windows.TextDecoration" /> object within the collection. </summary>
	/// <returns>The zero-based index of <paramref name="value" />, if found; otherwise -1;</returns>
	/// <param name="value">The <see cref="T:System.Windows.TextDecoration" /> object to locate.</param>
	public int IndexOf(TextDecoration value)
	{
		ReadPreamble();
		return _collection.IndexOf(value);
	}

	/// <summary>Inserts the specified <see cref="T:System.Windows.TextDecoration" /> object at the specified index position in the collection.</summary>
	/// <param name="index">The zero-based index position to insert the object.</param>
	/// <param name="value">The <see cref="T:System.Windows.TextDecoration" /> object to insert.</param>
	public void Insert(int index, TextDecoration value)
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

	/// <summary>Removes the specified <see cref="T:System.Windows.TextDecoration" /> object from the collection.</summary>
	/// <returns>true if <paramref name="value" /> was successfully deleted; otherwise false.</returns>
	/// <param name="value">The <see cref="T:System.Windows.TextDecoration" /> object to remove.</param>
	public bool Remove(TextDecoration value)
	{
		WritePreamble();
		int num = IndexOf(value);
		if (num >= 0)
		{
			TextDecoration oldValue = _collection[num];
			OnFreezablePropertyChanged(oldValue, null);
			_collection.RemoveAt(num);
			_version++;
			WritePostscript();
			return true;
		}
		return false;
	}

	/// <summary>Removes the specified <see cref="T:System.Windows.TextDecoration" /> object from the collection at the specified index.</summary>
	/// <param name="index">The zero-based index position from where to delete the object.</param>
	public void RemoveAt(int index)
	{
		RemoveAtWithoutFiringPublicEvents(index);
		WritePostscript();
	}

	internal void RemoveAtWithoutFiringPublicEvents(int index)
	{
		WritePreamble();
		TextDecoration oldValue = _collection[index];
		OnFreezablePropertyChanged(oldValue, null);
		_collection.RemoveAt(index);
		_version++;
	}

	/// <summary>Copies the <see cref="T:System.Windows.TextDecoration" /> objects in the collection into an array of <see cref="T:System.Windows.TextDecorationCollection" />, starting at the specified index position.</summary>
	/// <param name="array">The destination array.</param>
	/// <param name="index">The zero-based index position where copying begins.</param>
	public void CopyTo(TextDecoration[] array, int index)
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
	/// <returns>An enumerator that can iterate through the collection.</returns>
	public Enumerator GetEnumerator()
	{
		ReadPreamble();
		return new Enumerator(this);
	}

	IEnumerator<TextDecoration> IEnumerable<TextDecoration>.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>Adds an item to the collection.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.TextDecorationCollection" />.</param>
	int IList.Add(object value)
	{
		return AddHelper(Cast(value));
	}

	/// <summary>Determines whether the collection contains a specific value.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.TextDecorationCollection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.TextEffectCollection" />.</param>
	bool IList.Contains(object value)
	{
		return Contains(value as TextDecoration);
	}

	/// <summary>Determines the index of a specific item in the collection.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.TextDecorationCollection" />.</param>
	int IList.IndexOf(object value)
	{
		return IndexOf(value as TextDecoration);
	}

	/// <summary>Inserts an item to the collection at the specified index.</summary>
	/// <param name="index">The zero-based index at which to insert the <see cref="T:System.Object" />.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Windows.TextDecorationCollection" />.</param>
	void IList.Insert(int index, object value)
	{
		Insert(index, Cast(value));
	}

	/// <summary>Removes the first occurrence of a specific object from the collection.</summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.TextDecorationCollection" />.</param>
	void IList.Remove(object value)
	{
		Remove(value as TextDecoration);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items from the <see cref="T:System.Windows.TextDecorationCollection" />.</param>
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

	/// <summary>Returns an enumerator that iterates through the collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	internal TextDecoration Internal_GetItem(int i)
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

	private TextDecoration Cast(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is TextDecoration))
		{
			throw new ArgumentException(SR.Format(SR.Collection_BadType, GetType().Name, value.GetType().Name, "TextDecoration"));
		}
		return (TextDecoration)value;
	}

	private int AddHelper(TextDecoration value)
	{
		int result = AddWithoutFiringPublicEvents(value);
		WritePostscript();
		return result;
	}

	internal int AddWithoutFiringPublicEvents(TextDecoration value)
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
		return new TextDecorationCollection();
	}

	protected override void CloneCore(Freezable source)
	{
		TextDecorationCollection textDecorationCollection = (TextDecorationCollection)source;
		base.CloneCore(source);
		int count = textDecorationCollection._collection.Count;
		_collection = new FrugalStructList<TextDecoration>(count);
		for (int i = 0; i < count; i++)
		{
			TextDecoration textDecoration = textDecorationCollection._collection[i].Clone();
			OnFreezablePropertyChanged(null, textDecoration);
			_collection.Add(textDecoration);
		}
	}

	protected override void CloneCurrentValueCore(Freezable source)
	{
		TextDecorationCollection textDecorationCollection = (TextDecorationCollection)source;
		base.CloneCurrentValueCore(source);
		int count = textDecorationCollection._collection.Count;
		_collection = new FrugalStructList<TextDecoration>(count);
		for (int i = 0; i < count; i++)
		{
			TextDecoration textDecoration = textDecorationCollection._collection[i].CloneCurrentValue();
			OnFreezablePropertyChanged(null, textDecoration);
			_collection.Add(textDecoration);
		}
	}

	protected override void GetAsFrozenCore(Freezable source)
	{
		TextDecorationCollection textDecorationCollection = (TextDecorationCollection)source;
		base.GetAsFrozenCore(source);
		int count = textDecorationCollection._collection.Count;
		_collection = new FrugalStructList<TextDecoration>(count);
		for (int i = 0; i < count; i++)
		{
			TextDecoration textDecoration = (TextDecoration)textDecorationCollection._collection[i].GetAsFrozen();
			OnFreezablePropertyChanged(null, textDecoration);
			_collection.Add(textDecoration);
		}
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable source)
	{
		TextDecorationCollection textDecorationCollection = (TextDecorationCollection)source;
		base.GetCurrentValueAsFrozenCore(source);
		int count = textDecorationCollection._collection.Count;
		_collection = new FrugalStructList<TextDecoration>(count);
		for (int i = 0; i < count; i++)
		{
			TextDecoration textDecoration = (TextDecoration)textDecorationCollection._collection[i].GetCurrentValueAsFrozen();
			OnFreezablePropertyChanged(null, textDecoration);
			_collection.Add(textDecoration);
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

	/// <summary>Initializes a new <see cref="T:System.Windows.TextDecorationCollection" /> instance that is empty.</summary>
	public TextDecorationCollection()
	{
		_collection = default(FrugalStructList<TextDecoration>);
	}

	/// <summary>Initializes a new <see cref="T:System.Windows.TextDecorationCollection" /> instance that is empty and has the specified initial capacity.</summary>
	/// <param name="capacity">The number of elements that the new collection is initially capable of storing.</param>
	public TextDecorationCollection(int capacity)
	{
		_collection = new FrugalStructList<TextDecoration>(capacity);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.TextDecorationCollection" /> class by specifying an enumerator.</summary>
	/// <param name="collection">An enumerator of type <see cref="T:System.Collections.Generic.IEnumerable`1" />.</param>
	public TextDecorationCollection(IEnumerable<TextDecoration> collection)
	{
		WritePreamble();
		if (collection != null)
		{
			bool flag = true;
			if (collection is ICollection<TextDecoration> collection2)
			{
				_collection = new FrugalStructList<TextDecoration>(collection2);
			}
			else if (collection is ICollection collection3)
			{
				_collection = new FrugalStructList<TextDecoration>(collection3);
			}
			else
			{
				_collection = default(FrugalStructList<TextDecoration>);
				foreach (TextDecoration item in collection)
				{
					TextDecoration textDecoration = item ?? throw new ArgumentException(SR.Collection_NoNull);
					OnFreezablePropertyChanged(null, textDecoration);
					_collection.Add(textDecoration);
				}
				flag = false;
			}
			if (flag)
			{
				foreach (TextDecoration item2 in collection)
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

	[FriendAccessAllowed]
	internal bool ValueEquals(TextDecorationCollection textDecorations)
	{
		if (textDecorations == null)
		{
			return false;
		}
		if (this == textDecorations)
		{
			return true;
		}
		if (Count != textDecorations.Count)
		{
			return false;
		}
		for (int i = 0; i < Count; i++)
		{
			if (!this[i].ValueEquals(textDecorations[i]))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Adds a generic <see cref="T:System.Collections.Generic.IEnumerable`1" /> to the collection.</summary>
	/// <param name="textDecorations">A generic <see cref="T:System.Collections.Generic.IEnumerable`1" /> of type <see cref="T:System.Windows.TextDecoration" />.</param>
	[CLSCompliant(false)]
	public void Add(IEnumerable<TextDecoration> textDecorations)
	{
		if (textDecorations == null)
		{
			throw new ArgumentNullException("textDecorations");
		}
		foreach (TextDecoration textDecoration in textDecorations)
		{
			Add(textDecoration);
		}
	}

	public bool TryRemove(IEnumerable<TextDecoration> textDecorations, out TextDecorationCollection result)
	{
		if (textDecorations == null)
		{
			throw new ArgumentNullException("textDecorations");
		}
		bool result2 = false;
		result = Clone();
		foreach (TextDecoration textDecoration in textDecorations)
		{
			for (int num = result.Count - 1; num >= 0; num--)
			{
				if (result[num].ValueEquals(textDecoration))
				{
					result.RemoveAt(num);
					result2 = true;
				}
			}
		}
		return result2;
	}
}
