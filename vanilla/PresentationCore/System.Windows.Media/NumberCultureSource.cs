namespace System.Windows.Media;

/// <summary>Specifies how the culture for numbers in a text run is determined.</summary>
public enum NumberCultureSource
{
	/// <summary>Default. Number culture is derived from the value of the <see cref="P:System.Windows.Media.TextFormatting.TextRunProperties.CultureInfo" /> property, which is the culture of the text run. In markup, this is represented by the xml:lang attribute.</summary>
	Text,
	/// <summary>Number culture is derived from the culture value of the current thread, which by default is the user default culture.</summary>
	User,
	/// <summary>Number culture is derived from the <see cref="P:System.Windows.Media.NumberSubstitution.CultureOverride" /> property.</summary>
	Override
}
