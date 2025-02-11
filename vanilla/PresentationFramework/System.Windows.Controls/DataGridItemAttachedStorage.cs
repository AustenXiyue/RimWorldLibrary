using System.Collections.Generic;

namespace System.Windows.Controls;

internal class DataGridItemAttachedStorage
{
	private Dictionary<object, Dictionary<DependencyProperty, object>> _itemStorageMap;

	public void SetValue(object item, DependencyProperty property, object value)
	{
		EnsureItem(item)[property] = value;
	}

	public bool TryGetValue(object item, DependencyProperty property, out object value)
	{
		value = null;
		EnsureItemStorageMap();
		if (_itemStorageMap.TryGetValue(item, out var value2))
		{
			return value2.TryGetValue(property, out value);
		}
		return false;
	}

	public void ClearValue(object item, DependencyProperty property)
	{
		EnsureItemStorageMap();
		if (_itemStorageMap.TryGetValue(item, out var value))
		{
			value.Remove(property);
		}
	}

	public void ClearItem(object item)
	{
		EnsureItemStorageMap();
		_itemStorageMap.Remove(item);
	}

	public void Clear()
	{
		_itemStorageMap = null;
	}

	private void EnsureItemStorageMap()
	{
		if (_itemStorageMap == null)
		{
			_itemStorageMap = new Dictionary<object, Dictionary<DependencyProperty, object>>();
		}
	}

	private Dictionary<DependencyProperty, object> EnsureItem(object item)
	{
		EnsureItemStorageMap();
		if (!_itemStorageMap.TryGetValue(item, out var value))
		{
			value = new Dictionary<DependencyProperty, object>();
			_itemStorageMap[item] = value;
		}
		return value;
	}
}
