using System.Collections;
using System.Collections.Generic;
using System.Windows.Media.Animation;
using MS.Internal.PresentationCore;
using MS.Utility;

namespace System.Windows.Media;

/// <summary>Provides collection support for a collection of <see cref="T:System.Windows.Media.TextEffect" /> objects.</summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public sealed class TextEffectCollection : Animatable, IList, ICollection, IEnumerable, IList<TextEffect>, ICollection<TextEffect>, IEnumerable<TextEffect>
{
	/// <summary>Enumerates <see cref="T:System.Windows.Media.TextEffect" /> items in a <see cref="T:System.Windows.Media.TextEffectCollection" />.</summary>
	public struct Enumerator : IEnumerator, IEnumerator<TextEffect>, IDisposable
	{
		private TextEffect _current;

		private TextEffectCollection _list;

		private uint _version;

		private int _index;

		/// <summary>For a description of this members, see <see cref="P:System.Collections.IEnumerator.Current" />.</summary>
		/// <returns>The current element in the collection.</returns>
		object IEnumerator.Current => Current;

		/// <summary>Gets the current element in the collection.</summary>
		/// <returns>The current element in the collection.</returns>
		public TextEffect Current
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

		internal Enumerator(TextEffectCollection list)
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

	private static TextEffectCollection s_empty;

	internal FrugalStructList<TextEffect> _collection;

	internal uint _version;

	/// <summary>Gets or sets the item that is stored at the zero-based index of the collection.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the collection from which to get or set the item.</param>
	public TextEffect this[int index]
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
				TextEffect oldValue = _collection[index];
				OnFreezablePropertyChanged(oldValue, value);
				_collection[index] = value;
			}
			_version++;
			WritePostscript();
		}
	}

	/// <summary>Gets the number of elements in the collection.</summary>
	/// <returns>The number of elements in the collection.</returns>
	public int Count
	{
		get
		{
			ReadPreamble();
			return _collection.Count;
		}
	}

	bool ICollection<TextEffect>.IsReadOnly
	{
		get
		{
			ReadPreamble();
			return base.IsFrozen;
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsReadOnly" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.TextEffectCollection" /> is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => ((ICollection<TextEffect>)this).IsReadOnly;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsFixedSize" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.TextEffectCollection" /> has a fixed size; otherwise, false.</returns>
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
	/// <returns>true if access to the <see cref="T:System.Windows.Media.TextEffectCollection" /> is synchronized (thread safe); otherwise, false.</returns>
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
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Windows.Media.TextEffectCollection" />.</returns>
	object ICollection.SyncRoot
	{
		get
		{
			ReadPreamble();
			return this;
		}
	}

	internal static TextEffectCollection Empty
	{
		get
		{
			if (s_empty == null)
			{
				TextEffectCollection textEffectCollection = new TextEffectCollection();
				textEffectCollection.Freeze();
				s_empty = textEffectCollection;
			}
			return s_empty;
		}
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.TextEffectCollection" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new TextEffectCollection Clone()
	{
		return (TextEffectCollection)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.TextEffectCollection" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new TextEffectCollection CloneCurrentValue()
	{
		return (TextEffectCollection)base.CloneCurrentValue();
	}

	/// <summary>Adds a <see cref="T:System.Windows.Media.TextEffect" /> to the end of the collection.</summary>
	/// <param name="value">The <see cref="T:System.Windows.Media.TextEffect" /> to add to the collection.</param>
	public void Add(TextEffect value)
	{
		AddHelper(value);
	}

	/// <summary>Removes all elements from the list.</summary>
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

	/// <summary>Determines if the specified item is in the collection.</summary>
	/// <returns>true if the collection contains <paramref name="value" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Windows.Media.TextEffect" /> to locate in the collection</param>
	public bool Contains(TextEffect value)
	{
		ReadPreamble();
		return _collection.Contains(value);
	}

	/// <summary>Searches for the specified <see cref="T:System.Windows.Media.TextEffect" /> and returns the zero-based index of the first occurrence within the entire collection.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="value" /> within the entire collection, if found; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Windows.Media.TextEffect" /> to locate in the collection.</param>
	public int IndexOf(TextEffect value)
	{
		ReadPreamble();
		return _collection.IndexOf(value);
	}

	/// <summary>Inserts a <see cref="T:System.Windows.Media.TextEffect" /> into a specific location in the collection.</summary>
	/// <param name="index">The zero-based index at which the value <see cref="T:System.Windows.Media.TextEffect" /> be inserted.</param>
	/// <param name="value">The <see cref="T:System.Windows.Media.TextEffect" /> to insert into the collection.</param>
	public void Insert(int index, TextEffect value)
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

	/// <summary>Removes the specified <see cref="T:System.Windows.Media.TextEffect" /> object from the collection.</summary>
	/// <returns>true if <paramref name="value" /> was removed fro the collection; otherwise, false. </returns>
	/// <param name="value">The <see cref="T:System.Windows.Media.TextEffect" /> to remove from the collection.</param>
	public bool Remove(TextEffect value)
	{
		WritePreamble();
		int num = IndexOf(value);
		if (num >= 0)
		{
			TextEffect oldValue = _collection[num];
			OnFreezablePropertyChanged(oldValue, null);
			_collection.RemoveAt(num);
			_version++;
			WritePostscript();
			return true;
		}
		return false;
	}

	/// <summary>Removes the <see cref="T:System.Windows.Media.TextEffect" /> at the specified index in the collection.</summary>
	/// <param name="index">The zero-based index of the <see cref="T:System.Windows.Media.TextEffect" /> to remove.</param>
	public void RemoveAt(int index)
	{
		RemoveAtWithoutFiringPublicEvents(index);
		WritePostscript();
	}

	internal void RemoveAtWithoutFiringPublicEvents(int index)
	{
		WritePreamble();
		TextEffect oldValue = _collection[index];
		OnFreezablePropertyChanged(oldValue, null);
		_collection.RemoveAt(index);
		_version++;
	}

	/// <summary>Copies the entire <see cref="T:System.Windows.Media.TextEffectCollection" /> to a one-dimensional array of type <see cref="T:System.Windows.Media.TextEffect" />, starting at the specified index of the target array.</summary>
	/// <param name="array">The array into which the collection's items are to be copied.</param>
	/// <param name="index">The index of <paramref name="array" /> at which to start copying the contents of the <see cref="T:System.Windows.Media.TextEffectCollection" />.</param>
	public void CopyTo(TextEffect[] array, int index)
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
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can iterate through the collection.</returns>
	public Enumerator GetEnumerator()
	{
		ReadPreamble();
		return new Enumerator(this);
	}

	IEnumerator<TextEffect> IEnumerable<TextEffect>.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Add(System.Object)" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.Media.TextEffectCollection" />.</param>
	int IList.Add(object value)
	{
		return AddHelper(Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Contains(System.Object)" />.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.Media.TextEffectCollection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.TextEffectCollection" />.</param>
	bool IList.Contains(object value)
	{
		return Contains(value as TextEffect);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.IndexOf(System.Object)" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.TextEffectCollection" />.</param>
	int IList.IndexOf(object value)
	{
		return IndexOf(value as TextEffect);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Insert(System.Int32,System.Object)" />.</summary>
	/// <param name="index">The zero-based index at which to insert the <see cref="T:System.Object" />.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Windows.Media.TextEffectCollection" />.</param>
	void IList.Insert(int index, object value)
	{
		Insert(index, Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Remove(System.Object)" />.</summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.Media.TextEffectCollection" />.</param>
	void IList.Remove(object value)
	{
		Remove(value as TextEffect);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items from the <see cref="T:System.Windows.Media.TextEffectCollection" />.</param>
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

	internal TextEffect Internal_GetItem(int i)
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

	private TextEffect Cast(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is TextEffect))
		{
			throw new ArgumentException(SR.Format(SR.Collection_BadType, GetType().Name, value.GetType().Name, "TextEffect"));
		}
		return (TextEffect)value;
	}

	private int AddHelper(TextEffect value)
	{
		int result = AddWithoutFiringPublicEvents(value);
		WritePostscript();
		return result;
	}

	internal int AddWithoutFiringPublicEvents(TextEffect value)
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
		return new TextEffectCollection();
	}

	protected override void CloneCore(Freezable source)
	{
		TextEffectCollection textEffectCollection = (TextEffectCollection)source;
		base.CloneCore(source);
		int count = textEffectCollection._collection.Count;
		_collection = new FrugalStructList<TextEffect>(count);
		for (int i = 0; i < count; i++)
		{
			TextEffect textEffect = textEffectCollection._collection[i].Clone();
			OnFreezablePropertyChanged(null, textEffect);
			_collection.Add(textEffect);
		}
	}

	protected override void CloneCurrentValueCore(Freezable source)
	{
		TextEffectCollection textEffectCollection = (TextEffectCollection)source;
		base.CloneCurrentValueCore(source);
		int count = textEffectCollection._collection.Count;
		_collection = new FrugalStructList<TextEffect>(count);
		for (int i = 0; i < count; i++)
		{
			TextEffect textEffect = textEffectCollection._collection[i].CloneCurrentValue();
			OnFreezablePropertyChanged(null, textEffect);
			_collection.Add(textEffect);
		}
	}

	protected override void GetAsFrozenCore(Freezable source)
	{
		TextEffectCollection textEffectCollection = (TextEffectCollection)source;
		base.GetAsFrozenCore(source);
		int count = textEffectCollection._collection.Count;
		_collection = new FrugalStructList<TextEffect>(count);
		for (int i = 0; i < count; i++)
		{
			TextEffect textEffect = (TextEffect)textEffectCollection._collection[i].GetAsFrozen();
			OnFreezablePropertyChanged(null, textEffect);
			_collection.Add(textEffect);
		}
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable source)
	{
		TextEffectCollection textEffectCollection = (TextEffectCollection)source;
		base.GetCurrentValueAsFrozenCore(source);
		int count = textEffectCollection._collection.Count;
		_collection = new FrugalStructList<TextEffect>(count);
		for (int i = 0; i < count; i++)
		{
			TextEffect textEffect = (TextEffect)textEffectCollection._collection[i].GetCurrentValueAsFrozen();
			OnFreezablePropertyChanged(null, textEffect);
			_collection.Add(textEffect);
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextEffectCollection" /> class. </summary>
	public TextEffectCollection()
	{
		_collection = default(FrugalStructList<TextEffect>);
	}

	/// <summary>Initializes a new <see cref="T:System.Windows.Media.TextEffectCollection" /> instance that is empty and has the specified initial capacity. </summary>
	/// <param name="capacity">The number of elements that the new collection is initially capable of storing.</param>
	public TextEffectCollection(int capacity)
	{
		_collection = new FrugalStructList<TextEffect>(capacity);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextEffectCollection" /> class.</summary>
	/// <param name="collection">An enumerator of type <see cref="T:System.Collections.IEnumerable" />.</param>
	public TextEffectCollection(IEnumerable<TextEffect> collection)
	{
		WritePreamble();
		if (collection != null)
		{
			bool flag = true;
			if (collection is ICollection<TextEffect> collection2)
			{
				_collection = new FrugalStructList<TextEffect>(collection2);
			}
			else if (collection is ICollection collection3)
			{
				_collection = new FrugalStructList<TextEffect>(collection3);
			}
			else
			{
				_collection = default(FrugalStructList<TextEffect>);
				foreach (TextEffect item in collection)
				{
					TextEffect textEffect = item ?? throw new ArgumentException(SR.Collection_NoNull);
					OnFreezablePropertyChanged(null, textEffect);
					_collection.Add(textEffect);
				}
				flag = false;
			}
			if (flag)
			{
				foreach (TextEffect item2 in collection)
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
