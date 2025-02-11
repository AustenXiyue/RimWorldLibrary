using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.Win32.SafeHandles;
using MS.Utility;
using MS.Win32;

namespace MS.Internal;

internal static class DpiUtil
{
	private static class DpiAwarenessContextHelper
	{
		private static bool IsGetWindowDpiAwarenessContextMethodSupported { get; set; } = true;

		internal static DpiAwarenessContextHandle GetDpiAwarenessContext(nint hWnd)
		{
			if (IsGetWindowDpiAwarenessContextMethodSupported)
			{
				try
				{
					return GetWindowDpiAwarenessContext(hWnd);
				}
				catch (Exception ex) when (ex is EntryPointNotFoundException || ex is MissingMethodException || ex is DllNotFoundException)
				{
					IsGetWindowDpiAwarenessContextMethodSupported = false;
				}
			}
			return GetProcessDpiAwarenessContext(hWnd);
		}

		private static DpiAwarenessContextHandle GetProcessDpiAwarenessContext(nint hWnd)
		{
			return GetProcessDpiAwarenessContext(ProcessDpiAwarenessHelper.GetProcessDpiAwareness(hWnd));
		}

		internal static DpiAwarenessContextHandle GetProcessDpiAwarenessContext(MS.Win32.NativeMethods.PROCESS_DPI_AWARENESS dpiAwareness)
		{
			return dpiAwareness switch
			{
				MS.Win32.NativeMethods.PROCESS_DPI_AWARENESS.PROCESS_SYSTEM_DPI_AWARE => MS.Win32.NativeMethods.DPI_AWARENESS_CONTEXT_SYSTEM_AWARE, 
				MS.Win32.NativeMethods.PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE => MS.Win32.NativeMethods.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE, 
				_ => MS.Win32.NativeMethods.DPI_AWARENESS_CONTEXT_UNAWARE, 
			};
		}

		private static DpiAwarenessContextHandle GetWindowDpiAwarenessContext(nint hWnd)
		{
			return SafeNativeMethods.GetWindowDpiAwarenessContext(hWnd);
		}
	}

	internal class DpiAwarenessScope : IDisposable
	{
		private static bool OperationSupported { get; set; } = true;

		private bool IsThreadInMixedHostingBehavior => SafeNativeMethods.GetThreadDpiHostingBehavior() == MS.Win32.NativeMethods.DPI_HOSTING_BEHAVIOR.DPI_HOSTING_BEHAVIOR_MIXED;

		private DpiAwarenessContextHandle OldDpiAwarenessContext { get; set; }

		public DpiAwarenessScope(DpiAwarenessContextValue dpiAwarenessContextEnumValue)
			: this(dpiAwarenessContextEnumValue, updateIfThreadInMixedHostingMode: false, updateIfWindowIsSystemAwareOrUnaware: false, IntPtr.Zero)
		{
		}

		public DpiAwarenessScope(DpiAwarenessContextValue dpiAwarenessContextEnumValue, bool updateIfThreadInMixedHostingMode)
			: this(dpiAwarenessContextEnumValue, updateIfThreadInMixedHostingMode, updateIfWindowIsSystemAwareOrUnaware: false, IntPtr.Zero)
		{
		}

		public DpiAwarenessScope(DpiAwarenessContextValue dpiAwarenessContextEnumValue, bool updateIfThreadInMixedHostingMode, nint hWnd)
			: this(dpiAwarenessContextEnumValue, updateIfThreadInMixedHostingMode, updateIfWindowIsSystemAwareOrUnaware: true, hWnd)
		{
		}

		private DpiAwarenessScope(DpiAwarenessContextValue dpiAwarenessContextValue, bool updateIfThreadInMixedHostingMode, bool updateIfWindowIsSystemAwareOrUnaware, nint hWnd)
		{
			if (dpiAwarenessContextValue == DpiAwarenessContextValue.Invalid || !OperationSupported || (updateIfThreadInMixedHostingMode && !IsThreadInMixedHostingBehavior) || (updateIfWindowIsSystemAwareOrUnaware && (hWnd == IntPtr.Zero || !IsWindowUnawareOrSystemAware(hWnd))))
			{
				return;
			}
			try
			{
				OldDpiAwarenessContext = MS.Win32.UnsafeNativeMethods.SetThreadDpiAwarenessContext(new DpiAwarenessContextHandle(dpiAwarenessContextValue));
			}
			catch (Exception ex) when (ex is EntryPointNotFoundException || ex is MissingMethodException || ex is DllNotFoundException)
			{
				OperationSupported = false;
			}
		}

