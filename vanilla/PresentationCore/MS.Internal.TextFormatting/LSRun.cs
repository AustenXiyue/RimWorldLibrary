using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using MS.Internal.PresentationCore;
using MS.Internal.Text.TextInterface;

namespace MS.Internal.TextFormatting;

internal sealed class LSRun
{
	private enum CustomOpenTypeFeatures
	{
		AlternativeFractions,
		PetiteCapitalsFromCapitals,
		SmallCapitalsFromCapitals,
		ContextualAlternates,
		CaseSensitiveForms,
		ContextualLigatures,
		CapitalSpacing,
		ContextualSwash,
		CursivePositioning,
		DiscretionaryLigatures,
		ExpertForms,
		Fractions,
		FullWidth,
		HalfForms,
		HalantForms,
		AlternateHalfWidth,
		HistoricalForms,
		HorizontalKanaAlternates,
		HistoricalLigatures,
		HojoKanjiForms,
		HalfWidth,
		JIS78Forms,
		JIS83Forms,
		JIS90Forms,
		JIS04Forms,
		Kerning,
		StandardLigatures,
		LiningFigures,
		MathematicalGreek,
		AlternateAnnotationForms,
		NLCKanjiForms,
		OldStyleFigures,
		Ordinals,
		ProportionalAlternateWidth,
		PetiteCapitals,
		ProportionalFigures,
		ProportionalWidths,
		QuarterWidths,
		RubyNotationForms,
		StylisticAlternates,
		ScientificInferiors,
		SmallCapitals,
		SimplifiedForms,
		StylisticSet1,
		StylisticSet2,
		StylisticSet3,
		StylisticSet4,
		StylisticSet5,
		StylisticSet6,
		StylisticSet7,
		StylisticSet8,
		StylisticSet9,
		StylisticSet10,
		StylisticSet11,
		StylisticSet12,
		StylisticSet13,
		StylisticSet14,
		StylisticSet15,
		StylisticSet16,
		StylisticSet17,
		StylisticSet18,
		StylisticSet19,
		StylisticSet20,
		Subscript,
		Superscript,
		Swash,
		Titling,
		TraditionalNameForms,
		TabularFigures,
		TraditionalForms,
		ThirdWidths,
		Unicase,
		SlashedZero,
		Count
	}

	private TextRunInfo _runInfo;

	private Plsrun _type;

	private int _offsetToFirstCp;

	private int _textRunLength;

	private CharacterBufferRange _charBufferRange;

	private int _baselineOffset;

	private int _height;

	private int _baselineMoveOffset;

	private int _emSize;

	private TextShapeableSymbols _shapeable;

	private ushort _charFlags;

	private byte _bidiLevel;

	private IList<TextEffect> _textEffects;

	private const ushort FeatureNotEnabled = ushort.MaxValue;

	internal bool IsHitTestable => _type == Plsrun.Text;

	internal bool IsVisible
	{
		get
		{
			if (_type != Plsrun.Text)
			{
				return _type == Plsrun.InlineObject;
			}
			return true;
		}
	}

	internal bool IsNewline
	{
		get
		{
			if (_type != Plsrun.LineBreak)
			{
				return _type == Plsrun.ParaBreak;
			}
			return true;
		}
	}

	internal bool NeedsCaretInfo
	{
		get
		{
			if (_shapeable != null)
			{
				return _shapeable.NeedsCaretInfo;
			}
			return false;
		}
	}

	internal bool HasExtendedCharacter
	{
		get
		{
			if (_shapeable != null)
			{
				return _shapeable.HasExtendedCharacter;
			}
			return false;
		}
	}

	internal byte BidiLevel => _bidiLevel;

	internal bool IsSymbol
	{
		get
		{
			if (_shapeable is TextShapeableCharacters textShapeableCharacters)
			{
				return textShapeableCharacters.IsSymbol;
			}
			return false;
		}
	}

	internal int OffsetToFirstCp => _offsetToFirstCp;

	internal int Length => _textRunLength;

	internal TextModifierScope TextModifierScope => _runInfo.TextModifierScope;

	internal Plsrun Type => _type;

	internal ushort CharacterAttributeFlags => _charFlags;

	internal CharacterBuffer CharacterBuffer => _charBufferRange.CharacterBuffer;

