using System.Collections;
using System.Collections.Generic;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Represents a dictionary of <see cref="T:System.Windows.Media.CharacterMetrics" /> objects for a device font that is indexed by Unicode scalar values.</summary>
public sealed class CharacterMetricsDictionary : IDictionary<int, CharacterMetrics>, ICollection<KeyValuePair<int, CharacterMetrics>>, IEnumerable<KeyValuePair<int, CharacterMetrics>>, IEnumerable, IDictionary, ICollection
{
	private struct Enumerator : IDictionaryEnumerator, IEnumerator, IEnumerator<KeyValuePair<int, CharacterMetrics>>, IDisposable
	{
		private CharacterMetricsDictionary _dictionary;

		private int _unicodeScalar;

		private CharacterMetrics _value;

		object IEnumerator.Current
		{
			get
			{
				KeyValuePair<int, CharacterMetrics> currentEntry = GetCurrentEntry();
				return new DictionaryEntry(currentEntry.Key, currentEntry.Value);
			}
		}

		public KeyValuePair<int, CharacterMetrics> Current => new KeyValuePair<int, CharacterMetrics>(_unicodeScalar, _value);

		DictionaryEntry IDictionaryEnumerator.Entry
		{
			get
			{
				KeyValuePair<int, CharacterMetrics> currentEntry = GetCurrentEntry();
				return new DictionaryEntry(currentEntry.Key, currentEntry.Value);
			}
		}

		object IDictionaryEnumerator.Key => GetCurrentEntry().Key;

		object IDictionaryEnumerator.Value => GetCurrentEntry().Value;

		internal Enumerator(CharacterMetricsDictionary dictionary)
		{
			_dictionary = dictionary;
			_unicodeScalar = -1;
			_value = null;
		}

		void IDisposable.Dispose()
		{
		}

		public bool MoveNext()
		{
			if (_unicodeScalar < int.MaxValue)
			{
				_value = _dictionary.GetNextValue(ref _unicodeScalar);
			}
			return _value != null;
		}

		void IEnumerator.Reset()
		{
			_unicodeScalar = -1;
		}

		private KeyValuePair<int, CharacterMetrics> GetCurrentEntry()
		{
			if (_value != null)
			{
				return new KeyValuePair<int, CharacterMetrics>(_unicodeScalar, _value);
			}
			throw new InvalidOperationException(SR.Enumerator_VerifyContext);
		}
	}

	internal const int LastDeviceFontCharacterCode = 65535;

	internal const int PageShift = 8;

	internal const int PageSize = 256;

	internal const int PageMask = 255;

	internal const int PageCount = 256;

	private CharacterMetrics[][] _pageTable;

	private int _count;

	/// <summary>Gets the number of elements in the collection.</summary>
	/// <returns>A value of type <see cref="T:System.Int32" />.</returns>
	public int Count
	{
		get
		{
			if (_count == 0)
			{
				_count = CountValues();
			}
			return _count;
		}
	}

	/// <summary>Gets a value indicating whether the <see cref="T:System.Windows.Media.CharacterMetricsDictionary" /> is read-only.</summary>
	/// <returns>true if the dictionary is read-only; otherwise, false.</returns>
	public bool IsReadOnly => false;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.IsSynchronized" />.</summary>
	/// <returns>true if access to the <see cref="T:System.Windows.Media.CharacterMetricsDictionary" /> is synchronized (thread safe); otherwise, false.</returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.SyncRoot" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Windows.Media.CharacterMetricsDictionary" />.</returns>
	object ICollection.SyncRoot => this;

	/// <summary>Gets or sets a value for the item in the collection that matches a specified key.</summary>
	/// <returns>A value of type <see cref="T:System.Windows.Media.CharacterMetrics" />.</returns>
	/// <param name="key">A value of type <see cref="T:System.Int32" />.</param>
	public CharacterMetrics this[int key]
	{
		get
		{
			return GetValue(key);
		}
		set
		{
			SetValue(key, value, failIfExists: false);
		}
	}

	/// <summary>Gets a collection of character codes from <see cref="T:System.Windows.Media.CharacterMetricsDictionary" />.</summary>
	/// <returns>A collection of keys of type <see cref="T:System.Int32" />.</returns>
	[CLSCompliant(false)]
	public ICollection<int> Keys => GetKeys();

