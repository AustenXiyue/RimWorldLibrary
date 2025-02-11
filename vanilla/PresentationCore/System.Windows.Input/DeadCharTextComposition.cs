namespace System.Windows.Input;

internal sealed class DeadCharTextComposition : TextComposition
{
	private bool _composed;

	internal bool Composed
	{
		get
		{
			return _composed;
		}
		set
		{
			_composed = value;
		}
	}

	internal DeadCharTextComposition(InputManager inputManager, IInputElement source, string text, TextCompositionAutoComplete autoComplete, InputDevice inputDevice)
		: base(inputManager, source, text, autoComplete, inputDevice)
	{
	}
}
