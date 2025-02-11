using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Markup;
using System.Windows.Media.Media3D.Converters;
using MS.Internal;
using MS.Internal.Media;
using MS.Internal.PresentationCore;
using MS.Utility;

namespace System.Windows.Media.Media3D;

/// <summary>Represents an ordered collection of <see cref="T:System.Windows.Media.Media3D.Point3D" /> objects. </summary>
[TypeConverter(typeof(Point3DCollectionConverter))]
[ValueSerializer(typeof(Point3DCollectionValueSerializer))]
public sealed class Point3DCollection : Freezable, IFormattable, IList, ICollection, IEnumerable, IList<Point3D>, ICollection<Point3D>, IEnumerable<Point3D>
{
	/// <summary>Enumerates items in a <see cref="T:System.Windows.Media.Media3D.Point3DCollection" />. </summary>
	public struct Enumerator : IEnumerator, IEnumerator<Point3D>, IDisposable
	{
		private Point3D _current;

		private Point3DCollection _list;

		private uint _version;

		private int _index;

		/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
		object IEnumerator.Current => Current;

		/// <summary> Gets the current <see cref="T:System.Windows.Media.Media3D.Point3D" /> in the collection. </summary>
		/// <returns>
		///   <see cref="T:System.Windows.Media.Media3D.Point3D" /> at the current position in the collection.</returns>
		public Point3D Current
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

		internal Enumerator(Point3DCollection list)
		{
			_list = list;
			_version = list._version;
			_index = -1;
			_current = default(Point3D);
		}

		/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
		void IDisposable.Dispose()
		{
		}

		/// <summary>Advances the enumerator to the next element of the collection. </summary>
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

	private static Point3DCollection s_empty;

	internal FrugalStructList<Point3D> _collection;

	internal uint _version;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Media3D.Point3D" /> at the specified zero-based index.</summary>
	/// <returns>The item at the specified index.</returns>
	/// <param name="index">The zero-based index of the Point3D object to get or set.</param>
	public Point3D this[int index]
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

	/// <summary>Gets the number of <see cref="T:System.Windows.Media.Media3D.Point3D" /> objects contained in the <see cref="T:System.Windows.Media.Media3D.Point3DCollection" />.</summary>
	/// <returns>The number of <see cref="T:System.Windows.Media.Media3D.Point3D" /> objects contained in the <see cref="T:System.Windows.Media.Media3D.Point3DCollection" />.</returns>
	public int Count
	{
		get
		{
			ReadPreamble();
			return _collection.Count;
		}
	}

