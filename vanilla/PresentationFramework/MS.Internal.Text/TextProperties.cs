using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace MS.Internal.Text;

internal sealed class TextProperties : TextRunProperties
{
	private Typeface _typeface;

	private double _fontSize;

	private Brush _foreground;

	private TextEffectCollection _textEffects;

	private TextDecorationCollection _textDecorations;

	private BaselineAlignment _baselineAlignment;

	private Brush _backgroundBrush;

	private CultureInfo _cultureInfo;

	private NumberSubstitution _numberSubstitution;

	private TextRunTypographyProperties _typographyProperties;

	public override Typeface Typeface => _typeface;

	public override double FontRenderingEmSize
	{
		get
		{
			double offset = _fontSize;
			TextDpi.EnsureValidLineOffset(ref offset);
			return offset;
		}
	}

	public override double FontHintingEmSize => 12.0;

	public override TextDecorationCollection TextDecorations => _textDecorations;

	public override Brush ForegroundBrush => _foreground;

	public override Brush BackgroundBrush => _backgroundBrush;

	public override BaselineAlignment BaselineAlignment => _baselineAlignment;

	public override CultureInfo CultureInfo => _cultureInfo;

	public override NumberSubstitution NumberSubstitution => _numberSubstitution;

	public override TextRunTypographyProperties TypographyProperties => _typographyProperties;

	public override TextEffectCollection TextEffects => _textEffects;

	internal TextProperties(FrameworkElement target, bool isTypographyDefaultValue)
	{
		if (!target.HasNumberSubstitutionChanged)
		{
			_numberSubstitution = FrameworkElement.DefaultNumberSubstitution;
		}
		base.PixelsPerDip = target.GetDpi().PixelsPerDip;
		InitCommon(target);
		if (!isTypographyDefaultValue)
		{
			_typographyProperties = TextElement.GetTypographyProperties(target);
		}
		else
		{
			_typographyProperties = Typography.Default;
		}
		_baselineAlignment = BaselineAlignment.Baseline;
	}

	internal TextProperties(DependencyObject target, StaticTextPointer position, bool inlineObjects, bool getBackground, double pixelsPerDip)
	{
		if (target is FrameworkContentElement frameworkContentElement)
		{
			if (!frameworkContentElement.HasNumberSubstitutionChanged)
			{
				_numberSubstitution = FrameworkContentElement.DefaultNumberSubstitution;
			}
		}
		else if (target is FrameworkElement { HasNumberSubstitutionChanged: false })
		{
			_numberSubstitution = FrameworkElement.DefaultNumberSubstitution;
		}
		base.PixelsPerDip = pixelsPerDip;
		InitCommon(target);
		_typographyProperties = GetTypographyProperties(target);
		if (!inlineObjects)
		{
			_baselineAlignment = DynamicPropertyReader.GetBaselineAlignment(target);
			if (!position.IsNull)
			{
				TextDecorationCollection highlightTextDecorations = GetHighlightTextDecorations(position);
				if (highlightTextDecorations != null)
				{
					_textDecorations = highlightTextDecorations;
				}
			}
			if (getBackground)
			{
				_backgroundBrush = DynamicPropertyReader.GetBackgroundBrush(target);
			}
		}
		else
		{
			_baselineAlignment = DynamicPropertyReader.GetBaselineAlignmentForInlineObject(target);
			_textDecorations = DynamicPropertyReader.GetTextDecorationsForInlineObject(target, _textDecorations);
			if (getBackground)
			{
				_backgroundBrush = DynamicPropertyReader.GetBackgroundBrushForInlineObject(position);
			}
		}
	}

	internal TextProperties(TextProperties source, TextDecorationCollection textDecorations)
	{
		_backgroundBrush = source._backgroundBrush;
		_typeface = source._typeface;
		_fontSize = source._fontSize;
		_foreground = source._foreground;
		_textEffects = source._textEffects;
		_cultureInfo = source._cultureInfo;
		_numberSubstitution = source._numberSubstitution;
		_typographyProperties = source._typographyProperties;
		_baselineAlignment = source._baselineAlignment;
		base.PixelsPerDip = source.PixelsPerDip;
		_textDecorations = textDecorations;
	}

	private void InitCommon(DependencyObject target)
	{
		_typeface = DynamicPropertyReader.GetTypeface(target);
		_fontSize = (double)target.GetValue(TextElement.FontSizeProperty);
		_foreground = (Brush)target.GetValue(TextElement.ForegroundProperty);
		_textEffects = DynamicPropertyReader.GetTextEffects(target);
		_cultureInfo = DynamicPropertyReader.GetCultureInfo(target);
		_textDecorations = DynamicPropertyReader.GetTextDecorations(target);
		if (_numberSubstitution == null)
		{
			_numberSubstitution = DynamicPropertyReader.GetNumberSubstitution(target);
		}
	}

	private static TextDecorationCollection GetHighlightTextDecorations(StaticTextPointer highlightPosition)
	{
		TextDecorationCollection result = null;
		Highlights highlights = highlightPosition.TextContainer.Highlights;
		if (highlights == null)
		{
			return result;
		}
		return highlights.GetHighlightValue(highlightPosition, LogicalDirection.Forward, typeof(SpellerHighlightLayer)) as TextDecorationCollection;
	}

	private static TypographyProperties GetTypographyProperties(DependencyObject element)
	{
		if (element is TextBlock textBlock)
		{
			if (!textBlock.IsTypographyDefaultValue)
			{
				return TextElement.GetTypographyProperties(element);
			}
			return Typography.Default;
		}
		if (element is TextBox textBox)
		{
			if (!textBox.IsTypographyDefaultValue)
			{
				return TextElement.GetTypographyProperties(element);
			}
			return Typography.Default;
		}
		if (element is TextElement textElement)
		{
			return textElement.TypographyPropertiesGroup;
		}
		if (element is FlowDocument flowDocument)
		{
			return flowDocument.TypographyPropertiesGroup;
		}
		return Typography.Default;
	}

	internal void SetBackgroundBrush(Brush backgroundBrush)
	{
		_backgroundBrush = backgroundBrush;
	}

	internal void SetForegroundBrush(Brush foregroundBrush)
	{
		_foreground = foregroundBrush;
	}
}