		public void Dispose()
		{
			if (OldDpiAwarenessContext != null)
			{
				MS.Win32.UnsafeNativeMethods.SetThreadDpiAwarenessContext(OldDpiAwarenessContext);
				OldDpiAwarenessContext = null;
			}
		}

		private bool IsWindowUnawareOrSystemAware(nint hWnd)
		{
			DpiAwarenessContextHandle dpiAwarenessContext = GetDpiAwarenessContext(hWnd);
			if (!dpiAwarenessContext.Equals(DpiAwarenessContextValue.Unaware))
			{
				return dpiAwarenessContext.Equals(DpiAwarenessContextValue.SystemAware);
			}
			return true;
		}
	}

	public class HwndDpiInfo : Tuple<DpiAwarenessContextValue, DpiScale2>
	{
		internal MS.Win32.NativeMethods.RECT ContainingMonitorScreenRect { get; }

		internal DpiAwarenessContextValue DpiAwarenessContextValue => base.Item1;

		internal DpiScale2 DpiScale => base.Item2;

		internal HwndDpiInfo(nint hWnd, bool fallbackToNearestMonitorHeuristic)
			: base((DpiAwarenessContextValue)GetDpiAwarenessContext(hWnd), GetWindowDpi(hWnd, fallbackToNearestMonitorHeuristic))
		{
			ContainingMonitorScreenRect = NearestMonitorInfoFromWindow(hWnd).rcMonitor;
		}

		internal HwndDpiInfo(DpiAwarenessContextValue dpiAwarenessContextValue, DpiScale2 dpiScale)
			: base(dpiAwarenessContextValue, dpiScale)
		{
			ContainingMonitorScreenRect = NearestMonitorInfoFromWindow(IntPtr.Zero).rcMonitor;
		}

		private static MS.Win32.NativeMethods.MONITORINFOEX NearestMonitorInfoFromWindow(nint hwnd)
		{
			nint num = SafeNativeMethods.MonitorFromWindow(new HandleRef(null, hwnd), 2);
			if (num == IntPtr.Zero)
			{
				throw new Win32Exception();
			}
			MS.Win32.NativeMethods.MONITORINFOEX mONITORINFOEX = new MS.Win32.NativeMethods.MONITORINFOEX();
			SafeNativeMethods.GetMonitorInfo(new HandleRef(null, num), mONITORINFOEX);
			return mONITORINFOEX;
		}
	}

	private static class ProcessDpiAwarenessHelper
	{
		private static bool IsGetProcessDpiAwarenessFunctionSupported { get; set; } = true;

		internal static MS.Win32.NativeMethods.PROCESS_DPI_AWARENESS GetLegacyProcessDpiAwareness()
		{
			if (!MS.Win32.UnsafeNativeMethods.IsProcessDPIAware())
			{
				return MS.Win32.NativeMethods.PROCESS_DPI_AWARENESS.PROCESS_DPI_UNAWARE;
			}
			return MS.Win32.NativeMethods.PROCESS_DPI_AWARENESS.PROCESS_SYSTEM_DPI_AWARE;
		}

		internal static MS.Win32.NativeMethods.PROCESS_DPI_AWARENESS GetProcessDpiAwareness(nint hWnd)
		{
			if (IsGetProcessDpiAwarenessFunctionSupported)
			{
				try
				{
					return GetProcessDpiAwarenessFromWindow(hWnd);
				}
				catch (Exception ex) when (ex is EntryPointNotFoundException || ex is MissingMethodException || ex is DllNotFoundException)
				{
					IsGetProcessDpiAwarenessFunctionSupported = false;
				}
				catch (Exception ex2) when (ex2 is ArgumentException || ex2 is UnauthorizedAccessException || ex2 is COMException)
				{
				}
			}
			return GetLegacyProcessDpiAwareness();
		}

