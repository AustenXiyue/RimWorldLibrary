using System.Collections.Generic;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Provides functionality for physical and composite font embedding.</summary>
public class FontEmbeddingManager
{
	private class UriComparer : IEqualityComparer<Uri>
	{
		public bool Equals(Uri x, Uri y)
		{
			return string.Equals(x.ToString(), y.ToString(), StringComparison.OrdinalIgnoreCase);
		}

		public int GetHashCode(Uri obj)
		{
			return obj.GetHashCode();
		}
	}

	private Dictionary<Uri, Dictionary<ushort, bool>> _collectedGlyphTypefaces;

	private static UriComparer _uriComparer = new UriComparer();

	/// <summary>Returns the collection of glyph typefaces used by the <see cref="T:System.Windows.Media.GlyphRun" /> specified in the <see cref="M:System.Windows.Media.FontEmbeddingManager.RecordUsage(System.Windows.Media.GlyphRun)" /> method.</summary>
	/// <returns>A collection of <see cref="T:System.Uri" /> values that represent glyph typefaces.</returns>
	[CLSCompliant(false)]
	public ICollection<Uri> GlyphTypefaceUris => _collectedGlyphTypefaces.Keys;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.FontEmbeddingManager" /> class.</summary>
	public FontEmbeddingManager()
	{
		_collectedGlyphTypefaces = new Dictionary<Uri, Dictionary<ushort, bool>>(_uriComparer);
	}

	/// <summary>Initiates the collection of usage information about the glyph typeface and indices used by a specified <see cref="T:System.Windows.Media.GlyphRun" />.</summary>
	/// <param name="glyphRun">The <see cref="T:System.Windows.Media.GlyphRun" /> whose usage information is collected.</param>
	public void RecordUsage(GlyphRun glyphRun)
	{
		if (glyphRun == null)
		{
			throw new ArgumentNullException("glyphRun");
		}
		Uri fontUri = glyphRun.GlyphTypeface.FontUri;
		Dictionary<ushort, bool> dictionary;
		if (_collectedGlyphTypefaces.ContainsKey(fontUri))
		{
			dictionary = _collectedGlyphTypefaces[fontUri];
		}
		else
		{
			Dictionary<ushort, bool> dictionary3 = (_collectedGlyphTypefaces[fontUri] = new Dictionary<ushort, bool>());
			dictionary = dictionary3;
		}
		foreach (ushort glyphIndex in glyphRun.GlyphIndices)
		{
			dictionary[glyphIndex] = true;
		}
	}

	/// <summary>Retrieves the list of glyphs used by the glyph typeface.</summary>
	/// <returns>A collection of <see cref="T:System.UInt16" /> values that represent the glyphs.</returns>
	/// <param name="glyphTypeface">A <see cref="T:System.Uri" /> value that represents the location of the glyph typeface containing the glyphs.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="glyphTypeface" /> value does not reference a previously recorded glyph typeface.</exception>
	[CLSCompliant(false)]
	public ICollection<ushort> GetUsedGlyphs(Uri glyphTypeface)
	{
		return (_collectedGlyphTypefaces[glyphTypeface] ?? throw new ArgumentException(SR.GlyphTypefaceNotRecorded, "glyphTypeface")).Keys;
	}
}
