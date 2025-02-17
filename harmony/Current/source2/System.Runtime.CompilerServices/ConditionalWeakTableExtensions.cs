using System.Collections.Generic;
using System.Reflection;

namespace System.Runtime.CompilerServices;

internal static class ConditionalWeakTableExtensions
{
	private static class CWTInfoHolder<TKey, TValue> where TKey : class where TValue : class?
	{
		public delegate IEnumerable<TKey> GetKeys(ConditionalWeakTable<TKey, TValue> cwt);

		private static readonly MethodInfo? get_KeysMethod;

		public static readonly GetKeys? get_Keys;

		static CWTInfoHolder()
		{
			get_KeysMethod = typeof(ConditionalWeakTable<TKey, TValue>).GetProperty("Keys", BindingFlags.Instance | BindingFlags.NonPublic)?.GetGetMethod(nonPublic: true);
			if ((object)get_KeysMethod != null)
			{
				get_Keys = (GetKeys)Delegate.CreateDelegate(typeof(GetKeys), get_KeysMethod);
			}
		}
	}

	public static IEnumerable<KeyValuePair<TKey, TValue>> AsEnumerable<TKey, TValue>(this ConditionalWeakTable<TKey, TValue> self) where TKey : class where TValue : class?
	{
		System.ThrowHelper.ThrowIfArgumentNull(self, "self");
		if (self is IEnumerable<KeyValuePair<TKey, TValue>> result)
		{
			return result;
		}
		if (self is ICWTEnumerable<KeyValuePair<TKey, TValue>> iCWTEnumerable)
		{
			return iCWTEnumerable.SelfEnumerable;
		}
		return new CWTEnumerable<TKey, TValue>(self);
	}

	public static IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator<TKey, TValue>(this ConditionalWeakTable<TKey, TValue> self) where TKey : class where TValue : class?
	{
		System.ThrowHelper.ThrowIfArgumentNull(self, "self");
		if (self is IEnumerable<KeyValuePair<TKey, TValue>> enumerable)
		{
			return enumerable.GetEnumerator();
		}
		if (self is ICWTEnumerable<KeyValuePair<TKey, TValue>> iCWTEnumerable)
		{
			return iCWTEnumerable.GetEnumerator();
		}
		CWTInfoHolder<TKey, TValue>.GetKeys get_Keys = CWTInfoHolder<TKey, TValue>.get_Keys;
		if (get_Keys != null)
		{
			return Enumerate(self, get_Keys(self));
		}
		throw new PlatformNotSupportedException("Could not get Keys property of ConditionalWeakTable to enumerate it");
		static IEnumerator<KeyValuePair<TKey, TValue>> Enumerate(ConditionalWeakTable<TKey, TValue> cwt, IEnumerable<TKey> keys)
		{
			foreach (TKey key in keys)
			{
				if (cwt.TryGetValue(key, out var value))
				{
					yield return new KeyValuePair<TKey, TValue>(key, value);
				}
			}
		}
	}

	public static void Clear<TKey, TValue>(this ConditionalWeakTable<TKey, TValue> self) where TKey : class where TValue : class?
	{
		System.ThrowHelper.ThrowIfArgumentNull(self, "self");
		using IEnumerator<KeyValuePair<TKey, TValue>> enumerator = self.GetEnumerator();
		while (enumerator.MoveNext())
		{
			self.Remove(enumerator.Current.Key);
		}
	}

	public static bool TryAdd<TKey, TValue>(this ConditionalWeakTable<TKey, TValue> self, TKey key, TValue value) where TKey : class where TValue : class?
	{
		System.ThrowHelper.ThrowIfArgumentNull(self, "self");
		bool didAdd = false;
		self.GetValue(key, delegate
		{
			didAdd = true;
			return value;
		});
		return didAdd;
	}
}
