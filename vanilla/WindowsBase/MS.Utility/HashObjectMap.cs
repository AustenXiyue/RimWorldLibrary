using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using MS.Internal.WindowsBase;

namespace MS.Utility;

internal sealed class HashObjectMap : FrugalMapBase
{
	internal const int MINSIZE = 163;

	private static object NullValue = new object();

	internal Dictionary<int, object> _entries;

	public override int Count => _entries.Count;

	public override FrugalMapStoreState InsertEntry(int key, object value)
	{
		if (_entries == null)
		{
			_entries = new Dictionary<int, object>(163);
		}
		_entries[key] = ((value != NullValue && value != null) ? value : NullValue);
		return FrugalMapStoreState.Success;
	}

	public override void RemoveEntry(int key)
	{
		_entries.Remove(key);
	}

	public override object Search(int key)
	{
		if (!_entries.TryGetValue(key, out var value) || value == NullValue)
		{
			return DependencyProperty.UnsetValue;
		}
		return value;
	}

	public override void Sort()
	{
	}

	public override void GetKeyValuePair(int index, out int key, out object value)
	{
		if (index < _entries.Count)
		{
			Dictionary<int, object>.Enumerator enumerator = _entries.GetEnumerator();
			enumerator.MoveNext();
			for (int i = 0; i < index; i++)
			{
				enumerator.MoveNext();
			}
			KeyValuePair<int, object> current = enumerator.Current;
			key = current.Key;
			value = ((current.Value != NullValue) ? current.Value : DependencyProperty.UnsetValue);
			return;
		}
		value = DependencyProperty.UnsetValue;
		key = int.MaxValue;
		throw new ArgumentOutOfRangeException("index");
	}

	public override void Iterate(ArrayList list, FrugalMapIterationCallback callback)
	{
		foreach (KeyValuePair<int, object> entry in _entries)
		{
			object value = ((entry.Value != NullValue) ? entry.Value : DependencyProperty.UnsetValue);
			callback(list, entry.Key, value);
		}
	}

	public override void Promote(FrugalMapBase newMap)
	{
		throw new InvalidOperationException(SR.Format(SR.FrugalMap_CannotPromoteBeyondHashtable));
	}
}
