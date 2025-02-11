using System.Collections.Generic;
using MS.Internal;

namespace System.Windows;

internal abstract class ReverseInheritProperty
{
	protected DependencyPropertyKey FlagKey;

	protected CoreFlags FlagCache;

	protected CoreFlags FlagChanged;

	protected CoreFlags FlagOldOriginCache;

	protected CoreFlags FlagNewOriginCache;

	internal ReverseInheritProperty(DependencyPropertyKey flagKey, CoreFlags flagCache, CoreFlags flagChanged)
		: this(flagKey, flagCache, flagChanged, CoreFlags.None, CoreFlags.None)
	{
	}

	internal ReverseInheritProperty(DependencyPropertyKey flagKey, CoreFlags flagCache, CoreFlags flagChanged, CoreFlags flagOldOriginCache, CoreFlags flagNewOriginCache)
	{
		FlagKey = flagKey;
		FlagCache = flagCache;
		FlagChanged = flagChanged;
		FlagOldOriginCache = flagOldOriginCache;
		FlagNewOriginCache = flagNewOriginCache;
	}

	internal abstract void FireNotifications(UIElement uie, ContentElement ce, UIElement3D uie3D, bool oldValue);

	internal void OnOriginValueChanged(DependencyObject oldOrigin, DependencyObject newOrigin, ref DeferredElementTreeState oldTreeState)
	{
		OnOriginValueChanged(oldOrigin, newOrigin, null, ref oldTreeState, null);
	}

	internal void OnOriginValueChanged(DependencyObject oldOrigin, DependencyObject newOrigin, IList<DependencyObject> otherOrigins, ref DeferredElementTreeState oldTreeState, Action<DependencyObject, bool> originChangedAction)
	{
		DeferredElementTreeState deferredElementTreeState = oldTreeState;
		oldTreeState = null;
		bool setOriginCacheFlag = originChangedAction != null && FlagOldOriginCache != 0 && FlagNewOriginCache != CoreFlags.None;
		if (oldOrigin != null)
		{
			SetCacheFlagInAncestry(oldOrigin, newValue: false, deferredElementTreeState, shortCircuit: true, setOriginCacheFlag);
		}
		if (newOrigin != null)
		{
			SetCacheFlagInAncestry(newOrigin, newValue: true, null, shortCircuit: true, setOriginCacheFlag);
		}
		int num = otherOrigins?.Count ?? 0;
		for (int i = 0; i < num; i++)
		{
			SetCacheFlagInAncestry(otherOrigins[i], newValue: true, null, shortCircuit: false, setOriginCacheFlag: false);
		}
		if (oldOrigin != null)
		{
			FirePropertyChangeInAncestry(oldOrigin, oldValue: true, deferredElementTreeState, originChangedAction);
		}
		if (newOrigin != null)
		{
			FirePropertyChangeInAncestry(newOrigin, oldValue: false, null, originChangedAction);
		}
		if (oldTreeState == null && deferredElementTreeState != null)
		{
			deferredElementTreeState.Clear();
			oldTreeState = deferredElementTreeState;
		}
	}

	private void SetCacheFlagInAncestry(DependencyObject element, bool newValue, DeferredElementTreeState treeState, bool shortCircuit, bool setOriginCacheFlag)
	{
		CastElement(element, out var uie, out var ce, out var uie3D);
		bool flag = IsFlagSet(uie, ce, uie3D, FlagCache);
		bool flag2 = setOriginCacheFlag && IsFlagSet(uie, ce, uie3D, newValue ? FlagNewOriginCache : FlagOldOriginCache);
		if (newValue == flag && (!setOriginCacheFlag || flag2) && shortCircuit)
		{
			return;
		}
		if (newValue != flag)
		{
			SetFlag(uie, ce, uie3D, FlagCache, newValue);
			SetFlag(uie, ce, uie3D, FlagChanged, !IsFlagSet(uie, ce, uie3D, FlagChanged));
		}
		if (setOriginCacheFlag && !flag2)
		{
			SetFlag(uie, ce, uie3D, newValue ? FlagNewOriginCache : FlagOldOriginCache, value: true);
		}
		if (!BlockReverseInheritance(uie, ce, uie3D))
		{
			DependencyObject inputElementParent = DeferredElementTreeState.GetInputElementParent(element, treeState);
			DependencyObject logicalParent = DeferredElementTreeState.GetLogicalParent(element, treeState);
			if (inputElementParent != null)
			{
				SetCacheFlagInAncestry(inputElementParent, newValue, treeState, shortCircuit, setOriginCacheFlag);
			}
			if (logicalParent != null && logicalParent != inputElementParent)
			{
				SetCacheFlagInAncestry(logicalParent, newValue, treeState, shortCircuit, setOriginCacheFlag);
			}
		}
	}

