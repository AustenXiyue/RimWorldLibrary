using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Markup;

namespace System.Xaml;

internal class NameScope : INameScopeDictionary, INameScope, IDictionary<string, object>, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable
{
	private class Enumerator : IEnumerator<KeyValuePair<string, object>>, IEnumerator, IDisposable
	{
		private IDictionaryEnumerator _enumerator;

		public KeyValuePair<string, object> Current
		{
			get
			{
				if (_enumerator == null)
				{
					return default(KeyValuePair<string, object>);
				}
				return new KeyValuePair<string, object>((string)_enumerator.Key, _enumerator.Value);
			}
		}

		object IEnumerator.Current => Current;

		public Enumerator(HybridDictionary nameMap)
		{
			_enumerator = null;
			if (nameMap != null)
			{
				_enumerator = nameMap.GetEnumerator();
			}
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		public bool MoveNext()
		{
			if (_enumerator == null)
			{
				return false;
			}
			return _enumerator.MoveNext();
		}

		void IEnumerator.Reset()
		{
			if (_enumerator != null)
			{
				_enumerator.Reset();
			}
		}
	}

	private HybridDictionary _nameMap;

	public int Count
	{
		get
		{
			if (_nameMap == null)
			{
				return 0;
			}
			return _nameMap.Count;
		}
	}

	public bool IsReadOnly => false;

	public object this[string key]
	{
		get
		{
			ArgumentNullException.ThrowIfNull(key, "key");
			return FindName(key);
		}
		set
		{
			ArgumentNullException.ThrowIfNull(key, "key");
			ArgumentNullException.ThrowIfNull(value, "value");
			RegisterName(key, value);
		}
	}

	public ICollection<string> Keys
	{
		get
		{
			if (_nameMap == null)
			{
				return null;
			}
			List<string> list = new List<string>(_nameMap.Keys.Count);
			foreach (string key in _nameMap.Keys)
			{
				list.Add(key);
			}
			return list;
		}
	}

	public ICollection<object> Values
	{
		get
		{
			if (_nameMap == null)
			{
				return null;
			}
			List<object> list = new List<object>(_nameMap.Values.Count);
			foreach (object value in _nameMap.Values)
			{
				list.Add(value);
			}
			return list;
		}
	}

	public void RegisterName(string name, object scopedElement)
	{
		ArgumentNullException.ThrowIfNull(name, "name");
		ArgumentNullException.ThrowIfNull(scopedElement, "scopedElement");
		if (name.Length == 0)
		{
			throw new ArgumentException(System.SR.NameScopeNameNotEmptyString);
		}
		if (!NameValidationHelper.IsValidIdentifierName(name))
		{
			throw new ArgumentException(System.SR.Format(System.SR.NameScopeInvalidIdentifierName, name));
		}
		if (_nameMap == null)
		{
			_nameMap = new HybridDictionary();
			_nameMap[name] = scopedElement;
			return;
		}
		object obj = _nameMap[name];
		if (obj == null)
		{
			_nameMap[name] = scopedElement;
		}
		else if (scopedElement != obj)
		{
			throw new ArgumentException(System.SR.Format(System.SR.NameScopeDuplicateNamesNotAllowed, name));
		}
	}

	public void UnregisterName(string name)
	{
		ArgumentNullException.ThrowIfNull(name, "name");
		if (name.Length == 0)
		{
			throw new ArgumentException(System.SR.NameScopeNameNotEmptyString);
		}
		if (_nameMap != null && _nameMap[name] != null)
		{
			_nameMap.Remove(name);
			return;
		}
		throw new ArgumentException(System.SR.Format(System.SR.NameScopeNameNotFound, name));
	}

	public object FindName(string name)
	{
		if (_nameMap == null || string.IsNullOrEmpty(name))
		{
			return null;
		}
		return _nameMap[name];
	}

	private IEnumerator<KeyValuePair<string, object>> GetEnumerator()
	{
		return new Enumerator(_nameMap);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Clear()
	{
		_nameMap = null;
	}

	public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
	{
		if (_nameMap == null)
		{
			array = null;
			return;
		}
		foreach (DictionaryEntry item in _nameMap)
		{
			array[arrayIndex++] = new KeyValuePair<string, object>((string)item.Key, item.Value);
		}
	}

	public bool Remove(KeyValuePair<string, object> item)
	{
		if (!Contains(item))
		{
			return false;
		}
		if (item.Value != this[item.Key])
		{
			return false;
		}
		return Remove(item.Key);
	}

	public void Add(KeyValuePair<string, object> item)
	{
		if (item.Key == null)
		{
			throw new ArgumentException(System.SR.Format(System.SR.ReferenceIsNull, "item.Key"), "item");
		}
		if (item.Value == null)
		{
			throw new ArgumentException(System.SR.Format(System.SR.ReferenceIsNull, "item.Value"), "item");
		}
		Add(item.Key, item.Value);
	}

	public bool Contains(KeyValuePair<string, object> item)
	{
		if (item.Key == null)
		{
			throw new ArgumentException(System.SR.Format(System.SR.ReferenceIsNull, "item.Key"), "item");
		}
		return ContainsKey(item.Key);
	}

	public void Add(string key, object value)
	{
		ArgumentNullException.ThrowIfNull(key, "key");
		RegisterName(key, value);
	}

	public bool ContainsKey(string key)
	{
		ArgumentNullException.ThrowIfNull(key, "key");
		return FindName(key) != null;
	}

	public bool Remove(string key)
	{
		if (!ContainsKey(key))
		{
			return false;
		}
		UnregisterName(key);
		return true;
	}

	public bool TryGetValue(string key, out object value)
	{
		if (!ContainsKey(key))
		{
			value = null;
			return false;
		}
		value = FindName(key);
		return true;
	}
}
