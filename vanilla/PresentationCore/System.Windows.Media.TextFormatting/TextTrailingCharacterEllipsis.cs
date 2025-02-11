namespace System.Windows.Media.TextFormatting;

/// <summary>Defines collapsed text properties for collapsing a whole line toward the end at character granularity, and with ellipsis being the collapsed text symbol.</summary>
public class TextTrailingCharacterEllipsis : TextCollapsingProperties
{
	private double _width;

	private TextRun _ellipsis;

	private const string StringHorizontalEllipsis = "…";

	/// <summary>Gets the width for which the specified collapsed text range is constrained to.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the width.</returns>
	public sealed override double Width => _width;

	/// <summary>Gets the text run that is used as the collapsed text symbol.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextFormatting.TextRun" /> value that represents the collapsed text symbol.</returns>
	public sealed override TextRun Symbol => _ellipsis;

	/// <summary>Gets the style of collapsed text.</summary>
	/// <returns>An enumerated value of <see cref="T:System.Windows.Media.TextFormatting.TextCollapsingStyle" />.</returns>
	public sealed override TextCollapsingStyle Style => TextCollapsingStyle.TrailingCharacter;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.TextTrailingCharacterEllipsis" /> class by specifying collapsed text properties.</summary>
	/// <param name="width">A <see cref="T:System.Double" /> that represents the width for which the specified collapsed text range is constrained to.</param>
	/// <param name="textRunProperties">A <see cref="T:System.Windows.Media.TextFormatting.TextRunProperties" /> value that represents the set of shared text properties.</param>
	public TextTrailingCharacterEllipsis(double width, TextRunProperties textRunProperties)
	{
		_width = width;
		_ellipsis = new TextCharacters("…", textRunProperties);
	}
}