	private void FirePropertyChangeInAncestry(DependencyObject element, bool oldValue, DeferredElementTreeState treeState, Action<DependencyObject, bool> originChangedAction)
	{
		CastElement(element, out var uie, out var ce, out var uie3D);
		bool flag = IsFlagSet(uie, ce, uie3D, FlagChanged);
		bool flag2 = FlagOldOriginCache != 0 && IsFlagSet(uie, ce, uie3D, FlagOldOriginCache);
		bool flag3 = FlagNewOriginCache != 0 && IsFlagSet(uie, ce, uie3D, FlagNewOriginCache);
		if (!(flag || flag2 || flag3))
		{
			return;
		}
		if (flag)
		{
			SetFlag(uie, ce, uie3D, FlagChanged, value: false);
			if (oldValue)
			{
				element.ClearValue(FlagKey);
			}
			else
			{
				element.SetValue(FlagKey, value: true);
			}
			FireNotifications(uie, ce, uie3D, oldValue);
		}
		if (flag2 || flag3)
		{
			SetFlag(uie, ce, uie3D, FlagOldOriginCache, value: false);
			SetFlag(uie, ce, uie3D, FlagNewOriginCache, value: false);
			if (flag2 != flag3)
			{
				originChangedAction(element, oldValue);
			}
		}
		if (!BlockReverseInheritance(uie, ce, uie3D))
		{
			DependencyObject inputElementParent = DeferredElementTreeState.GetInputElementParent(element, treeState);
			DependencyObject logicalParent = DeferredElementTreeState.GetLogicalParent(element, treeState);
			if (inputElementParent != null)
			{
				FirePropertyChangeInAncestry(inputElementParent, oldValue, treeState, originChangedAction);
			}
			if (logicalParent != null && logicalParent != inputElementParent)
			{
				FirePropertyChangeInAncestry(logicalParent, oldValue, treeState, originChangedAction);
			}
		}
	}

	private static bool BlockReverseInheritance(UIElement uie, ContentElement ce, UIElement3D uie3D)
	{
		return uie?.BlockReverseInheritance() ?? ce?.BlockReverseInheritance() ?? uie3D?.BlockReverseInheritance() ?? false;
	}

	private static void SetFlag(UIElement uie, ContentElement ce, UIElement3D uie3D, CoreFlags flag, bool value)
	{
		if (uie != null)
		{
			uie.WriteFlag(flag, value);
		}
		else if (ce != null)
		{
			ce.WriteFlag(flag, value);
		}
		else
		{
			uie3D?.WriteFlag(flag, value);
		}
	}

	private static bool IsFlagSet(UIElement uie, ContentElement ce, UIElement3D uie3D, CoreFlags flag)
	{
		return uie?.ReadFlag(flag) ?? ce?.ReadFlag(flag) ?? uie3D?.ReadFlag(flag) ?? false;
	}

	private static void CastElement(DependencyObject o, out UIElement uie, out ContentElement ce, out UIElement3D uie3D)
	{
		uie = o as UIElement;
		ce = ((uie != null) ? null : (o as ContentElement));
		uie3D = ((uie != null || ce != null) ? null : (o as UIElement3D));
	}
}
