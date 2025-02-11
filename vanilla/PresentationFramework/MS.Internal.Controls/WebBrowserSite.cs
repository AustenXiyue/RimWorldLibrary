using System;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using MS.Win32;

namespace MS.Internal.Controls;

internal class WebBrowserSite : ActiveXSite, MS.Win32.UnsafeNativeMethods.IDocHostUIHandler, MS.Win32.UnsafeNativeMethods.IOleControlSite
{
	internal WebBrowserSite(WebBrowser host)
		: base(host)
	{
	}

	int MS.Win32.UnsafeNativeMethods.IDocHostUIHandler.ShowContextMenu(int dwID, ref MS.Win32.NativeMethods.POINT pt, object pcmdtReserved, object pdispReserved)
	{
		return 1;
	}

	int MS.Win32.UnsafeNativeMethods.IDocHostUIHandler.GetHostInfo(MS.Win32.NativeMethods.DOCHOSTUIINFO info)
	{
		_ = (WebBrowser)base.Host;
		info.dwDoubleClick = 0;
		info.dwFlags = 94846994;
		return 0;
	}

	int MS.Win32.UnsafeNativeMethods.IDocHostUIHandler.EnableModeless(bool fEnable)
	{
		return -2147467263;
	}

	int MS.Win32.UnsafeNativeMethods.IDocHostUIHandler.ShowUI(int dwID, MS.Win32.UnsafeNativeMethods.IOleInPlaceActiveObject activeObject, MS.Win32.NativeMethods.IOleCommandTarget commandTarget, MS.Win32.UnsafeNativeMethods.IOleInPlaceFrame frame, MS.Win32.UnsafeNativeMethods.IOleInPlaceUIWindow doc)
	{
		return -2147467263;
	}

	int MS.Win32.UnsafeNativeMethods.IDocHostUIHandler.HideUI()
	{
		return -2147467263;
	}

	int MS.Win32.UnsafeNativeMethods.IDocHostUIHandler.UpdateUI()
	{
		return -2147467263;
	}

	int MS.Win32.UnsafeNativeMethods.IDocHostUIHandler.OnDocWindowActivate(bool fActivate)
	{
		return -2147467263;
	}

	int MS.Win32.UnsafeNativeMethods.IDocHostUIHandler.OnFrameWindowActivate(bool fActivate)
	{
		return -2147467263;
	}

	int MS.Win32.UnsafeNativeMethods.IDocHostUIHandler.ResizeBorder(MS.Win32.NativeMethods.COMRECT rect, MS.Win32.UnsafeNativeMethods.IOleInPlaceUIWindow doc, bool fFrameWindow)
	{
		return -2147467263;
	}

	int MS.Win32.UnsafeNativeMethods.IDocHostUIHandler.GetOptionKeyPath(string[] pbstrKey, int dw)
	{
		return -2147467263;
	}

	int MS.Win32.UnsafeNativeMethods.IDocHostUIHandler.GetDropTarget(MS.Win32.UnsafeNativeMethods.IOleDropTarget pDropTarget, out MS.Win32.UnsafeNativeMethods.IOleDropTarget ppDropTarget)
	{
		ppDropTarget = null;
		return -2147467263;
	}

	int MS.Win32.UnsafeNativeMethods.IDocHostUIHandler.GetExternal(out object ppDispatch)
	{
		WebBrowser webBrowser = (WebBrowser)base.Host;
		ppDispatch = webBrowser.HostingAdaptor.ObjectForScripting;
		return 0;
	}

	int MS.Win32.UnsafeNativeMethods.IDocHostUIHandler.TranslateAccelerator(ref MSG msg, ref Guid group, int nCmdID)
	{
		return 1;
	}

	int MS.Win32.UnsafeNativeMethods.IDocHostUIHandler.TranslateUrl(int dwTranslate, string strUrlIn, out string pstrUrlOut)
	{
		pstrUrlOut = null;
		return -2147467263;
	}

	int MS.Win32.UnsafeNativeMethods.IDocHostUIHandler.FilterDataObject(IDataObject pDO, out IDataObject ppDORet)
	{
		ppDORet = null;
		return -2147467263;
	}

	int MS.Win32.UnsafeNativeMethods.IOleControlSite.TranslateAccelerator(ref MSG msg, int grfModifiers)
	{
		if (msg.message == 256 && (int)msg.wParam == 9)
		{
			FocusNavigationDirection focusNavigationDirection = (((grfModifiers & 1) != 0) ? FocusNavigationDirection.Previous : FocusNavigationDirection.Next);
			base.Host.Dispatcher.Invoke(DispatcherPriority.Send, new SendOrPostCallback(MoveFocusCallback), focusNavigationDirection);
			return 0;
		}
		return 1;
	}

	private void MoveFocusCallback(object direction)
	{
		base.Host.MoveFocus(new TraversalRequest((FocusNavigationDirection)direction));
	}
}
