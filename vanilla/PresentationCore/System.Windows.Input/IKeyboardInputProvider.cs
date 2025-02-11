namespace System.Windows.Input;

internal interface IKeyboardInputProvider : IInputProvider
{
	bool AcquireFocus(bool checkOnly);
}
