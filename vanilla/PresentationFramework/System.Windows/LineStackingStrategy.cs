namespace System.Windows;

/// <summary> Describes a mechanism by which a line box is determined for each line.  </summary>
public enum LineStackingStrategy
{
	/// <summary> The stack height is determined by the block element line-height property value. </summary>
	BlockLineHeight,
	/// <summary> The stack height is the smallest value that containing all the inline elements on that line when those elements are properly aligned. </summary>
	MaxHeight
}
