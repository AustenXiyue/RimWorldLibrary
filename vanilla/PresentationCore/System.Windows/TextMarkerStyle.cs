namespace System.Windows;

/// <summary>Describes the appearance of a list item's bullet style.</summary>
public enum TextMarkerStyle
{
	/// <summary>No marker is displayed.</summary>
	None,
	/// <summary>A solid disc circle is displayed.</summary>
	Disc,
	/// <summary>A hollow disc circle is displayed.</summary>
	Circle,
	/// <summary>A hollow square shape is displayed.</summary>
	Square,
	/// <summary>A solid square box is displayed.</summary>
	Box,
	/// <summary>A lowercase Roman numeral is displayed, starting with the numeral i, for example, i, ii, iii, and iv. The <see cref="T:System.Windows.TextMarkerStyle" /> is automatically incremented for each item added to the list.</summary>
	LowerRoman,
	/// <summary>An uppercase Roman numeral is displayed, starting with the numeral I, for example, I, II, III, and IV. The <see cref="T:System.Windows.TextMarkerStyle" /> is automatically incremented for each item added to the list.</summary>
	UpperRoman,
	/// <summary>A lowercase ASCII character is displayed, starting with the letter a, for example, a, b, and c. The <see cref="T:System.Windows.TextMarkerStyle" /> is automatically incremented for each item added to the list.</summary>
	LowerLatin,
	/// <summary>An uppercase ASCII character is displayed, starting with the letter A, for example, A, B, and C. The <see cref="T:System.Windows.TextMarkerStyle" /> is automatically incremented for each item added to the list.</summary>
	UpperLatin,
	/// <summary>A decimal is displayed, starting with the number one, for example, 1, 2, and 3. The <see cref="T:System.Windows.TextMarkerStyle" /> is automatically incremented for each item added to the list.</summary>
	Decimal
}
