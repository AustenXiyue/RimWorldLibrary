using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace MS.Win32.Penimc;

internal static class UnsafeNativeMethods
{
	private const uint ReleaseManagerExt = 4294958765u;

	private const int ReleaseTabletExt = -1;

	private const int GetWispTabletKey = -2;

	private const int GetWispManagerKey = -3;

	private const int LockTabletExt = -4;

	private const int GetWispContextKey = -1;

	[ThreadStatic]
	private static uint? _wispManagerKey;

	[ThreadStatic]
	private static bool _wispManagerLocked = false;

	[ThreadStatic]
	private static IPimcManager3 _pimcManagerThreadStatic;

	[ThreadStatic]
	private static nint _pimcActCtxCookie = IntPtr.Zero;

	internal static IPimcManager3 PimcManager
	{
		get
		{
			if (_pimcManagerThreadStatic == null)
			{
				_pimcManagerThreadStatic = CreatePimcManager();
			}
			return _pimcManagerThreadStatic;
		}
	}

	[DllImport("PenIMC_cor3.dll", CharSet = CharSet.Auto)]
	internal static extern nint RegisterDllForSxSCOM();

	internal static void EnsurePenImcClassesActivated()
	{
		if (_pimcActCtxCookie == IntPtr.Zero && (_pimcActCtxCookie = RegisterDllForSxSCOM()) == IntPtr.Zero)
		{
			throw new InvalidOperationException(SR.Format(SR.PenImcSxSRegistrationFailed, "PenIMC_cor3.dll"));
		}
	}

	internal static void DeactivatePenImcClasses()
	{
		if (_pimcActCtxCookie != IntPtr.Zero && DeactivateActCtx(0, _pimcActCtxCookie))
		{
			_pimcActCtxCookie = IntPtr.Zero;
		}
	}

	private static IPimcManager3 CreatePimcManager()
	{
		Guid clsid = Guid.Parse("DB88ADFD-BEC7-47B8-A6B5-58CA3DA2B8D6");
		Guid iid = Guid.Parse("BD2C38C2-E064-41D0-A999-940F526219C2");
		return (IPimcManager3)CoCreateInstance(ref clsid, null, 1, ref iid);
	}

	internal static void CheckedLockWispObjectFromGit(uint gitKey)
	{
		if (OSVersionHelper.IsOsWindows8OrGreater && !LockWispObjectFromGit(gitKey))
		{
			throw new InvalidOperationException();
		}
	}

	internal static void CheckedUnlockWispObjectFromGit(uint gitKey)
	{
		if (OSVersionHelper.IsOsWindows8OrGreater && !UnlockWispObjectFromGit(gitKey))
		{
			throw new InvalidOperationException();
		}
	}

	private static void ReleaseManagerExternalLockImpl(IPimcManager3 manager)
	{
		IPimcTablet3 IPimcTablet = null;
		manager.GetTablet(4294958765u, out IPimcTablet);
	}

	internal static void ReleaseManagerExternalLock()
	{
		if (_pimcManagerThreadStatic != null)
		{
			ReleaseManagerExternalLockImpl(_pimcManagerThreadStatic);
		}
	}

	internal static void SetWispManagerKey(IPimcTablet3 tablet)
	{
		uint num = QueryWispKeyFromTablet(-3, tablet);
		Invariant.Assert(!_wispManagerKey.HasValue || _wispManagerKey.Value == num);
		_wispManagerKey = num;
	}

	internal static void LockWispManager()
	{
		if (!_wispManagerLocked && _wispManagerKey.HasValue)
		{
			CheckedLockWispObjectFromGit(_wispManagerKey.Value);
			_wispManagerLocked = true;
		}
	}

	internal static void UnlockWispManager()
	{
		if (_wispManagerLocked && _wispManagerKey.HasValue)
		{
			CheckedUnlockWispObjectFromGit(_wispManagerKey.Value);
			_wispManagerLocked = false;
		}
	}

	internal static void AcquireTabletExternalLock(IPimcTablet3 tablet)
	{
		int cButtons = 0;
		tablet.GetCursorButtonCount(-4, out cButtons);
	}

	internal static void ReleaseTabletExternalLock(IPimcTablet3 tablet)
	{
		int cButtons = 0;
		tablet.GetCursorButtonCount(-1, out cButtons);
	}

	private static uint QueryWispKeyFromTablet(int keyType, IPimcTablet3 tablet)
	{
		int cButtons = 0;
		tablet.GetCursorButtonCount(keyType, out cButtons);
		if (cButtons == 0)
		{
			throw new InvalidOperationException();
		}
		return (uint)cButtons;
	}

	internal static uint QueryWispTabletKey(IPimcTablet3 tablet)
	{
		return QueryWispKeyFromTablet(-2, tablet);
	}

	internal static uint QueryWispContextKey(IPimcContext3 context)
	{
		int iMin = 0;
		Guid guid = Guid.Empty;
		int iMax = 0;
		int iUnits = 0;
		float flResolution = 0f;
		context.GetPacketPropertyInfo(-1, out guid, out iMin, out iMax, out iUnits, out flResolution);
		if (iMin == 0)
		{
			throw new InvalidOperationException();
		}
		return (uint)iMin;
	}

	[DllImport("PenIMC_cor3.dll", CharSet = CharSet.Auto)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool GetPenEvent(nint commHandle, nint handleReset, out int evt, out int stylusPointerId, out int cPackets, out int cbPacket, out nint pPackets);

	[DllImport("PenIMC_cor3.dll", CharSet = CharSet.Auto)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool GetPenEventMultiple(int cCommHandles, nint[] commHandles, nint handleReset, out int iHandle, out int evt, out int stylusPointerId, out int cPackets, out int cbPacket, out nint pPackets);

	[DllImport("PenIMC_cor3.dll", CharSet = CharSet.Auto)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool GetLastSystemEventData(nint commHandle, out int evt, out int modifier, out int key, out int x, out int y, out int cursorMode, out int buttonState);

	[DllImport("PenIMC_cor3.dll", CharSet = CharSet.Auto)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool CreateResetEvent(out nint handle);

	[DllImport("PenIMC_cor3.dll", CharSet = CharSet.Auto)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool DestroyResetEvent(nint handle);

	[DllImport("PenIMC_cor3.dll", CharSet = CharSet.Auto)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool RaiseResetEvent(nint handle);

	[DllImport("PenIMC_cor3.dll", CharSet = CharSet.Auto)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool LockWispObjectFromGit(uint gitKey);

	[DllImport("PenIMC_cor3.dll", CharSet = CharSet.Auto)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool UnlockWispObjectFromGit(uint gitKey);

	[DllImport("ole32.dll", ExactSpelling = true, PreserveSig = false)]
	[return: MarshalAs(UnmanagedType.Interface)]
	private static extern object CoCreateInstance([In] ref Guid clsid, [MarshalAs(UnmanagedType.Interface)] object punkOuter, int context, [In] ref Guid iid);

	[DllImport("kernel32.dll", ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool DeactivateActCtx(int flags, nint activationCtxCookie);
}
