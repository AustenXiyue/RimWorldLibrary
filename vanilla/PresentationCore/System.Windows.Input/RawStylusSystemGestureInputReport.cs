using System.ComponentModel;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

internal class RawStylusSystemGestureInputReport : RawStylusInputReport
{
	internal const SystemGesture InternalSystemGestureDoubleTap = (SystemGesture)17;

	private SystemGesture _id;

	private int _gestureX;

	private int _gestureY;

	private int _buttonState;

	internal SystemGesture SystemGesture => _id;

	internal int GestureX => _gestureX;

	internal int GestureY => _gestureY;

	internal int ButtonState => _buttonState;

	internal static bool IsValidSystemGesture(SystemGesture systemGesture, bool allowFlick, bool allowDoubleTap)
	{
		switch (systemGesture)
		{
		case SystemGesture.None:
		case SystemGesture.Tap:
		case SystemGesture.RightTap:
		case SystemGesture.Drag:
		case SystemGesture.RightDrag:
		case SystemGesture.HoldEnter:
		case SystemGesture.HoldLeave:
		case SystemGesture.HoverEnter:
		case SystemGesture.HoverLeave:
		case SystemGesture.TwoFingerTap:
			return true;
		case SystemGesture.Flick:
			return allowFlick;
		case (SystemGesture)17:
			return allowDoubleTap;
		default:
			return false;
		}
	}

	internal RawStylusSystemGestureInputReport(InputMode mode, int timestamp, PresentationSource inputSource, Func<StylusPointDescription> stylusPointDescGenerator, int tabletId, int stylusDeviceId, SystemGesture systemGesture, int gestureX, int gestureY, int buttonState)
		: base(mode, timestamp, inputSource, RawStylusActions.SystemGesture, stylusPointDescGenerator, tabletId, stylusDeviceId, Array.Empty<int>())
	{
		Initialize(systemGesture, gestureX, gestureY, buttonState);
	}

	internal RawStylusSystemGestureInputReport(InputMode mode, int timestamp, PresentationSource inputSource, PenContext penContext, int tabletId, int stylusDeviceId, SystemGesture systemGesture, int gestureX, int gestureY, int buttonState)
		: base(mode, timestamp, inputSource, penContext, RawStylusActions.SystemGesture, tabletId, stylusDeviceId, Array.Empty<int>())
	{
		Initialize(systemGesture, gestureX, gestureY, buttonState);
	}

	private void Initialize(SystemGesture systemGesture, int gestureX, int gestureY, int buttonState)
	{
		if (!IsValidSystemGesture(systemGesture, allowFlick: true, allowDoubleTap: true))
		{
			throw new InvalidEnumArgumentException(SR.Format(SR.Enum_Invalid, "systemGesture"));
		}
		_id = systemGesture;
		_gestureX = gestureX;
		_gestureY = gestureY;
		_buttonState = buttonState;
	}
}
