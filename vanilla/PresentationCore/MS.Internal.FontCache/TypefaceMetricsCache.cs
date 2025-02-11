using System.Collections;

namespace MS.Internal.FontCache;

internal static class TypefaceMetricsCache
{
	private static Hashtable _hashTable = new Hashtable(64);

	private static readonly object _lock = new object();

	private const int MaxCacheCapacity = 64;

	internal static object ReadonlyLookup(object key)
	{
		return _hashTable[key];
	}

	internal static void Add(object key, object value)
	{
		lock (_lock)
		{
			if (_hashTable.Count >= 64)
			{
				_hashTable = new Hashtable(64);
			}
			_hashTable[key] = value;
		}
	}
}
