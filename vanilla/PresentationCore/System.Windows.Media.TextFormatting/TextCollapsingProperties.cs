namespace System.Windows.Media.TextFormatting;

/// <summary>Represents the characteristics of collapsed text.</summary>
public abstract class TextCollapsingProperties
{
	/// <summary>Gets the width of the range of collapsed text.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the width of the range of collapsed text.</returns>
	public abstract double Width { get; }

	/// <summary>Gets the text run that is used as the collapsed text symbol.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextFormatting.TextRun" /> value that represents the collapsed text symbol.</returns>
	public abstract TextRun Symbol { get; }

	/// <summary>Gets the style of the collapsed text.</summary>
	/// <returns>An enumerated value of <see cref="T:System.Windows.Media.TextFormatting.TextCollapsingStyle" />.</returns>
	public abstract TextCollapsingStyle Style { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.TextCollapsingProperties" /> class.</summary>
	protected TextCollapsingProperties()
	{
	}
}
