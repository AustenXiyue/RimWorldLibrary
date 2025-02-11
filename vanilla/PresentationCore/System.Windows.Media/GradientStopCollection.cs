using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Utility;

namespace System.Windows.Media;

/// <summary>Represents a collection of <see cref="T:System.Windows.Media.GradientStop" /> objects that can be individually accessed by index. </summary>
public sealed class GradientStopCollection : Animatable, IFormattable, IList, ICollection, IEnumerable, IList<GradientStop>, ICollection<GradientStop>, IEnumerable<GradientStop>
{
	/// <summary>Enumerates <see cref="T:System.Windows.Media.GradientStop" /> items in a <see cref="T:System.Windows.Media.GradientStopCollection" />. </summary>
	public struct Enumerator : IEnumerator, IEnumerator<GradientStop>, IDisposable
	{
		private GradientStop _current;

		private GradientStopCollection _list;

		private uint _version;

		private int _index;

		/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
		object IEnumerator.Current => Current;

		/// <summary>Gets the current element in the collection.</summary>
		/// <returns>The current element in the collection.</returns>
		public GradientStop Current
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

		internal Enumerator(GradientStopCollection list)
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

	private static GradientStopCollection s_empty;

	internal FrugalStructList<GradientStop> _collection;

	internal uint _version;

