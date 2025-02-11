namespace System.Windows;

/// <summary>Provides a mechanism for the user to select glyphs of different width styles.</summary>
public enum FontEastAsianWidths
{
	/// <summary>Default width style.</summary>
	Normal,
	/// <summary>Replaces uniform width glyphs with proportionally spaced glyphs.</summary>
	Proportional,
	/// <summary>Replaces uniform width glyphs with full width (usually em) glyphs.</summary>
	Full,
	/// <summary>Replaces uniform width glyphs with half width (half em) glyphs.</summary>
	Half,
	/// <summary>Replaces uniform width glyphs with one-third width (one-third em) glyphs.</summary>
	Third,
	/// <summary>Replaces uniform width glyphs with one-quarter width (one-quarter em) glyphs.</summary>
	Quarter
}
