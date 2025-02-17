using System.Collections;
using System.Collections.Generic;

namespace System.Runtime.CompilerServices;

internal sealed class CWTEnumerable<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable where TKey : class where TValue : class?
{
	private readonly ConditionalWeakTable<TKey, TValue> cwt;

	public CWTEnumerable(ConditionalWeakTable<TKey, TValue> table)
	{
		cwt = table;
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return cwt.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
