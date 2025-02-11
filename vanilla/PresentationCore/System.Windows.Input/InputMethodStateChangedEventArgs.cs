namespace System.Windows.Input;

/// <summary>Contains arguments associated with the <see cref="E:System.Windows.Input.InputMethod.StateChanged" /> event.</summary>
public class InputMethodStateChangedEventArgs : EventArgs
{
	private InputMethodStateType _statetype;

	/// <summary>Gets a value that indicates whether or not the <see cref="P:System.Windows.Input.InputMethod.ImeState" /> property changed.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Input.InputMethod.ImeState" /> property changed; otherwise, false.This property has no default value.</returns>
	public bool IsImeStateChanged => _statetype == InputMethodStateType.ImeState;

	/// <summary>Gets a value that indicates whether or not the <see cref="P:System.Windows.Input.InputMethod.MicrophoneState" /> property changed.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Input.InputMethod.MicrophoneState" /> property changed; otherwise, false.This property has no default value.</returns>
	public bool IsMicrophoneStateChanged => _statetype == InputMethodStateType.MicrophoneState;

	/// <summary>Gets a value that indicates whether or not the <see cref="P:System.Windows.Input.InputMethod.HandwritingState" /> property changed.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Input.InputMethod.HandwritingState" /> property changed; otherwise, false.This property has no default value.</returns>
	public bool IsHandwritingStateChanged => _statetype == InputMethodStateType.HandwritingState;

	/// <summary>Gets a value that indicates whether or not the <see cref="P:System.Windows.Input.InputMethod.SpeechMode" /> property changed.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Input.InputMethod.SpeechMode" /> property changed; otherwise, false.This property has no default value.</returns>
	public bool IsSpeechModeChanged => _statetype == InputMethodStateType.SpeechMode;

	/// <summary>Gets a value that indicates whether or not the <see cref="P:System.Windows.Input.InputMethod.ImeConversionMode" /> property changed.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Input.InputMethod.ImeConversionMode" /> property changed; otherwise, false.This property has no default value.</returns>
	public bool IsImeConversionModeChanged => _statetype == InputMethodStateType.ImeConversionModeValues;

	/// <summary>Gets a value that indicates whether or not the <see cref="P:System.Windows.Input.InputMethod.ImeSentenceMode" /> property changed.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Input.InputMethod.ImeSentenceMode" /> property changed; otherwise, false.This property has no default value.</returns>
	public bool IsImeSentenceModeChanged => _statetype == InputMethodStateType.ImeSentenceModeValues;

	internal InputMethodStateChangedEventArgs(InputMethodStateType statetype)
	{
		_statetype = statetype;
	}
}
