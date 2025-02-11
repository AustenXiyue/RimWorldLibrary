namespace System.Windows;

/// <summary> Describes the point of reference of a figure in the vertical direction. </summary>
public enum FigureVerticalAnchor
{
	/// <summary> Anchor the figure to the top of the page area. </summary>
	PageTop,
	/// <summary> Anchor the figure to the center of the page area. </summary>
	PageCenter,
	/// <summary> Anchor the figure to the bottom of the page area. </summary>
	PageBottom,
	/// <summary> Anchor the figure to the top of the page content area. </summary>
	ContentTop,
	/// <summary> Anchor the figure to the center of the page content area. </summary>
	ContentCenter,
	/// <summary> Anchor the figure to the bottom of the page content area. </summary>
	ContentBottom,
	/// <summary> Anchor the figure to the top of the current paragraph. </summary>
	ParagraphTop
}