	internal int StringLength => _charBufferRange.Length;

	internal int OffsetToFirstChar => _charBufferRange.OffsetToFirstChar;

	internal TextRun TextRun => _runInfo.TextRun;

	internal TextShapeableSymbols Shapeable => _shapeable;

	internal int BaselineOffset
	{
		get
		{
			return _baselineOffset;
		}
		set
		{
			_baselineOffset = value;
		}
	}

	internal int Height
	{
		get
		{
			return _height;
		}
		set
		{
			_height = value;
		}
	}

	internal int Descent => Height - BaselineOffset;

	internal TextRunProperties RunProp => _runInfo.Properties;

	internal CultureInfo TextCulture => CultureMapper.GetSpecificCulture((RunProp != null) ? RunProp.CultureInfo : null);

	internal int EmSize => _emSize;

	internal int BaselineMoveOffset => _baselineMoveOffset;

	internal LSRun(TextRunInfo runInfo, IList<TextEffect> textEffects, Plsrun type, int offsetToFirstCp, int textRunLength, int emSize, ushort charFlags, CharacterBufferRange charBufferRange, TextShapeableSymbols shapeable, double realToIdeal, byte bidiLevel)
		: this(runInfo, textEffects, type, offsetToFirstCp, textRunLength, emSize, charFlags, charBufferRange, (shapeable != null) ? ((int)Math.Round(shapeable.Baseline * realToIdeal)) : 0, (shapeable != null) ? ((int)Math.Round(shapeable.Height * realToIdeal)) : 0, shapeable, bidiLevel)
	{
	}

	private LSRun(TextRunInfo runInfo, IList<TextEffect> textEffects, Plsrun type, int offsetToFirstCp, int textRunLength, int emSize, ushort charFlags, CharacterBufferRange charBufferRange, int baselineOffset, int height, TextShapeableSymbols shapeable, byte bidiLevel)
	{
		_runInfo = runInfo;
		_type = type;
		_offsetToFirstCp = offsetToFirstCp;
		_textRunLength = textRunLength;
		_emSize = emSize;
		_charFlags = charFlags;
		_charBufferRange = charBufferRange;
		_baselineOffset = baselineOffset;
		_height = height;
		_bidiLevel = bidiLevel;
		_shapeable = shapeable;
		_textEffects = textEffects;
	}

	internal LSRun(Plsrun type, nint controlChar)
		: this(null, type, controlChar, 0, -1, 0)
	{
	}

	internal unsafe LSRun(TextRunInfo runInfo, Plsrun type, nint controlChar, int textRunLength, int offsetToFirstCp, byte bidiLevel)
	{
		_runInfo = runInfo;
		_type = type;
		_charBufferRange = new CharacterBufferRange((char*)controlChar, 1);
		_textRunLength = textRunLength;
		_offsetToFirstCp = offsetToFirstCp;
		_bidiLevel = bidiLevel;
	}

	internal void Truncate(int newLength)
	{
		_charBufferRange = new CharacterBufferRange(_charBufferRange.CharacterBufferReference, newLength);
		_textRunLength = newLength;
	}

	internal Rect DrawGlyphRun(DrawingContext drawingContext, Brush foregroundBrush, GlyphRun glyphRun)
	{
		Rect result = glyphRun.ComputeInkBoundingBox();
		if (!result.IsEmpty)
		{
			result.X += glyphRun.BaselineOrigin.X;
			result.Y += glyphRun.BaselineOrigin.Y;
		}
		if (drawingContext != null)
		{
			int num = 0;
			try
			{
				if (_textEffects != null)
				{
					for (int i = 0; i < _textEffects.Count; i++)
					{
						TextEffect textEffect = _textEffects[i];
						if (textEffect.Transform != null && textEffect.Transform != Transform.Identity)
						{
							drawingContext.PushTransform(textEffect.Transform);
							num++;
						}
						if (textEffect.Clip != null)
						{
							drawingContext.PushClip(textEffect.Clip);
							num++;
						}
						if (textEffect.Foreground != null)
						{
							foregroundBrush = textEffect.Foreground;
						}
					}
				}
				_shapeable.Draw(drawingContext, foregroundBrush, glyphRun);
			}
			finally
			{
				for (int j = 0; j < num; j++)
				{
					drawingContext.Pop();
				}
			}
		}
		return result;
	}