	/// <summary>Gets the collection of <see cref="T:System.Windows.Media.CharacterMetrics" /> values in the <see cref="T:System.Windows.Media.CharacterMetricsDictionary" />.</summary>
	/// <returns>A collection of type <see cref="T:System.Windows.Media.CharacterMetrics" />.</returns>
	[CLSCompliant(false)]
	public ICollection<CharacterMetrics> Values => GetValues();

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IDictionary.IsFixedSize" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.CharacterMetricsDictionary" /> has a fixed size; otherwise, false.</returns>
	bool IDictionary.IsFixedSize => false;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IDictionary.Item(System.Object)" />.</summary>
	/// <returns>The element with the specified key.</returns>
	/// <param name="key">The key of the element to get or set. </param>
	object IDictionary.this[object key]
	{
		get
		{
			if (!(key is int))
			{
				return null;
			}
			return GetValue((int)key);
		}
		set
		{
			SetValue(ConvertKey(key), ConvertValue(value), failIfExists: false);
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IDictionary.Keys" />.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> object containing the keys of the <see cref="T:System.Collections.IDictionary" /> object.</returns>
	ICollection IDictionary.Keys => GetKeys();

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IDictionary.Values" />.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> object containing the values in the <see cref="T:System.Collections.IDictionary" /> object.</returns>
	ICollection IDictionary.Values => GetValues();

	internal CharacterMetricsDictionary()
	{
	}

	/// <summary>Returns an enumerator that iterates through the collection.</summary>
	/// <returns>An enumerator that iterates through the collection.</returns>
	[CLSCompliant(false)]
	public IEnumerator<KeyValuePair<int, CharacterMetrics>> GetEnumerator()
	{
		return new Enumerator(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IEnumerable.GetEnumerator" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IDictionary.GetEnumerator" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		return new Enumerator(this);
	}

	/// <summary>Retrieves the <see cref="T:System.Windows.Media.CharacterMetrics" /> value in the dictionary for a specified character code value.</summary>
	/// <returns>true if the dictionary contains an entry for <paramref name="key" />; otherwise false.</returns>
	/// <param name="key">A value of type <see cref="T:System.Int32" />.</param>
	/// <param name="value">A value of type <see cref="T:System.Windows.Media.CharacterMetrics" />.</param>
	public bool TryGetValue(int key, out CharacterMetrics value)
	{
		value = GetValue(key);
		return value != null;
	}

	/// <summary>Adds a character code and associated <see cref="T:System.Windows.Media.CharacterMetrics" /> value to the collection using a key/value pair.</summary>
	/// <param name="item">The key/value pair representing the character code and associated <see cref="T:System.Windows.Media.CharacterMetrics" /> value.</param>
	[CLSCompliant(false)]
	public void Add(KeyValuePair<int, CharacterMetrics> item)
	{
		SetValue(item.Key, item.Value, failIfExists: true);
	}

	/// <summary>Removes all elements from the collection.</summary>
	public void Clear()
	{
		_count = 0;
		_pageTable = null;
	}

	/// <summary>Determines whether the collection contains the specified key/value pair.</summary>
	/// <returns>true if the dictionary contains the <see cref="T:System.Windows.Media.CharacterMetrics" /> represented by the character code in <paramref name="item" />; otherwise, false.</returns>
	/// <param name="item">The key/value pair representing the character code and associated <see cref="T:System.Windows.Media.CharacterMetrics" /> value.</param>
	[CLSCompliant(false)]
	public bool Contains(KeyValuePair<int, CharacterMetrics> item)
	{
		if (item.Value != null)
		{
			return item.Value.Equals(GetValue(item.Key));
		}
		return false;
	}

	/// <summary>Copies the items in the collection to an array, starting at a specific array index.</summary>
	/// <param name="array">The one-dimensional array that is the destination of the elements that are copied from the <see cref="T:System.Windows.Media.CharacterMetricsDictionary" />. </param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	[CLSCompliant(false)]
	public void CopyTo(KeyValuePair<int, CharacterMetrics>[] array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (index >= array.Length)
		{
			throw new ArgumentException(SR.Format(SR.Collection_CopyTo_IndexGreaterThanOrEqualToArrayLength, "index", "array"));
		}
		CharacterMetrics[][] pageTable = _pageTable;
		if (pageTable == null)
		{
			return;
		}
		int num = index;
		for (int i = 0; i < pageTable.Length; i++)
		{
			CharacterMetrics[] array2 = pageTable[i];
			if (array2 == null)
			{
				continue;
			}
			for (int j = 0; j < array2.Length; j++)
			{
				CharacterMetrics characterMetrics = array2[j];
				if (characterMetrics != null)
				{
					if (num >= array.Length)
					{
						throw new ArgumentException(SR.Format(SR.Collection_CopyTo_NumberOfElementsExceedsArrayLength, index, "array"));
					}
					array[num++] = new KeyValuePair<int, CharacterMetrics>((i << 8) | j, characterMetrics);
				}
			}
		}
	}

	/// <summary>Removes the element from <see cref="T:System.Windows.Media.CharacterMetricsDictionary" /> based on the specified key/value pair.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.CharacterMetrics" /> item was successfully deleted; otherwise false.</returns>
	/// <param name="item">The key/value pair representing the character code and associated <see cref="T:System.Windows.Media.CharacterMetrics" /> value.</param>
	[CLSCompliant(false)]
	public bool Remove(KeyValuePair<int, CharacterMetrics> item)
	{
		if (item.Value != null)
		{
			return RemoveValue(item.Key, item.Value);
		}
		return false;
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items from the <see cref="T:System.Windows.Media.CharacterMetricsDictionary" />.</param>
	/// <param name="index">The first position in the specified <see cref="T:System.Array" /> to receive the copied contents.</param>
	void ICollection.CopyTo(Array array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (index >= array.Length)
		{
			throw new ArgumentException(SR.Format(SR.Collection_CopyTo_IndexGreaterThanOrEqualToArrayLength, "index", "array"));
		}
		if (Count > array.Length - index)
		{
			throw new ArgumentException(SR.Format(SR.Collection_CopyTo_NumberOfElementsExceedsArrayLength, index, "array"));
		}
		if (array is DictionaryEntry[] array2)
		{
			using IEnumerator<KeyValuePair<int, CharacterMetrics>> enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<int, CharacterMetrics> current = enumerator.Current;
				array2[index++] = new DictionaryEntry(current.Key, current.Value);
			}
			return;
		}
		if (array.Rank != 1)
		{
			throw new ArgumentException(SR.Collection_CopyTo_ArrayCannotBeMultidimensional);
		}
		Type elementType = array.GetType().GetElementType();
		if (!elementType.IsAssignableFrom(typeof(DictionaryEntry)))
		{
			throw new ArgumentException(SR.Format(SR.CannotConvertType, typeof(DictionaryEntry), elementType));
		}
		using IEnumerator<KeyValuePair<int, CharacterMetrics>> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<int, CharacterMetrics> current2 = enumerator.Current;
			array.SetValue(new DictionaryEntry(current2.Key, current2.Value), index++);
		}
	}

	/// <summary>Adds a character code and associated <see cref="T:System.Windows.Media.CharacterMetrics" /> value to the collection.</summary>
	/// <param name="key">A value of type <see cref="T:System.Int32" /> representing the character code.</param>
	/// <param name="value">A value of type <see cref="T:System.Windows.Media.CharacterMetrics" />.</param>
	public void Add(int key, CharacterMetrics value)
	{
		SetValue(key, value, failIfExists: true);
	}

	/// <summary>Determines whether the collection contains the specified character code.</summary>
	/// <returns>true if the dictionary contains <paramref name="key" />; otherwise, false.</returns>
	/// <param name="key">A value of type <see cref="T:System.Int32" /> representing the character code.</param>
	public bool ContainsKey(int key)
	{
		return GetValue(key) != null;
	}

	/// <summary>Removes the element from <see cref="T:System.Windows.Media.CharacterMetricsDictionary" /> based on the specified character code.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.CharacterMetrics" /> item was successfully deleted; otherwise false.</returns>
	/// <param name="key">A value of type <see cref="T:System.Int32" /> representing the character code.</param>
	public bool Remove(int key)
	{
		return RemoveValue(key, null);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IDictionary.Add(System.Object,System.Object)" />.</summary>
	/// <param name="key">The <see cref="T:System.Object" /> to use as the key of the element to add.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Windows.Media.CharacterMetricsDictionary" />.</param>
	void IDictionary.Add(object key, object value)
	{
		SetValue(ConvertKey(key), ConvertValue(value), failIfExists: false);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IDictionary.Contains(System.Object)" />.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Windows.Media.CharacterMetricsDictionary" />; otherwise, false.</returns>
	/// <param name="key">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Windows.Media.CharacterMetricsDictionary" />.</param>
	bool IDictionary.Contains(object key)
	{
		if (key is int)
		{
			return GetValue((int)key) != null;
		}
		return false;
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IDictionary.Remove(System.Object)" />.</summary>
	/// <param name="key">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Windows.Media.CharacterMetricsDictionary" />.</param>
	void IDictionary.Remove(object key)
	{
		if (key is int)
		{
			RemoveValue((int)key, null);
		}
	}

	internal CharacterMetrics[] GetPage(int i)
	{
		if (_pageTable == null)
		{
			return null;
		}
		return _pageTable[i];
	}

	private CharacterMetrics[] GetPageFromUnicodeScalar(int unicodeScalar)
	{
		int num = unicodeScalar >> 8;
		CharacterMetrics[] array;
		if (_pageTable != null)
		{
			array = _pageTable[num];
			if (array == null)
			{
				array = (_pageTable[num] = new CharacterMetrics[256]);
			}
		}
		else
		{
			_pageTable = new CharacterMetrics[256][];
			array = (_pageTable[num] = new CharacterMetrics[256]);
		}
		return array;
	}

	private void SetValue(int key, CharacterMetrics value, bool failIfExists)
	{
		if (key < 0 || key > 65535)
		{
			throw new ArgumentOutOfRangeException(SR.Format(SR.CodePointOutOfRange, key));
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		CharacterMetrics[] pageFromUnicodeScalar = GetPageFromUnicodeScalar(key);
		int num = key & 0xFF;
		if (failIfExists && pageFromUnicodeScalar[num] != null)
		{
			throw new ArgumentException(SR.Format(SR.CollectionDuplicateKey, key));
		}
		pageFromUnicodeScalar[num] = value;
		_count = 0;
	}

	internal CharacterMetrics GetValue(int key)
	{
		CharacterMetrics result = null;
		if (key >= 0 && key <= 1114111 && _pageTable != null)
		{
			CharacterMetrics[] array = _pageTable[key >> 8];
			if (array != null)
			{
				result = array[key & 0xFF];
			}
		}
		return result;
	}

	private bool RemoveValue(int key, CharacterMetrics value)
	{
		if (key >= 0 && key <= 1114111 && _pageTable != null)
		{
			CharacterMetrics[] array = _pageTable[key >> 8];
			if (array != null)
			{
				int num = key & 0xFF;
				CharacterMetrics characterMetrics = array[num];
				if (characterMetrics != null && (value == null || characterMetrics.Equals(value)))
				{
					array[num] = null;
					_count = 0;
					return true;
				}
			}
		}
		return false;
	}

	private CharacterMetrics GetNextValue(ref int unicodeScalar)
	{
		CharacterMetrics[][] pageTable = _pageTable;
		if (pageTable != null)
		{
			int i = (unicodeScalar + 1) & 0xFF;
			for (int j = unicodeScalar + 1 >> 8; j < 256; j++)
			{
				CharacterMetrics[] array = pageTable[j];
				if (array == null)
				{
					continue;
				}
				for (; i < 256; i++)
				{
					CharacterMetrics characterMetrics = array[i];
					if (characterMetrics != null)
					{
						unicodeScalar = (j << 8) | i;
						return characterMetrics;
					}
				}
				i = 0;
			}
		}
		unicodeScalar = int.MaxValue;
		return null;
	}

	private int CountValues()
	{
		int num = 0;
		CharacterMetrics[][] pageTable = _pageTable;
		if (pageTable != null)
		{
			foreach (CharacterMetrics[] array in pageTable)
			{
				if (array == null)
				{
					continue;
				}
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j] != null)
					{
						num++;
					}
				}
			}
		}
		return num;
	}

	private int[] GetKeys()
	{
		int[] array = new int[Count];
		int num = 0;
		using IEnumerator<KeyValuePair<int, CharacterMetrics>> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<int, CharacterMetrics> current = enumerator.Current;
			array[num++] = current.Key;
		}
		return array;
	}

	private CharacterMetrics[] GetValues()
	{
		CharacterMetrics[] array = new CharacterMetrics[Count];
		int num = 0;
		using IEnumerator<KeyValuePair<int, CharacterMetrics>> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<int, CharacterMetrics> current = enumerator.Current;
			array[num++] = current.Value;
		}
		return array;
	}

	internal static int ConvertKey(object key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		int number;
		if (key is string text)
		{
			int index = 0;
			if (!FontFamilyMap.ParseHexNumber(text, ref index, out number) || index < text.Length)
			{
				throw new ArgumentException(SR.Format(SR.CannotConvertStringToType, text, "int"), "key");
			}
		}
		else
		{
			if (!(key is int))
			{
				throw new ArgumentException(SR.Format(SR.CannotConvertType, key.GetType(), "int"), "key");
			}
			number = (int)key;
		}
		if (number < 0 || number > 1114111)
		{
			throw new ArgumentException(SR.Format(SR.CodePointOutOfRange, number), "key");
		}
		return number;
	}

	private CharacterMetrics ConvertValue(object value)
	{
		if (value is CharacterMetrics result)
		{
			return result;
		}
		if (value != null)
		{
			throw new ArgumentException(SR.Format(SR.CannotConvertType, typeof(CharacterMetrics), value.GetType()));
		}
		throw new ArgumentNullException("value");
	}
}