		private static MS.Win32.NativeMethods.PROCESS_DPI_AWARENESS GetProcessDpiAwarenessFromWindow(nint hWnd)
		{
			int lpdwProcessId = 0;
			if (hWnd != IntPtr.Zero)
			{
				MS.Win32.UnsafeNativeMethods.GetWindowThreadProcessId(new HandleRef(null, hWnd), out lpdwProcessId);
			}
			else
			{
				lpdwProcessId = SafeNativeMethods.GetCurrentProcessId();
			}
			using SafeProcessHandle safeProcessHandle = new SafeProcessHandle(MS.Win32.UnsafeNativeMethods.OpenProcess(2035711, fInherit: false, lpdwProcessId), ownsHandle: true);
			return SafeNativeMethods.GetProcessDpiAwareness(new HandleRef(null, safeProcessHandle.DangerousGetHandle()));
		}
	}

	private static class SystemDpiHelper
	{
		private static bool IsGetDpiForSystemFunctionAvailable { get; set; } = true;

		internal static DpiScale2 GetSystemDpi()
		{
			if (IsGetDpiForSystemFunctionAvailable)
			{
				try
				{
					return GetDpiForSystem();
				}
				catch (Exception ex) when (ex is EntryPointNotFoundException || ex is MissingMethodException || ex is DllNotFoundException)
				{
					IsGetDpiForSystemFunctionAvailable = false;
				}
			}
			return GetSystemDpiFromDeviceCaps();
		}

		internal static DpiScale2 GetSystemDpiFromUIElementCache()
		{
			lock (UIElement.DpiLock)
			{
				return new DpiScale2(UIElement.DpiScaleXValues[0], UIElement.DpiScaleYValues[0]);
			}
		}

		internal static void UpdateUIElementCacheForSystemDpi(DpiScale2 systemDpiScale)
		{
			lock (UIElement.DpiLock)
			{
				UIElement.DpiScaleXValues.Insert(0, systemDpiScale.DpiScaleX);
				UIElement.DpiScaleYValues.Insert(0, systemDpiScale.DpiScaleY);
			}
		}

		private static DpiScale2 GetDpiForSystem()
		{
			uint dpiForSystem = SafeNativeMethods.GetDpiForSystem();
			return DpiScale2.FromPixelsPerInch(dpiForSystem, dpiForSystem);
		}

		private static DpiScale2 GetSystemDpiFromDeviceCaps()
		{
			HandleRef hWnd = new HandleRef(IntPtr.Zero, IntPtr.Zero);
			HandleRef hDC = new HandleRef(IntPtr.Zero, MS.Win32.UnsafeNativeMethods.GetDC(hWnd));
			if (hDC.Handle == IntPtr.Zero)
			{
				return null;
			}
			try
			{
				int deviceCaps = MS.Win32.UnsafeNativeMethods.GetDeviceCaps(hDC, 88);
				int deviceCaps2 = MS.Win32.UnsafeNativeMethods.GetDeviceCaps(hDC, 90);
				return DpiScale2.FromPixelsPerInch(deviceCaps, deviceCaps2);
			}
			finally
			{
				MS.Win32.UnsafeNativeMethods.ReleaseDC(hWnd, hDC);
			}
		}
	}

	private static class WindowDpiScaleHelper
	{
		private static bool IsGetDpiForWindowFunctionEnabled { get; set; } = true;

		internal static DpiScale2 GetWindowDpi(nint hWnd, bool fallbackToNearestMonitorHeuristic)
		{
			if (IsGetDpiForWindowFunctionEnabled)
			{
				try
				{
					return GetDpiForWindow(hWnd);
				}
				catch (Exception ex) when (ex is EntryPointNotFoundException || ex is MissingMethodException || ex is DllNotFoundException)
				{
					IsGetDpiForWindowFunctionEnabled = false;
				}
				catch (Exception ex2) when (ex2 is COMException)
				{
				}
			}
			if (fallbackToNearestMonitorHeuristic)
			{
				try
				{
					return GetDpiForWindowFromNearestMonitor(hWnd);
				}
				catch (Exception ex3) when (ex3 is COMException)
				{
				}
			}
			return null;
		}

		private static DpiScale2 GetDpiForWindow(nint hWnd)
		{
			uint dpiForWindow = SafeNativeMethods.GetDpiForWindow(new HandleRef(IntPtr.Zero, hWnd));
			return DpiScale2.FromPixelsPerInch(dpiForWindow, dpiForWindow);
		}

