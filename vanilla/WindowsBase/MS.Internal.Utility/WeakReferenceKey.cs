using System;

namespace MS.Internal.Utility;

internal class WeakReferenceKey<T>
{
	private WeakReference _item;

	private int _hashCode;

	public T Item => (T)_item.Target;

	public WeakReferenceKey(T item)
	{
		Invariant.Assert(item != null);
		_item = new WeakReference(item);
		_hashCode = item.GetHashCode();
	}

	public override bool Equals(object o)
	{
		if (o == this)
		{
			return true;
		}
		if (o is WeakReferenceKey<T> weakReferenceKey)
		{
			T item = Item;
			if (item == null)
			{
				return false;
			}
			if (_hashCode == weakReferenceKey._hashCode)
			{
				return object.Equals(item, weakReferenceKey.Item);
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _hashCode;
	}
}
