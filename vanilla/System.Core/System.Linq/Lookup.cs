using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity;

namespace System.Linq;

/// <summary>Represents a collection of keys each mapped to one or more values.</summary>
/// <typeparam name="TKey">The type of the keys in the <see cref="T:System.Linq.Lookup`2" />.</typeparam>
/// <typeparam name="TElement">The type of the elements of each <see cref="T:System.Collections.Generic.IEnumerable`1" /> value in the <see cref="T:System.Linq.Lookup`2" />.</typeparam>
/// <filterpriority>2</filterpriority>
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(SystemLinq_LookupDebugView<, >))]
public class Lookup<TKey, TElement> : ILookup<TKey, TElement>, IEnumerable<IGrouping<TKey, TElement>>, IEnumerable, IIListProvider<IGrouping<TKey, TElement>>
{
	private readonly IEqualityComparer<TKey> _comparer;

	private Grouping<TKey, TElement>[] _groupings;

	private Grouping<TKey, TElement> _lastGrouping;

	private int _count;

	/// <summary>Gets the number of key/value collection pairs in the <see cref="T:System.Linq.Lookup`2" />.</summary>
	/// <returns>The number of key/value collection pairs in the <see cref="T:System.Linq.Lookup`2" />.</returns>
	public int Count => _count;

	/// <summary>Gets the collection of values indexed by the specified key.</summary>
	/// <returns>The collection of values indexed by the specified key.</returns>
	/// <param name="key">The key of the desired collection of values.</param>
	public IEnumerable<TElement> this[TKey key]
	{
		get
		{
			Grouping<TKey, TElement> grouping = GetGrouping(key, create: false);
			if (grouping != null)
			{
				return grouping;
			}
			return Array.Empty<TElement>();
		}
	}

	internal static Lookup<TKey, TElement> Create<TSource>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
	{
		Lookup<TKey, TElement> lookup = new Lookup<TKey, TElement>(comparer);
		foreach (TSource item in source)
		{
			lookup.GetGrouping(keySelector(item), create: true).Add(elementSelector(item));
		}
		return lookup;
	}

	internal static Lookup<TKey, TElement> Create(IEnumerable<TElement> source, Func<TElement, TKey> keySelector, IEqualityComparer<TKey> comparer)
	{
		Lookup<TKey, TElement> lookup = new Lookup<TKey, TElement>(comparer);
		foreach (TElement item in source)
		{
			lookup.GetGrouping(keySelector(item), create: true).Add(item);
		}
		return lookup;
	}

	internal static Lookup<TKey, TElement> CreateForJoin(IEnumerable<TElement> source, Func<TElement, TKey> keySelector, IEqualityComparer<TKey> comparer)
	{
		Lookup<TKey, TElement> lookup = new Lookup<TKey, TElement>(comparer);
		foreach (TElement item in source)
		{
			TKey val = keySelector(item);
			if (val != null)
			{
				lookup.GetGrouping(val, create: true).Add(item);
			}
		}
		return lookup;
	}

	private Lookup(IEqualityComparer<TKey> comparer)
	{
		_comparer = comparer ?? EqualityComparer<TKey>.Default;
		_groupings = new Grouping<TKey, TElement>[7];
	}

	/// <summary>Determines whether a specified key is in the <see cref="T:System.Linq.Lookup`2" />.</summary>
	/// <returns>true if <paramref name="key" /> is in the <see cref="T:System.Linq.Lookup`2" />; otherwise, false.</returns>
	/// <param name="key">The key to find in the <see cref="T:System.Linq.Lookup`2" />.</param>
	public bool Contains(TKey key)
	{
		return GetGrouping(key, create: false) != null;
	}

	/// <summary>Returns a generic enumerator that iterates through the <see cref="T:System.Linq.Lookup`2" />.</summary>
	/// <returns>An enumerator for the <see cref="T:System.Linq.Lookup`2" />.</returns>
	public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
	{
		Grouping<TKey, TElement> g = _lastGrouping;
		if (g != null)
		{
			do
			{
				g = g._next;
				yield return g;
			}
			while (g != _lastGrouping);
		}
	}

	IGrouping<TKey, TElement>[] IIListProvider<IGrouping<TKey, TElement>>.ToArray()
	{
		IGrouping<TKey, TElement>[] array = new IGrouping<TKey, TElement>[_count];
		int num = 0;
		Grouping<TKey, TElement> grouping = _lastGrouping;
		if (grouping != null)
		{
			do
			{
				grouping = (Grouping<TKey, TElement>)(array[num] = grouping._next);
				num++;
			}
			while (grouping != _lastGrouping);
		}
		return array;
	}

