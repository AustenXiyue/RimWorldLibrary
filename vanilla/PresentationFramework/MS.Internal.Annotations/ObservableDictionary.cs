using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace MS.Internal.Annotations;

internal class ObservableDictionary : IDictionary<string, string>, ICollection<KeyValuePair<string, string>>, IEnumerable<KeyValuePair<string, string>>, IEnumerable, INotifyPropertyChanged
{
	private Dictionary<string, string> _nameValues;

	public int Count => _nameValues.Count;

	public string this[string key]
	{
		get
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			string value = null;
			_nameValues.TryGetValue(key, out value);
			return value;
		}
		set
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			string value2 = null;
			_nameValues.TryGetValue(key, out value2);
			if (value2 == null || value2 != value)
			{
				_nameValues[key] = value;
				FireDictionaryChanged();
			}
		}
	}

	public bool IsReadOnly => false;

	public ICollection<string> Keys => _nameValues.Keys;

	public ICollection<string> Values => _nameValues.Values;

	public event PropertyChangedEventHandler PropertyChanged;

	public ObservableDictionary()
	{
		_nameValues = new Dictionary<string, string>();
	}

	public void Add(string key, string val)
	{
		if (key == null || val == null)
		{
			throw new ArgumentNullException((key == null) ? "key" : "val");
		}
		_nameValues.Add(key, val);
		FireDictionaryChanged();
	}

	public void Clear()
	{
		if (_nameValues.Count > 0)
		{
			_nameValues.Clear();
			FireDictionaryChanged();
		}
	}

	public bool ContainsKey(string key)
	{
		return _nameValues.ContainsKey(key);
	}

	public bool Remove(string key)
	{
		bool num = _nameValues.Remove(key);
		if (num)
		{
			FireDictionaryChanged();
		}
		return num;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _nameValues.GetEnumerator();
	}

	public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
	{
		return ((IEnumerable<KeyValuePair<string, string>>)_nameValues).GetEnumerator();
	}

	public bool TryGetValue(string key, out string value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		return _nameValues.TryGetValue(key, out value);
	}

	void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> pair)
	{
		((ICollection<KeyValuePair<string, string>>)_nameValues).Add(pair);
	}

	bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> pair)
	{
		return ((ICollection<KeyValuePair<string, string>>)_nameValues).Contains(pair);
	}

	bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> pair)
	{
		return ((ICollection<KeyValuePair<string, string>>)_nameValues).Remove(pair);
	}

	void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] target, int startIndex)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (startIndex < 0 || startIndex > target.Length)
		{
			throw new ArgumentOutOfRangeException("startIndex");
		}
		((ICollection<KeyValuePair<string, string>>)_nameValues).CopyTo(target, startIndex);
	}

	private void FireDictionaryChanged()
	{
		if (this.PropertyChanged != null)
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(null));
		}
	}
}
