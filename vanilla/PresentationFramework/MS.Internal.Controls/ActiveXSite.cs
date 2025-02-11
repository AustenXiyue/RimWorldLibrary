using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using MS.Win32;

namespace MS.Internal.Controls;

internal class ActiveXSite : MS.Win32.UnsafeNativeMethods.IOleControlSite, MS.Win32.UnsafeNativeMethods.IOleClientSite, MS.Win32.UnsafeNativeMethods.IOleInPlaceSite, MS.Win32.UnsafeNativeMethods.IPropertyNotifySink
{
	private ActiveXHost _host;

	private ConnectionPointCookie _connectionPoint;

	private ActiveXHelper.ActiveXState HostState
	{
		get
		{
			return Host.ActiveXState;
		}
		set
		{
			Host.ActiveXState = value;
		}
	}

	internal MS.Win32.NativeMethods.COMRECT HostBounds => Host.Bounds;

	internal ActiveXHost Host => _host;

	internal ActiveXSite(ActiveXHost host)
	{
		if (host == null)
		{
			throw new ArgumentNullException("host");
		}
		_host = host;
	}

	int MS.Win32.UnsafeNativeMethods.IOleControlSite.OnControlInfoChanged()
	{
		return 0;
	}

	int MS.Win32.UnsafeNativeMethods.IOleControlSite.LockInPlaceActive(int fLock)
	{
		return -2147467263;
	}

	int MS.Win32.UnsafeNativeMethods.IOleControlSite.GetExtendedControl(out object ppDisp)
	{
		ppDisp = null;
		return -2147467263;
	}

	int MS.Win32.UnsafeNativeMethods.IOleControlSite.TransformCoords(ref MS.Win32.NativeMethods.POINT pPtlHimetric, ref MS.Win32.NativeMethods.POINTF pPtfContainer, int dwFlags)
	{
		if ((dwFlags & 4) != 0)
		{
			if ((dwFlags & 2) != 0)
			{
				pPtfContainer.x = ActiveXHelper.HM2Pix(pPtlHimetric.x, ActiveXHelper.LogPixelsX);
				pPtfContainer.y = ActiveXHelper.HM2Pix(pPtlHimetric.y, ActiveXHelper.LogPixelsY);
			}
			else
			{
				if ((dwFlags & 1) == 0)
				{
					return -2147024809;
				}
				pPtfContainer.x = ActiveXHelper.HM2Pix(pPtlHimetric.x, ActiveXHelper.LogPixelsX);
				pPtfContainer.y = ActiveXHelper.HM2Pix(pPtlHimetric.y, ActiveXHelper.LogPixelsY);
			}
		}
		else
		{
			if ((dwFlags & 8) == 0)
			{
				return -2147024809;
			}
			if ((dwFlags & 2) != 0)
			{
				pPtlHimetric.x = ActiveXHelper.Pix2HM((int)pPtfContainer.x, ActiveXHelper.LogPixelsX);
				pPtlHimetric.y = ActiveXHelper.Pix2HM((int)pPtfContainer.y, ActiveXHelper.LogPixelsY);
			}
			else
			{
				if ((dwFlags & 1) == 0)
				{
					return -2147024809;
				}
				pPtlHimetric.x = ActiveXHelper.Pix2HM((int)pPtfContainer.x, ActiveXHelper.LogPixelsX);
				pPtlHimetric.y = ActiveXHelper.Pix2HM((int)pPtfContainer.y, ActiveXHelper.LogPixelsY);
			}
		}
		return 0;
	}

	int MS.Win32.UnsafeNativeMethods.IOleControlSite.TranslateAccelerator(ref MSG pMsg, int grfModifiers)
	{
		return 1;
	}

	int MS.Win32.UnsafeNativeMethods.IOleControlSite.OnFocus(int fGotFocus)
	{
		return 0;
	}

	int MS.Win32.UnsafeNativeMethods.IOleControlSite.ShowPropertyFrame()
	{
		return -2147467263;
	}

	int MS.Win32.UnsafeNativeMethods.IOleClientSite.SaveObject()
	{
		return -2147467263;
	}

	int MS.Win32.UnsafeNativeMethods.IOleClientSite.GetMoniker(int dwAssign, int dwWhichMoniker, out object moniker)
	{
		moniker = null;
		return -2147467263;
	}

	int MS.Win32.UnsafeNativeMethods.IOleClientSite.GetContainer(out MS.Win32.UnsafeNativeMethods.IOleContainer container)
	{
		container = Host.Container;
		return 0;
	}

	int MS.Win32.UnsafeNativeMethods.IOleClientSite.ShowObject()
	{
		if (HostState >= ActiveXHelper.ActiveXState.InPlaceActive)
		{
			if (MS.Win32.NativeMethods.Succeeded(Host.ActiveXInPlaceObject.GetWindow(out var hwnd)))
			{
				if (Host.ControlHandle.Handle != hwnd && hwnd != IntPtr.Zero)
				{
					Host.AttachWindow(hwnd);
					OnActiveXRectChange(Host.Bounds);
				}
			}
			else if (Host.ActiveXInPlaceObject is MS.Win32.UnsafeNativeMethods.IOleInPlaceObjectWindowless)
			{
				throw new InvalidOperationException(SR.AxWindowlessControl);
			}
		}
		return 0;
	}

	int MS.Win32.UnsafeNativeMethods.IOleClientSite.OnShowWindow(int fShow)
	{
		return 0;
	}

	int MS.Win32.UnsafeNativeMethods.IOleClientSite.RequestNewObjectLayout()
	{
		return -2147467263;
	}

