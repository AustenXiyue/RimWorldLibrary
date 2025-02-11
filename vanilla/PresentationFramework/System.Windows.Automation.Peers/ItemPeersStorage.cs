using System.Collections.Generic;
using MS.Internal;
using MS.Internal.Automation;
using MS.Internal.Hashing.PresentationFramework;

namespace System.Windows.Automation.Peers;

internal class ItemPeersStorage<T> where T : class
{
	private WeakDictionary<object, T> _hashtable;

	private List<KeyValuePair<object, T>> _list;

	private int _count;

	private bool _usesHashCode;

	public T this[object item]
	{
		get
		{
			if (_count == 0 || item == null)
			{
				return null;
			}
			if (_usesHashCode)
			{
				if (_hashtable == null || !_hashtable.ContainsKey(item))
				{
					return null;
				}
				return _hashtable[item];
			}
			if (_list == null)
			{
				return null;
			}
			for (int i = 0; i < _list.Count; i++)
			{
				KeyValuePair<object, T> keyValuePair = _list[i];
				if (object.Equals(item, keyValuePair.Key))
				{
					return keyValuePair.Value;
				}
			}
			return null;
		}
		set
		{
			if (item == null)
			{
				return;
			}
			if (_count == 0)
			{
				_usesHashCode = item != null && HashHelper.HasReliableHashCode(item);
			}
			if (_usesHashCode)
			{
				if (_hashtable == null)
				{
					_hashtable = new WeakDictionary<object, T>();
				}
				if (!_hashtable.ContainsKey(item) && value != null)
				{
					_hashtable[item] = value;
				}
			}
			else
			{
				if (_list == null)
				{
					_list = new List<KeyValuePair<object, T>>();
				}
				if (value != null)
				{
					_list.Add(new KeyValuePair<object, T>(item, value));
				}
			}
			_count++;
		}
	}

	public int Count => _count;

	public void Clear()
	{
		_usesHashCode = false;
		_count = 0;
		if (_hashtable != null)
		{
			_hashtable.Clear();
		}
		if (_list != null)
		{
			_list.Clear();
		}
	}

	public void Remove(object item)
	{
		if (_usesHashCode)
		{
			if (item != null && _hashtable.ContainsKey(item))
			{
				_hashtable.Remove(item);
				if (!_hashtable.ContainsKey(item))
				{
					_count--;
				}
			}
		}
		else if (_list != null)
		{
			int num = 0;
			for (num = 0; num < _list.Count && !object.Equals(item, _list[num].Key); num++)
			{
			}
			if (num < _list.Count)
			{
				_list.RemoveAt(num);
				_count--;
			}
		}
	}

	public void PurgeWeakRefCollection()
	{
		if (!typeof(T).IsAssignableFrom(typeof(WeakReference)))
		{
			return;
		}
		List<object> list = new List<object>();
		if (_usesHashCode)
		{
			if (_hashtable == null)
			{
				return;
			}
			foreach (KeyValuePair<object, T> item in _hashtable)
			{
				if (!(item.Value is WeakReference weakReference))
				{
					list.Add(item.Key);
				}
				else if (!(weakReference.Target is ElementProxy elementProxy))
				{
					list.Add(item.Key);
				}
				else if (!(elementProxy.Peer is ItemAutomationPeer))
				{
					list.Add(item.Key);
				}
			}
		}
		else
		{
			if (_list == null)
			{
				return;
			}
			foreach (KeyValuePair<object, T> item2 in _list)
			{
				if (!(item2.Value is WeakReference weakReference2))
				{
					list.Add(item2.Key);
				}
				else if (!(weakReference2.Target is ElementProxy elementProxy2))
				{
					list.Add(item2.Key);
				}
				else if (!(elementProxy2.Peer is ItemAutomationPeer))
				{
					list.Add(item2.Key);
				}
			}
		}
		foreach (object item3 in list)
		{
			Remove(item3);
		}
	}
}
