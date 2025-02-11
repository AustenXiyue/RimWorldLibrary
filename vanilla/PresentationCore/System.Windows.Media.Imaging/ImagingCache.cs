using System.Collections;

namespace System.Windows.Media.Imaging;

internal static class ImagingCache
{
	private static Hashtable _imageCache = new Hashtable();

	private static Hashtable _decoderCache = new Hashtable();

	private static int MAX_CACHE_SIZE = 300;

	internal static void AddToImageCache(Uri uri, object obj)
	{
		AddToCache(uri, obj, _imageCache);
	}

	internal static void RemoveFromImageCache(Uri uri)
	{
		RemoveFromCache(uri, _imageCache);
	}

	internal static object CheckImageCache(Uri uri)
	{
		return CheckCache(uri, _imageCache);
	}

	internal static void AddToDecoderCache(Uri uri, object obj)
	{
		AddToCache(uri, obj, _decoderCache);
	}

	internal static void RemoveFromDecoderCache(Uri uri)
	{
		RemoveFromCache(uri, _decoderCache);
	}

	internal static object CheckDecoderCache(Uri uri)
	{
		return CheckCache(uri, _decoderCache);
	}

	private static void AddToCache(Uri uri, object obj, Hashtable table)
	{
		lock (table)
		{
			if (table.Contains(uri))
			{
				return;
			}
			if (table.Count == MAX_CACHE_SIZE)
			{
				ArrayList arrayList = new ArrayList();
				foreach (DictionaryEntry item in table)
				{
					if (item.Value is WeakReference { Target: null })
					{
						arrayList.Add(item.Key);
					}
				}
				foreach (object item2 in arrayList)
				{
					table.Remove(item2);
				}
			}
			if (table.Count != MAX_CACHE_SIZE)
			{
				table[uri] = obj;
			}
		}
	}

	private static void RemoveFromCache(Uri uri, Hashtable table)
	{
		lock (table)
		{
			if (table.Contains(uri))
			{
				table.Remove(uri);
			}
		}
	}

	private static object CheckCache(Uri uri, Hashtable table)
	{
		lock (table)
		{
			return table[uri];
		}
	}
}