	nint MS.Win32.UnsafeNativeMethods.IOleInPlaceSite.GetWindow()
	{
		try
		{
			return Host.ParentHandle.Handle;
		}
		catch (Exception)
		{
			throw;
		}
	}

	int MS.Win32.UnsafeNativeMethods.IOleInPlaceSite.ContextSensitiveHelp(int fEnterMode)
	{
		return -2147467263;
	}

	int MS.Win32.UnsafeNativeMethods.IOleInPlaceSite.CanInPlaceActivate()
	{
		return 0;
	}

	int MS.Win32.UnsafeNativeMethods.IOleInPlaceSite.OnInPlaceActivate()
	{
		HostState = ActiveXHelper.ActiveXState.InPlaceActive;
		if (!HostBounds.IsEmpty)
		{
			OnActiveXRectChange(HostBounds);
		}
		return 0;
	}

	int MS.Win32.UnsafeNativeMethods.IOleInPlaceSite.OnUIActivate()
	{
		HostState = ActiveXHelper.ActiveXState.UIActive;
		Host.Container.OnUIActivate(Host);
		return 0;
	}

	int MS.Win32.UnsafeNativeMethods.IOleInPlaceSite.GetWindowContext(out MS.Win32.UnsafeNativeMethods.IOleInPlaceFrame ppFrame, out MS.Win32.UnsafeNativeMethods.IOleInPlaceUIWindow ppDoc, MS.Win32.NativeMethods.COMRECT lprcPosRect, MS.Win32.NativeMethods.COMRECT lprcClipRect, MS.Win32.NativeMethods.OLEINPLACEFRAMEINFO lpFrameInfo)
	{
		ppDoc = null;
		ppFrame = Host.Container;
		lprcPosRect.left = Host.Bounds.left;
		lprcPosRect.top = Host.Bounds.top;
		lprcPosRect.right = Host.Bounds.right;
		lprcPosRect.bottom = Host.Bounds.bottom;
		lprcClipRect = Host.Bounds;
		if (lpFrameInfo != null)
		{
			lpFrameInfo.cb = (uint)Marshal.SizeOf(typeof(MS.Win32.NativeMethods.OLEINPLACEFRAMEINFO));
			lpFrameInfo.fMDIApp = false;
			lpFrameInfo.hAccel = IntPtr.Zero;
			lpFrameInfo.cAccelEntries = 0u;
			lpFrameInfo.hwndFrame = Host.ParentHandle.Handle;
		}
		return 0;
	}

	int MS.Win32.UnsafeNativeMethods.IOleInPlaceSite.Scroll(MS.Win32.NativeMethods.SIZE scrollExtant)
	{
		return 1;
	}

	int MS.Win32.UnsafeNativeMethods.IOleInPlaceSite.OnUIDeactivate(int fUndoable)
	{
		Host.Container.OnUIDeactivate(Host);
		if (HostState > ActiveXHelper.ActiveXState.InPlaceActive)
		{
			HostState = ActiveXHelper.ActiveXState.InPlaceActive;
		}
		return 0;
	}

	int MS.Win32.UnsafeNativeMethods.IOleInPlaceSite.OnInPlaceDeactivate()
	{
		if (HostState == ActiveXHelper.ActiveXState.UIActive)
		{
			((MS.Win32.UnsafeNativeMethods.IOleInPlaceSite)this).OnUIDeactivate(0);
		}
		Host.Container.OnInPlaceDeactivate(Host);
		HostState = ActiveXHelper.ActiveXState.Running;
		return 0;
	}

	int MS.Win32.UnsafeNativeMethods.IOleInPlaceSite.DiscardUndoState()
	{
		return 0;
	}

	int MS.Win32.UnsafeNativeMethods.IOleInPlaceSite.DeactivateAndUndo()
	{
		return Host.ActiveXInPlaceObject.UIDeactivate();
	}

	int MS.Win32.UnsafeNativeMethods.IOleInPlaceSite.OnPosRectChange(MS.Win32.NativeMethods.COMRECT lprcPosRect)
	{
		return OnActiveXRectChange(lprcPosRect);
	}

	void MS.Win32.UnsafeNativeMethods.IPropertyNotifySink.OnChanged(int dispid)
	{
		try
		{
			OnPropertyChanged(dispid);
		}
		catch (Exception)
		{
			throw;
		}
	}

	int MS.Win32.UnsafeNativeMethods.IPropertyNotifySink.OnRequestEdit(int dispid)
	{
		return 0;
	}

	internal virtual void OnPropertyChanged(int dispid)
	{
	}

	internal void StartEvents()
	{
		if (_connectionPoint != null)
		{
			return;
		}
		object activeXInstance = Host.ActiveXInstance;
		if (activeXInstance == null)
		{
			return;
		}
		try
		{
			_connectionPoint = new ConnectionPointCookie(activeXInstance, this, typeof(MS.Win32.UnsafeNativeMethods.IPropertyNotifySink));
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
		}
	}

	internal void StopEvents()
	{
		if (_connectionPoint != null)
		{
			_connectionPoint.Disconnect();
			_connectionPoint = null;
		}
	}

	internal int OnActiveXRectChange(MS.Win32.NativeMethods.COMRECT lprcPosRect)
	{
		if (Host.ActiveXInPlaceObject != null)
		{
			Host.ActiveXInPlaceObject.SetObjectRects(lprcPosRect, lprcPosRect);
			Host.Bounds = lprcPosRect;
		}
		return 0;
	}
}
