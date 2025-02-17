using System.Collections.Generic;

namespace System.Collections.Concurrent;

internal static class ConcurrentExtensions
{
	public static void Clear<T>(this ConcurrentBag<T> bag)
	{
		System.ThrowHelper.ThrowIfArgumentNull(bag, "bag");
		T result;
		while (bag.TryTake(out result))
		{
		}
	}

	public static void Clear<T>(this ConcurrentQueue<T> queue)
	{
		System.ThrowHelper.ThrowIfArgumentNull(queue, "queue");
		T result;
		while (queue.TryDequeue(out result))
		{
		}
	}

	public static TValue AddOrUpdate<TKey, TValue, TArg>(this ConcurrentDictionary<TKey, TValue> dict, TKey key, Func<TKey, TArg, TValue> addValueFactory, Func<TKey, TValue, TArg, TValue> updateValueFactory, TArg factoryArgument) where TKey : notnull
	{
		System.ThrowHelper.ThrowIfArgumentNull(dict, "dict");
		return dict.AddOrUpdate(key, (TKey k) => addValueFactory(k, factoryArgument), (TKey k, TValue v) => updateValueFactory(k, v, factoryArgument));
	}

	public static TValue GetOrAdd<TKey, TValue, TArg>(this ConcurrentDictionary<TKey, TValue> dict, TKey key, Func<TKey, TArg, TValue> valueFactory, TArg factoryArgument) where TKey : notnull
	{
		System.ThrowHelper.ThrowIfArgumentNull(dict, "dict");
		return dict.GetOrAdd(key, (TKey k) => valueFactory(k, factoryArgument));
	}

	public static bool TryRemove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, KeyValuePair<TKey, TValue> item) where TKey : notnull
	{
		System.ThrowHelper.ThrowIfArgumentNull(dict, "dict");
		if (dict.TryRemove(item.Key, out TValue value))
		{
			if (EqualityComparer<TValue>.Default.Equals(item.Value, value))
			{
				return true;
			}
			dict.AddOrUpdate(item.Key, (TKey _) => value, (TKey _, TValue _) => value);
			return false;
		}
		return false;
	}
}
