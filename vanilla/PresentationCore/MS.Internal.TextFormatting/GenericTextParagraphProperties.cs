using System.Windows;
using System.Windows.Media.TextFormatting;

namespace MS.Internal.TextFormatting;

internal sealed class GenericTextParagraphProperties : TextParagraphProperties
{
	private FlowDirection _flowDirection;

	private TextAlignment _textAlignment;

	private bool _firstLineInParagraph;

	private bool _alwaysCollapsible;

	private TextRunProperties _defaultTextRunProperties;

	private TextWrapping _textWrap;

	private double _indent;

	private double _lineHeight;

	public override FlowDirection FlowDirection => _flowDirection;

	public override TextAlignment TextAlignment => _textAlignment;

	public override double LineHeight => _lineHeight;

	public override bool FirstLineInParagraph => _firstLineInParagraph;

	public override bool AlwaysCollapsible => _alwaysCollapsible;

	public override TextRunProperties DefaultTextRunProperties => _defaultTextRunProperties;

	public override TextWrapping TextWrapping => _textWrap;

	public override TextMarkerProperties TextMarkerProperties => null;

	public override double Indent => _indent;

	public GenericTextParagraphProperties(FlowDirection flowDirection, TextAlignment textAlignment, bool firstLineInParagraph, bool alwaysCollapsible, TextRunProperties defaultTextRunProperties, TextWrapping textWrap, double lineHeight, double indent)
	{
		_flowDirection = flowDirection;
		_textAlignment = textAlignment;
		_firstLineInParagraph = firstLineInParagraph;
		_alwaysCollapsible = alwaysCollapsible;
		_defaultTextRunProperties = defaultTextRunProperties;
		_textWrap = textWrap;
		_lineHeight = lineHeight;
		_indent = indent;
	}

	public GenericTextParagraphProperties(TextParagraphProperties textParagraphProperties)
	{
		_flowDirection = textParagraphProperties.FlowDirection;
		_defaultTextRunProperties = textParagraphProperties.DefaultTextRunProperties;
		_textAlignment = textParagraphProperties.TextAlignment;
		_lineHeight = textParagraphProperties.LineHeight;
		_firstLineInParagraph = textParagraphProperties.FirstLineInParagraph;
		_alwaysCollapsible = textParagraphProperties.AlwaysCollapsible;
		_textWrap = textParagraphProperties.TextWrapping;
		_indent = textParagraphProperties.Indent;
	}

	internal void SetFlowDirection(FlowDirection flowDirection)
	{
		_flowDirection = flowDirection;
	}

	internal void SetTextAlignment(TextAlignment textAlignment)
	{
		_textAlignment = textAlignment;
	}

	internal void SetLineHeight(double lineHeight)
	{
		_lineHeight = lineHeight;
	}

	internal void SetTextWrapping(TextWrapping textWrap)
	{
		_textWrap = textWrap;
	}
}
