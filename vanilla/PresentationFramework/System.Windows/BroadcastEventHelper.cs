using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.PresentationFramework;

namespace System.Windows;

internal static class BroadcastEventHelper
{
	private struct BroadcastEventData
	{
		internal DependencyObject Root;

		internal RoutedEvent RoutedEvent;

		internal List<DependencyObject> EventRoute;

		internal BroadcastEventData(DependencyObject root, RoutedEvent routedEvent, List<DependencyObject> eventRoute)
		{
			Root = root;
			RoutedEvent = routedEvent;
			EventRoute = eventRoute;
		}
	}

	private static VisitedCallback<BroadcastEventData> BroadcastDelegate = OnBroadcastCallback;

	internal static void AddLoadedCallback(DependencyObject d, DependencyObject logicalParent)
	{
		DispatcherOperationCallback dispatcherOperationCallback = BroadcastLoadedEvent;
		LoadedOrUnloadedOperation loadedOrUnloadedOperation = MediaContext.From(d.Dispatcher).AddLoadedOrUnloadedCallback(dispatcherOperationCallback, d);
		DispatcherOperation dispatcherOperation = d.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, dispatcherOperationCallback, d);
		d.SetValue(FrameworkElement.LoadedPendingPropertyKey, new object[3] { loadedOrUnloadedOperation, dispatcherOperation, logicalParent });
	}

	internal static void RemoveLoadedCallback(DependencyObject d, object[] loadedPending)
	{
		if (loadedPending != null)
		{
			d.ClearValue(FrameworkElement.LoadedPendingPropertyKey);
			DispatcherOperation dispatcherOperation = (DispatcherOperation)loadedPending[1];
			if (dispatcherOperation.Status == DispatcherOperationStatus.Pending)
			{
				dispatcherOperation.Abort();
			}
			MediaContext.From(d.Dispatcher).RemoveLoadedOrUnloadedCallback((LoadedOrUnloadedOperation)loadedPending[0]);
		}
	}

	internal static void AddUnloadedCallback(DependencyObject d, DependencyObject logicalParent)
	{
		DispatcherOperationCallback dispatcherOperationCallback = BroadcastUnloadedEvent;
		LoadedOrUnloadedOperation loadedOrUnloadedOperation = MediaContext.From(d.Dispatcher).AddLoadedOrUnloadedCallback(dispatcherOperationCallback, d);
		DispatcherOperation dispatcherOperation = d.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, dispatcherOperationCallback, d);
		d.SetValue(FrameworkElement.UnloadedPendingPropertyKey, new object[3] { loadedOrUnloadedOperation, dispatcherOperation, logicalParent });
	}

	internal static void RemoveUnloadedCallback(DependencyObject d, object[] unloadedPending)
	{
		if (unloadedPending != null)
		{
			d.ClearValue(FrameworkElement.UnloadedPendingPropertyKey);
			DispatcherOperation dispatcherOperation = (DispatcherOperation)unloadedPending[1];
			if (dispatcherOperation.Status == DispatcherOperationStatus.Pending)
			{
				dispatcherOperation.Abort();
			}
			MediaContext.From(d.Dispatcher).RemoveLoadedOrUnloadedCallback((LoadedOrUnloadedOperation)unloadedPending[0]);
		}
	}

	internal static void BroadcastLoadedOrUnloadedEvent(DependencyObject d, DependencyObject oldParent, DependencyObject newParent)
	{
		if (oldParent == null && newParent != null)
		{
			if (IsLoadedHelper(newParent))
			{
				FireLoadedOnDescendentsHelper(d);
			}
		}
		else if (oldParent != null && newParent == null && IsLoadedHelper(oldParent))
		{
			FireUnloadedOnDescendentsHelper(d);
		}
	}

	internal static object BroadcastLoadedEvent(object root)
	{
		DependencyObject obj = (DependencyObject)root;
		object[] loadedPending = (object[])obj.GetValue(FrameworkElement.LoadedPendingProperty);
		bool isLoaded = IsLoadedHelper(obj);
		RemoveLoadedCallback(obj, loadedPending);
		BroadcastLoadedSynchronously(obj, isLoaded);
		return null;
	}

	internal static void BroadcastLoadedSynchronously(DependencyObject rootDO, bool isLoaded)
	{
		if (!isLoaded)
		{
			BroadcastEvent(rootDO, FrameworkElement.LoadedEvent);
		}
	}

	internal static object BroadcastUnloadedEvent(object root)
	{
		DependencyObject obj = (DependencyObject)root;
		object[] unloadedPending = (object[])obj.GetValue(FrameworkElement.UnloadedPendingProperty);
		bool isLoaded = IsLoadedHelper(obj);
		RemoveUnloadedCallback(obj, unloadedPending);
		BroadcastUnloadedSynchronously(obj, isLoaded);
		return null;
	}

	internal static void BroadcastUnloadedSynchronously(DependencyObject rootDO, bool isLoaded)
	{
		if (isLoaded)
		{
			BroadcastEvent(rootDO, FrameworkElement.UnloadedEvent);
		}
	}

	private static void BroadcastEvent(DependencyObject root, RoutedEvent routedEvent)
	{
		List<DependencyObject> list = new List<DependencyObject>();
		new DescendentsWalker<BroadcastEventData>(TreeWalkPriority.VisualTree, BroadcastDelegate, new BroadcastEventData(root, routedEvent, list)).StartWalk(root);
		for (int i = 0; i < list.Count; i++)
		{
			DependencyObject dependencyObject = list[i];
			RoutedEventArgs args = new RoutedEventArgs(routedEvent, dependencyObject);
			FrameworkObject frameworkObject = new FrameworkObject(dependencyObject, throwIfNeither: true);
			if (routedEvent == FrameworkElement.LoadedEvent)
			{
				frameworkObject.OnLoaded(args);
			}
			else
			{
				frameworkObject.OnUnloaded(args);
			}
		}
	}

	private static bool OnBroadcastCallback(DependencyObject d, BroadcastEventData data, bool visitedViaVisualTree)
	{
		DependencyObject root = data.Root;
		RoutedEvent routedEvent = data.RoutedEvent;
		List<DependencyObject> eventRoute = data.EventRoute;
		if (d is FrameworkElement frameworkElement)
		{
			if (frameworkElement != root && routedEvent == FrameworkElement.LoadedEvent && frameworkElement.UnloadedPending != null)
			{
				frameworkElement.FireLoadedOnDescendentsInternal();
			}
			else if (frameworkElement != root && routedEvent == FrameworkElement.UnloadedEvent && frameworkElement.LoadedPending != null)
			{
				RemoveLoadedCallback(frameworkElement, frameworkElement.LoadedPending);
			}
			else
			{
				if (frameworkElement != root)
				{
					if (routedEvent == FrameworkElement.LoadedEvent && frameworkElement.LoadedPending != null)
					{
						RemoveLoadedCallback(frameworkElement, frameworkElement.LoadedPending);
					}
					else if (routedEvent == FrameworkElement.UnloadedEvent && frameworkElement.UnloadedPending != null)
					{
						RemoveUnloadedCallback(frameworkElement, frameworkElement.UnloadedPending);
					}
				}
				if (frameworkElement.SubtreeHasLoadedChangeHandler)
				{
					frameworkElement.IsLoadedCache = routedEvent == FrameworkElement.LoadedEvent;
					eventRoute.Add(frameworkElement);
					return true;
				}
			}
		}
		else
		{
			FrameworkContentElement frameworkContentElement = (FrameworkContentElement)d;
			if (frameworkContentElement != root && routedEvent == FrameworkElement.LoadedEvent && frameworkContentElement.UnloadedPending != null)
			{
				frameworkContentElement.FireLoadedOnDescendentsInternal();
			}
			else if (frameworkContentElement != root && routedEvent == FrameworkElement.UnloadedEvent && frameworkContentElement.LoadedPending != null)
			{
				RemoveLoadedCallback(frameworkContentElement, frameworkContentElement.LoadedPending);
			}
			else
			{
				if (frameworkContentElement != root)
				{
					if (routedEvent == FrameworkElement.LoadedEvent && frameworkContentElement.LoadedPending != null)
					{
						RemoveLoadedCallback(frameworkContentElement, frameworkContentElement.LoadedPending);
					}
					else if (routedEvent == FrameworkElement.UnloadedEvent && frameworkContentElement.UnloadedPending != null)
					{
						RemoveUnloadedCallback(frameworkContentElement, frameworkContentElement.UnloadedPending);
					}
				}
				if (frameworkContentElement.SubtreeHasLoadedChangeHandler)
				{
					frameworkContentElement.IsLoadedCache = routedEvent == FrameworkElement.LoadedEvent;
					eventRoute.Add(frameworkContentElement);
					return true;
				}
			}
		}
		return false;
	}

	private static bool SubtreeHasLoadedChangeHandlerHelper(DependencyObject d)
	{
		if (d is FrameworkElement frameworkElement)
		{
			return frameworkElement.SubtreeHasLoadedChangeHandler;
		}
		if (d is FrameworkContentElement frameworkContentElement)
		{
			return frameworkContentElement.SubtreeHasLoadedChangeHandler;
		}
		return false;
	}

	private static void FireLoadedOnDescendentsHelper(DependencyObject d)
	{
		if (d is FrameworkElement frameworkElement)
		{
			frameworkElement.FireLoadedOnDescendentsInternal();
		}
		else
		{
			((FrameworkContentElement)d).FireLoadedOnDescendentsInternal();
		}
	}

	private static void FireUnloadedOnDescendentsHelper(DependencyObject d)
	{
		if (d is FrameworkElement frameworkElement)
		{
			frameworkElement.FireUnloadedOnDescendentsInternal();
		}
		else
		{
			((FrameworkContentElement)d).FireUnloadedOnDescendentsInternal();
		}
	}

	private static bool IsLoadedHelper(DependencyObject d)
	{
		return new FrameworkObject(d).IsLoaded;
	}

	internal static bool IsParentLoaded(DependencyObject d)
	{
		DependencyObject effectiveParent = new FrameworkObject(d).EffectiveParent;
		if (effectiveParent != null)
		{
			return IsLoadedHelper(effectiveParent);
		}
		if (d is Visual visual)
		{
			return SafeSecurityHelper.IsConnectedToPresentationSource(visual);
		}
		if (d is Visual3D reference)
		{
			Visual containingVisual2D = VisualTreeHelper.GetContainingVisual2D(reference);
			if (containingVisual2D != null)
			{
				return SafeSecurityHelper.IsConnectedToPresentationSource(containingVisual2D);
			}
			return false;
		}
		return false;
	}

	internal static FrameworkElementFactory GetFEFTreeRoot(DependencyObject templatedParent)
	{
		return new FrameworkObject(templatedParent, throwIfNeither: true).FE.TemplateInternal.VisualTree;
	}

	internal static void AddOrRemoveHasLoadedChangeHandlerFlag(DependencyObject d, DependencyObject oldParent, DependencyObject newParent)
	{
		if (SubtreeHasLoadedChangeHandlerHelper(d))
		{
			if (oldParent == null && newParent != null)
			{
				AddHasLoadedChangeHandlerFlagInAncestry(newParent);
			}
			else if (oldParent != null && newParent == null)
			{
				RemoveHasLoadedChangeHandlerFlagInAncestry(oldParent);
			}
		}
	}

	internal static void AddHasLoadedChangeHandlerFlagInAncestry(DependencyObject d)
	{
		UpdateHasLoadedChangeHandlerFlagInAncestry(d, addHandler: true);
	}

	internal static void RemoveHasLoadedChangeHandlerFlagInAncestry(DependencyObject d)
	{
		UpdateHasLoadedChangeHandlerFlagInAncestry(d, addHandler: false);
	}

	private static bool AreThereLoadedChangeHandlersInSubtree(ref FrameworkObject fo)
	{
		if (!fo.IsValid)
		{
			return false;
		}
		if (fo.ThisHasLoadedChangeEventHandler)
		{
			return true;
		}
		if (fo.IsFE)
		{
			Visual fE = fo.FE;
			int childrenCount = VisualTreeHelper.GetChildrenCount(fE);
			for (int i = 0; i < childrenCount; i++)
			{
				if (VisualTreeHelper.GetChild(fE, i) is FrameworkElement { SubtreeHasLoadedChangeHandler: not false })
				{
					return true;
				}
			}
		}
		foreach (object child in LogicalTreeHelper.GetChildren(fo.DO))
		{
			if (child is DependencyObject d && SubtreeHasLoadedChangeHandlerHelper(d))
			{
				return true;
			}
		}
		return false;
	}

	private static void UpdateHasLoadedChangeHandlerFlagInAncestry(DependencyObject d, bool addHandler)
	{
		FrameworkObject fo = new FrameworkObject(d);
		if (!addHandler && AreThereLoadedChangeHandlersInSubtree(ref fo))
		{
			return;
		}
		if (fo.IsValid)
		{
			if (fo.SubtreeHasLoadedChangeHandler == addHandler)
			{
				return;
			}
			DependencyObject dependencyObject = (fo.IsFE ? VisualTreeHelper.GetParent(fo.FE) : null);
			DependencyObject parent = fo.Parent;
			DependencyObject dependencyObject2 = null;
			fo.SubtreeHasLoadedChangeHandler = addHandler;
			if (dependencyObject != null)
			{
				UpdateHasLoadedChangeHandlerFlagInAncestry(dependencyObject, addHandler);
				dependencyObject2 = dependencyObject;
			}
			if (parent != null && parent != dependencyObject)
			{
				UpdateHasLoadedChangeHandlerFlagInAncestry(parent, addHandler);
				if (fo.IsFCE)
				{
					dependencyObject2 = parent;
				}
			}
			if (parent == null && dependencyObject == null)
			{
				dependencyObject2 = Helper.FindMentor(fo.DO.InheritanceContext);
				if (dependencyObject2 != null)
				{
					fo.ChangeSubtreeHasLoadedChangedHandler(dependencyObject2);
				}
			}
			if (addHandler)
			{
				if (fo.IsFE)
				{
					UpdateIsLoadedCache(fo.FE, dependencyObject2);
				}
				else
				{
					UpdateIsLoadedCache(fo.FCE, dependencyObject2);
				}
			}
		}
		else
		{
			DependencyObject dependencyObject3 = null;
			if (d is Visual reference)
			{
				dependencyObject3 = VisualTreeHelper.GetParent(reference);
			}
			else if (d is ContentElement reference2)
			{
				dependencyObject3 = ContentOperations.GetParent(reference2);
			}
			else if (d is Visual3D reference3)
			{
				dependencyObject3 = VisualTreeHelper.GetParent(reference3);
			}
			if (dependencyObject3 != null)
			{
				UpdateHasLoadedChangeHandlerFlagInAncestry(dependencyObject3, addHandler);
			}
		}
	}

	private static void UpdateIsLoadedCache(FrameworkElement fe, DependencyObject parent)
	{
		if (fe.GetValue(FrameworkElement.LoadedPendingProperty) == null)
		{
			if (parent != null)
			{
				fe.IsLoadedCache = IsLoadedHelper(parent);
			}
			else if (SafeSecurityHelper.IsConnectedToPresentationSource(fe))
			{
				fe.IsLoadedCache = true;
			}
			else
			{
				fe.IsLoadedCache = false;
			}
		}
		else
		{
			fe.IsLoadedCache = false;
		}
	}

	private static void UpdateIsLoadedCache(FrameworkContentElement fce, DependencyObject parent)
	{
		if (fce.GetValue(FrameworkElement.LoadedPendingProperty) == null)
		{
			fce.IsLoadedCache = IsLoadedHelper(parent);
		}
		else
		{
			fce.IsLoadedCache = false;
		}
	}
}
