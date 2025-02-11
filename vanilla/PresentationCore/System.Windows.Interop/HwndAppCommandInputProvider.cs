using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Interop;
using MS.Win32;

namespace System.Windows.Interop;

internal sealed class HwndAppCommandInputProvider : DispatcherObject, IInputProvider, IDisposable
{
	private SecurityCriticalDataClass<HwndSource> _source;

	private SecurityCriticalDataClass<InputProviderSite> _site;

	internal HwndAppCommandInputProvider(HwndSource source)
	{
		_site = new SecurityCriticalDataClass<InputProviderSite>(InputManager.Current.RegisterInputProvider(this));
		_source = new SecurityCriticalDataClass<HwndSource>(source);
	}

	public void Dispose()
	{
		if (_site != null)
		{
			_site.Value.Dispose();
			_site = null;
		}
		_source = null;
	}

	bool IInputProvider.ProvidesInputForRootVisual(Visual v)
	{
		return _source.Value.RootVisual == v;
	}

	void IInputProvider.NotifyDeactivate()
	{
	}

	internal nint FilterMessage(nint hwnd, WindowMessage msg, nint wParam, nint lParam, ref bool handled)
	{
		if (_source == null || _source.Value == null)
		{
			return IntPtr.Zero;
		}
		if (msg == WindowMessage.WM_APPCOMMAND)
		{
			RawAppCommandInputReport inputReport = new RawAppCommandInputReport(_source.Value, InputMode.Foreground, SafeNativeMethods.GetMessageTime(), GetAppCommand(lParam), GetDevice(lParam), InputType.Command);
			handled = _site.Value.ReportInput(inputReport);
		}
		if (!handled)
		{
			return IntPtr.Zero;
		}
		return new IntPtr(1);
	}

	private static int GetAppCommand(nint lParam)
	{
		return (short)(MS.Win32.NativeMethods.SignedHIWORD(MS.Win32.NativeMethods.IntPtrToInt32(lParam)) & -61441);
	}

	private static InputType GetDevice(nint lParam)
	{
		InputType inputType = InputType.Hid;
		return (ushort)(MS.Win32.NativeMethods.SignedHIWORD(MS.Win32.NativeMethods.IntPtrToInt32(lParam)) & 0xF000) switch
		{
			32768 => InputType.Mouse, 
			0 => InputType.Keyboard, 
			_ => InputType.Hid, 
		};
	}
}
