namespace System.Windows;

/// <summary>Specifies whether the text in the object is left-aligned, right-aligned, centered, or justified.</summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public enum TextAlignment
{
	/// <summary>Default. Text is aligned to the left.</summary>
	Left,
	/// <summary>Text is aligned to the right.</summary>
	Right,
	/// <summary>Text is centered.</summary>
	Center,
	/// <summary>Text is justified.</summary>
	Justify
}
