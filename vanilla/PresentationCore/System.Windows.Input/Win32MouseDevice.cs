using MS.Win32;

namespace System.Windows.Input;

internal sealed class Win32MouseDevice : MouseDevice
{
	internal Win32MouseDevice(InputManager inputManager)
		: base(inputManager)
	{
	}

	internal override MouseButtonState GetButtonStateFromSystem(MouseButton mouseButton)
	{
		MouseButtonState result = MouseButtonState.Released;
		if (base.IsActive)
		{
			int keyCode = 0;
			switch (mouseButton)
			{
			case MouseButton.Left:
				keyCode = 1;
				break;
			case MouseButton.Right:
				keyCode = 2;
				break;
			case MouseButton.Middle:
				keyCode = 4;
				break;
			case MouseButton.XButton1:
				keyCode = 5;
				break;
			case MouseButton.XButton2:
				keyCode = 6;
				break;
			}
			result = (((MS.Win32.UnsafeNativeMethods.GetKeyState(keyCode) & 0x8000) != 0) ? MouseButtonState.Pressed : MouseButtonState.Released);
		}
		return result;
	}
}
