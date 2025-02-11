using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.TextFormatting;
using MS.Internal.PtsHost;

namespace MS.Internal.Text;

internal class LineProperties : TextParagraphProperties
{
	private sealed class FirstLineProperties : TextParagraphProperties
	{
		private LineProperties _lp;

		public override FlowDirection FlowDirection => _lp.FlowDirection;

		public override TextAlignment TextAlignment => _lp.TextAlignment;

		public override double LineHeight => _lp.LineHeight;

		public override bool FirstLineInParagraph => true;

		public override TextRunProperties DefaultTextRunProperties => _lp.DefaultTextRunProperties;

		public override TextDecorationCollection TextDecorations => _lp.TextDecorations;

		public override TextWrapping TextWrapping => _lp.TextWrapping;

		public override TextMarkerProperties TextMarkerProperties => _lp.TextMarkerProperties;

		public override double Indent => _lp._textIndent;

		internal FirstLineProperties(LineProperties lp)
		{
			_lp = lp;
			Hyphenator = lp.Hyphenator;
		}
	}

	private sealed class ParaEllipsisLineProperties : TextParagraphProperties
	{
		private TextParagraphProperties _lp;

		public override FlowDirection FlowDirection => _lp.FlowDirection;

		public override TextAlignment TextAlignment => _lp.TextAlignment;

		public override double LineHeight => _lp.LineHeight;

		public override bool FirstLineInParagraph => _lp.FirstLineInParagraph;

		public override bool AlwaysCollapsible => _lp.AlwaysCollapsible;

		public override TextRunProperties DefaultTextRunProperties => _lp.DefaultTextRunProperties;

		public override TextDecorationCollection TextDecorations => _lp.TextDecorations;

		public override TextWrapping TextWrapping => TextWrapping.NoWrap;

		public override TextMarkerProperties TextMarkerProperties => _lp.TextMarkerProperties;

		public override double Indent => _lp.Indent;

		internal ParaEllipsisLineProperties(TextParagraphProperties lp)
		{
			_lp = lp;
		}
	}

	private TextRunProperties _defaultTextProperties;

	private TextMarkerProperties _markerProperties;

	private FirstLineProperties _firstLineProperties;

	private bool _ignoreTextAlignment;

	private FlowDirection _flowDirection;

	private TextAlignment _textAlignment;

	private TextWrapping _textWrapping;

	private TextTrimming _textTrimming;

	private double _lineHeight;

	private double _textIndent;

	private LineStackingStrategy _lineStackingStrategy;

	public override FlowDirection FlowDirection => _flowDirection;

	public override TextAlignment TextAlignment
	{
		get
		{
			if (!IgnoreTextAlignment)
			{
				return _textAlignment;
			}
			return TextAlignment.Left;
		}
	}

	public override double LineHeight
	{
		get
		{
			if (LineStackingStrategy == LineStackingStrategy.BlockLineHeight && !double.IsNaN(_lineHeight))
			{
				return _lineHeight;
			}
			return 0.0;
		}
	}

	public override bool FirstLineInParagraph => false;

	public override TextRunProperties DefaultTextRunProperties => _defaultTextProperties;

	public override TextDecorationCollection TextDecorations => _defaultTextProperties.TextDecorations;

	public override TextWrapping TextWrapping => _textWrapping;

	public override TextMarkerProperties TextMarkerProperties => _markerProperties;

	public override double Indent => 0.0;

	internal TextAlignment TextAlignmentInternal => _textAlignment;

	internal bool IgnoreTextAlignment
	{
		get
		{
			return _ignoreTextAlignment;
		}
		set
		{
			_ignoreTextAlignment = value;
		}
	}

	internal LineStackingStrategy LineStackingStrategy => _lineStackingStrategy;

	internal TextTrimming TextTrimming => _textTrimming;

	internal bool HasFirstLineProperties
	{
		get
		{
			if (_markerProperties == null)
			{
				return !DoubleUtil.IsZero(_textIndent);
			}
			return true;
		}
	}

	internal TextParagraphProperties FirstLineProps
	{
		get
		{
			if (_firstLineProperties == null)
			{
				_firstLineProperties = new FirstLineProperties(this);
			}
			return _firstLineProperties;
		}
	}

	internal LineProperties(DependencyObject element, DependencyObject contentHost, TextProperties defaultTextProperties, MarkerProperties markerProperties)
		: this(element, contentHost, defaultTextProperties, markerProperties, (TextAlignment)element.GetValue(Block.TextAlignmentProperty))
	{
	}

	internal LineProperties(DependencyObject element, DependencyObject contentHost, TextProperties defaultTextProperties, MarkerProperties markerProperties, TextAlignment textAlignment)
	{
		_defaultTextProperties = defaultTextProperties;
		_markerProperties = markerProperties?.GetTextMarkerProperties(this);
		_flowDirection = (FlowDirection)element.GetValue(Block.FlowDirectionProperty);
		_textAlignment = textAlignment;
		_lineHeight = (double)element.GetValue(Block.LineHeightProperty);
		_textIndent = (double)element.GetValue(Paragraph.TextIndentProperty);
		_lineStackingStrategy = (LineStackingStrategy)element.GetValue(Block.LineStackingStrategyProperty);
		_textWrapping = TextWrapping.Wrap;
		_textTrimming = TextTrimming.None;
		if (contentHost is TextBlock || contentHost is ITextBoxViewHost)
		{
			_textWrapping = (TextWrapping)contentHost.GetValue(TextBlock.TextWrappingProperty);
			_textTrimming = (TextTrimming)contentHost.GetValue(TextBlock.TextTrimmingProperty);
		}
		else if (contentHost is FlowDocument)
		{
			_textWrapping = ((FlowDocument)contentHost).TextWrapping;
		}
	}

	internal double CalcLineAdvanceForTextParagraph(TextParagraph textParagraph, int dcp, double lineAdvance)
	{
		if (!double.IsNaN(_lineHeight))
		{
			lineAdvance = LineStackingStrategy switch
			{
				LineStackingStrategy.BlockLineHeight => _lineHeight, 
				_ => (dcp != 0 || !textParagraph.HasFiguresOrFloaters() || textParagraph.GetLastDcpAttachedObjectBeforeLine(0) + textParagraph.ParagraphStartCharacterPosition != textParagraph.ParagraphEndCharacterPosition) ? Math.Max(lineAdvance, _lineHeight) : _lineHeight, 
			};
		}
		return lineAdvance;
	}

	internal double CalcLineAdvance(double lineAdvance)
	{
		if (!double.IsNaN(_lineHeight))
		{
			lineAdvance = LineStackingStrategy switch
			{
				LineStackingStrategy.BlockLineHeight => _lineHeight, 
				_ => Math.Max(lineAdvance, _lineHeight), 
			};
		}
		return lineAdvance;
	}

	internal TextParagraphProperties GetParaEllipsisLineProps(bool firstLine)
	{
		return new ParaEllipsisLineProperties(firstLine ? FirstLineProps : this);
	}
}
