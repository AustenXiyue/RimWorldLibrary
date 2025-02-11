using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace MS.Internal.TextFormatting;

internal sealed class SimpleRun
{
	[Flags]
	internal enum Flags : ushort
	{
		None = 0,
		EOT = 1,
		Ghost = 2,
		TrimTrailingUnderline = 4,
		Tab = 8
	}

	public CharacterBufferReference CharBufferReference;

	public int Length;

	public int[] NominalAdvances;

	public int IdealWidth;

	public TextRun TextRun;

	public TextDecoration Underline;

	public Flags RunFlags;

	private TextFormatterImp _textFormatterImp;

	private double _pixelsPerDip;

	internal bool EOT => (RunFlags & Flags.EOT) != 0;

	internal bool Ghost => (RunFlags & Flags.Ghost) != 0;

	internal bool Tab => (RunFlags & Flags.Tab) != 0;

	internal bool TrimTrailingUnderline
	{
		get
		{
			return (RunFlags & Flags.TrimTrailingUnderline) != 0;
		}
		set
		{
			if (value)
			{
				RunFlags |= Flags.TrimTrailingUnderline;
			}
			else
			{
				RunFlags &= ~Flags.TrimTrailingUnderline;
			}
		}
	}

	internal double Baseline
	{
		get
		{
			if (Ghost || EOT)
			{
				return 0.0;
			}
			return TextRun.Properties.Typeface.Baseline(TextRun.Properties.FontRenderingEmSize, 1.0, _pixelsPerDip, _textFormatterImp.TextFormattingMode);
		}
	}

	internal double Height
	{
		get
		{
			if (Ghost || EOT)
			{
				return 0.0;
			}
			return TextRun.Properties.Typeface.LineSpacing(TextRun.Properties.FontRenderingEmSize, 1.0, _pixelsPerDip, _textFormatterImp.TextFormattingMode);
		}
	}

	internal Typeface Typeface => TextRun.Properties.Typeface;

	internal double EmSize => TextRun.Properties.FontRenderingEmSize;

	internal bool IsVisible => TextRun is TextCharacters;

	internal SimpleRun(TextFormatterImp textFormatterImp, double pixelsPerDip)
	{
		_textFormatterImp = textFormatterImp;
		_pixelsPerDip = pixelsPerDip;
	}

	public static SimpleRun Create(FormatSettings settings, int cp, int cpFirst, int widthLeft, int widthMax, int idealRunOffsetUnRounded, double pixelsPerDip)
	{
		TextRun textRun;
		int runLength;
		CharacterBufferRange charString = settings.FetchTextRun(cp, cpFirst, out textRun, out runLength);
		return Create(settings, charString, textRun, cp, cpFirst, runLength, widthLeft, idealRunOffsetUnRounded, pixelsPerDip);
	}

