using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Media.TextFormatting;
using MS.Internal;
using MS.Internal.FontCache;
using MS.Internal.PresentationCore;
using MS.Internal.TextFormatting;

namespace System.Windows.Media;

/// <summary>Provides low-level control for drawing text in Windows Presentation Foundation (WPF) applications.</summary>
public class FormattedText
{
	private struct LineEnumerator : IEnumerator, IDisposable
	{
		private int _textStorePosition;

		private int _lineCount;

		private double _totalHeight;

		private TextLine _currentLine;

		private TextLine _nextLine;

		private TextFormatter _formatter;

		private FormattedText _that;

		private double _previousHeight;

		private int _previousLength;

		private TextLineBreak _previousLineBreak;

		internal int Position => _textStorePosition;

		internal int Length => _previousLength;

		public TextLine Current => _currentLine;

		object IEnumerator.Current => Current;

		internal double CurrentParagraphWidth => MaxLineLength(_lineCount);

		internal LineEnumerator(FormattedText text)
		{
			_previousHeight = 0.0;
			_previousLength = 0;
			_previousLineBreak = null;
			_textStorePosition = 0;
			_lineCount = 0;
			_totalHeight = 0.0;
			_currentLine = null;
			_nextLine = null;
			_formatter = TextFormatter.FromCurrentDispatcher(text._textFormattingMode);
			_that = text;
			if (_that._textSourceImpl == null)
			{
				_that._textSourceImpl = new TextSourceImplementation(_that);
			}
		}

		public void Dispose()
		{
			if (_currentLine != null)
			{
				_currentLine.Dispose();
				_currentLine = null;
			}
			if (_nextLine != null)
			{
				_nextLine.Dispose();
				_nextLine = null;
			}
		}

		private double MaxLineLength(int line)
		{
			if (_that._maxTextWidths == null)
			{
				return _that._maxTextWidth;
			}
			return _that._maxTextWidths[Math.Min(line, _that._maxTextWidths.Length - 1)];
		}

		public bool MoveNext()
		{
			if (_currentLine == null)
			{
				if (_that._text.Length == 0)
				{
					return false;
				}
				_currentLine = FormatLine(_that._textSourceImpl, _textStorePosition, MaxLineLength(_lineCount), _that._defaultParaProps, null);
				if (_totalHeight + _currentLine.Height > _that._maxTextHeight)
				{
					_currentLine.Dispose();
					_currentLine = null;
					return false;
				}
			}
			else
			{
				if (_nextLine == null)
				{
					return false;
				}
				_totalHeight += _previousHeight;
				_textStorePosition += _previousLength;
				_lineCount++;
				_currentLine = _nextLine;
				_nextLine = null;
			}
			TextLineBreak textLineBreak = _currentLine.GetTextLineBreak();
			if (_textStorePosition + _currentLine.Length < _that._text.Length)
			{
				bool flag;
				if (_lineCount + 1 >= _that._maxLineCount)
				{
					flag = false;
				}
				else
				{
					_nextLine = FormatLine(_that._textSourceImpl, _textStorePosition + _currentLine.Length, MaxLineLength(_lineCount + 1), _that._defaultParaProps, textLineBreak);
					flag = _totalHeight + _currentLine.Height + _nextLine.Height <= _that._maxTextHeight;
				}
				if (!flag)
				{
					if (_nextLine != null)
					{
						_nextLine.Dispose();
						_nextLine = null;
					}
					if (_that._trimming != 0 && !_currentLine.HasCollapsed)
					{
						TextWrapping textWrapping = _that._defaultParaProps.TextWrapping;
						_that._defaultParaProps.SetTextWrapping(TextWrapping.NoWrap);
						textLineBreak?.Dispose();
						_currentLine.Dispose();
						_currentLine = FormatLine(_that._textSourceImpl, _textStorePosition, MaxLineLength(_lineCount), _that._defaultParaProps, _previousLineBreak);
						textLineBreak = _currentLine.GetTextLineBreak();
						_that._defaultParaProps.SetTextWrapping(textWrapping);
					}
				}
			}
			_previousHeight = _currentLine.Height;
			_previousLength = _currentLine.Length;
			if (_previousLineBreak != null)
			{
				_previousLineBreak.Dispose();
			}
			_previousLineBreak = textLineBreak;
			return true;
		}

		private TextLine FormatLine(TextSource textSource, int textSourcePosition, double maxLineLength, TextParagraphProperties paraProps, TextLineBreak lineBreak)
		{
			TextLine textLine = _formatter.FormatLine(textSource, textSourcePosition, maxLineLength, paraProps, lineBreak);
			if (_that._trimming != 0 && textLine.HasOverflowed && textLine.Length > 0)
			{
				GenericTextRunProperties textRunProperties = new SpanRider(_that._formatRuns, _that._latestPosition, Math.Min(textSourcePosition + textLine.Length - 1, _that._text.Length - 1)).CurrentElement as GenericTextRunProperties;
				TextCollapsingProperties textCollapsingProperties = ((_that._trimming != TextTrimming.CharacterEllipsis) ? ((TextCollapsingProperties)new TextTrailingWordEllipsis(maxLineLength, textRunProperties)) : ((TextCollapsingProperties)new TextTrailingCharacterEllipsis(maxLineLength, textRunProperties)));
				TextLine textLine2 = textLine.Collapse(textCollapsingProperties);
				if (textLine2 != textLine)
				{
					textLine.Dispose();
					textLine = textLine2;
				}
			}
			return textLine;
		}

		public void Reset()
		{
			_textStorePosition = 0;
			_lineCount = 0;
			_totalHeight = 0.0;
			_currentLine = null;
			_nextLine = null;
		}
	}

	private class CachedMetrics
	{
		public double Height;

		public double Baseline;

		public double Width;

		public double WidthIncludingTrailingWhitespace;

		public double Extent;

		public double OverhangAfter;

		public double OverhangLeading;

		public double OverhangTrailing;
	}

	private class TextSourceImplementation : TextSource
	{
		private FormattedText _that;

		public TextSourceImplementation(FormattedText text)
		{
			_that = text;
			base.PixelsPerDip = _that.PixelsPerDip;
		}

		public override TextRun GetTextRun(int textSourceCharacterIndex)
		{
			if (textSourceCharacterIndex >= _that._text.Length)
			{
				return new TextEndOfParagraph(1);
			}
			SpanRider spanRider = new SpanRider(_that._formatRuns, _that._latestPosition, textSourceCharacterIndex);
			TextRunProperties textRunProperties = spanRider.CurrentElement as GenericTextRunProperties;
			TextCharacters result = new TextCharacters(_that._text, textSourceCharacterIndex, spanRider.Length, textRunProperties);
			textRunProperties.PixelsPerDip = base.PixelsPerDip;
			return result;
		}

