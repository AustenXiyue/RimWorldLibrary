namespace System.Windows.Media;

/// <summary>Defines the rendering behavior of static or animated text.</summary>
public enum TextHintingMode
{
	/// <summary>The rendering engine automatically determines whether to draw text with quality settings appropriate for animated or static text.</summary>
	Auto,
	/// <summary>The rendering engine renders text with the highest static quality.</summary>
	Fixed,
	/// <summary>The rendering engine renders text with the highest animated quality. </summary>
	Animated
}