		private static DpiScale2 GetDpiForWindowFromNearestMonitor(nint hWnd)
		{
			nint handle = SafeNativeMethods.MonitorFromWindow(new HandleRef(IntPtr.Zero, hWnd), 2);
			Marshal.ThrowExceptionForHR((int)MS.Win32.UnsafeNativeMethods.GetDpiForMonitor(new HandleRef(IntPtr.Zero, handle), MS.Win32.NativeMethods.MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out var dpiX, out var dpiY));
			return DpiScale2.FromPixelsPerInch(dpiX, dpiY);
		}
	}

	internal const double DefaultPixelsPerInch = 96.0;

	internal static DpiAwarenessContextHandle GetDpiAwarenessContext(nint hWnd)
	{
		return DpiAwarenessContextHelper.GetDpiAwarenessContext(hWnd);
	}

	internal static MS.Win32.NativeMethods.PROCESS_DPI_AWARENESS GetProcessDpiAwareness(nint hWnd)
	{
		return ProcessDpiAwarenessHelper.GetProcessDpiAwareness(hWnd);
	}

	internal static DpiAwarenessContextValue GetProcessDpiAwarenessContextValue(nint hWnd)
	{
		return (DpiAwarenessContextValue)DpiAwarenessContextHelper.GetProcessDpiAwarenessContext(ProcessDpiAwarenessHelper.GetProcessDpiAwareness(hWnd));
	}

	internal static MS.Win32.NativeMethods.PROCESS_DPI_AWARENESS GetLegacyProcessDpiAwareness()
	{
		return ProcessDpiAwarenessHelper.GetLegacyProcessDpiAwareness();
	}

	internal static DpiScale2 GetSystemDpi()
	{
		return SystemDpiHelper.GetSystemDpi();
	}

	internal static DpiScale2 GetSystemDpiFromUIElementCache()
	{
		return SystemDpiHelper.GetSystemDpiFromUIElementCache();
	}

	internal static void UpdateUIElementCacheForSystemDpi(DpiScale2 systemDpiScale)
	{
		SystemDpiHelper.UpdateUIElementCacheForSystemDpi(systemDpiScale);
	}

	internal static DpiScale2 GetWindowDpi(nint hWnd, bool fallbackToNearestMonitorHeuristic)
	{
		return WindowDpiScaleHelper.GetWindowDpi(hWnd, fallbackToNearestMonitorHeuristic);
	}

	internal static HwndDpiInfo GetExtendedDpiInfoForWindow(nint hWnd, bool fallbackToNearestMonitorHeuristic)
	{
		return new HwndDpiInfo(hWnd, fallbackToNearestMonitorHeuristic);
	}

	internal static HwndDpiInfo GetExtendedDpiInfoForWindow(nint hWnd)
	{
		return GetExtendedDpiInfoForWindow(hWnd, fallbackToNearestMonitorHeuristic: true);
	}

	internal static IDisposable WithDpiAwarenessContext(DpiAwarenessContextValue dpiAwarenessContext)
	{
		return new DpiAwarenessScope(dpiAwarenessContext);
	}

	internal static DpiFlags UpdateDpiScalesAndGetIndex(double pixelsPerInchX, double pixelsPerInchY)
	{
		lock (UIElement.DpiLock)
		{
			int num = 0;
			int count = UIElement.DpiScaleXValues.Count;
			for (num = 0; num < count && (UIElement.DpiScaleXValues[num] != pixelsPerInchX / 96.0 || UIElement.DpiScaleYValues[num] != pixelsPerInchY / 96.0); num++)
			{
			}
			if (num == count)
			{
				UIElement.DpiScaleXValues.Add(pixelsPerInchX / 96.0);
				UIElement.DpiScaleYValues.Add(pixelsPerInchY / 96.0);
			}
			bool dpiScaleFlag;
			bool dpiScaleFlag2;
			if (num < 3)
			{
				dpiScaleFlag = (num & 1) != 0;
				dpiScaleFlag2 = (num & 2) != 0;
			}
			else
			{
				dpiScaleFlag = (dpiScaleFlag2 = true);
			}
			return new DpiFlags(dpiScaleFlag, dpiScaleFlag2, num);
		}
	}
}