	bool ICollection<Point3D>.IsReadOnly
	{
		get
		{
			ReadPreamble();
			return base.IsFrozen;
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsReadOnly" />.</summary>
	/// <returns>true if the the <see cref="T:System.Windows.Media.Media3D.Point3DCollection" /> is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => ((ICollection<Point3D>)this).IsReadOnly;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsFixedSize" />.</summary>
	/// <returns>For a description of this member, see <see cref="P:System.Collections.IList.IsFixedSize" />.</returns>
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
	/// <returns>true if access to the <see cref="T:System.Windows.Media.Media3D.Point3DCollection" /> is synchronized (thread safe); otherwise, false.</returns>
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
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Windows.Media.Media3D.Point3DCollection" />.</returns>
	object ICollection.SyncRoot
	{
		get
		{
			ReadPreamble();
			return this;
		}
	}

	internal static Point3DCollection Empty
	{
		get
		{
			if (s_empty == null)
			{
				Point3DCollection point3DCollection = new Point3DCollection();
				point3DCollection.Freeze();
				s_empty = point3DCollection;
			}
			return s_empty;
		}
	}

	[FriendAccessAllowed]
	internal static object DeserializeFrom(BinaryReader reader)
	{
		uint num = reader.ReadUInt32();
		Point3DCollection point3DCollection = new Point3DCollection((int)num);
		for (uint num2 = 0u; num2 < num; num2++)
		{
			Point3D value = new Point3D(XamlSerializationHelper.ReadDouble(reader), XamlSerializationHelper.ReadDouble(reader), XamlSerializationHelper.ReadDouble(reader));
			point3DCollection.Add(value);
		}
		return point3DCollection;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.Point3DCollection" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Point3DCollection Clone()
	{
		return (Point3DCollection)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.Point3DCollection" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Point3DCollection CloneCurrentValue()
	{
		return (Point3DCollection)base.CloneCurrentValue();
	}

	/// <summary>Adds a <see cref="T:System.Windows.Media.Media3D.Point3D" /> object to the end of the <see cref="T:System.Windows.Media.Media3D.Point3DCollection" />.</summary>
	/// <param name="value">Item to add to the end of this collection.</param>
	public void Add(Point3D value)
	{
		AddHelper(value);
	}

	/// <summary>Removes all the items from this <see cref="T:System.Windows.Media.Media3D.Point3DCollection" />.</summary>
	public void Clear()
	{
		WritePreamble();
		_collection.Clear();
		_version++;
		WritePostscript();
	}

	/// <summary>Determines whether the specified <see cref="T:System.Windows.Media.Media3D.Point3D" /> is in this <see cref="T:System.Windows.Media.Media3D.Point3DCollection" />.</summary>
	/// <returns>true if <paramref name="value" />, the specified Point3D, is in this Point3DCollection; otherwise, false.</returns>
	/// <param name="value">The item to locate in this collection.</param>
	public bool Contains(Point3D value)
	{
		ReadPreamble();
		return _collection.Contains(value);
	}

	/// <summary>Gets the index position of the first occurrence of the specified <see cref="T:System.Windows.Media.Media3D.Point3D" />.</summary>
	/// <returns>The index position of the specified Point3D.</returns>
	/// <param name="value">The Point3D to search for.</param>
	public int IndexOf(Point3D value)
	{
		ReadPreamble();
		return _collection.IndexOf(value);
	}

	/// <summary>Inserts a <see cref="T:System.Windows.Media.Media3D.Point3D" /> into this <see cref="T:System.Windows.Media.Media3D.Point3DCollection" /> at the specified index position.</summary>
	/// <param name="index">The index position at which to insert the specified Point3D.</param>
	/// <param name="value">The Point3D to insert.</param>
	public void Insert(int index, Point3D value)
	{
		WritePreamble();
		_collection.Insert(index, value);
		_version++;
		WritePostscript();
	}

	/// <summary>Removes the first occurrence of the specified <see cref="T:System.Windows.Media.Media3D.Point3D" /> from the <see cref="T:System.Windows.Media.Media3D.Point3DCollection" />.</summary>
	/// <returns>true if <paramref name="value" /> was removed from the collection; otherwise, false. </returns>
	/// <param name="value">The Point3D to remove from this collection.</param>
	public bool Remove(Point3D value)
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

	/// <summary>Removes the <see cref="T:System.Windows.Media.Media3D.Point3D" /> at the specified index position from the <see cref="T:System.Windows.Media.Media3D.Point3DCollection" />.</summary>
	/// <param name="index">The index position of the Point3D to remove.</param>
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

	/// <summary>Copies the items of this <see cref="T:System.Windows.Media.Media3D.Point3DCollection" />, starting with the specified index value, into an array of <see cref="T:System.Windows.Media.Media3D.Point3D" /> objects.</summary>
	/// <param name="array">The array that is the destination of the items copied from this Point3DCollection.</param>
	/// <param name="index">The index at which copying begins.</param>
	public void CopyTo(Point3D[] array, int index)
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

	IEnumerator<Point3D> IEnumerable<Point3D>.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Add(System.Object)" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.Media.Media3D.Point3DCollection" />.</param>
	int IList.Add(object value)
	{
		return AddHelper(Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Contains(System.Object)" />.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.Media.Media3D.Point3DCollection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.Media3D.Point3DCollection" />.</param>
	bool IList.Contains(object value)
	{
		if (value is Point3D)
		{
			return Contains((Point3D)value);
		}
		return false;
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.IndexOf(System.Object)" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.Media3D.Point3DCollection" />.</param>
	int IList.IndexOf(object value)
	{
		if (value is Point3D)
		{
			return IndexOf((Point3D)value);
		}
		return -1;
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Insert(System.Int32,System.Object)" />.</summary>
	/// <param name="index">The zero-based index at which to insert the <see cref="T:System.Object" />.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Windows.Media.Media3D.Point3DCollection" />.</param>
	void IList.Insert(int index, object value)
	{
		Insert(index, Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Remove(System.Object)" />.</summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.Media.Media3D.Point3DCollection" />.</param>
	void IList.Remove(object value)
	{
		if (value is Point3D)
		{
			Remove((Point3D)value);
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items from the <see cref="T:System.Windows.Media.Media3D.Point3DCollection" />.</param>
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

	internal Point3D Internal_GetItem(int i)
	{
		return _collection[i];
	}

	private Point3D Cast(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is Point3D))
		{
			throw new ArgumentException(SR.Format(SR.Collection_BadType, GetType().Name, value.GetType().Name, "Point3D"));
		}
		return (Point3D)value;
	}

	private int AddHelper(Point3D value)
	{
		int result = AddWithoutFiringPublicEvents(value);
		WritePostscript();
		return result;
	}

	internal int AddWithoutFiringPublicEvents(Point3D value)
	{
		WritePreamble();
		int result = _collection.Add(value);
		_version++;
		return result;
	}

	protected override Freezable CreateInstanceCore()
	{
		return new Point3DCollection();
	}

	protected override void CloneCore(Freezable source)
	{
		Point3DCollection point3DCollection = (Point3DCollection)source;
		base.CloneCore(source);
		int count = point3DCollection._collection.Count;
		_collection = new FrugalStructList<Point3D>(count);
		for (int i = 0; i < count; i++)
		{
			_collection.Add(point3DCollection._collection[i]);
		}
	}

	protected override void CloneCurrentValueCore(Freezable source)
	{
		Point3DCollection point3DCollection = (Point3DCollection)source;
		base.CloneCurrentValueCore(source);
		int count = point3DCollection._collection.Count;
		_collection = new FrugalStructList<Point3D>(count);
		for (int i = 0; i < count; i++)
		{
			_collection.Add(point3DCollection._collection[i]);
		}
	}

	protected override void GetAsFrozenCore(Freezable source)
	{
		Point3DCollection point3DCollection = (Point3DCollection)source;
		base.GetAsFrozenCore(source);
		int count = point3DCollection._collection.Count;
		_collection = new FrugalStructList<Point3D>(count);
		for (int i = 0; i < count; i++)
		{
			_collection.Add(point3DCollection._collection[i]);
		}
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable source)
	{
		Point3DCollection point3DCollection = (Point3DCollection)source;
		base.GetCurrentValueAsFrozenCore(source);
		int count = point3DCollection._collection.Count;
		_collection = new FrugalStructList<Point3D>(count);
		for (int i = 0; i < count; i++)
		{
			_collection.Add(point3DCollection._collection[i]);
		}
	}

	/// <summary> Creates a string representation of this <see cref="T:System.Windows.Media.Media3D.Point3DCollection" />. </summary>
	/// <returns>String representation of the object.</returns>
	public override string ToString()
	{
		ReadPreamble();
		return ConvertToString(null, null);
	}

	/// <summary>Creates a string representation of this <see cref="T:System.Windows.Media.Media3D.Point3DCollection" />. </summary>
	/// <returns>String representation of the object.</returns>
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

	/// <summary>Converts a string representation of a collection of <see cref="T:System.Windows.Media.Media3D.Point3D" /> objects into an equivalent <see cref="T:System.Windows.Media.Media3D.Point3DCollection" />.</summary>
	/// <returns>The equivalent Point3DCollection.</returns>
	/// <param name="source">The string representation of the collection of Point3D objects.</param>
	public static Point3DCollection Parse(string source)
	{
		IFormatProvider invariantEnglishUS = System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS;
		TokenizerHelper tokenizerHelper = new TokenizerHelper(source, invariantEnglishUS);
		Point3DCollection point3DCollection = new Point3DCollection();
		while (tokenizerHelper.NextToken())
		{
			Point3D value = new Point3D(Convert.ToDouble(tokenizerHelper.GetCurrentToken(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS));
			point3DCollection.Add(value);
		}
		return point3DCollection;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.Point3DCollection" /> class.</summary>
	public Point3DCollection()
	{
		_collection = default(FrugalStructList<Point3D>);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.Point3DCollection" /> class with the specified capacity.</summary>
	/// <param name="capacity">Integer that specifies the capacity of the Point3DCollection.</param>
	public Point3DCollection(int capacity)
	{
		_collection = new FrugalStructList<Point3D>(capacity);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.Point3DCollection" /> class using the specified collection.</summary>
	/// <param name="collection">Collection with which to instantiate the Point3DCollection.</param>
	public Point3DCollection(IEnumerable<Point3D> collection)
	{
		WritePreamble();
		if (collection != null)
		{
			if (collection is ICollection<Point3D> collection2)
			{
				_collection = new FrugalStructList<Point3D>(collection2);
			}
			else if (collection is ICollection collection3)
			{
				_collection = new FrugalStructList<Point3D>(collection3);
			}
			else
			{
				_collection = default(FrugalStructList<Point3D>);
				foreach (Point3D item in collection)
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
