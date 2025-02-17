using System;
using System.Collections.Generic;

namespace MonoMod.Utils;

internal sealed class WeakReferenceComparer : EqualityComparer<WeakReference>
{
	public override bool Equals(WeakReference? x, WeakReference? y)
	{
		if (x?.SafeGetTarget() == y?.SafeGetTarget())
		{
			return x?.SafeGetIsAlive() == y?.SafeGetIsAlive();
		}
		return false;
	}

	public override int GetHashCode(WeakReference obj)
	{
		return obj.SafeGetTarget()?.GetHashCode() ?? 0;
	}
}
