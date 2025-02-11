namespace System.Windows.Input;

internal class RawTextInputReport : InputReport
{
	private readonly bool _isDeadCharacter;

	private readonly bool _isSystemCharacter;

	private readonly bool _isControlCharacter;

	private readonly char _characterCode;

	public bool IsDeadCharacter => _isDeadCharacter;

	public bool IsSystemCharacter => _isSystemCharacter;

	public bool IsControlCharacter => _isControlCharacter;

	public char CharacterCode => _characterCode;

	public RawTextInputReport(PresentationSource inputSource, InputMode mode, int timestamp, bool isDeadCharacter, bool isSystemCharacter, bool isControlCharacter, char characterCode)
		: base(inputSource, InputType.Text, mode, timestamp)
	{
		_isDeadCharacter = isDeadCharacter;
		_isSystemCharacter = isSystemCharacter;
		_isControlCharacter = isControlCharacter;
		_characterCode = characterCode;
	}
}
