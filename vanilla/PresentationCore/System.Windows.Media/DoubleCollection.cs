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

/// <summary> Represents an ordered collection of <see cref="T:System.Double" /> values. </summary>
[TypeConverter(typeof(DoubleCollectionConverter))]
[ValueSerializer(typeof(DoubleCollectionValueSerializer))]
public sealed class DoubleCollection : Freezable, IFormattable, IList, ICollection, IEnumerable, IList<double>, ICollection<double>, IEnumerable<double>
{
	/// <summary>Enumerates <see cref="T:System.Double" /> items in a <see cref="T:System.Windows.Media.DoubleCollection" />. </summary>
	public struct Enumerator : IEnumerator, IEnumerator<double>, IDisposable
	{
		private double _current;

		private DoubleCollection _list;

		private uint _version;

		private int _index;

		/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
		object IEnumerator.Current => Current;

		/// <summary>Gets the current element in the collection.</summary>
		/// <returns>The current element in the collection.</returns>
		public double Current
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

		internal Enumerator(DoubleCollection list)
		{
			_list = list;
			_version = list._version;
			_index = -1;
			_current = 0.0;
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

	private static DoubleCollection s_empty;

	internal FrugalStructList<double> _collection;

	internal uint _version;

	/// <summary> Gets or sets the <see cref="T:System.Double" /> at the specified zero-based index. </summary>
	/// <returns>The <see cref="T:System.Double" /> at the specified index.</returns>
	/// <param name="index">The zero-based index of the <see cref="T:System.Double" /> value to get or set. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the <see cref="T:System.Windows.Media.DoubleCollection" />.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.DoubleCollection" /> is read-only.- or -The <see cref="T:System.Windows.Media.DoubleCollection" /> has a fixed size.</exception>
	public double this[int index]
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

	/// <summary> Gets the number of doubles contained in the <see cref="T:System.Windows.Media.DoubleCollection" />.  </summary>
	/// <returns>The number of doubles contained in the <see cref="T:System.Windows.Media.DoubleCollection" />. </returns>
	public int Count
	{
		get
		{
			ReadPreamble();
			return _collection.Count;
		}
	}

	bool ICollection<double>.IsReadOnly
	{
		get
		{
			ReadPreamble();
			return base.IsFrozen;
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsReadOnly" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.DoubleCollection" /> is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => ((ICollection<double>)this).IsReadOnly;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsFixedSize" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.DoubleCollection" /> has a fixed size; otherwise, false.</returns>
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
	/// <returns>true if access to the <see cref="T:System.Windows.Media.DoubleCollection" /> is synchronized (thread safe); otherwise, false.</returns>
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
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Windows.Media.DoubleCollection" />.</returns>
	object ICollection.SyncRoot
	{
		get
		{
			ReadPreamble();
			return this;
		}
	}

	internal static DoubleCollection Empty
	{
		get
		{
			if (s_empty == null)
			{
				DoubleCollection doubleCollection = new DoubleCollection();
				doubleCollection.Freeze();
				s_empty = doubleCollection;
			}
			return s_empty;
		}
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.DoubleCollection" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new DoubleCollection Clone()
	{
		return (DoubleCollection)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.DoubleCollection" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new DoubleCollection CloneCurrentValue()
	{
		return (DoubleCollection)base.CloneCurrentValue();
	}

	/// <summary> Adds a <see cref="T:System.Double" /> to the end of this <see cref="T:System.Windows.Media.DoubleCollection" />. </summary>
	/// <param name="value">The item to add to the end of this collection.</param>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.DoubleCollection" /> is read-only.- or -The <see cref="T:System.Windows.Media.DoubleCollection" /> has a fixed size.</exception>
	public void Add(double value)
	{
		AddHelper(value);
	}

	/// <summary> Removes all the items from this <see cref="T:System.Windows.Media.DoubleCollection" />. </summary>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.DoubleCollection" /> is read-only.</exception>
	public void Clear()
	{
		WritePreamble();
		_collection.Clear();
		_version++;
		WritePostscript();
	}

	/// <summary> Determines whether a <see cref="T:System.Double" /> is in this <see cref="T:System.Windows.Media.DoubleCollection" />. </summary>
	/// <returns>true if <paramref name="value" />, the specified <see cref="T:System.Double" />, is in this <see cref="T:System.Windows.Media.DoubleCollection" />; otherwise, false.</returns>
	/// <param name="value">The item to locate in this collection.</param>
	public bool Contains(double value)
	{
		ReadPreamble();
		return _collection.Contains(value);
	}

	/// <summary> Gets the index of the first occurrence of the specified <see cref="T:System.Double" />. </summary>
	/// <returns>The index of <paramref name="value" /> if found in the <see cref="T:System.Windows.Media.DoubleCollection" />; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Double" /> to locate in the <see cref="T:System.Windows.Media.DoubleCollection" />.</param>
	public int IndexOf(double value)
	{
		ReadPreamble();
		return _collection.IndexOf(value);
	}

	/// <summary> Inserts a <see cref="T:System.Double" /> into this <see cref="T:System.Windows.Media.DoubleCollection" /> at the specified index. </summary>
	/// <param name="index">The index at which to insert <paramref name="value" />, the specified <see cref="T:System.Double" />.</param>
	/// <param name="value">The item to insert.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the <see cref="T:System.Windows.Media.DoubleCollection" />.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.DoubleCollection" /> is read-only.- or -The <see cref="T:System.Windows.Media.DoubleCollection" /> has a fixed size.</exception>
	public void Insert(int index, double value)
	{
		WritePreamble();
		_collection.Insert(index, value);
		_version++;
		WritePostscript();
	}

	/// <summary> Removes the first occurrence of the specified <see cref="T:System.Double" /> from this <see cref="T:System.Windows.Media.DoubleCollection" />. </summary>
	/// <returns>true if <paramref name="value" /> was removed from the <see cref="T:System.Windows.Media.DoubleCollection" />; otherwise, false.</returns>
	/// <param name="value">The item to remove from this collection.</param>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.DoubleCollection" /> is read-only.- or -The <see cref="T:System.Windows.Media.DoubleCollection" /> has a fixed size.</exception>
	public bool Remove(double value)
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

	/// <summary> Removes the <see cref="T:System.Double" /> at the specified index from this <see cref="T:System.Windows.Media.DoubleCollection" />. </summary>
	/// <param name="index">The index of the item to remove.</param>
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

	/// <summary> Copies the items of this <see cref="T:System.Windows.Media.DoubleCollection" />, starting with the specified index, into an array of  <see cref="T:System.Double" /> values. </summary>
	/// <param name="array">The array that is the destination of the items copied from this <see cref="T:System.Windows.Media.DoubleCollection" />.</param>
	/// <param name="index">The index at which to begin copying.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional. -or-The number of items in the source <see cref="T:System.Windows.Media.DoubleCollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
	public void CopyTo(double[] array, int index)
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
	/// <returns>An <see cref="T:System.Windows.Media.DoubleCollection.Enumerator" /> that can iterate through the collection.</returns>
	public Enumerator GetEnumerator()
	{
		ReadPreamble();
		return new Enumerator(this);
	}

	IEnumerator<double> IEnumerable<double>.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Add(System.Object)" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.Media.DoubleCollection" />.</param>
	int IList.Add(object value)
	{
		return AddHelper(Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Contains(System.Object)" />.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.Media.DoubleCollection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.DoubleCollection" />.</param>
	bool IList.Contains(object value)
	{
		if (value is double)
		{
			return Contains((double)value);
		}
		return false;
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.IndexOf(System.Object)" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.DoubleCollection" />.</param>
	int IList.IndexOf(object value)
	{
		if (value is double)
		{
			return IndexOf((double)value);
		}
		return -1;
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Insert(System.Int32,System.Object)" />.</summary>
	/// <param name="index">The zero-based index at which to insert the <see cref="T:System.Object" />.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Windows.Media.DoubleCollection" />.</param>
	void IList.Insert(int index, object value)
	{
		Insert(index, Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Remove(System.Object)" />.</summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.Media.DoubleCollection" />.</param>
	void IList.Remove(object value)
	{
		if (value is double)
		{
			Remove((double)value);
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items from the <see cref="T:System.Windows.Media.DoubleCollection" />.</param>
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

	internal double Internal_GetItem(int i)
	{
		return _collection[i];
	}

	private double Cast(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is double))
		{
			throw new ArgumentException(SR.Format(SR.Collection_BadType, GetType().Name, value.GetType().Name, "double"));
		}
		return (double)value;
	}

	private int AddHelper(double value)
	{
		int result = AddWithoutFiringPublicEvents(value);
		WritePostscript();
		return result;
	}

	internal int AddWithoutFiringPublicEvents(double value)
	{
		WritePreamble();
		int result = _collection.Add(value);
		_version++;
		return result;
	}

	protected override Freezable CreateInstanceCore()
	{
		return new DoubleCollection();
	}

	protected override void CloneCore(Freezable source)
	{
		DoubleCollection doubleCollection = (DoubleCollection)source;
		base.CloneCore(source);
		int count = doubleCollection._collection.Count;
		_collection = new FrugalStructList<double>(count);
		for (int i = 0; i < count; i++)
		{
			_collection.Add(doubleCollection._collection[i]);
		}
	}

	protected override void CloneCurrentValueCore(Freezable source)
	{
		DoubleCollection doubleCollection = (DoubleCollection)source;
		base.CloneCurrentValueCore(source);
		int count = doubleCollection._collection.Count;
		_collection = new FrugalStructList<double>(count);
		for (int i = 0; i < count; i++)
		{
			_collection.Add(doubleCollection._collection[i]);
		}
	}

	protected override void GetAsFrozenCore(Freezable source)
	{
		DoubleCollection doubleCollection = (DoubleCollection)source;
		base.GetAsFrozenCore(source);
		int count = doubleCollection._collection.Count;
		_collection = new FrugalStructList<double>(count);
		for (int i = 0; i < count; i++)
		{
			_collection.Add(doubleCollection._collection[i]);
		}
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable source)
	{
		DoubleCollection doubleCollection = (DoubleCollection)source;
		base.GetCurrentValueAsFrozenCore(source);
		int count = doubleCollection._collection.Count;
		_collection = new FrugalStructList<double>(count);
		for (int i = 0; i < count; i++)
		{
			_collection.Add(doubleCollection._collection[i]);
		}
	}

	/// <summary> Creates a <see cref="T:System.String" /> representation of this <see cref="T:System.Windows.Media.DoubleCollection" />. </summary>
	/// <returns>Returns a <see cref="T:System.String" /> containing the values of this <see cref="T:System.Windows.Media.DoubleCollection" />.</returns>
	public override string ToString()
	{
		ReadPreamble();
		return ConvertToString(null, null);
	}

	/// <summary> Creates a <see cref="T:System.String" /> representation of this <see cref="T:System.Windows.Media.DoubleCollection" />. </summary>
	/// <returns>Returns a <see cref="T:System.String" /> containing the values of this <see cref="T:System.Windows.Media.DoubleCollection" />.</returns>
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

	/// <summary> Converts a <see cref="T:System.String" /> representation of a collection of doubles into an equivalent <see cref="T:System.Windows.Media.DoubleCollection" />. </summary>
	/// <returns>Returns the equivalent <see cref="T:System.Windows.Media.DoubleCollection" />.</returns>
	/// <param name="source">The <see cref="T:System.String" /> representation of the collection of doubles. </param>
	public static DoubleCollection Parse(string source)
	{
		IFormatProvider invariantEnglishUS = System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS;
		TokenizerHelper tokenizerHelper = new TokenizerHelper(source, invariantEnglishUS);
		DoubleCollection doubleCollection = new DoubleCollection();
		while (tokenizerHelper.NextToken())
		{
			double value = Convert.ToDouble(tokenizerHelper.GetCurrentToken(), invariantEnglishUS);
			doubleCollection.Add(value);
		}
		return doubleCollection;
	}

	/// <summary> Initializes a new instance of a <see cref="T:System.Windows.Media.DoubleCollection" />. </summary>
	public DoubleCollection()
	{
		_collection = default(FrugalStructList<double>);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.DoubleCollection" /> class with the specified capacity, or the number of <see cref="T:System.Double" /> values the collection is initially capable of storing.</summary>
	/// <param name="capacity">The number of <see cref="T:System.Double" /> values that the collection is initially capable of storing.</param>
	public DoubleCollection(int capacity)
	{
		_collection = new FrugalStructList<double>(capacity);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.DoubleCollection" /> class with the specified collection of <see cref="T:System.Double" /> values.</summary>
	/// <param name="collection">The collection of <see cref="T:System.Double" /> values that make up the <see cref="T:System.Windows.Media.DoubleCollection" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="collection" /> is null.</exception>
	public DoubleCollection(IEnumerable<double> collection)
	{
		WritePreamble();
		if (collection != null)
		{
			if (collection is ICollection<double> collection2)
			{
				_collection = new FrugalStructList<double>(collection2);
			}
			else if (collection is ICollection collection3)
			{
				_collection = new FrugalStructList<double>(collection3);
			}
			else
			{
				_collection = default(FrugalStructList<double>);
				foreach (double item in collection)
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
