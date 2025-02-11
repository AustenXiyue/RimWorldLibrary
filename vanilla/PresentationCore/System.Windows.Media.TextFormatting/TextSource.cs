using MS.Internal.FontCache;

namespace System.Windows.Media.TextFormatting;

/// <summary>Provides an abstract class for specifying character data and formatting properties to be used by the <see cref="T:System.Windows.Media.TextFormatting.TextFormatter" /> object.</summary>
public abstract class TextSource
{
	private double _pixelsPerDip = Util.PixelsPerDip;

	public double PixelsPerDip
	{
		get
		{
			return _pixelsPerDip;
		}
		set
		{
			_pixelsPerDip = value;
		}
	}

	/// <summary>Retrieves a <see cref="T:System.Windows.Media.TextFormatting.TextRun" /> starting at a specified <see cref="T:System.Windows.Media.TextFormatting.TextSource" /> position.</summary>
	/// <returns>A value that represents a <see cref="T:System.Windows.Media.TextFormatting.TextRun" />, or an object derived from <see cref="T:System.Windows.Media.TextFormatting.TextRun" />.</returns>
	/// <param name="textSourceCharacterIndex">Specifies the character index position in the <see cref="T:System.Windows.Media.TextFormatting.TextSource" /> where the <see cref="T:System.Windows.Media.TextFormatting.TextRun" /> is retrieved.</param>
	public abstract TextRun GetTextRun(int textSourceCharacterIndex);

	/// <summary>Retrieves the text span immediately before the specified <see cref="T:System.Windows.Media.TextFormatting.TextSource" /> position.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextFormatting.CultureSpecificCharacterBufferRange" /> value that represents the text span immediately before <paramref name="textSourceCharacterIndexLimit" />.</returns>
	/// <param name="textSourceCharacterIndexLimit">An <see cref="T:System.Int32" /> value that specifies the character index position where text retrieval stops.</param>
	public abstract TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int textSourceCharacterIndexLimit);

	/// <summary>Retrieves a value that maps a <see cref="T:System.Windows.Media.TextFormatting.TextSource" /> character index to a <see cref="T:System.Windows.Media.TextEffect" /> character index.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that represents the <see cref="T:System.Windows.Media.TextEffect" /> character index.</returns>
	/// <param name="textSourceCharacterIndex">An <see cref="T:System.Int32" /> value that specifies the <see cref="T:System.Windows.Media.TextFormatting.TextSource" /> character index to map.</param>
	public abstract int GetTextEffectCharacterIndexFromTextSourceCharacterIndex(int textSourceCharacterIndex);

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.TextSource" /> class.</summary>
	protected TextSource()
	{
	}
}