	/// <summary> Gets or sets the <see cref="T:System.Windows.Media.GradientStop" /> at the specified zero-based index. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.GradientStop" /> at the specified index.</returns>
	/// <param name="index">The zero-based index of the <see cref="T:System.Windows.Media.GradientStop" /> to get or set.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the <see cref="T:System.Windows.Media.GradientStopCollection" />.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.GradientStopCollection" /> is read-only.- or -The <see cref="T:System.Windows.Media.GradientStopCollection" /> has a fixed size.</exception>
	public GradientStop this[int index]
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
				GradientStop oldValue = _collection[index];
				OnFreezablePropertyChanged(oldValue, value);
				_collection[index] = value;
			}
			_version++;
			WritePostscript();
		}
	}

	/// <summary> Gets the number of items contained in a <see cref="T:System.Windows.Media.GradientStopCollection" />. </summary>
	/// <returns>The number of items in a <see cref="T:System.Windows.Media.GradientStopCollection" />.</returns>
	public int Count
	{
		get
		{
			ReadPreamble();
			return _collection.Count;
		}
	}

	bool ICollection<GradientStop>.IsReadOnly
	{
		get
		{
			ReadPreamble();
			return base.IsFrozen;
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsReadOnly" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.GradientStopCollection" /> is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => ((ICollection<GradientStop>)this).IsReadOnly;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsFixedSize" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.GradientStopCollection" /> has a fixed size; otherwise, false.</returns>
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
	/// <returns>true if access to the <see cref="T:System.Windows.Media.GradientStopCollection" /> is synchronized (thread safe); otherwise, false.</returns>
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
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Windows.Media.GradientStopCollection" />.</returns>
	object ICollection.SyncRoot
	{
		get
		{
			ReadPreamble();
			return this;
		}
	}

	internal static GradientStopCollection Empty
	{
		get
		{
			if (s_empty == null)
			{
				GradientStopCollection gradientStopCollection = new GradientStopCollection();
				gradientStopCollection.Freeze();
				s_empty = gradientStopCollection;
			}
			return s_empty;
		}
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.GradientStopCollection" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new GradientStopCollection Clone()
	{
		return (GradientStopCollection)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.GradientStopCollection" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new GradientStopCollection CloneCurrentValue()
	{
		return (GradientStopCollection)base.CloneCurrentValue();
	}

	/// <summary> Adds a <see cref="T:System.Windows.Media.GradientStop" /> to the gradient stop collection. </summary>
	/// <param name="value">The <see cref="T:System.Windows.Media.GradientStop" /> to add to the end of the <see cref="T:System.Windows.Media.GradientStopCollection" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.GradientStopCollection" /> is read-only.- or -The <see cref="T:System.Windows.Media.GradientStopCollection" /> has a fixed size.</exception>
	public void Add(GradientStop value)
	{
		AddHelper(value);
	}

	/// <summary> Removes all items from the gradient stop list. </summary>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.GradientStopCollection" /> is read-only.</exception>
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

	/// <summary> Determines whether the collection contains the specified <see cref="T:System.Windows.Media.GradientStop" />. </summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.GradientStop" /> is found in the <see cref="T:System.Windows.Media.GradientStopCollection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Windows.Media.GradientStop" /> to locate in the <see cref="T:System.Windows.Media.GradientStopCollection" />. </param>
	public bool Contains(GradientStop value)
	{
		ReadPreamble();
		return _collection.Contains(value);
	}

	/// <summary>Returns the zero-based index of the specified <see cref="T:System.Windows.Media.GradientStop" />. </summary>
	/// <returns>The index if the object was found; otherwise, -1.</returns>
	/// <param name="value">The item to search for.</param>
	public int IndexOf(GradientStop value)
	{
		ReadPreamble();
		return _collection.IndexOf(value);
	}

	/// <summary> Inserts a <see cref="T:System.Windows.Media.GradientStop" /> at the specified position in the gradient stop list. </summary>
	/// <param name="index">The zero-based index at which to insert the object.</param>
	/// <param name="value">The <see cref="T:System.Windows.Media.GradientStop" /> to insert.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the <see cref="T:System.Windows.Media.GradientStopCollection" />.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.GradientStopCollection" /> is read-only.- or -The <see cref="T:System.Windows.Media.GradientStopCollection" /> has a fixed size.</exception>
	public void Insert(int index, GradientStop value)
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

	/// <summary> Removes the first occurrence of the specified <see cref="T:System.Windows.Media.GradientStop" />  from this <see cref="T:System.Windows.Media.GradientStopCollection" />. </summary>
	/// <returns>true if <paramref name="value" /> was removed from the <see cref="T:System.Windows.Media.GradientStopCollection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Windows.Media.GradientStop" />  to remove from this <see cref="T:System.Windows.Media.GradientStopCollection" />.</param>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.GradientStopCollection" /> is read-only.- or -The <see cref="T:System.Windows.Media.GradientStopCollection" /> has a fixed size.</exception>
	public bool Remove(GradientStop value)
	{
		WritePreamble();
		int num = IndexOf(value);
		if (num >= 0)
		{
			GradientStop oldValue = _collection[num];
			OnFreezablePropertyChanged(oldValue, null);
			_collection.RemoveAt(num);
			_version++;
			WritePostscript();
			return true;
		}
		return false;
	}

	/// <summary> Removes the <see cref="T:System.Windows.Media.GradientStop" />  at the specified index from this <see cref="T:System.Windows.Media.GradientStopCollection" />. </summary>
	/// <param name="index">The index of the <see cref="T:System.Windows.Media.GradientStop" />  to remove.</param>
	public void RemoveAt(int index)
	{
		RemoveAtWithoutFiringPublicEvents(index);
		WritePostscript();
	}

	internal void RemoveAtWithoutFiringPublicEvents(int index)
	{
		WritePreamble();
		GradientStop oldValue = _collection[index];
		OnFreezablePropertyChanged(oldValue, null);
		_collection.RemoveAt(index);
		_version++;
	}

	/// <summary> Copies the entire <see cref="T:System.Windows.Media.GradientStopCollection" /> to a compatible one-dimensional <see cref="T:System.Array" />, starting at the specified index of the target array. </summary>
	/// <param name="array">The one-dimensional array that is the destination of the items copied from the <see cref="T:System.Windows.Media.GradientStopCollection" />. The array must have zero-based indexing.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional. -or-The number of items in the source <see cref="T:System.Windows.Media.GradientStopCollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
	public void CopyTo(GradientStop[] array, int index)
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

	/// <summary>  Returns an enumerator that can iterate through the collection.  </summary>
	/// <returns>An <see cref="T:System.Windows.Media.GradientStopCollection.Enumerator" /> that can iterate through the collection.</returns>
	public Enumerator GetEnumerator()
	{
		ReadPreamble();
		return new Enumerator(this);
	}

	IEnumerator<GradientStop> IEnumerable<GradientStop>.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Add(System.Object)" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.Media.GradientStopCollection" />.</param>
	int IList.Add(object value)
	{
		return AddHelper(Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Contains(System.Object)" />.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.Media.GradientStopCollection" />; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.GradientStopCollection" />.</param>
	bool IList.Contains(object value)
	{
		return Contains(value as GradientStop);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.IndexOf(System.Object)" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.GradientStopCollection" />.</param>
	int IList.IndexOf(object value)
	{
		return IndexOf(value as GradientStop);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Insert(System.Int32,System.Object)" />.</summary>
	/// <param name="index">The zero-based index at which to insert the <see cref="T:System.Object" />.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert into the <see cref="T:System.Windows.Media.GradientStopCollection" />.</param>
	void IList.Insert(int index, object value)
	{
		Insert(index, Cast(value));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Remove(System.Object)" />.</summary>
	/// <param name="value">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.Media.GradientStopCollection" />.</param>
	void IList.Remove(object value)
	{
		Remove(value as GradientStop);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items from the <see cref="T:System.Windows.Media.GradientStopCollection" />.</param>
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

	internal GradientStop Internal_GetItem(int i)
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

	private GradientStop Cast(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is GradientStop))
		{
			throw new ArgumentException(SR.Format(SR.Collection_BadType, GetType().Name, value.GetType().Name, "GradientStop"));
		}
		return (GradientStop)value;
	}

	private int AddHelper(GradientStop value)
	{
		int result = AddWithoutFiringPublicEvents(value);
		WritePostscript();
		return result;
	}

	internal int AddWithoutFiringPublicEvents(GradientStop value)
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
		return new GradientStopCollection();
	}

	protected override void CloneCore(Freezable source)
	{
		GradientStopCollection gradientStopCollection = (GradientStopCollection)source;
		base.CloneCore(source);
		int count = gradientStopCollection._collection.Count;
		_collection = new FrugalStructList<GradientStop>(count);
		for (int i = 0; i < count; i++)
		{
			GradientStop gradientStop = gradientStopCollection._collection[i].Clone();
			OnFreezablePropertyChanged(null, gradientStop);
			_collection.Add(gradientStop);
		}
	}

	protected override void CloneCurrentValueCore(Freezable source)
	{
		GradientStopCollection gradientStopCollection = (GradientStopCollection)source;
		base.CloneCurrentValueCore(source);
		int count = gradientStopCollection._collection.Count;
		_collection = new FrugalStructList<GradientStop>(count);
		for (int i = 0; i < count; i++)
		{
			GradientStop gradientStop = gradientStopCollection._collection[i].CloneCurrentValue();
			OnFreezablePropertyChanged(null, gradientStop);
			_collection.Add(gradientStop);
		}
	}

	protected override void GetAsFrozenCore(Freezable source)
	{
		GradientStopCollection gradientStopCollection = (GradientStopCollection)source;
		base.GetAsFrozenCore(source);
		int count = gradientStopCollection._collection.Count;
		_collection = new FrugalStructList<GradientStop>(count);
		for (int i = 0; i < count; i++)
		{
			GradientStop gradientStop = (GradientStop)gradientStopCollection._collection[i].GetAsFrozen();
			OnFreezablePropertyChanged(null, gradientStop);
			_collection.Add(gradientStop);
		}
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable source)
	{
		GradientStopCollection gradientStopCollection = (GradientStopCollection)source;
		base.GetCurrentValueAsFrozenCore(source);
		int count = gradientStopCollection._collection.Count;
		_collection = new FrugalStructList<GradientStop>(count);
		for (int i = 0; i < count; i++)
		{
			GradientStop gradientStop = (GradientStop)gradientStopCollection._collection[i].GetCurrentValueAsFrozen();
			OnFreezablePropertyChanged(null, gradientStop);
			_collection.Add(gradientStop);
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

	/// <summary> Creates a <see cref="T:System.String" /> representation of this <see cref="T:System.Windows.Media.GradientStopCollection" />. </summary>
	/// <returns>Returns a <see cref="T:System.String" /> containing the values of this <see cref="T:System.Windows.Media.GradientStopCollection" />.</returns>
	public override string ToString()
	{
		ReadPreamble();
		return ConvertToString(null, null);
	}

	/// <summary> Creates a <see cref="T:System.String" /> representation of this <see cref="T:System.Windows.Media.GradientStopCollection" />. </summary>
	/// <returns>Returns a <see cref="T:System.String" /> containing the values of this <see cref="T:System.Windows.Media.GradientStopCollection" />.</returns>
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

	/// <summary> Converts a <see cref="T:System.String" /> representation of a GradientStopCollection into the equivalent <see cref="T:System.Windows.Media.GradientStopCollection" />.      </summary>
	/// <returns>Returns the equivalent <see cref="T:System.Windows.Media.GradientStopCollection" />. </returns>
	/// <param name="source">The <see cref="T:System.String" /> representation of the GradientStopCollection. </param>
	public static GradientStopCollection Parse(string source)
	{
		IFormatProvider invariantEnglishUS = System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS;
		TokenizerHelper tokenizerHelper = new TokenizerHelper(source, invariantEnglishUS);
		GradientStopCollection gradientStopCollection = new GradientStopCollection();
		while (tokenizerHelper.NextToken())
		{
			GradientStop value = new GradientStop(Parsers.ParseColor(tokenizerHelper.GetCurrentToken(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS));
			gradientStopCollection.Add(value);
		}
		return gradientStopCollection;
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.GradientStopCollection" /> class. </summary>
	public GradientStopCollection()
	{
		_collection = default(FrugalStructList<GradientStop>);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.GradientStopCollection" /> class that is initially capable of storing the specified number of items.</summary>
	/// <param name="capacity">The number of <see cref="T:System.Windows.Media.GradientStop" /> objects that the collection is initially capable of storing.</param>
	public GradientStopCollection(int capacity)
	{
		_collection = new FrugalStructList<GradientStop>(capacity);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.GradientStopCollection" /> class that contains the elements in the specified collection.</summary>
	/// <param name="collection">The collection to copy.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="collection" /> is null.</exception>
	public GradientStopCollection(IEnumerable<GradientStop> collection)
	{
		WritePreamble();
		if (collection != null)
		{
			bool flag = true;
			if (collection is ICollection<GradientStop> collection2)
			{
				_collection = new FrugalStructList<GradientStop>(collection2);
			}
			else if (collection is ICollection collection3)
			{
				_collection = new FrugalStructList<GradientStop>(collection3);
			}
			else
			{
				_collection = default(FrugalStructList<GradientStop>);
				foreach (GradientStop item in collection)
				{
					GradientStop gradientStop = item ?? throw new ArgumentException(SR.Collection_NoNull);
					OnFreezablePropertyChanged(null, gradientStop);
					_collection.Add(gradientStop);
				}
				flag = false;
			}
			if (flag)
			{
				foreach (GradientStop item2 in collection)
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