	public static SimpleRun Create(FormatSettings settings, CharacterBufferRange charString, TextRun textRun, int cp, int cpFirst, int runLength, int widthLeft, int idealRunOffsetUnRounded, double pixelsPerDip)
	{
		SimpleRun simpleRun = null;
		if (textRun is TextCharacters)
		{
			if (textRun.Properties.BaselineAlignment != BaselineAlignment.Baseline || (textRun.Properties.TextEffects != null && textRun.Properties.TextEffects.Count != 0))
			{
				return null;
			}
			TextDecorationCollection textDecorations = textRun.Properties.TextDecorations;
			if (textDecorations != null && textDecorations.Count != 0 && !textDecorations.ValueEquals(TextDecorations.Underline))
			{
				return null;
			}
			settings.DigitState.SetTextRunProperties(textRun.Properties);
			if (settings.DigitState.RequiresNumberSubstitution)
			{
				return null;
			}
			bool flag = CanProcessTabsInSimpleShapingPath(settings.Pap, settings.Formatter.TextFormattingMode);
			if (charString[0] == '\r')
			{
				runLength = 1;
				if (charString.Length > 1 && charString[1] == '\n')
				{
					runLength = 2;
				}
				else if (charString.Length == 1)
				{
					TextRun textRun2;
					int runLength2;
					CharacterBufferRange characterBufferRange = settings.FetchTextRun(cp + 1, cpFirst, out textRun2, out runLength2);
					if (characterBufferRange.Length > 0 && characterBufferRange[0] == '\n')
					{
						int num = 2;
						char[] array = new char[num];
						array[0] = '\r';
						array[1] = '\n';
						TextRun textRun3 = new TextCharacters(array, 0, num, textRun.Properties);
						return new SimpleRun(num, textRun3, Flags.EOT | Flags.Ghost, settings.Formatter, pixelsPerDip);
					}
				}
				return new SimpleRun(runLength, textRun, Flags.EOT | Flags.Ghost, settings.Formatter, pixelsPerDip);
			}
			if (charString[0] == '\n')
			{
				runLength = 1;
				return new SimpleRun(runLength, textRun, Flags.EOT | Flags.Ghost, settings.Formatter, pixelsPerDip);
			}
			if (flag && charString[0] == '\t')
			{
				return CreateSimpleRunForTab(settings, textRun, idealRunOffsetUnRounded, pixelsPerDip);
			}
			simpleRun = CreateSimpleTextRun(charString, textRun, settings.Formatter, widthLeft, settings.Pap.EmergencyWrap, flag, pixelsPerDip);
			if (simpleRun == null)
			{
				return null;
			}
			if (textDecorations != null && textDecorations.Count == 1)
			{
				simpleRun.Underline = textDecorations[0];
			}
		}
		else if (textRun is TextEndOfLine)
		{
			simpleRun = new SimpleRun(runLength, textRun, Flags.EOT | Flags.Ghost, settings.Formatter, pixelsPerDip);
		}
		else if (textRun is TextHidden)
		{
			simpleRun = new SimpleRun(runLength, textRun, Flags.Ghost, settings.Formatter, pixelsPerDip);
		}
		return simpleRun;
	}

	private static SimpleRun CreateSimpleRunForTab(FormatSettings settings, TextRun textRun, int idealRunOffsetUnRounded, double pixelsPerDip)
	{
		if (settings == null || textRun == null || textRun.Properties == null || textRun.Properties.Typeface == null)
		{
			return null;
		}
		GlyphTypeface glyphTypeface = textRun.Properties.Typeface.TryGetGlyphTypeface();
		if (glyphTypeface == null || !glyphTypeface.HasCharacter(32u))
		{
			return null;
		}
		TextRun textRun2 = new TextCharacters(" ", textRun.Properties);
		CharacterBufferRange charBufferRange = new CharacterBufferRange(textRun2);
		SimpleRun simpleRun = new SimpleRun(1, textRun2, Flags.Tab, settings.Formatter, pixelsPerDip);
		simpleRun.CharBufferReference = charBufferRange.CharacterBufferReference;
		simpleRun.TextRun.Properties.Typeface.GetCharacterNominalWidthsAndIdealWidth(charBufferRange, simpleRun.EmSize, (float)pixelsPerDip, TextFormatterImp.ToIdeal, settings.Formatter.TextFormattingMode, isSideways: false, out simpleRun.NominalAdvances);
		int num = TextFormatterImp.RealToIdeal(settings.Pap.DefaultIncrementalTab);
		int num2 = (idealRunOffsetUnRounded / num + 1) * num;
		simpleRun.IdealWidth = (simpleRun.NominalAdvances[0] = num2 - idealRunOffsetUnRounded);
		return simpleRun;
	}

	private static bool CanProcessTabsInSimpleShapingPath(ParaProp textParagraphProperties, TextFormattingMode textFormattingMode)
	{
		if (textParagraphProperties.Tabs == null)
		{
			return textParagraphProperties.DefaultIncrementalTab > 0.0;
		}
		return false;
	}

