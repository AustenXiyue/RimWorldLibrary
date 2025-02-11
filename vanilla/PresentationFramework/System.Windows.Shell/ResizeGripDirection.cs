namespace System.Windows.Shell;

/// <summary>Specifies constants that indicate the direction of the resize grip behavior on an input element.</summary>
public enum ResizeGripDirection
{
	/// <summary>No resize behavior is specified.</summary>
	None,
	/// <summary>The window resizes from its top-left corner.</summary>
	TopLeft,
	/// <summary>The window resizes from its top edge.</summary>
	Top,
	/// <summary>The window resizes from its top-right corner.</summary>
	TopRight,
	/// <summary>The window resizes from its right edge.</summary>
	Right,
	/// <summary>The window resizes from its bottom-right corner.</summary>
	BottomRight,
	/// <summary>The window resizes from its bottom edge.</summary>
	Bottom,
	/// <summary>The window resizes from its bottom-left corner.</summary>
	BottomLeft,
	/// <summary>The windows resizes from its left edge.</summary>
	Left
}
