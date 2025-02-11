using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal.Media;
using MS.Internal.PresentationCore;
using MS.Win32;

namespace MS.Internal.Automation;

internal class ElementUtil
{
	private ElementUtil()
	{
	}

	internal static Visual GetParent(Visual el)
	{
		return VisualTreeHelper.GetParent(el) as Visual;
	}

	internal static Visual GetFirstChild(Visual el)
	{
		if (el == null)
		{
			return null;
		}
		return FindVisibleSibling(el, 0, searchForwards: true);
	}

	internal static Visual GetLastChild(Visual el)
	{
		if (el == null)
		{
			return null;
		}
		return FindVisibleSibling(el, el.InternalVisualChildrenCount - 1, searchForwards: false);
	}

	internal static Visual GetNextSibling(Visual el)
	{
		if (!(VisualTreeHelper.GetParent(el) is Visual parent))
		{
			return null;
		}
		return FindVisibleSibling(parent, el, searchForwards: true);
	}

	internal static Visual GetPreviousSibling(Visual el)
	{
		if (!(VisualTreeHelper.GetParent(el) is Visual parent))
		{
			return null;
		}
		return FindVisibleSibling(parent, el, searchForwards: false);
	}

	internal static Visual GetRoot(Visual el)
	{
		Visual visual = el;
		while (VisualTreeHelper.GetParent(visual) is Visual visual2)
		{
			visual = visual2;
		}
		return visual;
	}

	internal static Rect GetLocalRect(UIElement element)
	{
		Visual root = GetRoot(element);
		double height = element.RenderSize.Height;
		double width = element.RenderSize.Width;
		Rect rect = new Rect(0.0, 0.0, width, height);
		return element.TransformToAncestor(root).TransformBounds(rect);
	}

	internal static Rect GetScreenRect(nint hwnd, UIElement el)
	{
		Rect localRect = GetLocalRect(el);
		MS.Win32.NativeMethods.RECT rcWindowCoords = new MS.Win32.NativeMethods.RECT((int)localRect.Left, (int)localRect.Top, (int)localRect.Right, (int)localRect.Bottom);
		try
		{
			SafeSecurityHelper.TransformLocalRectToScreen(new HandleRef(null, hwnd), ref rcWindowCoords);
		}
		catch (Win32Exception)
		{
			return Rect.Empty;
		}
		return new Rect(rcWindowCoords.left, rcWindowCoords.top, rcWindowCoords.right - rcWindowCoords.left, rcWindowCoords.bottom - rcWindowCoords.top);
	}

	internal static Visual GetElementFromPoint(nint hwnd, Visual root, Point pointScreen)
	{
		HwndSource hwndSource = HwndSource.CriticalFromHwnd(hwnd);
		if (hwndSource == null)
		{
			return null;
		}
		Point point = PointUtil.ClientToRoot(PointUtil.ScreenToClient(pointScreen, hwndSource), hwndSource);
		return VisualTreeUtils.AsNearestPointHitTestResult(VisualTreeHelper.HitTest(root, point))?.VisualHit;
	}

	internal static void CheckEnabled(Visual visual)
	{
		if (visual is UIElement { IsEnabled: false })
		{
			throw new ElementNotEnabledException();
		}
	}

	internal static object Invoke(AutomationPeer peer, DispatcherOperationCallback work, object arg)
	{
		Dispatcher dispatcher = peer.Dispatcher;
		if (dispatcher == null)
		{
			throw new ElementNotAvailableException();
		}
		Exception remoteException = null;
		bool completed = false;
		object result = dispatcher.Invoke(DispatcherPriority.Send, TimeSpan.FromMinutes(3.0), (DispatcherOperationCallback)delegate
		{
			try
			{
				return work(arg);
			}
			catch (Exception ex)
			{
				remoteException = ex;
				return (object)null;
			}
			catch
			{
				remoteException = null;
				return (object)null;
			}
			finally
			{
				completed = true;
			}
		}, null);
		if (completed)
		{
			if (remoteException != null)
			{
				throw remoteException;
			}
			return result;
		}
		if (dispatcher.HasShutdownStarted)
		{
			throw new InvalidOperationException(SR.AutomationDispatcherShutdown);
		}
		throw new TimeoutException(SR.AutomationTimeout);
	}

	private static Visual FindVisibleSibling(Visual parent, int start, bool searchForwards)
	{
		int i = start;
		for (int internalVisualChildrenCount = parent.InternalVisualChildrenCount; i >= 0 && i < internalVisualChildrenCount; i += (searchForwards ? 1 : (-1)))
		{
			Visual visual = parent.InternalGetVisualChild(i);
			if (!(visual is UIElement) || ((UIElement)visual).Visibility == Visibility.Visible)
			{
				return visual;
			}
		}
		return null;
	}

	private static Visual FindVisibleSibling(Visual parent, Visual child, bool searchForwards)
	{
		int internalVisualChildrenCount = parent.InternalVisualChildrenCount;
		int i;
		for (i = 0; i < internalVisualChildrenCount && parent.InternalGetVisualChild(i) != child; i++)
		{
		}
		if (searchForwards)
		{
			return FindVisibleSibling(parent, i + 1, searchForwards);
		}
		return FindVisibleSibling(parent, i - 1, searchForwards);
	}
}
