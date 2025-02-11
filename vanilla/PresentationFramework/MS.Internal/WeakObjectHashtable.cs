using System;
using System.Collections;

namespace MS.Internal;

internal sealed class WeakObjectHashtable : Hashtable, IWeakHashtable
{
	private class WeakKeyComparer : IEqualityComparer
	{
		bool IEqualityComparer.Equals(object x, object y)
		{
			if (x == null)
			{
				return y == null;
			}
			if (y == null || x.GetHashCode() != y.GetHashCode())
			{
				return false;
			}
			if (x == y)
			{
				return true;
			}
			if (x is EqualityWeakReference equalityWeakReference)
			{
				x = equalityWeakReference.Target;
				if (x == null)
				{
					return false;
				}
			}
			if (y is EqualityWeakReference equalityWeakReference2)
			{
				y = equalityWeakReference2.Target;
				if (y == null)
				{
					return false;
				}
			}
			return object.Equals(x, y);
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

	internal WeakObjectHashtable()
		: base(_comparer)
	{
	}

	public void SetWeak(object key, object value)
	{
		ScavengeKeys();
		WrapKey(ref key);
		this[key] = value;
	}

	private void WrapKey(ref object key)
	{
		if (key != null && !key.GetType().IsValueType)
		{
			key = new EqualityWeakReference(key);
		}
	}

	public object UnwrapKey(object key)
	{
		if (!(key is EqualityWeakReference equalityWeakReference))
		{
			return key;
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
		long num = totalMemory - _lastGlobalMem;
		long num2 = count - _lastHashCount;
		if (num < 0 && num2 >= 0)
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
}
