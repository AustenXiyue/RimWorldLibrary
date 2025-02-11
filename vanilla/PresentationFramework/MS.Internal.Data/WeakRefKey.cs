using System;

namespace MS.Internal.Data;

internal struct WeakRefKey
{
	private WeakReference _weakRef;

	private int _hashCode;

	internal object Target => _weakRef.Target;

	internal WeakRefKey(object target)
	{
		_weakRef = new WeakReference(target);
		_hashCode = target?.GetHashCode() ?? 314159;
	}

	public override int GetHashCode()
	{
		return _hashCode;
	}

	public override bool Equals(object o)
	{
		if (o is WeakRefKey weakRefKey)
		{
			object target = Target;
			object target2 = weakRefKey.Target;
			if (target != null && target2 != null)
			{
				return target == target2;
			}
			return _weakRef == weakRefKey._weakRef;
		}
		return false;
	}

	public static bool operator ==(WeakRefKey left, WeakRefKey right)
	{
		if ((object)left == null)
		{
			return (object)right == null;
		}
		return left.Equals(right);
	}

	public static bool operator !=(WeakRefKey left, WeakRefKey right)
	{
		return !(left == right);
	}
}
