using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Markup;
using System.Windows.Media.Converters;
using MS.Internal;
using MS.Internal.Media;
using MS.Internal.PresentationCore;
using MS.Utility;

namespace System.Windows.Media;

/// <summary> Represents a collection of <see cref="T:System.Windows.Point" /> values that can be individually accessed by index. </summary>
[TypeConverter(typeof(PointCollectionConverter))]
[ValueSerializer(typeof(PointCollectionValueSerializer))]
public sealed class PointCollection : Freezable, IFormattable, IList, ICollection, IEnumerable, IList<Point>, ICollection<Point>, IEnumerable<Point>
{
	/// <summary>Enumerates <see cref="T:System.Windows.Point" /> items in a <see cref="T:System.Windows.Media.PointCollection" />. </summary>
	public struct Enumerator : IEnumerator, IEnumerator<Point>, IDisposable
	{
		private Point _current;

		private PointCollection _list;

		private uint _version;

		private int _index;

		/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
		object IEnumerator.Current => Current;

		/// <summary>Gets the current element in the collection.</summary>
		/// <returns>The current element in the collection.</returns>
		/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Media.PointCollection" /> was modified after the enumerator was created.</exception>
		public Point Current
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

		internal Enumerator(PointCollection list)
		{
			_list = list;
			_version = list._version;
			_index = -1;
			_current = default(Point);
		}

		/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
		void IDisposable.Dispose()
		{
		}

		/// <summary>Advances the enumerator to the next element in the collection. </summary>
		/// <returns>true if the enumerator successfully advanced to the next element; otherwise, false.</returns>
		/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Media.PointCollection" /> was modified after the enumerator was created.</exception>
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
		/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Media.PointCollection" /> was modified after the enumerator was created.</exception>
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

	private static PointCollection s_empty;

	internal FrugalStructList<Point> _collection;

	internal uint _version;

	/// <summary> Gets or sets the <see cref="T:System.Windows.Point" /> at the specified index. </summary>
	/// <returns>The <see cref="T:System.Windows.Point" /> at the specified index.</returns>
	/// <param name="index">The zero-based index of the <see cref="T:System.Windows.Point" /> to get or set.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the <see cref="T:System.Windows.Media.PointCollection" />.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.PointCollection" /> is read-only.- or -The <see cref="T:System.Windows.Media.PointCollection" /> has a fixed size.</exception>
	public Point this[int index]
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

	/// <summary> Gets the number of items contained in the <see cref="T:System.Windows.Media.PointCollection" />. </summary>
	/// <returns>The number of items in the <see cref="T:System.Windows.Media.PointCollection" />.</returns>
	public int Count
	{
		get
		{
			ReadPreamble();
			return _collection.Count;
		}
	}

