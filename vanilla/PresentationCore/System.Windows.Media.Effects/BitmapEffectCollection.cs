using System.Collections;
using System.Collections.Generic;
using System.Windows.Media.Animation;
using MS.Internal.PresentationCore;
using MS.Utility;

namespace System.Windows.Media.Effects;

/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Represents a collection of <see cref="T:System.Windows.Media.Effects.BitmapEffect" /> objects. This collection is used as part of a <see cref="T:System.Windows.Media.Effects.BitmapEffectGroup" /> to apply multiple bitmap effects to visual content.</summary>
public sealed class BitmapEffectCollection : Animatable, IList, ICollection, IEnumerable, IList<BitmapEffect>, ICollection<BitmapEffect>, IEnumerable<BitmapEffect>
{
	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Enumerates <see cref="T:System.Windows.Media.Effects.BitmapEffect" /> objects in a <see cref="T:System.Windows.Media.Effects.BitmapEffectCollection" />.</summary>
	public struct Enumerator : IEnumerator, IEnumerator<BitmapEffect>, IDisposable
	{
		private BitmapEffect _current;

		private BitmapEffectCollection _list;

		private uint _version;

		private int _index;

		/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
		object IEnumerator.Current => Current;

		/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Gets the current element in the collection.</summary>
		/// <returns>The current element in the collection.</returns>
		public BitmapEffect Current
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

		internal Enumerator(BitmapEffectCollection list)
		{
			_list = list;
			_version = list._version;
			_index = -1;
			_current = null;
		}

		/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
		void IDisposable.Dispose()
		{
		}

		/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Advanced the enumerator to the next element in the collection.</summary>
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

		/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Resets the enumerator to its initial position, which is before the first element in the collection.</summary>
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

	private static BitmapEffectCollection s_empty;

	internal FrugalStructList<BitmapEffect> _collection;

