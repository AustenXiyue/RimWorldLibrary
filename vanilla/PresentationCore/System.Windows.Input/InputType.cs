namespace System.Windows.Input;

/// <summary>Specifies the possible types of input being reported.</summary>
public enum InputType
{
	/// <summary>Input was provided by a keyboard.</summary>
	Keyboard,
	/// <summary>Input was provided by a mouse.</summary>
	Mouse,
	/// <summary>Input was provided by a stylus.</summary>
	Stylus,
	/// <summary>Input was provided a Human Interface Device that was not a keyboard, a mouse, or a stylus.</summary>
	Hid,
	/// <summary>Input was provided by text.</summary>
	Text,
	/// <summary>Input was provided by a command.</summary>
	Command
}
