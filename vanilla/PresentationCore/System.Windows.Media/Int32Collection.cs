using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Markup;
using System.Windows.Media.Converters;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Utility;

namespace System.Windows.Media;

/// <summary>Represents a collection of <see cref="T:System.Int32" /> values. </summary>
[TypeConverter(typeof(Int32CollectionConverter))]
[ValueSerializer(typeof(Int32CollectionValueSerializer))]
public sealed class Int32Collection : Freezable, IFormattable, IList, ICollection, IEnumerable, IList<int>, ICollection<int>, IEnumerable<int>
{
	/// <summary>Enumerates <see cref="T:System.Int32" /> items in a <see cref="T:System.Windows.Media.Int32Collection" />. </summary>
	public struct Enumerator : IEnumerator, IEnumerator<int>, IDisposable
	{
		private int _current;

		private Int32Collection _list;

		private uint _version;

		private int _index;

		/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
		object IEnumerator.Current => Current;

		/// <summary>Gets the current element in the collection.</summary>
		/// <returns>The current element in the collection.</returns>
		public int Current
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

		internal Enumerator(Int32Collection list)
		{
			_list = list;
			_version = list._version;
			_index = -1;
			_current = 0;
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

	private static Int32Collection s_empty;

	internal FrugalStructList<int> _collection;

	internal uint _version;

	/// <summary>Gets or sets the <see cref="T:System.Int32" /> at the specified index position.</summary>
	/// <returns>The <see cref="T:System.Int32" /> at the specified index.</returns>
	/// <param name="index">The zero-based index of the <see cref="T:System.Int32" /> to get or set.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is equal to or greater than the <see cref="P:System.Windows.Media.Int32Collection.Count" />.</exception>
	public int this[int index]
	{
		get
		{
			ReadPreamble();
			return _collection[index];
		}
		set
		{
			WritePreamble();
			_collection[index] = value;
			_version++;
			WritePostscript();
		}
	}

	/// <summary>Gets the number of <see cref="T:System.Int32" /> values contained in the <see cref="T:System.Windows.Media.Int32Collection" />.</summary>
	/// <returns>The number of <see cref="T:System.Int32" /> values contained in the <see cref="T:System.Windows.Media.Int32Collection" />.</returns>
	public int Count
	{
		get
		{
			ReadPreamble();
			return _collection.Count;
		}
	}

	bool ICollection<int>.IsReadOnly
	{
		get
		{
			ReadPreamble();
			return base.IsFrozen;
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsReadOnly" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.Int32Collection" /> is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => ((ICollection<int>)this).IsReadOnly;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsFixedSize" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.Int32Collection" /> has a fixed size; otherwise, false.</returns>
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
	/// <returns>true if access to the <see cref="T:System.Windows.Media.Int32Collection" /> is synchronized (thread safe); otherwise, false.</returns>
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
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Windows.Media.Int32Collection" />.</returns>
	object ICollection.SyncRoot
	{
		get
		{
			ReadPreamble();
			return this;
		}
	}

	internal static Int32Collection Empty
	{
		get
		{
			if (s_empty == null)
			{
				Int32Collection int32Collection = new Int32Collection();
				int32Collection.Freeze();
				s_empty = int32Collection;
			}
			return s_empty;
		}
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Int32Collection" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Int32Collection Clone()
	{
		return (Int32Collection)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Int32Collection" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Int32Collection CloneCurrentValue()
	{
		return (Int32Collection)base.CloneCurrentValue();
	}

	/// <summary>Adds an <see cref="T:System.Int32" /> to the end of the collection.</summary>
	/// <param name="value">The <see cref="T:System.Int32" /> to add to the end of the collection.</param>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.Int32Collection" /> is read-only.- or -The <see cref="T:System.Windows.Media.Int32Collection" /> has a fixed size.</exception>
	public void Add(int value)
	{
		AddHelper(value);
	}

	/// <summary>Removes all <see cref="T:System.Int32" /> values from the collection.</summary>
	public void Clear()
	{
		WritePreamble();
		_collection.Clear();
		_version++;
		WritePostscript();
	}

	/// <summary>Gets a value that indicates whether the collection contains the specified <see cref="T:System.Int32" />.</summary>
	/// <returns>true if the <see cref="T:System.Int32" /> is found in the <see cref="T:System.Windows.Media.Int32Collection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Int32" /> to locate in the collection.</param>
	public bool Contains(int value)
	{
		ReadPreamble();
		return _collection.Contains(value);
	}

	/// <summary>Searches for the specified <see cref="T:System.Int32" /> and returns the zero-based index of the first occurrence within the entire collection.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="value" /> within the entire collection, if found; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Int32" /> to locate in the collection.</param>
	public int IndexOf(int value)
	{
		ReadPreamble();
		return _collection.IndexOf(value);
	}

	/// <summary>Inserts an <see cref="T:System.Int32" /> into a specific location within the collection.</summary>
	/// <param name="index">The index position at which the <see cref="T:System.Int32" /> is inserted.</param>
	/// <param name="value">The <see cref="T:System.Int32" /> to insert in the collection.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the <see cref="T:System.Windows.Media.Int32Collection" />.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.Int32Collection" /> is read-only.- or -The <see cref="T:System.Windows.Media.Int32Collection" /> has a fixed size.</exception>
	public void Insert(int index, int value)
	{
		WritePreamble();
		_collection.Insert(index, value);
		_version++;
		WritePostscript();
	}

	/// <summary>Removes an <see cref="T:System.Int32" /> from the collection.</summary>
	/// <returns>true if <paramref name="value" /> was removed from the <see cref="T:System.Windows.Media.Int32Collection" />; otherwise, false.</returns>
	/// <param name="value">Identifies the <see cref="T:System.Int32" /> to remove from the collection.</param>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.Int32Collection" /> is read-only.- or -The <see cref="T:System.Windows.Media.Int32Collection" /> has a fixed size.</exception>
	public bool Remove(int value)
	{
		WritePreamble();
		int num = IndexOf(value);
		if (num >= 0)
		{
			_collection.RemoveAt(num);
			_version++;
			WritePostscript();
			return true;
		}
		return false;
	}

	/// <summary>Removes the <see cref="T:System.Int32" /> at the specified index position from the collection.</summary>
	/// <param name="index">Index position of the <see cref="T:System.Int32" /> to be removed.</param>
	public void RemoveAt(int index)
	{
		RemoveAtWithoutFiringPublicEvents(index);
		WritePostscript();
	}

	internal void RemoveAtWithoutFiringPublicEvents(int index)
	{
		WritePreamble();
		_collection.RemoveAt(index);
		_version++;
	}

	/// <summary>Copies all of the <see cref="T:System.Int32" /> values in a collection to a specified array.</summary>
	/// <param name="array">Identifies the array to which content is copied.</param>
	/// <param name="index">Index position in the array to which the contents of the collection are copied.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional. -or-The number of items in the source <see cref="T:System.Windows.Media.Int32Collection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
	public void CopyTo(int[] array, int index)
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

	/// <summary>Returns an <see cref="T:System.Windows.Media.Int32Collection.Enumerator" /> that can iterate through the collection.</summary>
	/// <returns>An <see cref="T:System.Windows.Media.Int32Collection.Enumerator" /> that can iterate through the collection.</returns>
	public Enumerator GetEnumerator()
	{
		ReadPreamble();
		return new Enumerator(this);
	}

	IEnumerator<int> IEnumerable<int>.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Add(System.Object)" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.Media.Int32Collection" />.</param>
	int IList.Add(object value)
	{
		return AddHelper(Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Contains(System.Object)" />.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.Media.Int32Collection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.Int32Collection" />.</param>
	bool IList.Contains(object value)
	{
		if (value is int)
		{
			return Contains((int)value);
		}
		return false;
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.IndexOf(System.Object)" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.Int32Collection" />.</param>
	int IList.IndexOf(object value)
	{
		if (value is int)
		{
			return IndexOf((int)value);
		}
		return -1;
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Insert(System.Int32,System.Object)" />.</summary>
	/// <param name="index">The zero-based index at which to insert the <see cref="T:System.Object" />.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Windows.Media.Int32Collection" />.</param>
	void IList.Insert(int index, object value)
	{
		Insert(index, Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Remove(System.Object)" />.</summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.Media.Int32Collection" />.</param>
	void IList.Remove(object value)
	{
		if (value is int)
		{
			Remove((int)value);
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items from the <see cref="T:System.Windows.Media.Int32Collection" />.</param>
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

	internal int Internal_GetItem(int i)
	{
		return _collection[i];
	}

	private int Cast(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is int))
		{
			throw new ArgumentException(SR.Format(SR.Collection_BadType, GetType().Name, value.GetType().Name, "int"));
		}
		return (int)value;
	}

	private int AddHelper(int value)
	{
		int result = AddWithoutFiringPublicEvents(value);
		WritePostscript();
		return result;
	}

	internal int AddWithoutFiringPublicEvents(int value)
	{
		WritePreamble();
		int result = _collection.Add(value);
		_version++;
		return result;
	}

	protected override Freezable CreateInstanceCore()
	{
		return new Int32Collection();
	}

	protected override void CloneCore(Freezable source)
	{
		Int32Collection int32Collection = (Int32Collection)source;
		base.CloneCore(source);
		int count = int32Collection._collection.Count;
		_collection = new FrugalStructList<int>(count);
		for (int i = 0; i < count; i++)
		{
			_collection.Add(int32Collection._collection[i]);
		}
	}

	protected override void CloneCurrentValueCore(Freezable source)
	{
		Int32Collection int32Collection = (Int32Collection)source;
		base.CloneCurrentValueCore(source);
		int count = int32Collection._collection.Count;
		_collection = new FrugalStructList<int>(count);
		for (int i = 0; i < count; i++)
		{
			_collection.Add(int32Collection._collection[i]);
		}
	}

	protected override void GetAsFrozenCore(Freezable source)
	{
		Int32Collection int32Collection = (Int32Collection)source;
		base.GetAsFrozenCore(source);
		int count = int32Collection._collection.Count;
		_collection = new FrugalStructList<int>(count);
		for (int i = 0; i < count; i++)
		{
			_collection.Add(int32Collection._collection[i]);
		}
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable source)
	{
		Int32Collection int32Collection = (Int32Collection)source;
		base.GetCurrentValueAsFrozenCore(source);
		int count = int32Collection._collection.Count;
		_collection = new FrugalStructList<int>(count);
		for (int i = 0; i < count; i++)
		{
			_collection.Add(int32Collection._collection[i]);
		}
	}

	/// <summary>Converts the current value of a <see cref="T:System.Windows.Media.Int32Collection" /> to a <see cref="T:System.String" />.</summary>
	/// <returns>A string representation of the <see cref="T:System.Windows.Media.Int32Collection" />.</returns>
	public override string ToString()
	{
		ReadPreamble();
		return ConvertToString(null, null);
	}

	/// <summary>Converts the current value of a <see cref="T:System.Windows.Media.Int32Collection" /> to a <see cref="T:System.String" /> using the specified culture-specific formatting information.</summary>
	/// <returns>A string representation of the <see cref="T:System.Windows.Media.Int32Collection" />.</returns>
	/// <param name="provider">Culture-specific formatting information.</param>
	public string ToString(IFormatProvider provider)
	{
		ReadPreamble();
		return ConvertToString(null, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IFormattable.ToString(System.String,System.IFormatProvider)" />.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the value of the current instance in the specified format.</returns>
	/// <param name="format">The <see cref="T:System.String" /> specifying the format to use.-or- null (Nothing in Visual Basic) to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation.</param>
	/// <param name="provider">The <see cref="T:System.IFormatProvider" /> to use to format the value.-or- null (Nothing in Visual Basic) to obtain the numeric format information from the current locale setting of the operating system.</param>
	string IFormattable.ToString(string format, IFormatProvider provider)
	{
		ReadPreamble();
		return ConvertToString(format, provider);
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
			stringBuilder.AppendFormat(provider, "{0:" + format + "}", _collection[i]);
			if (i != _collection.Count - 1)
			{
				stringBuilder.Append(' ');
			}
		}
		return stringBuilder.ToString();
	}

	/// <summary>      Returns an instance of <see cref="T:System.Windows.Media.Int32Collection" /> created from a specified string.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Int32Collection" /> created from <paramref name="source" />.</returns>
	/// <param name="source">The string that is converted to an <see cref="T:System.Int32" />.</param>
	public static Int32Collection Parse(string source)
	{
		IFormatProvider invariantEnglishUS = System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS;
		TokenizerHelper tokenizerHelper = new TokenizerHelper(source, invariantEnglishUS);
		Int32Collection int32Collection = new Int32Collection();
		while (tokenizerHelper.NextToken())
		{
			int value = Convert.ToInt32(tokenizerHelper.GetCurrentToken(), invariantEnglishUS);
			int32Collection.Add(value);
		}
		return int32Collection;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Int32Collection" /> class.</summary>
	public Int32Collection()
	{
		_collection = default(FrugalStructList<int>);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Int32Collection" /> class with the specified capacity, or the number of <see cref="T:System.Int32" /> values the collection is initially capable of storing.</summary>
	/// <param name="capacity">The number of <see cref="T:System.Int32" /> values that the collection is initially capable of storing.</param>
	public Int32Collection(int capacity)
	{
		_collection = new FrugalStructList<int>(capacity);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Int32Collection" /> class with the specified collection of <see cref="T:System.Int32" /> values.</summary>
	/// <param name="collection">The collection of <see cref="T:System.Int32" /> values that make up the <see cref="T:System.Windows.Media.Int32Collection" />.</param>
	public Int32Collection(IEnumerable<int> collection)
	{
		WritePreamble();
		if (collection != null)
		{
			if (collection is ICollection<int> collection2)
			{
				_collection = new FrugalStructList<int>(collection2);
			}
			else if (collection is ICollection collection3)
			{
				_collection = new FrugalStructList<int>(collection3);
			}
			else
			{
				_collection = default(FrugalStructList<int>);
				foreach (int item in collection)
				{
					_collection.Add(item);
				}
			}
			WritePostscript();
			return;
		}
		throw new ArgumentNullException("collection");
	}
}
