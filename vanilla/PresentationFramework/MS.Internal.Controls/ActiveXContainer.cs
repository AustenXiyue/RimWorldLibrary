using System.Windows.Interop;
using MS.Win32;

namespace MS.Internal.Controls;

internal class ActiveXContainer : MS.Win32.UnsafeNativeMethods.IOleContainer, MS.Win32.UnsafeNativeMethods.IOleInPlaceFrame
{
	private ActiveXHost _host;

	private ActiveXHost _siteUIActive;

	internal ActiveXHost ActiveXHost => _host;

	internal ActiveXContainer(ActiveXHost host)
	{
		_host = host;
		Invariant.Assert(_host != null);
	}

	int MS.Win32.UnsafeNativeMethods.IOleContainer.ParseDisplayName(object pbc, string pszDisplayName, int[] pchEaten, object[] ppmkOut)
	{
		if (ppmkOut != null)
		{
			ppmkOut[0] = null;
		}
		return -2147467263;
	}

	int MS.Win32.UnsafeNativeMethods.IOleContainer.EnumObjects(int grfFlags, out MS.Win32.UnsafeNativeMethods.IEnumUnknown ppenum)
	{
		ppenum = null;
		object activeXInstance = _host.ActiveXInstance;
		if (activeXInstance != null && ((grfFlags & 1) != 0 || ((grfFlags & 0x10) != 0 && _host.ActiveXState == ActiveXHelper.ActiveXState.Running)))
		{
			ppenum = new EnumUnknown(new object[1] { activeXInstance });
			return 0;
		}
		ppenum = new EnumUnknown(null);
		return 0;
	}

	int MS.Win32.UnsafeNativeMethods.IOleContainer.LockContainer(bool fLock)
	{
		return -2147467263;
	}

	nint MS.Win32.UnsafeNativeMethods.IOleInPlaceFrame.GetWindow()
	{
		return _host.ParentHandle.Handle;
	}

	int MS.Win32.UnsafeNativeMethods.IOleInPlaceFrame.ContextSensitiveHelp(int fEnterMode)
	{
		return 0;
	}

	int MS.Win32.UnsafeNativeMethods.IOleInPlaceFrame.GetBorder(MS.Win32.NativeMethods.COMRECT lprectBorder)
	{
		return -2147467263;
	}

	int MS.Win32.UnsafeNativeMethods.IOleInPlaceFrame.RequestBorderSpace(MS.Win32.NativeMethods.COMRECT pborderwidths)
	{
		return -2147467263;
	}

	int MS.Win32.UnsafeNativeMethods.IOleInPlaceFrame.SetBorderSpace(MS.Win32.NativeMethods.COMRECT pborderwidths)
	{
		return -2147467263;
	}

	int MS.Win32.UnsafeNativeMethods.IOleInPlaceFrame.SetActiveObject(MS.Win32.UnsafeNativeMethods.IOleInPlaceActiveObject pActiveObject, string pszObjName)
	{
		return 0;
	}

	int MS.Win32.UnsafeNativeMethods.IOleInPlaceFrame.InsertMenus(nint hmenuShared, MS.Win32.NativeMethods.tagOleMenuGroupWidths lpMenuWidths)
	{
		return 0;
	}

	int MS.Win32.UnsafeNativeMethods.IOleInPlaceFrame.SetMenu(nint hmenuShared, nint holemenu, nint hwndActiveObject)
	{
		return -2147467263;
	}

	int MS.Win32.UnsafeNativeMethods.IOleInPlaceFrame.RemoveMenus(nint hmenuShared)
	{
		return -2147467263;
	}

	int MS.Win32.UnsafeNativeMethods.IOleInPlaceFrame.SetStatusText(string pszStatusText)
	{
		return -2147467263;
	}

	int MS.Win32.UnsafeNativeMethods.IOleInPlaceFrame.EnableModeless(bool fEnable)
	{
		return -2147467263;
	}

	int MS.Win32.UnsafeNativeMethods.IOleInPlaceFrame.TranslateAccelerator(ref MSG lpmsg, short wID)
	{
		return 1;
	}

	internal void OnUIActivate(ActiveXHost site)
	{
		if (_siteUIActive != site)
		{
			if (_siteUIActive != null)
			{
				_siteUIActive.ActiveXInPlaceObject.UIDeactivate();
			}
			_siteUIActive = site;
		}
	}

	internal void OnUIDeactivate(ActiveXHost site)
	{
		_siteUIActive = null;
	}

	internal void OnInPlaceDeactivate(ActiveXHost site)
	{
		_ = ActiveXHost;
	}
}
