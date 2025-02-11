using System;
using System.Collections.Specialized;
using System.Reflection;
using MS.Internal.Hashing.WindowsBase;
using MS.Internal.WindowsBase;

namespace MS.Internal;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal static class BaseHashHelper
{
	private static HybridDictionary _table;

	static BaseHashHelper()
	{
		_table = new HybridDictionary(3);
		HashHelper.Initialize();
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal static void RegisterTypes(Assembly assembly, Type[] types)
	{
		HybridDictionary value = DictionaryFromList(types);
		lock (_table)
		{
			_table[assembly] = value;
		}
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal static bool HasReliableHashCode(object item)
	{
		if (item == null)
		{
			return false;
		}
		Type type = item.GetType();
		Assembly assembly = type.Assembly;
		HybridDictionary hybridDictionary;
		lock (_table)
		{
			hybridDictionary = (HybridDictionary)_table[assembly];
		}
		if (hybridDictionary == null)
		{
			hybridDictionary = new HybridDictionary();
			lock (_table)
			{
				_table[assembly] = hybridDictionary;
			}
		}
		return !hybridDictionary.Contains(type);
	}

	private static HybridDictionary DictionaryFromList(Type[] types)
	{
		HybridDictionary hybridDictionary = new HybridDictionary(types.Length);
		for (int i = 0; i < types.Length; i++)
		{
			hybridDictionary.Add(types[i], null);
		}
		return hybridDictionary;
	}
}
