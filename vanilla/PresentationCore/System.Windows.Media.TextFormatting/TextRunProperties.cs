using System.Globalization;
using MS.Internal.FontCache;

namespace System.Windows.Media.TextFormatting;

/// <summary>Provides a set of properties, such as typeface or foreground brush, that can be applied to a <see cref="T:System.Windows.Media.TextFormatting.TextRun" /> object. This is an abstract class.</summary>
public abstract class TextRunProperties
{
	private double _pixelsPerDip = Util.PixelsPerDip;

	/// <summary>Gets the typeface for the text run.</summary>
	/// <returns>A value of <see cref="T:System.Windows.Media.Typeface" />.</returns>
	public abstract Typeface Typeface { get; }

	/// <summary>Gets the text size in points for the text run.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the text size in DIPs (Device Independent Pixels). The default is 12 DIP.</returns>
	public abstract double FontRenderingEmSize { get; }

	/// <summary>Gets the text size in points, which is then used for font hinting.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the text size in points. The default is 12 pt.</returns>
	public abstract double FontHintingEmSize { get; }

	/// <summary>Gets the collection of  <see cref="T:System.Windows.TextDecoration" /> objects used for the text run.</summary>
	/// <returns>A <see cref="T:System.Windows.TextDecorationCollection" /> value.</returns>
	public abstract TextDecorationCollection TextDecorations { get; }

	/// <summary>Gets the brush that is used to paint the foreground color of the text run.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Brush" /> value that represents the foreground color.</returns>
	public abstract Brush ForegroundBrush { get; }

	/// <summary>Gets the brush that is used to paint the background color of the text run.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Brush" /> value that represents the background color.</returns>
	public abstract Brush BackgroundBrush { get; }

	/// <summary>Gets the culture information for the text run.</summary>
	/// <returns>A value of <see cref="T:System.Globalization.CultureInfo" /> that represents the culture of the text run.</returns>
	public abstract CultureInfo CultureInfo { get; }

	/// <summary>Gets the collection of <see cref="T:System.Windows.Media.TextEffect" /> objects used for the text run.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextEffectCollection" /> value.</returns>
	public abstract TextEffectCollection TextEffects { get; }

	/// <summary>Gets the baseline style for a text that is positioned on the vertical axis.</summary>
	/// <returns>An enumerated value of <see cref="T:System.Windows.BaselineAlignment" />.</returns>
	public virtual BaselineAlignment BaselineAlignment => BaselineAlignment.Baseline;

	/// <summary>Gets the typography properties for the text run.</summary>
	/// <returns>A value of <see cref="T:System.Windows.Media.TextFormatting.TextRunTypographyProperties" />.</returns>
	public virtual TextRunTypographyProperties TypographyProperties => null;

	/// <summary>Gets the number substitution settings, which determines who numbers in text are displayed in different cultures.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.NumberSubstitution" /> value.</returns>
	public virtual NumberSubstitution NumberSubstitution => null;

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

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.TextRunProperties" /> class.</summary>
	protected TextRunProperties()
	{
	}
}
