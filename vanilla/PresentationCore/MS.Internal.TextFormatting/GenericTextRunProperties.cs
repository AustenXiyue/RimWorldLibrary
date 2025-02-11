using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace MS.Internal.TextFormatting;

internal sealed class GenericTextRunProperties : TextRunProperties
{
	private Typeface _typeface;

	private double _emSize;

	private double _emHintingSize;

	private TextDecorationCollection _textDecorations;

	private Brush _foregroundBrush;

	private Brush _backgroundBrush;

	private BaselineAlignment _baselineAlignment;

	private CultureInfo _culture;

	private NumberSubstitution _numberSubstitution;

	public override Typeface Typeface => _typeface;

	public override double FontRenderingEmSize => _emSize;

	public override double FontHintingEmSize => _emHintingSize;

	public override TextDecorationCollection TextDecorations => _textDecorations;

	public override Brush ForegroundBrush => _foregroundBrush;

	public override Brush BackgroundBrush => _backgroundBrush;

	public override BaselineAlignment BaselineAlignment => _baselineAlignment;

	public override CultureInfo CultureInfo => _culture;

	public override TextRunTypographyProperties TypographyProperties => null;

	public override TextEffectCollection TextEffects => null;

	public override NumberSubstitution NumberSubstitution => _numberSubstitution;

	public GenericTextRunProperties(Typeface typeface, double size, double hintingSize, double pixelsPerDip, TextDecorationCollection textDecorations, Brush foregroundBrush, Brush backgroundBrush, BaselineAlignment baselineAlignment, CultureInfo culture, NumberSubstitution substitution)
	{
		_typeface = typeface;
		_emSize = size;
		_emHintingSize = hintingSize;
		_textDecorations = textDecorations;
		_foregroundBrush = foregroundBrush;
		_backgroundBrush = backgroundBrush;
		_baselineAlignment = baselineAlignment;
		_culture = culture;
		_numberSubstitution = substitution;
		base.PixelsPerDip = pixelsPerDip;
	}

	public override int GetHashCode()
	{
		return _typeface.GetHashCode() ^ _emSize.GetHashCode() ^ _emHintingSize.GetHashCode() ^ ((_foregroundBrush != null) ? _foregroundBrush.GetHashCode() : 0) ^ ((_backgroundBrush != null) ? _backgroundBrush.GetHashCode() : 0) ^ ((_textDecorations != null) ? _textDecorations.GetHashCode() : 0) ^ ((int)_baselineAlignment << 3) ^ (_culture.GetHashCode() << 6) ^ ((_numberSubstitution != null) ? _numberSubstitution.GetHashCode() : 0);
	}

	public override bool Equals(object o)
	{
		if (o == null || !(o is TextRunProperties))
		{
			return false;
		}
		TextRunProperties textRunProperties = (TextRunProperties)o;
		if (_emSize == textRunProperties.FontRenderingEmSize && _emHintingSize == textRunProperties.FontHintingEmSize && _culture == textRunProperties.CultureInfo && _typeface.Equals(textRunProperties.Typeface) && ((_textDecorations == null) ? (textRunProperties.TextDecorations == null) : _textDecorations.ValueEquals(textRunProperties.TextDecorations)) && _baselineAlignment == textRunProperties.BaselineAlignment && ((_foregroundBrush == null) ? (textRunProperties.ForegroundBrush == null) : _foregroundBrush.Equals(textRunProperties.ForegroundBrush)) && ((_backgroundBrush == null) ? (textRunProperties.BackgroundBrush == null) : _backgroundBrush.Equals(textRunProperties.BackgroundBrush)))
		{
			if (_numberSubstitution != null)
			{
				return _numberSubstitution.Equals(textRunProperties.NumberSubstitution);
			}
			return textRunProperties.NumberSubstitution == null;
		}
		return false;
	}
}
