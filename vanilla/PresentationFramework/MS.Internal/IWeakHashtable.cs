using System.Collections;

namespace MS.Internal;

internal interface IWeakHashtable
{
	object this[object key] { get; }

	ICollection Keys { get; }

	int Count { get; }

	bool ContainsKey(object key);

	void Remove(object key);

	void Clear();

	void SetWeak(object key, object value);

	object UnwrapKey(object key);
}
