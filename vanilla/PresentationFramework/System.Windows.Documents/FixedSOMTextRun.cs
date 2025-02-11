using System.Collections.Generic;
using System.Globalization;
using System.Windows.Markup;
using System.Windows.Media;

namespace System.Windows.Documents;

internal sealed class FixedSOMTextRun : FixedSOMElement, IComparable
{
	private double _defaultCharWidth;

	private Uri _fontUri;

	private CultureInfo _cultureInfo;

	private bool _isSideways;

	private int _bidiLevel;

	private bool _isWhiteSpace;

	private bool _isReversed;

	private FixedSOMFixedBlock _fixedBlock;

	private int _lineIndex;

	private string _text;

	private Brush _foreground;

	private double _fontSize;

	private string _fontFamily;

	private FontStyle _fontStyle;

	private FontWeight _fontWeight;

	private FontStretch _fontStretch;

	public double DefaultCharWidth => _defaultCharWidth;

	public bool IsSideways => _isSideways;

	public bool IsWhiteSpace => _isWhiteSpace;

	public CultureInfo CultureInfo => _cultureInfo;

	public bool IsLTR
	{
		get
		{
			if ((_bidiLevel & 1) == 0)
			{
				return !_isReversed;
			}
			return false;
		}
	}

	public bool IsRTL => !IsLTR;

	public string Text
	{
		get
		{
			return _text;
		}
		set
		{
			_text = value;
		}
	}

	public FixedSOMFixedBlock FixedBlock
	{
		get
		{
			return _fixedBlock;
		}
		set
		{
			_fixedBlock = value;
		}
	}

	public string FontFamily => _fontFamily;

	public FontStyle FontStyle => _fontStyle;

	public FontWeight FontWeight => _fontWeight;

	public FontStretch FontStretch => _fontStretch;

	public double FontRenderingEmSize => _fontSize;

	public Brush Foreground => _foreground;

	public bool IsReversed => _isReversed;

	internal int LineIndex
	{
		get
		{
			return _lineIndex;
		}
		set
		{
			_lineIndex = value;
		}
	}

	private FixedSOMTextRun(Rect boundingRect, GeneralTransform trans, FixedNode fixedNode, int startIndex, int endIndex)
		: base(fixedNode, startIndex, endIndex, trans)
	{
		_boundingRect = trans.TransformBounds(boundingRect);
	}

	int IComparable.CompareTo(object comparedObj)
	{
		FixedSOMTextRun fixedSOMTextRun = comparedObj as FixedSOMTextRun;
		int num = 0;
		if (_fixedBlock.IsRTL)
		{
			Rect boundingRect = base.BoundingRect;
			Rect boundingRect2 = fixedSOMTextRun.BoundingRect;
			if (!base.Matrix.IsIdentity)
			{
				Matrix mat = _mat;
				mat.Invert();
				boundingRect.Transform(mat);
				boundingRect.Offset(_mat.OffsetX, _mat.OffsetY);
				boundingRect2.Transform(mat);
				boundingRect2.Offset(_mat.OffsetX, _mat.OffsetY);
			}
			boundingRect.Offset(_mat.OffsetX, _mat.OffsetY);
			boundingRect2.Offset(fixedSOMTextRun.Matrix.OffsetX, fixedSOMTextRun.Matrix.OffsetY);
			if (FixedTextBuilder.IsSameLine(boundingRect2.Top - boundingRect.Top, boundingRect.Height, boundingRect2.Height))
			{
				return (boundingRect.Left < boundingRect2.Left) ? 1 : (-1);
			}
			return (!(boundingRect.Top < boundingRect2.Top)) ? 1 : (-1);
		}
		List<FixedNode> markupOrder = FixedBlock.FixedSOMPage.MarkupOrder;
		return markupOrder.IndexOf(base.FixedNode) - markupOrder.IndexOf(fixedSOMTextRun.FixedNode);
	}

