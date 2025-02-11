using System;
using System.Collections;

namespace MS.Internal;

internal sealed class WeakHashtable : Hashtable, IWeakHashtable
{
	private class WeakKeyComparer : IEqualityComparer
	{
		bool IEqualityComparer.Equals(object x, object y)
		{
			if (x == null)
			{
				return y == null;
			}
			if (y != null && x.GetHashCode() == y.GetHashCode())
			{
				EqualityWeakReference equalityWeakReference = x as EqualityWeakReference;
				EqualityWeakReference equalityWeakReference2 = y as EqualityWeakReference;
				if (equalityWeakReference != null && equalityWeakReference2 != null && !equalityWeakReference2.IsAlive && !equalityWeakReference.IsAlive)
				{
					return true;
				}
				if (equalityWeakReference != null)
				{
					x = equalityWeakReference.Target;
				}
				if (equalityWeakReference2 != null)
				{
					y = equalityWeakReference2.Target;
				}
				return x == y;
			}
			return false;
		}

		int IEqualityComparer.GetHashCode(object obj)
		{
			return obj.GetHashCode();
		}
	}

	internal sealed class EqualityWeakReference
	{
		private int _hashCode;

		private WeakReference _weakRef;

		public bool IsAlive => _weakRef.IsAlive;

		public object Target => _weakRef.Target;

		internal EqualityWeakReference(object o)
		{
			_weakRef = new WeakReference(o);
			_hashCode = o.GetHashCode();
		}

		public override bool Equals(object o)
		{
			if (o == null)
			{
				return false;
			}
			if (o.GetHashCode() != _hashCode)
			{
				return false;
			}
			if (o == this || o == Target)
			{
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return _hashCode;
		}
	}

	private static IEqualityComparer _comparer = new WeakKeyComparer();

	private long _lastGlobalMem;

	private int _lastHashCount;

	internal WeakHashtable()
		: base(_comparer)
	{
	}

	public override void Clear()
	{
		base.Clear();
	}

	public override void Remove(object key)
	{
		base.Remove(key);
	}

	public void SetWeak(object key, object value)
	{
		ScavengeKeys();
		this[new EqualityWeakReference(key)] = value;
	}

	public object UnwrapKey(object key)
	{
		if (!(key is EqualityWeakReference equalityWeakReference))
		{
			return null;
		}
		return equalityWeakReference.Target;
	}

	private void ScavengeKeys()
	{
		int count = Count;
		if (count == 0)
		{
			return;
		}
		if (_lastHashCount == 0)
		{
			_lastHashCount = count;
			return;
		}
		long totalMemory = GC.GetTotalMemory(forceFullCollection: false);
		if (_lastGlobalMem == 0L)
		{
			_lastGlobalMem = totalMemory;
			return;
		}
		float num = (float)(totalMemory - _lastGlobalMem) / (float)_lastGlobalMem;
		float num2 = (float)(count - _lastHashCount) / (float)_lastHashCount;
		if (num < 0f && num2 >= 0f)
		{
			ArrayList arrayList = null;
			foreach (object key in Keys)
			{
				if (key is EqualityWeakReference { IsAlive: false } equalityWeakReference)
				{
					if (arrayList == null)
					{
						arrayList = new ArrayList();
					}
					arrayList.Add(equalityWeakReference);
				}
			}
			if (arrayList != null)
			{
				foreach (object item in arrayList)
				{
					Remove(item);
				}
			}
		}
		_lastGlobalMem = totalMemory;
		_lastHashCount = count;
	}

	public static IWeakHashtable FromKeyType(Type tKey)
	{
		if (tKey == typeof(object) || tKey.IsValueType)
		{
			return new WeakObjectHashtable();
		}
		return new WeakHashtable();
	}
}
