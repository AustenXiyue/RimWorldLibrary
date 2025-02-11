using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MS.Win32;

namespace MS.Utility;

internal class DpiAwarenessContextHandle : SafeHandle, IEquatable<nint>, IEquatable<DpiAwarenessContextHandle>, IEquatable<DpiAwarenessContextValue>
{
	private static class DpiUtil
	{
		private static bool IsAreDpiAwarenessContextsEqualMethodSupported { get; set; } = true;

		internal static bool AreDpiAwarenessContextsEqual(nint dpiContextA, nint dpiContextB)
		{
			if (IsAreDpiAwarenessContextsEqualMethodSupported)
			{
				try
				{
					return SafeNativeMethods.AreDpiAwarenessContextsEqual(dpiContextA, dpiContextB);
				}
				catch (Exception ex) when (ex is EntryPointNotFoundException || ex is MissingMethodException || ex is DllNotFoundException)
				{
					IsAreDpiAwarenessContextsEqualMethodSupported = false;
				}
			}
			return AreDpiAwarenessContextsTriviallyEqual(dpiContextA, dpiContextB);
		}

		private static bool AreDpiAwarenessContextsTriviallyEqual(nint dpiContextA, nint dpiContextB)
		{
			return dpiContextA == dpiContextB;
		}
	}

	public override bool IsInvalid => true;

	internal static DpiAwarenessContextHandle DPI_AWARENESS_CONTEXT_UNAWARE { get; }

	internal static DpiAwarenessContextHandle DPI_AWARENESS_CONTEXT_SYSTEM_AWARE { get; }

	internal static DpiAwarenessContextHandle DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE { get; }

	internal static DpiAwarenessContextHandle DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 { get; }

	private static Dictionary<DpiAwarenessContextValue, nint> WellKnownContextValues { get; }

	static DpiAwarenessContextHandle()
	{
		WellKnownContextValues = new Dictionary<DpiAwarenessContextValue, nint>
		{
			{
				DpiAwarenessContextValue.Unaware,
				new IntPtr(-1)
			},
			{
				DpiAwarenessContextValue.SystemAware,
				new IntPtr(-2)
			},
			{
				DpiAwarenessContextValue.PerMonitorAware,
				new IntPtr(-3)
			},
			{
				DpiAwarenessContextValue.PerMonitorAwareVersion2,
				new IntPtr(-4)
			}
		};
		DPI_AWARENESS_CONTEXT_UNAWARE = new DpiAwarenessContextHandle(WellKnownContextValues[DpiAwarenessContextValue.Unaware]);
		DPI_AWARENESS_CONTEXT_SYSTEM_AWARE = new DpiAwarenessContextHandle(WellKnownContextValues[DpiAwarenessContextValue.SystemAware]);
		DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE = new DpiAwarenessContextHandle(WellKnownContextValues[DpiAwarenessContextValue.PerMonitorAware]);
		DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new DpiAwarenessContextHandle(WellKnownContextValues[DpiAwarenessContextValue.PerMonitorAwareVersion2]);
	}

	internal DpiAwarenessContextHandle()
		: base(IntPtr.Zero, ownsHandle: false)
	{
	}

	internal DpiAwarenessContextHandle(DpiAwarenessContextValue dpiAwarenessContextValue)
		: base(WellKnownContextValues[dpiAwarenessContextValue], ownsHandle: false)
	{
	}

	internal DpiAwarenessContextHandle(nint dpiContext)
		: base(dpiContext, ownsHandle: false)
	{
	}

	protected DpiAwarenessContextHandle(nint invalidHandleValue, bool ownsHandle)
		: base(invalidHandleValue, ownsHandle: false)
	{
	}

	public static explicit operator DpiAwarenessContextValue(DpiAwarenessContextHandle dpiAwarenessContextHandle)
	{
		foreach (DpiAwarenessContextValue value in Enum.GetValues(typeof(DpiAwarenessContextValue)))
		{
			if (value != 0 && dpiAwarenessContextHandle.Equals(value))
			{
				return value;
			}
		}
		return DpiAwarenessContextValue.Invalid;
	}

	public bool Equals(DpiAwarenessContextHandle dpiContextHandle)
	{
		return SafeNativeMethods.AreDpiAwarenessContextsEqual(DangerousGetHandle(), dpiContextHandle.DangerousGetHandle());
	}

	public bool Equals(nint dpiContext)
	{
		return DpiUtil.AreDpiAwarenessContextsEqual(DangerousGetHandle(), dpiContext);
	}

	public bool Equals(DpiAwarenessContextValue dpiContextEnumValue)
	{
		return Equals(WellKnownContextValues[dpiContextEnumValue]);
	}

	public override bool Equals(object obj)
	{
		if (obj is nint)
		{
			return Equals((nint)obj);
		}
		if (obj is DpiAwarenessContextHandle)
		{
			return Equals((DpiAwarenessContextHandle)obj);
		}
		if (obj is DpiAwarenessContextValue)
		{
			return Equals((DpiAwarenessContextValue)obj);
		}
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return ((DpiAwarenessContextValue)this).GetHashCode();
	}

	protected override bool ReleaseHandle()
	{
		return true;
	}
}
