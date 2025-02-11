using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace MS.Internal.Text;

internal class TextSpanModifier : TextModifier
{
	private class MergedTextRunProperties : TextRunProperties
	{
		private TextRunProperties _runProperties;

		private TextDecorationCollection _textDecorations;

		public override Typeface Typeface => _runProperties.Typeface;

		public override double FontRenderingEmSize => _runProperties.FontRenderingEmSize;

		public override double FontHintingEmSize => _runProperties.FontHintingEmSize;

		public override TextDecorationCollection TextDecorations => _textDecorations;

		public override Brush ForegroundBrush => _runProperties.ForegroundBrush;

		public override Brush BackgroundBrush => _runProperties.BackgroundBrush;

		public override CultureInfo CultureInfo => _runProperties.CultureInfo;

		public override TextEffectCollection TextEffects => _runProperties.TextEffects;

		public override BaselineAlignment BaselineAlignment => _runProperties.BaselineAlignment;

		public override TextRunTypographyProperties TypographyProperties => _runProperties.TypographyProperties;

		public override NumberSubstitution NumberSubstitution => _runProperties.NumberSubstitution;

		internal MergedTextRunProperties(TextRunProperties runProperties, TextDecorationCollection textDecorations)
		{
			_runProperties = runProperties;
			_textDecorations = textDecorations;
			base.PixelsPerDip = _runProperties.PixelsPerDip;
		}
	}

	private int _length;

	private TextDecorationCollection _modifierDecorations;

	private Brush _modifierBrush;

	private FlowDirection _flowDirection;

	private bool _hasDirectionalEmbedding;

	public sealed override int Length => _length;

	public sealed override TextRunProperties Properties => null;

	public override bool HasDirectionalEmbedding => _hasDirectionalEmbedding;

	public override FlowDirection FlowDirection => _flowDirection;

	public TextSpanModifier(int length, TextDecorationCollection textDecorations, Brush foregroundBrush)
	{
		_length = length;
		_modifierDecorations = textDecorations;
		_modifierBrush = foregroundBrush;
	}

	public TextSpanModifier(int length, TextDecorationCollection textDecorations, Brush foregroundBrush, FlowDirection flowDirection)
		: this(length, textDecorations, foregroundBrush)
	{
		_hasDirectionalEmbedding = true;
		_flowDirection = flowDirection;
	}

	public sealed override TextRunProperties ModifyProperties(TextRunProperties properties)
	{
		if (properties == null || _modifierDecorations == null || _modifierDecorations.Count == 0)
		{
			return properties;
		}
		Brush brush = _modifierBrush;
		if (brush == properties.ForegroundBrush)
		{
			brush = null;
		}
		TextDecorationCollection textDecorations = properties.TextDecorations;
		TextDecorationCollection textDecorationCollection;
		if (textDecorations == null || textDecorations.Count == 0)
		{
			textDecorationCollection = ((brush != null) ? CopyTextDecorations(_modifierDecorations, brush) : _modifierDecorations);
		}
		else
		{
			textDecorationCollection = CopyTextDecorations(_modifierDecorations, brush);
			foreach (TextDecoration item in textDecorations)
			{
				textDecorationCollection.Add(item);
			}
		}
		return new MergedTextRunProperties(properties, textDecorationCollection);
	}

	private TextDecorationCollection CopyTextDecorations(TextDecorationCollection textDecorations, Brush brush)
	{
		TextDecorationCollection textDecorationCollection = new TextDecorationCollection();
		Pen pen = null;
		foreach (TextDecoration textDecoration2 in textDecorations)
		{
			if (textDecoration2.Pen == null && brush != null)
			{
				if (pen == null)
				{
					pen = new Pen(brush, 1.0);
				}
				TextDecoration textDecoration = textDecoration2.Clone();
				textDecoration.Pen = pen;
				textDecorationCollection.Add(textDecoration);
			}
			else
			{
				textDecorationCollection.Add(textDecoration2);
			}
		}
		return textDecorationCollection;
	}
}