	public static FixedSOMTextRun Create(Rect boundingRect, GeneralTransform transform, Glyphs glyphs, FixedNode fixedNode, int startIndex, int endIndex, bool allowReverseGlyphs)
	{
		if (string.IsNullOrEmpty(glyphs.UnicodeString) || glyphs.FontRenderingEmSize <= 0.0)
		{
			return null;
		}
		FixedSOMTextRun fixedSOMTextRun = new FixedSOMTextRun(boundingRect, transform, fixedNode, startIndex, endIndex);
		fixedSOMTextRun._fontUri = glyphs.FontUri;
		fixedSOMTextRun._cultureInfo = glyphs.Language.GetCompatibleCulture();
		fixedSOMTextRun._bidiLevel = glyphs.BidiLevel;
		fixedSOMTextRun._isSideways = glyphs.IsSideways;
		fixedSOMTextRun._fontSize = glyphs.FontRenderingEmSize;
		GlyphRun glyphRun = glyphs.ToGlyphRun();
		GlyphTypeface glyphTypeface = glyphRun.GlyphTypeface;
		glyphTypeface.FamilyNames.TryGetValue(fixedSOMTextRun._cultureInfo, out fixedSOMTextRun._fontFamily);
		if (fixedSOMTextRun._fontFamily == null)
		{
			glyphTypeface.FamilyNames.TryGetValue(System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS, out fixedSOMTextRun._fontFamily);
		}
		fixedSOMTextRun._fontStyle = glyphTypeface.Style;
		fixedSOMTextRun._fontWeight = glyphTypeface.Weight;
		fixedSOMTextRun._fontStretch = glyphTypeface.Stretch;
		fixedSOMTextRun._defaultCharWidth = ((glyphTypeface.XHeight > 0.0) ? (glyphTypeface.XHeight * glyphs.FontRenderingEmSize) : glyphRun.AdvanceWidths[startIndex]);
		Transform affineTransform = transform.AffineTransform;
		if (affineTransform != null && !affineTransform.Value.IsIdentity)
		{
			Matrix value = affineTransform.Value;
			double num = Math.Sqrt(value.M12 * value.M12 + value.M22 * value.M22);
			double num2 = Math.Sqrt(value.M11 * value.M11 + value.M21 * value.M21);
			fixedSOMTextRun._fontSize *= num;
			fixedSOMTextRun._defaultCharWidth *= num2;
		}
		fixedSOMTextRun._foreground = glyphs.Fill;
		string unicodeString = glyphs.UnicodeString;
		fixedSOMTextRun.Text = unicodeString.Substring(startIndex, endIndex - startIndex);
		if (allowReverseGlyphs && fixedSOMTextRun._bidiLevel == 0 && !fixedSOMTextRun._isSideways && startIndex == 0 && endIndex == unicodeString.Length && string.IsNullOrEmpty(glyphs.CaretStops) && FixedTextBuilder.MostlyRTL(unicodeString))
		{
			fixedSOMTextRun._isReversed = true;
			fixedSOMTextRun.Text = string.Create(fixedSOMTextRun.Text.Length, fixedSOMTextRun.Text, delegate(Span<char> destination, string runText)
			{
				for (int i = 0; i < destination.Length; i++)
				{
					destination[i] = runText[runText.Length - 1 - i];
				}
			});
		}
		if (unicodeString == "" && glyphs.Indices != null && glyphs.Indices.Length > 0)
		{
			fixedSOMTextRun._isWhiteSpace = false;
		}
		else
		{
			fixedSOMTextRun._isWhiteSpace = true;
			for (int j = 0; j < unicodeString.Length; j++)
			{
				if (!char.IsWhiteSpace(unicodeString[j]))
				{
					fixedSOMTextRun._isWhiteSpace = false;
					break;
				}
			}
		}
		return fixedSOMTextRun;
	}

	public bool HasSameRichProperties(FixedSOMTextRun run)
	{
		if (run.FontRenderingEmSize == FontRenderingEmSize && run.CultureInfo.Equals(CultureInfo) && run.FontStyle.Equals(FontStyle) && run.FontStretch.Equals(FontStretch) && run.FontWeight.Equals(FontWeight) && run.FontFamily == FontFamily && run.IsRTL == IsRTL)
		{
			SolidColorBrush solidColorBrush = Foreground as SolidColorBrush;
			SolidColorBrush solidColorBrush2 = run.Foreground as SolidColorBrush;
			if ((run.Foreground == null && Foreground == null) || (solidColorBrush != null && solidColorBrush2 != null && solidColorBrush.Color == solidColorBrush2.Color && solidColorBrush.Opacity == solidColorBrush2.Opacity))
			{
				return true;
			}
		}
		return false;
	}

	public override void SetRTFProperties(FixedElement element)
	{
		if (_cultureInfo != null)
		{
			element.SetValue(FrameworkElement.LanguageProperty, XmlLanguage.GetLanguage(_cultureInfo.IetfLanguageTag));
		}
		element.SetValue(TextElement.FontSizeProperty, _fontSize);
		element.SetValue(TextElement.FontWeightProperty, _fontWeight);
		element.SetValue(TextElement.FontStretchProperty, _fontStretch);
		element.SetValue(TextElement.FontStyleProperty, _fontStyle);
		if (IsRTL)
		{
			element.SetValue(FrameworkElement.FlowDirectionProperty, FlowDirection.RightToLeft);
		}
		else
		{
			element.SetValue(FrameworkElement.FlowDirectionProperty, FlowDirection.LeftToRight);
		}
		if (_fontFamily != null)
		{
			element.SetValue(TextElement.FontFamilyProperty, new FontFamily(_fontFamily));
		}
		element.SetValue(TextElement.ForegroundProperty, _foreground);
	}
}
