using System.Collections;
using System.Windows;

namespace MS.Internal.Automation;

internal class EventPropertyMap
{
	private static ReaderWriterLockWrapper _propertyLock = new ReaderWriterLockWrapper();

	private static Hashtable _propertyTable;

	private EventPropertyMap()
	{
	}

	internal static bool IsInterestingDP(DependencyProperty dp)
	{
		using (_propertyLock.ReadLock)
		{
			if (_propertyTable != null && _propertyTable.ContainsKey(dp))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool AddPropertyNotify(DependencyProperty[] properties)
	{
		if (properties == null)
		{
			return false;
		}
		bool result = false;
		using (_propertyLock.WriteLock)
		{
			if (_propertyTable == null)
			{
				_propertyTable = new Hashtable(20, 0.1f);
				result = true;
			}
			_ = _propertyTable.Count;
			foreach (DependencyProperty dependencyProperty in properties)
			{
				if (dependencyProperty != null)
				{
					int num = 0;
					if (_propertyTable.ContainsKey(dependencyProperty))
					{
						num = (int)_propertyTable[dependencyProperty];
					}
					num++;
					_propertyTable[dependencyProperty] = num;
				}
			}
			return result;
		}
	}

	internal static bool RemovePropertyNotify(DependencyProperty[] properties)
	{
		bool flag = false;
		using (_propertyLock.WriteLock)
		{
			if (_propertyTable != null)
			{
				_ = _propertyTable.Count;
				foreach (DependencyProperty key in properties)
				{
					if (_propertyTable.ContainsKey(key))
					{
						int num = (int)_propertyTable[key];
						num--;
						if (num > 0)
						{
							_propertyTable[key] = num;
						}
						else
						{
							_propertyTable.Remove(key);
						}
					}
				}
				if (_propertyTable.Count == 0)
				{
					_propertyTable = null;
				}
			}
			return _propertyTable == null;
		}
	}
}
