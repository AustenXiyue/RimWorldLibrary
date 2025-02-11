using System.ComponentModel;
using MS.Internal;

namespace System.Windows.Input;

internal class RawKeyboardInputReport : InputReport
{
	private RawKeyboardActions _actions;

	private int _scanCode;

	private bool _isExtendedKey;

	private bool _isSystemKey;

	private int _virtualKey;

	private SecurityCriticalData<nint> _extraInformation;

	public RawKeyboardActions Actions => _actions;

	public int ScanCode => _scanCode;

	public bool IsExtendedKey => _isExtendedKey;

	public bool IsSystemKey => _isSystemKey;

	public int VirtualKey => _virtualKey;

	public nint ExtraInformation => _extraInformation.Value;

	public RawKeyboardInputReport(PresentationSource inputSource, InputMode mode, int timestamp, RawKeyboardActions actions, int scanCode, bool isExtendedKey, bool isSystemKey, int virtualKey, nint extraInformation)
		: base(inputSource, InputType.Keyboard, mode, timestamp)
	{
		if (!IsValidRawKeyboardActions(actions))
		{
			throw new InvalidEnumArgumentException("actions", (int)actions, typeof(RawKeyboardActions));
		}
		_actions = actions;
		_scanCode = scanCode;
		_isExtendedKey = isExtendedKey;
		_isSystemKey = isSystemKey;
		_virtualKey = virtualKey;
		_extraInformation = new SecurityCriticalData<nint>(extraInformation);
	}

	internal static bool IsValidRawKeyboardActions(RawKeyboardActions actions)
	{
		if (((RawKeyboardActions.AttributesChanged | RawKeyboardActions.Activate | RawKeyboardActions.Deactivate | RawKeyboardActions.KeyDown | RawKeyboardActions.KeyUp) & actions) == actions && ((RawKeyboardActions.KeyDown | RawKeyboardActions.KeyUp) & actions) != (RawKeyboardActions.KeyDown | RawKeyboardActions.KeyUp) && ((RawKeyboardActions.Deactivate & actions) != actions || RawKeyboardActions.Deactivate == actions))
		{
			return true;
		}
		return false;
	}
}