	internal static Point UVToXY(Point origin, Point vectorToOrigin, double u, double v, TextMetrics.FullTextLine line)
	{
		origin.Y += vectorToOrigin.Y;
		return (!line.RightToLeft) ? new Point(u + vectorToOrigin.X + origin.X, v + origin.Y) : new Point(line.Formatter.IdealToReal(line.ParagraphWidth, line.PixelsPerDip) - vectorToOrigin.X - u + origin.X, v + origin.Y);
	}

	internal static Point UVToXY(Point origin, Point vectorToOrigin, int u, int v, TextMetrics.FullTextLine line)
	{
		origin.Y += vectorToOrigin.Y;
		return (!line.RightToLeft) ? new Point(line.Formatter.IdealToReal(u, line.PixelsPerDip) + vectorToOrigin.X + origin.X, line.Formatter.IdealToReal(v, line.PixelsPerDip) + origin.Y) : new Point(line.Formatter.IdealToReal(line.ParagraphWidth - u, line.PixelsPerDip) - vectorToOrigin.X + origin.X, line.Formatter.IdealToReal(v, line.PixelsPerDip) + origin.Y);
	}

	internal static void UVToNominalXY(Point origin, Point vectorToOrigin, int u, int v, TextMetrics.FullTextLine line, out int nominalX, out int nominalY)
	{
		origin.Y += vectorToOrigin.Y;
		if (line.RightToLeft)
		{
			nominalX = line.ParagraphWidth - u + TextFormatterImp.RealToIdeal(0.0 - vectorToOrigin.X + origin.X);
		}
		else
		{
			nominalX = u + TextFormatterImp.RealToIdeal(vectorToOrigin.X + origin.X);
		}
		nominalY = v + TextFormatterImp.RealToIdeal(origin.Y);
	}

	internal static Rect RectUV(Point origin, LSPOINT topLeft, LSPOINT bottomRight, TextMetrics.FullTextLine line)
	{
		int num = topLeft.x - bottomRight.x;
		if (num == 1 || num == -1)
		{
			bottomRight.x = topLeft.x;
		}
		Rect result = new Rect(new Point(line.Formatter.IdealToReal(topLeft.x, line.PixelsPerDip), line.Formatter.IdealToReal(topLeft.y, line.PixelsPerDip)), new Point(line.Formatter.IdealToReal(bottomRight.x, line.PixelsPerDip), line.Formatter.IdealToReal(bottomRight.y, line.PixelsPerDip)));
		if (DoubleUtil.AreClose(result.TopLeft.X, result.BottomRight.X))
		{
			result.Width = 0.0;
		}
		if (DoubleUtil.AreClose(result.TopLeft.Y, result.BottomRight.Y))
		{
			result.Height = 0.0;
		}
		return result;
	}

	internal void Move(int baselineMoveOffset)
	{
		_baselineMoveOffset += baselineMoveOffset;
	}

