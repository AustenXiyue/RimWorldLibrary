using MS.Win32.Pointer;

namespace System.Windows.Input.StylusPointer;

internal class PointerData
{
	private UnsafeNativeMethods.POINTER_INFO _info;

	private UnsafeNativeMethods.POINTER_TOUCH_INFO _touchInfo;

	private UnsafeNativeMethods.POINTER_PEN_INFO _penInfo;

	private UnsafeNativeMethods.POINTER_INFO[] _history;

	internal bool IsValid { get; private set; }

	internal UnsafeNativeMethods.POINTER_INFO Info => _info;

	internal UnsafeNativeMethods.POINTER_TOUCH_INFO TouchInfo => _touchInfo;

	internal UnsafeNativeMethods.POINTER_PEN_INFO PenInfo => _penInfo;

	internal UnsafeNativeMethods.POINTER_INFO[] History => _history;

	internal PointerData(uint pointerId)
	{
		if (IsValid = UnsafeNativeMethods.GetPointerInfo(pointerId, ref _info))
		{
			_history = new UnsafeNativeMethods.POINTER_INFO[_info.historyCount];
			if (!UnsafeNativeMethods.GetPointerInfoHistory(pointerId, ref _info.historyCount, _history))
			{
				_history = Array.Empty<UnsafeNativeMethods.POINTER_INFO>();
			}
			switch (_info.pointerType)
			{
			case UnsafeNativeMethods.POINTER_INPUT_TYPE.PT_TOUCH:
				IsValid &= UnsafeNativeMethods.GetPointerTouchInfo(pointerId, ref _touchInfo);
				break;
			case UnsafeNativeMethods.POINTER_INPUT_TYPE.PT_PEN:
				IsValid &= UnsafeNativeMethods.GetPointerPenInfo(pointerId, ref _penInfo);
				break;
			default:
				IsValid = false;
				break;
			}
		}
	}
}
