using System.Linq;

namespace System.Collections.Generic;

internal static class EnumerableHelpers
{
	internal static bool TryGetCount<T>(IEnumerable<T> source, out int count)
	{
		if (source is ICollection<T> collection)
		{
			count = collection.Count;
			return true;
		}
		if (source is IIListProvider<T> iIListProvider)
		{
			return (count = iIListProvider.GetCount(onlyIfCheap: true)) >= 0;
		}
		count = -1;
		return false;
	}

	internal static void Copy<T>(IEnumerable<T> source, T[] array, int arrayIndex, int count)
	{
		if (source is ICollection<T> collection)
		{
			collection.CopyTo(array, arrayIndex);
		}
		else
		{
			IterativeCopy(source, array, arrayIndex, count);
		}
	}

	internal static void IterativeCopy<T>(IEnumerable<T> source, T[] array, int arrayIndex, int count)
	{
		foreach (T item in source)
		{
			array[arrayIndex++] = item;
		}
	}

	internal static T[] ToArray<T>(IEnumerable<T> source)
	{
		if (source is ICollection<T> { Count: var count } collection)
		{
			if (count == 0)
			{
				return Array.Empty<T>();
			}
			T[] array = new T[count];
			collection.CopyTo(array, 0);
			return array;
		}
		System.Collections.Generic.LargeArrayBuilder<T> largeArrayBuilder = new System.Collections.Generic.LargeArrayBuilder<T>(initialize: true);
		largeArrayBuilder.AddRange(source);
		return largeArrayBuilder.ToArray();
	}

	internal static T[] ToArray<T>(IEnumerable<T> source, out int length)
	{
		if (source is ICollection<T> { Count: var count } collection)
		{
			if (count != 0)
			{
				T[] array = new T[count];
				collection.CopyTo(array, 0);
				length = count;
				return array;
			}
		}
		else
		{
			using IEnumerator<T> enumerator = source.GetEnumerator();
			if (enumerator.MoveNext())
			{
				T[] array2 = new T[4]
				{
					enumerator.Current,
					default(T),
					default(T),
					default(T)
				};
				int num = 1;
				while (enumerator.MoveNext())
				{
					if (num == array2.Length)
					{
						int num2 = num << 1;
						if ((uint)num2 > 2146435071u)
						{
							num2 = ((2146435071 <= num) ? (num + 1) : 2146435071);
						}
						Array.Resize(ref array2, num2);
					}
					array2[num++] = enumerator.Current;
				}
				length = num;
				return array2;
			}
		}
		length = 0;
		return Array.Empty<T>();
	}
}
