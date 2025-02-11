using System.Runtime.InteropServices;
using MS.Internal;
using MS.Win32;

namespace System.Windows.Documents;

internal class WinEventHandler : IDisposable
{
	private sealed class WinEventHandlerShutDownListener : ShutDownListener
	{
		public WinEventHandlerShutDownListener(WinEventHandler target)
			: base(target, ShutDownEvents.DispatcherShutdown)
		{
		}

		internal override void OnShutDown(object target, object sender, EventArgs e)
		{
			((WinEventHandler)target).Stop();
		}
	}

	private int _eventMin;

	private int _eventMax;

	private MS.Internal.SecurityCriticalDataForSet<nint> _hHook;

	private MS.Internal.SecurityCriticalDataForSet<MS.Win32.NativeMethods.WinEventProcDef> _winEventProc;

	private GCHandle _gchThis;

	private WinEventHandlerShutDownListener _shutdownListener;

	internal WinEventHandler(int eventMin, int eventMax)
	{
		_eventMin = eventMin;
		_eventMax = eventMax;
		_winEventProc.Value = WinEventDefaultProc;
		_gchThis = GCHandle.Alloc(_winEventProc.Value);
		_shutdownListener = new WinEventHandlerShutDownListener(this);
	}

	~WinEventHandler()
	{
		Clear();
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Clear();
	}

	internal virtual void WinEventProc(int eventId, nint hwnd)
	{
	}

	internal void Clear()
	{
		Stop();
		if (_gchThis.IsAllocated)
		{
			_gchThis.Free();
		}
	}

	internal void Start()
	{
		if (_gchThis.IsAllocated)
		{
			_hHook.Value = MS.Win32.UnsafeNativeMethods.SetWinEventHook(_eventMin, _eventMax, IntPtr.Zero, _winEventProc.Value, 0u, 0u, 0);
			if (_hHook.Value == IntPtr.Zero)
			{
				Stop();
			}
		}
	}

	internal void Stop()
	{
		if (_hHook.Value != IntPtr.Zero)
		{
			MS.Win32.UnsafeNativeMethods.UnhookWinEvent(_hHook.Value);
			_hHook.Value = IntPtr.Zero;
		}
		if (_shutdownListener != null)
		{
			_shutdownListener.StopListening();
			_shutdownListener = null;
		}
	}

	private void WinEventDefaultProc(int winEventHook, int eventId, nint hwnd, int idObject, int idChild, int eventThread, int eventTime)
	{
		WinEventProc(eventId, hwnd);
	}
}
