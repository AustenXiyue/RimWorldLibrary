namespace System.Windows;

/// <summary>Renders variant typographic glyph forms.</summary>
public enum FontVariants
{
	/// <summary>Default font behavior. Font scaling and positioning is normal.</summary>
	Normal,
	/// <summary>Replaces a default glyph with a superscript glyph. Superscript is commonly used for footnotes.</summary>
	Superscript,
	/// <summary>Replaces a default glyph with a subscript glyph.</summary>
	Subscript,
	/// <summary>Replaces a default glyph with an ordinal glyph, or it may combine glyph substitution with positioning adjustments for proper placement. Ordinal forms are normally associated with numeric notation of an ordinal word, such as "1st" for "first."</summary>
	Ordinal,
	/// <summary>Replaces a default glyph with an inferior glyph, or it may combine glyph substitution with positioning adjustments for proper placement. Inferior forms are typically used in chemical formulas or mathematical notation.</summary>
	Inferior,
	/// <summary>Replaces a default glyph with a smaller Japanese Kana glyph. This is used to clarify the meaning of Kanji, which may be unfamiliar to the reader.</summary>
	Ruby
}
