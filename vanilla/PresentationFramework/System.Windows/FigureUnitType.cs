namespace System.Windows;

/// <summary>Describes the unit type associated with the width or height of a <see cref="T:System.Windows.FigureLength" />.</summary>
public enum FigureUnitType
{
	/// <summary>Default value when the <see cref="T:System.Windows.FigureLength" /> is not specified which creates a value for the width or height of the <see cref="T:System.Windows.Documents.Figure" /> that is calculated without constraints. Note: When <see cref="T:System.Windows.FigureUnitType" /> is set to <see cref="F:System.Windows.FigureUnitType.Auto" />, the <see cref="P:System.Windows.FigureLength.Value" /> property of <see cref="T:System.Windows.FigureLength" /> is set to 1. </summary>
	Auto,
	/// <summary>The value of the width or height of the <see cref="T:System.Windows.Documents.Figure" /> is expressed in pixels (96 pixels-per-inch).</summary>
	Pixel,
	/// <summary>The value of the width or height of the <see cref="T:System.Windows.Documents.Figure" /> is expressed as a fraction (including fractions greater then 1) of the width of the column the <see cref="T:System.Windows.Documents.Figure" /> is in.</summary>
	Column,
	/// <summary>The value of the width or height of the <see cref="T:System.Windows.Documents.Figure" /> is expressed as a fraction (including fractions greater then 1) of the content width of the <see cref="T:System.Windows.Documents.Figure" />. Note: Note: When <see cref="T:System.Windows.FigureUnitType" /> is set to <see cref="F:System.Windows.FigureUnitType.Content" />, the <see cref="P:System.Windows.FigureLength.Value" /> property of <see cref="T:System.Windows.FigureLength" /> must be set to a value between 0 and 1.</summary>
	Content,
	/// <summary>The value of the width or height of the <see cref="T:System.Windows.Documents.Figure" /> is expressed as a fraction (including fractions greater then 1) of the page width of that the <see cref="T:System.Windows.Documents.Figure" /> is in. Note: Note: When <see cref="T:System.Windows.FigureUnitType" /> is set to <see cref="F:System.Windows.FigureUnitType.Page" />, the <see cref="P:System.Windows.FigureLength.Value" /> property of <see cref="T:System.Windows.FigureLength" /> must be set to a value between 0 and 1.</summary>
	Page
}