		public override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int textSourceCharacterIndexLimit)
		{
			CharacterBufferRange characterBufferRange = CharacterBufferRange.Empty;
			CultureInfo culture = null;
			if (textSourceCharacterIndexLimit > 0)
			{
				SpanRider spanRider = new SpanRider(_that._formatRuns, _that._latestPosition, textSourceCharacterIndexLimit - 1);
				characterBufferRange = new CharacterBufferRange(new CharacterBufferReference(_that._text, spanRider.CurrentSpanStart), textSourceCharacterIndexLimit - spanRider.CurrentSpanStart);
				culture = ((TextRunProperties)spanRider.CurrentElement).CultureInfo;
			}
			return new TextSpan<CultureSpecificCharacterBufferRange>(characterBufferRange.Length, new CultureSpecificCharacterBufferRange(culture, characterBufferRange));
		}

		public override int GetTextEffectCharacterIndexFromTextSourceCharacterIndex(int textSourceCharacterIndex)
		{
			throw new NotSupportedException();
		}
	}

	private string _text;

	private double _pixelsPerDip = Util.PixelsPerDip;

	private SpanVector _formatRuns = new SpanVector(null);

	private SpanPosition _latestPosition;

	private GenericTextParagraphProperties _defaultParaProps;

	private double _maxTextWidth;

	private double[] _maxTextWidths;

	private double _maxTextHeight = double.MaxValue;

	private int _maxLineCount = int.MaxValue;

	private TextTrimming _trimming = TextTrimming.WordEllipsis;

	private TextFormattingMode _textFormattingMode;

	private TextSourceImplementation _textSourceImpl;

	private CachedMetrics _metrics;

	private double _minWidth;

	private const double MaxFontEmSize = 35791.39406666667;

	/// <summary>Gets the string of text to be displayed.</summary>
	/// <returns>The string of text to be displayed.</returns>
	public string Text => _text;

	public double PixelsPerDip
	{
		get
		{
			return _pixelsPerDip;
		}
		set
		{
			_pixelsPerDip = value;
			_defaultParaProps.DefaultTextRunProperties.PixelsPerDip = _pixelsPerDip;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.FlowDirection" /> of a <see cref="T:System.Windows.Media.FormattedText" /> object.</summary>
	/// <returns>The <see cref="T:System.Windows.FlowDirection" /> of the formatted text.</returns>
	public FlowDirection FlowDirection
	{
		get
		{
			return _defaultParaProps.FlowDirection;
		}
		set
		{
			ValidateFlowDirection(value, "value");
			_defaultParaProps.SetFlowDirection(value);
			InvalidateMetrics();
		}
	}

	/// <summary>Gets or sets the alignment of text within a <see cref="T:System.Windows.Media.FormattedText" /> object.</summary>
	/// <returns>One of the <see cref="T:System.Windows.TextAlignment" /> values that specifies the alignment of text within a <see cref="T:System.Windows.Media.FormattedText" /> object.</returns>
	public TextAlignment TextAlignment
	{
		get
		{
			return _defaultParaProps.TextAlignment;
		}
		set
		{
			_defaultParaProps.SetTextAlignment(value);
			InvalidateMetrics();
		}
	}

	/// <summary>Gets the line height, or line spacing, between lines of text.</summary>
	/// <returns>The line spacing between lines of text, provided in device-independent units (1/96th inch per unit).</returns>
	public double LineHeight
	{
		get
		{
			return _defaultParaProps.LineHeight;
		}
		set
		{
			if (value < 0.0)
			{
				throw new ArgumentOutOfRangeException("value", SR.ParameterCannotBeNegative);
			}
			_defaultParaProps.SetLineHeight(value);
			InvalidateMetrics();
		}
	}

	/// <summary>Gets or sets the maximum text width (length) for a line of text.</summary>
	/// <returns>The maximum text width for a line of text, provided in device-independent units (1/96th inch per unit).</returns>
	public double MaxTextWidth
	{
		get
		{
			return _maxTextWidth;
		}
		set
		{
			if (value < 0.0)
			{
				throw new ArgumentOutOfRangeException("value", SR.ParameterCannotBeNegative);
			}
			_maxTextWidth = value;
			InvalidateMetrics();
		}
	}

	/// <summary>Gets or sets the maximum height of a text column.</summary>
	/// <returns>The maximum height of a text column, provided in device-independent units (1/96th inch per unit).</returns>
	public double MaxTextHeight
	{
		get
		{
			return _maxTextHeight;
		}
		set
		{
			if (value <= 0.0)
			{
				throw new ArgumentOutOfRangeException("value", SR.Format(SR.PropertyMustBeGreaterThanZero, "MaxTextHeight"));
			}
			if (double.IsNaN(value))
			{
				throw new ArgumentOutOfRangeException("value", SR.Format(SR.PropertyValueCannotBeNaN, "MaxTextHeight"));
			}
			_maxTextHeight = value;
			InvalidateMetrics();
		}
	}

	/// <summary>Gets or sets the maximum number of lines to display. Text exceeding the <see cref="P:System.Windows.Media.FormattedText.MaxLineCount" />  will not be displayed. </summary>
	/// <returns>The maximum number of lines to display.</returns>
	public int MaxLineCount
	{
		get
		{
			return _maxLineCount;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentOutOfRangeException("value", SR.ParameterMustBeGreaterThanZero);
			}
			_maxLineCount = value;
			InvalidateMetrics();
		}
	}

	/// <summary>Gets or sets the means by which the omission of text is indicated.</summary>
	/// <returns>One of the <see cref="T:System.Windows.TextTrimming" /> values that specifies how the omission of text is indicated. The default is <see cref="F:System.Windows.TextTrimming.WordEllipsis" />.</returns>
	public TextTrimming Trimming
	{
		get
		{
			return _trimming;
		}
		set
		{
			if (value < TextTrimming.None || value > TextTrimming.WordEllipsis)
			{
				throw new InvalidEnumArgumentException("value", (int)value, typeof(TextTrimming));
			}
			_trimming = value;
			if (_trimming == TextTrimming.None)
			{
				_defaultParaProps.SetTextWrapping(TextWrapping.Wrap);
			}
			else
			{
				_defaultParaProps.SetTextWrapping(TextWrapping.WrapWithOverflow);
			}
			InvalidateMetrics();
		}
	}

	private CachedMetrics Metrics
	{
		get
		{
			if (_metrics == null)
			{
				_metrics = DrawAndCalculateMetrics(null, default(Point), getBlackBoxMetrics: false);
			}
			return _metrics;
		}
	}

	private CachedMetrics BlackBoxMetrics
	{
		get
		{
			if (_metrics == null || double.IsNaN(_metrics.Extent))
			{
				_metrics = DrawAndCalculateMetrics(null, default(Point), getBlackBoxMetrics: true);
			}
			return _metrics;
		}
	}

	/// <summary>Gets the distance from the top of the first line to the bottom of the last line of the <see cref="T:System.Windows.Media.FormattedText" /> object.</summary>
	/// <returns>The distance from the top of the first line to the bottom of the last line, provided in device-independent units (1/96th inch per unit).</returns>
	public double Height => Metrics.Height;

	/// <summary>Gets the distance from the topmost drawn pixel of the first line to the bottommost drawn pixel of the last line.</summary>
	/// <returns>The distance from the topmost drawn pixel of the first line to the bottommost drawn pixel of the last line, provided in device-independent units (1/96th inch per unit).</returns>
	public double Extent => BlackBoxMetrics.Extent;

	/// <summary>Gets the distance from the top of the first line to the baseline of the first line of a <see cref="T:System.Windows.Media.FormattedText" /> object.</summary>
	/// <returns>The distance from the top of the first line to the baseline of the first line, provided in device-independent units (1/96th inch per unit).</returns>
	public double Baseline => Metrics.Baseline;

	/// <summary>Gets the distance from the bottom of the last line of text to the bottommost drawn pixel.</summary>
	/// <returns>The distance from the bottom of the last line to the bottommost inked pixel, provided in device-independent units (1/96th inch per unit).</returns>
	public double OverhangAfter => BlackBoxMetrics.OverhangAfter;

	/// <summary>Gets the maximum distance from the leading alignment point to the leading drawn pixel of a line.</summary>
	/// <returns>The maximum distance from the leading alignment point to the leading drawn pixel of a line, provided in device-independent units (1/96th inch per unit).</returns>
	public double OverhangLeading => BlackBoxMetrics.OverhangLeading;

	/// <summary>Gets the maximum distance from the trailing inked pixel to the trailing alignment point of a line.</summary>
	/// <returns>The maximum distance from the trailing inked pixel to the trailing alignment point of a line, provided in device-independent units (1/96th inch per unit).</returns>
	public double OverhangTrailing => BlackBoxMetrics.OverhangTrailing;

	/// <summary>Gets the width between the leading and trailing alignment points of a line, excluding any trailing white-space characters.</summary>
	/// <returns>The width between the leading and trailing alignment points of a line, excluding any trailing white-space characters. Provided in device-independent units (1/96th inch per unit).</returns>
	public double Width => Metrics.Width;

	/// <summary>Gets the width between the leading and trailing alignment points of a line, including any trailing white-space characters.</summary>
	/// <returns>The width between the leading and trailing alignment points of a line, including any trailing white-space characters. Provided in device-independent units (1/96th inch per unit).</returns>
	public double WidthIncludingTrailingWhitespace => Metrics.WidthIncludingTrailingWhitespace;

	/// <summary>Gets the smallest possible text width that can fully contain the specified text content.</summary>
	/// <returns>The minimum text width of the text source, provided in device-independent units (1/96th inch per unit).</returns>
	public double MinWidth
	{
		get
		{
			if (_minWidth != double.MinValue)
			{
				return _minWidth;
			}
			if (_textSourceImpl == null)
			{
				_textSourceImpl = new TextSourceImplementation(this);
			}
			_minWidth = TextFormatter.FromCurrentDispatcher(_textFormattingMode).FormatMinMaxParagraphWidth(_textSourceImpl, 0, _defaultParaProps).MinWidth;
			return _minWidth;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.FormattedText" /> class with the specified text, culture, flow direction, typeface, font size, and brush.</summary>
	/// <param name="textToFormat">The text to be displayed.</param>
	/// <param name="culture">The specific culture of the text.</param>
	/// <param name="flowDirection">The direction the text is read.</param>
	/// <param name="typeface">The font family, weight, style and stretch the text should be formatted with.</param>
	/// <param name="emSize">The font size the text should be formatted at.</param>
	/// <param name="foreground">The brush used to paint the each glyph.</param>
	[Obsolete("Use the PixelsPerDip override", false)]
	public FormattedText(string textToFormat, CultureInfo culture, FlowDirection flowDirection, Typeface typeface, double emSize, Brush foreground)
		: this(textToFormat, culture, flowDirection, typeface, emSize, foreground, null, TextFormattingMode.Ideal)
	{
	}

	public FormattedText(string textToFormat, CultureInfo culture, FlowDirection flowDirection, Typeface typeface, double emSize, Brush foreground, double pixelsPerDip)
		: this(textToFormat, culture, flowDirection, typeface, emSize, foreground, null, TextFormattingMode.Ideal, pixelsPerDip)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.FormattedText" /> class with the specified text, culture, flow direction, typeface, font size, brush, and number substitution behavior.</summary>
	/// <param name="textToFormat">The text to be displayed.</param>
	/// <param name="culture">The specific culture of the text.</param>
	/// <param name="flowDirection">The direction the text is read.</param>
	/// <param name="typeface">The font family, weight, style and stretch the text should be formatted with.</param>
	/// <param name="emSize">The font size for the text's em measure, provided in device-independent units (1/96th inch per unit).</param>
	/// <param name="foreground">The brush used to paint the each glyph.</param>
	/// <param name="numberSubstitution">The number substitution behavior to apply to the text.</param>
	[Obsolete("Use the PixelsPerDip override", false)]
	public FormattedText(string textToFormat, CultureInfo culture, FlowDirection flowDirection, Typeface typeface, double emSize, Brush foreground, NumberSubstitution numberSubstitution)
		: this(textToFormat, culture, flowDirection, typeface, emSize, foreground, numberSubstitution, TextFormattingMode.Ideal)
	{
	}

	public FormattedText(string textToFormat, CultureInfo culture, FlowDirection flowDirection, Typeface typeface, double emSize, Brush foreground, NumberSubstitution numberSubstitution, double pixelsPerDip)
		: this(textToFormat, culture, flowDirection, typeface, emSize, foreground, numberSubstitution, TextFormattingMode.Ideal, pixelsPerDip)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.FormattedText" /> class with the specified text, culture, flow direction, typeface, font size, brush, number substitution behavior, and text formatting mode.</summary>
	/// <param name="textToFormat">The text to be displayed.</param>
	/// <param name="culture">The specific culture of the text.</param>
	/// <param name="flowDirection">The direction the text is read.</param>
	/// <param name="typeface">The font family, weight, style and stretch the text should be formatted with.</param>
	/// <param name="emSize">The font size for the text's em measure, provided in device-independent units (1/96th inch per unit).</param>
	/// <param name="foreground">The brush used to paint the each glyph.</param>
	/// <param name="numberSubstitution">The number substitution behavior to apply to the text.</param>
	/// <param name="textFormattingMode">The <see cref="T:System.Windows.Media.TextFormattingMode" />  to apply to the text.</param>
	[Obsolete("Use the PixelsPerDip override", false)]
	public FormattedText(string textToFormat, CultureInfo culture, FlowDirection flowDirection, Typeface typeface, double emSize, Brush foreground, NumberSubstitution numberSubstitution, TextFormattingMode textFormattingMode)
	{
		InitFormattedText(textToFormat, culture, flowDirection, typeface, emSize, foreground, numberSubstitution, textFormattingMode, _pixelsPerDip);
	}

	public FormattedText(string textToFormat, CultureInfo culture, FlowDirection flowDirection, Typeface typeface, double emSize, Brush foreground, NumberSubstitution numberSubstitution, TextFormattingMode textFormattingMode, double pixelsPerDip)
	{
		InitFormattedText(textToFormat, culture, flowDirection, typeface, emSize, foreground, numberSubstitution, textFormattingMode, pixelsPerDip);
	}

	private void InitFormattedText(string textToFormat, CultureInfo culture, FlowDirection flowDirection, Typeface typeface, double emSize, Brush foreground, NumberSubstitution numberSubstitution, TextFormattingMode textFormattingMode, double pixelsPerDip)
	{
		if (textToFormat == null)
		{
			throw new ArgumentNullException("textToFormat");
		}
		if (typeface == null)
		{
			throw new ArgumentNullException("typeface");
		}
		ValidateCulture(culture);
		ValidateFlowDirection(flowDirection, "flowDirection");
		ValidateFontSize(emSize);
		_pixelsPerDip = pixelsPerDip;
		_textFormattingMode = textFormattingMode;
		_text = textToFormat;
		GenericTextRunProperties genericTextRunProperties = new GenericTextRunProperties(typeface, emSize, 12.0, _pixelsPerDip, null, foreground, null, BaselineAlignment.Baseline, culture, numberSubstitution);
		_latestPosition = _formatRuns.SetValue(0, _text.Length, genericTextRunProperties, _latestPosition);
		_defaultParaProps = new GenericTextParagraphProperties(flowDirection, TextAlignment.Left, firstLineInParagraph: false, alwaysCollapsible: false, genericTextRunProperties, TextWrapping.WrapWithOverflow, 0.0, 0.0);
		InvalidateMetrics();
	}

	private static void ValidateCulture(CultureInfo culture)
	{
		if (culture == null)
		{
			throw new ArgumentNullException("culture");
		}
	}

	private static void ValidateFontSize(double emSize)
	{
		if (emSize <= 0.0)
		{
			throw new ArgumentOutOfRangeException("emSize", SR.ParameterMustBeGreaterThanZero);
		}
		if (emSize > 35791.39406666667)
		{
			throw new ArgumentOutOfRangeException("emSize", SR.Format(SR.ParameterCannotBeGreaterThan, 35791.39406666667));
		}
		if (double.IsNaN(emSize))
		{
			throw new ArgumentOutOfRangeException("emSize", SR.ParameterValueCannotBeNaN);
		}
	}

	private static void ValidateFlowDirection(FlowDirection flowDirection, string parameterName)
	{
		if (flowDirection < FlowDirection.LeftToRight || flowDirection > FlowDirection.RightToLeft)
		{
			throw new InvalidEnumArgumentException(parameterName, (int)flowDirection, typeof(FlowDirection));
		}
	}

	private int ValidateRange(int startIndex, int count)
	{
		if (startIndex < 0 || startIndex > _text.Length)
		{
			throw new ArgumentOutOfRangeException("startIndex");
		}
		int num = startIndex + count;
		if (count < 0 || num < startIndex || num > _text.Length)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		return num;
	}

	private void InvalidateMetrics()
	{
		_metrics = null;
		_minWidth = double.MinValue;
	}

	/// <summary>Changes the foreground <see cref="T:System.Windows.Media.Brush" /> for an entire <see cref="T:System.Windows.Media.FormattedText" /> object. </summary>
	/// <param name="foregroundBrush">The brush to use for the text foreground.</param>
	public void SetForegroundBrush(Brush foregroundBrush)
	{
		SetForegroundBrush(foregroundBrush, 0, _text.Length);
	}

	/// <summary>Changes the foreground <see cref="T:System.Windows.Media.Brush" /> for specified text within a <see cref="T:System.Windows.Media.FormattedText" /> object. </summary>
	/// <param name="foregroundBrush">The brush to use for the text foreground.</param>
	/// <param name="startIndex">The start index of the initial character to apply the foreground brush to.</param>
	/// <param name="count">The number of characters to apply the foreground brush to.</param>
	public void SetForegroundBrush(Brush foregroundBrush, int startIndex, int count)
	{
		int num = ValidateRange(startIndex, count);
		int num2 = startIndex;
		while (num2 < num)
		{
			SpanRider spanRider = new SpanRider(_formatRuns, _latestPosition, num2);
			num2 = Math.Min(num, num2 + spanRider.Length);
			GenericTextRunProperties genericTextRunProperties = spanRider.CurrentElement as GenericTextRunProperties;
			Invariant.Assert(genericTextRunProperties != null);
			if (genericTextRunProperties.ForegroundBrush != foregroundBrush)
			{
				GenericTextRunProperties element = new GenericTextRunProperties(genericTextRunProperties.Typeface, genericTextRunProperties.FontRenderingEmSize, genericTextRunProperties.FontHintingEmSize, _pixelsPerDip, genericTextRunProperties.TextDecorations, foregroundBrush, genericTextRunProperties.BackgroundBrush, genericTextRunProperties.BaselineAlignment, genericTextRunProperties.CultureInfo, genericTextRunProperties.NumberSubstitution);
				_latestPosition = _formatRuns.SetValue(spanRider.CurrentPosition, num2 - spanRider.CurrentPosition, element, spanRider.SpanPosition);
			}
		}
	}

	/// <summary>Sets the font family for the entire set of characters in the <see cref="T:System.Windows.Media.FormattedText" /> object.</summary>
	/// <param name="fontFamily">A string that constructs the <see cref="T:System.Windows.Media.FontFamily" /> to use for text formatting. Fallbacks are permitted; for details, see <see cref="T:System.Windows.Media.FontFamily" />.</param>
	public void SetFontFamily(string fontFamily)
	{
		SetFontFamily(fontFamily, 0, _text.Length);
	}

	/// <summary>Sets the font family for a specified subset of characters in the <see cref="T:System.Windows.Media.FormattedText" /> object.</summary>
	/// <param name="fontFamily">A string that constructs the <see cref="T:System.Windows.Media.FontFamily" /> to use for text formatting. Fallbacks are permitted; for details, see <see cref="T:System.Windows.Media.FontFamily" />.</param>
	/// <param name="startIndex">The starting index of the initial character to apply the font family change to.</param>
	/// <param name="count">The number of characters the change should apply to.</param>
	public void SetFontFamily(string fontFamily, int startIndex, int count)
	{
		if (fontFamily == null)
		{
			throw new ArgumentNullException("fontFamily");
		}
		SetFontFamily(new FontFamily(fontFamily), startIndex, count);
	}

	/// <summary>Sets the font family for a <see cref="T:System.Windows.Media.FormattedText" /> object.</summary>
	/// <param name="fontFamily">The <see cref="T:System.Windows.Media.FontFamily" /> to use for text formatting.</param>
	public void SetFontFamily(FontFamily fontFamily)
	{
		SetFontFamily(fontFamily, 0, _text.Length);
	}

	/// <summary>Sets the font family for a specified subset of characters in the <see cref="T:System.Windows.Media.FormattedText" /> object.</summary>
	/// <param name="fontFamily">The <see cref="T:System.Windows.Media.FontFamily" /> to use for text formatting.</param>
	/// <param name="startIndex">The starting index of the initial character to apply the font family change to.</param>
	/// <param name="count">The number of characters the change should apply to.</param>
	public void SetFontFamily(FontFamily fontFamily, int startIndex, int count)
	{
		if (fontFamily == null)
		{
			throw new ArgumentNullException("fontFamily");
		}
		int num = ValidateRange(startIndex, count);
		int num2 = startIndex;
		while (num2 < num)
		{
			SpanRider spanRider = new SpanRider(_formatRuns, _latestPosition, num2);
			num2 = Math.Min(num, num2 + spanRider.Length);
			GenericTextRunProperties genericTextRunProperties = spanRider.CurrentElement as GenericTextRunProperties;
			Invariant.Assert(genericTextRunProperties != null);
			Typeface typeface = genericTextRunProperties.Typeface;
			if (!fontFamily.Equals(typeface.FontFamily))
			{
				GenericTextRunProperties element = new GenericTextRunProperties(new Typeface(fontFamily, typeface.Style, typeface.Weight, typeface.Stretch), genericTextRunProperties.FontRenderingEmSize, genericTextRunProperties.FontHintingEmSize, _pixelsPerDip, genericTextRunProperties.TextDecorations, genericTextRunProperties.ForegroundBrush, genericTextRunProperties.BackgroundBrush, genericTextRunProperties.BaselineAlignment, genericTextRunProperties.CultureInfo, genericTextRunProperties.NumberSubstitution);
				_latestPosition = _formatRuns.SetValue(spanRider.CurrentPosition, num2 - spanRider.CurrentPosition, element, spanRider.SpanPosition);
				InvalidateMetrics();
			}
		}
	}

	/// <summary>Sets the font size for the entire set of characters in the <see cref="T:System.Windows.Media.FormattedText" /> object.</summary>
	/// <param name="emSize">The font 'em' measure size, provided in device-independent units (1/96th inch per unit).</param>
	public void SetFontSize(double emSize)
	{
		SetFontSize(emSize, 0, _text.Length);
	}

	/// <summary>Sets the font size for a specified subset of characters in the <see cref="T:System.Windows.Media.FormattedText" /> object.</summary>
	/// <param name="emSize">The font 'em' measure size, provided in device-independent units (1/96th inch per unit).</param>
	/// <param name="startIndex">The start index of the initial character to apply the font size to.</param>
	/// <param name="count">The number of characters to apply the font size to.</param>
	public void SetFontSize(double emSize, int startIndex, int count)
	{
		ValidateFontSize(emSize);
		int num = ValidateRange(startIndex, count);
		int num2 = startIndex;
		while (num2 < num)
		{
			SpanRider spanRider = new SpanRider(_formatRuns, _latestPosition, num2);
			num2 = Math.Min(num, num2 + spanRider.Length);
			GenericTextRunProperties genericTextRunProperties = spanRider.CurrentElement as GenericTextRunProperties;
			Invariant.Assert(genericTextRunProperties != null);
			if (genericTextRunProperties.FontRenderingEmSize != emSize)
			{
				GenericTextRunProperties element = new GenericTextRunProperties(genericTextRunProperties.Typeface, emSize, genericTextRunProperties.FontHintingEmSize, _pixelsPerDip, genericTextRunProperties.TextDecorations, genericTextRunProperties.ForegroundBrush, genericTextRunProperties.BackgroundBrush, genericTextRunProperties.BaselineAlignment, genericTextRunProperties.CultureInfo, genericTextRunProperties.NumberSubstitution);
				_latestPosition = _formatRuns.SetValue(spanRider.CurrentPosition, num2 - spanRider.CurrentPosition, element, spanRider.SpanPosition);
				InvalidateMetrics();
			}
		}
	}

	/// <summary>Sets the <see cref="T:System.Globalization.CultureInfo" /> for the entire set of characters in the <see cref="T:System.Windows.Media.FormattedText" /> object.</summary>
	/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to use for text formatting.</param>
	public void SetCulture(CultureInfo culture)
	{
		SetCulture(culture, 0, _text.Length);
	}

	/// <summary>Sets the <see cref="T:System.Globalization.CultureInfo" /> for a specified subset of characters in the <see cref="T:System.Windows.Media.FormattedText" /> object.</summary>
	/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to use for text formatting.</param>
	/// <param name="startIndex">The start index of initial character to apply the change to.</param>
	/// <param name="count">The number of characters the change should be applied to.</param>
	public void SetCulture(CultureInfo culture, int startIndex, int count)
	{
		ValidateCulture(culture);
		int num = ValidateRange(startIndex, count);
		int num2 = startIndex;
		while (num2 < num)
		{
			SpanRider spanRider = new SpanRider(_formatRuns, _latestPosition, num2);
			num2 = Math.Min(num, num2 + spanRider.Length);
			GenericTextRunProperties genericTextRunProperties = spanRider.CurrentElement as GenericTextRunProperties;
			Invariant.Assert(genericTextRunProperties != null);
			if (!genericTextRunProperties.CultureInfo.Equals(culture))
			{
				GenericTextRunProperties element = new GenericTextRunProperties(genericTextRunProperties.Typeface, genericTextRunProperties.FontRenderingEmSize, genericTextRunProperties.FontHintingEmSize, _pixelsPerDip, genericTextRunProperties.TextDecorations, genericTextRunProperties.ForegroundBrush, genericTextRunProperties.BackgroundBrush, genericTextRunProperties.BaselineAlignment, culture, genericTextRunProperties.NumberSubstitution);
				_latestPosition = _formatRuns.SetValue(spanRider.CurrentPosition, num2 - spanRider.CurrentPosition, element, spanRider.SpanPosition);
				InvalidateMetrics();
			}
		}
	}

	/// <summary>Sets the number substitution behavior for the entire set of characters in the <see cref="T:System.Windows.Media.FormattedText" /> object.</summary>
	/// <param name="numberSubstitution">Number substitution behavior to apply to the text; can be null, in which case the default number substitution method for the text culture is used.</param>
	public void SetNumberSubstitution(NumberSubstitution numberSubstitution)
	{
		SetNumberSubstitution(numberSubstitution, 0, _text.Length);
	}

	/// <summary>Sets the number substitution behavior for specified text within a <see cref="T:System.Windows.Media.FormattedText" /> object.</summary>
	/// <param name="numberSubstitution">Number substitution behavior to apply to the text; can be null, in which case the default number substitution method for the text culture is used.</param>
	/// <param name="startIndex">The start index of initial character to apply the change to.</param>
	/// <param name="count">The number of characters the change should be applied to.</param>
	public void SetNumberSubstitution(NumberSubstitution numberSubstitution, int startIndex, int count)
	{
		int num = ValidateRange(startIndex, count);
		int num2 = startIndex;
		while (num2 < num)
		{
			SpanRider spanRider = new SpanRider(_formatRuns, _latestPosition, num2);
			num2 = Math.Min(num, num2 + spanRider.Length);
			GenericTextRunProperties genericTextRunProperties = spanRider.CurrentElement as GenericTextRunProperties;
			Invariant.Assert(genericTextRunProperties != null);
			if (numberSubstitution != null)
			{
				if (numberSubstitution.Equals(genericTextRunProperties.NumberSubstitution))
				{
					continue;
				}
			}
			else if (genericTextRunProperties.NumberSubstitution == null)
			{
				continue;
			}
			GenericTextRunProperties element = new GenericTextRunProperties(genericTextRunProperties.Typeface, genericTextRunProperties.FontRenderingEmSize, genericTextRunProperties.FontHintingEmSize, _pixelsPerDip, genericTextRunProperties.TextDecorations, genericTextRunProperties.ForegroundBrush, genericTextRunProperties.BackgroundBrush, genericTextRunProperties.BaselineAlignment, genericTextRunProperties.CultureInfo, numberSubstitution);
			_latestPosition = _formatRuns.SetValue(spanRider.CurrentPosition, num2 - spanRider.CurrentPosition, element, spanRider.SpanPosition);
			InvalidateMetrics();
		}
	}

	/// <summary>Sets the font weight for the entire set of characters in the <see cref="T:System.Windows.Media.FormattedText" /> object.</summary>
	/// <param name="weight">The <see cref="T:System.Windows.FontWeight" /> to use for text formatting.</param>
	public void SetFontWeight(FontWeight weight)
	{
		SetFontWeight(weight, 0, _text.Length);
	}

	/// <summary>Changes the <see cref="T:System.Windows.FontWeight" /> for specified text within a <see cref="T:System.Windows.Media.FormattedText" /> object. </summary>
	/// <param name="weight">The font weight to use for text formatting.</param>
	/// <param name="startIndex">The start index of the initial character to apply the font weight to.</param>
	/// <param name="count">The number of characters to apply the font weight to.</param>
	public void SetFontWeight(FontWeight weight, int startIndex, int count)
	{
		int num = ValidateRange(startIndex, count);
		int num2 = startIndex;
		while (num2 < num)
		{
			SpanRider spanRider = new SpanRider(_formatRuns, _latestPosition, num2);
			num2 = Math.Min(num, num2 + spanRider.Length);
			GenericTextRunProperties genericTextRunProperties = spanRider.CurrentElement as GenericTextRunProperties;
			Invariant.Assert(genericTextRunProperties != null);
			Typeface typeface = genericTextRunProperties.Typeface;
			if (!(typeface.Weight == weight))
			{
				GenericTextRunProperties element = new GenericTextRunProperties(new Typeface(typeface.FontFamily, typeface.Style, weight, typeface.Stretch), genericTextRunProperties.FontRenderingEmSize, genericTextRunProperties.FontHintingEmSize, _pixelsPerDip, genericTextRunProperties.TextDecorations, genericTextRunProperties.ForegroundBrush, genericTextRunProperties.BackgroundBrush, genericTextRunProperties.BaselineAlignment, genericTextRunProperties.CultureInfo, genericTextRunProperties.NumberSubstitution);
				_latestPosition = _formatRuns.SetValue(spanRider.CurrentPosition, num2 - spanRider.CurrentPosition, element, spanRider.SpanPosition);
				InvalidateMetrics();
			}
		}
	}

	/// <summary>Sets the font style for the entire set of characters in the <see cref="T:System.Windows.Media.FormattedText" /> object.</summary>
	/// <param name="style">The <see cref="T:System.Windows.FontStyle" /> value to use for text formatting.</param>
	public void SetFontStyle(FontStyle style)
	{
		SetFontStyle(style, 0, _text.Length);
	}

	/// <summary>Sets the font style for a specified subset of characters in the <see cref="T:System.Windows.Media.FormattedText" /> object.</summary>
	/// <param name="style">The <see cref="T:System.Windows.FontStyle" /> value to use for text formatting.</param>
	/// <param name="startIndex">The start index of the initial character to apply the font style to.</param>
	/// <param name="count">The number of characters to apply the font style to.</param>
	public void SetFontStyle(FontStyle style, int startIndex, int count)
	{
		int num = ValidateRange(startIndex, count);
		int num2 = startIndex;
		while (num2 < num)
		{
			SpanRider spanRider = new SpanRider(_formatRuns, _latestPosition, num2);
			num2 = Math.Min(num, num2 + spanRider.Length);
			GenericTextRunProperties genericTextRunProperties = spanRider.CurrentElement as GenericTextRunProperties;
			Invariant.Assert(genericTextRunProperties != null);
			Typeface typeface = genericTextRunProperties.Typeface;
			if (!(typeface.Style == style))
			{
				GenericTextRunProperties element = new GenericTextRunProperties(new Typeface(typeface.FontFamily, style, typeface.Weight, typeface.Stretch), genericTextRunProperties.FontRenderingEmSize, genericTextRunProperties.FontHintingEmSize, _pixelsPerDip, genericTextRunProperties.TextDecorations, genericTextRunProperties.ForegroundBrush, genericTextRunProperties.BackgroundBrush, genericTextRunProperties.BaselineAlignment, genericTextRunProperties.CultureInfo, genericTextRunProperties.NumberSubstitution);
				_latestPosition = _formatRuns.SetValue(spanRider.CurrentPosition, num2 - spanRider.CurrentPosition, element, spanRider.SpanPosition);
				InvalidateMetrics();
			}
		}
	}

	/// <summary>Sets the font stretch value for the entire set of characters in the <see cref="T:System.Windows.Media.FormattedText" /> object.</summary>
	/// <param name="stretch">The desired <see cref="T:System.Windows.FontStretch" /> value to use for text formatting.</param>
	public void SetFontStretch(FontStretch stretch)
	{
		SetFontStretch(stretch, 0, _text.Length);
	}

	/// <summary>Sets the font stretch value for a specified subset of characters in the <see cref="T:System.Windows.Media.FormattedText" /> object.</summary>
	/// <param name="stretch">The desired <see cref="T:System.Windows.FontStretch" /> value to use for text formatting.</param>
	/// <param name="startIndex">The start index of the initial character to apply the font stretch to.</param>
	/// <param name="count">The number of characters to apply the font stretch to.</param>
	public void SetFontStretch(FontStretch stretch, int startIndex, int count)
	{
		int num = ValidateRange(startIndex, count);
		int num2 = startIndex;
		while (num2 < num)
		{
			SpanRider spanRider = new SpanRider(_formatRuns, _latestPosition, num2);
			num2 = Math.Min(num, num2 + spanRider.Length);
			GenericTextRunProperties genericTextRunProperties = spanRider.CurrentElement as GenericTextRunProperties;
			Invariant.Assert(genericTextRunProperties != null);
			Typeface typeface = genericTextRunProperties.Typeface;
			if (!(typeface.Stretch == stretch))
			{
				GenericTextRunProperties element = new GenericTextRunProperties(new Typeface(typeface.FontFamily, typeface.Style, typeface.Weight, stretch), genericTextRunProperties.FontRenderingEmSize, genericTextRunProperties.FontHintingEmSize, _pixelsPerDip, genericTextRunProperties.TextDecorations, genericTextRunProperties.ForegroundBrush, genericTextRunProperties.BackgroundBrush, genericTextRunProperties.BaselineAlignment, genericTextRunProperties.CultureInfo, genericTextRunProperties.NumberSubstitution);
				_latestPosition = _formatRuns.SetValue(spanRider.CurrentPosition, num2 - spanRider.CurrentPosition, element, spanRider.SpanPosition);
				InvalidateMetrics();
			}
		}
	}

	/// <summary>Sets the font typeface for the entire set of characters in the <see cref="T:System.Windows.Media.FormattedText" /> object.</summary>
	/// <param name="typeface">The <see cref="T:System.Windows.Media.Typeface" /> to use for text formatting.</param>
	public void SetFontTypeface(Typeface typeface)
	{
		SetFontTypeface(typeface, 0, _text.Length);
	}

	/// <summary>Sets the font typeface for a specified subset of characters in the <see cref="T:System.Windows.Media.FormattedText" /> object.</summary>
	/// <param name="typeface">The <see cref="T:System.Windows.Media.Typeface" /> to use for text formatting.</param>
	/// <param name="startIndex">The start index of the initial character to apply the typeface to.</param>
	/// <param name="count">The number of characters to apply the typeface to.</param>
	public void SetFontTypeface(Typeface typeface, int startIndex, int count)
	{
		int num = ValidateRange(startIndex, count);
		int num2 = startIndex;
		while (num2 < num)
		{
			SpanRider spanRider = new SpanRider(_formatRuns, _latestPosition, num2);
			num2 = Math.Min(num, num2 + spanRider.Length);
			GenericTextRunProperties genericTextRunProperties = spanRider.CurrentElement as GenericTextRunProperties;
			Invariant.Assert(genericTextRunProperties != null);
			if (genericTextRunProperties.Typeface != typeface)
			{
				GenericTextRunProperties element = new GenericTextRunProperties(typeface, genericTextRunProperties.FontRenderingEmSize, genericTextRunProperties.FontHintingEmSize, _pixelsPerDip, genericTextRunProperties.TextDecorations, genericTextRunProperties.ForegroundBrush, genericTextRunProperties.BackgroundBrush, genericTextRunProperties.BaselineAlignment, genericTextRunProperties.CultureInfo, genericTextRunProperties.NumberSubstitution);
				_latestPosition = _formatRuns.SetValue(spanRider.CurrentPosition, num2 - spanRider.CurrentPosition, element, spanRider.SpanPosition);
				InvalidateMetrics();
			}
		}
	}

	/// <summary>Sets the <see cref="T:System.Windows.TextDecorationCollection" /> for the entire set of characters in the <see cref="T:System.Windows.Media.FormattedText" /> object.</summary>
	/// <param name="textDecorations">The <see cref="T:System.Windows.TextDecorationCollection" /> to apply to the text.</param>
	public void SetTextDecorations(TextDecorationCollection textDecorations)
	{
		SetTextDecorations(textDecorations, 0, _text.Length);
	}

	/// <summary>Sets the <see cref="T:System.Windows.TextDecorationCollection" /> for specified text within a <see cref="T:System.Windows.Media.FormattedText" /> object.</summary>
	/// <param name="textDecorations">The <see cref="T:System.Windows.TextDecorationCollection" /> to apply to the text.</param>
	/// <param name="startIndex">The start index of the initial character to apply the text decorations to.</param>
	/// <param name="count">The number of characters to apply the text decorations to.</param>
	public void SetTextDecorations(TextDecorationCollection textDecorations, int startIndex, int count)
	{
		int num = ValidateRange(startIndex, count);
		int num2 = startIndex;
		while (num2 < num)
		{
			SpanRider spanRider = new SpanRider(_formatRuns, _latestPosition, num2);
			num2 = Math.Min(num, num2 + spanRider.Length);
			GenericTextRunProperties genericTextRunProperties = spanRider.CurrentElement as GenericTextRunProperties;
			Invariant.Assert(genericTextRunProperties != null);
			if (genericTextRunProperties.TextDecorations != textDecorations)
			{
				GenericTextRunProperties element = new GenericTextRunProperties(genericTextRunProperties.Typeface, genericTextRunProperties.FontRenderingEmSize, genericTextRunProperties.FontHintingEmSize, _pixelsPerDip, textDecorations, genericTextRunProperties.ForegroundBrush, genericTextRunProperties.BackgroundBrush, genericTextRunProperties.BaselineAlignment, genericTextRunProperties.CultureInfo, genericTextRunProperties.NumberSubstitution);
				_latestPosition = _formatRuns.SetValue(spanRider.CurrentPosition, num2 - spanRider.CurrentPosition, element, spanRider.SpanPosition);
			}
		}
	}

	private LineEnumerator GetEnumerator()
	{
		return new LineEnumerator(this);
	}

	private void AdvanceLineOrigin(ref Point lineOrigin, TextLine currentLine)
	{
		double height = currentLine.Height;
		FlowDirection flowDirection = _defaultParaProps.FlowDirection;
		if ((uint)flowDirection <= 1u)
		{
			lineOrigin.Y += height;
		}
	}

	/// <summary>Sets an array of maximum text widths within the <see cref="T:System.Windows.Media.FormattedText" />, on a per-line basis. Each element in the array represents the maximum text width of sequential lines of text. </summary>
	/// <param name="maxTextWidths">An array of maximum text widths, each width provided in device-independent units (1/96th inch per unit).</param>
	public void SetMaxTextWidths(double[] maxTextWidths)
	{
		if (maxTextWidths == null || maxTextWidths.Length == 0)
		{
			throw new ArgumentNullException("maxTextWidths");
		}
		_maxTextWidths = maxTextWidths;
		InvalidateMetrics();
	}

	/// <summary>Retrieves an array of text widths. Each element in the array represents the maximum text width of sequential lines of text.</summary>
	/// <returns>An array of maximum text widths, each width provided in device-independent units (1/96th inch per unit).</returns>
	public double[] GetMaxTextWidths()
	{
		if (_maxTextWidths != null)
		{
			return (double[])_maxTextWidths.Clone();
		}
		return null;
	}

	/// <summary>Returns a <see cref="T:System.Windows.Media.Geometry" /> object that represents the highlight bounding box of the formatted text.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.Geometry" /> object that represents the highlight bounding box of the formatted text.</returns>
	/// <param name="origin">The origin of the highlight region.</param>
	public Geometry BuildHighlightGeometry(Point origin)
	{
		return BuildHighlightGeometry(origin, 0, _text.Length);
	}

	/// <summary>Returns a <see cref="T:System.Windows.Media.Geometry" /> object that represents the formatted text, including all glyphs and text decorations.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.Geometry" /> object representation of the formatted text.</returns>
	/// <param name="origin">The top-left origin of the resulting geometry.</param>
	public Geometry BuildGeometry(Point origin)
	{
		GeometryGroup accumulatedGeometry = null;
		Point lineOrigin = origin;
		DrawingGroup drawingGroup = new DrawingGroup();
		DrawingContext drawingContext = drawingGroup.Open();
		using (LineEnumerator lineEnumerator = GetEnumerator())
		{
			while (lineEnumerator.MoveNext())
			{
				using TextLine textLine = lineEnumerator.Current;
				textLine.Draw(drawingContext, lineOrigin, InvertAxes.None);
				AdvanceLineOrigin(ref lineOrigin, textLine);
			}
		}
		drawingContext.Close();
		CombineGeometryRecursive(drawingGroup, ref accumulatedGeometry);
		if (accumulatedGeometry == null || accumulatedGeometry.IsEmpty())
		{
			return Geometry.Empty;
		}
		return accumulatedGeometry;
	}

	/// <summary>Returns a <see cref="T:System.Windows.Media.Geometry" /> object that represents the highlight bounding box for a specified substring of the formatted text.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.Geometry" /> object that represents the highlight bounding box of the formatted text substring.</returns>
	/// <param name="origin">The origin of the highlight region.</param>
	/// <param name="startIndex">The index of the initial character the highlight bounds should be obtained for.</param>
	/// <param name="count">The number of characters the highlight bounds should contain.</param>
	public Geometry BuildHighlightGeometry(Point origin, int startIndex, int count)
	{
		ValidateRange(startIndex, count);
		PathGeometry pathGeometry = null;
		using (LineEnumerator lineEnumerator = GetEnumerator())
		{
			Point lineOrigin = origin;
			while (lineEnumerator.MoveNext())
			{
				using TextLine textLine = lineEnumerator.Current;
				int num = Math.Max(lineEnumerator.Position, startIndex);
				int num2 = Math.Min(lineEnumerator.Position + lineEnumerator.Length, startIndex + count);
				if (num < num2)
				{
					IList<TextBounds> textBounds = textLine.GetTextBounds(num, num2 - num);
					if (textBounds != null)
					{
						foreach (TextBounds item in textBounds)
						{
							Rect rectangle = item.Rectangle;
							if (FlowDirection == FlowDirection.RightToLeft)
							{
								rectangle.X = lineEnumerator.CurrentParagraphWidth - rectangle.Right;
							}
							rectangle.X += lineOrigin.X;
							rectangle.Y += lineOrigin.Y;
							RectangleGeometry rectangleGeometry = new RectangleGeometry(rectangle);
							pathGeometry = ((pathGeometry != null) ? Geometry.Combine(pathGeometry, rectangleGeometry, GeometryCombineMode.Union, null) : rectangleGeometry.GetAsPathGeometry());
						}
					}
				}
				AdvanceLineOrigin(ref lineOrigin, textLine);
			}
		}
		if (pathGeometry == null || pathGeometry.IsEmpty())
		{
			return null;
		}
		return pathGeometry;
	}

	internal void Draw(DrawingContext dc, Point origin)
	{
		Point lineOrigin = origin;
		if (_metrics != null && !double.IsNaN(_metrics.Extent))
		{
			using (LineEnumerator lineEnumerator = GetEnumerator())
			{
				while (lineEnumerator.MoveNext())
				{
					using TextLine textLine = lineEnumerator.Current;
					textLine.Draw(dc, lineOrigin, InvertAxes.None);
					AdvanceLineOrigin(ref lineOrigin, textLine);
				}
				return;
			}
		}
		_metrics = DrawAndCalculateMetrics(dc, origin, getBlackBoxMetrics: true);
	}

	private CachedMetrics DrawAndCalculateMetrics(DrawingContext dc, Point drawingOffset, bool getBlackBoxMetrics)
	{
		CachedMetrics cachedMetrics = new CachedMetrics();
		if (_text.Length == 0)
		{
			return cachedMetrics;
		}
		using (LineEnumerator lineEnumerator = GetEnumerator())
		{
			bool flag = true;
			double num;
			double num2 = (num = double.MaxValue);
			double num3;
			double num4 = (num3 = double.MinValue);
			Point lineOrigin = new Point(0.0, 0.0);
			double num5 = double.MaxValue;
			while (lineEnumerator.MoveNext())
			{
				using TextLine textLine = lineEnumerator.Current;
				if (dc != null)
				{
					textLine.Draw(dc, new Point(lineOrigin.X + drawingOffset.X, lineOrigin.Y + drawingOffset.Y), InvertAxes.None);
				}
				if (getBlackBoxMetrics)
				{
					double val = lineOrigin.X + textLine.Start + textLine.OverhangLeading;
					double val2 = lineOrigin.X + textLine.Start + textLine.Width - textLine.OverhangTrailing;
					double num6 = lineOrigin.Y + textLine.Height + textLine.OverhangAfter;
					double val3 = num6 - textLine.Extent;
					num2 = Math.Min(num2, val);
					num4 = Math.Max(num4, val2);
					num3 = Math.Max(num3, num6);
					num = Math.Min(num, val3);
					cachedMetrics.OverhangAfter = textLine.OverhangAfter;
				}
				cachedMetrics.Height += textLine.Height;
				cachedMetrics.Width = Math.Max(cachedMetrics.Width, textLine.Width);
				cachedMetrics.WidthIncludingTrailingWhitespace = Math.Max(cachedMetrics.WidthIncludingTrailingWhitespace, textLine.WidthIncludingTrailingWhitespace);
				num5 = Math.Min(num5, textLine.Start);
				if (flag)
				{
					cachedMetrics.Baseline = textLine.Baseline;
					flag = false;
				}
				AdvanceLineOrigin(ref lineOrigin, textLine);
			}
			if (getBlackBoxMetrics)
			{
				cachedMetrics.Extent = num3 - num;
				cachedMetrics.OverhangLeading = num2 - num5;
				cachedMetrics.OverhangTrailing = cachedMetrics.Width - (num4 - num5);
			}
			else
			{
				cachedMetrics.Extent = double.NaN;
			}
		}
		return cachedMetrics;
	}

	private void CombineGeometryRecursive(Drawing drawing, ref GeometryGroup accumulatedGeometry)
	{
		if (drawing is DrawingGroup drawingGroup)
		{
			{
				foreach (Drawing child in drawingGroup.Children)
				{
					CombineGeometryRecursive(child, ref accumulatedGeometry);
				}
				return;
			}
		}
		if (drawing is GlyphRunDrawing { GlyphRun: var glyphRun })
		{
			if (glyphRun == null)
			{
				return;
			}
			Geometry geometry = glyphRun.BuildGeometry();
			if (!geometry.IsEmpty())
			{
				if (accumulatedGeometry == null)
				{
					accumulatedGeometry = new GeometryGroup();
					accumulatedGeometry.FillRule = FillRule.Nonzero;
				}
				accumulatedGeometry.Children.Add(geometry);
			}
		}
		else
		{
			if (!(drawing is GeometryDrawing { Geometry: { } geometry2 } geometryDrawing))
			{
				return;
			}
			if (geometry2 is LineGeometry { Bounds: var bounds })
			{
				if (bounds.Height == 0.0)
				{
					bounds.Height = geometryDrawing.Pen.Thickness;
				}
				else if (bounds.Width == 0.0)
				{
					bounds.Width = geometryDrawing.Pen.Thickness;
				}
				geometry2 = new RectangleGeometry(bounds);
			}
			if (accumulatedGeometry == null)
			{
				accumulatedGeometry = new GeometryGroup();
				accumulatedGeometry.FillRule = FillRule.Nonzero;
			}
			accumulatedGeometry.Children.Add(geometry2);
		}
	}
}