	internal static SimpleRun CreateSimpleTextRun(CharacterBufferRange charBufferRange, TextRun textRun, TextFormatterImp formatter, int widthLeft, bool emergencyWrap, bool breakOnTabs, double pixelsPerDip)
	{
		Invariant.Assert(textRun is TextCharacters);
		SimpleRun simpleRun = new SimpleRun(formatter, pixelsPerDip);
		simpleRun.CharBufferReference = charBufferRange.CharacterBufferReference;
		simpleRun.TextRun = textRun;
		if (!simpleRun.TextRun.Properties.Typeface.CheckFastPathNominalGlyphs(charBufferRange, simpleRun.EmSize, (float)pixelsPerDip, 1.0, formatter.IdealToReal(widthLeft, pixelsPerDip), !emergencyWrap, numberSubstitution: false, CultureMapper.GetSpecificCulture(simpleRun.TextRun.Properties.CultureInfo), formatter.TextFormattingMode, isSideways: false, breakOnTabs, out simpleRun.Length))
		{
			return null;
		}
		simpleRun.TextRun.Properties.Typeface.GetCharacterNominalWidthsAndIdealWidth(new CharacterBufferRange(simpleRun.CharBufferReference, simpleRun.Length), simpleRun.EmSize, (float)pixelsPerDip, TextFormatterImp.ToIdeal, formatter.TextFormattingMode, isSideways: false, out simpleRun.NominalAdvances, out simpleRun.IdealWidth);
		return simpleRun;
	}

	private SimpleRun(int length, TextRun textRun, Flags flags, TextFormatterImp textFormatterImp, double pixelsPerDip)
	{
		Length = length;
		TextRun = textRun;
		RunFlags = flags;
		_textFormatterImp = textFormatterImp;
		_pixelsPerDip = pixelsPerDip;
	}

	internal Rect Draw(DrawingContext drawingContext, double x, double y, bool visiCodePath)
	{
		if (Length <= 0 || Ghost)
		{
			return Rect.Empty;
		}
		Brush brush = TextRun.Properties.ForegroundBrush;
		if (visiCodePath && brush is SolidColorBrush)
		{
			Color color = ((SolidColorBrush)brush).Color;
			brush = new SolidColorBrush(Color.FromArgb((byte)(color.A >> 2), color.R, color.G, color.B));
		}
		IList<double> list;
		if (_textFormatterImp.TextFormattingMode == TextFormattingMode.Ideal)
		{
			list = new ThousandthOfEmRealDoubles(EmSize, NominalAdvances.Length);
			for (int i = 0; i < list.Count; i++)
			{
				list[i] = _textFormatterImp.IdealToReal(NominalAdvances[i], _pixelsPerDip);
			}
		}
		else
		{
			list = new List<double>(NominalAdvances.Length);
			for (int j = 0; j < NominalAdvances.Length; j++)
			{
				list.Add(_textFormatterImp.IdealToReal(NominalAdvances[j], _pixelsPerDip));
			}
		}
		CharacterBufferRange charBufferRange = new CharacterBufferRange(CharBufferReference, Length);
		GlyphTypeface glyphTypeface = Typeface.TryGetGlyphTypeface();
		Invariant.Assert(glyphTypeface != null);
		GlyphRun glyphRun = glyphTypeface.ComputeUnshapedGlyphRun(new Point(x, y), charBufferRange, list, EmSize, (float)_pixelsPerDip, TextRun.Properties.FontHintingEmSize, Typeface.NullFont, CultureMapper.GetSpecificCulture(TextRun.Properties.CultureInfo), null, _textFormatterImp.TextFormattingMode);
		Rect result = glyphRun?.ComputeInkBoundingBox() ?? Rect.Empty;
		if (!result.IsEmpty)
		{
			result.X += glyphRun.BaselineOrigin.X;
			result.Y += glyphRun.BaselineOrigin.Y;
		}
		if (drawingContext != null)
		{
			if (glyphRun != null)
			{
				glyphRun.EmitBackground(drawingContext, TextRun.Properties.BackgroundBrush);
				drawingContext.DrawGlyphRun(brush, glyphRun);
			}
			if (Underline != null)
			{
				int num = Length;
				if (TrimTrailingUnderline)
				{
					while (num > 0 && IsSpace(charBufferRange[num - 1]))
					{
						num--;
					}
				}
				double num2 = 0.0;
				for (int k = 0; k < num; k++)
				{
					num2 += _textFormatterImp.IdealToReal(NominalAdvances[k], _pixelsPerDip);
				}
				double num3 = (0.0 - Typeface.UnderlinePosition) * EmSize;
				double num4 = Typeface.UnderlineThickness * EmSize;
				Point point = new Point(x, y + num3);
				Rect rect = new Rect(point.X, point.Y - num4 * 0.5, num2, num4);
				drawingContext.PushGuidelineY2(y, point.Y - num4 * 0.5 - y);
				try
				{
					drawingContext.DrawRectangle(brush, null, rect);
				}
				finally
				{
					drawingContext.Pop();
				}
				result.Union(rect);
			}
		}
		return result;
	}

