using System.Globalization;

namespace System.Windows.Media.TextFormatting;

/// <summary>Represents a range of characters that are associated with a culture.</summary>
public class CultureSpecificCharacterBufferRange
{
	private CultureInfo _culture;

	private CharacterBufferRange _characterBufferRange;

	/// <summary>Gets the <see cref="T:System.Globalization.CultureInfo" /> of the <see cref="T:System.Windows.Media.TextFormatting.CultureSpecificCharacterBufferRange" />.</summary>
	/// <returns>A value of type <see cref="T:System.Globalization.CultureInfo" />.</returns>
	public CultureInfo CultureInfo => _culture;

	/// <summary>Gets the <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferRange" /> of the <see cref="T:System.Windows.Media.TextFormatting.CultureSpecificCharacterBufferRange" />.</summary>
	/// <returns>A value of type <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferRange" />.</returns>
	public CharacterBufferRange CharacterBufferRange => _characterBufferRange;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.CultureSpecificCharacterBufferRange" /> class.</summary>
	/// <param name="culture">A value of <see cref="T:System.Globalization.CultureInfo" /> that represents the culture of the containing range of characters.</param>
	/// <param name="characterBufferRange">A value of <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferRange" /> that represents the range of characters.</param>
	public CultureSpecificCharacterBufferRange(CultureInfo culture, CharacterBufferRange characterBufferRange)
	{
		_culture = culture;
		_characterBufferRange = characterBufferRange;
	}
}
