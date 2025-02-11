using MS.Win32;

namespace System.Windows.Input;

internal sealed class Win32KeyboardDevice : KeyboardDevice
{
	internal Win32KeyboardDevice(InputManager inputManager)
		: base(inputManager)
	{
	}

	protected override KeyStates GetKeyStatesFromSystem(Key key)
	{
		KeyStates keyStates = KeyStates.None;
		short keyState = MS.Win32.UnsafeNativeMethods.GetKeyState(KeyInterop.VirtualKeyFromKey(key));
		if ((keyState & 0x8000) == 32768)
		{
			keyStates |= KeyStates.Down;
		}
		if ((keyState & 1) == 1)
		{
			keyStates |= KeyStates.Toggled;
		}
		return keyStates;
	}
}