	internal bool CollectTrailingSpaces(TextFormatterImp formatter, ref int trailing, ref int trailingSpaceWidth)
	{
		if (Ghost)
		{
			if (!EOT)
			{
				trailing += Length;
				trailingSpaceWidth += IdealWidth;
			}
			return true;
		}
		if (Tab)
		{
			return false;
		}
		int offsetToFirstChar = CharBufferReference.OffsetToFirstChar;
		CharacterBuffer characterBuffer = CharBufferReference.CharacterBuffer;
		int num = Length;
		if (num > 0 && IsSpace(characterBuffer[offsetToFirstChar + num - 1]))
		{
			while (num > 0 && IsSpace(characterBuffer[offsetToFirstChar + num - 1]))
			{
				trailingSpaceWidth += NominalAdvances[num - 1];
				num--;
				trailing++;
			}
			return num == 0;
		}
		return false;
	}

	private static bool IsSpace(char ch)
	{
		if (TextStore.IsSpace(ch))
		{
			return true;
		}
		return Classification.CharAttributeOf(Classification.GetUnicodeClassUTF16(ch)).BiDi == DirectionClass.WhiteSpace;
	}

	internal bool IsUnderlineCompatible(SimpleRun nextRun)
	{
		if (Typeface.Equals(nextRun.Typeface) && EmSize == nextRun.EmSize)
		{
			return Baseline == nextRun.Baseline;
		}
		return false;
	}

	internal int DistanceFromDcp(int dcp)
	{
		if (Ghost || Tab)
		{
			if (dcp > 0)
			{
				return IdealWidth;
			}
			return 0;
		}
		if (dcp > Length)
		{
			dcp = Length;
		}
		int num = 0;
		for (int i = 0; i < dcp; i++)
		{
			num += NominalAdvances[i];
		}
		return num;
	}

	internal CharacterHit DcpFromDistance(int idealDistance)
	{
		if (Ghost)
		{
			if (!EOT && idealDistance > 0)
			{
				return new CharacterHit(Length, 0);
			}
			return default(CharacterHit);
		}
		if (Length <= 0)
		{
			return default(CharacterHit);
		}
		int i = 0;
		int num = 0;
		for (; i < Length; i++)
		{
			if (idealDistance < ((!Tab) ? (num = NominalAdvances[i]) : (num = IdealWidth / Length)))
			{
				break;
			}
			idealDistance -= num;
		}
		if (i < Length)
		{
			return new CharacterHit(i, (idealDistance > num / 2) ? 1 : 0);
		}
		return new CharacterHit(Length - 1, 1);
	}
}
