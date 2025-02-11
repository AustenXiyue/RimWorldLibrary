using System;
using System.Windows;
using MS.Internal.PresentationCore;

namespace MS.Internal;

[FriendAccessAllowed]
internal static class FreezableOperations
{
	internal static Freezable Clone(Freezable freezable)
	{
		return freezable?.Clone();
	}

	public static Freezable GetAsFrozen(Freezable freezable)
	{
		return freezable?.GetAsFrozen();
	}

	internal static Freezable GetAsFrozenIfPossible(Freezable freezable)
	{
		if (freezable == null)
		{
			return null;
		}
		if (freezable.CanFreeze)
		{
			freezable = freezable.GetAsFrozen();
		}
		return freezable;
	}

	internal static void PropagateChangedHandlers(Freezable oldValue, Freezable newValue, EventHandler changedHandler)
	{
		if (newValue != null && !newValue.IsFrozen)
		{
			newValue.Changed += changedHandler;
		}
		if (oldValue != null && !oldValue.IsFrozen)
		{
			oldValue.Changed -= changedHandler;
		}
	}
}
