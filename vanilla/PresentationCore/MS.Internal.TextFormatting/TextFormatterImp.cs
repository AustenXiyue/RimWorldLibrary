using System;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using MS.Internal.FontCache;
using MS.Internal.PresentationCore;
using MS.Internal.Shaping;
using MS.Internal.Text.TextInterface;
using MS.Utility;

namespace MS.Internal.TextFormatting;

internal sealed class TextFormatterImp : TextFormatter
{
	private FrugalStructList<TextFormatterContext> _contextList;

	private bool _multipleContextProhibited;

	private GlyphingCache _glyphingCache;

	private TextFormattingMode _textFormattingMode;

	private TextAnalyzer _textAnalyzer;

	private const int MaxGlyphingCacheCapacity = 16;

	internal TextFormattingMode TextFormattingMode => _textFormattingMode;

	internal static double ToIdeal => 300.0;

	internal GlyphingCache GlyphingCache
	{
		get
		{
			if (_glyphingCache == null)
			{
				_glyphingCache = new GlyphingCache(16);
			}
			return _glyphingCache;
		}
	}

	internal TextAnalyzer TextAnalyzer
	{
		get
		{
			if (_textAnalyzer == null)
			{
				_textAnalyzer = DWriteFactory.Instance.CreateTextAnalyzer();
			}
			return _textAnalyzer;
		}
	}

	internal TextFormatterImp(TextFormattingMode textFormattingMode)
		: this(null, textFormattingMode)
	{
	}

	internal TextFormatterImp()
		: this(null, TextFormattingMode.Ideal)
	{
	}

	internal TextFormatterImp(TextFormatterContext soleContext, TextFormattingMode textFormattingMode)
	{
		_textFormattingMode = textFormattingMode;
		if (soleContext != null)
		{
			_contextList.Add(soleContext);
		}
		_multipleContextProhibited = _contextList.Count != 0;
	}

	~TextFormatterImp()
	{
		CleanupInternal();
	}

	public override void Dispose()
	{
		CleanupInternal();
		base.Dispose();
		GC.SuppressFinalize(this);
	}

	private void CleanupInternal()
	{
		for (int i = 0; i < _contextList.Count; i++)
		{
			_contextList[i].Destroy();
		}
		_contextList.Clear();
	}

	public override TextLine FormatLine(TextSource textSource, int firstCharIndex, double paragraphWidth, TextParagraphProperties paragraphProperties, TextLineBreak previousLineBreak)
	{
		return FormatLineInternal(textSource, firstCharIndex, 0, paragraphWidth, paragraphProperties, previousLineBreak, new TextRunCache());
	}

	public override TextLine FormatLine(TextSource textSource, int firstCharIndex, double paragraphWidth, TextParagraphProperties paragraphProperties, TextLineBreak previousLineBreak, TextRunCache textRunCache)
	{
		return FormatLineInternal(textSource, firstCharIndex, 0, paragraphWidth, paragraphProperties, previousLineBreak, textRunCache);
	}

	internal override TextLine RecreateLine(TextSource textSource, int firstCharIndex, int lineLength, double paragraphWidth, TextParagraphProperties paragraphProperties, TextLineBreak previousLineBreak, TextRunCache textRunCache)
	{
		return FormatLineInternal(textSource, firstCharIndex, lineLength, paragraphWidth, paragraphProperties, previousLineBreak, textRunCache);
	}

