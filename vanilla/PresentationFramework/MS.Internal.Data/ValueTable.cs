using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;

namespace MS.Internal.Data;

internal sealed class ValueTable : IWeakEventListener
{
	private class ValueTableKey
	{
		private WeakReference _item;

		private WeakReference _descriptor;

		private int _hashCode;

		public object Item => _item.Target;

		public PropertyDescriptor PropertyDescriptor => (PropertyDescriptor)_descriptor.Target;

		public bool IsStale
		{
			get
			{
				if (Item != null)
				{
					return PropertyDescriptor == null;
				}
				return true;
			}
		}

		public ValueTableKey(object item, PropertyDescriptor pd)
		{
			Invariant.Assert(item != null && pd != null);
			_item = new WeakReference(item);
			_descriptor = new WeakReference(pd);
			_hashCode = item.GetHashCode() + pd.GetHashCode();
		}

		public override bool Equals(object o)
		{
			if (o == this)
			{
				return true;
			}
			if (o is ValueTableKey valueTableKey)
			{
				object item = Item;
				PropertyDescriptor propertyDescriptor = PropertyDescriptor;
				if (item == null || propertyDescriptor == null)
				{
					return false;
				}
				if (_hashCode == valueTableKey._hashCode && object.Equals(item, valueTableKey.Item))
				{
					return object.Equals(propertyDescriptor, valueTableKey.PropertyDescriptor);
				}
				return false;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return _hashCode;
		}
	}

	private HybridDictionary _table;

	private static object CachedNull = new object();

	internal static bool ShouldCache(object item, PropertyDescriptor pd)
	{
		if (SystemDataHelper.IsDataSetCollectionProperty(pd))
		{
			return true;
		}
		if (SystemXmlLinqHelper.IsXLinqCollectionProperty(pd))
		{
			return true;
		}
		return false;
	}

	internal object GetValue(object item, PropertyDescriptor pd, bool indexerIsNext)
	{
		if (!ShouldCache(item, pd))
		{
			return pd.GetValue(item);
		}
		if (_table == null)
		{
			_table = new HybridDictionary();
		}
		bool isXLinqCollectionProperty = SystemXmlLinqHelper.IsXLinqCollectionProperty(pd);
		ValueTableKey key = new ValueTableKey(item, pd);
		object value = _table[key];
		Action action = delegate
		{
			if (value == null)
			{
				if (SystemDataHelper.IsDataSetCollectionProperty(pd))
				{
					value = SystemDataHelper.GetValue(item, pd, !FrameworkAppContextSwitches.DoNotUseFollowParentWhenBindingToADODataRelation);
				}
				else if (isXLinqCollectionProperty)
				{
					value = new XDeferredAxisSource(item, pd);
				}
				else
				{
					value = pd.GetValue(item);
				}
				if (value == null)
				{
					value = CachedNull;
				}
				if (SystemDataHelper.IsDataSetCollectionProperty(pd))
				{
					value = new WeakReference(value);
				}
				_table[key] = value;
			}
			if (SystemDataHelper.IsDataSetCollectionProperty(pd) && value is WeakReference weakReference)
			{
				value = weakReference.Target;
			}
		};
		action();
		if (value == null)
		{
			action();
		}
		if (value == CachedNull)
		{
			value = null;
		}
		else if (isXLinqCollectionProperty && !indexerIsNext)
		{
			XDeferredAxisSource xDeferredAxisSource = (XDeferredAxisSource)value;
			value = xDeferredAxisSource.FullCollection;
		}
		return value;
	}

	internal void RegisterForChanges(object item, PropertyDescriptor pd, DataBindEngine engine)
	{
		if (_table == null)
		{
			_table = new HybridDictionary();
		}
		ValueTableKey key = new ValueTableKey(item, pd);
		if (_table[key] == null)
		{
			if (item is INotifyPropertyChanged source)
			{
				PropertyChangedEventManager.AddHandler(source, OnPropertyChanged, pd.Name);
			}
			else
			{
				ValueChangedEventManager.AddHandler(item, OnValueChanged, pd);
			}
		}
	}

	private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		string text = e.PropertyName;
		if (text == null)
		{
			text = string.Empty;
		}
		InvalidateCache(sender, text);
	}

	private void OnValueChanged(object sender, ValueChangedEventArgs e)
	{
		InvalidateCache(sender, e.PropertyDescriptor);
	}

	bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
	{
		return false;
	}

	private void InvalidateCache(object item, string name)
	{
		if (name == string.Empty)
		{
			foreach (PropertyDescriptor item2 in GetPropertiesForItem(item))
			{
				InvalidateCache(item, item2);
			}
			return;
		}
		PropertyDescriptor pd = ((!(item is ICustomTypeDescriptor)) ? TypeDescriptor.GetProperties(item.GetType())[name] : TypeDescriptor.GetProperties(item)[name]);
		InvalidateCache(item, pd);
	}

	private void InvalidateCache(object item, PropertyDescriptor pd)
	{
		if (!SystemXmlLinqHelper.IsXLinqCollectionProperty(pd))
		{
			ValueTableKey key = new ValueTableKey(item, pd);
			_table.Remove(key);
		}
	}

	private IEnumerable<PropertyDescriptor> GetPropertiesForItem(object item)
	{
		List<PropertyDescriptor> list = new List<PropertyDescriptor>();
		foreach (DictionaryEntry item2 in _table)
		{
			ValueTableKey valueTableKey = (ValueTableKey)item2.Key;
			if (object.Equals(item, valueTableKey.Item))
			{
				list.Add(valueTableKey.PropertyDescriptor);
			}
		}
		return list;
	}

	internal bool Purge()
	{
		if (_table == null)
		{
			return false;
		}
		bool flag = false;
		ICollection keys = _table.Keys;
		foreach (ValueTableKey item in keys)
		{
			if (item.IsStale)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			ValueTableKey[] array = new ValueTableKey[keys.Count];
			keys.CopyTo(array, 0);
			for (int num = array.Length - 1; num >= 0; num--)
			{
				if (array[num].IsStale)
				{
					_table.Remove(array[num]);
				}
			}
		}
		return flag;
	}
}
