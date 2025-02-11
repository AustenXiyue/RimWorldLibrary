using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Input.StylusWisp;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Interop;
using MS.Utility;
using MS.Win32;

namespace System.Windows.Interop;

internal sealed class HwndStylusInputProvider : DispatcherObject, IStylusInputProvider, IInputProvider, IDisposable
{
	private const uint TABLET_PRESSANDHOLD_DISABLED = 1u;

	private const uint TABLET_TAPFEEDBACK_DISABLED = 8u;

	private const uint TABLET_TOUCHUI_FORCEON = 256u;

	private const uint TABLET_TOUCHUI_FORCEOFF = 512u;

	private const uint TABLET_FLICKS_DISABLED = 65536u;

	private const int MultiTouchEnabledFlag = 16777216;

	private SecurityCriticalDataClass<WispLogic> _stylusLogic;

	private SecurityCriticalDataClass<HwndSource> _source;

	private SecurityCriticalDataClass<InputProviderSite> _site;

	internal HwndStylusInputProvider(HwndSource source)
	{
		InputManager current = InputManager.Current;
		_stylusLogic = new SecurityCriticalDataClass<WispLogic>(StylusLogic.GetCurrentStylusLogicAs<WispLogic>());
		_site = new SecurityCriticalDataClass<InputProviderSite>(current.RegisterInputProvider(this));
		nint handle = source.Handle;
		_stylusLogic.Value.RegisterHwndForInput(current, source);
		_source = new SecurityCriticalDataClass<HwndSource>(source);
		MS.Win32.UnsafeNativeMethods.SetProp(new HandleRef(this, handle), "MicrosoftTabletPenServiceProperty", new HandleRef(null, new IntPtr(16777216)));
	}

	public void Dispose()
	{
		if (_site != null)
		{
			_site.Value.Dispose();
			_site = null;
			_stylusLogic.Value.UnRegisterHwndForInput(_source.Value);
			_stylusLogic = null;
			_source = null;
		}
	}

	bool IInputProvider.ProvidesInputForRootVisual(Visual v)
	{
		return _source.Value.RootVisual == v;
	}

	void IInputProvider.NotifyDeactivate()
	{
	}

	nint IStylusInputProvider.FilterMessage(nint hwnd, WindowMessage msg, nint wParam, nint lParam, ref bool handled)
	{
		nint result = IntPtr.Zero;
		if (_source == null || _source.Value == null)
		{
			return result;
		}
		switch (msg)
		{
		case WindowMessage.WM_ENABLE:
			_stylusLogic.Value.OnWindowEnableChanged(hwnd, MS.Win32.NativeMethods.IntPtrToInt32(wParam) == 0);
			break;
		case WindowMessage.WM_TABLET_QUERYSYSTEMGESTURESTATUS:
		{
			handled = true;
			MS.Win32.NativeMethods.POINT pt = new MS.Win32.NativeMethods.POINT(MS.Win32.NativeMethods.SignedLOWORD(lParam), MS.Win32.NativeMethods.SignedHIWORD(lParam));
			SafeNativeMethods.ScreenToClient(new HandleRef(this, hwnd), ref pt);
			IInputElement inputElement = StylusDevice.LocalHitTest(pt: new Point(pt.x, pt.y), inputSource: _source.Value);
			if (inputElement != null)
			{
				DependencyObject element = (DependencyObject)inputElement;
				bool isPressAndHoldEnabled = Stylus.GetIsPressAndHoldEnabled(element);
				bool isFlicksEnabled = Stylus.GetIsFlicksEnabled(element);
				bool isTapFeedbackEnabled = Stylus.GetIsTapFeedbackEnabled(element);
				bool isTouchFeedbackEnabled = Stylus.GetIsTouchFeedbackEnabled(element);
				uint num = 0u;
				if (!isPressAndHoldEnabled)
				{
					num |= 1;
				}
				if (!isTapFeedbackEnabled)
				{
					num |= 8;
				}
				num = ((!isTouchFeedbackEnabled) ? (num | 0x200) : (num | 0x100));
				if (!isFlicksEnabled)
				{
					num |= 0x10000;
				}
				result = new IntPtr(num);
			}
			break;
		}
		case WindowMessage.WM_TABLET_FLICK:
		{
			handled = true;
			int flickData = MS.Win32.NativeMethods.IntPtrToInt32(wParam);
			if (_stylusLogic != null && _stylusLogic.Value.Enabled && StylusLogic.GetFlickAction(flickData) == StylusLogic.FlickAction.Scroll)
			{
				result = new IntPtr(1);
			}
			break;
		}
		}
		if (handled && EventTrace.IsEnabled(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordInput, EventTrace.Level.Info))
		{
			EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientInputMessage, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordInput, EventTrace.Level.Info, (_source.Value.CompositionTarget != null) ? _source.Value.CompositionTarget.Dispatcher.GetHashCode() : 0, ((IntPtr)hwnd).ToInt64(), msg, (int)wParam, (int)lParam);
		}
		return result;
	}
}
