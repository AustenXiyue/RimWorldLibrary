using System.Runtime.InteropServices;
using MS.Internal;
using MS.Internal.Interop;
using MS.Win32;

namespace System.Windows.Media;

internal class MediaContextNotificationWindow : IDisposable
{
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	private delegate void ChangeWindowMessageFilterNative(WindowMessage message, uint flag);

	private bool _isDisposed;

	private MediaContext _ownerMediaContext;

	private SecurityCriticalDataClass<HwndWrapper> _hwndNotification;

	private HwndWrapperHook _hwndNotificationHook;

	private static WindowMessage s_channelNotifyMessage;

	private static WindowMessage s_dwmRedirectionEnvironmentChanged;

	static MediaContextNotificationWindow()
	{
		s_channelNotifyMessage = MS.Win32.UnsafeNativeMethods.RegisterWindowMessage("MilChannelNotify");
		s_dwmRedirectionEnvironmentChanged = MS.Win32.UnsafeNativeMethods.RegisterWindowMessage("DwmRedirectionEnvironmentChangedHint");
	}

	internal MediaContextNotificationWindow(MediaContext ownerMediaContext)
	{
		_ownerMediaContext = ownerMediaContext;
		HwndWrapper value = new HwndWrapper(0, int.MinValue, 0, 0, 0, 0, 0, "MediaContextNotificationWindow", IntPtr.Zero, null);
		_hwndNotificationHook = MessageFilter;
		_hwndNotification = new SecurityCriticalDataClass<HwndWrapper>(value);
		_hwndNotification.Value.AddHook(_hwndNotificationHook);
		_isDisposed = false;
		ChangeWindowMessageFilter(s_dwmRedirectionEnvironmentChanged, 1u);
		MS.Internal.HRESULT.Check(MilContent_AttachToHwnd(_hwndNotification.Value.Handle));
	}

	public void Dispose()
	{
		if (!_isDisposed)
		{
			MS.Internal.HRESULT.Check(MilContent_DetachFromHwnd(_hwndNotification.Value.Handle));
			_hwndNotification.Value.Dispose();
			_hwndNotificationHook = null;
			_hwndNotification = null;
			_ownerMediaContext = null;
			_isDisposed = true;
			GC.SuppressFinalize(this);
		}
	}

	internal void SetAsChannelNotificationWindow()
	{
		if (_isDisposed)
		{
			throw new ObjectDisposedException("MediaContextNotificationWindow");
		}
		_ownerMediaContext.Channel.SetNotificationWindow(_hwndNotification.Value.Handle, s_channelNotifyMessage);
	}

	private nint MessageFilter(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
	{
		if (_isDisposed)
		{
			throw new ObjectDisposedException("MediaContextNotificationWindow");
		}
		if (msg == 798)
		{
			MS.Internal.HRESULT.Check(MilContent_AttachToHwnd(_hwndNotification.Value.Handle));
		}
		else if (msg == (int)s_channelNotifyMessage)
		{
			_ownerMediaContext.NotifyChannelMessage();
		}
		else if (msg == (int)s_dwmRedirectionEnvironmentChanged)
		{
			MediaSystem.NotifyRedirectionEnvironmentChanged();
		}
		return IntPtr.Zero;
	}

	[DllImport("wpfgfx_cor3.dll")]
	private static extern int MilContent_AttachToHwnd(nint hwnd);

	[DllImport("wpfgfx_cor3.dll")]
	private static extern int MilContent_DetachFromHwnd(nint hwnd);

	private void ChangeWindowMessageFilter(WindowMessage message, uint flag)
	{
		nint moduleHandle = MS.Win32.UnsafeNativeMethods.GetModuleHandle("user32.dll");
		nint procAddressNoThrow = MS.Win32.UnsafeNativeMethods.GetProcAddressNoThrow(new HandleRef(null, moduleHandle), "ChangeWindowMessageFilter");
		if (procAddressNoThrow != IntPtr.Zero)
		{
			(Marshal.GetDelegateForFunctionPointer(procAddressNoThrow, typeof(ChangeWindowMessageFilterNative)) as ChangeWindowMessageFilterNative)(message, flag);
		}
	}
}
