namespace System.Windows.Media.TextFormatting;

/// <summary>Allows text engine clients to map a text source character index to the corresponding <see cref="T:System.Windows.Media.GlyphRun" />.</summary>
public sealed class IndexedGlyphRun
{
	private GlyphRun _glyphRun;

	private int _textSourceCharacterIndex;

	private int _length;

	/// <summary>Gets the text source character index that corresponds to the beginning of the <see cref="T:System.Windows.Media.GlyphRun" />.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that represents the text source character index.</returns>
	public int TextSourceCharacterIndex => _textSourceCharacterIndex;

	/// <summary>Gets the text source character length that corresponds to the <see cref="T:System.Windows.Media.TextFormatting.IndexedGlyphRun" /> object.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that represents the length of the text source character.</returns>
	public int TextSourceLength => _length;

	/// <summary>Gets the <see cref="T:System.Windows.Media.GlyphRun" /> that corresponds to the <see cref="T:System.Windows.Media.TextFormatting.IndexedGlyphRun" /> object.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.GlyphRun" /> object.</returns>
	public GlyphRun GlyphRun => _glyphRun;

	internal IndexedGlyphRun(int textSourceCharacterIndex, int textSourceCharacterLength, GlyphRun glyphRun)
	{
		_textSourceCharacterIndex = textSourceCharacterIndex;
		_length = textSourceCharacterLength;
		_glyphRun = glyphRun;
	}
}