	private static DWriteFontFeature[] CreateDWriteFontFeatures(TextRunTypographyProperties textRunTypographyProperties)
	{
		checked
		{
			if (textRunTypographyProperties != null)
			{
				if (textRunTypographyProperties.CachedFeatureSet != null)
				{
					return textRunTypographyProperties.CachedFeatureSet;
				}
				List<DWriteFontFeature> list = new List<DWriteFontFeature>(73);
				if (textRunTypographyProperties.CapitalSpacing)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.CapitalSpacing, 1u));
				}
				if (textRunTypographyProperties.CaseSensitiveForms)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.CaseSensitiveForms, 1u));
				}
				if (textRunTypographyProperties.ContextualAlternates)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.ContextualAlternates, 1u));
				}
				if (textRunTypographyProperties.ContextualLigatures)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.ContextualLigatures, 1u));
				}
				if (textRunTypographyProperties.DiscretionaryLigatures)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.DiscretionaryLigatures, 1u));
				}
				if (textRunTypographyProperties.HistoricalForms)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.HistoricalForms, 1u));
				}
				if (textRunTypographyProperties.HistoricalLigatures)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.HistoricalLigatures, 1u));
				}
				if (textRunTypographyProperties.Kerning)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.Kerning, 1u));
				}
				if (textRunTypographyProperties.MathematicalGreek)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.MathematicalGreek, 1u));
				}
				if (textRunTypographyProperties.SlashedZero)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.SlashedZero, 1u));
				}
				if (textRunTypographyProperties.StandardLigatures)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.StandardLigatures, 1u));
				}
				if (textRunTypographyProperties.StylisticSet1)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.StylisticSet1, 1u));
				}
				if (textRunTypographyProperties.StylisticSet10)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.StylisticSet10, 1u));
				}
				if (textRunTypographyProperties.StylisticSet11)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.StylisticSet11, 1u));
				}
				if (textRunTypographyProperties.StylisticSet12)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.StylisticSet12, 1u));
				}
				if (textRunTypographyProperties.StylisticSet13)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.StylisticSet13, 1u));
				}
				if (textRunTypographyProperties.StylisticSet14)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.StylisticSet14, 1u));
				}
				if (textRunTypographyProperties.StylisticSet15)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.StylisticSet15, 1u));
				}
				if (textRunTypographyProperties.StylisticSet16)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.StylisticSet16, 1u));
				}
				if (textRunTypographyProperties.StylisticSet17)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.StylisticSet17, 1u));
				}
				if (textRunTypographyProperties.StylisticSet18)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.StylisticSet18, 1u));
				}
				if (textRunTypographyProperties.StylisticSet19)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.StylisticSet19, 1u));
				}
				if (textRunTypographyProperties.StylisticSet2)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.StylisticSet2, 1u));
				}
				if (textRunTypographyProperties.StylisticSet20)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.StylisticSet20, 1u));
				}
				if (textRunTypographyProperties.StylisticSet3)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.StylisticSet3, 1u));
				}
				if (textRunTypographyProperties.StylisticSet4)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.StylisticSet4, 1u));
				}
				if (textRunTypographyProperties.StylisticSet5)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.StylisticSet5, 1u));
				}
				if (textRunTypographyProperties.StylisticSet6)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.StylisticSet6, 1u));
				}
				if (textRunTypographyProperties.StylisticSet7)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.StylisticSet7, 1u));
				}
				if (textRunTypographyProperties.StylisticSet8)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.StylisticSet8, 1u));
				}
				if (textRunTypographyProperties.StylisticSet9)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.StylisticSet9, 1u));
				}
				if (textRunTypographyProperties.EastAsianExpertForms)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.ExpertForms, 1u));
				}
				if (textRunTypographyProperties.AnnotationAlternates > 0)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.AlternateAnnotationForms, (uint)textRunTypographyProperties.AnnotationAlternates));
				}
				if (textRunTypographyProperties.ContextualSwashes > 0)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.ContextualSwash, (uint)textRunTypographyProperties.ContextualSwashes));
				}
				if (textRunTypographyProperties.StylisticAlternates > 0)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.StylisticAlternates, (uint)textRunTypographyProperties.StylisticAlternates));
				}
				if (textRunTypographyProperties.StandardSwashes > 0)
				{
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.Swash, (uint)textRunTypographyProperties.StandardSwashes));
				}
				switch (textRunTypographyProperties.Capitals)
				{
				case FontCapitals.AllPetiteCaps:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.PetiteCapitals, 1u));
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.PetiteCapitalsFromCapitals, 1u));
					break;
				case FontCapitals.AllSmallCaps:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.SmallCapitals, 1u));
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.SmallCapitalsFromCapitals, 1u));
					break;
				case FontCapitals.PetiteCaps:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.PetiteCapitals, 1u));
					break;
				case FontCapitals.SmallCaps:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.SmallCapitals, 1u));
					break;
				case FontCapitals.Titling:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.Titling, 1u));
					break;
				case FontCapitals.Unicase:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.Unicase, 1u));
					break;
				}
				switch (textRunTypographyProperties.EastAsianLanguage)
				{
				case FontEastAsianLanguage.Simplified:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.SimplifiedForms, 1u));
					break;
				case FontEastAsianLanguage.Traditional:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.TraditionalForms, 1u));
					break;
				case FontEastAsianLanguage.TraditionalNames:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.TraditionalNameForms, 1u));
					break;
				case FontEastAsianLanguage.NlcKanji:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.NLCKanjiForms, 1u));
					break;
				case FontEastAsianLanguage.HojoKanji:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.HojoKanjiForms, 1u));
					break;
				case FontEastAsianLanguage.Jis78:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.JIS78Forms, 1u));
					break;
				case FontEastAsianLanguage.Jis83:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.JIS83Forms, 1u));
					break;
				case FontEastAsianLanguage.Jis90:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.JIS90Forms, 1u));
					break;
				case FontEastAsianLanguage.Jis04:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.JIS04Forms, 1u));
					break;
				}
				switch (textRunTypographyProperties.Fraction)
				{
				case FontFraction.Stacked:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.AlternativeFractions, 1u));
					break;
				case FontFraction.Slashed:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.Fractions, 1u));
					break;
				}
				switch (textRunTypographyProperties.NumeralAlignment)
				{
				case FontNumeralAlignment.Proportional:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.ProportionalFigures, 1u));
					break;
				case FontNumeralAlignment.Tabular:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.TabularFigures, 1u));
					break;
				}
				switch (textRunTypographyProperties.NumeralStyle)
				{
				case FontNumeralStyle.Lining:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.LiningFigures, 1u));
					break;
				case FontNumeralStyle.OldStyle:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.OldStyleFigures, 1u));
					break;
				}
				switch (textRunTypographyProperties.Variants)
				{
				case FontVariants.Inferior:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.ScientificInferiors, 1u));
					break;
				case FontVariants.Ordinal:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.Ordinals, 1u));
					break;
				case FontVariants.Ruby:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.RubyNotationForms, 1u));
					break;
				case FontVariants.Subscript:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.Subscript, 1u));
					break;
				case FontVariants.Superscript:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.Superscript, 1u));
					break;
				}
				switch (textRunTypographyProperties.EastAsianWidths)
				{
				case FontEastAsianWidths.Proportional:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.ProportionalWidths, 1u));
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.ProportionalAlternateWidth, 1u));
					break;
				case FontEastAsianWidths.Full:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.FullWidth, 1u));
					break;
				case FontEastAsianWidths.Half:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.HalfWidth, 1u));
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.AlternateHalfWidth, 1u));
					break;
				case FontEastAsianWidths.Third:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.ThirdWidths, 1u));
					break;
				case FontEastAsianWidths.Quarter:
					list.Add(new DWriteFontFeature(DWriteFontFeatureTag.QuarterWidths, 1u));
					break;
				}
				textRunTypographyProperties.CachedFeatureSet = list.ToArray();
				return textRunTypographyProperties.CachedFeatureSet;
			}
			return null;
		}
	}

	internal unsafe static void CompileFeatureSet(LSRun[] lsruns, int* pcchRuns, uint totalLength, out DWriteFontFeature[][] fontFeatures, out uint[] fontFeatureRanges)
	{
		if (lsruns[0].RunProp.TypographyProperties == null)
		{
			for (int i = 1; i < lsruns.Length; i++)
			{
				if (lsruns[i].RunProp.TypographyProperties != null)
				{
					throw new ArgumentException(SR.CompileFeatureSet_InvalidTypographyProperties);
				}
			}
			fontFeatures = null;
			fontFeatureRanges = null;
		}
		else
		{
			fontFeatures = new DWriteFontFeature[lsruns.Length][];
			fontFeatureRanges = new uint[lsruns.Length];
			for (int j = 0; j < lsruns.Length; j++)
			{
				TextRunTypographyProperties typographyProperties = lsruns[j].RunProp.TypographyProperties;
				fontFeatures[j] = CreateDWriteFontFeatures(typographyProperties);
				checked
				{
					fontFeatureRanges[j] = (uint)(*unchecked((int*)((byte*)pcchRuns + checked(unchecked((nint)j) * (nint)4))));
				}
			}
		}
	}

	internal static void CompileFeatureSet(TextRunTypographyProperties textRunTypographyProperties, uint totalLength, out DWriteFontFeature[][] fontFeatures, out uint[] fontFeatureRanges)
	{
		if (textRunTypographyProperties == null)
		{
			fontFeatures = null;
			fontFeatureRanges = null;
			return;
		}
		fontFeatures = new DWriteFontFeature[1][];
		fontFeatureRanges = new uint[1];
		fontFeatures[0] = CreateDWriteFontFeatures(textRunTypographyProperties);
		fontFeatureRanges[0] = totalLength;
	}
}
