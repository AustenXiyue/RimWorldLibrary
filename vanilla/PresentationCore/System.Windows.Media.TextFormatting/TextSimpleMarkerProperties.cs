using MS.Internal.PresentationCore;
using MS.Internal.TextFormatting;

namespace System.Windows.Media.TextFormatting;

/// <summary>Provides for a generic implementation of text marker properties.</summary>
public class TextSimpleMarkerProperties : TextMarkerProperties
{
	private double _offset;

	private TextSource _textSource;

	/// <summary>Gets the distance from the start of the line to the end of the text marker symbol.</summary>
	/// <returns>An <see cref="T:System.Int32" /> object that represents the offset of the text marker symbol.</returns>
	public sealed override double Offset => _offset;

	/// <summary>Gets the source of the text runs used for the text marker.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextFormatting.TextSource" /> value that represents the text run used for the text marker.</returns>
	public sealed override TextSource TextSource => _textSource;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.TextSimpleMarkerProperties" /> class.</summary>
	/// <param name="style">An enumerated value of <see cref="T:System.Windows.TextMarkerStyle" />.</param>
	/// <param name="offset">A <see cref="T:System.Double" /> that represents the distance from the start of the line to the end of the text marker symbol.</param>
	/// <param name="autoNumberingIndex">An <see cref="T:System.Int32" /> value that represents the auto-numbering counter of counter-style text marker.</param>
	/// <param name="textParagraphProperties">A <see cref="T:System.Windows.Media.TextFormatting.TextParagraphProperties" /> value that represents the properties shared by every text character of the text marker.</param>
	public TextSimpleMarkerProperties(TextMarkerStyle style, double offset, int autoNumberingIndex, TextParagraphProperties textParagraphProperties)
	{
		if (textParagraphProperties == null)
		{
			throw new ArgumentNullException("textParagraphProperties");
		}
		_offset = offset;
		if (style == TextMarkerStyle.None)
		{
			return;
		}
		if (!TextMarkerSource.IsKnownSymbolMarkerStyle(style))
		{
			if (!TextMarkerSource.IsKnownIndexMarkerStyle(style))
			{
				throw new ArgumentException(SR.Format(SR.Enum_Invalid, typeof(TextMarkerStyle)), "style");
			}
			if (autoNumberingIndex < 1)
			{
				throw new ArgumentOutOfRangeException("autoNumberingIndex", SR.Format(SR.ParameterCannotBeLessThan, 1));
			}
		}
		_textSource = new TextMarkerSource(textParagraphProperties, style, autoNumberingIndex);
	}
}