	internal TResult[] ToArray<TResult>(Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
	{
		TResult[] array = new TResult[_count];
		int num = 0;
		Grouping<TKey, TElement> grouping = _lastGrouping;
		if (grouping != null)
		{
			do
			{
				grouping = grouping._next;
				grouping.Trim();
				array[num] = resultSelector(grouping._key, grouping._elements);
				num++;
			}
			while (grouping != _lastGrouping);
		}
		return array;
	}

	List<IGrouping<TKey, TElement>> IIListProvider<IGrouping<TKey, TElement>>.ToList()
	{
		List<IGrouping<TKey, TElement>> list = new List<IGrouping<TKey, TElement>>(_count);
		Grouping<TKey, TElement> grouping = _lastGrouping;
		if (grouping != null)
		{
			do
			{
				grouping = grouping._next;
				list.Add(grouping);
			}
			while (grouping != _lastGrouping);
		}
		return list;
	}

	internal List<TResult> ToList<TResult>(Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
	{
		List<TResult> list = new List<TResult>(_count);
		Grouping<TKey, TElement> grouping = _lastGrouping;
		if (grouping != null)
		{
			do
			{
				grouping = grouping._next;
				grouping.Trim();
				list.Add(resultSelector(grouping._key, grouping._elements));
			}
			while (grouping != _lastGrouping);
		}
		return list;
	}

	int IIListProvider<IGrouping<TKey, TElement>>.GetCount(bool onlyIfCheap)
	{
		return _count;
	}

	/// <summary>Applies a transform function to each key and its associated values and returns the results.</summary>
	/// <returns>A collection that contains one value for each key/value collection pair in the <see cref="T:System.Linq.Lookup`2" />.</returns>
	/// <param name="resultSelector">A function to project a result value from each key and its associated values.</param>
	/// <typeparam name="TResult">The type of the result values produced by <paramref name="resultSelector" />.</typeparam>
	/// <filterpriority>2</filterpriority>
	public IEnumerable<TResult> ApplyResultSelector<TResult>(Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
	{
		Grouping<TKey, TElement> g = _lastGrouping;
		if (g != null)
		{
			do
			{
				g = g._next;
				g.Trim();
				yield return resultSelector(g._key, g._elements);
			}
			while (g != _lastGrouping);
		}
	}

	/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Linq.Lookup`2" />. This class cannot be inherited.</summary>
	/// <returns>An enumerator for the <see cref="T:System.Linq.Lookup`2" />.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private int InternalGetHashCode(TKey key)
	{
		if (key != null)
		{
			return _comparer.GetHashCode(key) & 0x7FFFFFFF;
		}
		return 0;
	}

	internal Grouping<TKey, TElement> GetGrouping(TKey key, bool create)
	{
		int num = InternalGetHashCode(key);
		for (Grouping<TKey, TElement> grouping = _groupings[num % _groupings.Length]; grouping != null; grouping = grouping._hashNext)
		{
			if (grouping._hashCode == num && _comparer.Equals(grouping._key, key))
			{
				return grouping;
			}
		}
		if (create)
		{
			if (_count == _groupings.Length)
			{
				Resize();
			}
			int num2 = num % _groupings.Length;
			Grouping<TKey, TElement> grouping2 = new Grouping<TKey, TElement>();
			grouping2._key = key;
			grouping2._hashCode = num;
			grouping2._elements = new TElement[1];
			grouping2._hashNext = _groupings[num2];
			_groupings[num2] = grouping2;
			if (_lastGrouping == null)
			{
				grouping2._next = grouping2;
			}
			else
			{
				grouping2._next = _lastGrouping._next;
				_lastGrouping._next = grouping2;
			}
			_lastGrouping = grouping2;
			_count++;
			return grouping2;
		}
		return null;
	}

	private void Resize()
	{
		int num = checked(_count * 2 + 1);
		Grouping<TKey, TElement>[] array = new Grouping<TKey, TElement>[num];
		Grouping<TKey, TElement> grouping = _lastGrouping;
		do
		{
			grouping = grouping._next;
			int num2 = grouping._hashCode % num;
			grouping._hashNext = array[num2];
			array[num2] = grouping;
		}
		while (grouping != _lastGrouping);
		_groupings = array;
	}

	internal Lookup()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