	bool ICollection<Point>.IsReadOnly
	{
		get
		{
			ReadPreamble();
			return base.IsFrozen;
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsReadOnly" />.</summary>
	/// <returns>true if the the <see cref="T:System.Windows.Media.PointCollection" /> is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => ((ICollection<Point>)this).IsReadOnly;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsFixedSize" />.</summary>
	/// <returns>true if the the <see cref="T:System.Windows.Media.PointCollection" /> has a fixed size; otherwise, false.</returns>
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
	/// <returns>true if access to the <see cref="T:System.Windows.Media.PointCollection" /> is synchronized (thread safe); otherwise, false.</returns>
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
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Windows.Media.PointCollection" />.</returns>
	object ICollection.SyncRoot
	{
		get
		{
			ReadPreamble();
			return this;
		}
	}

	internal static PointCollection Empty
	{
		get
		{
			if (s_empty == null)
			{
				PointCollection pointCollection = new PointCollection();
				pointCollection.Freeze();
				s_empty = pointCollection;
			}
			return s_empty;
		}
	}

	[FriendAccessAllowed]
	internal static object DeserializeFrom(BinaryReader reader)
	{
		uint num = reader.ReadUInt32();
		PointCollection pointCollection = new PointCollection((int)num);
		for (uint num2 = 0u; num2 < num; num2++)
		{
			Point value = new Point(XamlSerializationHelper.ReadDouble(reader), XamlSerializationHelper.ReadDouble(reader));
			pointCollection.Add(value);
		}
		return pointCollection;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.PointCollection" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new PointCollection Clone()
	{
		return (PointCollection)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.PointCollection" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new PointCollection CloneCurrentValue()
	{
		return (PointCollection)base.CloneCurrentValue();
	}

	/// <summary>Adds a <see cref="T:System.Windows.Point" /> to the end of the <see cref="T:System.Windows.Media.PointCollection" />. </summary>
	/// <param name="value">The <see cref="T:System.Windows.Point" /> to add to the end of the <see cref="T:System.Windows.Media.PointCollection" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.PointCollection" /> is read-only.- or -The <see cref="T:System.Windows.Media.PointCollection" /> has a fixed size.</exception>
	public void Add(Point value)
	{
		AddHelper(value);
	}

	/// <summary>Removes all items from the <see cref="T:System.Windows.Media.PointCollection" />. </summary>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.PointCollection" /> is read-only.</exception>
	public void Clear()
	{
		WritePreamble();
		_collection.Clear();
		_version++;
		WritePostscript();
	}

	/// <summary>Determines whether the <see cref="T:System.Windows.Media.PointCollection" /> contains the specified <see cref="T:System.Windows.Point" />. </summary>
	/// <returns>true if the <see cref="T:System.Windows.Point" /> is found in the <see cref="T:System.Windows.Media.PointCollection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Windows.Point" /> to locate in the <see cref="T:System.Windows.Media.PointCollection" />. </param>
	public bool Contains(Point value)
	{
		ReadPreamble();
		return _collection.Contains(value);
	}

	/// <summary>Determines the index of the specified item in the <see cref="T:System.Windows.Media.PointCollection" />. </summary>
	/// <returns>The index of <paramref name="value" /> if found in the <see cref="T:System.Windows.Media.PointCollection" />; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Windows.Point" /> to locate in the <see cref="T:System.Windows.Media.PointCollection" />.</param>
	public int IndexOf(Point value)
	{
		ReadPreamble();
		return _collection.IndexOf(value);
	}

	/// <summary> Inserts a <see cref="T:System.Windows.Point" /> into the <see cref="T:System.Windows.Media.PointCollection" /> at the specified index. </summary>
	/// <param name="index">The zero-based index at which <paramref name="value" /> should be inserted.</param>
	/// <param name="value">The <see cref="T:System.Windows.Point" /> to insert into the <see cref="T:System.Windows.Media.PointCollection" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the <see cref="T:System.Windows.Media.PointCollection" />.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.PointCollection" /> is read-only.- or -The <see cref="T:System.Windows.Media.PointCollection" /> has a fixed size.</exception>
	public void Insert(int index, Point value)
	{
		WritePreamble();
		_collection.Insert(index, value);
		_version++;
		WritePostscript();
	}

	/// <summary>Removes the first occurrence of the specified <see cref="T:System.Windows.Point" /> from the <see cref="T:System.Windows.Media.PointCollection" />. </summary>
	/// <returns>true if <paramref name="value" /> was removed from the <see cref="T:System.Windows.Media.PointCollection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Windows.Point" /> to remove from the <see cref="T:System.Windows.Media.PointCollection" />.</param>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.PointCollection" /> is read-only.- or -The <see cref="T:System.Windows.Media.PointCollection" /> has a fixed size.</exception>
	public bool Remove(Point value)
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

	/// <summary> Removes the <see cref="T:System.Windows.Point" /> at the specified index. </summary>
	/// <param name="index">The zero-based index of the <see cref="T:System.Windows.Point" /> to remove.</param>
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

	/// <summary>Copies the items of the <see cref="T:System.Windows.Media.PointCollection" /> to an array, starting at the specified array index. </summary>
	/// <param name="array">The one-dimensional array that is the destination of the items copied from the <see cref="T:System.Windows.Media.PointCollection" />. The array must have zero-based indexing.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional. -or-The number of items in the source <see cref="T:System.Windows.Media.PointCollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
	public void CopyTo(Point[] array, int index)
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

	/// <summary>Returns an enumerator that can iterate through the <see cref="T:System.Windows.Media.PointCollection" />. </summary>
	/// <returns>An <see cref="T:System.Windows.Media.PointCollection.Enumerator" /> that can be used to iterate through the <see cref="T:System.Windows.Media.PointCollection" />.</returns>
	public Enumerator GetEnumerator()
	{
		ReadPreamble();
		return new Enumerator(this);
	}

	IEnumerator<Point> IEnumerable<Point>.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Add(System.Object)" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.Media.PointCollection" />.</param>
	int IList.Add(object value)
	{
		return AddHelper(Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Contains(System.Object)" />.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.Media.PointCollection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.PointCollection" />.</param>
	bool IList.Contains(object value)
	{
		if (value is Point)
		{
			return Contains((Point)value);
		}
		return false;
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.IndexOf(System.Object)" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.PointCollection" />.</param>
	int IList.IndexOf(object value)
	{
		if (value is Point)
		{
			return IndexOf((Point)value);
		}
		return -1;
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Insert(System.Int32,System.Object)" />.</summary>
	/// <param name="index">The zero-based index at which to insert the <see cref="T:System.Object" />.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Windows.Media.PointCollection" />.</param>
	void IList.Insert(int index, object value)
	{
		Insert(index, Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Remove(System.Object)" />.</summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.Media.PointCollection" />.</param>
	void IList.Remove(object value)
	{
		if (value is Point)
		{
			Remove((Point)value);
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items from the <see cref="T:System.Windows.Media.PointCollection" />.</param>
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

	internal Point Internal_GetItem(int i)
	{
		return _collection[i];
	}

	private Point Cast(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is Point))
		{
			throw new ArgumentException(SR.Format(SR.Collection_BadType, GetType().Name, value.GetType().Name, "Point"));
		}
		return (Point)value;
	}

	private int AddHelper(Point value)
	{
		int result = AddWithoutFiringPublicEvents(value);
		WritePostscript();
		return result;
	}

	internal int AddWithoutFiringPublicEvents(Point value)
	{
		WritePreamble();
		int result = _collection.Add(value);
		_version++;
		return result;
	}

	protected override Freezable CreateInstanceCore()
	{
		return new PointCollection();
	}

	protected override void CloneCore(Freezable source)
	{
		PointCollection pointCollection = (PointCollection)source;
		base.CloneCore(source);
		int count = pointCollection._collection.Count;
		_collection = new FrugalStructList<Point>(count);
		for (int i = 0; i < count; i++)
		{
			_collection.Add(pointCollection._collection[i]);
		}
	}

	protected override void CloneCurrentValueCore(Freezable source)
	{
		PointCollection pointCollection = (PointCollection)source;
		base.CloneCurrentValueCore(source);
		int count = pointCollection._collection.Count;
		_collection = new FrugalStructList<Point>(count);
		for (int i = 0; i < count; i++)
		{
			_collection.Add(pointCollection._collection[i]);
		}
	}

	protected override void GetAsFrozenCore(Freezable source)
	{
		PointCollection pointCollection = (PointCollection)source;
		base.GetAsFrozenCore(source);
		int count = pointCollection._collection.Count;
		_collection = new FrugalStructList<Point>(count);
		for (int i = 0; i < count; i++)
		{
			_collection.Add(pointCollection._collection[i]);
		}
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable source)
	{
		PointCollection pointCollection = (PointCollection)source;
		base.GetCurrentValueAsFrozenCore(source);
		int count = pointCollection._collection.Count;
		_collection = new FrugalStructList<Point>(count);
		for (int i = 0; i < count; i++)
		{
			_collection.Add(pointCollection._collection[i]);
		}
	}

	/// <summary>Creates a <see cref="T:System.String" /> representation of this <see cref="T:System.Windows.Media.PointCollection" />. </summary>
	/// <returns>Returns a <see cref="T:System.String" /> containing the <see cref="P:System.Windows.Point.X" /> and <see cref="P:System.Windows.Point.Y" /> values of the <see cref="T:System.Windows.Point" /> structures in this <see cref="T:System.Windows.Media.PointCollection" />.</returns>
	public override string ToString()
	{
		ReadPreamble();
		return ConvertToString(null, null);
	}

	/// <summary>Creates a <see cref="T:System.String" /> representation of this <see cref="T:System.Windows.Media.PointCollection" />. </summary>
	/// <returns>Returns a <see cref="T:System.String" /> containing the <see cref="P:System.Windows.Point.X" /> and <see cref="P:System.Windows.Point.Y" /> values of the <see cref="T:System.Windows.Point" /> structures in this <see cref="T:System.Windows.Media.PointCollection" />.</returns>
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

	/// <summary>Converts a <see cref="T:System.String" /> representation of a collection of points into an equivalent <see cref="T:System.Windows.Media.PointCollection" />.      </summary>
	/// <returns>The equivalent <see cref="T:System.Windows.Media.PointCollection" />.</returns>
	/// <param name="source">The <see cref="T:System.String" /> representation of the collection of points.</param>
	public static PointCollection Parse(string source)
	{
		IFormatProvider invariantEnglishUS = System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS;
		TokenizerHelper tokenizerHelper = new TokenizerHelper(source, invariantEnglishUS);
		PointCollection pointCollection = new PointCollection();
		while (tokenizerHelper.NextToken())
		{
			Point value = new Point(Convert.ToDouble(tokenizerHelper.GetCurrentToken(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS));
			pointCollection.Add(value);
		}
		return pointCollection;
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.PointCollection" /> class. </summary>
	public PointCollection()
	{
		_collection = default(FrugalStructList<Point>);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.PointCollection" /> class with the specified capacity.</summary>
	/// <param name="capacity">The number of <see cref="T:System.Windows.Point" /> values that the collection is initially capable of storing.</param>
	public PointCollection(int capacity)
	{
		_collection = new FrugalStructList<Point>(capacity);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.PointCollection" /> class that contains items copied from the specified collection of <see cref="T:System.Windows.Point" /> values and has the same initial capacity as the number of items copied.</summary>
	/// <param name="collection">The collection whose items are copied to the new <see cref="T:System.Windows.Media.PointCollection" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="collection" /> is null.</exception>
	public PointCollection(IEnumerable<Point> collection)
	{
		WritePreamble();
		if (collection != null)
		{
			if (collection is ICollection<Point> collection2)
			{
				_collection = new FrugalStructList<Point>(collection2);
			}
			else if (collection is ICollection collection3)
			{
				_collection = new FrugalStructList<Point>(collection3);
			}
			else
			{
				_collection = default(FrugalStructList<Point>);
				foreach (Point item in collection)
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
