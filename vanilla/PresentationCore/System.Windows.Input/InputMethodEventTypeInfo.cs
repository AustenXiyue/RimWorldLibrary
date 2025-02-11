using MS.Win32;

namespace System.Windows.Input;

internal class InputMethodEventTypeInfo
{
	private static readonly InputMethodEventTypeInfo _iminfoImeState = new InputMethodEventTypeInfo(InputMethodStateType.ImeState, MS.Win32.UnsafeNativeMethods.GUID_COMPARTMENT_KEYBOARD_OPENCLOSE, CompartmentScope.Thread);

	private static readonly InputMethodEventTypeInfo _iminfoHandwritingState = new InputMethodEventTypeInfo(InputMethodStateType.HandwritingState, MS.Win32.UnsafeNativeMethods.GUID_COMPARTMENT_HANDWRITING_OPENCLOSE, CompartmentScope.Thread);

	private static readonly InputMethodEventTypeInfo _iminfoMicrophoneState = new InputMethodEventTypeInfo(InputMethodStateType.MicrophoneState, MS.Win32.UnsafeNativeMethods.GUID_COMPARTMENT_SPEECH_OPENCLOSE, CompartmentScope.Global);

	private static readonly InputMethodEventTypeInfo _iminfoSpeechMode = new InputMethodEventTypeInfo(InputMethodStateType.SpeechMode, MS.Win32.UnsafeNativeMethods.GUID_COMPARTMENT_SPEECH_GLOBALSTATE, CompartmentScope.Global);

	private static readonly InputMethodEventTypeInfo _iminfoImeConversionMode = new InputMethodEventTypeInfo(InputMethodStateType.ImeConversionModeValues, MS.Win32.UnsafeNativeMethods.GUID_COMPARTMENT_KEYBOARD_INPUTMODE_CONVERSION, CompartmentScope.Thread);

	private static readonly InputMethodEventTypeInfo _iminfoImeSentenceMode = new InputMethodEventTypeInfo(InputMethodStateType.ImeSentenceModeValues, MS.Win32.UnsafeNativeMethods.GUID_COMPARTMENT_KEYBOARD_INPUTMODE_SENTENCE, CompartmentScope.Thread);

	private static readonly InputMethodEventTypeInfo[] _iminfo = new InputMethodEventTypeInfo[6] { _iminfoImeState, _iminfoHandwritingState, _iminfoMicrophoneState, _iminfoSpeechMode, _iminfoImeConversionMode, _iminfoImeSentenceMode };

	private InputMethodStateType _inputmethodstatetype;

	private Guid _guid;

	private CompartmentScope _scope;

	internal InputMethodStateType Type => _inputmethodstatetype;

	internal Guid Guid => _guid;

	internal CompartmentScope Scope => _scope;

	internal static InputMethodEventTypeInfo[] InfoList => _iminfo;

	internal InputMethodEventTypeInfo(InputMethodStateType type, Guid guid, CompartmentScope scope)
	{
		_inputmethodstatetype = type;
		_guid = guid;
		_scope = scope;
	}

	internal static InputMethodStateType ToType(ref Guid rguid)
	{
		for (int i = 0; i < _iminfo.Length; i++)
		{
			InputMethodEventTypeInfo inputMethodEventTypeInfo = _iminfo[i];
			if (rguid == inputMethodEventTypeInfo._guid)
			{
				return inputMethodEventTypeInfo._inputmethodstatetype;
			}
		}
		return InputMethodStateType.Invalid;
	}
}
