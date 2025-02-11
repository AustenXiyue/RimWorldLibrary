namespace System.Windows.Input;

/// <summary>Specifies constants that define the state of a key.</summary>
[Flags]
public enum KeyStates : byte
{
	/// <summary>The key is not pressed.</summary>
	None = 0,
	/// <summary>The key is pressed.</summary>
	Down = 1,
	/// <summary>The key is toggled.</summary>
	Toggled = 2
}
