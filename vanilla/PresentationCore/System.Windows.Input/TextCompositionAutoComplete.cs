namespace System.Windows.Input;

/// <summary>Defines a set of states for the handling of automatic completion of a text composition.</summary>
public enum TextCompositionAutoComplete
{
	/// <summary>Auto-complete is off.</summary>
	Off,
	/// <summary>Auto-complete is on. A <see cref="E:System.Windows.Input.TextCompositionManager.TextInput" /> event will be raised automatically by the <see cref="T:System.Windows.Input.TextCompositionManager" /> after a <see cref="E:System.Windows.Input.TextCompositionManager.TextInputStart" /> event is handled.</summary>
	On
}
