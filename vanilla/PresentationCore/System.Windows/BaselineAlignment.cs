namespace System.Windows;

/// <summary>Describes how the baseline for a text-based element is positioned on the vertical axis, relative to the established baseline for text.</summary>
public enum BaselineAlignment
{
	/// <summary>A baseline that is aligned to the upper edge of the containing box.</summary>
	Top,
	/// <summary>A baseline that is aligned to the center of the containing box.</summary>
	Center,
	/// <summary>A baseline that is aligned at the lower edge of the containing box.</summary>
	Bottom,
	/// <summary>A baseline that is aligned at the actual baseline of the containing box.</summary>
	Baseline,
	/// <summary>A baseline that is aligned at the upper edge of the text baseline.</summary>
	TextTop,
	/// <summary>A baseline that is aligned at the lower edge of the text baseline.</summary>
	TextBottom,
	/// <summary>A baseline that is aligned at the subscript position of the containing box.</summary>
	Subscript,
	/// <summary>A baseline that is aligned at the superscript position of the containing box.</summary>
	Superscript
}
