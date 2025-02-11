namespace System.Windows.Controls;

/// <summary>Specifies the spelling reform rules used by the spellchecker of the text editing control (i.e. <see cref="T:System.Windows.Controls.TextBox" /> or <see cref="T:System.Windows.Controls.RichTextBox" />).</summary>
public enum SpellingReform
{
	/// <summary>Use spelling rules from both before and after the spelling reform.</summary>
	PreAndPostreform,
	/// <summary>Use spelling rules from before the spelling reform.</summary>
	Prereform,
	/// <summary>Use spelling rules from after the spelling reform.</summary>
	Postreform
}