	internal uint _version;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Gets or sets the <see cref="T:System.Windows.Media.Effects.BitmapEffect" /> at the specified index.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.Effects.BitmapEffect" /> at the specified index.</returns>
	/// <param name="index">The zero-based index of the <see cref="T:System.Windows.Media.Effects.BitmapEffect" /> to get or set.</param>
	public BitmapEffect this[int index]
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
				BitmapEffect oldValue = _collection[index];
				OnFreezablePropertyChanged(oldValue, value);
				_collection[index] = value;
			}
			_version++;
			WritePostscript();
		}
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Gets the number of effects contained in the <see cref="T:System.Windows.Media.Effects.BitmapEffectCollection" />.</summary>
	/// <returns>The number of effects contained in the <see cref="T:System.Windows.Media.Effects.BitmapEffectCollection" />.</returns>
	public int Count
	{
		get
		{
			ReadPreamble();
			return _collection.Count;
		}
	}

	bool ICollection<BitmapEffect>.IsReadOnly
	{
		get
		{
			ReadPreamble();
			return base.IsFrozen;
		}
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. For a description of this member, see <see cref="P:System.Collections.IList.IsReadOnly" />.</summary>
	/// <returns>true if the the <see cref="T:System.Windows.Media.Effects.BitmapEffectCollection" /> is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => ((ICollection<BitmapEffect>)this).IsReadOnly;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. For a description of this member, see <see cref="P:System.Collections.IList.IsFixedSize" />.</summary>
	/// <returns>true if the the <see cref="T:System.Windows.Media.Effects.BitmapEffectCollection" /> has a fixed size; otherwise, false.</returns>
	bool IList.IsFixedSize
	{
		get
		{
			ReadPreamble();
			return base.IsFrozen;
		}
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. For a description of this member, see <see cref="P:System.Collections.IList.Item(System.Int32)" />.</summary>
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

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. For a description of this member, see <see cref="P:System.Collections.ICollection.IsSynchronized" />.</summary>
	/// <returns>true if access to the <see cref="T:System.Windows.Media.Effects.BitmapEffectCollection" /> is synchronized (thread safe); otherwise, false.</returns>
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

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. For a description of this member, see <see cref="P:System.Collections.ICollection.SyncRoot" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Windows.Media.Effects.BitmapEffectCollection" />.</returns>
	object ICollection.SyncRoot
	{
		get
		{
			ReadPreamble();
			return this;
		}
	}

	internal static BitmapEffectCollection Empty
	{
		get
		{
			if (s_empty == null)
			{
				BitmapEffectCollection bitmapEffectCollection = new BitmapEffectCollection();
				bitmapEffectCollection.Freeze();
				s_empty = bitmapEffectCollection;
			}
			return s_empty;
		}
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Creates a modifiable clone of this <see cref="T:System.Windows.Media.Effects.BitmapEffectCollection" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new BitmapEffectCollection Clone()
	{
		return (BitmapEffectCollection)base.Clone();
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Creates a modifiable clone of this <see cref="T:System.Windows.Media.Effects.BitmapEffectCollection" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new BitmapEffectCollection CloneCurrentValue()
	{
		return (BitmapEffectCollection)base.CloneCurrentValue();
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Adds a <see cref="T:System.Windows.Media.Effects.BitmapEffect" /> at the end of the collection.</summary>
	/// <param name="value">The <see cref="T:System.Windows.Media.Effects.BitmapEffect" /> to add to the end of the collection.</param>
	public void Add(BitmapEffect value)
	{
		AddHelper(value);
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Removes all effects from the collection.</summary>
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

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Indicates whether the collection contains the specified <see cref="T:System.Windows.Media.Effects.BitmapEffect" />.</summary>
	/// <returns>true if the collection contains value; otherwise, false.</returns>
	/// <param name="value">The bitmap effect to locate in the collection.</param>
	public bool Contains(BitmapEffect value)
	{
		ReadPreamble();
		return _collection.Contains(value);
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Retrieves the index of the first instance of the specified <see cref="T:System.Windows.Media.Effects.BitmapEffect" />.</summary>
	/// <returns>The index of the specified effect.</returns>
	/// <param name="value">The effect to find in the collection.</param>
	public int IndexOf(BitmapEffect value)
	{
		ReadPreamble();
		return _collection.IndexOf(value);
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Inserts a <see cref="T:System.Windows.Media.Effects.BitmapEffect" /> into this collection at the specified index.</summary>
	/// <param name="index">The index to insert the effect at.</param>
	/// <param name="value">The specified effect to insert.</param>
	public void Insert(int index, BitmapEffect value)
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

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Removes the first occurrence of the specified <see cref="T:System.Windows.Media.Effects.BitmapEffect" /> for this collection.</summary>
	/// <returns>true if <paramref name="value" /> was removed; otherwise, false. </returns>
	/// <param name="value">The effect to remove from the collection</param>
	public bool Remove(BitmapEffect value)
	{
		WritePreamble();
		int num = IndexOf(value);
		if (num >= 0)
		{
			BitmapEffect oldValue = _collection[num];
			OnFreezablePropertyChanged(oldValue, null);
			_collection.RemoveAt(num);
			_version++;
			WritePostscript();
			return true;
		}
		return false;
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Remove the <see cref="T:System.Windows.Media.Effects.BitmapEffect" /> at the specified index from the collection.</summary>
	/// <param name="index">The index of the effect to remove.</param>
	public void RemoveAt(int index)
	{
		RemoveAtWithoutFiringPublicEvents(index);
		WritePostscript();
	}

	internal void RemoveAtWithoutFiringPublicEvents(int index)
	{
		WritePreamble();
		BitmapEffect oldValue = _collection[index];
		OnFreezablePropertyChanged(oldValue, null);
		_collection.RemoveAt(index);
		_version++;
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Copies the elements of the collection to an array starting at the given index.</summary>
	/// <param name="array">The array to copy to.</param>
	/// <param name="index">The collection index to begin coping.</param>
	public void CopyTo(BitmapEffect[] array, int index)
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

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Returns an enumerator that can iterate through the collection.</summary>
	/// <returns>An <see cref="T:System.Windows.Media.Effects.BitmapEffectCollection.Enumerator" /> that can iterate through the collection.</returns>
	public Enumerator GetEnumerator()
	{
		ReadPreamble();
		return new Enumerator(this);
	}

	IEnumerator<BitmapEffect> IEnumerable<BitmapEffect>.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. For a description of this member, see <see cref="M:System.Collections.IList.Add(System.Object)" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.Media.Effects.BitmapEffectCollection" />.</param>
	int IList.Add(object value)
	{
		return AddHelper(Cast(value));
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. For a description of this member, see <see cref="M:System.Collections.IList.Contains(System.Object)" />.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.Media.Effects.BitmapEffectCollection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.Effects.BitmapEffectCollection" />.</param>
	bool IList.Contains(object value)
	{
		return Contains(value as BitmapEffect);
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. For a description of this member, see <see cref="M:System.Collections.IList.IndexOf(System.Object)" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.Effects.BitmapEffectCollection" />.</param>
	int IList.IndexOf(object value)
	{
		return IndexOf(value as BitmapEffect);
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. For a description of this member, see <see cref="M:System.Collections.IList.Insert(System.Int32,System.Object)" />.</summary>
	/// <param name="index">The zero-based index at which to insert the <see cref="T:System.Object" />.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Windows.Media.Effects.BitmapEffectCollection" />.</param>
	void IList.Insert(int index, object value)
	{
		Insert(index, Cast(value));
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. For a description of this member, see <see cref="M:System.Collections.IList.Remove(System.Object)" />.</summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.Media.Effects.BitmapEffectCollection" />.</param>
	void IList.Remove(object value)
	{
		Remove(value as BitmapEffect);
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items from the <see cref="T:System.Windows.Media.Effects.BitmapEffectCollection" />.</param>
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

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. For a description of this member, see <see cref="M:System.Collections.IEnumerable.GetEnumerator" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	internal BitmapEffect Internal_GetItem(int i)
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

	private BitmapEffect Cast(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is BitmapEffect))
		{
			throw new ArgumentException(SR.Format(SR.Collection_BadType, GetType().Name, value.GetType().Name, "BitmapEffect"));
		}
		return (BitmapEffect)value;
	}

	private int AddHelper(BitmapEffect value)
	{
		int result = AddWithoutFiringPublicEvents(value);
		WritePostscript();
		return result;
	}

	internal int AddWithoutFiringPublicEvents(BitmapEffect value)
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
		return new BitmapEffectCollection();
	}

	protected override void CloneCore(Freezable source)
	{
		BitmapEffectCollection bitmapEffectCollection = (BitmapEffectCollection)source;
		base.CloneCore(source);
		int count = bitmapEffectCollection._collection.Count;
		_collection = new FrugalStructList<BitmapEffect>(count);
		for (int i = 0; i < count; i++)
		{
			BitmapEffect bitmapEffect = bitmapEffectCollection._collection[i].Clone();
			OnFreezablePropertyChanged(null, bitmapEffect);
			_collection.Add(bitmapEffect);
		}
	}

	protected override void CloneCurrentValueCore(Freezable source)
	{
		BitmapEffectCollection bitmapEffectCollection = (BitmapEffectCollection)source;
		base.CloneCurrentValueCore(source);
		int count = bitmapEffectCollection._collection.Count;
		_collection = new FrugalStructList<BitmapEffect>(count);
		for (int i = 0; i < count; i++)
		{
			BitmapEffect bitmapEffect = bitmapEffectCollection._collection[i].CloneCurrentValue();
			OnFreezablePropertyChanged(null, bitmapEffect);
			_collection.Add(bitmapEffect);
		}
	}

	protected override void GetAsFrozenCore(Freezable source)
	{
		BitmapEffectCollection bitmapEffectCollection = (BitmapEffectCollection)source;
		base.GetAsFrozenCore(source);
		int count = bitmapEffectCollection._collection.Count;
		_collection = new FrugalStructList<BitmapEffect>(count);
		for (int i = 0; i < count; i++)
		{
			BitmapEffect bitmapEffect = (BitmapEffect)bitmapEffectCollection._collection[i].GetAsFrozen();
			OnFreezablePropertyChanged(null, bitmapEffect);
			_collection.Add(bitmapEffect);
		}
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable source)
	{
		BitmapEffectCollection bitmapEffectCollection = (BitmapEffectCollection)source;
		base.GetCurrentValueAsFrozenCore(source);
		int count = bitmapEffectCollection._collection.Count;
		_collection = new FrugalStructList<BitmapEffect>(count);
		for (int i = 0; i < count; i++)
		{
			BitmapEffect bitmapEffect = (BitmapEffect)bitmapEffectCollection._collection[i].GetCurrentValueAsFrozen();
			OnFreezablePropertyChanged(null, bitmapEffect);
			_collection.Add(bitmapEffect);
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

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Initializes a new instance of the <see cref="T:System.Windows.Media.Effects.BitmapEffectCollection" /> class.</summary>
	public BitmapEffectCollection()
	{
		_collection = default(FrugalStructList<BitmapEffect>);
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Initializes a new instance the <see cref="T:System.Windows.Media.Effects.BitmapEffectCollection" /> class with a specified capacity, or the number of <see cref="T:System.Windows.Media.Effects.BitmapEffect" /> objects the collection is initially capable of storing.</summary>
	/// <param name="capacity">The initial capacity of the collection.</param>
	public BitmapEffectCollection(int capacity)
	{
		_collection = new FrugalStructList<BitmapEffect>(capacity);
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Initializes a new instance of the <see cref="T:System.Windows.Media.Effects.BitmapEffectCollection" /> class using the given collection.</summary>
	/// <param name="collection">The collection used for initialization.</param>
	public BitmapEffectCollection(IEnumerable<BitmapEffect> collection)
	{
		WritePreamble();
		if (collection != null)
		{
			bool flag = true;
			if (collection is ICollection<BitmapEffect> collection2)
			{
				_collection = new FrugalStructList<BitmapEffect>(collection2);
			}
			else if (collection is ICollection collection3)
			{
				_collection = new FrugalStructList<BitmapEffect>(collection3);
			}
			else
			{
				_collection = default(FrugalStructList<BitmapEffect>);
				foreach (BitmapEffect item in collection)
				{
					BitmapEffect bitmapEffect = item ?? throw new ArgumentException(SR.Collection_NoNull);
					OnFreezablePropertyChanged(null, bitmapEffect);
					_collection.Add(bitmapEffect);
				}
				flag = false;
			}
			if (flag)
			{
				foreach (BitmapEffect item2 in collection)
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