	private TextLine FormatLineInternal(TextSource textSource, int firstCharIndex, int lineLength, double paragraphWidth, TextParagraphProperties paragraphProperties, TextLineBreak previousLineBreak, TextRunCache textRunCache)
	{
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordText, EventTrace.Level.Verbose, EventTrace.Event.WClientStringBegin, "TextFormatterImp.FormatLineInternal Start");
		FormatSettings formatSettings = PrepareFormatSettings(textSource, firstCharIndex, paragraphWidth, paragraphProperties, previousLineBreak, textRunCache, lineLength != 0, isSingleLineFormatting: true, _textFormattingMode);
		TextLine textLine = null;
		if (!formatSettings.Pap.AlwaysCollapsible && previousLineBreak == null && lineLength <= 0)
		{
			textLine = SimpleTextLine.Create(formatSettings, firstCharIndex, RealToIdealFloor(paragraphWidth), textSource.PixelsPerDip);
		}
		if (textLine == null)
		{
			textLine = new TextMetrics.FullTextLine(formatSettings, firstCharIndex, lineLength, RealToIdealFloor(paragraphWidth), LineFlags.None);
		}
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordText, EventTrace.Level.Verbose, EventTrace.Event.WClientStringEnd, "TextFormatterImp.FormatLineInternal End");
		return textLine;
	}

	public override MinMaxParagraphWidth FormatMinMaxParagraphWidth(TextSource textSource, int firstCharIndex, TextParagraphProperties paragraphProperties)
	{
		return FormatMinMaxParagraphWidth(textSource, firstCharIndex, paragraphProperties, new TextRunCache());
	}

	public override MinMaxParagraphWidth FormatMinMaxParagraphWidth(TextSource textSource, int firstCharIndex, TextParagraphProperties paragraphProperties, TextRunCache textRunCache)
	{
		TextMetrics.FullTextLine fullTextLine = new TextMetrics.FullTextLine(PrepareFormatSettings(textSource, firstCharIndex, 0.0, paragraphProperties, null, textRunCache, useOptimalBreak: false, isSingleLineFormatting: true, _textFormattingMode), firstCharIndex, 0, 0, LineFlags.MinMax | LineFlags.KeepState);
		MinMaxParagraphWidth result = new MinMaxParagraphWidth(fullTextLine.MinWidth, fullTextLine.Width);
		fullTextLine.Dispose();
		return result;
	}

	internal override TextParagraphCache CreateParagraphCache(TextSource textSource, int firstCharIndex, double paragraphWidth, TextParagraphProperties paragraphProperties, TextLineBreak previousLineBreak, TextRunCache textRunCache)
	{
		FormatSettings formatSettings = PrepareFormatSettings(textSource, firstCharIndex, paragraphWidth, paragraphProperties, previousLineBreak, textRunCache, useOptimalBreak: true, isSingleLineFormatting: false, _textFormattingMode);
		if (!formatSettings.Pap.Wrap && formatSettings.Pap.OptimalBreak)
		{
			throw new ArgumentException(SR.OptimalParagraphMustWrap);
		}
		return new TextParagraphCache(formatSettings, firstCharIndex, RealToIdeal(paragraphWidth));
	}

	private FormatSettings PrepareFormatSettings(TextSource textSource, int firstCharIndex, double paragraphWidth, TextParagraphProperties paragraphProperties, TextLineBreak previousLineBreak, TextRunCache textRunCache, bool useOptimalBreak, bool isSingleLineFormatting, TextFormattingMode textFormattingMode)
	{
		VerifyTextFormattingArguments(textSource, firstCharIndex, paragraphWidth, paragraphProperties, textRunCache);
		if (textRunCache.Imp == null)
		{
			textRunCache.Imp = new TextRunCacheImp();
		}
		return new FormatSettings(this, textSource, textRunCache.Imp, new ParaProp(this, paragraphProperties, useOptimalBreak), previousLineBreak, isSingleLineFormatting, textFormattingMode, isSideways: false);
	}

	private void VerifyTextFormattingArguments(TextSource textSource, int firstCharIndex, double paragraphWidth, TextParagraphProperties paragraphProperties, TextRunCache textRunCache)
	{
		if (textSource == null)
		{
			throw new ArgumentNullException("textSource");
		}
		if (textRunCache == null)
		{
			throw new ArgumentNullException("textRunCache");
		}
		if (paragraphProperties == null)
		{
			throw new ArgumentNullException("paragraphProperties");
		}
		if (paragraphProperties.DefaultTextRunProperties == null)
		{
			throw new ArgumentNullException("paragraphProperties.DefaultTextRunProperties");
		}
		if (paragraphProperties.DefaultTextRunProperties.Typeface == null)
		{
			throw new ArgumentNullException("paragraphProperties.DefaultTextRunProperties.Typeface");
		}
		if (double.IsNaN(paragraphWidth))
		{
			throw new ArgumentOutOfRangeException("paragraphWidth", SR.ParameterValueCannotBeNaN);
		}
		if (double.IsInfinity(paragraphWidth))
		{
			throw new ArgumentOutOfRangeException("paragraphWidth", SR.ParameterValueCannotBeInfinity);
		}
		if (paragraphWidth < 0.0 || paragraphWidth > 3579139.4066666667)
		{
			throw new ArgumentOutOfRangeException("paragraphWidth", SR.Format(SR.ParameterMustBeBetween, 0, 3579139.4066666667));
		}
		double num = 35791.39406666667;
		if (paragraphProperties.DefaultTextRunProperties.FontRenderingEmSize < 0.0 || paragraphProperties.DefaultTextRunProperties.FontRenderingEmSize > num)
		{
			throw new ArgumentOutOfRangeException("paragraphProperties.DefaultTextRunProperties.FontRenderingEmSize", SR.Format(SR.ParameterMustBeBetween, 0, num));
		}
		if (paragraphProperties.Indent > 3579139.4066666667)
		{
			throw new ArgumentOutOfRangeException("paragraphProperties.Indent", SR.Format(SR.ParameterCannotBeGreaterThan, 3579139.4066666667));
		}
		if (paragraphProperties.LineHeight > 3579139.4066666667)
		{
			throw new ArgumentOutOfRangeException("paragraphProperties.LineHeight", SR.Format(SR.ParameterCannotBeGreaterThan, 3579139.4066666667));
		}
		if (paragraphProperties.DefaultIncrementalTab < 0.0 || paragraphProperties.DefaultIncrementalTab > 3579139.4066666667)
		{
			throw new ArgumentOutOfRangeException("paragraphProperties.DefaultIncrementalTab", SR.Format(SR.ParameterMustBeBetween, 0, 3579139.4066666667));
		}
	}

	internal static void VerifyCaretCharacterHit(CharacterHit characterHit, int cpFirst, int cchLength)
	{
		if (characterHit.FirstCharacterIndex < cpFirst || characterHit.FirstCharacterIndex > cpFirst + cchLength)
		{
			throw new ArgumentOutOfRangeException("cpFirst", SR.Format(SR.ParameterMustBeBetween, cpFirst, cpFirst + cchLength));
		}
		if (characterHit.TrailingLength < 0)
		{
			throw new ArgumentOutOfRangeException("cchLength", SR.ParameterCannotBeNegative);
		}
	}

	internal TextFormatterContext AcquireContext(object owner, nint ploc)
	{
		Invariant.Assert(owner != null);
		TextFormatterContext textFormatterContext = null;
		int count = _contextList.Count;
		int i;
		for (i = 0; i < count; i++)
		{
			textFormatterContext = _contextList[i];
			if (ploc == IntPtr.Zero)
			{
				if (textFormatterContext.Owner == null)
				{
					break;
				}
			}
			else if (ploc == textFormatterContext.Ploc.Value)
			{
				break;
			}
		}
		if (i == count)
		{
			if (count != 0 && _multipleContextProhibited)
			{
				throw new InvalidOperationException(SR.TextFormatterReentranceProhibited);
			}
			textFormatterContext = new TextFormatterContext();
			_contextList.Add(textFormatterContext);
		}
		textFormatterContext.Owner = owner;
		return textFormatterContext;
	}

	internal static MatrixTransform CreateAntiInversionTransform(InvertAxes inversion, double paragraphWidth, double lineHeight)
	{
		if (inversion == InvertAxes.None)
		{
			return null;
		}
		double num = 1.0;
		double num2 = 1.0;
		double offsetX = 0.0;
		double offsetY = 0.0;
		if ((inversion & InvertAxes.Horizontal) != 0)
		{
			num = 0.0 - num;
			offsetX = paragraphWidth;
		}
		if ((inversion & InvertAxes.Vertical) != 0)
		{
			num2 = 0.0 - num2;
			offsetY = lineHeight;
		}
		return new MatrixTransform(num, 0.0, 0.0, num2, offsetX, offsetY);
	}

	internal static int CompareReal(double x, double y, double pixelsPerDip, TextFormattingMode mode)
	{
		double num = x;
		double num2 = y;
		if (mode == TextFormattingMode.Display)
		{
			num = RoundDipForDisplayMode(x, pixelsPerDip);
			num2 = RoundDipForDisplayMode(y, pixelsPerDip);
		}
		if (num > num2)
		{
			return 1;
		}
		if (num < num2)
		{
			return -1;
		}
		return 0;
	}

	internal static double RoundDip(double value, double pixelsPerDip, TextFormattingMode textFormattingMode)
	{
		if (TextFormattingMode.Display == textFormattingMode)
		{
			return RoundDipForDisplayMode(value, pixelsPerDip);
		}
		return value;
	}

	internal static double RoundDipForDisplayMode(double value, double pixelsPerDip)
	{
		return RoundDipForDisplayMode(value, pixelsPerDip, MidpointRounding.ToEven);
	}

	private static double RoundDipForDisplayMode(double value, double pixelsPerDip, MidpointRounding midpointRounding)
	{
		return Math.Round(value * pixelsPerDip, midpointRounding) / pixelsPerDip;
	}

	internal static double RoundDipForDisplayModeJustifiedText(double value, double pixelsPerDip)
	{
		return RoundDipForDisplayMode(value, pixelsPerDip, MidpointRounding.AwayFromZero);
	}

	internal static double IdealToRealWithNoRounding(double i)
	{
		return i * (1.0 / 300.0);
	}

	internal double IdealToReal(double i, double pixelsPerDip)
	{
		double num = IdealToRealWithNoRounding(i);
		if (_textFormattingMode == TextFormattingMode.Display)
		{
			num = RoundDipForDisplayMode(num, pixelsPerDip);
		}
		if (i > 0.0)
		{
			num = Math.Max(num, 1.0 / 300.0);
		}
		return num;
	}

	internal static int RealToIdeal(double i)
	{
		int num = (int)Math.Round(i * ToIdeal);
		if (i > 0.0)
		{
			num = Math.Max(num, 1);
		}
		return num;
	}

	internal static int RealToIdealFloor(double i)
	{
		int num = (int)Math.Floor(i * ToIdeal);
		if (i > 0.0)
		{
			num = Math.Max(num, 1);
		}
		return num;
	}
}
