using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace MS.Internal.TextFormatting;

internal sealed class ParaProp
{
	[Flags]
	private enum StatusFlags
	{
		Rtl = 1,
		OptimalBreak = 2
	}

	private StatusFlags _statusFlags;

	private TextParagraphProperties _paragraphProperties;

	private int _emSize;

	private int _indent;

	private int _paragraphIndent;

	private int _height;

	internal bool RightToLeft => (_statusFlags & StatusFlags.Rtl) != 0;

	internal bool OptimalBreak => (_statusFlags & StatusFlags.OptimalBreak) != 0;

	internal bool FirstLineInParagraph => _paragraphProperties.FirstLineInParagraph;

	internal bool AlwaysCollapsible => _paragraphProperties.AlwaysCollapsible;

	internal int Indent => _indent;

	internal int ParagraphIndent => _paragraphIndent;

	internal double DefaultIncrementalTab => _paragraphProperties.DefaultIncrementalTab;

	internal IList<TextTabProperties> Tabs => _paragraphProperties.Tabs;

	internal TextAlignment Align => _paragraphProperties.TextAlignment;

	internal bool Justify => _paragraphProperties.TextAlignment == TextAlignment.Justify;

	internal bool EmergencyWrap => _paragraphProperties.TextWrapping == TextWrapping.Wrap;

	internal bool Wrap
	{
		get
		{
			if (_paragraphProperties.TextWrapping != 0)
			{
				return EmergencyWrap;
			}
			return true;
		}
	}

	internal Typeface DefaultTypeface => _paragraphProperties.DefaultTextRunProperties.Typeface;

	internal int EmSize => _emSize;

	internal int LineHeight => _height;

	internal TextMarkerProperties TextMarkerProperties => _paragraphProperties.TextMarkerProperties;

	internal TextLexicalService Hyphenator => _paragraphProperties.Hyphenator;

	internal TextDecorationCollection TextDecorations => _paragraphProperties.TextDecorations;

	internal Brush DefaultTextDecorationsBrush => _paragraphProperties.DefaultTextRunProperties.ForegroundBrush;

	internal ParaProp(TextFormatterImp formatter, TextParagraphProperties paragraphProperties, bool optimalBreak)
	{
		_paragraphProperties = paragraphProperties;
		_emSize = TextFormatterImp.RealToIdeal(paragraphProperties.DefaultTextRunProperties.FontRenderingEmSize);
		_indent = TextFormatterImp.RealToIdeal(paragraphProperties.Indent);
		_paragraphIndent = TextFormatterImp.RealToIdeal(paragraphProperties.ParagraphIndent);
		_height = TextFormatterImp.RealToIdeal(paragraphProperties.LineHeight);
		if (_paragraphProperties.FlowDirection == FlowDirection.RightToLeft)
		{
			_statusFlags |= StatusFlags.Rtl;
		}
		if (optimalBreak)
		{
			_statusFlags |= StatusFlags.OptimalBreak;
		}
	}
}
