using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MS.Internal.PresentationCore;

namespace MS.Internal;

internal static class UIElementHelper
{
	[FriendAccessAllowed]
	internal static bool IsHitTestVisible(DependencyObject o)
	{
		if (o is UIElement uIElement)
		{
			return uIElement.IsHitTestVisible;
		}
		return ((UIElement3D)o).IsHitTestVisible;
	}

	[FriendAccessAllowed]
	internal static bool IsVisible(DependencyObject o)
	{
		if (o is UIElement uIElement)
		{
			return uIElement.IsVisible;
		}
		return ((UIElement3D)o).IsVisible;
	}

	[FriendAccessAllowed]
	internal static DependencyObject PredictFocus(DependencyObject o, FocusNavigationDirection direction)
	{
		if (o is UIElement uIElement)
		{
			return uIElement.PredictFocus(direction);
		}
		if (o is ContentElement contentElement)
		{
			return contentElement.PredictFocus(direction);
		}
		if (o is UIElement3D uIElement3D)
		{
			return uIElement3D.PredictFocus(direction);
		}
		return null;
	}

	[FriendAccessAllowed]
	internal static UIElement GetContainingUIElement2D(DependencyObject reference)
	{
		UIElement uIElement = null;
		while (reference != null)
		{
			uIElement = reference as UIElement;
			if (uIElement != null)
			{
				break;
			}
			reference = VisualTreeHelper.GetParent(reference);
		}
		return uIElement;
	}

	[FriendAccessAllowed]
	internal static DependencyObject GetUIParent(DependencyObject child)
	{
		return GetUIParent(child, continuePastVisualTree: false);
	}

	[FriendAccessAllowed]
	internal static DependencyObject GetUIParent(DependencyObject child, bool continuePastVisualTree)
	{
		DependencyObject dependencyObject = null;
		DependencyObject dependencyObject2 = null;
		dependencyObject2 = ((!(child is Visual)) ? ((Visual3D)child).InternalVisualParent : ((Visual)child).InternalVisualParent);
		dependencyObject = InputElement.GetContainingUIElement(dependencyObject2);
		if (dependencyObject == null && continuePastVisualTree)
		{
			if (child is UIElement uIElement)
			{
				dependencyObject = InputElement.GetContainingInputElement(uIElement.GetUIParentCore()) as DependencyObject;
			}
			else if (child is UIElement3D uIElement3D)
			{
				dependencyObject = InputElement.GetContainingInputElement(uIElement3D.GetUIParentCore()) as DependencyObject;
			}
		}
		return dependencyObject;
	}

	[FriendAccessAllowed]
	internal static bool IsUIElementOrUIElement3D(DependencyObject o)
	{
		if (o is UIElement || o is UIElement3D)
		{
			return true;
		}
		return false;
	}

	[FriendAccessAllowed]
	internal static void InvalidateAutomationAncestors(DependencyObject o)
	{
		UIElement e = null;
		UIElement3D e3d = null;
		ContentElement ce = null;
		Stack<DependencyObject> branchNodeStack = new Stack<DependencyObject>();
		bool flag = true;
		while (o != null && flag)
		{
			flag &= InvalidateAutomationPeer(o, out e, out ce, out e3d);
			bool continuePastVisualTree = false;
			if (e != null)
			{
				flag &= e.InvalidateAutomationAncestorsCore(branchNodeStack, out continuePastVisualTree);
				o = e.GetUIParent(continuePastVisualTree);
			}
			else if (ce != null)
			{
				flag &= ce.InvalidateAutomationAncestorsCore(branchNodeStack, out continuePastVisualTree);
				o = ce.GetUIParent(continuePastVisualTree);
			}
			else if (e3d != null)
			{
				flag &= e3d.InvalidateAutomationAncestorsCore(branchNodeStack, out continuePastVisualTree);
				o = e3d.GetUIParent(continuePastVisualTree);
			}
		}
	}

	internal static bool InvalidateAutomationPeer(DependencyObject o, out UIElement e, out ContentElement ce, out UIElement3D e3d)
	{
		e = null;
		ce = null;
		e3d = null;
		AutomationPeer automationPeer = null;
		e = o as UIElement;
		if (e != null)
		{
			if (e.HasAutomationPeer)
			{
				automationPeer = e.GetAutomationPeer();
			}
		}
		else
		{
			ce = o as ContentElement;
			if (ce != null)
			{
				if (ce.HasAutomationPeer)
				{
					automationPeer = ce.GetAutomationPeer();
				}
			}
			else
			{
				e3d = o as UIElement3D;
				if (e3d != null && e3d.HasAutomationPeer)
				{
					automationPeer = e3d.GetAutomationPeer();
				}
			}
		}
		if (automationPeer != null)
		{
			automationPeer.InvalidateAncestorsRecursive();
			if (automationPeer.GetParent() != null)
			{
				return false;
			}
		}
		return true;
	}
}
