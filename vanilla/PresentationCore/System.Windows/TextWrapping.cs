namespace System.Windows;

/// <summary> Specifies whether text wraps when it reaches the edge of the containing box </summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public enum TextWrapping
{
	/// <summary> Line-breaking occurs if the line overflows beyond the available block width. However, a line may overflow beyond the block width if the line breaking algorithm cannot determine a line break opportunity, as in the case of a very long word constrained in a fixed-width container with no scrolling allowed. </summary>
	WrapWithOverflow,
	/// <summary> No line wrapping is performed. </summary>
	NoWrap,
	/// <summary> Line-breaking occurs if the line overflows beyond the available block width, even if the standard line breaking algorithm cannot determine any line break opportunity, as in the case of a very long word constrained in a fixed-width container with no scrolling allowed. </summary>
	Wrap
}
