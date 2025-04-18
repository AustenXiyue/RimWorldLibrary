using System;
using System.Collections.Generic;
using System.Linq;

namespace HarmonyLib;

public static class CollectionExtensions
{
	public static void Do<T>(this IEnumerable<T> sequence, Action<T> action)
	{
		if (sequence != null)
		{
			IEnumerator<T> enumerator = sequence.GetEnumerator();
			while (enumerator.MoveNext())
			{
				action(enumerator.Current);
			}
		}
	}

	public static void DoIf<T>(this IEnumerable<T> sequence, Func<T, bool> condition, Action<T> action)
	{
		sequence.Where(condition).Do(action);
	}

	public static IEnumerable<T> AddItem<T>(this IEnumerable<T> sequence, T item)
	{
		return (sequence ?? Array.Empty<T>()).Concat(new T[1] { item });
	}

	public static T[] AddToArray<T>(this T[] sequence, T item)
	{
		return sequence.AddItem(item).ToArray();
	}

	public static T[] AddRangeToArray<T>(this T[] sequence, T[] items)
	{
		return (sequence ?? Enumerable.Empty<T>()).Concat(items).ToArray();
	}

	internal static Dictionary<K, V> Merge<K, V>(this IEnumerable<KeyValuePair<K, V>> firstDict, params IEnumerable<KeyValuePair<K, V>>[] otherDicts)
	{
		Dictionary<K, V> dictionary = new Dictionary<K, V>();
		foreach (KeyValuePair<K, V> item in firstDict)
		{
			dictionary[item.Key] = item.Value;
		}
		foreach (IEnumerable<KeyValuePair<K, V>> enumerable in otherDicts)
		{
			foreach (KeyValuePair<K, V> item2 in enumerable)
			{
				dictionary[item2.Key] = item2.Value;
			}
		}
		return dictionary;
	}

	internal static Dictionary<K, V> TransformKeys<K, V>(this Dictionary<K, V> origDict, Func<K, K> transform)
	{
		Dictionary<K, V> dictionary = new Dictionary<K, V>();
		foreach (KeyValuePair<K, V> item in origDict)
		{
			dictionary.Add(transform(item.Key), item.Value);
		}
		return dictionary;
	}
}
