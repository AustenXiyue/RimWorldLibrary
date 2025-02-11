using System;
using System.Collections;
using System.Collections.Generic;

namespace MS.Internal;

internal class WeakDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable where TKey : class
{
	private class KeyCollection<KeyType, ValueType> : ICollection<KeyType>, IEnumerable<KeyType>, IEnumerable where KeyType : class
	{
		public WeakDictionary<KeyType, ValueType> Dict { get; private set; }

		public int Count => Dict.Count;

		public bool IsReadOnly => true;

		public KeyCollection(WeakDictionary<KeyType, ValueType> dict)
		{
			Dict = dict;
		}

		public void Add(KeyType item)
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains(KeyType item)
		{
			return Dict.ContainsKey(item);
		}

		public void CopyTo(KeyType[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public bool Remove(KeyType item)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<KeyType> GetEnumerator()
		{
			IWeakHashtable hashTable = Dict._hashTable;
			foreach (object key in hashTable.Keys)
			{
				if (hashTable.UnwrapKey(key) is KeyType val)
				{
					yield return val;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	private class ValueCollection<KeyType, ValueType> : ICollection<ValueType>, IEnumerable<ValueType>, IEnumerable where KeyType : class
	{
		public WeakDictionary<KeyType, ValueType> Dict { get; private set; }

		public int Count => Dict.Count;

		public bool IsReadOnly => true;

		public ValueCollection(WeakDictionary<KeyType, ValueType> dict)
		{
			Dict = dict;
		}

		public void Add(ValueType item)
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains(ValueType item)
		{
			throw new NotImplementedException();
		}

		public void CopyTo(ValueType[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public bool Remove(ValueType item)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<ValueType> GetEnumerator()
		{
			IWeakHashtable hashTable = Dict._hashTable;
			foreach (object key in hashTable.Keys)
			{
				if (hashTable.UnwrapKey(key) as KeyType != null)
				{
					yield return (ValueType)hashTable[key];
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	private IWeakHashtable _hashTable = WeakHashtable.FromKeyType(typeof(TKey));

	private KeyCollection<TKey, TValue> _keys;

	private ValueCollection<TKey, TValue> _values;

	public ICollection<TKey> Keys
	{
		get
		{
			if (_keys == null)
			{
				_keys = new KeyCollection<TKey, TValue>(this);
			}
			return _keys;
		}
	}

	public ICollection<TValue> Values
	{
		get
		{
			if (_values == null)
			{
				_values = new ValueCollection<TKey, TValue>(this);
			}
			return _values;
		}
	}

	public TValue this[TKey key]
	{
		get
		{
			if (!_hashTable.ContainsKey(key))
			{
				throw new KeyNotFoundException();
			}
			return (TValue)_hashTable[key];
		}
		set
		{
			_hashTable.SetWeak(key, value);
		}
	}

	public int Count => _hashTable.Count;

	public bool IsReadOnly => false;

	public void Add(TKey key, TValue value)
	{
		_hashTable.SetWeak(key, value);
	}

	public bool ContainsKey(TKey key)
	{
		return _hashTable.ContainsKey(key);
	}

	public bool Remove(TKey key)
	{
		if (_hashTable.ContainsKey(key))
		{
			_hashTable.Remove(key);
			return true;
		}
		return false;
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		if (_hashTable.ContainsKey(key))
		{
			value = (TValue)_hashTable[key];
			return true;
		}
		value = default(TValue);
		return false;
	}

	public void Add(KeyValuePair<TKey, TValue> item)
	{
		Add(item.Key, item.Value);
	}

	public void Clear()
	{
		_hashTable.Clear();
	}

	public bool Contains(KeyValuePair<TKey, TValue> item)
	{
		if (!_hashTable.ContainsKey(item.Key))
		{
			return false;
		}
		if (object.Equals(_hashTable[item.Key], item.Value))
		{
			return true;
		}
		return false;
	}

	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		if (arrayIndex < 0)
		{
			throw new ArgumentOutOfRangeException("arrayIndex");
		}
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		int num = 0;
		using (IEnumerator<KeyValuePair<TKey, TValue>> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				_ = enumerator.Current;
				num++;
			}
		}
		if (num + arrayIndex > array.Length)
		{
			throw new ArgumentOutOfRangeException("arrayIndex");
		}
		using IEnumerator<KeyValuePair<TKey, TValue>> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<TKey, TValue> current = enumerator.Current;
			array[arrayIndex++] = current;
		}
	}

	public bool Remove(KeyValuePair<TKey, TValue> item)
	{
		if (Contains(item))
		{
			return Remove(item.Key);
		}
		return false;
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		foreach (object key2 in _hashTable.Keys)
		{
			if (_hashTable.UnwrapKey(key2) is TKey key)
			{
				yield return new KeyValuePair<TKey, TValue>(key, (TValue)_hashTable[key2]);
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
