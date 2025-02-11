namespace System.Windows.Media;

/// <summary>Determines how the edges of non-text drawing primitives are rendered.</summary>
public enum EdgeMode
{
	/// <summary>No edge mode is specified. Do not alter the current edge mode of non-text drawing primitives. This is the default value.</summary>
	Unspecified,
	/// <summary>Render the edges of non-text drawing primitives as aliased edges.</summary>
	Aliased
}
